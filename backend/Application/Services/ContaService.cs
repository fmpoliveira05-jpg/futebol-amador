using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    /// <summary>
    /// Direitos dos titulares dos dados (RGPD): exportação (acesso e portabilidade), eliminação da
    /// conta (apagamento) e limpeza periódica (limitação da conservação). Ver docs/RGPD.md.
    /// </summary>
    public class ContaService : IContaService
    {
        private readonly IContaRepository contas;
        private readonly IPlayerService jogadores;
        private readonly IAuthService auth;
        private readonly IChatRoomService chat;
        private readonly IImagensUtilizador imagens;
        private readonly IUnityOfWork unidade;
        private readonly TimeProvider relogio;
        private readonly ILogger<ContaService> logger;
        private readonly int diasContaPorConfirmar;
        private readonly int diasPedidosAdesao;
        private readonly int diasConvitesExpirados;

        public ContaService(IContaRepository contas, IPlayerService jogadores, IAuthService auth, IChatRoomService chat,
            IImagensUtilizador imagens, IUnityOfWork unidade, IConfiguration configuracao, TimeProvider relogio,
            ILogger<ContaService> logger)
        {
            this.contas = contas;
            this.jogadores = jogadores;
            this.auth = auth;
            this.chat = chat;
            this.imagens = imagens;
            this.unidade = unidade;
            this.relogio = relogio;
            this.logger = logger;
            VersaoPoliticaAtual = configuracao["Rgpd:VersaoPolitica"] ?? VersaoPoliticaPredefinida;
            diasContaPorConfirmar = configuracao.GetValue("Rgpd:DiasContaPorConfirmar", 30);
            diasPedidosAdesao = configuracao.GetValue("Rgpd:DiasPedidosAdesao", 90);
            diasConvitesExpirados = configuracao.GetValue("Rgpd:DiasConvitesExpirados", 30);
        }

        /// <summary>Versão da política publicada na web e na app (mudar nos três sítios ao mesmo tempo).</summary>
        public const string VersaoPoliticaPredefinida = "2026-09-27";

        public string VersaoPoliticaAtual { get; }

        public async Task<ExportacaoDadosDto> ExportarAsync(string uid)
        {
            var jogador = await contas.GetJogadorComEquipaAsync(uid);
            if (jogador == null || jogador.EliminadoEm != null)
            {
                throw new NotFoundException("Conta não encontrada.");
            }

            var exportacao = new ExportacaoDadosDto
            {
                GeradoEm = relogio.GetUtcNow().UtcDateTime,
                Conta = new ContaExportadaDto
                {
                    Id = jogador.Id,
                    Nome = jogador.Name,
                    Email = jogador.Email,
                    Telefone = jogador.Phone,
                    Morada = jogador.Address,
                    DataNascimento = jogador.DateOfBirth,
                    CriadaEm = jogador.CreationDate,
                    Posicao = jogador.Position.ToString(),
                    Altura = jogador.Height,
                    Peso = jogador.Weight,
                    PePreferido = jogador.PreferredFoot?.ToString(),
                    Nacionalidade = jogador.Nationality,
                    PaisNascimento = jogador.CountryOfBirth,
                    Imagem = jogador.ImageUrl,
                    Situacao = jogador.Status.ToString(),
                    AdministradorDeEquipa = jogador.IsAdmin,
                    NotificacoesPushRegistadas = !string.IsNullOrEmpty(jogador.DeviceToken),
                    PoliticaPrivacidadeVersao = jogador.PoliticaPrivacidadeVersao,
                    PoliticaPrivacidadeAceiteEm = jogador.PoliticaPrivacidadeAceiteEm,
                },
                Equipa = jogador.Team == null ? null : new EquipaExportadaDto
                {
                    Id = jogador.Team.Id,
                    Nome = jogador.Team.Name,
                    DesdeEm = jogador.JoinedTeamAt,
                },
            };

            await contas.PreencherExportacaoAsync(uid, exportacao);

            try
            {
                var salas = await chat.GetMyRoomsAsync(uid) ?? new();
                exportacao.SalasDeChat = salas.Select(s => new SalaChatExportadaDto { Id = s.RoomId, Nome = s.RoomName }).ToList();
                exportacao.MensagensEnviadas = await chat.ExportarMensagensAsync(uid) ?? new();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning("Exportação sem o chat ({Tipo}).", ex.GetType().Name);
                exportacao.Notas.Add("Não foi possível obter as mensagens do chat neste momento. Pede-as pelo contacto da Política de Privacidade.");
            }

            return exportacao;
        }

        public async Task EliminarContaAsync(string uid, string palavraPasse)
        {
            var jogador = await contas.GetJogadorComEquipaAsync(uid);
            if (jogador == null || jogador.EliminadoEm != null)
            {
                throw new NotFoundException("Conta não encontrada.");
            }

            // Reautenticação: uma sessão roubada não chega para apagar a conta.
            await auth.ConfirmarPalavraPasseAsync(uid, palavraPasse);

            // Sair da equipa com as regras normais: passa a administração (e o estatuto de
            // administrador principal) ao membro seguinte, ou apaga a equipa se era o único membro.
            if (jogador.IdTeam != null)
            {
                await jogadores.LeaveTeam(uid);
                jogador = await contas.GetJogadorComEquipaAsync(uid) ?? throw new NotFoundException("Conta não encontrada.");
            }

            await contas.RemoverPendentesDoJogadorAsync(uid);

            // Anonimização: a linha fica para as estatísticas e relatórios dos jogos já disputados
            // (sem ninguém identificável); os dados pessoais desaparecem.
            jogador.Name = ModelConstants.RgpdConst.NomeAnonimo;
            jogador.Email = $"removido-{Guid.NewGuid():N}@anonimo.invalid";
            jogador.Phone = "+000000000000";
            jogador.Address = "Removido";
            jogador.DateOfBirth = new DateOnly(1900, 1, 1);
            jogador.ImageUrl = null;
            jogador.Weight = null;
            jogador.PreferredFoot = null;
            jogador.Nationality = null;
            jogador.CountryOfBirth = null;
            jogador.DeviceToken = null;
            jogador.IsAdmin = false;
            jogador.PoliticaPrivacidadeVersao = null;
            jogador.PoliticaPrivacidadeAceiteEm = null;
            jogador.EliminadoEm = relogio.GetUtcNow().UtcDateTime;
            await unidade.SaveChangesAsync();

            // Serviços externos: uma falha fica no log para seguimento manual (docs/RGPD.md), mas não
            // impede o resto — os dados na base de dados já foram anonimizados.
            try
            {
                await chat.EliminarDadosUtilizadorAsync(uid);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError("Eliminação da conta: falhou a limpeza do chat ({Tipo}). Repetir manualmente.", ex.GetType().Name);
            }

            try
            {
                await imagens.ApagarDoUtilizadorAsync(uid, await contas.GetIconesEmUsoAsync());
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError("Eliminação da conta: falhou a limpeza das imagens ({Tipo}). Repetir manualmente.", ex.GetType().Name);
            }

            // Por último: sem a conta no Firebase já não há sessão possível.
            await auth.DeleteUserAsync(uid);
        }

        public async Task<(int Contas, int Pedidos, int Convites)> AplicarRetencaoAsync(DateTime agora)
        {
            var apagadas = 0;
            var candidatos = await contas.GetJogadoresSemEquipaCriadosAntesAsync(agora.AddDays(-diasContaPorConfirmar), 500);
            if (candidatos.Count > 0)
            {
                var porConfirmar = await auth.ContasPorConfirmarAsync(candidatos);
                foreach (var uid in porConfirmar)
                {
                    var jogador = await contas.GetJogadorComEquipaAsync(uid);
                    if (jogador == null || jogador.IdTeam != null)
                    {
                        continue;
                    }

                    await contas.RemoverPendentesDoJogadorAsync(uid);
                    contas.RemoverJogador(jogador);
                    await unidade.SaveChangesAsync();

                    try
                    {
                        await auth.DeleteUserAsync(uid);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        // Já pode não existir no Firebase.
                        logger.LogInformation("Retenção: conta por confirmar sem utilizador no Firebase ({Tipo}).", ex.GetType().Name);
                    }
                    apagadas++;
                }
            }

            var pedidos = await contas.ApagarPedidosAdesaoAntigosAsync(agora.AddDays(-diasPedidosAdesao));
            var convites = await contas.ApagarConvitesExpiradosAsync(agora.AddDays(-diasConvitesExpirados));

            return (apagadas, pedidos, convites);
        }
    }
}
