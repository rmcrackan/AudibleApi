using System;
using Newtonsoft.Json;

namespace AudibleApi.Common
{
	public abstract class V10Base<T> : DtoBase<T> where T : DtoBase<T>
	{
        [JsonProperty("response_groups")]
        public string[] ResponseGroups { get; set; }

        /// <summary>
        /// If not null, then main content was not returned by the Api.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
