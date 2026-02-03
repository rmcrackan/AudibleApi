using Dinah.Core.Net.Http;
using System;

namespace AudibleApi;

public interface IHttpClientSharer
{
	IHttpClientActions GetSharedHttpClient(Uri target);
	IHttpClientActions GetSharedHttpClient(string target);
}