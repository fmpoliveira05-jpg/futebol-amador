namespace Application.Interfaces.Validators
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio para dados de utilizador ([User]).
    /// 
    /// Esta interface define as regras de validação síncrona essenciais que verificam o formato e a integridade de campos de identidade (Email, Telefone, ID)
    /// antes de o utilizador ser criado ou eliminado no sistema.
    /// </summary>
    public interface IUserDataValidator
    {
        /// <summary>
        /// Valida o formato de um endereço de correio eletrónico.
        /// </summary>
        /// <param name="email">O endereço de email a ser validado.</param>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">Lançada se o formato do email for inválido.</exception>
        void EmailValidation(string email);

        /// <summary>
        /// Valida o formato de um número de telemóvel.
        /// </summary>
        /// <param name="phoneNumber">O número de telefone a ser validado.</param>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">Lançada se o número de telemóvel for inválido.</exception>
        void PhoneNumberValidation(string phoneNumber);

        /// <summary>
        /// Valida se o ID de utilizador retornado pelo fornecedor de autenticação (ex: Firebase) é válido.
        /// </summary>
        /// <remarks>
        /// Utilizado para garantir que a criação da identidade do utilizador no sistema de autenticação foi bem-sucedida.
        /// </remarks>
        /// <param name="newUserId">O ID de utilizador (UID) gerado.</param>
        /// <exception cref="System.Exception">Lançada se o ID for nulo ou vazio.</exception>
        void CreateUserValidation(string newUserId);

        /// <summary>
        /// Valida se o ID de utilizador fornecido para eliminação é válido.
        /// </summary>
        /// <param name="userIdToDelete">O ID do utilizador a ser eliminado.</param>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">Lançada se o ID for nulo ou inválido.</exception>
        void DeleteUserValidation(string userIdToDelete);
    }
}