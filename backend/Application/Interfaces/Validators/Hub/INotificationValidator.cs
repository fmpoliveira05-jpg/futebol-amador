namespace Application.Interfaces.Validators.Hub
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio para o Hub SignalR de Notificações.
    /// 
    /// Define a regra de validação essencial para verificar se um utilizador autenticado
    /// pertence a uma equipa antes de permitir que ele subscreva o seu canal de notificações.
    /// </summary>
    public interface INotificationValidator
    {
        /// <summary>
        /// Valida se o utilizador autenticado é membro da equipa alvo.
        /// </summary>
        /// <remarks>
        /// Esta é uma verificação de segurança crucial: apenas membros da equipa podem subscrever o seu canal de notificações.
        /// O serviço deve buscar tanto a equipa quanto o jogador para verificar a afiliação.
        /// </remarks>
        /// <param name="teamId">O ID (GUID) da equipa alvo cuja subscrição está a ser solicitada.</param>
        /// <param name="userId">O ID (string) do utilizador autenticado que está a tentar subscrever.</param>
        /// <returns>Uma tarefa assíncrona que completa sem valor se a validação for bem-sucedida.</returns>
        /// <exception cref="System.ArgumentException">Lançada se o utilizador não pertencer à equipa ou se a equipa/utilizador não existir.</exception>
        public Task ValidateTeamMembershipAsync(Guid teamId, string? userId);
    }
}