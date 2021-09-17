using System;
using System.Threading.Tasks;
using AudibleApi.Authorization;

namespace AudibleApi
{
	/// <summary>
	/// The Api class is backed by a complex set of interconnected tiny classes, each with a specific purpose. To avoid all of the complexity, use EzApiCreator. It will accept the minimum requirements and provide access to the api and will maintain auth info and keep it up to date in the file specified.
	/// </summary>
	public static partial class EzApiCreator
	{
		/// <summary>Create a new Audible Api object</summary>
		/// <param name="locale">Audible region/locale to connect to</param>
		/// <param name="identityFilePath">Load from and save to the file at this path</param>
		/// <param name="jsonPath">Optional JSONPath for location of identity tokens inside identity file</param>
		/// <returns>Object which enables calls to the Audible API</returns>
		public static async Task<Api> GetApiAsync(Locale locale, string identityFilePath, string jsonPath = null)
		{
			StackBlocker.ApiTestBlocker();

			try
			{
				return await createApiAsync(identityFilePath, jsonPath);
			}
			catch (Exception debugEx)
			{
				Serilog.Log.Logger.Debug("GetApiAsync. {@DebugInfo}", new
				{
					localeName = locale?.Name ?? "[empty]",
					//jsonPath,//this exposes unmasked account name
					debugEx_Message = debugEx.Message,
					debugEx_StackTrace = debugEx.StackTrace
				});
				throw;
			}
		}
	}
}
