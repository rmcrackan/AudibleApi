using System;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi
{
	public interface IHttpClientSharer
	{
		ISealedHttpClient GetSharedHttpClient(Uri target);
		ISealedHttpClient GetSharedHttpClient(string target);
	}
}