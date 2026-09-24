import { expect, test } from '@playwright/test';
import { entrar, irPeloMenu } from './ajuda';

test.describe('Administrador de equipa', () => {
  test.beforeEach(async ({ page }) => entrar(page));

  test('a página inicial mostra os próximos jogos e a forma recente', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Próximos jogos' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Últimos resultados' })).toBeVisible();
  });

  test('aceita um pedido de adesão e o jogador passa a fazer parte do plantel', async ({ page }) => {
    await irPeloMenu(page, 'Pedidos de adesão');
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

  test('filtra o calendário por jogos realizados', async ({ page }) => {
    await irPeloMenu(page, 'Calendário');
    await page.getByRole('button', { name: 'Mostrar filtros' }).click();
    await page.getByLabel('Já realizado?').selectOption({ label: 'Sim' });
    await page.getByRole('button', { name: 'Aplicar filtros' }).click();
    await expect(page.locator('.match-card').first()).toContainText('Terminado');
    await expect(page.getByText('Agendado')).toHaveCount(0);
  });

  test('termina a sessão', async ({ page }) => {
    await irPeloMenu(page, 'Sair');
    await expect(page).toHaveURL(/\/login/);
    await page.goto('/settings');
    await expect(page).toHaveURL(/\/login/);
  });
});
