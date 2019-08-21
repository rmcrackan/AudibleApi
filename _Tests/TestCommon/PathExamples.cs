using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BaseLib;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestCommon;

namespace PathExamples
{
	[TestClass]
	public class SpecialFolders
	{
		[TestMethod]
		public void strongly_typed()
		{
			// get actual system dir:
			// eg: C:\Users\username\AppData\Roaming
			var appDataDir = Environment.ExpandEnvironmentVariables("%appdata%");
			Directory.Exists(appDataDir).Should().BeTrue();
			appDataDir.Should().NotEndWith("\\");


			Environment
				.GetFolderPath(Environment.SpecialFolder.ApplicationData)
				.Should().Be(appDataDir);
		}

		// windows environment variables. can use in %variable% format
		// https://ss64.com/nt/syntax-variables.html

		// official full list
		// https://docs.microsoft.com/en-us/windows/deployment/usmt/usmt-recognized-environment-variables

		[TestMethod]
		public void from_string()
		{
			// get actual system dirs:
			// eg: C:\Users\username\AppData\Roaming
			var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			Directory.Exists(appDataDir).Should().BeTrue();
			appDataDir.Should().NotEndWith("\\");

			// eg: C:\Program Files
			var programFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			Directory.Exists(programFilesDir).Should().BeTrue();
			programFilesDir.Should().NotEndWith("\\");


			Environment
				.ExpandEnvironmentVariables(@"%AppData%\stuff")
				.Should().Be(appDataDir + @"\stuff");

			Environment
				.ExpandEnvironmentVariables(@"%aPpdAtA%\HelloWorld")
				.Should().Be(appDataDir + @"\HelloWorld");

			// collection of paths
			Environment
				.ExpandEnvironmentVariables(@"%progRAMfiLES%\Adobe;%appdata%\FileZilla")
				.Should().Be($@"{programFilesDir}\Adobe;{appDataDir}\FileZilla");
		}
	}

	[TestClass]
	public class relative_path
	{
		string full = @"C:\d1\d2\d3\d4\d5\d6\d7";
		string up3 = @"C:\d1\d2\d3\d4";

		[TestMethod]
		public void method1()
		{
			var path = Path.Combine(full, @"..\..\..");
			var final = Path.GetFullPath(path);
			final.Should().Be(up3);
		}

		[TestMethod]
		public void method2()
		{
			var path = Path.Combine(full, @"..\..\..");
			var final = new DirectoryInfo(path).FullName;
			final.Should().Be(up3);
		}
	}
}
