using System;
using System.IO;

namespace Dinah.Core
{
    public static class SystemIOExtensions
    {
        public static string MD5(this FileInfo fileInfo)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            using (var stream = fileInfo.Create())
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
