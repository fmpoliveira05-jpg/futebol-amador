using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Hubs
{
    /// <summary>
    /// Objeto de Transferência de Estado (State DTO) que representa o resultado da tentativa de um administrador
    /// entrar no Hub de Início de Partida ([StartMatchHub]).
    /// 
    /// É utilizado para comunicar à lógica de negócio e ao cliente (mobile) se o administrador entrou na fila de espera
    /// ou se, pelo contrário, o jogo foi imediatamente iniciado.
    /// </summary>
    public class JoinStartMatchResult
    {
        /// <summary>
        /// Flag que indica se o administrador que acabou de se ligar é o primeiro a entrar no lobby da partida.
        /// </summary>
        [Required]
        public bool IsFirstAdmin { get; set; }

        /// <summary>
        /// Flag que indica se a partida foi imediatamente iniciada após a entrada deste administrador.
        /// </summary>
        /// <remarks>
        /// O valor <c>true</c> significa que este administrador foi o segundo a entrar no lobby, e o jogo pode prosseguir para o estado [IN_PROGRESS].
        /// </remarks>
        [Required]
        public bool MatchStarted { get; set; } // A true quer dizer que já temos os dois admins e a match vai ser iniciada

        /// <summary>
        /// O ID da equipa associado ao administrador que acabou de se ligar.
        /// </summary>
        [Required]
        public Guid TeamId { get; set; }

        /// <summary>
        /// O ID da conexão SignalR do primeiro administrador a entrar no lobby.
        /// 
        /// É utilizado para notificar ou remover o primeiro administrador do grupo quando o jogo é iniciado pelo segundo administrador.
        /// É nulo se este for o primeiro administrador.
        /// </summary>
        public string? FirstAdminConnectionId { get; set; }

        /// <summary>
        /// A entidade [Matches] que está a ser gerida.
        /// Contém os dados atualizados da partida se [MatchStarted] for <c>true</c>.
        /// </summary>
        public Matches? Match { get; set; }
    }
}