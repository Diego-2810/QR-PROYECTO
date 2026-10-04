namespace QrProfile.Models;

public class ProfileData
{
    public int Id { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string CoreValues { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Services { get; set; } = string.Empty;
}
