using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudibleApi.Authentication;

/// <summary>
/// rules for IsMatch and CreateResult
/// </summary>
internal abstract partial class ResultFactory : Enumeration<ResultFactory>
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
	public static ResultFactory ApprovalNeededPage { get; }
		= new ApprovalNeededPageFactory();
	public static ResultFactory MfaSelectionPage { get; }
		= new MfaSelectionPageFactory();
	public static ResultFactory LoginComplete { get; }
		= new LoginCompleteFactory();
	#endregion

	/// <summary>
	/// If true, the body sent to the IsMatch check may be blank
	/// </summary>
	protected virtual bool AllowBlankBodyIn_IsMatch => false;

	public static Task<bool> IsCompleteAsync(HttpResponseMessage? response)
		=> new LoginCompleteFactory().IsMatchAsync(response);

	private static int value = 0;
	protected ResultFactory(string displayName) : base(value++, displayName) { }

	public async Task<bool> IsMatchAsync(HttpResponseMessage? response)
	{
		if (response?.Content is null)
			return false;

		var body = await response.Content.ReadAsStringAsync();
		if (!AllowBlankBodyIn_IsMatch && string.IsNullOrWhiteSpace(body))
			return false;

		return _isMatchAsync(response, body);
	}
	protected abstract bool _isMatchAsync(HttpResponseMessage response, string body);

	public async Task<LoginResult> CreateResultAsync(Authenticate authenticate, HttpResponseMessage response, Dictionary<string, string?> oldInputs)
	{
		ArgumentValidator.EnsureNotNull(authenticate, nameof(authenticate));
		ArgumentValidator.EnsureNotNull(response, nameof(response));
		ArgumentValidator.EnsureNotNull(oldInputs, nameof(oldInputs));

		if (response.Content is null)
			throw new ArgumentException("response Content is null");

		if (!await IsMatchAsync(response))
			throw new LoginFailedException("IsMatch validation failed");

		var body = await response.Content.ReadAsStringAsync();
		return _createResultAsync(authenticate, response, body, oldInputs);
	}
	protected abstract LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string?> oldInputs);
}
