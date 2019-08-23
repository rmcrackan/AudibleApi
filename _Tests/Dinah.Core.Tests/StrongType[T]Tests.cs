using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core;
using FluentAssertions;

namespace StrongTypeTests
{
    [TestClass]
    public class equality
    {
        [TestMethod]
        public void null_value() => eq<string>(null, null, true);

        [TestMethod]
        [DataRow("1", 1)]
        [DataRow("", 1)]
        [DataRow("   ", 1)]
        [DataRow("Hello World", 1)]
        public void should_fail_different_types(string first, object second) => eq(first, second);

        [TestMethod]
        [DataRow("", null)]
        [DataRow("   ", null)]
        [DataRow("Hello World", null)]
        [DataRow("", "   ")]
        [DataRow("", "Hello World")]
        [DataRow("   ", "")]
        [DataRow("   ", "Hello World")]
        [DataRow("Hello World", "")]
        [DataRow("Hello World", "   ")]
        public void should_fail_unequal_string(string first, string second) => eq(first, second);
        [TestMethod]
        [DataRow(-1, 0)]
        [DataRow(-1, 1)]
        [DataRow(-1, int.MinValue)]
        [DataRow(-1, int.MaxValue)]
        [DataRow(0, 1)]
        [DataRow(0, int.MinValue)]
        [DataRow(0, int.MaxValue)]
        [DataRow(1, int.MinValue)]
        [DataRow(2, int.MaxValue)]
        [DataRow(int.MinValue, int.MaxValue)]
        public void should_fail_unequal_int(int first, int second) => eq(first, second);

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("Hello World")]
        public void should_pass_string(string value) => eq(value);

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void should_pass_int(int value) => eq(value);

        static void eq<T>(T t) => eq(t, t);
        static void eq<T>(T t1, T t2) => eq(t1, t2, t1.Equals(t2));
        static void eq<T>(T t1, T t2, bool isSame)
        {
            compare_strong_type_to_value(t1, t2, isSame);

            compare_2_strong_types(t1, t2, isSame);

            compare_Value(t1, t2, isSame);

            if (isSame)
            {
                test_ToString(t1, t2);
                test_GetHashCode(t1, t2);
            }
        }

        private static void compare_strong_type_to_value<T>(T t1, T t2, bool isSame)
        {
            var strong1 = new StrongType<T>(t1);
            var strong2 = new StrongType<T>(t2);

            // compare value (left) to strong type (right)
            Assert.AreEqual(isSame, t1 == strong2);
            Assert.AreEqual(!isSame, t1 != strong2);

            // compare strong type (left) to value (right)
            Assert.AreEqual(isSame, strong1.Equals(t2));
            Assert.AreEqual(isSame, strong1 == t2);
            Assert.AreEqual(!isSame, strong1 != t2);
        }

        private static void compare_2_strong_types<T>(T t1, T t2, bool isSame)
        {
            var strong1 = new StrongType<T>(t1);
            var strong2 = new StrongType<T>(t2);

            // compare 2 strong types
            Assert.AreEqual(isSame, strong1.Equals(strong2));
            Assert.AreEqual(isSame, strong1 == strong2);
            Assert.AreEqual(!isSame, strong1 != strong2);
            // compare 2 strong types, order reversed
            Assert.AreEqual(isSame, strong2.Equals(strong1));
            Assert.AreEqual(isSame, strong2 == strong1);
            Assert.AreEqual(!isSame, strong2 != strong1);
        }

        private static void compare_Value<T>(T t1, T t2, bool isSame)
        {
            var strong1 = new StrongType<T>(t1);
            var strong2 = new StrongType<T>(t2);

            if (strong1.Value != null)
            {
                Assert.AreEqual(isSame, strong1.Value.Equals(t2));
                Assert.AreEqual(isSame, strong1.Value.Equals(strong2.Value));
            }
            Assert.AreEqual(isSame, strong1.Equals(strong2.Value));
        }

        private static void test_ToString<T>(T t1, T t2)
        {
            var strong1 = new StrongType<T>(t1);
            var strong2 = new StrongType<T>(t2);

            Assert.AreEqual(t1?.ToString(), strong2?.ToString());
            Assert.AreEqual(t1?.ToString(), strong2.Value?.ToString());

            Assert.AreEqual(strong1?.ToString(), t2?.ToString());
            Assert.AreEqual(strong1?.ToString(), strong2?.ToString());
            Assert.AreEqual(strong1?.ToString(), strong2.Value?.ToString());

            Assert.AreEqual(strong1.Value?.ToString(), t2?.ToString());
            Assert.AreEqual(strong1.Value?.ToString(), strong2?.ToString());
            Assert.AreEqual(strong1.Value?.ToString(), strong2.Value?.ToString());
        }

        private static void test_GetHashCode<T>(T t1, T t2)
        {
            var strong1 = new StrongType<T>(t1);
            var strong2 = new StrongType<T>(t2);

            if (t1 != null)
            {
                Assert.AreEqual(t1.GetHashCode(), strong2.GetHashCode());
                Assert.AreEqual(t1.GetHashCode(), strong2.Value.GetHashCode());
            }

            if (t2 != null)
            {
                Assert.AreEqual(strong1.GetHashCode(), t2.GetHashCode());
                Assert.AreEqual(strong1.Value.GetHashCode(), t2.GetHashCode());
            }

            Assert.AreEqual(strong1.GetHashCode(), strong2.GetHashCode());
            if (strong1.Value != null)
                Assert.AreEqual(strong1.Value.GetHashCode(), strong2.GetHashCode());
            if (strong2.Value != null)
                Assert.AreEqual(strong1.GetHashCode(), strong2.Value.GetHashCode());
            if (strong1.Value != null && strong2.Value != null)
                Assert.AreEqual(strong1.Value.GetHashCode(), strong2.Value.GetHashCode());
        }
    }

    class Strong1 : StrongType<string>
    {
        public Strong1(string value) : base(value) { }
    }
    class Strong2 : StrongType<string>
    {
        public Strong2(string value) : base(value) { }
    }

    [TestClass]
    public class cross_class_compare
    {
        const string STRING_VALUE = "value";

        [TestMethod]
        public void equality()
        {
            var strong1 = new Strong1(STRING_VALUE);
            var strong2 = new Strong2(STRING_VALUE);

            Assert.IsTrue(strong1.Equals(strong2));
            Assert.IsTrue(strong1 == strong2);
        }

        [TestMethod]
        public void param_passing()
        {
            acceptsString(STRING_VALUE);
            acceptsString(new Strong1(STRING_VALUE));
            acceptsString(new Strong2(STRING_VALUE));

            acceptsStrong1(new Strong1(STRING_VALUE));
            // won't compile:
            //   acceptsStrong1(STRING_VALUE);
            //   acceptsStrong1(new Strong2(STRING_VALUE));
        }

        void acceptsString(string s1) { }
        void acceptsStrong1(Strong1 s1) { }
    }

	[TestClass]
	public class ValidateInput
	{
		class noValidationStrongString : StrongType<string>
		{
			public noValidationStrongString(string value)
				: base(value) { }
		}
		[TestMethod]
		public void no_validation_passes()
		{
			var nv = new noValidationStrongString(null);
			nv.Value.Should().BeNull();
		}

		class withValidationStrongString : StrongType<string>
		{
			public withValidationStrongString(string value)
				: base(value) { }
			protected override void ValidateInput(string value)
			{
				if (value is null)
					throw new ArgumentNullException();
			}
		}

		[TestMethod]
		public void validation_failure()
			=> Assert.ThrowsException<ArgumentNullException>(() => new withValidationStrongString(null));

		[TestMethod]
		public void validation_success()
		{
			var wv = new withValidationStrongString("");
			wv.Value.Should().Be("");
		}
	}
}
