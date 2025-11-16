using Application.DTOs.SuperAdmin;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Validators
{
    public class SuperAdminValidator : ISuperAdminValidator
    {
        IUserDataValidator emailValidator;

        public SuperAdminValidator(IUserDataValidator emailValidator)
        {
            this.emailValidator = emailValidator;
        }

        public void SuperAdminExists(SuperAdmin sadmin)
        {
            if (sadmin == null)
            {
                throw new NotFoundException("Super Admin doesn't exist.");
            }
        }

        public void CreateSuperAdminValidator(CreateSuperAdminDTO dto, User[] sadmins)
        {
            if (sadmins[0] != null)
            {
                throw new ValidationException($"The email '{sadmins[0].Email}' is already in use.");
            }

            if (sadmins[1] != null)
            {
                throw new ValidationException($"The phone number '{sadmins[1].Phone}' is already in use.");
            }

            emailValidator.EmailValidation(dto.Email);

            if (dto.DateOfBirth > DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
                || dto.DateOfBirth < DateOnly.FromDateTime(DateTime.Now).AddYears(-70))
            {
                throw new ValidationException("Invalid Date of birth");
            }
        }

        public void DeleteSuperAdminValidator(SuperAdmin sadmin)
        {
            SuperAdminExists(sadmin);
        }


        public void UpdateSuperAdminValidator(UpdateSuperAdminDTO dto, SuperAdmin sadmin, User[] sadmins)
        {
            SuperAdminExists(sadmin);

            if (dto.Email != sadmin.Email)
            {
                if (sadmins[0] != null)
                {
                    throw new ValidationException($"The email '{sadmins[0].Email}' is already in use.");
                }
            }

            if (dto.Phone != sadmin.Phone)
            {
                if (sadmins[1] != null)
                {
                    throw new ValidationException($"The phone number '{sadmins[1].Phone}' is already in use.");
                }
            }

            emailValidator.EmailValidation(dto.Email);

            if (dto.DateOfBirth > DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
                || dto.DateOfBirth < DateOnly.FromDateTime(DateTime.Now).AddYears(-70))
            {
                throw new ValidationException("Invalid Date of birth");
            }

            if (dto.Phone.Length != 9)
            {
                throw new ValidationException("Phone number must have 9 digits.");
            }

            if (!int.TryParse(dto.Phone, out _))
            {
                throw new ValidationException("Phone number must only have numbers.");
            }
        }

        public void GetSuperAdminByIdValidator(SuperAdmin sadmin)
        {
            SuperAdminExists(sadmin);
        }
    }
}
