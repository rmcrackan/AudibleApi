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

namespace Enumeration_T_Tests
{
    public abstract class SubClassing : Enumeration<SubClassing>
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
            => SubClassing.Manager.Id.Should().Be(0);
    }

    [TestClass]
    public class GetAll
    {
        [TestMethod]
        public void verify_all_values()
        {
            var all = Enumeration<SubClassing>.GetAll();
            all.Count().Should().Be(2);
            all.Any(a => a.Value == 0).Should().BeTrue();
            all.Any(a => a.Value == 1).Should().BeTrue();
        }
    }

    [TestClass]
    public class FromValue
    {
        [TestMethod]
        public void get_manager()
            => Enumeration<SubClassing>.FromValue(0)
            .Should().Be(SubClassing.Manager);
    }

    [TestClass]
    public class FromDisplayName
    {
        [TestMethod]
        public void get_manager()
            => Enumeration<SubClassing>.FromDisplayName("Manager")
            .Should().Be(SubClassing.Manager);
    }
}
