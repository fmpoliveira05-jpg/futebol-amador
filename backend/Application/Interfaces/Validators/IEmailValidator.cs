namespace Application.Interfaces.Validators
{
    public interface IEmailValidator
    {
        bool IsValid(string email);
    }
}