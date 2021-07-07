using System;
using System.Collections.Generic;
using Dinah.Core;

namespace AudibleApi.Authentication
{
    /// <summary>
    /// holds state. usually has specialized submit()
    /// </summary>
    public abstract class LoginResult
    {
        protected Authenticate Authenticate { get; }

		protected Dictionary<string, string> Inputs { get; }

		public IDictionary<string, string> GetInputsReadOnly()
            => new Dictionary<string, string>(Inputs);

        protected string ResponseBody { get; }

        protected LoginResult(Authenticate authenticate, string responseBody)
        {
            Authenticate = authenticate ?? throw new ArgumentNullException(nameof(authenticate));
            ResponseBody = responseBody ?? throw new ArgumentNullException(nameof(responseBody));

            Inputs = HtmlHelper.GetInputs(ResponseBody);
        }
    }
}
