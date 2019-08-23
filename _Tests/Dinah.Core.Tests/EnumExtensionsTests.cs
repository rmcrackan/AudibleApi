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
using Newtonsoft.Json.Linq;
using TestCommon;

namespace EnumExtensionsTests
{
	public struct NonEnum : IConvertible
	{
		public TypeCode GetTypeCode()
		{
			throw new NotImplementedException();
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public byte ToByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public char ToChar(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public decimal ToDecimal(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public double ToDouble(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public short ToInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public int ToInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public long ToInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public sbyte ToSByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public float ToSingle(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public object ToType(Type conversionType, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ushort ToUInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public uint ToUInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ulong ToUInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}
	}

	public enum ByteEnum : byte
	{
		None,
		Plop,
		Pouet,
		[System.ComponentModel.Description("foo")]
		Foo,
		Bar
	}

	public enum IntEnum
	{
		None,
		Plop,
		Pouet,
		[System.ComponentModel.Description("foo")]
		Foo,
		Bar
	}

	[Flags]
	public enum FlagsEnum
	{
		None = 0,
		Plop = 1,
		Pouet = 2,
		[System.ComponentModel.Description("foo")]
		Foo = 4,
		Bar = 8
	}

	[TestClass]
	public class ToValues
	{
		[TestMethod]
		public void non_enum_throws()
			=> Assert.ThrowsException<ArgumentException>(() => new NonEnum().ToValues().ToArray());

		[TestMethod]
		public void byte_enum_throws()
			=> Assert.ThrowsException<InvalidCastException>(() => ByteEnum.Bar.ToValues().ToArray());

		[TestMethod]
		public void non_flag_gives_unexpected_results()
		{
			var one = IntEnum.Foo;
			var oneValue = one.ToValues().ToArray();

			// Foo == decimal 3 == binary 11
			// binary 11 matches binary 01, 10, 11
			var oneExpected = new IntEnum[] { IntEnum.Plop, IntEnum.Pouet, IntEnum.Foo };
			oneValue.Should().BeEquivalentTo(oneExpected);

			var two = IntEnum.Foo | IntEnum.Bar;
			var twoValue = two.ToValues().ToArray();
			var twoExpected = new IntEnum[] { IntEnum.Plop, IntEnum.Pouet, IntEnum.Foo, IntEnum.Bar };
			twoValue.Should().BeEquivalentTo(twoExpected);
		}

		[TestMethod]
		public void enumerate_flags()
		{
			var flags = FlagsEnum.None | FlagsEnum.Plop | FlagsEnum.Foo;
			var array = flags.ToValues().ToArray();

			var expectedArray = new[] { FlagsEnum.Plop, FlagsEnum.Foo };
			array.Should().BeEquivalentTo(expectedArray);
		}
	}

	[TestClass]
	public class GetDescription
	{
		[TestMethod]
		public void non_enum_is_null()
			=> new NonEnum().GetDescription().Should().BeNull();

		[TestMethod]
		public void non_described_byte_enum_is_null()
			=> ByteEnum.Bar.GetDescription().Should().BeNull();

		[TestMethod]
		public void described_byte_enum_is_foo()
			=> ByteEnum.Foo.GetDescription().Should().Be("foo");

		[TestMethod]
		public void non_described_int_enum_is_null()
			=> IntEnum.Bar.GetDescription().Should().BeNull();

		[TestMethod]
		public void described_int_enum_is_foo()
			=> IntEnum.Foo.GetDescription().Should().Be("foo");

		[TestMethod]
		public void non_described_flag_enum_is_null()
			=> FlagsEnum.Bar.GetDescription().Should().BeNull();

		[TestMethod]
		public void described_flag_enum_is_foo()
			=> FlagsEnum.Foo.GetDescription().Should().Be("foo");

		[TestMethod]
		public void mult_flag_enums_is_null()
			=> (FlagsEnum.Bar | FlagsEnum.Foo).GetDescription().Should().BeNull();
	}
}
