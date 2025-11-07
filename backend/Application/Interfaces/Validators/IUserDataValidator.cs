namespace Application.Interfaces.Validators
{
    public interface IUserDataValidator
    {
        void EmailValidation(string email);

        void PhoneNumberValidation(string phoneNumber);

        void CreateUserValidation(string newUserId);

        void DeleteUserValidation(string userIdToDelete);
    }
}