using Dinah.Core;

namespace AudibleApi;

public record Locale
{
	public static Locale Empty => new Locale();
	private Locale()
	{
		Name = "[empty]";
		CountryCode = string.Empty;
		TopDomain = string.Empty;
		MarketPlaceId = string.Empty;
		Language = string.Empty;
	}

	public string Name { get; }

	public string CountryCode { get; }
	public string TopDomain { get; }
	public string MarketPlaceId { get; }
	public string Language { get; }
	public bool WithUsername { get; }

	public Locale(string name, string countryCode, string topDomain, string marketPlaceId, string language, bool withUsername = false)
	{
		Name = ArgumentValidator.EnsureNotNullOrWhiteSpace(name, nameof(name)).Trim();

		CountryCode = ArgumentValidator.EnsureNotNullOrWhiteSpace(countryCode, nameof(countryCode)).Trim();
		TopDomain = ArgumentValidator.EnsureNotNullOrWhiteSpace(topDomain, nameof(topDomain)).Trim();
		MarketPlaceId = ArgumentValidator.EnsureNotNullOrWhiteSpace(marketPlaceId, nameof(marketPlaceId)).Trim();
		Language = ArgumentValidator.EnsureNotNullOrWhiteSpace(language, nameof(language)).Trim();
		WithUsername = withUsername;
	}
}
