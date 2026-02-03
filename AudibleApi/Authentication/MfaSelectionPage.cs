using Dinah.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AudibleApi.Authentication;

internal class MfaSelectionPage : LoginResult
{
	public MfaConfig MfaConfig { get; }

	public MfaSelectionPage(Authenticate authenticate, Uri uri, string responseBody) : base(authenticate, responseBody)
	{
		// see: HtmlHelperTests.GetElements.parse_sample()

		static string? getText(HtmlAgilityPack.HtmlNode node)
		{
			var text = node.SelectSingleNode(".//span")
				?.InnerText
				.Trim();
			Serilog.Log.Logger.Debug("getText: {@DebugInfo}", new { node.OuterHtml, text = text ?? "[null]" });
			return text;
		}

		static string? getName(HtmlAgilityPack.HtmlNode node)
		{
			var name = node.SelectSingleNode(".//input")
				.Attributes["name"]
				?.Value;
			Serilog.Log.Logger.Debug("getName: {@DebugInfo}", new { node.OuterHtml, name = name ?? "[null]" });
			return name;
		}

		static string? getValue(HtmlAgilityPack.HtmlNode node)
		{
			var value = node.SelectSingleNode(".//input")
				.Attributes["value"]
				?.Value;
			Serilog.Log.Logger.Debug("getValue: {@DebugInfo}", new { node.OuterHtml, valueCount = value ?? "[null]" });
			return value;
		}

		var divs = HtmlHelper.GetElements(ResponseBody, "div", "data-a-input-name", "otpDeviceContext");
		var title = HtmlHelper.GetElements(ResponseBody, "title").FirstOrDefault()?.InnerText.Trim();
		Serilog.Log.Logger.Information("Page info {@DebugInfo}", new { divs.Count, title });

		Action = uri.ToString();
		MfaConfig = new MfaConfig
		{
			// optional
			Title = title
		};
		foreach (var div in divs)
			MfaConfig.Buttons.Add(new MfaConfigButton
			{
				// optional
				Text = getText(div),
				// mandatory
				Name = getName(div),
				Value = getValue(div)
			});
	}

	public async Task<LoginResult> SubmitAsync(string name, string value)
	{
		ArgumentValidator.EnsureNotNullOrWhiteSpace(name, nameof(name));
		ArgumentValidator.EnsureNotNullOrWhiteSpace(value, nameof(value));

		Inputs[name] = value;

		return await LoginResultRunner.GetResultsPageAsync(Authenticate, Inputs, Method, Action);
	}
}
