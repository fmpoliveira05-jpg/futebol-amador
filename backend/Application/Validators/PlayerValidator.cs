using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using System.Net.Mail;
using System.Numerics;

namespace Application.Validators
{
    internal class PlayerValidator : IPlayerValidator
    {
        IEmailValidator emailValidator;

        public PlayerValidator()
        {

        }
        public PlayerValidator(IEmailValidator emailValidator)
        {
            this.emailValidator = emailValidator;
        }

        public void PlayerExists(Player? player)
        {
            if (player == null)
            {
                throw new NotFoundException("Player doesn't exist.");
            }
        }

        public void CreatePlayerValidator(CreatePlayerDTO createPlayerDTO, Users[] players)
        {
            if (players[0] != null)
            {
                throw new ValidationException($"The email '{players[0].Email}' is already in use.");
            }

            if (players[1] != null)
            {
                throw new ValidationException($"The phone number '{players[1].Phone}' is already in use.");
            }

            if (!emailValidator.IsValid(createPlayerDTO.Email))
            {
                throw new ValidationException($"Email format is invalid.");
            }

            if (createPlayerDTO.Height < 100
                || createPlayerDTO.Height > 250)
            {
                throw new ValidationException("Height value is invalid");
            }

            if (createPlayerDTO.DateOfBirth > DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
                || createPlayerDTO.DateOfBirth < DateOnly.FromDateTime(DateTime.Now).AddYears(-70))
            {
                throw new ValidationException("Invalid Date of birth");
            }

            if (createPlayerDTO.Phone.Length != 9)
            {
                throw new ValidationException("Phone number must have 9 digits.");
            }

            if (!int.TryParse(createPlayerDTO.Phone, out _))
            {
                throw new ValidationException("Phone number must only have numbers.");
            }

            if (!Enum.IsDefined(typeof(Position), createPlayerDTO.Position))
            {
                throw new ValidationException("Position invalid.");
            }
        }

        public void DeletePlayerValidator(Player? player)
        {
            PlayerExists(player);
            //check if admin? if only player on team and have matches?
        }

        public void GetPlayerByIdValidator(Player player)
        {
            PlayerExists(player);
        }

        public void UpdatePlayerValidator(UpdatePlayerDTO updatePlayerDTO, Player player, Users[] players)
        {
            PlayerExists(player);

            if (updatePlayerDTO.Email != player.Email)
            {
                if (players[0] != null)
                {
                    throw new ValidationException($"The email '{players[0].Email}' is already in use.");
                }

            }

            if (updatePlayerDTO.Phone != player.Phone)
            {
                if (players[1] != null)
                {
                    throw new ValidationException($"The phone number '{players[1].Phone}' is already in use.");
                }
            }

            if (!emailValidator.IsValid(updatePlayerDTO.Email))
            {
                throw new ValidationException($"Email format is invalid.");
            }

            if (updatePlayerDTO.Height < 100
                || updatePlayerDTO.Height > 250)
            {
                throw new ValidationException("Height value is invalid");
            }

            if (updatePlayerDTO.DateOfBirth > DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
                || updatePlayerDTO.DateOfBirth < DateOnly.FromDateTime(DateTime.Now).AddYears(-70))
            {
                throw new ValidationException("Invalid Date of birth");
            }

            if (updatePlayerDTO.Phone.Length != 9)
            {
                throw new ValidationException("Phone number must have 9 digits.");
            }

            if (!int.TryParse(updatePlayerDTO.Phone, out _))
            {
                throw new ValidationException("Phone number must only have numbers.");
            }

            if (!Enum.IsDefined(typeof(Position), updatePlayerDTO.Position))
            {
                throw new ValidationException("Position invalid.");
            }
        }

        public void LeaveTeamValidator(Player player)
        {
            //same as the delete one
            if (player.Team == null)
            {
                throw new ValidationException("Player does not belong to any team.");
            }

            PlayerExists(player);
        }
    }
}
