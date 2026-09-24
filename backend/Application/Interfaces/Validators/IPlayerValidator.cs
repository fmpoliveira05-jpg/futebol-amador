using Application.DTOs.Filters;
using Application.DTOs.PlayerDTOs;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio e Controlo de Integridade para a entidade [Player].
    /// 
    /// Esta interface define as regras de validação síncrona que garantem a validade dos dados e a
    /// aplicação de regras de negócio (ex: unicidade de email/telefone, afiliação a equipas) antes das transações.
    /// </summary>
    public interface IPlayerValidator
    {
        /// <summary>
        /// Valida se a entidade [Player] foi encontrada (não é nula) após a consulta.
        /// </summary>
        /// <param name="player">A entidade Player a ser verificada.</param>
        /// <exception cref="Domain.Exceptions.NotFoundException">Lançada se o jogador for nulo.</exception>
        void PlayerExists(Player? player);

        /// <summary>
        /// Valida a existência de um jogador antes da eliminação.
        /// </summary>
        /// <param name="player">A entidade Player a ser eliminada.</param>
        void DeletePlayerValidator(Player? player);

        /// <summary>
        /// Valida a existência de um jogador antes da consulta de detalhes.
        /// </summary>
        /// <param name="player">A entidade Player a ser consultada.</param>
        void GetPlayerByIdValidator(Player? player);

        /// <summary>
        /// Valida todos os campos de um novo perfil de jogador antes da criação.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Unicidade de Email/Telefone, Limites de Altura e Idade, Formato da Posição.
        /// </remarks>
        /// <param name="CreatePlayerDto">O DTO com os dados do novo jogador.</param>
        /// <param name="phoneUser">O utilizador existente com o mesmo telefone.</param>
        /// <param name="emailUser">O utilizador existente com o mesmo email.</param>
        /// <exception cref="Domain.Exceptions.ValidationException">Se houver violação de unicidade ou formato inválido.</exception>
        void CreatePlayerValidator(CreatePlayerDto CreatePlayerDto, User? phoneUser, User? emailUser);

        /// <summary>
        /// Valida todos os campos de um perfil de jogador antes da atualização.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Existência do jogador, unicidade condicional de Email/Telefone e validação de formato.
        /// </remarks>
        /// <param name="UpdatePlayerDto">O DTO com os novos dados.</param>
        /// <param name="player">A entidade [Player] existente.</param>
        /// <param name="existingPlayers">Utilizadores existentes (para verificação de unicidade).</param>
        /// <exception cref="Domain.Exceptions.NotFoundException">Se o jogador não for encontrado.</exception>
        /// <exception cref="Domain.Exceptions.ValidationException">Se houver violação de unicidade ou formato inválido.</exception>
        void UpdatePlayerValidator(UpdatePlayerDto UpdatePlayerDto, Player player, User[] existingPlayers);

        /// <summary>
        /// Valida se houve alguma alteração de dados num objeto de atualização.
        /// </summary>
        /// <param name="hasChange">Booleano que indica se a comparação de DTOs detetou mudanças.</param>
        /// <exception cref="Domain.Exceptions.ValidationException">Lançada se não houver alteração nos dados do utilizador.</exception>
        void ValidateHasChangeDataPlayer(bool hasChange);

        /// <summary>
        /// Valida se o jogador pode sair da equipa.
        /// </summary>
        /// <remarks>
        /// Requer que o jogador pertença a uma equipa.
        /// </remarks>
        /// <param name="player">A entidade [Player] que está a tentar sair.</param>
        /// <exception cref="Domain.Exceptions.ValidationException">Lançada se o jogador não pertencer a nenhuma equipa.</exception>
        void LeaveTeamValidator(Player player);

        /// <summary>
        /// Valida um novo pedido de adesão (Join Request) enviado por um Jogador.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: O jogador não pode pertencer já a uma equipa e não pode haver pedido pendente.
        /// </remarks>
        /// <param name="player">O jogador que envia o pedido.</param>
        /// <param name="team">A equipa alvo.</param>
        /// <param name="request">O pedido pendente existente.</param>
        /// <exception cref="Domain.Exceptions.BusinessRuleException">Lançada se a regra de negócio for violada (ex: já tem equipa).</exception>
        void SendMembershipRequestValidator(Player player, Team team, MembershipRequest request);

        /// <summary>
        /// Valida a consistência do intervalo de filtros de listagem de equipas.
        /// </summary>
        /// <remarks>
        /// Verifica a consistência dos intervalos numéricos (Pontos, Idade Média, Contagem de Membros).
        /// </remarks>
        /// <param name="filter">O DTO de filtros.</param>
        /// <exception cref="System.InvalidOperationException">Lançada se os intervalos forem inconsistentes (Min > Max).</exception>
        void ValidateFiltersListTeams(FilterListTeamDto filter);
    }
}