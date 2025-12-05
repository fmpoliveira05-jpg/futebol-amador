describe('Fluxo de Envio de Convite de Partida', () => {
  const user = {
    email: 'player@player.com',
    password: 'Teste123.',
  };

  const receiverTeam = {
    id: '105abba3-76ff-4499-a6bd-44179287e563',
  };

  beforeEach(() => {
    cy.intercept('GET', '**/Team/*/search*').as('loadTeamsRequest');

    cy.intercept('GET', '**/Player/my-profile').as('loadProfile');
  });

  it('Deve permitir login, navegar até uma equipa e enviar um novo convite de partida', () => {
    // login
    cy.visit('http://localhost:4200/login');
    cy.get('input[formcontrolname="email"]').type(user.email);
    cy.get('input[formcontrolname="password"]').type(user.password);
    cy.get('button[type="submit"]').click();

    // navegar pra lista de equipas
    cy.get('a[routerLink="/teams"]').click();
    cy.url().should('include', '/teams');

    // esperar carregar lista de teams
    cy.wait('@loadTeamsRequest');

    // selecionar equipa (i = case insensitive)
    cy.get(`a[href*="${receiverTeam.id}" i]`).should('be.visible').click();

    cy.url().should('include', `/team/details/${receiverTeam.id}`);

    cy.intercept('POST', `**/MatchInvite/match-invites`).as('sendInviteRequest');

    cy.get('button').contains('Enviar pedido de partida').click();

    cy.url().should('include', '/team/createMatchInvite');

    // calcualr data
    const gameDate = new Date();
    gameDate.setDate(gameDate.getDate() + 10);
    gameDate.setHours(14, 30, 0, 0);

    // formatar data
    const dateString = gameDate.toISOString().slice(0, 16);

    // preencher formualrio
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
