import { HttpErrorResponse, HttpInterceptorFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable, delay, of, switchMap, throwError, timer } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  DEMO_EQUIPA_ID,
  DEMO_JOGADOR_ID,
  DemoBase,
  DemoEquipa,
  DemoJogador,
  DemoJogo,
  criarBaseDemo,
} from './dados-demo';
import { agruparTitulos, rotasCompeticao } from './demo-competicao';

/**
 * Modo demonstração: responde aos pedidos à API com dados em memória, para a aplicação poder
 * ser experimentada (e fotografada) sem backend, base de dados nem Firebase.
 *
 * Só é registado quando `environment.demo` é `true` (configuração `demo`). Os dados ficam no
 * `sessionStorage`, por isso sobrevivem a um refresh mas não passam para outro separador.
 */

// A versão muda quando os dados mudam de forma (os dados antigos do sessionStorage deixam de servir).
const CHAVE = 'futebol-amador-demo-v2';
const LATENCIA_MS = 250;

type Resposta = { corpo?: unknown; estado?: number };
type Handler = (req: HttpRequest<unknown>, params: string[]) => Resposta;

let base: DemoBase = carregar();

function carregar(): DemoBase {
  try {
    const guardado = sessionStorage.getItem(CHAVE);
    if (guardado) {
      return JSON.parse(guardado) as DemoBase;
    }
  } catch {
    // sessionStorage indisponível: usa os dados iniciais.
  }
  return criarBaseDemo();
}

function guardar(): void {
  try {
    sessionStorage.setItem(CHAVE, JSON.stringify(base));
  } catch {
    // Sem armazenamento, as alterações só duram até ao refresh.
  }
}

/** Repõe os dados iniciais (usado nos testes). */
export function reporDadosDemo(): void {
  base = criarBaseDemo();
  guardar();
}

// ---------- Utilitários ----------

function ok(corpo?: unknown): Resposta {
  return { corpo: corpo ?? null, estado: 200 };
}

function erro(estado: number, detail: string): never {
  throw new HttpErrorResponse({ status: estado, error: { title: detail, detail } });
}

function equipa(id: string): DemoEquipa {
  return base.equipas.find((e) => e.id === id) ?? erro(404, 'Equipa não encontrada.');
}

function jogador(id: string): DemoJogador {
  return base.jogadores.find((p) => p.id === id) ?? erro(404, 'Jogador não encontrado.');
}

function eu(): DemoJogador {
  return jogador(DEMO_JOGADOR_ID);
}

function idade(nascimento: string): number {
  const n = new Date(nascimento);
  const hoje = new Date();
  let anos = hoje.getFullYear() - n.getFullYear();
  if (hoje < new Date(hoje.getFullYear(), n.getMonth(), n.getDate())) {
    anos--;
  }
  return anos;
}

function membros(idEquipa: string): DemoJogador[] {
  return base.jogadores.filter((p) => p.equipa === idEquipa);
}

function numeroJogadores(e: DemoEquipa): number {
  const conhecidos = membros(e.id).length;
  return e.id === DEMO_EQUIPA_ID || conhecidos > e.jogadores ? conhecidos : e.jogadores;
}

function exigirAdmin(idEquipa: string): void {
  const me = eu();
  if (me.equipa !== idEquipa || !me.admin) {
    erro(403, 'Só os administradores da equipa podem fazer isto.');
  }
}

function novoId(prefixo: string): string {
  return `${prefixo}-${Math.random().toString(36).slice(2, 10)}`;
}

function jsonCorpo<T>(req: HttpRequest<unknown>): T {
  return (typeof req.body === 'string' ? JSON.parse(req.body) : req.body) as T;
}

function param(req: HttpRequest<unknown>, nome: string): string | null {
  const chave = req.params.keys().find((k) => k.toLowerCase() === nome.toLowerCase());
  return chave ? req.params.get(chave) : null;
}

/** Token com o formato de um JWT (só o `exp` é lido pela aplicação). */
function tokenDemo(): string {
  const b64 = (o: object) => btoa(JSON.stringify(o)).replace(/=+$/, '').replace(/\+/g, '-').replace(/\//g, '_');
  const exp = Math.floor(Date.now() / 1000) + 7 * 24 * 3600;
  return `${b64({ alg: 'none', typ: 'JWT' })}.${b64({ sub: DEMO_JOGADOR_ID, exp })}.demo`;
}

// ---------- Conversão para os DTOs da API ----------

function detalhesJogador(p: DemoJogador) {
  const e = p.equipa ? equipa(p.equipa) : null;
  return {
    playerId: p.id,
    name: p.nome,
    email: p.email,
    phoneNumber: p.telefone,
    dateOfBirth: p.nascimento,
    age: idade(p.nascimento),
    address: p.morada,
    position: p.posicao,
    height: p.altura,
    team: e ? { idTeam: e.id, name: e.nome, imageUrl: e.emblema } : null,
    isAdmin: p.equipa ? p.admin : null,
    isCreator: !!e && e.criador === p.id,
    status: p.situacao,
    nationality: p.nacionalidade,
    isListed: base.listados.includes(p.id),
  };
}

function infoEquipa(e: DemoEquipa) {
  return {
    id: e.id,
    name: e.nome,
    description: e.descricao,
    icon: e.emblema,
    address: e.campo.address,
    rank: { id: e.divisao, name: e.divisao },
    currentPoints: e.pontos,
    averageAge: e.idadeMedia,
    playerCount: numeroJogadores(e),
  };
}

function resumoEquipa(id: string) {
  const e = equipa(id);
  return { idTeam: e.id, name: e.nome, imageUrl: e.emblema };
}

/** Vista de um jogo do ponto de vista de `idEquipa`. */
function vistaJogo(j: DemoJogo, idEquipa: string) {
  const emCasa = j.casa === idEquipa;
  const minha = emCasa ? j.casa : j.fora;
  const outra = emCasa ? j.fora : j.casa;
  const meus = emCasa ? j.golosCasa : j.golosFora;
  const deles = emCasa ? j.golosFora : j.golosCasa;
  const resultado = j.estado !== 2 ? 3 : meus > deles ? 0 : meus < deles ? 1 : 2;
  return { emCasa, minha, outra, meus, deles, resultado };
}

function jogoCalendario(j: DemoJogo, idEquipa: string) {
  const v = vistaJogo(j, idEquipa);
  const campo = equipa(j.casa).campo;
  return {
    idMatch: j.id,
    matchStatus: j.estado,
    matchResult: v.resultado,
    gameDate: j.data,
    team: { idTeam: v.minha, name: equipa(v.minha).nome, numGoals: v.meus },
    opponent: { idTeam: v.outra, name: equipa(v.outra).nome, numGoals: v.deles },
    pitchGame: { name: campo.name, address: campo.address },
    isHome: v.emCasa,
    isCompetitive: j.competitivo,
    homeTeam: { idTeam: j.casa, name: equipa(j.casa).nome, numGoals: j.golosCasa },
    awayTeam: { idTeam: j.fora, name: equipa(j.fora).nome, numGoals: j.golosFora },
    leagueName: j.epoca ? nomeLiga(j.epoca) : null,
    round: j.jornada ?? null,
    reason: j.estado === 3 ? base.adiamentos.find((a) => a.jogo === j.id)?.motivo ?? j.motivo ?? null : j.motivo ?? null,
    postponedFrom: j.adiadoDe ?? null,
  };
}

function nomeLiga(idEpoca: string): string | null {
  const epoca = base.epocas.find((e) => e.id === idEpoca);
  return epoca ? base.ligas.find((l) => l.id === epoca.liga)?.nome ?? null : null;
}

function filtrarCalendario(req: HttpRequest<unknown>, idEquipa: string, jogos: DemoJogo[]) {
  const adversario = param(req, 'NameOpponent')?.toLowerCase();
  const realizado = param(req, 'IsRealized');
  const casa = param(req, 'IsHome');
  const competitivo = param(req, 'IsRanqued');
  const min = param(req, 'MinDate');
  const max = param(req, 'MaxDate');
  return jogos.filter((j) => {
    const v = vistaJogo(j, idEquipa);
    return (
      (!adversario || equipa(v.outra).nome.toLowerCase().includes(adversario)) &&
      (realizado === null || (j.estado === 2) === (realizado === 'true')) &&
      (casa === null || v.emCasa === (casa === 'true')) &&
      (competitivo === null || j.competitivo === (competitivo === 'true')) &&
      (!min || j.data.slice(0, 10) >= min) &&
      (!max || j.data.slice(0, 10) <= max)
    );
  });
}

function filtrarEquipas(req: HttpRequest<unknown>, equipas: DemoEquipa[]) {
  const nome = param(req, 'NameTeam')?.toLowerCase();
  const divisao = param(req, 'NameRank')?.toLowerCase();
  const cidade = param(req, 'City')?.toLowerCase();
  const num = (n: string) => (param(req, n) !== null ? Number(param(req, n)) : null);
  const [minP, maxP, minI, maxI, minJ, maxJ] = [
    'MinNumberPoints',
    'MaxNumberPoints',
    'MinAge',
    'MaxAge',
    'MinNumberPlayers',
    'MaxNumberPlayers',
  ].map(num);
  return equipas.filter(
    (e) =>
      (!nome || e.nome.toLowerCase().includes(nome)) &&
      (!divisao || e.divisao.toLowerCase().includes(divisao)) &&
      (!cidade || e.campo.address.toLowerCase().includes(cidade)) &&
      (minP === null || e.pontos >= minP) &&
      (maxP === null || e.pontos <= maxP) &&
      (minI === null || e.idadeMedia >= minI) &&
      (maxI === null || e.idadeMedia <= maxI) &&
      (minJ === null || numeroJogadores(e) >= minJ) &&
      (maxJ === null || numeroJogadores(e) <= maxJ)
  );
}

function pedidoAdesao(p: (typeof base.pedidos)[number]) {
  const e = equipa(p.equipa);
  return {
    requestId: p.id,
    player: { id: p.jogador, name: jogador(p.jogador).nome },
    team: { idTeam: e.id, name: e.nome },
    requestDate: p.data,
    isPlayerSender: p.doJogador,
  };
}

function entrarNaEquipa(idJogador: string, idEquipa: string): void {
  const p = jogador(idJogador);
  p.equipa = idEquipa;
  p.admin = false;
  p.desde = new Date().toISOString();
  base.movimentos.push({ jogador: p.id, de: null, para: idEquipa, tipo: 'ADESAO', data: p.desde });
  base.pedidos = base.pedidos.filter((x) => x.jogador !== idJogador);
}

// ---------- Rotas ----------

const rotas: Array<[string, RegExp, Handler]> = [
  // Ligas, transferências, onzes, relatório e perfil (primeiro: algumas rotas são mais específicas
  // do que as antigas, como Calendar/{id}/history).
  ...rotasCompeticao({
    base: () => base,
    ok,
    erro,
    equipa,
    jogador,
    membros,
    exigirAdmin,
    novoId,
    jsonCorpo,
    param,
    idade,
  }),

  // Conta
  [
    'POST',
    /^User\/login$/,
    () => {
      const me = eu();
      return ok({
        name: me.nome,
        idTeam: me.equipa,
        isAdmin: me.admin,
        firebaseLoginResponseDto: { idToken: tokenDemo(), localId: me.id, expiresIn: '3600' },
      });
    },
  ],
  ['GET', /^User\/logout$/, () => ok()],
  ['PUT', /^User\/password$/, () => ok()],
  ['POST', /^Player\/create-profile$/, () => ok()],

  // Jogadores
  ['GET', /^Player\/details\/([^/]+)$/, (_, [id]) => ok(detalhesJogador(jogador(id)))],
  ['GET', /^Player\/get-my-profile$/, () => ok(detalhesJogador(eu()))],
  [
    'GET',
    /^Player\/listPlayers$/,
    () =>
      ok(
        base.jogadores
          .filter((p) => !p.equipa)
          .map((p) => ({
            id: p.id,
            name: p.nome,
            position: p.posicao,
            heigth: p.altura,
            haveTeam: false,
            age: idade(p.nascimento),
            teamName: null,
            address: p.morada,
          }))
      ),
  ],
  [
    'PUT',
    /^Player\/update\/([^/]+)$/,
    (req, [id]) => {
      const p = jogador(id);
      const d = jsonCorpo<Record<string, unknown>>(req);
      p.nome = String(d['Name'] ?? p.nome);
      p.email = String(d['Email'] ?? p.email);
      p.telefone = String(d['Phone'] ?? p.telefone);
      p.morada = String(d['Address'] ?? p.morada);
      p.nascimento = String(d['DateOfBirth'] ?? p.nascimento);
      p.posicao = Number(d['Position'] ?? p.posicao);
      p.altura = Number(d['Height'] ?? p.altura);
      // Os campos do perfil são opcionais: nulo mantém o valor.
      if (d['Weight'] != null) p.peso = Number(d['Weight']);
      if (d['PreferredFoot'] != null) p.pe = Number(d['PreferredFoot']);
      if (d['Status'] != null) p.situacao = Number(d['Status']);
      if (d['Nationality']) p.nacionalidade = String(d['Nationality']);
      if (d['CountryOfBirth']) p.paisNascimento = String(d['CountryOfBirth']);
      return ok();
    },
  ],
  [
    'PUT',
    /^Player\/([^/]+)\/leave-team$/,
    (_, [id]) => {
      const p = jogador(id);
      if (p.equipa) {
        base.movimentos.push({ jogador: p.id, de: p.equipa, para: null, tipo: 'SAIDA', data: new Date().toISOString() });
        const e = equipa(p.equipa);
        if (e.criador === p.id) {
          e.criador = membros(e.id).find((m) => m.admin && m.id !== p.id)?.id ?? null;
        }
      }
      base.listados = base.listados.filter((x) => x !== p.id);
      p.equipa = null;
      p.admin = false;
      return ok();
    },
  ],
  ['DELETE', /^Player\/([^/]+)$/, () => erro(400, 'No modo demonstração não é possível apagar a conta.')],
  [
    'GET',
    /^Player\/([^/]+)\/listTeamsToMemberShipRequest$/,
    (req) => ok(filtrarEquipas(req, base.equipas).map(infoEquipa)),
  ],
  [
    'GET',
    /^Player\/([^/]+)\/membership-requests$/,
    (_, [id]) => ok(base.pedidos.filter((p) => p.jogador === id && !p.doJogador).map(pedidoAdesao)),
  ],
  [
    'POST',
    /^Player\/([^/]+)\/membership-requests\/accept$/,
    (req, [id]) => {
      const { requestId } = jsonCorpo<{ requestId: string }>(req);
      const pedido = base.pedidos.find((p) => p.id === requestId) ?? erro(404, 'Convite não encontrado.');
      entrarNaEquipa(id, pedido.equipa);
      return ok();
    },
  ],
  [
    'DELETE',
    /^Player\/([^/]+)\/membership-requests\/reject\/([^/]+)$/,
    (_, [, requestId]) => {
      base.pedidos = base.pedidos.filter((p) => p.id !== requestId);
      return ok();
    },
  ],
  [
    'POST',
    /^Player\/([^/]+)\/membership-requests\/send$/,
    (req, [id]) => {
      const { teamId } = jsonCorpo<{ teamId: string }>(req);
      base.pedidos.push({ id: novoId('ped'), jogador: id, equipa: teamId, data: new Date().toISOString(), doJogador: true });
      return ok();
    },
  ],

  // Equipas
  [
    'POST',
    /^Team$/,
    (req) => {
      const d = jsonCorpo<{ name: string; description: string; icon: string | null; homePitch: { name: string; address: string } }>(req);
      const id = novoId('eq');
      base.equipas.push({
        id,
        nome: d.name,
        descricao: d.description,
        emblema: d.icon ?? '',
        divisao: 'Divisão 4',
        pontos: 0,
        campo: d.homePitch,
        fundada: new Date().toISOString().slice(0, 10),
        idadeMedia: idade(eu().nascimento),
        jogadores: 1,
        liga: [...base.ligas].sort((a, b) => b.nivel - a.nivel)[0]?.id ?? null,
        criador: DEMO_JOGADOR_ID,
        titulos: [],
      });
      const me = eu();
      me.equipa = id;
      me.admin = true;
      return ok({ ...d, id });
    },
  ],
  [
    'GET',
    /^Team\/homeTeam\/([^/]+)$/,
    (_, [id]) => {
      const agora = new Date().toISOString();
      const meus = base.jogos.filter((j) => j.casa === id || j.fora === id);
      return ok({
        team: resumoEquipa(id),
        nextsMatchs: meus
          .filter((j) => j.estado === 0 && j.data >= agora)
          .sort((a, b) => a.data.localeCompare(b.data))
          .slice(0, 3)
          .map((j) => {
            const v = vistaJogo(j, id);
            return {
              idMatch: j.id,
              gameDate: j.data,
              isCompetitive: j.competitivo,
              isHome: v.emCasa,
              team: resumoEquipa(v.minha),
              opponent: resumoEquipa(v.outra),
            };
          }),
        historicPreviousGames: meus
          .filter((j) => j.estado === 2)
          .sort((a, b) => b.data.localeCompare(a.data))
          .slice(0, 5)
          .map((j) => {
            const v = vistaJogo(j, id);
            return { opponent: resumoEquipa(v.outra), result: `${v.meus} - ${v.deles}`, matchResult: v.resultado };
          }),
      });
    },
  ],
  [
    'GET',
    /^Team\/([^/]+)\/search$/,
    (req, [id]) => ok(filtrarEquipas(req, base.equipas.filter((e) => e.id !== id)).map(infoEquipa)),
  ],
  [
    'GET',
    /^Team\/([^/]+)\/members$/,
    (_, [id]) =>
      ok(
        membros(id).map((p) => ({
          ...detalhesJogador(p),
          isAdmin: p.admin,
          team: { idTeam: id, name: equipa(id).nome },
        }))
      ),
  ],
  [
    'PUT',
    /^Team\/([^/]+)\/members\/(promote|demote)\/([^/]+)$/,
    (_, [id, acao, idJogador]) => {
      exigirAdmin(id);
      if (acao === 'demote' && equipa(id).criador !== DEMO_JOGADOR_ID) {
        erro(403, 'Só o administrador principal (quem criou a equipa) pode despromover administradores.');
      }
      if (acao === 'demote' && equipa(id).criador === idJogador) {
        erro(400, 'O administrador principal não pode ser despromovido.');
      }
      const admins = membros(id).filter((p) => p.admin).length;
      if (acao === 'promote' && admins >= 4) {
        erro(400, 'Uma equipa pode ter no máximo 4 administradores.');
      }
      if (acao === 'demote' && admins <= 1) {
        erro(400, 'A equipa tem de ter pelo menos um administrador.');
      }
      jogador(idJogador).admin = acao === 'promote';
      return ok();
    },
  ],
  [
    'DELETE',
    /^Team\/([^/]+)\/members\/([^/]+)$/,
    (_, [id, idJogador]) => {
      exigirAdmin(id);
      const p = jogador(idJogador);
      if (equipa(id).criador === p.id) {
        erro(400, 'O administrador principal (quem criou a equipa) não pode ser removido.');
      }
      if (p.admin && equipa(id).criador !== DEMO_JOGADOR_ID) {
        erro(403, 'Só o administrador principal pode remover administradores.');
      }
      base.movimentos.push({ jogador: p.id, de: id, para: null, tipo: 'SAIDA', data: new Date().toISOString() });
      base.listados = base.listados.filter((x) => x !== p.id);
      p.equipa = null;
      p.admin = false;
      return ok();
    },
  ],
  [
    'GET',
    /^Team\/([^/]+)\/membership-request$/,
    (_, [id]) => {
      exigirAdmin(id);
      return ok(base.pedidos.filter((p) => p.equipa === id && p.doJogador).map(pedidoAdesao));
    },
  ],
  [
    'POST',
    /^Team\/([^/]+)\/membership-request\/accept$/,
    (req, [id]) => {
      exigirAdmin(id);
      const { requestId } = jsonCorpo<{ requestId: string }>(req);
      const pedido = base.pedidos.find((p) => p.id === requestId && p.equipa === id) ?? erro(404, 'Pedido não encontrado.');
      entrarNaEquipa(pedido.jogador, id);
      return ok();
    },
  ],
  [
    'DELETE',
    /^Team\/([^/]+)\/membership-request\/([^/]+)\/reject$/,
    (_, [id, requestId]) => {
      exigirAdmin(id);
      base.pedidos = base.pedidos.filter((p) => p.id !== requestId);
      return ok();
    },
  ],
  [
    'POST',
    /^Team\/([^/]+)\/membership-requests\/send$/,
    (req, [id]) => {
      exigirAdmin(id);
      const { playerId } = jsonCorpo<{ playerId: string }>(req);
      if (base.pedidos.some((p) => p.jogador === playerId && p.equipa === id)) {
        erro(400, 'Já existe um convite ou pedido pendente para este jogador.');
      }
      base.pedidos.push({ id: novoId('ped'), jogador: playerId, equipa: id, data: new Date().toISOString(), doJogador: false });
      return ok();
    },
  ],
  [
    'GET',
    /^Team\/([^/]+)\/PostPoneMatch$/,
    (_, [id]) => {
      exigirAdmin(id);
      return ok(
        base.adiamentos
          .filter((a) => a.pedidoPor !== id)
          .map((a) => base.jogos.find((j) => j.id === a.jogo)!)
          .filter((j) => j && (j.casa === id || j.fora === id))
          .map((j) => {
            const a = base.adiamentos.find((x) => x.jogo === j.id)!;
            const v = vistaJogo(j, id);
            return {
              idMatch: j.id,
              gameDate: j.data,
              postPoneDate: a.novaData,
              reason: a.motivo ?? null,
              team: { idTeam: v.minha, name: equipa(v.minha).nome },
              opponent: { idTeam: v.outra, name: equipa(v.outra).nome },
            };
          })
      );
    },
  ],
  [
    'POST',
    /^Team\/([^/]+)\/PostPoneMatch\/AcceptPostponeMatch$/,
    (req, [id]) => {
      exigirAdmin(id);
      const { idMatch } = jsonCorpo<{ idMatch: string }>(req);
      const a = base.adiamentos.find((x) => x.jogo === idMatch) ?? erro(404, 'Pedido não encontrado.');
      const j = base.jogos.find((x) => x.id === idMatch)!;
      base.marcas.push({ jogo: j.id, data: j.data, tipo: 'POSTPONED', motivo: a.motivo ?? null, novaData: a.novaData });
      j.adiadoDe = j.data;
      j.motivo = a.motivo;
      j.data = a.novaData;
      j.estado = 0;
      base.adiamentos = base.adiamentos.filter((x) => x !== a);
      return ok();
    },
  ],
  [
    'DELETE',
    /^Team\/([^/]+)\/PostPoneMatch\/RejectPostponeMatch$/,
    (req, [id]) => {
      exigirAdmin(id);
      const { idMatch } = jsonCorpo<{ idMatch: string }>(req);
      base.adiamentos = base.adiamentos.filter((x) => x.jogo !== idMatch);
      return ok();
    },
  ],
  [
    'GET',
    /^Team\/([^/]+)$/,
    (_, [id]) => {
      const e = equipa(id);
      return ok({
        id: e.id,
        name: e.nome,
        description: e.descricao,
        icon: e.emblema,
        foundationDate: e.fundada,
        totalPoints: e.pontos,
        rankName: e.divisao,
        pitchDto: e.campo,
        players: membros(id).map(detalhesJogador),
        creatorId: e.criador,
        leagueId: e.liga,
        leagueName: base.ligas.find((l) => l.id === e.liga)?.nome ?? null,
        titles: agruparTitulos(e),
      });
    },
  ],
  [
    'PUT',
    /^Team\/([^/]+)$/,
    (req, [id]) => {
      exigirAdmin(id);
      const e = equipa(id);
      const d = jsonCorpo<{ name: string; description: string; icon: string | null; homePitch: { name: string; address: string } }>(req);
      e.nome = d.name;
      e.descricao = d.description;
      e.emblema = d.icon ?? e.emblema;
      e.campo = d.homePitch;
      return ok();
    },
  ],
  [
    'DELETE',
    /^Team\/([^/]+)$/,
    (_, [id]) => {
      exigirAdmin(id);
      membros(id).forEach((p) => {
        p.equipa = null;
        p.admin = false;
      });
      base.equipas = base.equipas.filter((e) => e.id !== id);
      return ok();
    },
  ],

  // Calendário
  [
    'PUT',
    /^Calendar\/([^/]+)\/PostponeMatch$/,
    (req, [id]) => {
      exigirAdmin(id);
      const d = jsonCorpo<{ idMatch: string; postPoneDate: string; reason?: string }>(req);
      base.adiamentos = base.adiamentos.filter((a) => a.jogo !== d.idMatch);
      base.adiamentos.push({ jogo: d.idMatch, pedidoPor: id, novaData: new Date(d.postPoneDate).toISOString(), motivo: d.reason });
      const j = base.jogos.find((x) => x.id === d.idMatch);
      if (j) {
        j.estado = 3;
      }
      return ok();
    },
  ],
  [
    'DELETE',
    /^Calendar\/([^/]+)\/CancelMatch\/([^/]+)$/,
    (req, [id, idMatch]) => {
      exigirAdmin(id);
      const j = base.jogos.find((x) => x.id === idMatch) ?? erro(404, 'Jogo não encontrado.');
      if (j.epoca) {
        erro(400, 'Os jogos da liga não podem ficar cancelados: cancela e remarca com uma nova data.');
      }
      j.estado = 4;
      j.motivo = jsonCorpo<string>(req);
      return ok();
    },
  ],
  [
    'GET',
    /^Calendar\/([^/]+)\/([^/]+)$/,
    (_, [id, idMatch]) => {
      const j = base.jogos.find((x) => x.id === idMatch) ?? erro(404, 'Jogo não encontrado.');
      const v = vistaJogo(j, id);
      return ok({
        idMatch: j.id,
        gameDate: j.data,
        isCompetitive: j.competitivo,
        isHome: v.emCasa,
        team: { idTeam: v.minha, name: equipa(v.minha).nome },
        opponent: { idTeam: v.outra, name: equipa(v.outra).nome },
      });
    },
  ],
  [
    'GET',
    /^Calendar\/([^/]+)$/,
    (req, [id]) => {
      const meus = base.jogos.filter((j) => j.casa === id || j.fora === id);
      return ok(
        filtrarCalendario(req, id, meus)
          .sort((a, b) => b.data.localeCompare(a.data))
          .map((j) => jogoCalendario(j, id))
      );
    },
  ],

  // Convites de jogo
  [
    'GET',
    /^MatchInvite\/([^/]+)\/([^/]+)$/,
    (_, [, idConvite]) => {
      const c = base.convites.find((x) => x.id === idConvite) ?? erro(404, 'Convite não encontrado.');
      return ok(conviteDto(c));
    },
  ],
  [
    'GET',
    /^MatchInvite\/([^/]+)$/,
    (_, [id]) => ok(base.convites.filter((c) => c.de === id || c.para === id).map(conviteDto)),
  ],
  [
    'POST',
    /^MatchInvite\/([^/]+)\/match-invites$/,
    (req, [id]) => {
      exigirAdmin(id);
      const d = jsonCorpo<{ idReceiver: string; gameDate: string; homePitch: boolean }>(req);
      base.convites.push({
        id: novoId('conv'),
        de: id,
        para: d.idReceiver,
        data: new Date(d.gameDate).toISOString(),
        emCasaDoRemetente: d.homePitch,
      });
      return ok();
    },
  ],
  [
    'POST',
    /^MatchInvite\/([^/]+)\/AcceptMatchInvite$/,
    (req, [id]) => {
      exigirAdmin(id);
      const idConvite = jsonCorpo<string>(req);
      const c = base.convites.find((x) => x.id === idConvite) ?? erro(404, 'Convite não encontrado.');
      base.jogos.push({
        id: novoId('jogo'),
        casa: c.emCasaDoRemetente ? c.de : c.para,
        fora: c.emCasaDoRemetente ? c.para : c.de,
        data: c.data,
        estado: 0,
        golosCasa: 0,
        golosFora: 0,
        competitivo: false,
      });
      base.convites = base.convites.filter((x) => x !== c);
      return ok();
    },
  ],
  [
    'DELETE',
    /^MatchInvite\/([^/]+)\/RefuseMatchInvite$/,
    (req, [id]) => {
      exigirAdmin(id);
      const idConvite = jsonCorpo<string>(req);
      base.convites = base.convites.filter((x) => x.id !== idConvite);
      return ok();
    },
  ],
  [
    'PUT',
    /^MatchInvite\/([^/]+)\/Negociate$/,
    (req, [id]) => {
      exigirAdmin(id);
      const d = jsonCorpo<{ idReceiver: string; gameDate: string; homePitch: boolean }>(req);
      // A contraproposta substitui o convite original entre as duas equipas.
      base.convites = base.convites.filter((c) => !(c.de === d.idReceiver && c.para === id));
      base.convites.push({
        id: novoId('conv'),
        de: id,
        para: d.idReceiver,
        data: new Date(d.gameDate).toISOString(),
        emCasaDoRemetente: d.homePitch,
      });
      return ok();
    },
  ],
];

function conviteDto(c: (typeof base.convites)[number]) {
  const de = equipa(c.de);
  const para = equipa(c.para);
  return {
    id: c.id,
    sender: { idTeam: de.id, name: de.nome },
    receiver: { idTeam: para.id, name: para.nome },
    gameDate: c.data,
    namePitch: (c.emCasaDoRemetente ? de : para).campo.name,
  };
}

/** Interceptor do modo demonstração (ver o comentário no topo do ficheiro). */
export const demoInterceptor: HttpInterceptorFn = (req, next) => {
  const prefixo = `${environment.apiBaseUrl}/`;
  if (!req.url.startsWith(prefixo)) {
    return next(req);
  }
  const caminho = req.url.slice(prefixo.length).replace(/\/$/, '');

  for (const [metodo, padrao, handler] of rotas) {
    const m = req.method === metodo ? padrao.exec(caminho) : null;
    if (!m) {
      continue;
    }
    let resultado: Observable<HttpResponse<unknown>>;
    try {
      const { corpo, estado } = handler(req, m.slice(1).map(decodeURIComponent));
      guardar();
      resultado = of(new HttpResponse({ status: estado ?? 200, body: corpo ?? null, url: req.url }));
    } catch (e) {
      const falha = e instanceof HttpErrorResponse ? e : new HttpErrorResponse({ status: 500, url: req.url });
      return falhar(falha);
    }
    return resultado.pipe(delay(LATENCIA_MS));
  }

  return falhar(
    new HttpErrorResponse({
      status: 404,
      url: req.url,
      error: { detail: `Sem dados de demonstração para ${req.method} ${caminho}.` },
    })
  );
};

/** Erro com a mesma latência das respostas (o `delay` do RxJS não atrasa erros). */
function falhar(e: HttpErrorResponse): Observable<never> {
  return timer(LATENCIA_MS).pipe(switchMap(() => throwError(() => e)));
}
