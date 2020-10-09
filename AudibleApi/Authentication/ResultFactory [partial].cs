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
    /// rules for IsMatch and CreateResult
    /// </summary>
    public abstract partial class ResultFactory : Enumeration<ResultFactory>
    {
        #region enumeration single instances
        // MANDATORY to include here so that ResultFactory.GetAll() can find them.
        // also convenient exposure for unit testing
        public static ResultFactory CredentialsPage { get; }
            = new CredentialsPageFactory();
        public static ResultFactory CaptchaPage { get; }
            = new CaptchaPageFactory();
        public static ResultFactory TwoFactorAuthenticationPage { get; }
            = new TwoFactorAuthenticationPageFactory();
        public static ResultFactory LoginComplete { get; }
            = new LoginCompleteFactory();
        public static ResultFactory ApprovalNeeded { get; }
            = new ApprovalNeededFactory();
        #endregion

        public static Task<bool> IsCompleteAsync(HttpResponseMessage response)
            => new LoginCompleteFactory().IsMatchAsync(response);

        private static int value = 0;
        protected ResultFactory(string displayName) : base(value++, displayName) { }

        public virtual Task<bool> IsMatchAsync(HttpResponseMessage response)
            => Task.FromResult(response?.Content != null);

        public virtual async Task<LoginResult> CreateResultAsync(Authenticate authenticate, HttpResponseMessage response, Dictionary<string, string> oldInputs)
        {
            ArgumentValidator.EnsureNotNull(authenticate, nameof(authenticate));
            ArgumentValidator.EnsureNotNull(response, nameof(response));
            ArgumentValidator.EnsureNotNull(oldInputs, nameof(oldInputs));

            if (response.Content is null)
                throw new ArgumentException();

            if (!await IsMatchAsync(response))
                throw new LoginFailedException("IsMatch validation failed");

            return null;
        }
    }
}
