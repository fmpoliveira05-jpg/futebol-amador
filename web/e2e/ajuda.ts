import { Page, expect } from '@playwright/test';

/** Entra com a conta de demonstração (administrador dos "Leões da Constituição"). */
export async function entrar(page: Page): Promise<void> {
  await page.goto('/login');
  await page.getByLabel('E-mail').fill('francisco@exemplo.pt');
  await page.getByLabel('Palavra-passe').fill('segredo123');
  await page.getByRole('button', { name: 'Entrar' }).click();
  await expect(page.getByRole('heading', { level: 1, name: 'Leões da Constituição' })).toBeVisible();
}

/** Em ecrãs pequenos o menu está fechado: abre-o antes de clicar numa ligação. */
export async function irPeloMenu(page: Page, nome: string): Promise<void> {
  const menu = page.getByRole('navigation', { name: 'Menu principal' });
  const botao = page.getByRole('button', { name: 'Abrir menu' });
  if (await botao.isVisible()) {
    await botao.click();
  }
  await menu.getByRole('link', { name: nome, exact: true }).click();
}
