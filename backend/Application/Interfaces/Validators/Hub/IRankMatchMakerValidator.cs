using Application.DTOs.RankMatchMaker;
using Domain.Entities;
using System.Collections.Concurrent;

namespace Application.Interfaces.Validators.Hub
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio para o Hub SignalR de Matchmaker Ranqueado ([RankMatchMakerHub]).
    /// 
    /// Define as regras síncronas que verificam a elegibilidade das equipas, a validade dos IDs e a ausência de conflitos
    /// antes de o processo de emparelhamento começar.
    /// </summary>
    public interface IRankMatchMakerValidator
    {
        /// <summary>
        /// Valida os parâmetros de entrada essenciais para iniciar uma procura de Matchmaker.
        /// </summary>
        /// <param name="idPlayer">O ID do jogador (Admin) que inicia a procura.</param>
        /// <param name="idTeam">O ID da equipa que procura adversário.</param>
        /// <param name="hoursGame">A hora preferencial do jogo (Manhã, Tarde ou Noite).</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <exception cref="System.ArgumentException">Lançada se qualquer um dos IDs ou a hora do jogo for inválida.</exception>
        public void ValidateVariableJoinRankMatchMaker(string idPlayer, Guid idTeam, TimeOnly hoursGame, string connectionId);

        /// <summary>
        /// Valida se não há conflito de agendamento num intervalo de 12 horas com partidas já agendadas para a equipa.
        /// </summary>
        /// <remarks>
        /// A regra de negócio garante um espaçamento mínimo de 12 horas entre jogos.
        /// </remarks>
        /// <param name="differenteHoursNowAndGame">A diferença em horas entre a hora atual e a data proposta.</param>
        /// <param name="match">A partida de conflito encontrada (deve ser nula para continuar).</param>
        /// <exception cref="System.InvalidOperationException">Lançada se houver conflito com jogos próximos (12h).</exception>
        public void ValidateHoursToMatch(double differenteHoursNowAndGame, Matches match);

        /// <summary>
        /// Valida se a entidade [Team] foi encontrada.
        /// </summary>
        /// <param name="team">A entidade Team.</param>
        /// <exception cref="System.ArgumentException">Lançada se a equipa for nula.</exception>
        public void ValidateTeamJoinRankMatchMaker(Team? team);

        /// <summary>
        /// Valida as regras de elegibilidade de uma equipa para o Matchmaker Ranqueado.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Existência do administrador, número mínimo de jogadores, limites de idade média, e se a equipa já está no lobby.
        /// </remarks>
        /// <param name="team">A entidade Team (carregada com membros).</param>
        /// <param name="averageAge">A idade média calculada dos membros da equipa.</param>
        /// <param name="city">A cidade da equipa (para validação de contexto).</param>
        /// <param name="findUser">Booleano que indica se o utilizador que tenta entrar foi encontrado na BD.</param>
        /// <param name="hub">O dicionário ([ConcurrentDictionary]) que representa o estado atual do lobby.</param>
        /// <exception cref="Domain.Exceptions.NotFindException">Se o administrador não for encontrado.</exception>
        /// <exception cref="System.InvalidOperationException">Se a equipa não tiver membros suficientes ou já estiver em espera.</exception>
        public void ValidateJoinRankMatchMaker(Team team, float averageAge, string city, bool? findUser, ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub);

        /// <summary>
        /// Valida o ID da equipa ao sair do Matchmaker (garantindo que não é Guid.Empty).
        /// </summary>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <exception cref="System.ArgumentException">Lançada se o ID for Guid.Empty.</exception>
        public void ValidateLeaveRankMatchMaker(Guid idTeam);
    }
}