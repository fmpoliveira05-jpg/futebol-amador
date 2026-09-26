import { expect, test } from '@playwright/test';
import { entrar, irPeloMenu } from './ajuda';

test.describe('Administrador de equipa', () => {
  test.beforeEach(async ({ page }) => entrar(page));

  test('a página inicial mostra os próximos jogos e a forma recente', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Próximos jogos' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Últimos resultados' })).toBeVisible();
  });

  test('aceita um pedido de adesão e o jogador passa a fazer parte do plantel', async ({ page }) => {
    await irPeloMenu(page, 'Transferências');
    await page.locator('.page-header').getByRole('link', { name: 'Jogadores livres' }).click();
    const linha = page.getByRole('row', { name: /Bruno Teixeira/ });
    await linha.getByRole('button', { name: 'Aceitar' }).click();
    await expect(page.getByRole('status')).toContainText('Bruno Teixeira');

    await irPeloMenu(page, 'Membros');
    await expect(page.getByRole('row', { name: /Bruno Teixeira/ })).toBeVisible();
  });

  test('convida outra equipa para um jogo', async ({ page }) => {
    await irPeloMenu(page, 'Adversários');
    await page.getByRole('link', { name: /Veteranos do Bonfim/ }).click();
    await page.getByRole('link', { name: 'Convidar para jogo' }).click();

    const data = new Date(Date.now() + 5 * 24 * 3600 * 1000);
    const local = new Date(data.getTime() - data.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
    await page.getByLabel('Data do jogo').fill(local);
    await page.getByRole('button', { name: 'Enviar convite' }).click();

    await expect(page).toHaveURL(/\/team\/matchInvites/);
    await expect(page.getByRole('row', { name: /Veteranos do Bonfim/ }).last()).toContainText('À espera de resposta');
  });

  test('aceita um pedido de adiamento', async ({ page }) => {
    await irPeloMenu(page, 'Pedidos de adiamento');
    await page.getByRole('row', { name: /Estrela de Gaia/ }).getByRole('button', { name: 'Aceitar' }).click();
    await expect(page.getByRole('status')).toContainText('adiado');
    await expect(page.getByText('Não há pedidos de adiamento por responder.')).toBeVisible();
  });

  test('o calendário mostra o mês com a legenda e os detalhes de um dia', async ({ page }) => {
    await irPeloMenu(page, 'Calendário');
    await expect(page.getByText('Jogo da liga marcado')).toBeVisible();
    await expect(page.getByText('Feriado', { exact: true })).toBeVisible();
    await page.getByRole('button', { name: 'Mês seguinte' }).click();
    await page.getByRole('button', { name: 'Mês anterior' }).click();
    await page.getByRole('button', { name: 'Hoje' }).click();
    await expect(page.getByRole('heading', { name: 'Próximos jogos' })).toBeVisible();
  });

  test('a classificação tem PD, V, E, D, GM, GS, DG, P e a forma', async ({ page }) => {
    await irPeloMenu(page, 'Classificação');
    for (const coluna of ['PD', 'V', 'E', 'D', 'GM', 'GS', 'DG', 'P']) {
      await expect(page.getByRole('columnheader', { name: coluna, exact: true })).toBeVisible();
    }
    await expect(page.getByRole('img', { name: 'Vitória' }).first()).toBeVisible();
  });

  test('faz uma proposta no mercado', async ({ page }) => {
    await irPeloMenu(page, 'Mercado');
    await page.getByLabel('Tem equipa').selectOption({ label: 'Sim' });
    await page.getByRole('button', { name: 'Aplicar' }).click();
    const linha = page.getByRole('row', { name: /Sérgio Rocha/ });
    await linha.getByRole('button', { name: 'Fazer proposta' }).click();
    await linha.getByRole('button', { name: 'Enviar' }).click();
    await expect(linha).toContainText('Proposta enviada');
  });

  test('regista o resultado e os eventos de um jogo', async ({ page }) => {
    await page.goto('/jogos/jogo-0/resultado');
    await page.getByLabel('Leões da Constituição').fill('2');
    await page.getByLabel('Veteranos do Bonfim').fill('1');
    await page.getByRole('button', { name: '+ Golo' }).click();
    await page.getByRole('button', { name: 'Registar resultado' }).click();
    await expect(page.getByRole('status')).toContainText('o jogo terminou');
  });

  test('termina a sessão', async ({ page }) => {
    await irPeloMenu(page, 'Sair');
    await expect(page).toHaveURL(/\/login/);
    await page.goto('/settings');
    await expect(page).toHaveURL(/\/login/);
  });
});
