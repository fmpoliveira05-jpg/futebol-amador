using Application.DTOs.Filters;
using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Validators;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using System.Net.Mail;
using System.Text.RegularExpressions;
using static Domain.Constants.ModelConstants;
using Domain.Enums;

namespace Application.Validators
{
    public class PlayerValidator : IPlayerValidator
    {
        IUserDataValidator UserDatalValidator;


        public PlayerValidator(IUserDataValidator emailValidator)
        {
            this.UserDatalValidator = emailValidator;
        }
        public PlayerValidator()
        {

        }

        public void PlayerExists(Player? player)
        {
            if (player == null)
            {
                throw new NotFoundException("O Player não existe");
            }
        }

        public void CreatePlayerValidator(CreatePlayerDto CreatePlayerDto, User? phoneUser, User? emailUser)
        {
            if (emailUser != null)
            {
                throw new ValidationException($"The email '{emailUser.Email}' is already in use.");
            }

            if (phoneUser  != null)
            {
                throw new ValidationException($"The phone number '{phoneUser.Phone}' is already in use.");
            }

            
            ValidateHeigth(CreatePlayerDto.Height);

            ValidateAge(CreatePlayerDto.DateOfBirth);

            ValidatePosition(CreatePlayerDto.Position);

            ValidateAddress(CreatePlayerDto.Address);
        }

        public void DeletePlayerValidator(Player? player)
        {
            PlayerExists(player);
        }

        public void GetPlayerByIdValidator(Player? player)
        {
            PlayerExists(player);
        }

        public void UpdatePlayerValidator(UpdatePlayerDto UpdatePlayerDto, Player player, User[] players)
        {
            PlayerExists(player);

            if (UpdatePlayerDto.Email != player.Email)
            {
                if (players[0] != null)
                {
                    throw new ValidationException($"The email '{players[0].Email}' is already in use.");
                }

            }

            if (UpdatePlayerDto.Phone != player.Phone)
            {
                if (players[1] != null)
                {
                    throw new ValidationException($"The phone number '{players[1].Phone}' is already in use.");
                }
            }

            UserDatalValidator.EmailValidation(UpdatePlayerDto.Email);

            ValidateHeigth(UpdatePlayerDto.Height);

            ValidateAge(UpdatePlayerDto.DateOfBirth);

            ValidatePosition(UpdatePlayerDto.Position);

            ValidateAddress(UpdatePlayerDto.Address);

            ValidateAddress(UpdatePlayerDto.Address);
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
                if (filter.MinAge.Value > filter.MaxAge.Value)
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

        public void SendMembershipRequestValidator(Player player, Team team, MembershipRequest request)
        {
            PlayerExists(player);

            if (team == null)
            {
                throw new NotFoundException("Team doesn't exist.");
            }

            if (player.Team != null)
            {
                throw new BusinessRuleException("Player is already on a team and can't send membership requests.");
            }

            if (team.Members.Count >= TeamConst.MaxMembers)
            {
                throw new BusinessRuleException("Team is full and cannot accept new requests.");
            }

            if (request != null)
            {
                throw new BusinessRuleException("Já existe um pedido de adesão pendente para esta equipa.");
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
        public void PlayerHasChatRoomsValidation(Player player) {
            PlayerExists(player);
            
            
        }

        private void PlayerHasChatRooms(Player player) { 
            //if(player.)
        }

        private static void ValidateHeigth(int heigth)
        {
            if (heigth < ModelConstants.PlayerConst.MinHeight || heigth > ModelConstants.PlayerConst.MaxHeight)
            {
                throw new ValidationException("Height value is invalid");
            }
        }

        private static void ValidateEmail(string email)
        {
            if (!IsValidEmail(email))
            {
                throw new ValidationException($"Email format is invalid ({email}).");
            }
        }

        private static void ValidateAge(DateOnly dateOfBirth)
        {
            if (dateOfBirth > DateOnly.FromDateTime(DateTime.Now).AddYears(-ModelConstants.UserConst.MinAge)
                || dateOfBirth < DateOnly.FromDateTime(DateTime.Now).AddYears(-ModelConstants.UserConst.MaxAge))
            {
                throw new ValidationException("Invalid Date of birth");
            }
        }

        private static void ValidatePosition(Position position)
        {
            if (!Enum.IsDefined(typeof(Position), position))
            {
                throw new ValidationException("Position invalid.");
            }
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