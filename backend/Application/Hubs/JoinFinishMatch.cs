using Application.DTOs.Match;

namespace Application.Hubs
{
    /// <summary>
    /// Objeto de Transferência de Estado (State DTO) que representa o resultado imediato da tentativa de um administrador
    /// entrar ou submeter um resultado no Hub de Finalização de Partida ([FinishMatchHub]).
    /// 
    /// É utilizado para comunicar à lógica de negócio e ao cliente (mobile) o estado atual do processo de sincronização
    /// (se o jogo terminou, se os resultados coincidiram, ou se o administrador deve esperar).
    /// </summary>
    public class JoinFinishMatch
    {
        /// <summary>
        /// Flag que indica se o administrador que acabou de se ligar é o primeiro a submeter o resultado.
        /// </summary>
        public bool IsFirstAdmin { get; set; }

        /// <summary>
        /// Flag que indica se o jogo foi finalizado após a submissão (porque ambos os admins já submeteram).
        /// </summary>
        /// <remarks>
        /// O valor <c>true</c> significa que a lógica de Finalização/Sincronização foi acionada.
        /// </remarks>
        public bool MatchFinish { get; set; } // A true quer dizer que já temos os dois admins e a match vai ser iniciada

        /// <summary>
        /// O ID da equipa associado ao administrador que submeteu o resultado.
        /// </summary>
        public Guid IdTeam { get; set; }

        /// <summary>
        /// O DTO [ResultMatchDto] contendo os golos que este administrador submeteu.
        /// </summary>
        public ResultMatchDto? ResultMatch { get; set; }

        /// <summary>
        /// O ID da conexão SignalR do primeiro administrador a entrar no lobby.
        /// 
        /// É utilizado para notificar ou remover o primeiro administrador do grupo quando o jogo é finalizado pelo segundo.
        /// </summary>
        public string? FirstAdminConnectionId { get; set; }

        /// <summary>
        /// Flag que indica se os resultados submetidos pelas duas equipas **coincidiram**.
        /// </summary>
        /// <remarks>
        /// <c>true</c> se a submissão for bem-sucedida e os golos forem iguais; <c>false</c> se os resultados divergirem. 
        /// É nulo se apenas o primeiro administrador tiver submetido.
        /// </remarks>
        public bool? IsCoincides { get; set; }
    }
}