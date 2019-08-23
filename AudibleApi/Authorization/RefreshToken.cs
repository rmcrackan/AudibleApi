using System;
using Dinah.Core;

namespace AudibleApi.Authorization
{
	public class RefreshToken : StrongType<string>
	{
		public const string REQUIRED_BEGINNING = "Atnr|";

		public RefreshToken(string value) : base(value) { }

		protected override void ValidateInput(string value)
		{
			if (value is null)
				throw new ArgumentNullException(nameof(value));

			if (!value.StartsWith(REQUIRED_BEGINNING))
				throw new ArgumentException("Improperly formatted refresh token", nameof(value));
		}
	}
}
