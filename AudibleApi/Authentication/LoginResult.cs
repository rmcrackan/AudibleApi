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
        private IHttpClient _client { get; }
		protected ISystemDateTime SystemDateTime { get; }
		protected Dictionary<string, string> Inputs { get; }

		public IDictionary<string, string> GetInputsReadOnly()
            => new Dictionary<string, string>(Inputs);

        protected LoginResult(IHttpClient client, ISystemDateTime systemDateTime, string responseBody)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
			SystemDateTime = systemDateTime ?? throw new ArgumentNullException(nameof(systemDateTime));
			if (responseBody is null)
				throw new ArgumentNullException(nameof(responseBody));
			Inputs = HtmlHelper.GetInputs(responseBody);
		}

        protected Task<LoginResult> GetResultsPageAsync(Dictionary<string, string> inputs)
            => LoginResultRunner.GetResultsPageAsync(_client, SystemDateTime, inputs);
    }
}
