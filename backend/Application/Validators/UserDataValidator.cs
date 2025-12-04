using Application.Interfaces.Validators;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace Application.Validators
{
    /// <summary>
    /// Validador de dados de utilizador, responsável por verificar o formato e a validade de campos essenciais.
    /// 
    /// Implementa o contrato [IUserDataValidator].
    /// </summary>
    public class UserDataValidator: IUserDataValidator
    {
        /// <summary>
        /// Valida o formato de um endereço de correio eletrónico.
        /// </summary>
        /// <remarks>
        /// Tenta instanciar um objeto [MailAddress]. Se o formato for inválido, lança uma [ValidationException].
        /// </remarks>
        /// <param name="email">O endereço de email a ser validado.</param>
        /// <exception cref="ValidationException">Lançada se o formato do email for inválido.</exception>
        public void EmailValidation(string email)
        {
            try
            {
                var emailAddress = new MailAddress(email);
            }
            catch
            {
                throw new ValidationException("Formato de email invalido, Por favor insira um email valido");
            }
        }

        /// <summary>
        /// Valida o formato de um número de telemóvel.
        /// </summary>
        /// <remarks>
        /// Utiliza o atributo de validação [PhoneAttribute] do .NET para verificar o formato do número.
        /// </remarks>
        /// <param name="phoneNumber">O número de telefone a ser validado.</param>
        /// <exception cref="ValidationException">Lançada se o número de telemóvel for inválido.</exception>
        public void PhoneNumberValidation(string phoneNumber) {
            if (!new PhoneAttribute().IsValid(phoneNumber)) {
                throw new ValidationException("Formato de telemovel invalido, por favor valide o numero");
            }
        }

        /// <summary>
        /// Valida se o ID de utilizador recém-criado é válido e não é nulo.
        /// </summary>
        /// <remarks>
        /// Utilizado após a criação de um utilizador no Firebase (ou similar) para garantir que o processo foi bem-sucedido.
        /// </remarks>
        /// <param name="newUserId">O ID de utilizador (UID) gerado.</param>
        /// <exception cref="Exception">Lançada se o ID for nulo (indicando falha na criação).</exception>
        public void CreateUserValidation(string newUserId) {
            if (newUserId == null) {
                throw new Exception("O user não foi criado com sucesso");
            }
        }

        /// <summary>
        /// Valida se o ID de utilizador fornecido para eliminação não é nulo.
        /// </summary>
        /// <param name="userIdToDelete">O ID do utilizador a ser eliminado.</param>
        /// <exception cref="ValidationException">Lançada se o ID do utilizador for nulo.</exception>
        public void DeleteUserValidation(string userIdToDelete)
        {
            if (userIdToDelete == null)
            {
                throw new ValidationException("O a deletar user não Existe");
            }
        }
    }
}
