using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core;
using static ValueObjectTests.BaseClasses;

namespace ValueObjectTests
{
    public class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public int StateID { get; }
        public string ZipCode { get; }

        public Address(string street, string city, int stateID, string zipCode)
        {
            Street = street;
            City = city;
            StateID = stateID;
            ZipCode = zipCode;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return StateID;
            yield return ZipCode;
        }
    }

    public class BaseClasses
    {
        public static Address Address0 = new Address("123 Main Street", "New York", 1, "01010");

        public static Address AddressMatch = new Address("123 Main Street", "New York", 1, "01010");

        public static List<Address> AddressesFail = new List<Address> {
            null,
            new Address("123 Main Street", "New York", 1, "98989"),
            new Address("123 Main Street", "New York", 9, "01010"),
            new Address("123 Main Street", "New York", 9, "98989"),
            new Address("123 Main Street", "Chicago", 1, "01010"),
            new Address("123 Main Street", "Chicago", 1, "98989"),
            new Address("123 Main Street", "Chicago", 9, "01010"),
            new Address("123 Main Street", "Chicago", 9, "98989"),
            new Address("987 E West", "New York", 1, "01010"),
            new Address("987 E West", "New York", 1, "98989"),
            new Address("987 E West", "New York", 9, "01010"),
            new Address("987 E West", "New York", 9, "98989"),
            new Address("987 E West", "Chicago", 1, "01010"),
            new Address("987 E West", "Chicago", 1, "98989"),
            new Address("987 E West", "Chicago", 9, "01010"),
            new Address("987 E West", "Chicago", 9, "98989")
        };
    }

    [TestClass]
    public class Equals_method
    {
        [TestMethod]
        public void should_fail()
        {
            foreach (var testAddr in AddressesFail)
            {
                Assert.IsFalse(Address0.Equals(testAddr));
                if (testAddr != null)
                {
                    Assert.IsFalse(testAddr.Equals(Address0));
                }
            }
        }

        [TestMethod]
        public void should_pass()
        {
            Assert.IsTrue(Address0.Equals(AddressMatch));
            Assert.IsTrue(AddressMatch.Equals(Address0));
        }
    }

    [TestClass]
    public class Equals_operator
    {
        [TestMethod]
        public void should_fail()
        {
            foreach (var testAddr in AddressesFail)
            {
                Assert.IsFalse(Address0 == testAddr);
                Assert.IsFalse(testAddr == Address0);
            }
        }

        [TestMethod]
        public void should_pass()
        {
            Assert.IsTrue(Address0 == AddressMatch);
            Assert.IsTrue(AddressMatch == Address0);
        }
    }

    [TestClass]
    public class NotEquals_operator
    {
        [TestMethod]
        public void should_fail()
        {
            Assert.IsFalse(Address0 != AddressMatch);
            Assert.IsFalse(AddressMatch != Address0);
        }

        [TestMethod]
        public void should_pass()
        {
            foreach (var testAddr in AddressesFail)
            {
                Assert.IsTrue(Address0 != testAddr);
                Assert.IsTrue(testAddr != Address0);
            }
        }
    }

    [TestClass]
    public class GetHashCode
    {
        [TestMethod]
        public void should_throw_NullReferenceException()
        {
            foreach (var testAddr in AddressesFail.Where(a => a == null))
                Assert.ThrowsException<NullReferenceException>(() => Address0.GetHashCode() == testAddr.GetHashCode());
        }

        [TestMethod]
        public void should_fail()
        {
            foreach (var testAddr in AddressesFail.Where(a => a != null))
                Assert.IsFalse(Address0.GetHashCode() == testAddr.GetHashCode());
        }

        [TestMethod]
        public void should_pass() => Assert.AreEqual(Address0.GetHashCode(), AddressMatch.GetHashCode());
    }
}
