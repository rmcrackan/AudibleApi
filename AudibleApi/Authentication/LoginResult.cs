using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Dinah.Core;

namespace AudibleApi.Authentication
{
    /// <summary>
    /// holds state. usually has specialized submit()
    /// </summary>
    internal abstract class LoginResult
    {
        protected Authenticate Authenticate { get; }
        protected HttpMethod Method { get; }
        protected string Action { get; init; }
		protected Dictionary<string, string> Inputs { get; }

		public IDictionary<string, string> GetInputsReadOnly()
            => new Dictionary<string, string>(Inputs);

        protected string ResponseBody { get; }

        protected LoginResult(Authenticate authenticate, string responseBody)
        {
            Authenticate = ArgumentValidator.EnsureNotNull(authenticate, nameof(authenticate));
            ResponseBody = ArgumentValidator.EnsureNotNull(responseBody, nameof(responseBody));

            Inputs = HtmlHelper.GetInputs(ResponseBody);

            (var method, var action) = getNextAction();

            Method = string.IsNullOrEmpty(method) ? HttpMethod.Post : new HttpMethod(method);
            Action = string.IsNullOrEmpty(action) ? string.Empty : action;
        }

		//https://github.com/mkb79/Audible/blob/e0cc73ff667d6f0cee5e610269fc2e380a2d2204/src/audible/login.py#L157
		protected (string method, string url) getNextAction()
		{
			var signInForm
                = HtmlHelper.GetElements(ResponseBody, "form", "name", "signIn").FirstOrDefault()
                ?? HtmlHelper.GetElements(ResponseBody, "form").FirstOrDefault();

			var method = signInForm?.Attributes["method"]?.Value;
			var url = signInForm?.Attributes["action"]?.Value;
			return (method, url);
		}
	}
}
