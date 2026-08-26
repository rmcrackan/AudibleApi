using AudibleApi.Authorization;
using System.Threading.Tasks;

namespace AudibleApi;

/// <summary>
/// The Api class is backed by a complex set of interconnected tiny classes, each with a specific purpose. To avoid all of the complexity, use EzApiCreator. It will accept the minimum requirements and provide access to the api and will maintain auth info and keep it up to date in the file specified.
/// </summary>
public static partial class EzApiCreator
{
	private static async Task<Api> createApiAsync(Identity inMemoryIdentity, string identityFilePath, string? jsonPath = null, Locale? storeLocale = null)
	{
		var identityPersister = new IdentityPersister(inMemoryIdentity, identityFilePath, jsonPath);
		return await createApiAsync(identityPersister, storeLocale);
	}

	private static async Task<Api> createApiAsync(string identityFilePath, string? jsonPath = null, Locale? storeLocale = null)
	{
		// will fail if no file entry
		var identityPersister = new IdentityPersister(identityFilePath, jsonPath);

		// will fail if there's an invalid file entry. Eg: new account will have no cookies and will fail that validation step. this also means it has not yet logged in
		return await createApiAsync(identityPersister, storeLocale);
	}

	private static Task<Api> createApiAsync(IdentityPersister identityPersister, Locale? storeLocale = null)
		=> createApiAsync(identityPersister.Identity, storeLocale);

	private static async Task<Api> createApiAsync(IIdentity identity, Locale? storeLocale = null)
	{
		var identityMaintainer = await IdentityMaintainer.CreateAsync(identity);
		return new Api(identityMaintainer, storeLocale);
	}
}
