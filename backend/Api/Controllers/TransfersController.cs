using Microsoft.AspNetCore.RateLimiting;
using Api.Seguranca;
using Application.DTOs.Competition;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>Mercado de transferências: listagens, propostas e respostas.</summary>
    [Authorize]
    [ApiController]
    [Route("api/transfers")]
    [Produces("application/json")]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferService transfers;

        public TransfersController(ITransferService transfers)
        {
            this.transfers = transfers;
        }

        private string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        /// <summary>Jogadores de outras equipas e livres, com filtros (administrador da equipa).</summary>
        [HttpGet("market/{teamId:guid}")]
        [ProducesResponseType(typeof(List<MarketPlayerDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Market(Guid teamId, [FromQuery] MarketFilterDto filter) =>
            Ok(await transfers.GetMarketAsync(UserId, teamId, filter));

        /// <summary>Coloca um jogador da equipa no mercado.</summary>
        [HttpPost("listings/{teamId:guid}/{playerId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> List(Guid teamId, string playerId)
        {
            await transfers.ListPlayerAsync(UserId, teamId, playerId);
            return NoContent();
        }

        /// <summary>Tira um jogador do mercado.</summary>
        [HttpDelete("listings/{teamId:guid}/{playerId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Unlist(Guid teamId, string playerId)
        {
            await transfers.UnlistPlayerAsync(UserId, teamId, playerId);
            return NoContent();
        }

        /// <summary>Faz uma proposta por um jogador de outra equipa.</summary>
        [EnableRateLimiting(LimitacaoPedidos.Convites)]
        [HttpPost("offers")]
        [ProducesResponseType(typeof(TransferOfferDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Offer([FromBody] CreateTransferOfferDto dto) =>
            Ok(await transfers.CreateOfferAsync(UserId, dto));

        /// <summary>Propostas recebidas (pelos jogadores da equipa) e enviadas.</summary>
        [HttpGet("offers/team/{teamId:guid}")]
        [ProducesResponseType(typeof(TeamTransferOffersDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> TeamOffers(Guid teamId) => Ok(await transfers.GetTeamOffersAsync(UserId, teamId));

        /// <summary>Propostas à espera da resposta do jogador autenticado.</summary>
        [HttpGet("offers/player")]
        [ProducesResponseType(typeof(List<TransferOfferDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PlayerOffers() => Ok(await transfers.GetPlayerOffersAsync(UserId));

        /// <summary>Aceita a proposta (clube do jogador e, depois, o jogador).</summary>
        [HttpPost("offers/{offerId:guid}/accept")]
        [ProducesResponseType(typeof(TransferOfferDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Accept(Guid offerId) => Ok(await transfers.AcceptOfferAsync(UserId, offerId));

        /// <summary>Recusa a proposta (clube ou jogador) ou retira-a (equipa que a fez).</summary>
        [HttpPost("offers/{offerId:guid}/reject")]
        [ProducesResponseType(typeof(TransferOfferDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Reject(Guid offerId) => Ok(await transfers.RejectOfferAsync(UserId, offerId));
    }
}
