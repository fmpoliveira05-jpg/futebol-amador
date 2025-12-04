using Application.DTOs.Filters;
using Application.DTOs.Team;
using Application.Interfaces.Validators;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Validators
{
    /// <summary>
    /// Validador de Regras de Negócio e de Entidade para operações relacionadas com [Team].
    /// 
    /// Esta classe é responsável por verificar a validade das operações antes de a persistência de dados ser executada,
    /// garantindo a integridade dos dados e o controlo de permissões.
    /// </summary>
    public class TeamValidator : ITeamValidator
    {
        /// <summary>
        /// Instância injetada do validador de Jogadores, utilizada para validar a existência de jogadores.
        /// </summary>
        private readonly IPlayerValidator PlayerValidator = new PlayerValidator();

        /// <summary>
        /// Construtor padrão da classe [TeamValidator].
        /// </summary>
        public TeamValidator() { }

        /// <summary>
        /// Valida as condições para a criação de uma nova equipa.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>O Rank padrão (DefaultRank) deve existir.</item>
        ///     <item>O jogador criador deve existir (validação delegada a [PlayerValidator]).</item>
        ///     <item>Não deve existir já uma equipa com o nome pretendido.</item>
        ///     <item>O jogador criador não deve pertencer já a outra equipa.</item>
        ///     <item>O DTO de criação deve ser válido (Nome e Campo obrigatórios).</item>
        /// </list>
        /// </remarks>
        /// <param name="createTeamDto">O DTO com os dados da nova equipa.</param>
        /// <param name="rank">O Rank padrão a ser atribuído.</param>
        /// <param name="team">A equipa existente com o mesmo nome (se houver).</param>
        /// <param name="playerCreating">O jogador autenticado que está a criar a equipa.</param>
        /// <exception cref="ValidationException">Se o nome for inválido, o jogador já tiver equipa, ou faltarem dados obrigatórios.</exception>
        /// <exception cref="NotFoundException">Se já existir uma equipa com o mesmo nome ou o Rank padrão não for encontrado.</exception>
        public void CreateTeamValidation(CreateTeamDto? createTeamDto, Rank rank,Team? team, Player? playerCreating)
        {
            if (rank == null)
            {
                throw new ValidationException("Não foi possível atribuir a classificação padrão à equipa.");
            }

            PlayerValidator.PlayerExists(playerCreating);
            if (TeamExists(team))
            {
                throw new NotFoundException($"Já existe uma equipa com o nome'{team.Name}'");
            }


            if (playerCreating.IdTeam != null) {
                throw new ValidationException($"O jogador com o Id '{playerCreating.Id}' ja possui uma equipa.");
            }

            if (!CreateTeamDtoIsValid(createTeamDto))
            {
                throw new ValidationException($"O nome da equipa deve ter entre {ModelConstants.TeamConst.MinNameLength} e {ModelConstants.TeamConst.MaxNameLength} caracteres e a equipa deve possuir um campo.");
            }
        }

        /// <summary>
        /// Valida as condições para a atualização de dados de uma equipa.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>A equipa a ser atualizada deve existir.</item>
        ///     <item>O novo nome (se alterado) não pode pertencer a outra equipa existente.</item>
        /// </list>
        /// </remarks>
        /// <param name="existingTeamNewName">A equipa encontrada com o novo nome (para verificação de unicidade).</param>
        /// <param name="updatingTeam">A entidade da equipa que está a ser atualizada.</param>
        /// <exception cref="NotFoundException">Se a equipa não for encontrada.</exception>
        /// <exception cref="ValidationException">Se o nome pretendido já estiver em uso.</exception>
        public void UpdateTeamValidation(Team? existingTeamNewName, Team? updatingTeam)
        {
            if (updatingTeam == null)
            {
                throw new NotFoundException("A equipa não existe.");
            }

            if (existingTeamNewName != null && existingTeamNewName.Id != updatingTeam.Id)
            {
                throw new ValidationException($"Já existe uma equipa com o nome '{existingTeamNewName.Name}'.");
            }

        }

        /// <summary>
        /// Valida as condições para a eliminação de uma equipa.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>A equipa deve existir.</item>
        ///     <item>A equipa não deve ter partidas agendadas ou em progresso.</item>
        /// </list>
        /// </remarks>
        /// <param name="team">A entidade da equipa a ser eliminada (carregada com o calendário e partidas).</param>
        /// <exception cref="NotFoundException">Se a equipa não for encontrada.</exception>
        /// <exception cref="ValidationException">Se a equipa tiver partidas ativas.</exception>
        public void DeleteTeamValidation(Team? team)
        {
            if(!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }

            if (team.Calendar.Matches.Any(m => m.MatchStatus == MatchStatus.SCHEDULED || m.MatchStatus == MatchStatus.IN_PROGRESS))
            {
                throw new ValidationException($"A equipa com o Id '{team.Id}' tem partidas agendadas ou em progresso e não pode ser eliminada.");
            }
        }

        /// <summary>
        /// Validação de existência para o DTO de detalhes de equipa.
        /// </summary>
        /// <param name="team">O DTO de detalhes da equipa.</param>
        /// <exception cref="NotFoundException">Se o DTO for nulo (indicando que a equipa não foi encontrada).</exception>
        public void GetTeamByIdValidation(TeamDetailsDto team)
        {
            if (team == null)
            {
                throw new NotFoundException("A equipa não existe.");
            }
        }

        /// <summary>
        /// Valida se a coleção de equipas retornada não é nula nem vazia.
        /// </summary>
        /// <param name="Teams">A coleção de equipas.</param>
        /// <exception cref="NotFoundException">Se não existirem equipas na coleção.</exception>
        public void GetAllTeamsValidation(IEnumerable<Team?> Teams)
        {
            if (Teams == null || !Teams.Any())
            {
                throw new NotFoundException("Não existem equipas.");
            }
        }

        /// <summary>
        /// Valida as condições e permissões para remover um jogador de uma equipa.
        /// </summary>
        /// <remarks>
        /// **Regra Crítica:** Se o jogador a ser removido for um Admin, o Admin que está a remover
        /// tem de ter sido Admin por mais tempo (Admin mais antigo é mais poderoso).
        /// </remarks>
        /// <param name="team">A equipa.</param>
        /// <param name="playerRemoving">O administrador que está a remover/expulsar.</param>
        /// <param name="playerRemoved">O jogador que está a ser removido.</param>
        /// <exception cref="NotFoundException">Se a equipa não existir.</exception>
        /// <exception cref="ValidationException">Se o admin tentar expulsar-se a si próprio ou se a regra de antiguidade for violada.</exception>
        public void RemovePlayerFromTeamValidation(Team? team, Player? playerRemoving, Player? playerRemoved)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
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

        /// <summary>
        /// Valida a existência da equipa antes de obter a lista de membros.
        /// </summary>
        /// <param name="team">A equipa.</param>
        /// <exception cref="NotFoundException">Se a equipa não for encontrada.</exception>
        public void GetTeamMembersValidation(Team? team)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }

        }

        /// <summary>
        /// Valida se a equipa existe e se há pedidos de adesão pendentes.
        /// </summary>
        /// <param name="team">A equipa (carregada com MembershipRequests).</param>
        /// <exception cref="NotFoundException">Se a equipa não existir ou não houver pedidos pendentes.</exception>
        public void GetMembershipRequestsValidation(Team? team)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }
            
            if (team.MembershipRequests == null || !team.MembershipRequests.Any())
            {
                throw new NotFoundException("Não existem pedidos de adesão para esta equipa.");
            }
        }

        /// <summary>
        /// Valida o pedido de aprovação de adesão/convite.
        /// </summary>
        /// <remarks>
        /// Verifica a existência da equipa e se o pedido (request) está pendente na coleção da equipa.
        /// </remarks>
        /// <param name="team">A equipa.</param>
        /// <param name="requestToDelete">O ID do pedido de adesão.</param>
        /// <exception cref="NotFoundException">Se a equipa não existir.</exception>
        /// <exception cref="ValidationException">Se o pedido não pertencer à equipa.</exception>
        public void ApproveMembershipRequestValidation(Team? team, Guid requestToDelete)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException($"A equipa não existe.");
            }

            if (!team.MembershipRequests.Any(mr => mr.Id == requestToDelete))
            {
                throw new ValidationException($"A equipa com Id '{team.Id}' não possui um pedido de adesão com Id '{requestToDelete}'.");
            }

            ValidateTeamFull(team);
        }

        /// <summary>
        /// Valida o pedido de rejeição de adesão/convite.
        /// </summary>
        /// <param name="team">A equipa.</param>
        /// <param name="requestToDelete">O ID do pedido de adesão.</param>
        /// <exception cref="NotFoundException">Se a equipa não existir.</exception>
        /// <exception cref="ValidationException">Se o pedido não pertencer à equipa.</exception>
        public void RejectMembershipRequestValidation(Team? team, Guid requestToDelete)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }

            if (!team.MembershipRequests.Any(mr => mr.Id == requestToDelete))
            {
                throw new ValidationException($"A equipa com Id '{team.Id}' não possui um pedido de adesão com Id '{requestToDelete}'.");
            }
        }

        /// <summary>
        /// Valida um novo pedido de adesão (Join Request ou Convite de Recrutamento).
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>Não pode existir um pedido pendente (duplicado) entre os mesmos jogador/equipa.</item>
        ///     <item>A equipa não pode estar cheia.</item>
        /// </list>
        /// </remarks>
        /// <param name="mr">O pedido de adesão (se já existir).</param>
        /// <param name="team">A equipa alvo.</param>
        /// <exception cref="ValidationException">Se já existir um pedido pendente ou a equipa estiver cheia.</exception>
        public void SendMembershipRequestValidation(MembershipRequest? mr, Team? team)
        {
            if (mr != null)
            {
                throw new ValidationException("Já existe um pedido pendente entre a equipa e este jogador.");
            }

            if (TeamIsFull(team))
            {
                throw new ValidationException($"A equipa com o Id '{team.Id}' já atingiu o número máximo de jogadores.");
            }
            ValidateTeamFull(team);

        }

        /// <summary>
        /// Valida a lógica e permissões para despromover um Administrador a Membro regular.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>As entidades devem existir e pertencer à mesma equipa.</item>
        ///     <item>Um admin não pode despromover-se a si próprio.</item>
        ///     <item>O admin que despromove deve ser o admin "mais antigo" (regra de antiguidade).</item>
        /// </list>
        /// </remarks>
        /// <param name="team">A equipa.</param>
        /// <param name="adminToDemote">O admin alvo da despromoção.</param>
        /// <param name="adminDemoting">O admin que está a executar a ação.</param>
        /// <exception cref="NotFoundException">Se a equipa não existir.</exception>
        /// <exception cref="ValidationException">Se as permissões ou regras de antiguidade forem violadas.</exception>
        public void DemoteAdminToMemberValidation(Team? team, Player? adminToDemote, Player? adminDemoting)
        {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }

            if (adminToDemote.Team != adminDemoting.Team)
            {
                throw new ValidationException("O jogador alvo pertence a outra equipa!");
            }

            if (adminToDemote.Id == adminDemoting.Id)
            {
                throw new ValidationException("Um administrador não pode rebaixar-se a si próprio.");
            }

            if (!AdminOlderThanSecondAdmin(adminToDemote, adminDemoting))
            {
                throw new ValidationException($"O Player? de id '{adminDemoting.Id}' não pode demitir o administrador com id '{adminToDemote.Id}' porque este é administrador há mais tempo.");
            }
        }

        /// <summary>
        /// Valida as condições para promover um membro a Administrador da equipa.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>O alvo da promoção deve pertencer à equipa.</item>
        ///     <item>O número de administradores não pode exceder o limite (3).</item>
        ///     <item>O alvo não pode ser já um administrador.</item>
        /// </list>
        /// </remarks>
        /// <param name="team">A equipa (carregada com membros).</param>
        /// <param name="memberToPromote">O membro a ser promovido.</param>
        /// <param name="memberPromoting">O admin que executa a promoção.</param>
        /// <exception cref="NotFoundException">Se a equipa não for encontrada.</exception>
        /// <exception cref="ValidationException">Se a equipa tiver demasiados admins ou o alvo já for admin.</exception>
        public void PromoteMemberToAdminValidation(Team? team, Player? memberToPromote, Player? memberPromoting) {
            if (!TeamExists(team))
            {
                throw new NotFoundException("A equipa não existe.");
            }

            if (memberToPromote.Id == memberPromoting.Id)
            {
                throw new ValidationException("Um administrador não pode rebaixar-se a si próprio.");
            }

            if (memberToPromote.Team != memberPromoting.Team)
            {
                throw new ValidationException("O jogador alvo da promoção não pertence à equipa!");
            }

            var adminCount = team.Members.Count(m => m.IsAdmin);
            if (adminCount >= 3)
            {
                throw new ValidationException($"A equipa '{team.Name}' já tem o número máximo de administradores.");
            }

            if (memberToPromote.Team == null)
            {
                throw new ValidationException("O jogador alvo da promoção não pertence a nenhuma equipa!");
            }

            if (memberToPromote.IsAdmin)
            {
                throw new ValidationException("O jogador alvo já é administrador da equipa.");
            }
        }

        /// <summary>
        /// Valida a existência da equipa antes de obter a agenda.
        /// </summary>
        /// <param name="team">A equipa.</param>
        /// <exception cref="NotFoundException">Se a equipa não for encontrada.</exception>
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

        /// <summary>
        /// Valida se o ID da equipa é válido antes de iniciar a pesquisa.
        /// </summary>
        /// <param name="idTeam">O ID (GUID) da equipa.</param>
        /// <exception cref="ArgumentException">Se o ID for Guid.Empty.</exception>
        public void ValidateVariableSearchTeam(Guid idTeam)
        {
            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa está vazio");
            }
        }

        /// <summary>
        /// Valida o ID da equipa de pesquisa e os filtros de listagem.
        /// </summary>
        /// <param name="idTeam">O ID (GUID) da equipa.</param>
        /// <param name="filter">O filtro DTO.</param>
        public void ValidateVaribleSearchTeamWithFilters(Guid idTeam, FilterListTeamDto filter)
        {
            ValidateVariableSearchTeam(idTeam);
            validateFilterTeam(filter);
        }

        /// <summary>
        /// Valida apenas os filtros de listagem de equipas.
        /// </summary>
        /// <param name="filter">O DTO de filtros.</param>
        public void ValidateFilterTeams(FilterListTeamDto? filter)
        {
            validateFilterTeam(filter);
        }

        /// <summary>
        /// Valida se a equipa procurada existe.
        /// </summary>
        /// <param name="team">A equipa.</param>
        /// <exception cref="ArgumentException">Se a equipa for nula.</exception>
        public void ValidateTeamSearch(Team? team)
        {
            if (team == null)
            {
                throw new ArgumentException("A equipa não existe");
            }
        }

        /// <summary>
        /// Valida os filtros de pesquisa para jogadores sem equipa (Agentes Livres).
        /// </summary>
        /// <param name="filter">O DTO de filtros.</param>
        public void ValidateFiltersGetPlayersWithout(FilterTeamDto? filter)
        {
            if(filter != null)
            {
                if (filter.MinAge.HasValue && filter.MaxAge.HasValue && filter.MinAge > filter.MaxAge)
                {
                    throw new InvalidOperationException("A idade mínima do jogador deve ser inferior ou igual há idade máxima");
                }

                if (filter.MinHeight.HasValue && filter.MaxHeight.HasValue && filter.MinHeight > filter.MaxHeight)
                {
                    throw new InvalidOperationException("A altura mínima do jogador deve ser inferior ou igual há altura máxima");
                }

                if (filter.Position.HasValue && !Enum.IsDefined(typeof(Position), filter.Position.Value))
                {
                    throw new InvalidOperationException("A posição que introduziu não existe. Por favor introduza uma posição valida!");
                }
            }  
        }

        #region Private Methods

        /// <summary>
        /// Valida a consistência do intervalo de filtros numéricos (Pontos, Idade Média, Contagem de Membros).
        /// </summary>
        /// <param name="filter">O DTO de filtros de listagem de equipas.</param>
        private void validateFilterTeam(FilterListTeamDto? filter)
        {
            if (filter != null)
            {
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
        }

        /// <summary>
        /// Verifica se a equipa atingiu o número máximo de membros e lança [ValidationException] se estiver cheia.
        /// </summary>
        /// <param name="team">A equipa.</param>
        private static void ValidateTeamFull(Team? team)
        {
            if (TeamIsFull(team))
            {
                throw new ValidationException($"A equipa com o Id '{team.Id}' já atingiu o número máximo de jogadores.");
            }
        }

        /// <summary>
        /// Verifica se a entidade da equipa não é nula.
        /// </summary>
        /// <param name="team">A equipa.</param>
        /// <returns><c>true</c> se a equipa existir.</returns>
        private static bool TeamExists(Team? team)
        {
            bool exists = true;

            if (team == null)
            {
                exists = false;
            }

            return exists;
        }

        /// <summary>
        /// Verifica se o jogador pertence à equipa.
        /// </summary>
        /// <param name="team">A equipa (carregada com membros).</param>
        /// <param name="Player">O jogador.</param>
        /// <returns><c>true</c> se o jogador for membro.</returns>
        private static bool PlayerExistsInTeam(Team? team, Player? Player)
        {
            if (team == null || Player == null)
                return false;

            return team.Members.Any(m => m.Id == Player.Id);
        }

        /// <summary>
        /// Verifica se a equipa atingiu o número máximo de membros permitido.
        /// </summary>
        /// <param name="team">A equipa.</param>
        /// <returns><c>true</c> se a contagem de membros for igual ou superior ao máximo.</returns>
        private static bool TeamIsFull(Team? team)
        {
            bool isFull = false;
            
            if (team.Members.Count >= ModelConstants.TeamConst.MaxMembers)
            {
                isFull = true;
            }

            return isFull;
        }

        /// <summary>
        /// Regra de Antiguidade: Verifica se o administrador que está a demitir/rebaixar é mais antigo (ou igual) em tempo de administração do que o administrador alvo.
        /// </summary>
        /// <param name="adminToDemote">O admin alvo (a ser demitido).</param>
        /// <param name="adminDemoting">O admin que executa a ação (o remetente).</param>
        /// <returns><c>true</c> se o admin que demite for mais antigo ou se a data de alteração for a mesma.</returns>
        private static bool AdminOlderThanSecondAdmin(Player? adminToDemote, Player? adminDemoting)
        {
            bool isOlder = false;
            
            if (adminToDemote.IsAdminLastChangedAt >= adminDemoting.IsAdminLastChangedAt)
            {
                isOlder = true;
            }

            return isOlder;
        }

        /// <summary>
        /// Valida se o DTO de criação de equipa é válido (verificando campos obrigatórios).
        /// </summary>
        /// <param name="createTeamDto">O DTO de criação.</param>
        /// <returns><c>true</c> se Nome e Pitch existirem.</returns>
        private static bool CreateTeamDtoIsValid(CreateTeamDto createTeamDto)
        {
            bool invalid = true;

            if (string.IsNullOrWhiteSpace(createTeamDto.Name) || createTeamDto.HomePitch == null)
            {
                invalid = false;
            }

            return invalid;
        }
        #endregion
    }
}