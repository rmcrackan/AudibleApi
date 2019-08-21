using System;
using BaseLib;

namespace AudibleApi
{
	public interface IClientSharer
	{
		ISealedHttpClient GetSharedClient(Uri target);
	}
}