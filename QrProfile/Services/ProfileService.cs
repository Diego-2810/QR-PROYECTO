using QrProfile.Models;
using QrProfile.Repositories;

namespace QrProfile.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;

    public ProfileService(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<ProfileData?> GetProfileByLanguageAsync(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            languageCode = "es";
        }
        return await _profileRepository.GetByLanguageCodeAsync(languageCode.ToLower());
    }
}
