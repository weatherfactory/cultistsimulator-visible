using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using NUnit.Framework;

namespace CS.Tests
{
    [TestFixture]
    public class MetaInfoTests
    {
        [Test]
        public void MetaInfo_ReturnsYear()
        {
            MetaInfo mi=new MetaInfo("2017.1.a.2");
            Assert.AreEqual(2017,mi.GetVersionYear());

        }

        [Test]
        public void MetaInfo_ReturnsMonth()
        {
            MetaInfo mi = new MetaInfo("2017.1.a.2");
            Assert.AreEqual(1, mi.GetVersionMonth());

        }

        [Test]
        public void MetaInfo_ReturnsMinorVersion()
        {
            MetaInfo mi = new MetaInfo("2017.1.a.2");
            Assert.AreEqual('a', mi.GetVersionMinor());
        }

        [Test]
        public void MetaInfo_ReturnsBuildNumber()
        {
            MetaInfo mi = new MetaInfo("2017.1.a.2");
            Assert.AreEqual(2, mi.GetVersionBuildNumber());
        }


    }
}
