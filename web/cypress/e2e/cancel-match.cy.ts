/// <reference types="cypress" />

describe('Fluxo completo de pesquisa e cancelamento de partida', () => {

  const senderUser = {
    email: 'admin.equipa.a@exemplo.pt',
    password: 'Teste123.'
  };

  const receiverUser = {
    email: 'player@player.com',
    password: 'Teste123.'
  };

  const senderTeamId = '51A9C31B-527B-4346-8E7A-757D914A253F'; 
  const receiverTeamId = '246892f3-78ba-40d6-a14b-cdf5bf9aeb13'; 

  let senderToken = '';
  let receiverToken = '';
  let createdInviteId = '';

  const randomHour = Math.floor(Math.random() * (20 - 10 + 1)) + 10;
  const uniqueDate = `2026-08-20T${randomHour}:00:00`; 

  before(() => {
    cy.log(`🚀 Iniciando Setup: Criando partida para ${uniqueDate}`);

    cy.request({
      method: 'POST',
      url: 'http://localhost:5218/api/User/login',
      body: senderUser
    }).then((resp) => {
      senderToken = resp.body.firebaseLoginResponseDto.idToken;

      cy.request({
        method: 'POST',
        url: `http://localhost:5218/api/MatchInvite/${senderTeamId}/match-invites`,
        headers: { Authorization: `Bearer ${senderToken}` },
        body: {
          idSender: senderTeamId,
          idReceiver: receiverTeamId,
          gameDate: uniqueDate,
          homePitch: true
        },
        failOnStatusCode: true
      }).then((inviteResp) => {
        
        createdInviteId = inviteResp.body.id;
        cy.log(`✅ Convite Enviado. ID: ${createdInviteId}`);

        cy.request({
          method: 'POST',
          url: 'http://localhost:5218/api/User/login',
          body: receiverUser
        }).then((receiverResp) => {
          receiverToken = receiverResp.body.firebaseLoginResponseDto.idToken;

          cy.request({
            method: 'POST',
            url: `http://localhost:5218/api/MatchInvite/${receiverTeamId}/AcceptMatchInvite`,
            headers: { 
                'Authorization': `Bearer ${receiverToken}`,
                'Content-Type': 'application/json' 
            },
            body: JSON.stringify(createdInviteId), 
            failOnStatusCode: true 
          }).then(() => {
             cy.log('✅ Convite Aceite! Partida Criada.');
          });
        });
      });
    });
  });

  beforeEach(() => {
    if (senderToken) {
      cy.setCookie('access_token', senderToken);
      cy.setCookie('is_admin', 'true', { path: '/' });
    }
  });

  it('Deve permitir pesquisar e cancelar a partida', () => {
    cy.visit('http://localhost:4200/login');
    
    cy.get('body').then(($body) => {
        if ($body.find('input[formcontrolname="email"]').length > 0) {
            cy.get('input[formcontrolname="email"]').type(senderUser.email);
            cy.get('input[formcontrolname="password"]').type(senderUser.password);
            cy.get('button[type="submit"]').click();
        }
    });

    cy.get('button.btn.btn-secondary').contains('Ver calendário').should('be.visible').click();
    cy.url().should('match', /\/players\/calendar\/[\w-]+/);

    cy.get('button.filter-button').contains('Mostrar Filtros').click();
    cy.get('input#nameOpponent').type('Botafogo'); 
    
    cy.intercept('GET', '**/api/Calendar/**').as('searchMatches');
    cy.get('button').contains('Aplicar Filtros').click();
    cy.wait('@searchMatches');

    cy.get('.match-card').should('contain', 'Botafogo'); 
    
    cy.get('.match-card').first().find('.btn.cancel-btn').click(); 
    cy.url().should('include', 'cancel-match');

    cy.get('textarea[name="motivo-cancelamento"]')
        .should('be.visible')
        .type('Jogador bateu o joelho.');
    
    cy.get('button').contains('Confirmar Cancelamento').click();

    cy.url().should('include', '/players/calendar');
  });
});