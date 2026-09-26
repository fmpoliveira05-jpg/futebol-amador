import { test } from '@playwright/test';
import { entrar } from './ajuda';

/**
 * Gera as capturas de ecrã usadas no README (`docs/capturas`). Não corre por omissão:
 * `CAPTURAS=1 npx playwright test capturas --project=desktop`.
 */
test.skip(!process.env['CAPTURAS'], 'Só corre com CAPTURAS=1');
test.use({ viewport: { width: 1360, height: 860 } });

const pasta = '../docs/capturas';

test('capturas para o README', async ({ page }) => {
  await page.goto('/');
  await page.screenshot({ path: `${pasta}/web-inicio.png` });
  await entrar(page);
  await page.screenshot({ path: `${pasta}/web-painel.png` });

  const paginas: Array<[string, string]> = [
    ['web-equipa', '/team/details/eq-leoes'],
    ['web-calendario', '/players/calendar/eq-leoes'],
    ['web-adversarios', '/teams'],
    ['web-convites', '/team/matchInvites'],
    ['web-classificacao', '/leaderboard'],
    ['web-ligas', '/ligas'],
    ['web-mercado', '/mercado'],
    ['web-transferencias', '/transferencias'],
    ['web-onze', '/jogos/jogo-8/onze'],
    ['web-relatorio', '/jogos/jogo-6'],
    ['web-resultado', '/jogos/jogo-0/resultado'],
    ['web-jogador', '/players/details/demo-kiko'],
  ];
  for (const [nome, url] of paginas) {
    await page.goto(url);
    await page.waitForLoadState('networkidle');
    await page.screenshot({ path: `${pasta}/${nome}.png` });
  }
});
