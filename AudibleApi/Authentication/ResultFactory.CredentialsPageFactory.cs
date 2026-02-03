using Dinah.Core;
using System.Collections.Generic;
using System.Net.Http;

namespace AudibleApi.Authentication;

internal abstract partial class ResultFactory
{
	private class CredentialsPageFactory : ResultFactory
	{
		public CredentialsPageFactory() : base(nameof(CredentialsPageFactory)) { }

		protected override bool _isMatchAsync(HttpResponseMessage response, string body)
		{
			var newInputs = HtmlHelper.GetInputs(body);
			return
				newInputs.ContainsKey("email") &&
				newInputs.ContainsKey("password") &&
				!newInputs.ContainsKey("use_image_captcha");
		}

		protected override LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string?> oldInputs)
			// do not extract email or pw from inputs. if we're here then a previous login failed
			=> new CredentialsPage(authenticate, body);
	}
}
