using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace SystemIOExtensionsTests
{
    [TestClass]
    public class MD5
    {
        [TestMethod]
        public void verify_hash()
        {
            var temp = Path.GetTempFileName();
            try
            {
                // base 64 of a txt file with "test"
                var base64 = "dGVzdA==";
                var bytes = Convert.FromBase64String(base64);
                File.WriteAllBytes(temp, bytes);

                var info = new FileInfo(temp);
                var md5 = info.MD5();
                md5.Should().Be("d41d8cd98f00b204e9800998ecf8427e");
            }
            finally
            {
                File.Delete(temp);
            }
        }
    }
}
