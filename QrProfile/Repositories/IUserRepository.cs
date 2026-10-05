using QrProfile.Models;

namespace QrProfile.Repositories;

public interface IUserRepository
{
    Task<bool> CreateUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
}
