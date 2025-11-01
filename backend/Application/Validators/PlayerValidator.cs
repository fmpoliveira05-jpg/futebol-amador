using Application.DTOs.PlayerDTOs;
using Application.DTOs.Filters;
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
        public void PlayerExists(Player? player)
        {
            if (player == null)
            {
                throw new NotFoundException("Player doesn't exist.");
            }
        }

        public void CreatePlayerValidator(CreatePlayerDTO createPlayerDTO, Player[] players)
        {
            if (players[0] != null)
            {
                throw new ValidationException($"The email '{players[0].Email}' is already in use.");
            }

            if (players[1] != null)
            {
                throw new ValidationException($"The phone number '{players[1].Phone}' is already in use.");
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
        public void ValidateFiltersListTeams(FilterListTeamDto filter)
        {
            if (filter.MinNumberPoints.HasValue && filter.MaxNumberPoints.HasValue)
            {
                if (filter.MinNumberPoints > filter.MaxNumberPoints)
                {
                    throw new InvalidOperationException("O numero minimo de pontos de uma equipa, não deve ser superior ao numero maximo");
                }
            }

            if (filter.MinAge.HasValue && filter.MaxAge.HasValue)
            {
                if(filter.MinAge.Value > filter.MaxAge.Value)
                {
                    throw new InvalidOperationException("O numero minimo de idade minima tem de ser inferior à idade media maxima");
                }
            }

            if (filter.MinNumberPlayers.HasValue && filter.MaxNumberPlayers.HasValue)
            {
                if (filter.MinNumberPlayers.Value > filter.MaxNumberPlayers.Value)
                {
                    throw new InvalidOperationException("O número minimo de membros deve ser superior ao numero maximo de membros");
                }
            }
        }
    }
}
