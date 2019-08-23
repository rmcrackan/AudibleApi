using System;
using Dinah.Core;

namespace AudibleApi
{
	public interface IClientSharer
	{
		ISealedHttpClient GetSharedClient(Uri target);
	}
}