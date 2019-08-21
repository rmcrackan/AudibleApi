using System;
using BaseLib;

namespace AudibleApi.Authorization
{
	public class PrivateKey : StrongType<string>
	{
		public const string REQUIRED_BEGINNING = "-----BEGIN RSA PRIVATE KEY-----";
		public const string REQUIRED_ENDING = "-----END RSA PRIVATE KEY-----";

		public PrivateKey(string value) : base(value) { }

		protected override void ValidateInput(string value)
		{
			if (value is null)
				throw new ArgumentNullException(nameof(value));

			if (!value.Trim().StartsWith(REQUIRED_BEGINNING))
				throw new ArgumentException("Improperly formatted RSA private key", nameof(value));

			if (!value.Trim().EndsWith(REQUIRED_ENDING))
				throw new ArgumentException("Improperly formatted RSA private key", nameof(value));
		}
	}
}
