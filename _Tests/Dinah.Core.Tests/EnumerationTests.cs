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
using TestCommon;

namespace EnumerationTests
{
    public abstract class SubClassing : Enumeration
    {
        // these may be fields or properties
        public static readonly SubClassing Manager = new ManagerType();
        public static SubClassing Servant { get; } = new ServantType();

        private SubClassing(int value, string displayName) : base(value, displayName) { }

        public abstract decimal BonusSize { get; }

        private class ManagerType : SubClassing
        {
            public ManagerType() : base(0, "Manager") { }
            public override decimal BonusSize => 1000m;
        }

        private class ServantType : SubClassing
        {
            public ServantType() : base(1, "Servant") { }
            public override decimal BonusSize => 0m;
        }
    }

    [TestClass]
    public class ctor
    {
        [TestMethod]
        public void Value_is_stored()
            => SubClassing.Manager.Value.Should().Be(0);

        [TestMethod]
        public void DisplayName_is_stored()
            => SubClassing.Manager.DisplayName.Should().Be("Manager");
    }

    [TestClass]
    public class ToString
    {
        [TestMethod]
        public void outputs_DisplayName()
            => SubClassing.Manager.ToString().Should().Be("Manager");
    }

    [TestClass]
    public class GetAll_T_
    {
        [TestMethod]
        public void verify_all_values()
        {
            var all = Enumeration.GetAll<SubClassing>();
            all.Count().Should().Be(2);
            all.Any(a => a.Value == 0).Should().BeTrue();
            all.Any(a => a.Value == 1).Should().BeTrue();
        }
    }

    [TestClass]
    public class Equals
    {
        [TestMethod]
        public void instances_are_equal()
        {
            var manager1 = SubClassing.Manager;
            var manager2 = SubClassing.Manager;
            Assert.AreEqual(manager1, manager2);
        }
    }

    [TestClass]
    public class FromValue_T_
    {
        [TestMethod]
        public void get_manager()
            => Enumeration.FromValue<SubClassing>(0)
            .Should().Be(SubClassing.Manager);
    }

    [TestClass]
    public class FromDisplayName_T_
    {
        [TestMethod]
        public void get_manager()
            => Enumeration.FromDisplayName<SubClassing>("Manager")
            .Should().Be(SubClassing.Manager);
    }

    [TestClass]
    public class CompareTo
    {
        [TestMethod]
        public void compare_is_equal()
            => SubClassing.Manager
            .CompareTo(SubClassing.Manager)
            .Should().Be(0);
    }
}
