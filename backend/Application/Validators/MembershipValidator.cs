using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;
using System.Numerics;

namespace Application.Validators
{
    public class MembershipValidator : IMembershipValidator
    {
        public void ValidateSendRequestByPlayer(Player player, Team team, MembershipRequest? existingRequest)
        {
            if (player == null)
            {
                throw new ValidationException("O jogador não existe.");
            }

            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (player.IdTeam != null)
            {
                throw new ValidationException("O jogador já pertence a uma equipa.");
            }

            if (existingRequest != null)
            {
                throw new ValidationException("Já existe um pedido de adesão entre este jogador e esta equipa.");
            }
        }

        public void ValidateSendRequestByTeam(Team team, Player invitedPlayer, MembershipRequest? existingRequest)
        {
            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (invitedPlayer == null)
            {
                throw new ValidationException("O jogador a convidar não existe.");
            }

            /*
            if (invitedPlayer.IdTeam != null)
            {
                throw new ValidationException("O jogador já pertence a uma equipa.");
            }
            */

            if (existingRequest != null)
            {
                throw new ValidationException("Já existe um pedido de adesão entre esta equipa e este jogador.");
            }
        }

        public void ValidateAcceptRequestByTeam(Team team, MembershipRequest request, Player playerAccepting)
        {
            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (request == null)
            {
                throw new ValidationException("O pedido de adesão não existe.");
            }

            if (!playerAccepting.IsAdmin)
            {
                throw new ValidationException("O jogador que tenta aceitar o pedido não é administrador da equipa.");
            }

            if (team.Members.Any(m => m.Id == request.IdPlayer))
            {
                throw new ValidationException("O jogador já é membro desta equipa.");
            }
        }

        public void ValidateRejectRequestByTeam(Team team, MembershipRequest request, Player player)
        {
            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (!player.IsAdmin)
            {
                throw new ValidationException("O jogador não é administrador da equipa.");
            }

            if (request == null)
            {
                throw new ValidationException("O pedido de adesão não existe.");
            }
        }

        public void ValidateGetRequestsByTeam(Team team, Player player)
        {
            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (player.Team != team)
            {
                throw new ValidationException("O jogador não pertence à equipa.");
            }
            
            if (!player.IsAdmin)
            {
                throw new ValidationException("O jogador não é administrador da equipa.");
            }
        }

        public void ValidateAcceptRequestByPlayer(Player player, MembershipRequest request, Team team)
        {
            if (player == null)
            {
                throw new ValidationException("O jogador não existe.");
            }

            if (request == null)
            {
                throw new ValidationException("O pedido de adesão não existe.");
            }

            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (player.IdTeam != null)
            {
                throw new ValidationException("O jogador já pertence a uma equipa.");
            }

            if (request.IdPlayer != player.Id)
            {
                throw new ValidationException($"O jogador não possui um pedido de adesão com Id '{request.Id}' ou pertence a outra equipa.");
            }
        }

        public void ValidateRejectRequestByPlayer(MembershipRequest request, Player player)
        {
            if (request == null)
            {
                throw new ValidationException("O pedido de adesão não existe.");
            }

            if (request.IdPlayer != player.Id)
            {
                throw new ValidationException($"O jogador não possui um pedido de adesão com Id '{request.Id}' ou pertence a outra equipa.");
            }
        }
    }
}