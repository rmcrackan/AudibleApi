using System;
using BaseLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StringExtensionsTests
{
    [TestClass]
    public class EqualsInsensitive
    {
        [TestMethod]
        [DataRow(null, "")]
        [DataRow(null, "   ")]
        [DataRow("", "   ")]
        [DataRow("   ", "      ")]
        [DataRow("<>", "  <>  ")]
        public void should_fail(string str1, string str2) => Assert.IsFalse(str1.EqualsInsensitive(str2));

        [TestMethod]
        [DataRow(null, null)]
        [DataRow("", "")]
        [DataRow("   ", "   ")]
        [DataRow("hello world", "hello world")]
        [DataRow("hello world", "HELLO WORLD")]
        [DataRow("hello world", "Hello World")]
        [DataRow("hello world", "hELLo woRLd")]
        [DataRow("hello world<>!@#$%^&*()\"", "hELLo woRLd<>!@#$%^&*()\"")]
        public void should_pass(string str1, string str2) => Assert.IsTrue(str1.EqualsInsensitive(str2));
    }

    [TestClass]
    public class StartsWithInsensitive
    {
        [TestMethod]
        [DataRow(null, null)]
        [DataRow(null, "")]
        [DataRow(null, "   ")]
        public void should_throw_NullReferenceException(string fullString, string prefix)
            => Assert.ThrowsException<NullReferenceException>(() => fullString.StartsWithInsensitive(prefix));

        [TestMethod]
        [DataRow("", null)]
        [DataRow("   ", null)]
        public void should_throw_ArgumentNullException(string fullString, string prefix)
            => Assert.ThrowsException<ArgumentNullException>(() => fullString.StartsWithInsensitive(prefix));

        [TestMethod]
        [DataRow("", "   ")]
        [DataRow("  <>", "<>")]
        [DataRow("<>___", "<>  ")]
        [DataRow("<>___", "  <>")]
        public void should_fail(string fullString, string prefix) => Assert.IsFalse(fullString.StartsWithInsensitive(prefix));

        [TestMethod]
        [DataRow("   empty", "")]
        [DataRow("   3 spaces", "   ")]
        [DataRow("<>___", "<>")]
        [DataRow("hello world___", "hello world")]
        [DataRow("hello world___", "HELLO WORLD")]
        [DataRow("hello world___", "Hello World")]
        [DataRow("hello world___", "hELLo woRLd")]
        [DataRow("hello world<>!@#$%^&*()\"___", "hELLo woRLd<>!@#$%^&*()\"")]
        public void should_pass(string fullString, string prefix) => Assert.IsTrue(fullString.StartsWithInsensitive(prefix));
    }

    [TestClass]
    public class EndsWithInsensitive
    {
        [TestMethod]
        [DataRow(null, null)]
        [DataRow(null, "")]
        [DataRow(null, "   ")]
        public void should_throw_NullReferenceException(string fullString, string prefix)
            => Assert.ThrowsException<NullReferenceException>(() => fullString.EndsWithInsensitive(prefix));

        [TestMethod]
        [DataRow("", null)]
        [DataRow("   ", null)]
        public void should_throw_ArgumentNullException(string fullString, string prefix)
            => Assert.ThrowsException<ArgumentNullException>(() => fullString.EndsWithInsensitive(prefix));

        [TestMethod]
        [DataRow("", "   ")]
        [DataRow("<>  ", "<>")]
        [DataRow("___<>", "<>  ")]
        [DataRow("___<>", "  <>")]
        public void should_fail(string fullString, string suffix) => Assert.IsFalse(fullString.EndsWithInsensitive(suffix));

        [TestMethod]
        [DataRow("empty   ", "")]
        [DataRow("3 spaces   ", "   ")]
        [DataRow("___<>", "<>")]
        [DataRow("___hello world", "hello world")]
        [DataRow("___hello world", "HELLO WORLD")]
        [DataRow("___hello world", "Hello World")]
        [DataRow("___hello world", "hELLo woRLd")]
        [DataRow("___hello world<>!@#$%^&*()\"", "hELLo woRLd<>!@#$%^&*()\"")]
        public void should_pass(string fullString, string suffix) => Assert.IsTrue(fullString.EndsWithInsensitive(suffix));
    }

    [TestClass]
    public class ContainsInsensitive
    {
        [TestMethod]
        [DataRow("", null)]
        [DataRow("   ", null)]
        public void should_throw_ArgumentNullException(string fullString, string needle)
            => Assert.ThrowsException<ArgumentNullException>(() => fullString.ContainsInsensitive(needle));

        [TestMethod]
        [DataRow(null, null)]
        [DataRow(null, "")]
        [DataRow(null, "   ")]
        [DataRow("", "   ")]
        [DataRow("<>", "  <>  ")]
        [DataRow("  <>", "  <>  ")]
        [DataRow("<>  ", "  <>  ")]
        public void should_fail(string fullString, string needle) => Assert.IsFalse(fullString.ContainsInsensitive(needle));

        [TestMethod]
        [DataRow("empty", "")]
        [DataRow("3   spaces", "")]
        [DataRow("___<>___", "<>")]
        [DataRow("___hello world___", "hello world")]
        [DataRow("___hello world___", "HELLO WORLD")]
        [DataRow("___hello world___", "Hello World")]
        [DataRow("___hello world___", "hELLo woRLd")]
        [DataRow("___hello world<>!@#$%^&*()\"___", "hELLo woRLd<>!@#$%^&*()\"")]
        public void should_pass(string fullString, string needle) => Assert.IsTrue(fullString.ContainsInsensitive(needle));
    }
}
