/// <reference types="cypress" />

describe('Fluxo completo de criação, aceitação e cancelamento de partida', () => {

  beforeEach(() => {
    cy.setCookie('auth_token', 'fakeTokenWithAdminPrivileges');
    cy.setCookie('is_admin', 'true');
    
    cy.intercept('POST', 'http://localhost:4200/api/MatchInvite/*/match-invites', {
      statusCode: 200,
      body: {
        id: '81ed71ae-5981-4ab0-90af-eda4727ba574',
        sender: { idTeam: '246892f3-78ba-40d6-a14b-cdf5bf9aeb13', name: 'Botafogo' },
        receiver: { idTeam: '51a9c31b-527b-4346-8e7a-757d914a253f', name: 'Patos FC' },
        gameDate: '2025-12-10T19:10:29.467Z',
        namePitch: 'Largo dos Leões'
      }
    }).as('sendMatchInvite');

    cy.intercept('POST', 'http://localhost:4200/api/MatchInvite/*/AcceptMatchInvite', {
      statusCode: 200,
      body: {
        idMatch: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        gameDate: '2025-12-02T22:04:15.092Z',
        nameTeam: 'Patos FC',
        nameOpponent: 'Botafogo',
        namePitch: 'Largo dos Leões'
      }
    }).as('acceptMatchInvite');
    
    cy.intercept('GET', 'http://localhost:4200/api/Calendar/*', {
      statusCode: 200,
      body: [
        {
          idMatch: '37b5fba0-4b81-41a7-b11c-c0164a181268',
          matchStatus: 0,
          gameDate: '2025-12-03T22:14:08.022Z',
          matchResult: 3,
          isCompetitive: false,
          team: {
            idTeam: '51a9c31b-527b-4346-8e7a-757d914a253f',
            name: 'Patos FC',
            numGoals: 0
          },
          opponent: {
            idTeam: 'e152e7ae-e7b5-4cef-9588-d608e7ab292c',
            name: 'Benfica B',
            numGoals: 0
          },
          pitchGame: {
            name: 'Cebolas',
            address: 'Nao sei'
          },
          isHome: false
        }
      ]
    }).as('getMatches');
    
    cy.intercept('DELETE', '/api/Calendar/*/CancelMatch/*', {
      statusCode: 200,
      body: { message: 'Partida cancelada com sucesso' }
    }).as('cancelMatch');
  });

  it('Deve permitir criar, aceitar, pesquisar e cancelar a partida', () => {
    cy.visit('http://localhost:4200/login');
    cy.get('input[formcontrolname="email"]').type('admin.equipa.a@exemplo.pt');
    cy.get('input[formcontrolname="password"]').type('Teste123.');
    cy.get('button[type="submit"]').click();

    cy.wait('@sendMatchInvite');
    cy.wait('@acceptMatchInvite');
    
    cy.get('button.btn.btn-secondary').contains('Ver calendário').click();
    cy.url().should('match', /\/players\/calendar\/[\w-]+/);

    cy.get('button.filter-button').contains('Mostrar Filtros').click();
    cy.get('input#nameOpponent').type('Benfica B');
    cy.get('button').contains('Aplicar Filtros').click();

    cy.wait('@getMatches', { timeout: 10000 }); 
    
    cy.get('.match-card').should('contain', 'Benfica B'); 

    cy.get('.btn.cancel-btn').click(); 

    cy.window().then((win) => {
      cy.stub(win, 'prompt').returns('Um jogador ficou doente');
      cy.stub(win, 'confirm').returns(true);
      cy.stub(win, 'alert');
    });

    cy.get('textarea[name="motivo-cancelamento"]').type('Um jogador ficou doente'); 
    cy.get('button').contains('Confirmar Cancelamento').click(); 

    cy.wait('@cancelMatch'); 

    cy.get('.mensagem-confirmacao').should('be.visible').and('contain', 'Partida cancelada com sucesso'); 
  });
});