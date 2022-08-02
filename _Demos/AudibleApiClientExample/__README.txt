SETUP
=====
Demo example is Windows only. For a different OS, edit: LoginCallback.GetCaptchaAnswerAsync

1) to use demo app WITHOUT live libation settings, fill out these settings in _UserSetup.cs
	LOCALE_NAME
	IDENTITY_FILE_PATH
	JSON_PATH

2) to use demo app WITH live libation settings, create this file on desktop: SECRET.txt
  line 1: username/email. if live tokens are referenced in line 3 then line 1 can be left blank
  line 2: password. if live tokens are referenced in line 3 then line 1 can be left blank
  line 3: path to Libation's AccountsSettings.json. if logging in, this can be left blank
  line 4: which account to use. optional. If you have multiple accounts in AccountsSettings.json: "1" (without quotes) to use the first account. 2 to use the 2nd, etc
example file:
example@gmail.com
p@ssw0rd
C:\foo\bar\AccountsSettings.json
1


CUSTOMIZE
=========
Add your code in AudibleApiClient


RUN
===
In _UserSetup.Run(), add the call to your AudibleApiClient code
