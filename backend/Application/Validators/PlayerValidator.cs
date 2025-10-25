using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using System.Net.Mail;

namespace Application.Validators
{
    internal class PlayerValidator : IPlayerValidator
    {
        public void PlayerExists(Player? player)
        {
            if (player == null)
            {
                throw new NotFoundException("Player doesn't exist.");
            }
        }

        public void CreatePlayerValidator(CreatePlayerDTO createPlayerDTO, Player player)
        {
            if (player != null)
            {
                throw new ValidationException($"The email '{player.Email}' is already in use.");
            }

            if (!IsValidEmail(createPlayerDTO.Email))
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
            throw new NotImplementedException();
        }

        public void UpdatePlayerValidator(UpdatePlayerDTO updatePlayerDTO, Player player, Player playerEmail)
        {
            PlayerExists(player);

            if (updatePlayerDTO.Email != player.Email)
            {
                if (playerEmail != null)
                {
                    throw new ValidationException($"The email '{updatePlayerDTO.Email}' is already in use.");
                }
            }

            if (!IsValidEmail(updatePlayerDTO.Email))
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

        private bool IsValidEmail(string email)
        {
            var valid = true;

            try
            {
                var emailAddress = new MailAddress(email);
            }
            catch
            {
                valid = false;
            }
            
            if (!email.EndsWith(".com"))
            {
                valid = false;
            }

            return valid;
        }
    }
}
