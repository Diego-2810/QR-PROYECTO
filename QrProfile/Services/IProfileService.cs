using QrProfile.Models;

namespace QrProfile.Services;

public interface IProfileService
{
    Task<ProfileData?> GetProfileByLanguageAsync(string languageCode);
}
