using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchInviteController : ControllerBase
    {
        private readonly AmateurFootballContext context;

        public MatchInviteController(AmateurFootballContext amatuerFutebolContext) => this.context = amatuerFutebolContext;

        /***
         * Vai faltar as questões de aut neste metodo e nos restantes
         */
        [HttpPost("teams/{idTeam:guid}/match-invites/")]
        public IActionResult SendMatchInvite(Guid idTeam, [FromBody] SendMatchInviteDTO dto)
        {
            if (idTeam == Guid.Empty) {
                return BadRequest("O id da equipa está vazio");
            }

            if (dto.IdSender != idTeam) {
                return BadRequest("O id da equipa do DTO não bate com a do url");
            }

            //Regra de negocio
            if (dto.IdSender == dto.IdReceiver || dto.Sender.Equals(dto.Receiver)) {
                return BadRequest("O recetor do convite deve ser diferente do emissor!");
            }

            //Depois trocar para o Match Invite Criado
            return NoContent();
        }

        /**
         * Talvez eliminar o counter da variavel
         * Falta definir url
         * No Action Resukt vai estar o DTO/Resultado retornado ao user
         * Meter Try Catchm, por causa dos removes
         * 
         * Receberá o futuro o User para aut
         */
        //DELETE
        // api/.../RefuseMatchInvite/id_invite
        [HttpDelete("{idTeam:guid}/RefuseMatchInvite/{idMatchInvite:guid}")]
        public IActionResult RefuseMatchInvite(Guid idTeam, Guid idMatchInvite) {
            if (idTeam == Guid.Empty){
                return BadRequest("O id da equipa não pode estar vazio");
            }

            if (idMatchInvite == Guid.Empty) {
                return BadRequest("O id da partida não pode estar vazio");
            }

            var team = context.Team.FirstOrDefault(t => t.Id == idTeam);

            if (team == null){
                return NotFound("A equipa não foi encontrada");
            }

            var numReceivedInvites = team.ReceivedInvites.Count();
            if (numReceivedInvites == 0) {
                return Conflict("Não é possível recusar um convite porque não existem convites recebidos.");
            }

            var receivedInvitesList = team.ReceivedInvites;
            MatchInvite? matchInvite = receivedInvitesList.FirstOrDefault(i => i.Id == idMatchInvite);

            if (matchInvite == null) {
                throw new MatchInviteException("O convite a recusar não existe");
            }

            Teams opponentTeam = matchInvite.Sender;
            if (opponentTeam.SentInvites.FirstOrDefault(matchInvite) == null) {
                throw new MatchInviteException("O convite a recusar não existe na equipa oponetne");
            }

            //Removes
            //opponentTeam.SentInvites.Remove(matchInvite);
            //opponentTeam.CountSendInvites--;
            opponentTeam.removeSendMatchInvite(matchInvite);

            //team.ReceivedInvites.Remove(matchInvite);
            //team.CountReceivedIntes--;
            team.removeSendMatchInvite(matchInvite);

            context.MatchInvite.Remove(matchInvite);
            context.SaveChanges();
            return NoContent();
        }
    }
}
