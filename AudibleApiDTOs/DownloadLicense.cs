using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleApiDTOs
{
	public record DownloadLicense
	{
		public string DownloadUrl { get; init; }
		public string AudibleKey { get; init; }
		public string AudibleIV { get; init; }
	}
}
