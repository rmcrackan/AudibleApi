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
                => node.SelectSingleNode(".//span")
                ?.InnerText
                .Trim();

            static string getName(HtmlAgilityPack.HtmlNode node)
                => node.SelectSingleNode(".//input")
                .Attributes["name"]
                ?.Value;

            static string getValue(HtmlAgilityPack.HtmlNode node)
                => node.SelectSingleNode(".//input")
                .Attributes["value"]?
                .Value;

            var divs = HtmlHelper.GetElements(ResponseBody, "div", "data-a-input-name", "otpDeviceContext");

            MfaConfig = new MfaConfig
            {
                // optional settings
                Title = HtmlHelper.GetElements(ResponseBody, "title").FirstOrDefault()?.InnerText,

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
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Selected name may not be blank", nameof(name));

            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Selected value may not be blank", nameof(value));

            Inputs[name] = value;

            return await LoginResultRunner.GetResultsPageAsync(Authenticate, Inputs, LoginResultRunner.SignInPage.MFA);
        }
    }
}
