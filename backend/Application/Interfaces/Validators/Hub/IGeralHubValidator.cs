namespace Application.Interfaces.Validators.Hub
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio Genéricas para operações de Hub SignalR.
    /// 
    /// Esta interface define as regras de validação síncrona aplicáveis ao contexto de Hub,
    /// especialmente para a gestão de saída e desconexão de clientes.
    /// </summary>
    public interface IGeralHubValidator
    {
        /// <summary>
        /// Valida se os IDs essenciais para a operação de saída (Leave/Disconnect) não são nulos ou vazios.
        /// </summary>
        /// <remarks>
        /// Esta validação é uma verificação de segurança de baixo nível para garantir que o contexto de partida e equipa é conhecido.
        /// </remarks>
        /// <param name="idMatch">O ID da partida (contexto do grupo).</param>
        /// <param name="idTeam">O ID da equipa que está a sair.</param>
        /// <exception cref="System.ArgumentNullException">Lançada se o ID da partida ou o ID da equipa for Guid.Empty.</exception>
        public void ValidateIdMatchLeaveMatch(Guid idMatch, Guid idTeam);

        /// <summary>
        /// Valida se a operação de saída (limpeza no serviço/cache) foi bem-sucedida.
        /// </summary>
        /// <remarks>
        /// Utilizado para verificar o resultado booleano de um Manager Service que tenta remover um utilizador de um lobby ou grupo.
        /// </remarks>
        /// <param name="success">O resultado booleano da tentativa de saída do serviço.</param>
        /// <exception cref="System.InvalidOperationException">Lançada se o serviço indicar que houve uma falha ao sair do lobby (success = false).</exception>
        public void ValidateLeaveMatch(bool success);
    }
}