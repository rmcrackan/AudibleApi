using AudibleApi.Authorization;
using Dinah.Core;

namespace AudibleApi.Authentication;

internal class LoginComplete : LoginResult
{
	public Identity Identity { get; }

	public LoginComplete(Authenticate authenticate, string responseBody, Identity identity) : base(authenticate, responseBody)
		=> Identity = ArgumentValidator.EnsureNotNull(identity, nameof(identity));
}
