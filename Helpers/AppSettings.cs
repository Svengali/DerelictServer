namespace WebApi.Helpers;

public class AppSettings
{
	public string Secret { get; set; } = "secret";

	// refresh token time to live (in days), inactive tokens are
	// automatically deleted from the database after this time
	public int RefreshTokenTTL { get; set; } = 1;

	public string EmailFrom { get; set; } = "DerelictOnline@gmail.com";
	public string SmtpHost { get; set; } = "smtp.gmail.com";
	public int SmtpPort { get; set; } = 587;
	public string SmtpUser { get; set; } = "DerelictOnline";
	public string SmtpPass { get; set; } = "stggxcnvcfnbfgxh";
}