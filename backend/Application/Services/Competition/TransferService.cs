using Application.DTOs.Competition;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services.Competition
{
    /// <summary>
    /// Mercado de transferências sem dinheiro (ver docs/novas-funcionalidades.md, D6): o clube do jogador
    /// lista-o ou aceita uma proposta, e o jogador tem sempre a última palavra.
    /// </summary>
    public class TransferService : ITransferService
    {
        private readonly ITransferRepository transfers;
        private readonly IPlayerRepository players;
        private readonly ITeamRepository teams;
        private readonly IPlayerAuthorizationService authorization;
        private readonly IUnityOfWork unitOfWork;
        private readonly INotificationService notifications;
        private readonly TimeProvider clock;
        private readonly ILogger<TransferService> logger;

        public TransferService(ITransferRepository transfers, IPlayerRepository players, ITeamRepository teams,
            IPlayerAuthorizationService authorization, IUnityOfWork unitOfWork, INotificationService notifications,
            TimeProvider clock, ILogger<TransferService> logger)
        {
            this.transfers = transfers;
            this.players = players;
            this.teams = teams;
            this.authorization = authorization;
            this.unitOfWork = unitOfWork;
            this.notifications = notifications;
            this.clock = clock;
            this.logger = logger;
        }

        private DateTime Now => clock.GetUtcNow().UtcDateTime;

        public async Task<List<MarketPlayerDto>> GetMarketAsync(string? userId, Guid teamId, MarketFilterDto filter)
        {
            await authorization.UserAuthorizationIsAdminTeamById(userId!, teamId);
            var listed = await transfers.GetListedPlayerIdsAsync();
            var found = await transfers.SearchMarketAsync(filter, teamId, listed);
            var today = DateOnly.FromDateTime(Now);

            return found.Select(p => new MarketPlayerDto
            {
                PlayerId = p.Id,
                Name = p.Name,
                Age = Age(p.DateOfBirth, today),
                Position = p.Position,
                Nationality = p.Nationality,
                ImageUrl = p.ImageUrl,
                TeamId = p.IdTeam,
                TeamName = p.Team?.Name,
                LeagueId = p.Team?.IdLeague,
                LeagueName = p.Team?.League?.Name,
                IsListed = listed.Contains(p.Id),
            }).ToList();
        }

        public async Task ListPlayerAsync(string? userId, Guid teamId, string playerId)
        {
            await authorization.UserAuthorizationIsAdminTeamById(userId!, teamId);
            var player = await players.GetPlayerByIdAsync(playerId) ?? throw new NotFoundException("O jogador não existe.");
            if (player.IdTeam != teamId)
            {
                throw new ForbiddenException("Só se podem listar jogadores da própria equipa.");
            }

            if (await transfers.GetListingAsync(playerId) != null)
            {
                throw new BusinessRuleException("O jogador já está no mercado.");
            }

            await transfers.AddListingAsync(new TransferListing { PlayerId = playerId, IdTeam = teamId, ListedAt = Now });
            await unitOfWork.SaveChangesAsync();
            await NotifyUser(playerId, "Colocado no mercado", "A tua equipa colocou-te no mercado de transferências.");
        }

        public async Task UnlistPlayerAsync(string? userId, Guid teamId, string playerId)
        {
            await authorization.UserAuthorizationIsAdminTeamById(userId!, teamId);
            var listing = await transfers.GetListingAsync(playerId);
            if (listing == null || listing.IdTeam != teamId)
            {
                throw new NotFoundException("O jogador não está no mercado por esta equipa.");
            }

            transfers.RemoveListing(listing);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<TransferOfferDto> CreateOfferAsync(string? userId, CreateTransferOfferDto dto)
        {
            await authorization.UserAuthorizationIsAdminTeamById(userId!, dto.TeamId);

            var player = await players.GetPlayerByIdAsync(dto.PlayerId) ?? throw new NotFoundException("O jogador não existe.");
            if (player.IdTeam == null)
            {
                throw new BusinessRuleException("O jogador não tem equipa: convida-o pelos pedidos de adesão.");
            }

            if (player.IdTeam == dto.TeamId)
            {
                throw new BusinessRuleException("O jogador já é da tua equipa.");
            }

            var buyer = await teams.GetTeamForMemberManagementAsync(dto.TeamId) ?? throw new NotFoundException("A equipa não existe.");
            if (buyer.Members.Count >= ModelConstants.TeamConst.MaxMembers)
            {
                throw new BusinessRuleException($"A equipa já tem {ModelConstants.TeamConst.MaxMembers} jogadores.");
            }

            if (await transfers.HasOpenOfferAsync(player.Id, dto.TeamId))
            {
                throw new BusinessRuleException("Já existe uma proposta por este jogador à espera de resposta.");
            }

            var listing = await transfers.GetListingAsync(player.Id);
            var offer = new TransferOffer
            {
                PlayerId = player.Id,
                IdFromTeam = player.IdTeam.Value,
                IdToTeam = dto.TeamId,
                Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message.Trim(),
                ViaListing = listing != null,
                Status = listing != null ? TransferOfferStatus.PENDING_PLAYER : TransferOfferStatus.PENDING_CLUB,
                CreatedAt = Now,
            };

            await transfers.AddOfferAsync(offer);
            await unitOfWork.SaveChangesAsync();

            var saved = await transfers.GetOfferAsync(offer.Id) ?? offer;
            if (offer.Status == TransferOfferStatus.PENDING_PLAYER)
            {
                await NotifyUser(player.Id, "Proposta de transferência", $"A equipa {buyer.Name} quer contar contigo.");
            }
            else
            {
                await NotifyTeam(offer.IdFromTeam, "Proposta de transferência", $"A equipa {buyer.Name} fez uma proposta por {player.Name}.");
            }

            return ToDto(saved);
        }

        public async Task<TeamTransferOffersDto> GetTeamOffersAsync(string? userId, Guid teamId)
        {
            await authorization.UserAuthorizationIsAdminTeamById(userId!, teamId);
            var offers = await transfers.GetOffersOfTeamAsync(teamId);
            return new TeamTransferOffersDto
            {
                Received = offers.Where(o => o.IdFromTeam == teamId).Select(ToDto).ToList(),
                Sent = offers.Where(o => o.IdToTeam == teamId).Select(ToDto).ToList(),
            };
        }

        public async Task<List<TransferOfferDto>> GetPlayerOffersAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException();
            }

            return (await transfers.GetOffersWaitingForPlayerAsync(userId)).Select(ToDto).ToList();
        }

        public async Task<TransferOfferDto> AcceptOfferAsync(string? userId, Guid offerId)
        {
            var offer = await GetOpenOffer(offerId);

            if (offer.Status == TransferOfferStatus.PENDING_CLUB)
            {
                if (!IsAdmin(offer.FromTeam, userId))
                {
                    throw new ForbiddenException("Só um administrador da equipa do jogador pode aceitar a proposta.");
                }

                offer.Status = TransferOfferStatus.PENDING_PLAYER;
                await unitOfWork.SaveChangesAsync();
                await NotifyUser(offer.PlayerId, "Proposta de transferência",
                    $"A tua equipa aceitou a proposta da {offer.ToTeam.Name}. Falta a tua resposta.");
                return ToDto(offer);
            }

            // PENDING_PLAYER: só o próprio jogador decide.
            if (userId != offer.PlayerId)
            {
                throw new ForbiddenException("Só o jogador pode aceitar a transferência.");
            }

            var player = offer.Player;
            if (player.IdTeam != offer.IdFromTeam)
            {
                offer.Status = TransferOfferStatus.CANCELLED;
                offer.DecidedAt = Now;
                await unitOfWork.SaveChangesAsync();
                throw new BusinessRuleException("Já não estás na equipa que a proposta indicava; a proposta foi anulada.");
            }

            if (offer.ToTeam.Members.Count >= ModelConstants.TeamConst.MaxMembers)
            {
                throw new BusinessRuleException($"A equipa {offer.ToTeam.Name} já tem {ModelConstants.TeamConst.MaxMembers} jogadores.");
            }

            var from = offer.FromTeam;
            if (from.Members.Count == 1)
            {
                throw new BusinessRuleException("És o único jogador da equipa: sai ou apaga a equipa antes de mudar.");
            }

            if (player.IsAdmin && from.Members.Count(m => m.IsAdmin) == 1)
            {
                throw new BusinessRuleException("És o único administrador da equipa: promove outro jogador antes de mudar.");
            }

            // O administrador principal que sai passa o estatuto ao administrador mais antigo (D9).
            if (from.CreatorId == player.Id)
            {
                from.CreatorId = TeamHierarchy.NextSupremeAdmin(from, player.Id)?.Id;
            }

            from.Members.Remove(player);
            offer.ToTeam.Members.Add(player);
            player.IdTeam = offer.IdToTeam;
            player.Team = offer.ToTeam;
            player.IsAdmin = false;
            player.IsAdminLastChangedAt = Now;
            player.JoinedTeamAt = Now;

            var listing = await transfers.GetListingAsync(player.Id);
            if (listing != null)
            {
                transfers.RemoveListing(listing);
            }

            offer.Status = TransferOfferStatus.ACCEPTED;
            offer.DecidedAt = Now;

            foreach (var other in await transfers.GetOpenOffersOfPlayerAsync(player.Id))
            {
                if (other.Id != offer.Id)
                {
                    other.Status = TransferOfferStatus.CANCELLED;
                    other.DecidedAt = Now;
                }
            }

            await transfers.AddRecordAsync(new TransferRecord
            {
                PlayerId = player.Id,
                IdFromTeam = from.Id,
                FromTeamName = from.Name,
                IdToTeam = offer.ToTeam.Id,
                ToTeamName = offer.ToTeam.Name,
                Kind = TransferKind.TRANSFER,
                Date = Now,
            });

            await unitOfWork.SaveChangesAsync();

            await NotifyTeam(from.Id, "Transferência concluída", $"{player.Name} mudou-se para a {offer.ToTeam.Name}.");
            await NotifyTeam(offer.ToTeam.Id, "Transferência concluída", $"{player.Name} é o novo reforço da equipa.");
            return ToDto(offer);
        }

        public async Task<TransferOfferDto> RejectOfferAsync(string? userId, Guid offerId)
        {
            var offer = await GetOpenOffer(offerId);

            if (userId == offer.PlayerId || IsAdmin(offer.FromTeam, userId))
            {
                offer.Status = TransferOfferStatus.REJECTED;
            }
            else if (IsAdmin(offer.ToTeam, userId))
            {
                offer.Status = TransferOfferStatus.CANCELLED;
            }
            else
            {
                throw new ForbiddenException("Não tens permissão para responder a esta proposta.");
            }

            offer.DecidedAt = Now;
            await unitOfWork.SaveChangesAsync();

            if (offer.Status == TransferOfferStatus.REJECTED)
            {
                await NotifyTeam(offer.IdToTeam, "Proposta recusada", $"A proposta por {offer.Player.Name} foi recusada.");
            }

            return ToDto(offer);
        }

        private async Task<TransferOffer> GetOpenOffer(Guid offerId)
        {
            var offer = await transfers.GetOfferAsync(offerId) ?? throw new NotFoundException("A proposta não existe.");
            if (offer.Status is not (TransferOfferStatus.PENDING_CLUB or TransferOfferStatus.PENDING_PLAYER))
            {
                throw new BusinessRuleException("A proposta já foi decidida.");
            }

            return offer;
        }

        private static bool IsAdmin(Team team, string? userId) =>
            userId != null && team.Members.Any(m => m.Id == userId && m.IsAdmin);

        private static int Age(DateOnly birth, DateOnly today)
        {
            var age = today.Year - birth.Year;
            if (birth > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        public static TransferOfferDto ToDto(TransferOffer o) => new()
        {
            Id = o.Id,
            PlayerId = o.PlayerId,
            PlayerName = o.Player?.Name ?? "",
            FromTeamId = o.IdFromTeam,
            FromTeamName = o.FromTeam?.Name ?? "",
            ToTeamId = o.IdToTeam,
            ToTeamName = o.ToTeam?.Name ?? "",
            Status = o.Status,
            Message = o.Message,
            ViaListing = o.ViaListing,
            CreatedAt = o.CreatedAt,
            DecidedAt = o.DecidedAt,
        };

        private async Task NotifyUser(string userId, string title, string body)
        {
            try
            {
                await notifications.SendUserAsync(userId, title, body);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao notificar o jogador {Player}.", userId);
            }
        }

        private async Task NotifyTeam(Guid teamId, string title, string body)
        {
            try
            {
                await notifications.SendTeamAsync(teamId.ToString(), title, body);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao notificar a equipa {Team}.", teamId);
            }
        }
    }
}
