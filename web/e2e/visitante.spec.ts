import { expect, test } from '@playwright/test';

test.describe('Visitante', () => {
  test('vê a apresentação e a classificação', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { level: 1 })).toContainText('Junta a tua equipa');

    await page.getByRole('link', { name: 'Consulta a classificação' }).click();
    await expect(page.getByRole('heading', { name: 'Classificação', exact: true })).toBeVisible();
    await expect(page.getByRole('row').nth(1)).toContainText('Dragões de Campanhã');
  });

  test('é enviado para o login ao abrir uma página protegida, e volta a ela depois de entrar', async ({ page }) => {
    await page.goto('/team/members');
    await expect(page).toHaveURL(/\/login\?voltar=%2Fteam%2Fmembers/);

    await page.getByLabel('E-mail').fill('francisco@exemplo.pt');
    await page.getByLabel('Palavra-passe').fill('segredo123');
    await page.getByRole('button', { name: 'Entrar' }).click();
    await expect(page.getByRole('heading', { name: 'Membros da equipa' })).toBeVisible();
  });

  test('o formulário de login valida os campos', async ({ page }) => {
    await page.goto('/login');
    await page.getByRole('button', { name: 'Entrar' }).click();
    await expect(page.getByText('O e-mail é obrigatório.')).toBeVisible();
  });

  test('um endereço inexistente mostra a página 404', async ({ page }) => {
    await page.goto('/isto-nao-existe');
    await expect(page.getByRole('heading', { level: 1, name: 'Página não encontrada' })).toBeVisible();
    await page.getByRole('link', { name: 'Voltar à página inicial' }).click();
    await expect(page).toHaveURL(/\/$/);
  });

  test('a Política de Privacidade está no rodapé e o registo exige aceitá-la', async ({ page }) => {
    await page.goto('/signup');
    await page.locator('footer.rodape').getByRole('link', { name: 'Política de Privacidade' }).click();
    await expect(page.getByRole('heading', { level: 1, name: 'Política de Privacidade' })).toBeVisible();
    await expect(page.getByText('__Host-fa_session').first()).toBeVisible();

    await page.goto('/signup');
    await page.getByRole('button', { name: 'Criar conta' }).click();
    await expect(page.getByText('Para criar a conta tens de aceitar a Política de Privacidade.')).toBeVisible();
  });
});
