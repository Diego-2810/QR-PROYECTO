using QrProfile.Models;

namespace QrProfile.Repositories;

public interface IProfileRepository
{
    Task<ProfileData?> GetByLanguageCodeAsync(string languageCode);
}
