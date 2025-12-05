describe('Fluxo de Envio de Convite de Partida', () => {

  const senderUser = {
    email: 'player@player.com', 
    password: 'Teste123.',
  };

  const receiverUser = {
    email: 'admin.equipa.b@exemplo.pt', 
    password: 'Teste123.',
  };

  const senderTeamId = '246892f3-78ba-40d6-a14b-cdf5bf9aeb13';
  const receiverTeam = {
    id: '105abba3-76ff-4499-a6bd-44179287e563', 
  };

  before(() => {
    cy.request({
      method: 'POST',
      url: 'http://localhost:5218/api/User/login',
      body: receiverUser
    }).then((loginResp) => {
      const receiverToken = loginResp.body.firebaseLoginResponseDto.idToken;

      cy.request({
        method: 'GET',
        url: `http://localhost:5218/api/MatchInvite/${receiverTeam.id}`,
        headers: { Authorization: `Bearer ${receiverToken}` },
        failOnStatusCode: false
      }).then((invitesResp) => {
        
        if (invitesResp.isOkStatusCode && Array.isArray(invitesResp.body)) {
          const inviteToDelete = invitesResp.body.find((invite: any) => 
            invite.sender.idTeam.toUpperCase() === senderTeamId.toUpperCase()
          );

          if (inviteToDelete) {
            cy.log(`🗑️ Convite pendente encontrado (${inviteToDelete.id}). O Recetor vai recusar...`);
            
            cy.request({
              method: 'DELETE',
              url: `http://localhost:5218/api/MatchInvite/${receiverTeam.id}/RefuseMatchInvite`,
              headers: { 
                  'Authorization': `Bearer ${receiverToken}`,
                  'Content-Type': 'application/json'
              },
              qs: { idTeam: receiverTeam.id },
              body: JSON.stringify(inviteToDelete.id), 
              failOnStatusCode: false
            }).then(() => {
              cy.log('✅ Convite recusado/apagado com sucesso.');
            });
          } else {
            cy.log('ℹ️ Nenhum convite pendente deste remetente.');
          }
        }
      });
    });
  });

  beforeEach(() => {
    cy.intercept('GET', '**/Team/*/search*').as('loadTeamsRequest');
    cy.intercept('GET', '**/Player/my-profile').as('loadProfile');
  });

  it('Deve permitir login, navegar até uma equipa e enviar um novo convite de partida', () => {
    cy.visit('http://localhost:4200/login');
    cy.get('input[formcontrolname="email"]').type(senderUser.email);
    cy.get('input[formcontrolname="password"]').type(senderUser.password);
    cy.get('button[type="submit"]').click();

    cy.get('a[routerLink="/teams"]').click();
    cy.url().should('include', '/teams');

    cy.wait('@loadTeamsRequest');

    cy.get(`a[href*="${receiverTeam.id}" i]`).should('be.visible').click();

    cy.url().should('include', `/team/details/${receiverTeam.id}`);

    cy.intercept('POST', '**/MatchInvite/*/match-invites').as('sendInviteRequest');
    
    cy.get('button').contains('Enviar pedido de partida').click();

    cy.url().should('include', '/team/createMatchInvite');

    const gameDate = new Date();
    gameDate.setDate(gameDate.getDate() + 10);
    gameDate.setHours(14, 30, 0, 0);

    const dateString = gameDate.toISOString().slice(0, 16);

    cy.get('input[formControlName="gameDate"]').should('be.visible').type(dateString);

    cy.get('select[formControlName="homePitch"]').select('Adversário');

    cy.get('button').contains('Enviar').should('not.be.disabled');

    cy.get('button').contains('Enviar').click();

    cy.wait('@sendInviteRequest').then((interception) => {
      expect(interception.response?.statusCode).to.be.oneOf([200, 201]);
    });

    cy.url().should('include', '/team/matchInvites');
  });
});