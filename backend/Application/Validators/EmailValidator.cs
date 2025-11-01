using Application.Interfaces.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class EmailValidator: IEmailValidator
    {
        public bool IsValid(string email)
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
