using Application.Interfaces.Validators.Hub;

namespace Application.Validators.Hubs
{
    /// <summary>
    /// Validador de Regras de Negócio Genéricas para operações comuns a todos os Hubs SignalR.
    /// 
    /// Responsável por verificar a validade dos IDs de contexto e o sucesso das operações de saída.
    /// </summary>
    public class GeralHubValidator : IGeralHubValidator
    {
        /// <summary>
        /// Valida se os IDs essenciais para a operação de saída (Leave/Disconnect) não são nulos ou vazios.
        /// </summary>
        /// <param name="idMatch">O ID da partida (contexto do grupo) a ser validado.</param>
        /// <param name="idTeam">O ID da equipa que está a sair.</param>
        /// <exception cref="ArgumentNullException">Lançada se o ID da partida ou o ID da equipa for Guid.Empty.</exception>
        public void ValidateIdMatchLeaveMatch(Guid idMatch, Guid idTeam)
        {
            if (idMatch == Guid.Empty)
            {
                throw new ArgumentNullException("O id da match está vazio");
            }

            if (idTeam == Guid.Empty)
            {
                throw new ArgumentNullException("O id da equipa está vazio");
            }
        }

        /// <summary>
        /// Valida se a operação de saída (limpeza no serviço/cache) foi bem-sucedida.
        /// </summary>
        /// <param name="success">O resultado booleano da tentativa de saída do serviço.</param>
        /// <exception cref="InvalidOperationException">Lançada se o serviço indicar que houve uma falha ao sair do lobby (success = false).</exception>
        public void ValidateLeaveMatch(bool success)
        {
            if (!success)
            {
                throw new InvalidOperationException("Surguiu um problema a sair do lobby");
            }
        }
    }
}