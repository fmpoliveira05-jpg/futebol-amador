using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using System.Collections.Concurrent;

namespace Application.Validators.Hubs
{
    /// <summary>
    /// Validador de Regras de Negócio para o Hub SignalR de Matchmaker Ranqueado ([RankMatchMakerHub]).
    /// 
    /// Esta classe verifica se as equipas e os critérios de procura (data, horário, idade média) são válidos
    /// antes de o processo de emparelhamento começar ou progredir.
    /// </summary>
    public class RankMatchMakerValidator : IRankMatchMakerValidator
    {
        /// <summary>
        /// Valida os parâmetros de entrada para iniciar uma procura de Matchmaker.
        /// </summary>
        /// <param name="idPlayer">O ID do jogador (Admin) que está a iniciar a procura.</param>
        /// <param name="idTeam">O ID da equipa que procura adversário.</param>
        /// <param name="hoursGame">A hora preferencial do jogo (Manhã, Tarde ou Noite).</param>
        /// <param name="connectionId">O ID da conexão SignalR.</param>
        /// <exception cref="ArgumentException">Se qualquer um dos IDs ou a hora do jogo for inválida.</exception>
        public void ValidateVariableJoinRankMatchMaker(string idPlayer, Guid idTeam, TimeOnly hoursGame, string connectionId)
        {
            if (idPlayer == string.Empty)
            {
                throw new ArgumentException("O id do jogador está vazio");
            }

            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa está vazio");
            }

            if (hoursGame != ModelConstants.HoursValidToCompetitiveMatch.MORNING &&
                hoursGame != ModelConstants.HoursValidToCompetitiveMatch.AFTERNOON &&
                hoursGame != ModelConstants.HoursValidToCompetitiveMatch.NIGHT)
            {
                throw new ArgumentException($"A hora da partida não corrresponde a uma hora válida para uma partida (horas válidas {ModelConstants.HoursValidToCompetitiveMatch.MORNING}, {ModelConstants.HoursValidToCompetitiveMatch.AFTERNOON} ou {ModelConstants.HoursValidToCompetitiveMatch.NIGHT}");
            }

            if (string.IsNullOrEmpty(connectionId))
            {
                throw new ArgumentException("A connection string está vazia");
            }
        }

        /// <summary>
        /// Valida se não há conflito de agendamento num intervalo de 12 horas.
        /// </summary>
        /// <remarks>
        /// Verifica a regra de negócio para a criação de jogos competitivos com espaçamento mínimo.
        /// </remarks>
        /// <param name="differenteHoursNowAndGame">A diferença em horas entre agora e a data proposta.</param>
        /// <param name="match">A partida de conflito encontrada (se houver).</param>
        /// <exception cref="InvalidOperationException">Se houver conflito com jogos próximos (12h).</exception>
        public void ValidateHoursToMatch(double differenteHoursNowAndGame, Matches match)
        {
            if (match != null)
            {
                throw new InvalidOperationException("Não pode marcar um jogo para este domingo uma vez que já tem um jogo marcado a pelo menos 12 horas do jogo a marcar");
            }

            if (differenteHoursNowAndGame <= 12)
            {
                throw new InvalidOperationException("Não pode marcar um jogo para este domingo, porque está a menos de 12 horas da hora do jogo");
            }
        }

        /// <summary>
        /// Valida a existência da entidade [Team].
        /// </summary>
        /// <param name="team">A entidade Team.</param>
        /// <exception cref="ArgumentException">Lançada se a equipa for nula.</exception>
        public void ValidateTeamJoinRankMatchMaker(Team? team)
        {
            if (team == null)
            {
                throw new ArgumentException("A equipa está nula");
            }
        }

        /// <summary>
        /// Valida as regras de elegibilidade de uma equipa para o Matchmaker Ranqueado.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Existência do administrador, número mínimo de jogadores (11), limites de idade média, e se a equipa já está no lobby.
        /// </remarks>
        /// <param name="team">A entidade Team (carregada com membros).</param>
        /// <param name="averageAge">A idade média calculada dos membros da equipa.</param>
        /// <param name="city">A cidade da equipa (para validação de contexto).</param>
        /// <param name="findUser">Booleano que indica se o utilizador que tenta entrar foi encontrado na BD.</param>
        /// <param name="hub">O dicionário do lobby ([ConcurrentDictionary]) para verificar duplicados.</param>
        /// <exception cref="NotFindException">Se o administrador não for encontrado.</exception>
        /// <exception cref="InvalidOperationException">Se a equipa não tiver membros suficientes ou já estiver em espera.</exception>
        /// <exception cref="ArgumentException">Se a idade média ou a cidade estiverem fora dos limites.</exception>
        public void ValidateJoinRankMatchMaker(Team team, float averageAge, string city, bool? findUser, ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub)
        {
            if (!findUser.HasValue || !findUser.Value)
            {
                throw new NotFindException("O administrador que quer procurar uma partida ranqueada não existe");
            }

            if (team.Members?.Count() < 11)
            {
                throw new InvalidOperationException("A equipa não pode jogar partidas rankeadas, porque ainda não tem no mínimo 11 jogadores");
            }

            if (averageAge < ModelConstants.TeamConst.MinAverageAge || averageAge > ModelConstants.TeamConst.MaxAverageAge)
            {
                throw new ArgumentException($"A equipa não possui a idade média permitida ({averageAge})");
            }

            if (string.IsNullOrEmpty(city))
            {
                throw new ArgumentException("A cidade do campo da equipa está ou não foi encontrada na morada");
            }

            if (hub.ContainsKey(team.Id))
            {
                throw new InvalidOperationException("Já existe um admin desta equipa a iniciar a partida");
            }
        }

        /// <summary>
        /// Valida o ID da equipa ao sair do Matchmaker (garantindo que não é Guid.Empty).
        /// </summary>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <exception cref="ArgumentException">Lançada se o ID for Guid.Empty.</exception>
        public void ValidateLeaveRankMatchMaker(Guid idTeam)
        {
            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa está vazio");
            }
        }
    }
}