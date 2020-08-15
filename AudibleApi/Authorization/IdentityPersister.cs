using System;
using System.Collections.Generic;
using System.IO;
using Dinah.Core.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authorization
{
	public class IdentityPersister : JsonFilePersister<IIdentity>
	{
		/// <summary>Alias for Target </summary>
		public IIdentity Identity => Target;

		/// <summary>uses path. create file if doesn't yet exist</summary>
		public IdentityPersister(IIdentity identity, string path, string jsonPath = null)
			: base(identity, path, jsonPath) { }

		/// <summary>load from existing file</summary>
		public IdentityPersister(string path, string jsonPath = null)
			: base(path, jsonPath) { }

		protected override IIdentity DeserializeTarget(string json, string jsonPath)
			=> Authorization.Identity.FromJson(json, JsonPath);

		protected override JsonSerializerSettings GetSerializerSettings()
			=> Authorization.Identity.GetJsonSerializerSettings();
	}
}
