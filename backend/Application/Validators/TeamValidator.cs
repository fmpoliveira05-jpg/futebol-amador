using Application.DTOs.Team;
using Application.Interfaces.Validators;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Validators
{
    public class TeamValidator : ITeamValidator
    {
        private IPlayerValidator PlayerValidator = new PlayerValidator();
        public void CreateTeamValidation(CreateTeamDto? createTeamDto, Teams? team, Player? playerCreating)
        {
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
        public void UpdateTeamValidation(Teams? existingTeamNewName, Teams? updatingTeam, Player? playerEditing)
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
        public void DeleteTeamValidation(Teams? team, Player? playerDeleting)
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

        public void GetAllTeamsValidation(IEnumerable<Teams?> Teams)
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
        public void RemovePlayerFromTeamValidation(Teams? team, Player? playerRemoving, Player? playerRemoved)
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
        }

        // Não validar se o team tem membros, porque adicionamos verificação no Player e quando o ultimo membro sair a equipa é eliminada!
        public void GetTeamMembersValidation(Teams? team)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }

        }

        public void GetMembershipRequestsValidation(Teams? team, Player? adminPlayer)
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
        public void ApproveMembershipRequestValidation(Teams? team, Player? playerApproving, Guid requestToDelete)
        {
            ValidatePlayerAndTeamExists(team, playerApproving);
            ValidatePlayerBelongToTeamAndIsAdmin(team, playerApproving);
            if (team.MembershipRequests.Any(mr => mr.Id == requestToDelete))
            {
                throw new ValidationException($"A equipa com Id '{team.Id}' não possui um pedido de adesão com Id '{requestToDelete}'.");
            }
            ValidateTeamFull(team);

        }

        public void RejectMembershipRequestValidation(Teams? team, Player? playerRejecting, Guid requestToDelete)
        {
            ValidatePlayerAndTeamExists(team, playerRejecting);
            ValidatePlayerBelongToTeamAndIsAdmin(team, playerRejecting);
            if (team.MembershipRequests.Any(mr => mr.Id == requestToDelete))
            {
                throw new ValidationException($"A equipa com Id '{team.Id}' não possui um pedido de adesão com Id '{requestToDelete}'.");
            }
        }

        public void SendMembershipRequestValidation(Teams? team, Player? playerSending, Player? playerReceiving)
        {
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
        public void DemoteAdminToMemberValidation(Teams? team, Player? adminToDemote, Player? adminDemoting)
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

        public void PromoteMemberToAdminValidation(Teams? team, Player? memberToPromote, Player? memberPromoting) {
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
        }

        public void GetTeamScheduleValidation(Teams? team)
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

        private void ValidatePlayerAndTeamExists(Teams? team, Player? Player)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }

            PlayerValidator.PlayerExists(Player);
        }

        private void ValidatePlayerBelongToTeamAndIsAdmin(Teams? team, Player? Player)
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

        private void ValidateTeamFull(Teams? team)
        {
            if (TeamIsFull(team))
            {
                throw new ValidationException($"A equipa com o Id '{team.Id}' já atingiu o número máximo de jogadores.");
            }
        }

        private bool TeamExists(Teams? team)
        {
            if (team == null)
            {
                return false;
            }
            return true;
        }

        private bool PlayerExistsInTeam(Teams? team, Player? Player)
        {
            if (!team.Members.Contains(Player))
            {
                return false;
                
            }
            return true;
        }

        private bool TeamIsFull(Teams? team)
        {
            if (team.Members.Count >= ModelConstants.TeamConst.MaxPlayers)
            {
                return true;
            }
            return false;
        }

        private bool AdminOlderThanSecondAdmin(Player? adminToDemote, Player? adminDemoting)
        {
            if (adminToDemote.IsAdminLastChangedAt >= adminDemoting.IsAdminLastChangedAt)
            {
                return false;
                //throw new ValidationException($"O Player? de id '{adminDemoting.Id}' não pode demitir o administrador com id '{adminToDemote.Id}' porque este é administrador há mais tempo.");
            }
            return true;
        }

        private bool CreateTeamDtoIsValid(CreateTeamDto createTeamDto)
        {
            if (string.IsNullOrWhiteSpace(createTeamDto.Name) || createTeamDto.HomePitch == null)
            {
                return false;
            }
            return true;
        }


    }
}
