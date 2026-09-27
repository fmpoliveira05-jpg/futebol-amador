using Application.DTOs;

namespace Application.Interfaces.Services
{
    /// <summary>Direitos dos titulares (RGPD): acesso/portabilidade e apagamento.</summary>
    public interface IContaService
    {
        /// <summary>Versão atual da Política de Privacidade (<c>Rgpd:VersaoPolitica</c>).</summary>
        string VersaoPoliticaAtual { get; }

        /// <summary>Exporta todos os dados pessoais do titular (art. 15.º e 20.º).</summary>
        Task<ExportacaoDadosDto> ExportarAsync(string uid);

        /// <summary>
        /// Elimina a conta (art. 17.º): confirma a palavra-passe, sai da equipa (passando a
        /// administração, ou apagando a equipa se era o único membro), anonimiza os dados pessoais,
        /// apaga as mensagens do chat, as imagens no Cloudinary e a conta no Firebase.
        /// </summary>
        Task EliminarContaAsync(string uid, string palavraPasse);

        /// <summary>
        /// Limpeza periódica (retenção): contas por confirmar há mais de
        /// <c>Rgpd:DiasContaPorConfirmar</c>, pedidos de adesão e convites antigos.
        /// </summary>
        Task<(int Contas, int Pedidos, int Convites)> AplicarRetencaoAsync(DateTime agora);
    }

    /// <summary>Apaga as imagens de um utilizador no serviço de alojamento (Cloudinary).</summary>
    public interface IImagensUtilizador
    {
        /// <summary>Apaga as imagens da pasta do utilizador, exceto as indicadas (ainda em uso).</summary>
        Task ApagarDoUtilizadorAsync(string uid, IReadOnlyCollection<string> emUso, CancellationToken cancelamento = default);
    }
}
