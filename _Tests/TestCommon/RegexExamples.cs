using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestCommon;

namespace RegexExamples
{
	[TestClass]
	public class Replace
	{
		[TestMethod]
		public void case_insensitive_replace()
		{
			var input = @"%DeskToP%\path\file.txt";
			// these characters are literals, not patterns
			var pattern = Regex.Escape("%desktop%");
			var replacement = @"C:\User\whatever";

			Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase)
				.Should().Be(@"C:\User\whatever\path\file.txt");
		}
	}
}
