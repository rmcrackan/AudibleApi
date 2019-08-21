using System;
using System.Collections.Generic;
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
using TestCommon;

namespace HtmlHelperTests
{
    [TestClass]
    public class GetInputs
    {
        [TestMethod]
        public void null_param_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => HtmlHelper.GetInputs(null));

        string basicHidden => "<input type='hidden' name='foo' value='bar' />";
        string basicCb => "<input type='checkbox' name='cbFoo' value='cbBar' />";

        [TestMethod]
        public void empty()
            => HtmlHelper.GetInputs("").Count.Should().Be(0);

        [TestMethod]
        public void no_match()
            => HtmlHelper.GetInputs("<p></p>").Count.Should().Be(0);

        [TestMethod]
        public void match_hidden()
        {
            var inputs = HtmlHelper.GetInputs(basicHidden);
            inputs.Count.Should().Be(1);
            inputs.ContainsKey("foo").Should().BeTrue();
            inputs["foo"].Should().Be("bar");
        }

        [TestMethod]
        public void match_checkbox()
        {
            var inputs = HtmlHelper.GetInputs(basicCb);
            inputs.Count.Should().Be(1);
            inputs.ContainsKey("cbFoo").Should().BeTrue();
            inputs["cbFoo"].Should().Be("cbBar");
        }

        [TestMethod]
        public void match_hidden_and_checkbox()
        {
            var both = basicHidden + basicCb;

            var inputs = HtmlHelper.GetInputs(both);

            inputs.Count.Should().Be(2);

            inputs.ContainsKey("foo").Should().BeTrue();
            inputs["foo"].Should().Be("bar");

            inputs.ContainsKey("cbFoo").Should().BeTrue();
            inputs["cbFoo"].Should().Be("cbBar");
        }

        [TestMethod]
        public void find_1_top_level()
        {
            var inputs = HtmlHelper.GetInputs(basicHidden);
            inputs.Count.Should().Be(1);
            inputs.ContainsKey("foo").Should().BeTrue();
            inputs["foo"].Should().Be("bar");
        }

        [TestMethod]
        public void find_1_nest_1()
        {
            var html = $"<p>{basicHidden}</p>";
            var inputs = HtmlHelper.GetInputs(html);
            inputs.Count.Should().Be(1);
            inputs.ContainsKey("foo").Should().BeTrue();
            inputs["foo"].Should().Be("bar");
        }

        [TestMethod]
        public void find_1_nest_2()
        {
            var html = $"<p><p>{basicHidden}</p></p>";
            var inputs = HtmlHelper.GetInputs(html);
            inputs.Count.Should().Be(1);
            inputs.ContainsKey("foo").Should().BeTrue();
            inputs["foo"].Should().Be("bar");
        }

        [TestMethod]
        public void find_1_nest_3()
        {
            var html = $"<p><p><p>{basicHidden}</p></p></p>";
            var inputs = HtmlHelper.GetInputs(html);
            inputs.Count.Should().Be(1);
            inputs.ContainsKey("foo").Should().BeTrue();
            inputs["foo"].Should().Be("bar");
        }

        [TestMethod]
        public void find_1_invalid_nest1()
        {
            var html = $"<p><p><p>{basicHidden}</p></p>";
            var inputs = HtmlHelper.GetInputs(html);
            inputs.Count.Should().Be(1);
            inputs.ContainsKey("foo").Should().BeTrue();
            inputs["foo"].Should().Be("bar");
        }

        [TestMethod]
        public void find_1_invalid_nest2()
        {
            var html = $"<p><p>{basicHidden}</p></p></p>";
            var inputs = HtmlHelper.GetInputs(html);
            inputs.Count.Should().Be(1);
            inputs.ContainsKey("foo").Should().BeTrue();
            inputs["foo"].Should().Be("bar");
        }

        [TestMethod]
        public void dont_capture_null()
        {
            var html = @"<input type='hidden' value='foo' />";
            var inputs = HtmlHelper.GetInputs(html);
            inputs.Count.Should().Be(0);
        }

        [TestMethod]
        public void dont_capture_empty()
        {
            var html = @"<input type='hidden' name='' value='foo' />";
            var inputs = HtmlHelper.GetInputs(html);
            inputs.Count.Should().Be(0);
        }

        [TestMethod]
        public void dont_capture_blank()
        {
            var html = @"<input type='hidden' name='   ' value='foo' />";
            var inputs = HtmlHelper.GetInputs(html);
            inputs.Count.Should().Be(0);
        }

        [TestMethod]
        public void find_many()
        {
            var html = @"
<p>
  <p>
    <input type='hidden' name='h1' value='hv1' />
    <input type='hidden' name='h2' value='hv2' />
  </p>
</p>
<p>
  <input type='checkbox' name='cb1' value='cbv1' />
  <input type='checkbox' name='cb2' value='cbv2' />
</p>
  <input type='email' name='e1' value='a@b.xyz' />

  don't capture null: <input type='hidden' value='foo' />
  don't capture empty: <input type='hidden' name='' value='foo' />
";

            var inputs = HtmlHelper.GetInputs(html);

            inputs.Count.Should().Be(5);

            inputs.ContainsKey("h1").Should().BeTrue();
            inputs["h1"].Should().Be("hv1");

            inputs.ContainsKey("h2").Should().BeTrue();
            inputs["h2"].Should().Be("hv2");

            inputs.ContainsKey("cb1").Should().BeTrue();
            inputs["cb1"].Should().Be("cbv1");

            inputs.ContainsKey("cb2").Should().BeTrue();
            inputs["cb2"].Should().Be("cbv2");

            inputs.ContainsKey("e1").Should().BeTrue();
            inputs["e1"].Should().Be("a@b.xyz");
        }
    }
}
