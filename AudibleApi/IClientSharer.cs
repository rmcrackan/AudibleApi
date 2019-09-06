using System;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi
{
	public interface IClientSharer
	{
		ISealedHttpClient GetSharedClient(Uri target);
	}
}