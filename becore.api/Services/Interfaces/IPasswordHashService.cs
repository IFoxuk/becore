namespace becore.api.Services.Interfaces;

public interface IPasswordHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
