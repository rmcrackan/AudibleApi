﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authentication;
using AudibleApi.Authorization;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using TestCommon;

namespace Authentic.MfaSelectionPageTests
{
	[TestClass]
	public class ctor
    {
        const string SAMPLE = @"
<title>
  My title
</title>
<body>
    <div class='a-row a-spacing-small'>
      <fieldset class='a-spacing-small'>
        
          <div data-a-input-name='otpDeviceContext' class='a-radio auth-TOTP'><label><input type='radio' name='otpDeviceContext' value='aAbBcC=, TOTP' checked/><i class='a-icon a-icon-radio'></i><span class='a-label a-radio-label'>
            Enter the OTP from the authenticator app
          </span></label></div>
        
          <div data-a-input-name='otpDeviceContext' class='a-radio auth-SMS'><label><input type='radio' name='otpDeviceContext' value='dDeEfE=, SMS'/><i class='a-icon a-icon-radio'></i><span class='a-label a-radio-label'>
            Send an SMS to my number ending with 123
          </span></label></div>
        
          <div data-a-input-name='otpDeviceContext' class='a-radio auth-VOICE'><label><input type='radio' name='otpDeviceContext' value='dDeEfE=, VOICE'/><i class='a-icon a-icon-radio'></i><span class='a-label a-radio-label'>
            Call me on my number ending with 123
          </span></label></div>
        
      </fieldset>
    </div>
</body>
";

        [TestMethod]
		public void parse_sample()
		{
            var mfa = new MfaSelectionPage(AuthenticateShared.GetAuthenticate(), SAMPLE);

            var mfaConfig = new MfaConfig
            {
                // optional settings
                Title = "My title",

                Button1Text = "Enter the OTP from the authenticator app",
                Button2Text = "Send an SMS to my number ending with 123",
                Button3Text = "Call me on my number ending with 123",

                // mandatory values
                Button1Name = "otpDeviceContext",
                Button1Value = "aAbBcC=, TOTP",

                Button2Name = "otpDeviceContext",
                Button2Value = "dDeEfE=, SMS",

                Button3Name = "otpDeviceContext",
                Button3Value = "dDeEfE=, VOICE"
            };

            mfa.MfaConfig.Should().Be(mfaConfig);
        }
    }
}
