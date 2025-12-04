using System.ComponentModel.DataAnnotations;

namespace Application.Hubs
{
    /// <summary>
    /// Objeto de Estado que representa a entrada de um administrador (admin) no lobby de Finalização de Partida.
    /// 
    /// Esta estrutura de dados é utilizada pelo serviço [ManagerFinishMatchService] para rastrear qual admin
    /// submeteu qual resultado e qual a sua conexão SignalR correspondente, permitindo a sincronização e notificação.
    /// </summary>
    public class EntryHubFinishMatch
    {
        /// <summary>
        /// O identificador único da conexão SignalR do administrador no momento da submissão.
        /// </summary>
        [Required]
        public string ConnectionId = null!;

        /// <summary>
        /// O objeto [JoinFinishMatch] que contém o ID da equipa e o resultado (golos) que este administrador submeteu.
        /// </summary>
        [Required]
        public JoinFinishMatch Result = null!;
    }
}