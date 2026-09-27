import { BrowserContext, Page, Route, expect, test } from '@playwright/test';

/**
 * Armazenamento no browser e cookies da sessão, contra a build de produção (sem o modo
 * demonstração). A API é simulada aqui (`page.route`) com as mesmas respostas e cabeçalhos
 * `Set-Cookie` da API real para o browser.
 *
 * Confirma que:
 * - o localStorage e o sessionStorage não têm tokens (JWT) nem dados pessoais (e-mail, nome,
 *   telefone, morada, data de nascimento);
 * - os cookies com tokens são HttpOnly, Secure e SameSite=Strict (invisíveis ao JavaScript);
 * - terminar a sessão apaga os cookies e o armazenamento.
 */

const JOGADOR = {
  id: 'uid-e2e-123',
  nome: 'Francisca Armazenamento',
  email: 'francisca.e2e@exemplo.pt',
  telefone: '+351934567890',
  morada: 'Rua das Flores 12, Guimarães',
  nascimento: '1997-04-21',
};
const EQUIPA = 'eq-e2e-1';
const ID_TOKEN = 'eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJ1aWQtZTJlLTEyMyJ9.assinatura-id-token';
const REFRESH_TOKEN = 'AMf-refresh-token-e2e-segredo';

const cookiesSessao = [
  `__Host-fa_session=${ID_TOKEN}; Path=/; Secure; HttpOnly; SameSite=Strict; Max-Age=3600`,
  `__Secure-fa_refresh=${REFRESH_TOKEN}; Path=/api/User/refresh; Secure; HttpOnly; SameSite=Strict; Max-Age=604800`,
];
const cookiesApagados = [
  '__Host-fa_session=; Path=/; Secure; HttpOnly; SameSite=Strict; Expires=Thu, 01 Jan 1970 00:00:00 GMT',
  '__Secure-fa_refresh=; Path=/api/User/refresh; Secure; HttpOnly; SameSite=Strict; Expires=Thu, 01 Jan 1970 00:00:00 GMT',
];

const detalhes = {
  id: JOGADOR.id,
  name: JOGADOR.nome,
  email: JOGADOR.email,
  phone: JOGADOR.telefone,
  address: JOGADOR.morada,
  dateOfBirth: JOGADOR.nascimento,
  position: 1,
  height: 172,
  isAdmin: true,
  team: { idTeam: EQUIPA, name: 'Equipa E2E' },
};

async function json(route: Route, corpo: unknown, estado = 200, setCookie?: string[]): Promise<void> {
  await route.fulfill({
    status: estado,
    contentType: 'application/json',
    headers: setCookie ? { 'set-cookie': setCookie.join('\n') } : {},
    body: estado === 204 ? '' : JSON.stringify(corpo),
  });
}

/** API simulada: login e logout como a API real (cookies), o resto com respostas mínimas. */
async function simularApi(page: Page): Promise<void> {
  await page.route('**/api/**', async (route) => {
    const pedido = route.request();
    const caminho = new URL(pedido.url()).pathname.replace(/^\/api\//, '');
    const metodo = pedido.method();

    if (metodo === 'POST' && caminho === 'User/login') {
      return json(route, { name: JOGADOR.nome, email: JOGADOR.email, idTeam: EQUIPA, isAdmin: true,
        firebaseLoginResponseDto: { localId: JOGADOR.id, expiresIn: '3600' } }, 200, cookiesSessao);
    }
    if (metodo === 'POST' && caminho === 'User/logout') {
      return json(route, null, 204, cookiesApagados);
    }
    if (metodo === 'GET' && (caminho === `Player/details/${JOGADOR.id}` || caminho === 'Player/get-my-profile')) {
      return json(route, detalhes);
    }
    if (metodo === 'GET' && caminho === `Player/${JOGADOR.id}/profile`) {
      return json(route, { ...detalhes, seasons: [], transfers: [], titles: [] });
    }
    if (metodo === 'GET' && caminho.startsWith('Team/homeTeam/')) {
      return json(route, { nextMatches: [], lastMatches: [], sequence: [] });
    }
    if (metodo === 'GET') {
      return json(route, []);
    }
    return json(route, null, 204);
  });
}

async function armazenamento(page: Page): Promise<string> {
  return page.evaluate(() => {
    const tudo: Record<string, string | null> = {};
    for (const area of [localStorage, sessionStorage]) {
      for (let i = 0; i < area.length; i++) {
        const chave = area.key(i)!;
        tudo[`${area === localStorage ? 'local' : 'session'}:${chave}`] = area.getItem(chave);
      }
    }
    return JSON.stringify(tudo);
  });
}

function semDadosSensiveis(texto: string): void {
  expect(texto, 'sem JWT').not.toMatch(/eyJ[\w-]+\.[\w-]+/);
  expect(texto).not.toContain(REFRESH_TOKEN);
  expect(texto.toLowerCase()).not.toContain('token');
  for (const valor of [JOGADOR.email, JOGADOR.nome, JOGADOR.telefone, JOGADOR.morada, JOGADOR.nascimento, 'Guimarães']) {
    expect(texto, `sem "${valor}"`).not.toContain(valor);
  }
}

async function cookiesDaSessao(contexto: BrowserContext) {
  return (await contexto.cookies()).filter((c) => c.name.startsWith('__Host-fa_') || c.name.startsWith('__Secure-fa_'));
}

test.describe('Armazenamento no browser e cookies', () => {
  test('não guarda tokens nem dados pessoais e os cookies são HttpOnly e Secure', async ({ page, context }) => {
    await simularApi(page);

    await page.goto('/login');
    await page.getByLabel('E-mail').fill(JOGADOR.email);
    await page.getByLabel('Palavra-passe').fill('Segredo#2026x');
    await page.getByRole('button', { name: 'Entrar' }).click();
    await expect(page).not.toHaveURL(/\/login/);

    // Páginas que mostram dados pessoais (perfil e definições) não os copiam para o armazenamento.
    await page.goto('/players/me');
    await page.goto('/settings');
    await expect(page.getByRole('heading', { name: 'Definições' })).toBeVisible();

    const guardado = await armazenamento(page);
    semDadosSensiveis(guardado);
    // Só a indicação para a interface (id interno, equipa, administrador).
    expect(JSON.parse(guardado)['local:fa_sessao']).toBeTruthy();

    const cookies = await cookiesDaSessao(context);
    expect(cookies.map((c) => c.name).sort()).toEqual(['__Host-fa_session', '__Secure-fa_refresh']);
    for (const c of cookies) {
      expect(c.httpOnly, `${c.name} HttpOnly`).toBe(true);
      expect(c.secure, `${c.name} Secure`).toBe(true);
      expect(c.sameSite, `${c.name} SameSite`).toBe('Strict');
    }
    expect(cookies.find((c) => c.name === '__Secure-fa_refresh')!.path).toBe('/api/User/refresh');

    // O JavaScript da página não consegue ler os cookies.
    expect(await page.evaluate(() => document.cookie)).not.toContain('fa_session');
  });

  test('terminar a sessão apaga os cookies e o armazenamento', async ({ page, context }) => {
    await simularApi(page);

    await page.goto('/login');
    await page.getByLabel('E-mail').fill(JOGADOR.email);
    await page.getByLabel('Palavra-passe').fill('Segredo#2026x');
    await page.getByRole('button', { name: 'Entrar' }).click();
    await expect(page).not.toHaveURL(/\/login/);
    expect(await cookiesDaSessao(context)).toHaveLength(2);

    await page.goto('/logout');
    await expect(page).toHaveURL(/\/login/);

    expect(await cookiesDaSessao(context)).toHaveLength(0);
    const guardado = JSON.parse(await armazenamento(page));
    expect(guardado['local:fa_sessao']).toBeUndefined();
    expect(Object.keys(guardado).filter((k) => k.startsWith('session:'))).toEqual([]);
  });
});
