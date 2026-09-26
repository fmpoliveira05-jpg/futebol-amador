import { HttpRequest } from '@angular/common/http';
import {
  DEMO_JOGADOR_ID,
  DemoBase,
  DemoEquipa,
  DemoEvento,
  DemoJogador,
  DemoJogo,
  DemoProposta,
} from './dados-demo';

/**
 * Rotas do modo demonstração para as ligas, transferências, onzes, relatório do jogo e perfil.
 * Replicam as regras da API (docs/novas-funcionalidades.md) sobre os dados em memória.
 */

type Resposta = { corpo?: unknown; estado?: number };
type Handler = (req: HttpRequest<unknown>, params: string[]) => Resposta;

/** Funções do interceptor de que estas rotas precisam. */
export interface ContextoDemo {
  base: () => DemoBase;
  ok: (corpo?: unknown) => Resposta;
  erro: (estado: number, detail: string) => never;
  equipa: (id: string) => DemoEquipa;
  jogador: (id: string) => DemoJogador;
  membros: (idEquipa: string) => DemoJogador[];
  exigirAdmin: (idEquipa: string) => void;
  novoId: (prefixo: string) => string;
  jsonCorpo: <T>(req: HttpRequest<unknown>) => T;
  param: (req: HttpRequest<unknown>, nome: string) => string | null;
  idade: (nascimento: string) => number;
}

// ---------- Táticas (as mesmas da API) ----------

const PAPEL: Record<string, number> = { GR: 3, DD: 2, DC: 2, DE: 2, MDC: 1, MC: 1, MD: 1, ME: 1, MOC: 1, ED: 0, EE: 0, PL: 0 };

const DEFINICOES: Array<[string, Array<[string, number, number]>]> = [
  ['4-4-2', [['DD', 85, 72], ['DC', 62, 76], ['DC', 38, 76], ['DE', 15, 72], ['MD', 85, 48], ['MC', 62, 52], ['MC', 38, 52], ['ME', 15, 48], ['PL', 62, 22], ['PL', 38, 22]]],
  ['4-3-3', [['DD', 85, 72], ['DC', 62, 76], ['DC', 38, 76], ['DE', 15, 72], ['MC', 72, 50], ['MDC', 50, 56], ['MC', 28, 50], ['ED', 82, 24], ['PL', 50, 18], ['EE', 18, 24]]],
  ['4-2-3-1', [['DD', 85, 72], ['DC', 62, 76], ['DC', 38, 76], ['DE', 15, 72], ['MDC', 62, 58], ['MDC', 38, 58], ['ED', 82, 36], ['MOC', 50, 38], ['EE', 18, 36], ['PL', 50, 16]]],
  ['3-5-2', [['DC', 72, 76], ['DC', 50, 78], ['DC', 28, 76], ['MD', 90, 48], ['MC', 68, 52], ['MDC', 50, 58], ['MC', 32, 52], ['ME', 10, 48], ['PL', 62, 22], ['PL', 38, 22]]],
  ['3-4-3', [['DC', 72, 76], ['DC', 50, 78], ['DC', 28, 76], ['MD', 85, 50], ['MC', 62, 54], ['MC', 38, 54], ['ME', 15, 50], ['ED', 80, 24], ['PL', 50, 18], ['EE', 20, 24]]],
  ['5-3-2', [['DD', 90, 66], ['DC', 70, 76], ['DC', 50, 78], ['DC', 30, 76], ['DE', 10, 66], ['MC', 72, 50], ['MC', 50, 54], ['MC', 28, 50], ['PL', 62, 22], ['PL', 38, 22]]],
  ['4-5-1', [['DD', 85, 72], ['DC', 62, 76], ['DC', 38, 76], ['DE', 15, 72], ['MD', 88, 46], ['MC', 68, 52], ['MDC', 50, 58], ['MC', 32, 52], ['ME', 12, 46], ['PL', 50, 18]]],
];

export const TATICAS = DEFINICOES.map(([code, posicoes]) => ({
  code,
  slots: [{ slot: 0, positionCode: 'GR', role: 3, x: 50, y: 92 }].concat(
    posicoes.map(([pos, x, y], i) => ({ slot: i + 1, positionCode: pos, role: PAPEL[pos], x, y }))
  ),
}));

const PRAZO_HORAS = 2;

/** Época desportiva de uma data (agosto a julho). */
function epocaDesportiva(iso: string): string {
  const d = new Date(iso);
  const inicio = d.getMonth() >= 7 ? d.getFullYear() : d.getFullYear() - 1;
  return `${inicio}/${String((inicio + 1) % 100).padStart(2, '0')}`;
}

export function rotasCompeticao(c: ContextoDemo): Array<[string, RegExp, Handler]> {
  const b = () => c.base();

  // ---------- Ligas ----------

  const epocaAtual = (idLiga: string) => {
    const epocas = b().epocas.filter((e) => e.liga === idLiga);
    return epocas.find((e) => e.estado === 1) ?? epocas.find((e) => e.estado === 0) ?? epocas[epocas.length - 1] ?? null;
  };

  const epocaDto = (e: DemoBase['epocas'][number]) => ({
    id: e.id,
    leagueId: e.liga,
    name: e.nome,
    status: e.estado,
    startDate: e.inicio,
    endDate: e.fim,
    teamCount: e.equipas.length,
  });

  const ligaDto = (l: DemoBase['ligas'][number]) => {
    const e = epocaAtual(l.id);
    return {
      id: l.id,
      name: l.nome,
      level: l.nivel,
      promotionSpots: l.sobem,
      relegationSpots: l.descem,
      seasonDurationDays: l.dias,
      trophyName: l.trofeu,
      teamCount: b().equipas.filter((x) => x.liga === l.id).length,
      currentSeason: e ? epocaDto(e) : null,
    };
  };

  const classificacao = (idLiga: string) => {
    const liga = b().ligas.find((l) => l.id === idLiga) ?? c.erro(404, 'A liga não existe.');
    const epoca = epocaAtual(idLiga);
    const equipas = epoca?.equipas ?? b().equipas.filter((e) => e.liga === idLiga).map((e) => e.id);
    const linhas = new Map(
      equipas.map((id) => [id, { id, j: 0, v: 0, e: 0, d: 0, gm: 0, gs: 0, forma: [] as string[] }])
    );
    const jogos = b()
      .jogos.filter((j) => epoca && j.epoca === epoca.id && j.estado === 2)
      .sort((x, y) => x.data.localeCompare(y.data));
    for (const j of jogos) {
      const casa = linhas.get(j.casa);
      const fora = linhas.get(j.fora);
      if (!casa || !fora) {
        continue;
      }
      casa.j++;
      fora.j++;
      casa.gm += j.golosCasa;
      casa.gs += j.golosFora;
      fora.gm += j.golosFora;
      fora.gs += j.golosCasa;
      const [rc, rf] = j.golosCasa > j.golosFora ? ['V', 'D'] : j.golosCasa < j.golosFora ? ['D', 'V'] : ['E', 'E'];
      for (const [l, r] of [[casa, rc], [fora, rf]] as const) {
        l.forma.push(r);
        if (r === 'V') l.v++;
        else if (r === 'E') l.e++;
        else l.d++;
      }
    }
    const temAcima = b().ligas.some((l) => l.nivel < liga.nivel);
    const temAbaixo = b().ligas.some((l) => l.nivel > liga.nivel);
    const ordenadas = [...linhas.values()].sort(
      (x, y) =>
        y.v * 3 + y.e - (x.v * 3 + x.e) ||
        y.gm - y.gs - (x.gm - x.gs) ||
        y.gm - x.gm ||
        c.equipa(x.id).nome.localeCompare(c.equipa(y.id).nome, 'pt')
    );
    return {
      league: ligaDto(liga),
      season: epoca ? epocaDto(epoca) : null,
      rows: ordenadas.map((l, i) => ({
        position: i + 1,
        teamId: l.id,
        teamName: c.equipa(l.id).nome,
        icon: c.equipa(l.id).emblema,
        played: l.j,
        won: l.v,
        drawn: l.e,
        lost: l.d,
        goalsFor: l.gm,
        goalsAgainst: l.gs,
        goalDifference: l.gm - l.gs,
        points: l.v * 3 + l.e,
        form: l.forma.slice(-5),
        zone:
          temAcima && i < liga.sobem
            ? 'PROMOTION'
            : temAbaixo && i >= ordenadas.length - liga.descem && i >= liga.sobem
              ? 'RELEGATION'
              : null,
      })),
    };
  };

  const ligaDeTopo = () => [...b().ligas].sort((x, y) => x.nivel - y.nivel)[0];

  // ---------- Transferências ----------

  const propostaDto = (p: DemoProposta) => ({
    id: p.id,
    playerId: p.jogador,
    playerName: c.jogador(p.jogador).nome,
    fromTeamId: p.de,
    fromTeamName: c.equipa(p.de).nome,
    toTeamId: p.para,
    toTeamName: c.equipa(p.para).nome,
    status: p.estado,
    message: p.mensagem,
    viaListing: p.viaMercado,
    createdAt: p.data,
    decidedAt: p.decidida,
  });

  const eu = () => c.jogador(DEMO_JOGADOR_ID);
  const souAdminDe = (idEquipa: string) => eu().equipa === idEquipa && eu().admin;

  const concluirTransferencia = (p: DemoProposta) => {
    const j = c.jogador(p.jogador);
    const agora = new Date().toISOString();
    b().movimentos.push({ jogador: j.id, de: p.de, para: p.para, tipo: 'TRANSFERENCIA', data: agora });
    const equipaAntiga = c.equipa(p.de);
    if (equipaAntiga.criador === j.id) {
      equipaAntiga.criador = c.membros(p.de).find((m) => m.admin && m.id !== j.id)?.id ?? null;
    }
    j.equipa = p.para;
    j.admin = false;
    j.desde = agora;
    b().listados = b().listados.filter((id) => id !== j.id);
    p.estado = 2;
    p.decidida = agora;
    for (const outra of b().propostas) {
      if (outra !== p && outra.jogador === j.id && outra.estado <= 1) {
        outra.estado = 4;
        outra.decidida = agora;
      }
    }
  };

  // ---------- Onzes, eventos e estatísticas ----------

  const onzeDto = (j: DemoJogo, idEquipa: string) => {
    const o = b().onzes.find((x) => x.jogo === j.id && x.equipa === idEquipa);
    const prazo = new Date(new Date(j.data).getTime() - PRAZO_HORAS * 3600_000);
    const tatica = TATICAS.find((t) => t.code === o?.tatica);
    const linha = (id: string, slot: number | null) => {
      const p = c.jogador(id);
      return {
        slot,
        positionCode: slot !== null ? tatica?.slots[slot]?.positionCode ?? null : null,
        playerId: id,
        playerName: p.nome,
        position: p.posicao,
      };
    };
    return {
      matchId: j.id,
      teamId: idEquipa,
      formation: o?.tatica ?? null,
      deadline: prazo.toISOString(),
      isLocked: j.estado !== 0 || Date.now() >= prazo.getTime(),
      isAutoFilled: o?.automatico ?? false,
      exists: !!o,
      starters: o ? Object.entries(o.titulares).map(([slot, id]) => linha(id, Number(slot))).sort((x, y) => x.slot! - y.slot!) : [],
      bench: o ? o.suplentes.map((id) => linha(id, null)) : [],
    };
  };

  const jogoPorId = (id: string) => b().jogos.find((j) => j.id === id) ?? c.erro(404, 'O jogo não existe.');

  const revelado = (j: DemoJogo) => j.estado !== 0 || Date.now() >= new Date(j.data).getTime() - PRAZO_HORAS * 3600_000;

  const nome = (id: string | null) => (id ? b().jogadores.find((p) => p.id === id)?.nome ?? null : null);

  const relatorioEquipa = (j: DemoJogo, idEquipa: string) => {
    const ev = b().eventos.filter((e) => e.jogo === j.id && e.equipa === idEquipa);
    const emCasa = j.casa === idEquipa;
    return {
      teamId: idEquipa,
      teamName: c.equipa(idEquipa).nome,
      goals: j.estado === 2 ? (emCasa ? j.golosCasa : j.golosFora) : null,
      fouls: (emCasa ? j.faltasCasa : j.faltasFora) ?? null,
      yellowCards: ev.filter((e) => e.tipo === 'YELLOW_CARD').length,
      redCards: ev.filter((e) => e.tipo === 'RED_CARD').length,
      substitutions: ev.filter((e) => e.tipo === 'SUBSTITUTION').length,
      lineup: revelado(j) && b().onzes.some((o) => o.jogo === j.id && o.equipa === idEquipa) ? onzeDto(j, idEquipa) : null,
      events: ev
        .sort((x, y) => (x.minuto ?? 999) - (y.minuto ?? 999))
        .map((e) => ({
          type: e.tipo,
          minute: e.minuto,
          playerId: e.jogador,
          playerName: nome(e.jogador),
          relatedPlayerId: e.relacionado,
          relatedPlayerName: nome(e.relacionado),
        })),
    };
  };

  /** Minutos de um jogador num jogo (titular 90, suplente desde que entra, sai na substituição ou expulsão). */
  const minutosJogados = (idJogador: string, titular: boolean, ev: DemoEvento[]): number | null => {
    let entrou: number | null = titular ? 0 : null;
    if (entrou === null) {
      const s = ev.find((e) => e.tipo === 'SUBSTITUTION' && e.relacionado === idJogador);
      if (!s) {
        return null;
      }
      entrou = Math.min(90, s.minuto ?? 90);
    }
    let saiu = 90;
    for (const e of ev) {
      if ((e.tipo === 'SUBSTITUTION' || e.tipo === 'RED_CARD') && e.jogador === idJogador && (e.minuto ?? 90) >= entrou) {
        saiu = Math.min(saiu, Math.min(90, e.minuto ?? 90));
      }
    }
    return Math.max(0, saiu - entrou);
  };

  const perfil = (id: string) => {
    const p = c.jogador(id);
    const linhas = new Map<string, { season: string; teamId: string; teamName: string; games: number; goals: number; assists: number; minutes: number; yellowCards: number; redCards: number }>();
    for (const j of b().jogos.filter((x) => x.estado === 2)) {
      const onze = b().onzes.find((o) => o.jogo === j.id && (Object.values(o.titulares).includes(id) || o.suplentes.includes(id)));
      const ev = b().eventos.filter((e) => e.jogo === j.id);
      const meus = ev.filter((e) => e.jogador === id || e.relacionado === id);
      const equipaId = onze?.equipa ?? meus[0]?.equipa;
      if (!equipaId) {
        continue;
      }
      const evEquipa = ev.filter((e) => e.equipa === equipaId);
      const min = onze ? minutosJogados(id, Object.values(onze.titulares).includes(id), evEquipa) : null;
      const epoca = j.epoca ? b().epocas.find((e) => e.id === j.epoca)?.nome ?? epocaDesportiva(j.data) : epocaDesportiva(j.data);
      const chave = `${epoca}|${equipaId}`;
      const l = linhas.get(chave) ?? { season: epoca, teamId: equipaId, teamName: c.equipa(equipaId).nome, games: 0, goals: 0, assists: 0, minutes: 0, yellowCards: 0, redCards: 0 };
      if (min !== null || (!onze && meus.length)) {
        l.games++;
      }
      l.minutes += min ?? 0;
      l.goals += evEquipa.filter((e) => e.tipo === 'GOAL' && e.jogador === id).length;
      l.assists += evEquipa.filter((e) => e.tipo === 'GOAL' && e.relacionado === id).length;
      l.yellowCards += evEquipa.filter((e) => e.tipo === 'YELLOW_CARD' && e.jogador === id).length;
      l.redCards += evEquipa.filter((e) => e.tipo === 'RED_CARD' && e.jogador === id).length;
      linhas.set(chave, l);
    }
    const career = [...linhas.values()].sort((x, y) => y.season.localeCompare(x.season));
    const soma = (k: 'games' | 'goals' | 'assists' | 'minutes' | 'yellowCards' | 'redCards') => career.reduce((t, l) => t + l[k], 0);
    const e = p.equipa ? c.equipa(p.equipa) : null;
    return {
      id: p.id,
      name: p.nome,
      imageUrl: null,
      dateOfBirth: p.nascimento,
      age: c.idade(p.nascimento),
      position: p.posicao,
      height: p.altura,
      weight: p.peso,
      preferredFoot: p.pe,
      status: p.situacao,
      nationality: p.nacionalidade,
      countryOfBirth: p.paisNascimento,
      currentTeam: e ? { idTeam: e.id, name: e.nome } : null,
      joinedTeamAt: p.equipa ? p.desde : null,
      isListed: b().listados.includes(p.id),
      totals: { games: soma('games'), goals: soma('goals'), assists: soma('assists'), minutes: soma('minutes'), yellowCards: soma('yellowCards'), redCards: soma('redCards') },
      career,
      transfers: b()
        .movimentos.filter((m) => m.jogador === id)
        .sort((x, y) => y.data.localeCompare(x.data))
        .map((m) => ({
          date: m.data,
          fromTeamName: m.de ? b().equipas.find((x) => x.id === m.de)?.nome ?? null : null,
          toTeamName: m.para ? b().equipas.find((x) => x.id === m.para)?.nome ?? null : null,
          kind: m.tipo,
        })),
    };
  };

  return [
    // Ligas e classificação
    ['GET', /^leagues$/, () => c.ok([...b().ligas].sort((x, y) => x.nivel - y.nivel).map(ligaDto))],
    ['GET', /^leagues\/([^/]+)\/standings$/, (_, [id]) => c.ok(classificacao(id))],
    [
      'GET',
      /^Leaderboard$/,
      (req) => {
        const id = c.param(req, 'leagueId') ?? ligaDeTopo()?.id;
        return id ? c.ok(classificacao(id)) : { estado: 204 };
      },
    ],
    [
      'GET',
      /^leagues\/seasons\/([^/]+)\/fixtures$/,
      (_, [idEpoca]) => {
        const porJornada = new Map<number, DemoJogo[]>();
        for (const j of b().jogos.filter((x) => x.epoca === idEpoca)) {
          porJornada.set(j.jornada ?? 0, [...(porJornada.get(j.jornada ?? 0) ?? []), j]);
        }
        return c.ok(
          [...porJornada.entries()]
            .sort(([x], [y]) => x - y)
            .map(([round, jogos]) => ({
              round,
              matches: jogos
                .sort((x, y) => x.data.localeCompare(y.data))
                .map((j) => ({
                  idMatch: j.id,
                  date: j.data,
                  status: j.estado,
                  homeTeamId: j.casa,
                  homeTeamName: c.equipa(j.casa).nome,
                  awayTeamId: j.fora,
                  awayTeamName: c.equipa(j.fora).nome,
                  homeGoals: j.estado === 2 ? j.golosCasa : null,
                  awayGoals: j.estado === 2 ? j.golosFora : null,
                })),
            }))
        );
      },
    ],
    [
      'POST',
      /^leagues\/([^/]+)\/register\/([^/]+)$/,
      (_, [idLiga, idEquipa]) => {
        c.exigirAdmin(idEquipa);
        const epoca = b().epocas.find((e) => e.liga === idLiga && e.estado === 0) ?? c.erro(400, 'Esta liga não tem inscrições abertas.');
        if (b().epocas.some((e) => e.estado === 1 && e.equipas.includes(idEquipa))) {
          c.erro(400, 'A equipa está a meio de uma época noutra liga.');
        }
        for (const e of b().epocas.filter((x) => x.estado === 0)) {
          e.equipas = e.equipas.filter((x) => x !== idEquipa);
        }
        epoca.equipas.push(idEquipa);
        c.equipa(idEquipa).liga = idLiga;
        return c.ok(epocaDto(epoca));
      },
    ],
    [
      'GET',
      /^Team\/([^/]+)\/titles$/,
      (_, [id]) => c.ok(agruparTitulos(c.equipa(id))),
    ],

    // Transferências
    [
      'GET',
      /^transfers\/market\/([^/]+)$/,
      (req, [idEquipa]) => {
        c.exigirAdmin(idEquipa);
        const temEquipa = c.param(req, 'hasTeam');
        const liga = c.param(req, 'leagueId');
        const nac = c.param(req, 'nationality')?.toLowerCase();
        const pos = c.param(req, 'position');
        const nomeFiltro = c.param(req, 'name')?.toLowerCase();
        const soListados = c.param(req, 'onlyListed') === 'true';
        return c.ok(
          b()
            .jogadores.filter(
              (p) =>
                p.equipa !== idEquipa &&
                (temEquipa === null || !!p.equipa === (temEquipa === 'true')) &&
                (!liga || (p.equipa && c.equipa(p.equipa).liga === liga)) &&
                (!nac || p.nacionalidade?.toLowerCase() === nac) &&
                (pos === null || p.posicao === Number(pos)) &&
                (!nomeFiltro || p.nome.toLowerCase().includes(nomeFiltro)) &&
                (!soListados || b().listados.includes(p.id))
            )
            .sort((x, y) => x.nome.localeCompare(y.nome, 'pt'))
            .map((p) => {
              const e = p.equipa ? c.equipa(p.equipa) : null;
              const l = e?.liga ? b().ligas.find((x) => x.id === e.liga) : null;
              return {
                playerId: p.id,
                name: p.nome,
                age: c.idade(p.nascimento),
                position: p.posicao,
                nationality: p.nacionalidade,
                imageUrl: null,
                teamId: e?.id ?? null,
                teamName: e?.nome ?? null,
                leagueId: l?.id ?? null,
                leagueName: l?.nome ?? null,
                isListed: b().listados.includes(p.id),
              };
            })
        );
      },
    ],
    [
      'POST',
      /^transfers\/listings\/([^/]+)\/([^/]+)$/,
      (_, [idEquipa, idJogador]) => {
        c.exigirAdmin(idEquipa);
        if (c.jogador(idJogador).equipa !== idEquipa) {
          c.erro(403, 'Só se podem listar jogadores da própria equipa.');
        }
        if (!b().listados.includes(idJogador)) {
          b().listados.push(idJogador);
        }
        return { estado: 204 };
      },
    ],
    [
      'DELETE',
      /^transfers\/listings\/([^/]+)\/([^/]+)$/,
      (_, [idEquipa, idJogador]) => {
        c.exigirAdmin(idEquipa);
        b().listados = b().listados.filter((x) => x !== idJogador);
        return { estado: 204 };
      },
    ],
    [
      'POST',
      /^transfers\/offers$/,
      (req) => {
        const d = c.jsonCorpo<{ teamId: string; playerId: string; message?: string | null }>(req);
        c.exigirAdmin(d.teamId);
        const j = c.jogador(d.playerId);
        if (!j.equipa) {
          c.erro(400, 'O jogador não tem equipa: convida-o pelos pedidos de adesão.');
        }
        if (j.equipa === d.teamId) {
          c.erro(400, 'O jogador já é da tua equipa.');
        }
        if (b().propostas.some((p) => p.jogador === j.id && p.para === d.teamId && p.estado <= 1)) {
          c.erro(400, 'Já existe uma proposta por este jogador à espera de resposta.');
        }
        const listado = b().listados.includes(j.id);
        const p: DemoProposta = {
          id: c.novoId('prop'),
          jogador: j.id,
          de: j.equipa!,
          para: d.teamId,
          estado: listado ? 1 : 0,
          mensagem: d.message || null,
          viaMercado: listado,
          data: new Date().toISOString(),
          decidida: null,
        };
        b().propostas.push(p);
        return c.ok(propostaDto(p));
      },
    ],
    [
      'GET',
      /^transfers\/offers\/team\/([^/]+)$/,
      (_, [id]) => {
        c.exigirAdmin(id);
        const ordem = (x: DemoProposta, y: DemoProposta) => y.data.localeCompare(x.data);
        return c.ok({
          received: b().propostas.filter((p) => p.de === id).sort(ordem).map(propostaDto),
          sent: b().propostas.filter((p) => p.para === id).sort(ordem).map(propostaDto),
        });
      },
    ],
    [
      'GET',
      /^transfers\/offers\/player$/,
      () => c.ok(b().propostas.filter((p) => p.jogador === DEMO_JOGADOR_ID && p.estado === 1).map(propostaDto)),
    ],
    [
      'POST',
      /^transfers\/offers\/([^/]+)\/(accept|reject)$/,
      (_, [id, acao]) => {
        const p = b().propostas.find((x) => x.id === id) ?? c.erro(404, 'A proposta não existe.');
        if (p.estado > 1) {
          c.erro(400, 'A proposta já foi decidida.');
        }
        if (acao === 'reject') {
          if (p.jogador === DEMO_JOGADOR_ID || souAdminDe(p.de)) {
            p.estado = 3;
          } else if (souAdminDe(p.para)) {
            p.estado = 4;
          } else {
            c.erro(403, 'Não tens permissão para responder a esta proposta.');
          }
          p.decidida = new Date().toISOString();
          return c.ok(propostaDto(p));
        }
        if (p.estado === 0) {
          if (!souAdminDe(p.de)) {
            c.erro(403, 'Só um administrador da equipa do jogador pode aceitar a proposta.');
          }
          p.estado = 1;
          // Na demonstração, os jogadores das outras equipas respondem logo que sim.
          if (p.jogador !== DEMO_JOGADOR_ID) {
            concluirTransferencia(p);
          }
          return c.ok(propostaDto(p));
        }
        if (p.jogador !== DEMO_JOGADOR_ID) {
          c.erro(403, 'Só o jogador pode aceitar a transferência.');
        }
        concluirTransferencia(p);
        return c.ok(propostaDto(p));
      },
    ],

    // Perfil
    ['GET', /^Player\/([^/]+)\/profile$/, (_, [id]) => c.ok(perfil(id))],

    // Onze inicial
    ['GET', /^lineups\/formations$/, () => c.ok(TATICAS)],
    [
      'GET',
      /^lineups\/([^/]+)\/([^/]+)$/,
      (_, [idJogo, idEquipa]) => {
        const j = jogoPorId(idJogo);
        if (eu().equipa !== idEquipa && !revelado(j)) {
          c.erro(403, 'O onze do adversário só é revelado depois do prazo.');
        }
        return c.ok(onzeDto(j, idEquipa));
      },
    ],
    [
      'PUT',
      /^lineups\/([^/]+)\/([^/]+)$/,
      (req, [idJogo, idEquipa]) => {
        c.exigirAdmin(idEquipa);
        const j = jogoPorId(idJogo);
        if (j.estado !== 0) {
          c.erro(400, 'Só se define o onze de jogos marcados.');
        }
        if (Date.now() >= new Date(j.data).getTime() - PRAZO_HORAS * 3600_000) {
          c.erro(400, 'O prazo para definir o onze terminou (2 horas antes do jogo).');
        }
        const d = c.jsonCorpo<{ formation: string; starters: { slot: number; playerId: string }[]; bench: string[] }>(req);
        const plantel = new Set(c.membros(idEquipa).map((p) => p.id));
        const todos = [...d.starters.map((s) => s.playerId), ...d.bench];
        if (todos.some((id) => !plantel.has(id)) || new Set(todos).size !== todos.length) {
          c.erro(400, 'Só podem ser escolhidos jogadores da equipa, uma vez cada.');
        }
        if (d.starters.length !== Math.min(11, plantel.size)) {
          c.erro(400, `O onze tem de ter ${Math.min(11, plantel.size)} titulares.`);
        }
        b().onzes = b().onzes.filter((o) => !(o.jogo === idJogo && o.equipa === idEquipa));
        b().onzes.push({
          jogo: idJogo,
          equipa: idEquipa,
          tatica: d.formation,
          titulares: Object.fromEntries(d.starters.map((s) => [s.slot, s.playerId])),
          suplentes: d.bench,
          automatico: false,
        });
        return c.ok(onzeDto(j, idEquipa));
      },
    ],

    // Relatório e resultado
    [
      'GET',
      /^matches\/([^/]+)\/report$/,
      (_, [id]) => {
        const j = jogoPorId(id);
        const epoca = j.epoca ? b().epocas.find((e) => e.id === j.epoca) : null;
        return c.ok({
          matchId: j.id,
          date: j.data,
          status: j.estado,
          isCompetitive: j.competitivo,
          leagueName: epoca ? b().ligas.find((l) => l.id === epoca.liga)?.nome ?? null : null,
          round: j.jornada ?? null,
          pitchName: c.equipa(j.casa).campo.name,
          home: relatorioEquipa(j, j.casa),
          away: relatorioEquipa(j, j.fora),
        });
      },
    ],
    [
      'POST',
      /^matches\/([^/]+)\/result$/,
      (req, [id]) => {
        const j = jogoPorId(id);
        const d = c.jsonCorpo<{
          idTeam: string;
          numGoalsTeam: number;
          numGoalsOpponent: number;
          events: {
            fouls: number | null;
            goals: { scorerId: string | null; assistId: string | null; minute: number | null }[];
            cards: { playerId: string; type: number; minute: number | null }[];
            substitutions: { playerOutId: string; playerInId: string; minute: number | null }[];
          } | null;
        }>(req);
        c.exigirAdmin(d.idTeam);
        if (j.estado === 2) {
          c.erro(400, 'O jogo já terminou.');
        }
        if (new Date(j.data).getTime() + 90 * 60_000 > Date.now()) {
          c.erro(400, 'O jogo ainda não acabou: o resultado regista-se depois de 90 minutos.');
        }
        if ((d.events?.goals.length ?? 0) > d.numGoalsTeam) {
          c.erro(400, `Registaste ${d.events!.goals.length} golos, mas a equipa marcou ${d.numGoalsTeam}.`);
        }
        // Na demonstração o adversário confirma logo o mesmo resultado.
        const emCasa = j.casa === d.idTeam;
        j.golosCasa = emCasa ? d.numGoalsTeam : d.numGoalsOpponent;
        j.golosFora = emCasa ? d.numGoalsOpponent : d.numGoalsTeam;
        j.estado = 2;
        if (emCasa) j.faltasCasa = d.events?.fouls ?? null;
        else j.faltasFora = d.events?.fouls ?? null;
        b().eventos = b().eventos.filter((e) => !(e.jogo === j.id && e.equipa === d.idTeam));
        for (const g of d.events?.goals ?? []) {
          b().eventos.push({ jogo: j.id, equipa: d.idTeam, tipo: 'GOAL', minuto: g.minute, jogador: g.scorerId, relacionado: g.assistId });
        }
        for (const k of d.events?.cards ?? []) {
          b().eventos.push({ jogo: j.id, equipa: d.idTeam, tipo: k.type === 1 ? 'RED_CARD' : 'YELLOW_CARD', minuto: k.minute, jogador: k.playerId, relacionado: null });
        }
        for (const s of d.events?.substitutions ?? []) {
          b().eventos.push({ jogo: j.id, equipa: d.idTeam, tipo: 'SUBSTITUTION', minuto: s.minute, jogador: s.playerOutId, relacionado: s.playerInId });
        }
        return c.ok({
          submitted: true,
          matchFinished: true,
          resultsCoincide: true,
          message: 'Resultado confirmado pelas duas equipas: o jogo terminou. (Na demonstração, o adversário confirma logo.)',
        });
      },
    ],

    // Calendário
    [
      'GET',
      /^Calendar\/([^/]+)\/history$/,
      (_, [id]) =>
        c.ok(
          b()
            .marcas.map((m) => ({ m, j: b().jogos.find((x) => x.id === m.jogo) }))
            .filter(({ j }) => j && (j.casa === id || j.fora === id))
            .map(({ m, j }) => ({
              idMatch: m.jogo,
              date: m.data,
              kind: m.tipo,
              reason: m.motivo,
              opponentName: c.equipa(j!.casa === id ? j!.fora : j!.casa).nome,
              newDate: m.novaData,
            }))
        ),
    ],
    [
      'PUT',
      /^Calendar\/([^/]+)\/([^/]+)\/cancel-reschedule$/,
      (req, [id, idJogo]) => {
        c.exigirAdmin(id);
        const j = jogoPorId(idJogo);
        if (!j.epoca) {
          c.erro(400, 'Só os jogos da liga se cancelam com nova data; os amigáveis cancelam-se normalmente.');
        }
        const d = c.jsonCorpo<{ reason: string; newDate: string }>(req);
        const nova = new Date(d.newDate).toISOString();
        b().marcas.push({ jogo: j.id, data: j.data, tipo: 'CANCELLED', motivo: d.reason, novaData: nova });
        b().adiamentos = b().adiamentos.filter((a) => a.jogo !== j.id);
        j.data = nova;
        j.estado = 0;
        return c.ok({ idMatch: j.id, gameDate: nova, nameTeam: c.equipa(j.casa).nome, nameOpponent: c.equipa(j.fora).nome, namePitch: c.equipa(j.casa).campo.name });
      },
    ],
  ];
}

/** Títulos agrupados por troféu ("x3"). */
export function agruparTitulos(e: DemoEquipa) {
  const grupos = new Map<string, { trophyName: string; leagueName: string; count: number; seasons: string[] }>();
  for (const t of e.titulos) {
    const g = grupos.get(t.trofeu) ?? { trophyName: t.trofeu, leagueName: t.liga, count: 0, seasons: [] };
    g.count++;
    g.seasons.push(t.epoca);
    grupos.set(t.trofeu, g);
  }
  return [...grupos.values()].sort((x, y) => y.count - x.count);
}
