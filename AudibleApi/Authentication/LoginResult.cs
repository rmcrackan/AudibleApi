using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

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

        protected LoginResult(Authenticate authenticate, string responseBody)
        {
            Authenticate = authenticate ?? throw new ArgumentNullException(nameof(authenticate));

            if (responseBody is null)
                throw new ArgumentNullException(nameof(responseBody));
            Inputs = HtmlHelper.GetInputs(responseBody);
        }
    }
}
