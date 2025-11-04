using Application.DTOs.Filters;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Validators;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using System.Numerics;

namespace Application.Validators
{
    public class TeamValidator : ITeamValidator
    {
        private readonly IPlayerValidator PlayerValidator = new PlayerValidator();

        public void CreateTeamValidation(CreateTeamDto? createTeamDto, Rank rank,Team? team, Player? playerCreating)
        {
            if (rank == null)
            {
                throw new ValidationException("Não foi possível atribuir a classificação padrão à equipa.");
            }

            PlayerValidator.PlayerExists(playerCreating);
            if (TeamExists(team))
            {
                throw new ValidationException($"Já existe uma equipa com o nome'{team.Name}'");
            }


            if (playerCreating.IdTeam != null) {
                throw new ValidationException($"O jogador com o Id '{playerCreating.Id}' ja possui uma equipa.");
            }

            if (!CreateTeamDtoIsValid(createTeamDto))
            {
                throw new ValidationException($"O nome da equipa deve ter entre {ModelConstants.TeamConst.MinNameLength} e {ModelConstants.TeamConst.MaxNameLength} caracteres e a equipa deve possuir um campo.");
            }
        }

        /*
         Validações para atualizar uma equipa:
            - A equipa a ser atualizada deve existir.
            - O jogador que está a tentar atualizar a equipa deve existir.
            - O jogador que está a tentar atualizar a equipa deve ser um membro da equipa.
            - O jogador que está a tentar atualizar a equipa deve ser um administrador da equipa.
            - O novo nome da equipa (se for alterado) não deve ser igual ao nome de outra equipa existente.
         
         */
        public void UpdateTeamValidation(Team? existingTeamNewName, Team? updatingTeam, Player? playerEditing)
        {
            ValidatePlayerAndTeamExists(updatingTeam, playerEditing);


            ValidatePlayerBelongToTeamAndIsAdmin(updatingTeam, playerEditing);

            if (TeamExists(existingTeamNewName) && existingTeamNewName.Name != updatingTeam.Name)
            {
                throw new ValidationException($"Já existe uma equipa com o nome '{existingTeamNewName.Name}'.");
            }

        }

        /*
         Validações para eliminar uma equipa:
            - A equipa a ser eliminada deve existir.
            - O jogador que está a tentar eliminar a equipa deve existir.
            - O jogador que está a tentar eliminar a equipa deve ser um membro da equipa.
            - O jogador que está a tentar eliminar a equipa deve ser um administrador da equipa.
            - A equipa não deve ter partidas agendadas ou em progresso.
         */
        public void DeleteTeamValidation(Team? team, Player? playerDeleting)
        {
            ValidatePlayerAndTeamExists(team, playerDeleting);

            ValidatePlayerBelongToTeamAndIsAdmin(team, playerDeleting);

            if (team.Calendar.Matches.Any(m => m.MatchStatus == MatchStatus.SCHEDULED || m.MatchStatus == MatchStatus.IN_PROGRESS))
            {
                throw new ValidationException($"A equipa com o Id '{team.Id}' tem partidas agendadas ou em progresso e não pode ser eliminada.");
            }
        }

        public void GetTeamByIdValidation(TeamDetailsDto team)
        {
            if (team == null)
            {
                throw new NotFoundException("A equipa não existe.");
            }
        }

        public void GetAllTeamsValidation(IEnumerable<Team?> Teams)
        {
            if (Teams == null || !Teams.Any())
            {
                throw new NotFoundException("Não existem equipas.");
            }
        }

        /*
         Validações para remover um jogador de uma equipa:
            - A equipa deve existir.
            - O jogador que está a remover deve existir.
            - O jogador que está a ser removido deve existir.
            - O jogador que está a remover deve pertencer à equipa.
            - O jogador que está a remover deve ser um administrador da equipa.
            - O jogador que está a ser removido deve pertencer à equipa.
            - O jogador que está a remover não pode ser o mesmo que está a ser removido.
            - O jogador que está a remover deve ser administrador por mais tempo do que o jogador que está a ser removido.
         */
        public void RemovePlayerFromTeamValidation(Team? team, Player? playerRemoving, Player? playerRemoved)
        {
            ValidatePlayerAndTeamExists(team, playerRemoving);
            PlayerValidator.PlayerExists(playerRemoved);
            ValidatePlayerBelongToTeamAndIsAdmin(team, playerRemoving);
            if (!PlayerExistsInTeam(team, playerRemoved))
            {
                throw new ValidationException($"O jogador com o Id '{playerRemoved.Id}' não pertence à equipa com Id '{team.Id}'.");
            }
            if (playerRemoving.Id == playerRemoved.Id)
            {
                throw new ValidationException("Um administrador não pode expulsar-se a si próprio.");
            }
            if (playerRemoved.IsAdmin)
            {
                if (!AdminOlderThanSecondAdmin(playerRemoving, playerRemoved))
                {
                    throw new ValidationException($"O Player?? de id '{playerRemoving.Id}' não pode expulsar o jogador com id '{playerRemoved.Id}' porque este é administrador há mais tempo.");
                }
            }

            team.Members.Remove(playerRemoved);
            playerRemoved.IdTeam = null;
            if (playerRemoved.IsAdmin)
            {
                playerRemoved.IsAdmin = false;
                playerRemoved.IsAdminLastChangedAt = DateTime.UtcNow;
            }
        }

        // Não validar se o team tem membros, porque adicionamos verificação no Player e quando o ultimo membro sair a equipa é eliminada!
        public void GetTeamMembersValidation(Team? team)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }

        }

        public void GetMembershipRequestsValidation(Team? team, Player? adminPlayer)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }
            PlayerValidator.PlayerExists(adminPlayer);
            
            ValidatePlayerBelongToTeamAndIsAdmin(team, adminPlayer);

            if (team.MembershipRequests == null || !team.MembershipRequests.Any())
            {
                throw new NotFoundException("Não existem pedidos de adesão para esta equipa.");
            }
        }

        //não validar se o playerApproved pertence à equipa, porque a validação deve ser feita quando ele tenta aceitar o pedido!
        public void ApproveMembershipRequestValidation(Team? team, Player? playerApproving, Guid requestToDelete)
        {
            if (!TeamExists(team))
            {
                throw new ValidationException($"A equipa não existe.");
            }
            ValidatePlayerAndTeamExists(team, playerApproving);
            ValidatePlayerBelongToTeamAndIsAdmin(team, playerApproving);
            if (!team.MembershipRequests.Any(mr => mr.Id == requestToDelete))
            {
                throw new ValidationException($"A equipa com Id '{team.Id}' não possui um pedido de adesão com Id '{requestToDelete}'.");
            }
            ValidateTeamFull(team);

        }

        public void RejectMembershipRequestValidation(Team? team, Player? playerRejecting, Guid requestToDelete)
        {
            ValidatePlayerAndTeamExists(team, playerRejecting);
            ValidatePlayerBelongToTeamAndIsAdmin(team, playerRejecting);
            if (!team.MembershipRequests.Any(mr => mr.Id == requestToDelete))
            {
                throw new ValidationException($"A equipa com Id '{team.Id}' não possui um pedido de adesão com Id '{requestToDelete}'.");
            }
        }

        public void SendMembershipRequestValidation(MembershipRequest? mr, Team? team, Player? playerSending, Player? playerReceiving)
        {
            if (mr != null)
            {
                throw new ValidationException("Já existe um pedido pendente entre a equipa e este jogador.");
            }
            if (playerReceiving == null)
            {
                throw new ValidationException("O jogador convidado não foi encontrado.");
            }
            if (playerReceiving.IdTeam != Guid.Empty)
            {
                if (playerReceiving.IdTeam == team.Id)
                    throw new ValidationException("O jogador já pertence a esta equipa.");
                else
                    throw new ValidationException("O jogador já pertence a outra equipa e não pode ser convidado.");
            }

            ValidatePlayerAndTeamExists(team, playerSending);
            PlayerValidator.PlayerExists(playerReceiving);
            ValidatePlayerBelongToTeamAndIsAdmin(team, playerSending);
            if (TeamIsFull(team))
            {
                throw new ValidationException($"A equipa com o Id '{team.Id}' já atingiu o número máximo de jogadores.");
            }
            ValidateTeamFull(team);

        }

        /*
            validações para rebaixar um administrador a membro:
            - A equipa deve existir.
            - O jogador que está a ser rebaixado deve existir.
            - O jogador que está a rebaixar deve existir.
            - O jogador que está a ser rebaixado deve pertencer à equipa.
            - O jogador que está a rebaixar deve pertencer à equipa.
            - O jogador que está a ser rebaixado deve ser um administrador da equipa.
            - O jogador que está a rebaixar deve ser um administrador da equipa.
            - O jogador que está a rebaixar deve ter sido administrador por mais tempo do que o jogador que está a ser demitido.
         */
        public void DemoteAdminToMemberValidation(Team? team, Player? adminToDemote, Player? adminDemoting)
        {
            ValidatePlayerAndTeamExists(team, adminToDemote);

            PlayerValidator.PlayerExists(adminDemoting);

            if (adminToDemote.Id == adminDemoting.Id)
            {
                throw new ValidationException("Um administrador não pode rebaixar-se a si próprio.");
            }

            ValidatePlayerBelongToTeamAndIsAdmin(team, adminToDemote);
            ValidatePlayerBelongToTeamAndIsAdmin(team, adminDemoting);

            if (!AdminOlderThanSecondAdmin(adminToDemote, adminDemoting))
            {
                throw new ValidationException($"O Player? de id '{adminDemoting.Id}' não pode demitir o administrador com id '{adminToDemote.Id}' porque este é administrador há mais tempo.");
            }
        }

        public void PromoteMemberToAdminValidation(Team? team, Player? memberToPromote, Player? memberPromoting) {
            ValidatePlayerAndTeamExists(team, memberToPromote);

            PlayerValidator.PlayerExists(memberPromoting);

            if (memberToPromote.Id == memberPromoting.Id)
            {
                throw new ValidationException("Um administrador não pode rebaixar-se a si próprio.");
            }

            ValidatePlayerBelongToTeamAndIsAdmin(team, memberPromoting);

            if (!PlayerExistsInTeam(team,memberToPromote))
            {
                throw new ValidationException($"O Player? de id '{memberToPromote.Id}' não pertence a equipa '{team.Name}'.");
            }

            if (memberToPromote.IsAdmin)
            {
                throw new ValidationException($"O jogador '{memberToPromote.Name}' já é administrador da equipa '{team.Name}'.");
            }

            var adminCount = team.Members.Count(m => m.IsAdmin);
            if (adminCount >= 3)
            {
                throw new ValidationException($"A equipa '{team.Name}' já tem o número máximo de administradores.");
            }
        }

        public void GetTeamScheduleValidation(Team? team)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }
            if (team.Calendar.Matches == null || !team.Calendar.Matches.Any())
            {
                throw new NotFoundException("A equipa não possui partidas agendadas.");
            }
        }

        public void ValidateVariableSearchTeam(Guid idTeam)
        {
            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa está vazio");
            }
        }

        public void ValidateVaribleSearchTeamWithFilters(Guid idTeam, FilterListTeamDto filter)
        {
            ValidateVariableSearchTeam(idTeam);

            if (filter.MinNumberPoints.HasValue && filter.MaxNumberPoints.HasValue)
            {
                if (filter.MinNumberPoints > filter.MaxNumberPoints)
                {
                    throw new InvalidOperationException("O numero minimo de pontos de uma equipa, não deve ser superior ao numero maximo");
                }
            }

            if (filter.MinAge.HasValue && filter.MaxAge.HasValue)
            {
                if (filter.MinAge.Value > filter.MaxAge.Value)
                {
                    throw new InvalidOperationException("O numero minimo de idade minima tem de ser inferior à idade media maxima");
                }
            }

            if (filter.MinNumberPlayers.HasValue && filter.MaxNumberPlayers.HasValue)
            {
                if (filter.MinNumberPlayers.Value > filter.MaxNumberPlayers.Value)
                {
                    throw new InvalidOperationException("O número minimo de membros deve ser superior ao numero maximo de membros");
                }
            }
        }

        public void ValidateTeamSearch(Team team)
        {
            if (team == null)
            {
                throw new ArgumentException("A equipa não existe");
            }
        }

        public void ValidateFiltersGetPlayersWithout(FilterPlayersWithoutTeamDto filter)
        {
            if (filter.MinAge.HasValue && filter.MaxAge.HasValue)
            {
                if(filter.MinAge > filter.MaxAge)
                {
                    throw new InvalidOperationException("A idade mínima do jogador deve ser inferior ou igual há idade máxima");
                }
            }

            if (filter.MinHeight.HasValue && filter.MaxHeight.HasValue)
            {
                if (filter.MinHeight > filter.MaxHeight)
                {
                    throw new InvalidOperationException("A altura mínima do jogador deve ser inferior ou igual há altura máxima");
                }
            }

            if (filter.Position.HasValue && !Enum.IsDefined(typeof(Position), filter.Position))
            {
                throw new InvalidOperationException("A posição que introduziu não existe. Por favor introduza uma posição valida!");
            }
        }

        #region Private Methods
        private void ValidatePlayerAndTeamExists(Team? team, Player? Player)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }

            PlayerValidator.PlayerExists(Player);
        }

        private static void ValidatePlayerBelongToTeamAndIsAdmin(Team? team, Player? Player)
        {
            if (!PlayerExistsInTeam(team, Player))
            {
                throw new ValidationException($"O jogador com o Id '{Player?.Id}' não pertence à equipa com Id '{team.Id}'.");
            }
            if (!Player.IsAdmin)
            {
                throw new ValidationException($"O jogador com o Id '{Player?.Id}' não é administrador da equipa.");
            }
        }

        private static void ValidateTeamFull(Team? team)
        {
            if (TeamIsFull(team))
            {
                throw new ValidationException($"A equipa com o Id '{team.Id}' já atingiu o número máximo de jogadores.");
            }
        }

        private static bool TeamExists(Team? team)
        {
            if (team == null)
            {
                return false;
            }
            return true;
        }

        private static bool PlayerExistsInTeam(Team? team, Player? Player)
        {
            if (team == null || Player == null)
                return false;

            return team.Members.Any(m => m.Id == Player.Id);
        }

        private static bool TeamIsFull(Team? team)
        {
            if (team.Members.Count >= ModelConstants.TeamConst.MaxMembers)
            {
                return true;
            }
            return false;
        }

        private static bool AdminOlderThanSecondAdmin(Player? adminToDemote, Player? adminDemoting)
        {
            if (adminToDemote.IsAdminLastChangedAt >= adminDemoting.IsAdminLastChangedAt)
            {
                return false;
                //throw new ValidationException($"O Player? de id '{adminDemoting.Id}' não pode demitir o administrador com id '{adminToDemote.Id}' porque este é administrador há mais tempo.");
            }
            return true;
        }

        private static bool CreateTeamDtoIsValid(CreateTeamDto createTeamDto)
        {
            if (string.IsNullOrWhiteSpace(createTeamDto.Name) || createTeamDto.HomePitch == null)
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}
