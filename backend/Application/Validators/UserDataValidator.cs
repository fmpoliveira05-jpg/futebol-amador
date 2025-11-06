using Application.Interfaces.Validators;
using Google.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class UserDataValidator: IUserDataValidator
    {
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

        public void PhoneNumberValidation(string phoneNumber) {
            if (!new PhoneAttribute().IsValid(phoneNumber)) {
                throw new ValidationException("Formato de telemovel invalido, por favor valide o numero");
            }
        }

        public void CreateUserValidation(string newUserId) {
            if (newUserId == null) {
                throw new Exception("O user não foi criado com sucesso");
            }
        }
    }
}
