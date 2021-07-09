using System;
using System.Linq;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi.Authentication
{
	public class MfaSelectionPage : LoginResult
	{
        public MfaConfig MfaConfig { get; }

        public MfaSelectionPage(Authenticate authenticate, string responseBody) : base(authenticate, responseBody)
        {
            // see: HtmlHelperTests.GetElements.parse_sample()

            static string getText(HtmlAgilityPack.HtmlNode node)
            {
                var text = node.SelectSingleNode(".//span")
                    ?.InnerText
                    .Trim();
                Serilog.Log.Logger.Debug("getText: {@DebugInfo}", new { node.OuterHtml, text = text ?? "[null]"});
                return text;
            }

            static string getName(HtmlAgilityPack.HtmlNode node)
            {
                var name = node.SelectSingleNode(".//input")
                    .Attributes["name"]
                    ?.Value;
                Serilog.Log.Logger.Debug("getName: {@DebugInfo}", new { node.OuterHtml, name = name ?? "[null]" });
                return name;
            }

            static string getValue(HtmlAgilityPack.HtmlNode node)
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

            MfaConfig = new MfaConfig
            {
                // optional settings
                Title = title,

                Button1Text = getText(divs[0]),
                Button2Text = getText(divs[1]),
                Button3Text = getText(divs[2]),

                // mandatory values
                Button1Name = getName(divs[0]),
                Button1Value = getValue(divs[0]),

                Button2Name = getName(divs[1]),
                Button2Value = getValue(divs[1]),

                Button3Name = getName(divs[2]),
                Button3Value = getValue(divs[2])
            };
        }

        public async Task<LoginResult> SubmitAsync(string name, string value)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(name, nameof(name));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(value, nameof(value));

            Inputs[name] = value;

            return await LoginResultRunner.GetResultsPageAsync(Authenticate, Inputs, LoginResultRunner.SignInPage.MFA);
        }
    }
}
