using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dinah.Core;
using HtmlAgilityPack;

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

        public (string method, string url) GetNextAction()
        {
			HtmlDocument htmlDocument = new();
			htmlDocument.LoadHtml(ResponseBody);
			HtmlNodeCollection htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes(".//form");
			if (htmlNodeCollection == null)
				return default;

            var authValidateForm = htmlNodeCollection.FirstOrDefault(f => f.Attributes.Any(a => a.Name == "name" && a.Value == "signIn"));

			if (authValidateForm == null)
				return default;

            var method = authValidateForm.Attributes.FirstOrDefault(a => a.Name == "method")?.Value;
            var url = authValidateForm.Attributes.FirstOrDefault(a => a.Name == "action")?.Value;
            return (method, url);
		}
    }
}
