using Application.DTOs.Filters;
using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Application.Validators
{
    public class PlayerValidator : IPlayerValidator
    {
        public void PlayerExists(Player? player)
        {
            if (player == null)
            {
                throw new NotFoundException("Player doesn't exist.");
            }
        }

        public void CreatePlayerValidator(CreatePlayerDto createPlayerDTO, Player[] players)
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
                throw new ValidationException($"Email format is invalid ({createPlayerDTO.Email}).");
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

            if (!createPlayerDTO.Phone.All(char.IsDigit))
            {
                throw new ValidationException("Phone number must only contain digits (0-9).");
            }

            if (createPlayerDTO.Phone.StartsWith("0"))
            {
                throw new ValidationException("Phone number cannot start with '0'.");
            }

            if (!int.TryParse(createPlayerDTO.Phone, out _))
            {
                throw new ValidationException("Phone number must only have numbers.");
            }

            if (!Enum.IsDefined(typeof(Position), createPlayerDTO.Position))
            {
                throw new ValidationException("Position invalid.");
            }

            ValidateAddress(createPlayerDTO.Address);
        }

        public void DeletePlayerValidator(Player? player)
        {
            PlayerExists(player);
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

            if (!updatePlayerDTO.Phone.All(char.IsDigit))
            {
                throw new ValidationException("Phone number must only contain digits (0-9).");
            }

            if (updatePlayerDTO.Phone.StartsWith("0"))
            {
                throw new ValidationException("Phone number cannot start with '0'.");
            }

            if (!int.TryParse(updatePlayerDTO.Phone, out _))
            {
                throw new ValidationException("Phone number must only have numbers.");
            }

            if (!Enum.IsDefined(typeof(Position), updatePlayerDTO.Position))
            {
                throw new ValidationException("Position invalid.");
            }

            ValidateAddress(updatePlayerDTO.Address);
        }

        public void ValidateHasChangeDataPlayer(bool hasChange)
        {
            if (!hasChange)
            {
                throw new ValidationException("Não foi atualizado nenhuma informação do utilizador.");
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

        #region Private Validations
        private static bool IsValidEmail(string email)
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

        private static void ValidateAddress(string addressTeam)
        {
            if (string.IsNullOrWhiteSpace(addressTeam))
            {
                throw new ValidationException("O endereço não pode estar vazio.");
            }

            var pattern = new Regex(@",\s(?<city>.+)$", RegexOptions.Compiled);

            var match = pattern.Match(addressTeam);

            if (!match.Success)
            {
                throw new ValidationException($"Formato de endereço inválido. O endereço deve terminar com ', [NomeDaCidade]'. (Ex: 'Rua x, Lisboa')");
            }

        }
        #endregion
    }
}
