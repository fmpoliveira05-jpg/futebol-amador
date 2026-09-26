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
  /** Liga em que joga (id de `DemoLiga`). */
  liga: string | null;
  /** Quem criou a equipa (o administrador principal). */
  criador: string | null;
  titulos: DemoTitulo[];
}

export interface DemoTitulo {
  trofeu: string;
  liga: string;
  epoca: string;
}

export interface DemoLiga {
  id: string;
  nome: string;
  nivel: number;
  sobem: number;
  descem: number;
  dias: number;
  trofeu: string;
}

export interface DemoEpoca {
  id: string;
  liga: string;
  nome: string;
  /** 0 inscrições, 1 a decorrer, 2 terminada. */
  estado: number;
  inicio: string;
  fim: string;
  equipas: string[];
}

export interface DemoProposta {
  id: string;
  jogador: string;
  de: string;
  para: string;
  /** Enum `TransferOfferStatus`. */
  estado: number;
  mensagem: string | null;
  viaMercado: boolean;
  data: string;
  decidida: string | null;
}

export interface DemoMovimento {
  jogador: string;
  de: string | null;
  para: string | null;
  tipo: 'TRANSFERENCIA' | 'ADESAO' | 'SAIDA';
  data: string;
}

export interface DemoOnze {
  jogo: string;
  equipa: string;
  tatica: string;
  titulares: Record<number, string>;
  suplentes: string[];
  automatico: boolean;
}

export interface DemoEvento {
  jogo: string;
  equipa: string;
  tipo: 'GOAL' | 'YELLOW_CARD' | 'RED_CARD' | 'SUBSTITUTION';
  minuto: number | null;
  jogador: string | null;
  relacionado: string | null;
}

export interface DemoMarca {
  jogo: string;
  data: string;
  tipo: 'CANCELLED' | 'POSTPONED';
  motivo: string | null;
  novaData: string | null;
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
  nacionalidade: string | null;
  paisNascimento: string | null;
  peso: number | null;
  /** 0 direito, 1 esquerdo, 2 ambos. */
  pe: number | null;
  /** 0 ativo, 1 lesionado, 2 indisponível. */
  situacao: number;
  /** Na equipa desde. */
  desde: string | null;
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
  epoca?: string;
  jornada?: number;
  /** Motivo do cancelamento ou do adiamento. */
  motivo?: string;
  /** Data original, se foi adiado. */
  adiadoDe?: string;
  faltasCasa?: number | null;
  faltasFora?: number | null;
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
  motivo?: string;
}

export interface DemoBase {
  equipas: DemoEquipa[];
  jogadores: DemoJogador[];
  jogos: DemoJogo[];
  convites: DemoConvite[];
  pedidos: DemoPedidoAdesao[];
  adiamentos: DemoAdiamento[];
  ligas: DemoLiga[];
  epocas: DemoEpoca[];
  listados: string[];
  propostas: DemoProposta[];
  movimentos: DemoMovimento[];
  onzes: DemoOnze[];
  eventos: DemoEvento[];
  marcas: DemoMarca[];
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
      liga: 'liga-1',
      criador: null,
      titulos: [{ trofeu: 'Troféu Divisão 2', liga: 'Divisão 2', epoca: '2025/26' }],
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
      liga: 'liga-1',
      criador: null,
      titulos: [
        { trofeu: 'Troféu Divisão 1', liga: 'Divisão 1', epoca: '2024/25' },
        { trofeu: 'Troféu Divisão 1', liga: 'Divisão 1', epoca: '2025/26' },
      ],
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
      liga: 'liga-1',
      criador: null,
      titulos: [],
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
      liga: 'liga-1',
      criador: null,
      titulos: [],
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
      liga: 'liga-1',
      criador: null,
      titulos: [],
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
      liga: 'liga-1',
      criador: null,
      titulos: [],
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
      liga: 'liga-2',
      criador: null,
      titulos: [],
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
      liga: 'liga-2',
      criador: null,
      titulos: [],
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
    nacionalidade: 'Portugal',
    paisNascimento: 'Portugal',
    peso: 64 + ((altura * 7) % 25),
    pe: altura % 5 === 0 ? 1 : 0,
    situacao: 0,
    desde: equipa ? '2024-09-01T00:00:00.000Z' : null,
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
    j('demo-filipe', 'Filipe Nunes', 24, 2, 184, 'Porto', DEMO_EQUIPA_ID),
    j('demo-leandro', 'Leandro Tavares', 26, 2, 179, 'Gondomar', DEMO_EQUIPA_ID),
    j('demo-marco', 'Marco Antunes', 23, 1, 174, 'Porto', DEMO_EQUIPA_ID),
    j('demo-ze', 'José Monteiro', 32, 3, 190, 'Porto', DEMO_EQUIPA_ID),
    j('demo-kiko', 'Kiko Semedo', 21, 0, 176, 'Matosinhos', DEMO_EQUIPA_ID),
    // Sem equipa (aparecem em "Jogadores livres" e no mercado).
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

  // Alguns jogadores com outras nacionalidades, para os filtros do mercado.
  const nacionalidades: Record<string, [string, string]> = {
    'demo-miguel': ['Brasil', 'Brasil'],
    'demo-kiko': ['Cabo Verde', 'Cabo Verde'],
    'demo-leandro': ['Angola', 'Portugal'],
    'demo-hugo': ['Brasil', 'Brasil'],
    'demo-nuno': ['Espanha', 'Espanha'],
    'demo-sergio': ['Cabo Verde', 'Portugal'],
  };
  for (const p of jogadores) {
    const n = nacionalidades[p.id];
    if (n) {
      [p.nacionalidade, p.paisNascimento] = n;
    }
  }
  jogadores.filter((p) => p.equipa && p.id.startsWith('demo-eq-')).forEach((p, i) => {
    if (i % 5 === 2) {
      p.nacionalidade = 'Brasil';
    }
  });
  jogadores.find((p) => p.id === 'demo-rui')!.desde = '2021-09-20T00:00:00.000Z';
  jogadores.find((p) => p.id === DEMO_JOGADOR_ID)!.desde = '2021-09-15T00:00:00.000Z';
  jogadores.find((p) => p.id === 'demo-joao')!.situacao = 1;
  for (const e of equipas) {
    e.criador = e.id === DEMO_EQUIPA_ID ? DEMO_JOGADOR_ID : jogadores.find((p) => p.equipa === e.id && p.admin)?.id ?? null;
  }

  const epoca = 'ep-d1-2627';
  const jogo = (
    id: string,
    casa: string,
    fora: string,
    dias: number,
    estado: number,
    golosCasa = 0,
    golosFora = 0,
    competitivo = true,
    jornada?: number
  ): DemoJogo => ({
    id,
    casa,
    fora,
    data: dia(dias),
    estado,
    golosCasa,
    golosFora,
    competitivo,
    ...(competitivo ? { epoca, jornada } : {}),
    ...(estado === 2 ? { faltasCasa: 8 + (golosCasa * 3) % 7, faltasFora: 10 + (golosFora * 5) % 6 } : {}),
  });

  const jogos: DemoJogo[] = [
    jogo('jogo-1', DEMO_EQUIPA_ID, 'eq-gaia', -42, 2, 4, 1, true, 1),
    jogo('jogo-2', 'eq-boavista', DEMO_EQUIPA_ID, -35, 2, 2, 2, true, 2),
    jogo('jogo-3', DEMO_EQUIPA_ID, 'eq-dragoes', -28, 2, 1, 3, true, 3),
    jogo('jogo-4', 'eq-maia', DEMO_EQUIPA_ID, -21, 2, 0, 2, false),
    jogo('jogo-5', DEMO_EQUIPA_ID, 'eq-ermesinde', -14, 2, 3, 2, true, 4),
    jogo('jogo-6', 'eq-matosinhos', DEMO_EQUIPA_ID, -7, 2, 1, 2, true, 5),
    { ...jogo('jogo-7', DEMO_EQUIPA_ID, 'eq-bonfim', -3, 4, 0, 0, false), motivo: 'Falta de jogadores' },
    { ...jogo('jogo-8', DEMO_EQUIPA_ID, 'eq-boavista', 4, 0, 0, 0, true, 6), adiadoDe: dia(2), motivo: 'Campo em obras' },
    jogo('jogo-9', 'eq-gaia', DEMO_EQUIPA_ID, 11, 0, 0, 0, true, 7),
    jogo('jogo-10', DEMO_EQUIPA_ID, 'eq-maia', 18, 0, 0, 0, false),
    { ...jogo('jogo-11', 'eq-ermesinde', DEMO_EQUIPA_ID, 25, 3, 0, 0, true, 9), motivo: 'Torneio de futsal no mesmo dia' },
    jogo('jogo-12', 'eq-dragoes', 'eq-matosinhos', 6, 0, 0, 0, true, 6),
    // Amigável de ontem ainda sem resultado: serve para experimentar o formulário de fim de jogo.
    { ...jogo('jogo-0', DEMO_EQUIPA_ID, 'eq-bonfim', -1, 0, 0, 0, false), data: dia(-1, 19) },
    // Jogos da liga entre as outras equipas (para a classificação).
    jogo('jogo-13', 'eq-dragoes', 'eq-boavista', -42, 2, 2, 0, true, 1),
    jogo('jogo-14', 'eq-matosinhos', 'eq-ermesinde', -42, 2, 2, 2, true, 1),
    jogo('jogo-15', 'eq-gaia', 'eq-matosinhos', -35, 2, 1, 3, true, 2),
    jogo('jogo-16', 'eq-ermesinde', 'eq-dragoes', -35, 2, 1, 1, true, 2),
    jogo('jogo-17', 'eq-boavista', 'eq-ermesinde', -28, 2, 0, 1, true, 3),
    jogo('jogo-18', 'eq-matosinhos', 'eq-gaia', -28, 2, 2, 0, true, 3),
    jogo('jogo-19', 'eq-dragoes', 'eq-gaia', -14, 2, 4, 0, true, 4),
    jogo('jogo-20', 'eq-boavista', 'eq-matosinhos', -14, 2, 1, 3, true, 4),
    jogo('jogo-21', 'eq-gaia', 'eq-boavista', -7, 2, 0, 1, true, 5),
    jogo('jogo-22', 'eq-ermesinde', 'eq-dragoes', -7, 2, 0, 2, true, 5),
  ];

  const onze433 = (titulares: string[], suplentes: string[]): Pick<DemoOnze, 'titulares' | 'suplentes'> => ({
    titulares: Object.fromEntries(titulares.map((id, i) => [i, id])),
    suplentes,
  });
  const onzeLeoes = onze433(
    ['demo-rui', 'demo-leandro', 'demo-tiago', 'demo-andre', 'demo-filipe', 'demo-francisco', 'demo-marco', 'demo-miguel', 'demo-kiko', 'demo-pedro', 'demo-diogo'],
    ['demo-joao', 'demo-ze']
  );

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
    adiamentos: [{ jogo: 'jogo-9', pedidoPor: 'eq-gaia', novaData: dia(13, 21), motivo: 'Metade da equipa está num casamento' }],
    ligas: [
      { id: 'liga-1', nome: 'Divisão 1', nivel: 1, sobem: 0, descem: 2, dias: 120, trofeu: 'Troféu Divisão 1' },
      { id: 'liga-2', nome: 'Divisão 2', nivel: 2, sobem: 2, descem: 0, dias: 120, trofeu: 'Troféu Divisão 2' },
    ],
    epocas: [
      {
        id: epoca,
        liga: 'liga-1',
        nome: '2026/27',
        estado: 1,
        inicio: dia(-42, 0),
        fim: dia(78, 0),
        equipas: ['eq-dragoes', 'eq-matosinhos', DEMO_EQUIPA_ID, 'eq-boavista', 'eq-ermesinde', 'eq-gaia'],
      },
      {
        id: 'ep-d2-2627',
        liga: 'liga-2',
        nome: '2026/27',
        estado: 0,
        inicio: dia(12, 0),
        fim: dia(132, 0),
        equipas: ['eq-maia', 'eq-bonfim'],
      },
    ],
    listados: ['demo-andre'],
    propostas: [
      { id: 'prop-1', jogador: 'demo-joao', de: DEMO_EQUIPA_ID, para: 'eq-dragoes', estado: 0, mensagem: 'Precisamos de um médio para a segunda volta.', viaMercado: false, data: dia(-1, 11), decidida: null },
      { id: 'prop-2', jogador: 'demo-luis', de: 'eq-matosinhos', para: DEMO_EQUIPA_ID, estado: 0, mensagem: null, viaMercado: false, data: dia(-3, 19), decidida: null },
      { id: 'prop-3', jogador: 'demo-tiago', de: DEMO_EQUIPA_ID, para: 'eq-boavista', estado: 3, mensagem: null, viaMercado: false, data: dia(-20, 10), decidida: dia(-19, 10) },
    ],
    movimentos: [
      { jogador: DEMO_JOGADOR_ID, de: null, para: DEMO_EQUIPA_ID, tipo: 'ADESAO', data: '2021-09-15T00:00:00.000Z' },
      { jogador: 'demo-miguel', de: 'eq-boavista', para: DEMO_EQUIPA_ID, tipo: 'TRANSFERENCIA', data: dia(-60, 12) },
      { jogador: 'demo-miguel', de: null, para: 'eq-boavista', tipo: 'ADESAO', data: '2023-02-01T00:00:00.000Z' },
    ],
    onzes: [
      { jogo: 'jogo-5', equipa: DEMO_EQUIPA_ID, tatica: '4-3-3', automatico: false, ...onzeLeoes },
      { jogo: 'jogo-6', equipa: DEMO_EQUIPA_ID, tatica: '4-3-3', automatico: false, ...onzeLeoes },
      { jogo: 'jogo-0', equipa: DEMO_EQUIPA_ID, tatica: '4-3-3', automatico: true, ...onzeLeoes },
    ],
    eventos: [
      { jogo: 'jogo-5', equipa: DEMO_EQUIPA_ID, tipo: 'GOAL', minuto: 12, jogador: 'demo-pedro', relacionado: DEMO_JOGADOR_ID },
      { jogo: 'jogo-5', equipa: DEMO_EQUIPA_ID, tipo: 'GOAL', minuto: 40, jogador: DEMO_JOGADOR_ID, relacionado: 'demo-kiko' },
      { jogo: 'jogo-5', equipa: DEMO_EQUIPA_ID, tipo: 'GOAL', minuto: 77, jogador: 'demo-diogo', relacionado: null },
      { jogo: 'jogo-5', equipa: DEMO_EQUIPA_ID, tipo: 'YELLOW_CARD', minuto: 55, jogador: 'demo-tiago', relacionado: null },
      { jogo: 'jogo-6', equipa: DEMO_EQUIPA_ID, tipo: 'GOAL', minuto: 23, jogador: 'demo-kiko', relacionado: DEMO_JOGADOR_ID },
      { jogo: 'jogo-6', equipa: DEMO_EQUIPA_ID, tipo: 'GOAL', minuto: 81, jogador: 'demo-joao', relacionado: 'demo-marco' },
      { jogo: 'jogo-6', equipa: DEMO_EQUIPA_ID, tipo: 'YELLOW_CARD', minuto: 64, jogador: 'demo-andre', relacionado: null },
      { jogo: 'jogo-6', equipa: DEMO_EQUIPA_ID, tipo: 'SUBSTITUTION', minuto: 70, jogador: 'demo-diogo', relacionado: 'demo-joao' },
    ],
    marcas: [{ jogo: 'jogo-8', data: dia(2), tipo: 'POSTPONED', motivo: 'Campo em obras', novaData: dia(4) }],
  };
}
