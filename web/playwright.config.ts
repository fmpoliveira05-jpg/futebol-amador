import { defineConfig, devices } from '@playwright/test';

/**
 * Testes de ponta a ponta (Playwright) contra o modo demonstração: a aplicação corre com dados
 * em memória, por isso os testes não precisam da API, da base de dados nem do Firebase.
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 1 : 0,
  reporter: process.env['CI'] ? [['github'], ['html', { open: 'never' }]] : 'list',
  use: {
    baseURL: 'http://localhost:4300',
    locale: 'pt-PT',
    trace: 'retain-on-failure',
  },
  projects: [
    { name: 'desktop', use: { ...devices['Desktop Chrome'] } },
    { name: 'telemovel', use: { ...devices['Pixel 7'] }, testIgnore: /capturas/ },
  ],
  webServer: {
    command: 'npx ng serve --configuration demo --port 4300',
    url: 'http://localhost:4300',
    reuseExistingServer: !process.env['CI'],
    timeout: 180_000,
  },
});
