using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using TestCommon;

namespace JavascriptExamples
{
    // nuget jint: https://github.com/sebastienros/jint
    [TestClass]
    public class scratchpad
    {
        const string js = @"
function intadd(x, y) { return x + y; }
function stradd(x, y) { return x + "" "" + y; }
function listadd(x) { list(x); }
";

        [TestMethod]
        public void int_add()
        {
            var engine = new Jint.Engine();

            // load js
            engine.Execute(js);
            var answer = engine
                .Execute("intadd(1,2);")
                .GetCompletionValue()
                .ToObject()
                .ToString();
            answer.Should().Be("3");
        }

        [TestMethod]
        public void string_add()
        {
            var engine = new Jint.Engine();

            // load js
            engine.Execute(js);
            var answer = engine
                .Execute("stradd(1,2);")
                .GetCompletionValue()
                .ToObject()
                .ToString();
            answer.Should().Be("1 2");
        }

        [TestMethod]
        public void log()
        {
            var list = new List<object>();

            var engine = new Jint.Engine()
                .SetValue("log", new Action<object>(Console.WriteLine))
                .SetValue("list", new Action<object>(list.Add));
            engine.Execute(@"
                function hello() { 
                  log('Hello World');
                  list('Hello World');
                };
                hello();
            ");
            list.Count.Should().Be(1);
            list.Single().ToString().Should().Be("Hello World");

            engine.Execute(js);
            engine.Execute("listadd('two')");
            list.Count.Should().Be(2);
            list[1].ToString().Should().Be("two");
        }
    }
}
