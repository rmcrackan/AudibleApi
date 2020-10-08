using System;
using System.Collections.Generic;
using System.IO;

namespace AudibleApi.Authentication
{
    // do not derive from ApiErrorException. those are errors returned by API calls. Login is pre-API calls
    public class LoginFailedException : Exception
    {
        public string RequestUrl { get; set; }
        public System.Net.HttpStatusCode ResponseStatusCode { get; set; }
        public Dictionary<string, string> ResponseInputFields { get; set; }
        public string ResponseBodyFilePath { get; private set; }

        public LoginFailedException() : base() { }
        public LoginFailedException(string message) : base(message) { }
        public LoginFailedException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>response body is potentially huge; don't want it poluting logs. write to temp file</summary>
        public void SaveResponseBodyFile(string responseBody, string filename)
        {
            var responseBodyFilePath = Path.Combine(Path.GetTempPath(), filename);
			File.WriteAllText(responseBodyFilePath, responseBody);
            ResponseBodyFilePath = responseBodyFilePath;
        }

        public void MoveResponseBodyFile(string newDirectory)
        {
            if (string.IsNullOrWhiteSpace(ResponseBodyFilePath) || !File.Exists(ResponseBodyFilePath) || !Directory.Exists(newDirectory))
                return;

            var dest = Path.Combine(newDirectory, Path.GetFileName(ResponseBodyFilePath));
			File.Move(ResponseBodyFilePath, dest);
            ResponseBodyFilePath = dest;
        }
    }
}
