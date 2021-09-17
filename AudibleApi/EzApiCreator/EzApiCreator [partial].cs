using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi.Authentication;
using AudibleApi.Authorization;

namespace AudibleApi
{
	/// <summary>
	/// The Api class is backed by a complex set of interconnected tiny classes, each with a specific purpose. To avoid all of the complexity, use EzApiCreator. It will accept the minimum requirements and provide access to the api and will maintain auth info and keep it up to date in the file specified.
	/// </summary>
	public static partial class EzApiCreator
	{
		private static async Task<Api> createApiAsync(Identity inMemoryIdentity, string identityFilePath, string jsonPath = null)
		{
			var identityPersister = new IdentityPersister(inMemoryIdentity, identityFilePath, jsonPath);
			return await createApiAsync(identityPersister);
		}

		private static async Task<Api> createApiAsync(string identityFilePath, string jsonPath = null)
		{
			// will fail if no file entry
			var identityPersister = new IdentityPersister(identityFilePath, jsonPath);

			// will fail if there's an invalid file entry. Eg: new account will have no cookies and will fail that validation step. this also means it has not yet logged in
			return await createApiAsync(identityPersister);
		}

		private static Task<Api> createApiAsync(IdentityPersister identityPersister) => createApiAsync(identityPersister.Identity);

		private static async Task<Api> createApiAsync(IIdentity identity)
		{
			var identityMaintainer = await IdentityMaintainer.CreateAsync(identity);
			return new Api(identityMaintainer);
		}
	}
}
