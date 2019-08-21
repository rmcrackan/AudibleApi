using System;
using System.Threading.Tasks;
using AudibleApi.Authorization;

namespace AudibleApi
{
	/// <summary>
	/// The Api class is backed by a complex set of interconnected tiny classes, each with a specific purpose. To avoid all of the complexity, use EzApiCreator. It will accept the minimum requirements and provide access to the api and will maintain auth info and keep it up to date in the file specified.
	/// </summary>
	public static class EzApiCreator
	{
		/// <summary>
		/// Create a new Api object by loading identity info in existing file
		/// </summary>
		public static async Task<Api> GetApiAsync(string file)
		{
			StackBlocker.ApiTestBlocker();

			var identityPersistent = new IdentityPersistent(file);
			var api = await createApiAsync(identityPersistent);
			return api;
		}

		/// <summary>
		/// Create a new Api object. Use provided identity, save to file
		/// </summary>
		public static async Task<Api> GetApiAsync(string file, IIdentity identity)
		{
			StackBlocker.ApiTestBlocker();

			var identityPersistent = new IdentityPersistent(file, identity);
			var api = await createApiAsync(identityPersistent);
			return api;
		}

		private static async Task<Api> createApiAsync(this IdentityPersistent identityPersistent)
		{
			var identityMaintainer = await IdentityMaintainer.CreateAsync(identityPersistent);
			var api = new Api(identityMaintainer);
			return api;
		}
	}
}
