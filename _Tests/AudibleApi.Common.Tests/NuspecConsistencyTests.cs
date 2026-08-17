using System.IO;
using System.Xml.Linq;

namespace NuspecConsistencyTests;

/// <summary>
/// AudibleApi.nuspec is hand-maintained and it, not the csproj, is what the publish workflow packs:
/// <c>dotnet pack ... -p:NuspecFile=...</c>. So a PackageReference bump changes what the code builds against
/// while the published package goes on declaring the old floor. That shipped twice - 11.0.0.1 and 11.0.1.1 both
/// claimed Dinah.Core 10.2.1.1, which has no SecretString, the type the token values are held in. A consumer
/// taking AudibleApi on its own would have resolved 10.2.1.1 and hit a missing type at runtime.
/// <para>
/// Nothing about the csproj hints that the nuspec exists, so this test is the thing that notices.
/// </para>
/// </summary>
[TestClass]
public class Dependencies
{
	[TestMethod]
	public void the_nuspec_declares_the_same_Dinah_Core_the_code_builds_against()
	{
		var repoRoot = FindRepositoryRoot();

		var referenced = PackageReferenceVersion(
			Path.Combine(repoRoot, "AudibleApi.Common", "AudibleApi.Common.csproj"),
			"Dinah.Core");
		var declared = NuspecDependencyVersion(
			Path.Combine(repoRoot, "AudibleApi.nuspec"),
			"Dinah.Core");

		Assert.AreEqual(
			referenced,
			declared,
			"AudibleApi.nuspec declares a different Dinah.Core than AudibleApi.Common references. The nuspec is "
			+ "what gets packed, so the published package would tell consumers the wrong floor.");
	}

	/// <summary>
	/// Every dependency the nuspec declares has to be one the code actually references, or the published package
	/// asks consumers for something arbitrary.
	/// </summary>
	[TestMethod]
	public void the_nuspec_declares_nothing_the_projects_do_not_reference()
	{
		var repoRoot = FindRepositoryRoot();
		var nuspec = Path.Combine(repoRoot, "AudibleApi.nuspec");

		var referencedIds = new[] { "AudibleApi", "AudibleApi.Common" }
			.Select(project => Path.Combine(repoRoot, project, $"{project}.csproj"))
			.SelectMany(PackageReferenceIds)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		foreach (var (id, _) in NuspecDependencies(nuspec))
			Assert.IsTrue(
				referencedIds.Contains(id),
				$"AudibleApi.nuspec declares a dependency on '{id}', which neither project references. "
				+ "AudibleApi.Common ships inside this package as a file, so it must not be declared either.");
	}

	private static string FindRepositoryRoot()
	{
		for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
			if (File.Exists(Path.Combine(dir.FullName, "AudibleApi.nuspec")))
				return dir.FullName;

		// deliberately a failure rather than inconclusive: if the files cannot be found the premise is broken,
		// and silently skipping is how the drift got published in the first place
		throw new AssertFailedException(
			$"Could not find AudibleApi.nuspec above {AppContext.BaseDirectory}.");
	}

	private static string PackageReferenceVersion(string csproj, string id)
		=> PackageReferences(csproj)
			.Where(r => id.Equals(r.id, StringComparison.OrdinalIgnoreCase))
			.Select(r => r.version)
			.SingleOrDefault()
			?? throw new AssertFailedException($"{Path.GetFileName(csproj)} has no PackageReference to {id}.");

	private static IEnumerable<string> PackageReferenceIds(string csproj)
		=> PackageReferences(csproj).Select(r => r.id);

	private static IEnumerable<(string id, string? version)> PackageReferences(string csproj)
		=> XDocument.Load(csproj)
			.Descendants("PackageReference")
			.Select(e => (id: e.Attribute("Include")?.Value ?? "", version: e.Attribute("Version")?.Value))
			.Where(r => r.id.Length > 0);

	private static string NuspecDependencyVersion(string nuspec, string id)
		=> NuspecDependencies(nuspec)
			.Where(d => id.Equals(d.id, StringComparison.OrdinalIgnoreCase))
			.Select(d => d.version)
			.SingleOrDefault()
			?? throw new AssertFailedException($"AudibleApi.nuspec declares no dependency on {id}.");

	/// <summary>
	/// Only <c>&lt;metadata&gt;</c> counts. The file has carried a dependencies block outside it before, which
	/// NuGet silently ignores, so reading the whole document would report dependencies the package never had.
	/// </summary>
	private static IEnumerable<(string id, string? version)> NuspecDependencies(string nuspec)
		=> XDocument.Load(nuspec)
			.Root!
			.Elements("metadata")
			.Descendants("dependency")
			.Select(e => (id: e.Attribute("id")?.Value ?? "", version: e.Attribute("version")?.Value))
			.Where(d => d.id.Length > 0);
}
