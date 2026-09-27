using Application.DTOs;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <inheritdoc cref="IContaRepository"/>
    public class ContaRepository : IContaRepository
    {
        private readonly AmateurFootballContext db;

        public ContaRepository(AmateurFootballContext db) => this.db = db;

        public Task<Player?> GetJogadorComEquipaAsync(string uid) =>
            db.Player.Include(p => p.Team).FirstOrDefaultAsync(p => p.Id == uid);

        public async Task PreencherExportacaoAsync(string uid, ExportacaoDadosDto exportacao)
        {
            exportacao.PedidosDeAdesao = await db.MembershipRequests.AsNoTracking()
                .Where(m => m.IdPlayer == uid)
                .OrderBy(m => m.InviteDate)
                .Select(m => new PedidoAdesaoExportadoDto { Equipa = m.Team.Name, Data = m.InviteDate, EnviadoPeloJogador = m.IsPlayerSender })
                .ToListAsync();

            var registos = await db.TransferRecord.AsNoTracking()
                .Where(r => r.PlayerId == uid)
                .OrderBy(r => r.Date)
                .ToListAsync();
            exportacao.Transferencias = registos
                .Select(r => new TransferenciaExportadaDto { Tipo = r.Kind.ToString(), DeEquipa = r.FromTeamName, ParaEquipa = r.ToTeamName, Data = r.Date })
                .ToList();

            var propostas = await db.TransferOffer.AsNoTracking()
                .Where(o => o.PlayerId == uid)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
            exportacao.PropostasDeTransferencia = propostas
                .Select(o => new PropostaExportadaDto { Id = o.Id, Estado = o.Status.ToString(), Mensagem = o.Message, CriadaEm = o.CreatedAt })
                .ToList();

            var eventos = await db.MatchEvent.AsNoTracking()
                .Where(e => e.PlayerId == uid || e.RelatedPlayerId == uid)
                .OrderBy(e => e.IdMatch).ThenBy(e => e.Minute)
                .ToListAsync();
            exportacao.EventosDeJogo = eventos
                .Select(e => new EventoJogoExportadoDto { Jogo = e.IdMatch, Tipo = e.Type.ToString(), Minuto = e.Minute })
                .ToList();

            exportacao.JogosNoOnze = await db.LineupSlot.CountAsync(s => s.PlayerId == uid);
        }

        public async Task RemoverPendentesDoJogadorAsync(string uid)
        {
            db.MembershipRequests.RemoveRange(await db.MembershipRequests.Where(m => m.IdPlayer == uid).ToListAsync());
            db.TransferListing.RemoveRange(await db.TransferListing.Where(l => l.PlayerId == uid).ToListAsync());
            db.TransferOffer.RemoveRange(await db.TransferOffer
                .Where(o => o.PlayerId == uid && (o.Status == TransferOfferStatus.PENDING_CLUB || o.Status == TransferOfferStatus.PENDING_PLAYER))
                .ToListAsync());
        }

        public Task<List<string>> GetIconesEmUsoAsync() =>
            db.Team.AsNoTracking()
                .Where(t => t.Icon != null && t.Icon.StartsWith("https://"))
                .Select(t => t.Icon!)
                .ToListAsync();

        public Task<List<string>> GetJogadoresSemEquipaCriadosAntesAsync(DateTime antesDe, int maximo) =>
            db.Player.AsNoTracking()
                .Where(p => p.IdTeam == null && p.EliminadoEm == null && p.CreationDate < antesDe)
                .OrderBy(p => p.CreationDate)
                .Select(p => p.Id)
                .Take(maximo)
                .ToListAsync();

        public async Task<int> ApagarPedidosAdesaoAntigosAsync(DateTime antesDe)
        {
            var antigos = await db.MembershipRequests.Where(m => m.InviteDate < antesDe).ToListAsync();
            db.MembershipRequests.RemoveRange(antigos);
            await db.SaveChangesAsync();
            return antigos.Count;
        }

        public async Task<int> ApagarConvitesExpiradosAsync(DateTime antesDe)
        {
            var expirados = await db.MatchInvite.Where(i => i.GameDate < antesDe).ToListAsync();
            db.MatchInvite.RemoveRange(expirados);
            await db.SaveChangesAsync();
            return expirados.Count;
        }

        public void RemoverJogador(Player jogador) => db.Player.Remove(jogador);
    }
}
