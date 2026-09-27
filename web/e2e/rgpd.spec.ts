import { expect, test } from '@playwright/test';
import { entrar } from './ajuda';

test.describe('Os meus dados (RGPD)', () => {
  test('descarrega os dados em JSON e elimina a conta com a palavra-passe', async ({ page }) => {
    await entrar(page);
    await page.goto('/settings');

    const descarga = page.waitForEvent('download');
    await page.getByRole('button', { name: 'Descarregar os meus dados' }).click();
    const ficheiro = await descarga;
    expect(ficheiro.suggestedFilename()).toBe('futebol-amador-os-meus-dados.json');

    await page.getByRole('button', { name: 'Eliminar a minha conta' }).click();
    await expect(page.getByText('Indica a palavra-passe para confirmar.')).toBeVisible();

    await page.getByLabel('Palavra-passe atual').last().fill('segredo123');
    await page.getByLabel('Percebo que a eliminação é definitiva.').check();
    await page.getByRole('button', { name: 'Eliminar a minha conta' }).click();

    await expect(page).toHaveURL(/conta=eliminada/);
    expect(await page.evaluate(() => localStorage.getItem('fa_sessao'))).toBeNull();
  });
});
