using System;
using System.Collections.Generic;
using System.IO;

namespace AudibleApi.Authentication;

// do not derive from ApiErrorException. those are errors returned by API calls. Login is pre-API calls
public class LoginFailedException : Exception
{
	public string? RequestUrl { get; set; }
	public System.Net.HttpStatusCode ResponseStatusCode { get; set; }
	public Dictionary<string, string>? ResponseInputFields { get; set; }
	public List<string> ResponseBodyFilePaths { get; } = new List<string>();

	public LoginFailedException() : base() { }
	public LoginFailedException(string? message) : base(message) { }
	public LoginFailedException(string? message, Exception? innerException) : base(message, innerException) { }

	private List<(string filename, string contents)> files { get; } = new List<(string filename, string contents)>();

	/// <summary>response body is potentially huge; don't want it poluting logs. write to temp file. Call SaveFiles() in catch { }</summary>
	public void SetFile(string filename, string contents) => files.Add((filename, contents));

	public void SaveFiles(string directory)
	{
		for (var i = 0; i < files.Count; i++)
		{
			var (filename, contents) = files[i];

			// safe to write here
			ResponseBodyFilePaths.Add(Path.Combine(Path.GetTempPath(), filename));

			File.WriteAllText(ResponseBodyFilePaths[i], contents);

			// move if we can. if we can't then at least they persist in temp
			try
			{
				var dest = Path.Combine(directory, filename);
				File.Move(ResponseBodyFilePaths[i], dest);
				ResponseBodyFilePaths[i] = dest;
			}
			catch { }
		}
	}
}
