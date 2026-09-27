using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>Pedido de eliminação da conta: exige a palavra-passe atual (reautenticação).</summary>
    public class EliminarContaDto
    {
        [Required]
        [MaxLength(128)]
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// Cópia dos dados pessoais do titular (RGPD, direito de acesso — art. 15.º — e portabilidade —
    /// art. 20.º), em JSON.
    /// </summary>
    public class ExportacaoDadosDto
    {
        public DateTime GeradoEm { get; set; }
        public string Formato { get; set; } = "futebol-amador/exportacao-v1";
        public ContaExportadaDto Conta { get; set; } = null!;
        public EquipaExportadaDto? Equipa { get; set; }
        public List<PedidoAdesaoExportadoDto> PedidosDeAdesao { get; set; } = new();
        public List<TransferenciaExportadaDto> Transferencias { get; set; } = new();
        public List<PropostaExportadaDto> PropostasDeTransferencia { get; set; } = new();
        public List<EventoJogoExportadoDto> EventosDeJogo { get; set; } = new();
        public int JogosNoOnze { get; set; }
        public List<SalaChatExportadaDto> SalasDeChat { get; set; } = new();
        public List<MensagemExportadaDto> MensagensEnviadas { get; set; } = new();

        /// <summary>Notas sobre dados que não foi possível incluir (por exemplo, chat indisponível).</summary>
        public List<string> Notas { get; set; } = new();
    }

    public class ContaExportadaDto
    {
        public string Id { get; set; } = null!;
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telefone { get; set; } = null!;
        public string Morada { get; set; } = null!;
        public DateOnly DataNascimento { get; set; }
        public DateTime CriadaEm { get; set; }
        public string Posicao { get; set; } = null!;
        public int Altura { get; set; }
        public int? Peso { get; set; }
        public string? PePreferido { get; set; }
        public string? Nacionalidade { get; set; }
        public string? PaisNascimento { get; set; }
        public string? Imagem { get; set; }
        public string Situacao { get; set; } = null!;
        public bool AdministradorDeEquipa { get; set; }
        public bool NotificacoesPushRegistadas { get; set; }
        public string? PoliticaPrivacidadeVersao { get; set; }
        public DateTime? PoliticaPrivacidadeAceiteEm { get; set; }
    }

    public class EquipaExportadaDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = null!;
        public DateTime? DesdeEm { get; set; }
    }

    public class PedidoAdesaoExportadoDto
    {
        public string Equipa { get; set; } = null!;
        public DateTime Data { get; set; }
        public bool EnviadoPeloJogador { get; set; }
    }

    public class TransferenciaExportadaDto
    {
        public string Tipo { get; set; } = null!;
        public string? DeEquipa { get; set; }
        public string? ParaEquipa { get; set; }
        public DateTime Data { get; set; }
    }

    public class PropostaExportadaDto
    {
        public Guid Id { get; set; }
        public string Estado { get; set; } = null!;
        public string? Mensagem { get; set; }
        public DateTime CriadaEm { get; set; }
    }

    public class EventoJogoExportadoDto
    {
        public Guid Jogo { get; set; }
        public string Tipo { get; set; } = null!;
        public int? Minuto { get; set; }
    }

    public class SalaChatExportadaDto
    {
        public string Id { get; set; } = null!;
        public string Nome { get; set; } = null!;
    }

    public class MensagemExportadaDto
    {
        public string Sala { get; set; } = null!;
        public string Texto { get; set; } = null!;
        public DateTime? EnviadaEm { get; set; }
    }
}
