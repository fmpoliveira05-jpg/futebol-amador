using Application.DTOs.Filters;
using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Validators;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using System.Text.RegularExpressions;
using static Domain.Constants.ModelConstants;
using Domain.Enums;

namespace Application.Validators
{
    /// <summary>
    /// Validador de Regras de Negócio e de Entidade para operações relacionadas com [Player].
    /// 
    /// Esta classe verifica o formato, os limites de valores (ex: altura, idade) e as regras de negócio
    /// (ex: se o jogador pertence a uma equipa) antes de executar operações na base de dados.
    /// </summary>
    public class PlayerValidator : IPlayerValidator
    {
        /// <summary>
        /// Validador delegado para operações de validação de formato de baixo nível.
        /// </summary>
        private readonly IUserDataValidator UserDatalValidator;

        /// <summary>
        /// Construtor da classe [PlayerValidator] que aceita um validador delegado.
        /// </summary>
        /// <param name="emailValidator">O validador delegado de dados de utilizador, injetado via Dependency Injection.</param>
        public PlayerValidator(IUserDataValidator emailValidator)
        {
            this.UserDatalValidator = emailValidator;
        }

        /// <summary>
        /// Construtor padrão da classe [PlayerValidator].
        /// </summary>
        public PlayerValidator()
        {
        }

        /// <summary>
        /// Valida se a entidade [Player] existe.
        /// </summary>
        /// <param name="player">A entidade Player a ser verificada.</param>
        /// <exception cref="NotFoundException">Lançada se a entidade Player for nula.</exception>
        public void PlayerExists(Player? player)
        {
            if (player == null)
            {
                throw new NotFoundException("O Player não existe");
            }
        }

        /// <summary>
        /// Valida todos os campos de um novo perfil de jogador antes da criação.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Unicidade de Email/Telefone, Limites de Altura e Idade, e Formato da Posição.
        /// </remarks>
        /// <param name="CreatePlayerDto">O DTO com os dados do novo jogador.</param>
        /// <param name="phoneUser">O utilizador existente com o mesmo telefone (se houver).</param>
        /// <param name="emailUser">O utilizador existente com o mesmo email (se houver).</param>
        /// <exception cref="ValidationException">Se a altura, idade, posição, email ou telefone forem inválidos.</exception>
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

        /// <summary>
        /// Valida se a entidade [Player] existe antes de ser eliminada.
        /// </summary>
        /// <param name="player">A entidade Player a ser eliminada.</param>
        public void DeletePlayerValidator(Player? player)
        {
            PlayerExists(player);
        }

        /// <summary>
        /// Valida a existência de um jogador antes da consulta de detalhes.
        /// </summary>
        /// <param name="player">A entidade Player a ser consultada.</param>
        public void GetPlayerByIdValidator(Player? player)
        {
            PlayerExists(player);
        }

        /// <summary>
        /// Valida todos os campos de um perfil de jogador antes da atualização.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Unicidade de Email/Telefone (apenas se forem alterados), Altura, Idade, Posição e Endereço.
        /// </remarks>
        /// <param name="UpdatePlayerDto">O DTO com os novos dados.</param>
        /// <param name="player">A entidade [Player] existente.</param>
        /// <param name="players">Utilizadores existentes com o novo Email/Telefone (para verificação de unicidade).</param>
        /// <exception cref="ValidationException">Se houver violação de unicidade ou formato.</exception>
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
        }

        /// <summary>
        /// Valida se houve alguma alteração de dados num objeto de atualização.
        /// </summary>
        /// <param name="hasChange">Booleano que indica se a comparação de DTOs detetou mudanças.</param>
        /// <exception cref="ValidationException">Lançada se o DTO não tiver alterado nenhum valor.</exception>
        public void ValidateHasChangeDataPlayer(bool hasChange)
        {
            if (!hasChange)
            {
                throw new ValidationException("Não foi atualizado nenhuma informação do utilizador.");
            }
        }

        /// <summary>
        /// Valida se o jogador pode sair da equipa.
        /// </summary>
        /// <param name="player">A entidade [Player] que está a tentar sair.</param>
        /// <exception cref="ValidationException">Lançada se o jogador não pertencer a nenhuma equipa.</exception>
        public void LeaveTeamValidator(Player player)
        {
            if (player.Team == null && player.IdTeam == null)
            {
                throw new ValidationException("Player does not belong to any team.");
            }

            PlayerExists(player);
        }

        /// <summary>
        /// Valida os filtros de listagem de equipas (Marketplace/Pesquisa).
        /// </summary>
        /// <param name="filter">O DTO de filtros.</param>
        /// <exception cref="InvalidOperationException">Lançada se os intervalos de pontos, idade ou jogadores forem inconsistentes.</exception>
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

        /// <summary>
        /// Valida o pedido de adesão/convite de recrutamento.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Existência do jogador/equipa e ausência de pedidos pendentes.
        /// </remarks>
        /// <param name="player">O jogador.</param>
        /// <param name="team">A equipa.</param>
        /// <param name="request">O pedido pendente (se existir).</param>
        /// <exception cref="NotFoundException">Se a equipa não existir.</exception>
        /// <exception cref="BusinessRuleException">Se o jogador já tiver equipa ou já houver um pedido pendente.</exception>
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
        
        /// <summary>
        /// Valida se a altura ([heigth]) está dentro dos limites definidos nas constantes.
        /// </summary>
        /// <param name="heigth">A altura do jogador (em cm).</param>
        /// <exception cref="ValidationException">Lançada se o valor for menor que o mínimo ou maior que o máximo.</exception>
        private static void ValidateHeigth(int heigth)
        {
            if (heigth < ModelConstants.PlayerConst.MinHeight || heigth > ModelConstants.PlayerConst.MaxHeight)
            {
                throw new ValidationException("Height value is invalid");
            }
        }

        /// <summary>
        /// Valida se a data de nascimento é válida (o jogador deve ter entre 18 e 70 anos).
        /// </summary>
        /// <param name="dateOfBirth">A data de nascimento a ser verificada.</param>
        /// <exception cref="ValidationException">Lançada se o jogador for menor de 18 ou maior de 70.</exception>
        private static void ValidateAge(DateOnly dateOfBirth)
        {
            if (dateOfBirth > DateOnly.FromDateTime(DateTime.Now).AddYears(-ModelConstants.UserConst.MinAge)
                || dateOfBirth < DateOnly.FromDateTime(DateTime.Now).AddYears(-ModelConstants.UserConst.MaxAge))
            {
                throw new ValidationException("Invalid Date of birth");
            }
        }

        /// <summary>
        /// Valida se o valor da posição corresponde a um dos valores definidos no Enum [Position].
        /// </summary>
        /// <param name="position">O valor do Enum Position.</param>
        /// <exception cref="ValidationException">Lançada se o valor não for mapeável.</exception>
        private static void ValidatePosition(Position position)
        {
            if (!Enum.IsDefined(typeof(Position), position))
            {
                throw new ValidationException("Position invalid.");
            }
        }

        /// <summary>
        /// Valida se o formato do endereço está correto (deve terminar com ", [NomeDaCidade]").
        /// </summary>
        /// <param name="addressTeam">O endereço a ser validado.</param>
        /// <exception cref="ValidationException">Lançada se o endereço estiver vazio ou tiver formato incorreto.</exception>
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