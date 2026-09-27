using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>Consultas e alterações para os direitos dos titulares (RGPD) e a retenção de dados.</summary>
    public interface IContaRepository
    {
        Task<Player?> GetJogadorComEquipaAsync(string uid);

        /// <summary>Preenche a exportação com o que está no SQL Server (tudo menos o chat).</summary>
        Task PreencherExportacaoAsync(string uid, ExportacaoDadosDto exportacao);

        /// <summary>Apaga pedidos de adesão, propostas em aberto e a colocação no mercado do jogador.</summary>
        Task RemoverPendentesDoJogadorAsync(string uid);

        /// <summary>Ícones de equipas ainda em uso (não se apagam do Cloudinary).</summary>
        Task<List<string>> GetIconesEmUsoAsync();

        /// <summary>Jogadores sem equipa, não eliminados, criados antes da data (candidatos a limpeza).</summary>
        Task<List<string>> GetJogadoresSemEquipaCriadosAntesAsync(DateTime antesDe, int maximo);

        /// <summary>Apaga pedidos de adesão com mais de <paramref name="antesDe"/>.</summary>
        Task<int> ApagarPedidosAdesaoAntigosAsync(DateTime antesDe);

        /// <summary>Apaga convites de jogo cuja data já passou antes de <paramref name="antesDe"/>.</summary>
        Task<int> ApagarConvitesExpiradosAsync(DateTime antesDe);

        void RemoverJogador(Player jogador);
    }
}
