/**
 * Dados de exemplo do modo demonstração (`npm run demo`).
 *
 * Representam uma pequena liga do Grande Porto. O utilizador da demonstração é o administrador
 * dos "Leões da Constituição". Os nomes são inventados.
 */

export interface DemoEquipa {
  id: string;
  nome: string;
  descricao: string;
  emblema: string;
  divisao: string;
  pontos: number;
  campo: { name: string; address: string };
  fundada: string;
  idadeMedia: number;
  /** Número de jogadores nas equipas de que não se guardam os membros. */
  jogadores: number;
}

export interface DemoJogador {
  id: string;
  nome: string;
  email: string;
  telefone: string;
  nascimento: string;
  morada: string;
  posicao: number;
  altura: number;
  equipa: string | null;
  admin: boolean;
}

export interface DemoJogo {
  id: string;
  casa: string;
  fora: string;
  data: string;
  /** 0 agendado, 2 terminado, 3 adiado, 4 cancelado (enum `MatchStatus`). */
  estado: number;
  golosCasa: number;
  golosFora: number;
  competitivo: boolean;
}

export interface DemoConvite {
  id: string;
  de: string;
  para: string;
  data: string;
  emCasaDoRemetente: boolean;
}

export interface DemoPedidoAdesao {
  id: string;
  jogador: string;
  equipa: string;
  data: string;
  /** `true` se foi o jogador a pedir; `false` se foi a equipa a convidar. */
  doJogador: boolean;
}

export interface DemoAdiamento {
  jogo: string;
  pedidoPor: string;
  novaData: string;
}

export interface DemoBase {
  equipas: DemoEquipa[];
  jogadores: DemoJogador[];
  jogos: DemoJogo[];
  convites: DemoConvite[];
  pedidos: DemoPedidoAdesao[];
  adiamentos: DemoAdiamento[];
}

export const DEMO_JOGADOR_ID = 'demo-francisco';
export const DEMO_EQUIPA_ID = 'eq-leoes';

/** Emblema simples em SVG: um escudo com as iniciais da equipa. */
function emblema(iniciais: string, cor: string, corTexto = '#ffffff'): string {
  const svg =
    `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64">` +
    `<rect width="64" height="64" fill="${cor}"/>` +
    `<path d="M32 10 L50 17 V32 C50 43 42 51 32 55 C22 51 14 43 14 32 V17 Z" fill="none" stroke="${corTexto}" stroke-width="2.5" opacity="0.6"/>` +
    `<text x="32" y="38" text-anchor="middle" font-family="Arial, sans-serif" font-weight="700" font-size="15" fill="${corTexto}">${iniciais}</text>` +
    `</svg>`;
  return `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`;
}

/** Data relativa a hoje, em ISO, às `hora`:00. */
function dia(deslocamento: number, hora = 20): string {
  const d = new Date();
  d.setDate(d.getDate() + deslocamento);
  d.setHours(hora, 0, 0, 0);
  return d.toISOString();
}

function nascimento(idade: number, mes = 5): string {
  return `${new Date().getFullYear() - idade}-${String(mes).padStart(2, '0')}-14`;
}

export function criarBaseDemo(): DemoBase {
  const equipas: DemoEquipa[] = [
    {
      id: DEMO_EQUIPA_ID,
      nome: 'Leões da Constituição',
      descricao: 'Equipa de amigos que joga às quintas à noite desde os tempos da faculdade.',
      emblema: emblema('LC', '#b45309'),
      divisao: 'Divisão 2',
      pontos: 1240,
      campo: { name: 'Campo da Constituição', address: 'Rua da Constituição, Porto' },
      fundada: '2021-09-15',
      idadeMedia: 27.4,
      jogadores: 8,
    },
    {
      id: 'eq-dragoes',
      nome: 'Dragões de Campanhã',
      descricao: 'Jogamos para ganhar, mas o jantar depois do jogo é sagrado.',
      emblema: emblema('DC', '#1d4ed8'),
      divisao: 'Divisão 1',
      pontos: 1580,
      campo: { name: 'Sintético de Campanhã', address: 'Avenida de Paiva Couceiro, Porto' },
      fundada: '2019-03-02',
      idadeMedia: 29.1,
      jogadores: 11,
    },
    {
      id: 'eq-matosinhos',
      nome: 'Unidos de Matosinhos',
      descricao: 'Da praia para o relvado.',
      emblema: emblema('UM', '#0f766e'),
      divisao: 'Divisão 1',
      pontos: 1420,
      campo: { name: 'Complexo Desportivo do Mar', address: 'Rua Brito Capelo, Matosinhos' },
      fundada: '2020-06-20',
      idadeMedia: 25.8,
      jogadores: 10,
    },
    {
      id: 'eq-boavista',
      nome: 'Académico da Boavista',
      descricao: 'Estudantes e ex-estudantes da zona da Boavista.',
      emblema: emblema('AB', '#111827'),
      divisao: 'Divisão 2',
      pontos: 1105,
      campo: { name: 'Pavilhão da Boavista', address: 'Avenida da Boavista, Porto' },
      fundada: '2022-01-10',
      idadeMedia: 23.2,
      jogadores: 9,
    },
    {
      id: 'eq-ermesinde',
      nome: 'Atlético de Ermesinde',
      descricao: 'Futebol de sábado de manhã, faça chuva ou faça sol.',
      emblema: emblema('AE', '#7c3aed'),
      divisao: 'Divisão 2',
      pontos: 1010,
      campo: { name: 'Campo Municipal de Ermesinde', address: 'Rua Rodrigues de Freitas, Valongo' },
      fundada: '2018-10-05',
      idadeMedia: 31.5,
      jogadores: 12,
    },
    {
      id: 'eq-gaia',
      nome: 'Estrela de Gaia',
      descricao: 'Do outro lado do rio.',
      emblema: emblema('EG', '#dc2626'),
      divisao: 'Divisão 3',
      pontos: 870,
      campo: { name: 'Sintético de Santa Marinha', address: 'Rua Cândido dos Reis, Vila Nova de Gaia' },
      fundada: '2023-02-18',
      idadeMedia: 26.0,
      jogadores: 8,
    },
    {
      id: 'eq-maia',
      nome: 'Rapazes da Maia',
      descricao: 'Recém-chegados à liga, com vontade de subir.',
      emblema: emblema('RM', '#facc15', '#111827'),
      divisao: 'Divisão 3',
      pontos: 790,
      campo: { name: 'Parque Desportivo da Maia', address: 'Avenida Visconde de Barreiros, Maia' },
      fundada: '2024-04-01',
      idadeMedia: 22.7,
      jogadores: 7,
    },
    {
      id: 'eq-bonfim',
      nome: 'Veteranos do Bonfim',
      descricao: 'Mais de 35 anos, muita experiência e alguma velocidade.',
      emblema: emblema('VB', '#4b5563'),
      divisao: 'Divisão 4',
      pontos: 540,
      campo: { name: 'Campo do Bonfim', address: 'Rua do Heroísmo, Porto' },
      fundada: '2017-05-27',
      idadeMedia: 41.3,
      jogadores: 9,
    },
  ];

  const j = (
    id: string,
    nome: string,
    idade: number,
    posicao: number,
    altura: number,
    morada: string,
    equipa: string | null,
    admin = false
  ): DemoJogador => ({
    id,
    nome,
    email: `${id.replace('demo-', '')}@exemplo.pt`,
    telefone: '+351912345678',
    nascimento: nascimento(idade),
    morada,
    posicao,
    altura,
    equipa,
    admin,
  });

  const jogadores: DemoJogador[] = [
    j(DEMO_JOGADOR_ID, 'Francisco Oliveira', 22, 1, 178, 'Porto', DEMO_EQUIPA_ID, true),
    j('demo-rui', 'Rui Martins', 29, 3, 186, 'Porto', DEMO_EQUIPA_ID, true),
    j('demo-tiago', 'Tiago Ferreira', 27, 2, 183, 'Porto', DEMO_EQUIPA_ID),
    j('demo-andre', 'André Sousa', 31, 2, 180, 'Matosinhos', DEMO_EQUIPA_ID),
    j('demo-joao', 'João Carvalho', 25, 1, 175, 'Porto', DEMO_EQUIPA_ID),
    j('demo-miguel', 'Miguel Costa', 28, 1, 172, 'Vila Nova de Gaia', DEMO_EQUIPA_ID),
    j('demo-pedro', 'Pedro Almeida', 26, 0, 177, 'Porto', DEMO_EQUIPA_ID),
    j('demo-diogo', 'Diogo Ribeiro', 30, 0, 181, 'Maia', DEMO_EQUIPA_ID),
    // Sem equipa (aparecem em "Recrutar jogadores").
    j('demo-bruno', 'Bruno Teixeira', 24, 0, 179, 'Porto', null),
    j('demo-nuno', 'Nuno Pinto', 33, 3, 188, 'Valongo', null),
    j('demo-hugo', 'Hugo Moreira', 21, 1, 170, 'Matosinhos', null),
    j('demo-ricardo', 'Ricardo Lopes', 27, 2, 185, 'Porto', null),
    j('demo-vasco', 'Vasco Gomes', 36, 2, 182, 'Vila Nova de Gaia', null),
    j('demo-gil', 'Gil Fernandes', 19, 0, 174, 'Maia', null),
    // Alguns membros de outras equipas.
    j('demo-sergio', 'Sérgio Rocha', 30, 1, 176, 'Porto', 'eq-dragoes', true),
    j('demo-luis', 'Luís Barbosa', 27, 0, 180, 'Matosinhos', 'eq-matosinhos', true),
  ];

  // Completa o plantel das outras equipas com jogadores gerados a partir de listas de nomes.
  const nomes = ['Carlos', 'Daniel', 'Eduardo', 'Fábio', 'Gonçalo', 'Henrique', 'Ivo', 'Jorge', 'Leonardo', 'Mário', 'Nélson', 'Óscar', 'Paulo', 'Rafael', 'Samuel', 'Tomás'];
  const apelidos = ['Silva', 'Santos', 'Pereira', 'Oliveira', 'Rodrigues', 'Fernandes', 'Gonçalves', 'Marques', 'Pires', 'Cunha', 'Mendes', 'Faria'];
  let n = 0;
  for (const e of equipas.filter((x) => x.id !== DEMO_EQUIPA_ID)) {
    const cidade = e.campo.address.split(', ').pop() ?? 'Porto';
    for (let i = jogadores.filter((p) => p.equipa === e.id).length; i < e.jogadores; i++, n++) {
      const nome = `${nomes[n % nomes.length]} ${apelidos[(n * 7) % apelidos.length]}`;
      const idadeJogador = Math.round(e.idadeMedia + ((n % 7) - 3) * 1.5);
      jogadores.push(j(`demo-${e.id}-${i}`, nome, idadeJogador, n % 4, 168 + ((n * 5) % 22), cidade, e.id, i === 0));
    }
  }

  const jogo = (
    id: string,
    casa: string,
    fora: string,
    dias: number,
    estado: number,
    golosCasa = 0,
    golosFora = 0,
    competitivo = true
  ): DemoJogo => ({ id, casa, fora, data: dia(dias), estado, golosCasa, golosFora, competitivo });

  const jogos: DemoJogo[] = [
    jogo('jogo-1', DEMO_EQUIPA_ID, 'eq-gaia', -42, 2, 4, 1),
    jogo('jogo-2', 'eq-boavista', DEMO_EQUIPA_ID, -35, 2, 2, 2),
    jogo('jogo-3', DEMO_EQUIPA_ID, 'eq-dragoes', -28, 2, 1, 3),
    jogo('jogo-4', 'eq-maia', DEMO_EQUIPA_ID, -21, 2, 0, 2, false),
    jogo('jogo-5', DEMO_EQUIPA_ID, 'eq-ermesinde', -14, 2, 3, 2),
    jogo('jogo-6', 'eq-matosinhos', DEMO_EQUIPA_ID, -7, 2, 1, 2),
    jogo('jogo-7', DEMO_EQUIPA_ID, 'eq-bonfim', -3, 4, 0, 0, false),
    jogo('jogo-8', DEMO_EQUIPA_ID, 'eq-boavista', 4, 0),
    jogo('jogo-9', 'eq-gaia', DEMO_EQUIPA_ID, 11, 0),
    jogo('jogo-10', DEMO_EQUIPA_ID, 'eq-maia', 18, 0, 0, 0, false),
    jogo('jogo-11', 'eq-ermesinde', DEMO_EQUIPA_ID, 25, 3),
    jogo('jogo-12', 'eq-dragoes', 'eq-matosinhos', 6, 0),
  ];

  return {
    equipas,
    jogadores,
    jogos,
    convites: [
      { id: 'conv-1', de: 'eq-matosinhos', para: DEMO_EQUIPA_ID, data: dia(9, 21), emCasaDoRemetente: true },
      { id: 'conv-2', de: 'eq-bonfim', para: DEMO_EQUIPA_ID, data: dia(15, 19), emCasaDoRemetente: false },
      { id: 'conv-3', de: DEMO_EQUIPA_ID, para: 'eq-dragoes', data: dia(21, 20), emCasaDoRemetente: true },
    ],
    pedidos: [
      { id: 'ped-1', jogador: 'demo-bruno', equipa: DEMO_EQUIPA_ID, data: dia(-2, 18), doJogador: true },
      { id: 'ped-2', jogador: 'demo-ricardo', equipa: DEMO_EQUIPA_ID, data: dia(-1, 12), doJogador: true },
      { id: 'ped-3', jogador: 'demo-nuno', equipa: DEMO_EQUIPA_ID, data: dia(-4, 10), doJogador: false },
    ],
    adiamentos: [{ jogo: 'jogo-9', pedidoPor: 'eq-gaia', novaData: dia(13, 21) }],
  };
}
