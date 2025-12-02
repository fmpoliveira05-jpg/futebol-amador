/// <reference types="cypress" />

describe('Fluxo completo de pesquisa e cancelamento de partida', () => {

  beforeEach(() => {
    cy.setCookie('auth_token', 'fakeTokenWithAdminPrivileges');
    cy.setCookie('is_admin', 'true');
  });

  it('Deve permitir pesquisar e cancelar a partida', () => {
    cy.visit('http://localhost:4200/login');
    cy.get('input[formcontrolname="email"]').type('admin.equipa.a@exemplo.pt');
    cy.get('input[formcontrolname="password"]').type('Teste123.');
    cy.get('button[type="submit"]').click();

    cy.get('button.btn.btn-secondary').contains('Ver calendário').click();
    cy.url().should('match', /\/players\/calendar\/[\w-]+/);

    cy.get('button.filter-button').contains('Mostrar Filtros').click();
    cy.get('input#nameOpponent').type('Botafogo'); 
    cy.get('button').contains('Aplicar Filtros').click();

    cy.get('.match-card').should('contain', 'Botafogo'); 

    cy.get('.btn.cancel-btn').click(); 

    cy.window().then((win) => {
      cy.stub(win, 'prompt').returns('Um jogador ficou doente');
      cy.stub(win, 'confirm').returns(true);
      cy.stub(win, 'alert');
    });

    cy.get('textarea[name="motivo-cancelamento"]').type('Um jogador ficou doente');
    cy.get('button').contains('Confirmar Cancelamento').click();

    //cy.get('.mensagem-confirmacao').should('contain', 'Partida cancelada com sucesso!');
  });
});