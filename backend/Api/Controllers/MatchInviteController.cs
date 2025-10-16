using Application.DTOs;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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
         * Meter Try Catchs (Adds, Removes)
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

            //Validações de existencia das teams na BD
            Teams? Sender = context.Team.FirstOrDefault(t => t.Id == idTeam);

            if (Sender == null) { 
                return NotFound("A equipa que enviou o convite não foi encontrada");
            }

            Teams? Receiver = context.Team.FirstOrDefault(t => t.Id == dto.IdReceiver);

            if (Receiver == null)
            {
                return NotFound("A equipa que recebeu o convite não foi encontrada");
            }

            //Regra de negocio
            if (Sender.IdPitch != dto.IdPitch && Receiver.IdPitch != dto.IdPitch)
            {
                return BadRequest("O campo da partida não pertence a nenhuma das equipas");
            }

            //Regra de negocio
            if ((dto.GameDate - DateTime.UtcNow).TotalHours < 12) {
                return BadRequest("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
            }

            MatchInvite matchInvite = new MatchInvite(Sender, Receiver, dto.GameDate, dto.Pitch);
            try {                
                context.MatchInvite.Add(matchInvite);
                Receiver.AddReceiveMatchInvite(matchInvite);
                Sender.AddSendMatchInvite(matchInvite);
                context.SaveChanges();
            } catch (ArgumentNullException ex) { 
                return BadRequest(new { message = ex.Message });
            } catch (MatchInviteException ex) {
                return Conflict(new { message = ex.Message });
            } catch (DbUpdateException ex) {
                return StatusCode(500, new { message = "Erro ao guardar o convite na base de dados.", details = ex.Message });
            }
            
            return NoContent(); // Depois trocar para o Match Invite criado
        }

        /***
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
            try {
                context.MatchInvite.Remove(matchInvite);
                opponentTeam.removeSendMatchInvite(matchInvite);
                team.removeReceiverMatchInvite(matchInvite);
                context.SaveChanges();
            } catch (ArgumentNullException ex) {
                return BadRequest(new { message = ex.Message });
            } catch (MatchInviteException ex) {
                return Conflict(new { message = ex.Message });
            } catch (DbUpdateException ex) {
                return StatusCode(500, new { message = "Erro ao guardar o convite na base de dados.", details = ex.Message });
            }

            return NoContent();
        }
    }
}
