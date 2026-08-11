namespace MyTemplate.Application.Abstractions;

public interface IPasswordHasherService
{
    string Hash(string password);
    bool Verify(string hash, string password);
}
