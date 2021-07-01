using System;
using Dinah.Core.Net.Http;

namespace AudibleApi
{
	public interface IHttpClientSharer
	{
		IHttpClientActions GetSharedHttpClient(Uri target);
		IHttpClientActions GetSharedHttpClient(string target);
	}
}