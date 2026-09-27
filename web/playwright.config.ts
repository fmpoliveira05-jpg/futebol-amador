import { defineConfig, devices } from '@playwright/test';

/**
 * Testes de ponta a ponta (Playwright) contra o modo demonstração: a aplicação corre com dados
 * em memória, por isso os testes não precisam da API, da base de dados nem do Firebase.
 *
 * O projeto `armazenamento` usa a build de produção (configuração `e2e`, sem o modo demonstração)
 * com a API simulada pelo próprio teste, para verificar cookies e armazenamento do browser.
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
    { name: 'desktop', use: { ...devices['Desktop Chrome'] }, testIgnore: /armazenamento/ },
    { name: 'telemovel', use: { ...devices['Pixel 7'] }, testIgnore: /capturas|armazenamento/ },
    {
      name: 'armazenamento',
      use: { ...devices['Desktop Chrome'], baseURL: 'http://localhost:4310' },
      testMatch: /armazenamento/,
    },
  ],
  webServer: [
    {
      command: 'npx ng serve --configuration demo --port 4300',
      url: 'http://localhost:4300',
      reuseExistingServer: !process.env['CI'],
      timeout: 180_000,
    },
    {
      command: 'npx ng serve --configuration e2e --port 4310',
      url: 'http://localhost:4310',
      reuseExistingServer: !process.env['CI'],
      timeout: 180_000,
    },
  ],
});
