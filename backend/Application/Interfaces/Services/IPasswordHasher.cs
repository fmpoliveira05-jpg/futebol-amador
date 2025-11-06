namespace Application.Interfaces.Services
{
    // interface síncrona, não se usa Tasks
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}