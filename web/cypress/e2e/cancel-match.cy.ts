/// <reference types="cypress" />

describe('Fluxo completo do cancelamento de partida', () => {
  it('Deve permitir login, acessar ao perfil, pesquisar uma partida e cancelar a partida', () => {

    cy.visit('http://localhost:4200/login');
    cy.wait(1000);

    cy.get('input[formcontrolname="email"]').should('be.visible').type('admin.equipa.a@exemplo.pt');
    cy.get('input[formcontrolname="password"]').should('be.visible').type('Teste123.');
    cy.get('button[type="submit"]').click();

    cy.get('button.btn.btn-secondary').contains('Ver calendário').should('be.visible').click();

    cy.url().should('match', /\/players\/calendar\/[\w-]+/);

    cy.get('button.filter-button').contains('Mostrar Filtros').click();

    cy.get('input#nameOpponent').type('benfica b');

    cy.get('button').contains('Aplicar Filtros').click();

    cy.get('.match-card').should('contain', 'benfica b');

    cy.get('.btn.cancel-btn').click();

    cy.window().then((win) => {
      cy.stub(win, 'prompt').returns('Um jogador ficou doente'); 
      cy.stub(win, 'confirm').returns(true); 
      cy.stub(win, 'alert'); 
    });

    cy.get('textarea[name="motivo-cancelamento"]').type('Um jogador ficou doente');

    cy.get('.mensagem-confirmacao').should('contain', 'Partida cancelada com sucesso');
  });
});