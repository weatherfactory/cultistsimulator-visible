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
    public class VersionNumberTests
    {
        [Test]
        public void MetaInfo_ReturnsYear()
        {
            VersionNumber mi=new VersionNumber("2017.1.a.2");
            Assert.AreEqual(2017,mi.GetVersionYear());

        }

        [Test]
        public void MetaInfo_ReturnsMonth()
        {
            VersionNumber mi = new VersionNumber("2017.1.a.2");
            Assert.AreEqual(1, mi.GetVersionMonth());

        }

        [Test]
        public void MetaInfo_ReturnsMinorVersion()
        {
            VersionNumber mi = new VersionNumber("2017.1.a.2");
            Assert.AreEqual('a', mi.GetVersionMinor());
        }

        [Test]
        public void MetaInfo_ReturnsBuildNumber()
        {
            VersionNumber mi = new VersionNumber("2017.1.a.2");
            Assert.AreEqual(2, mi.GetVersionBuildNumber());
        }

        [Test]
        public void MetaInfo_IndicatesMajorVersionMatch()
        {
            string matchingversion = "2017.1.a.2";
            VersionNumber mi = new VersionNumber(matchingversion);
            Assert.AreEqual(true,mi.MajorVersionMatches(matchingversion));
        }

        [Test]
        public void MetaInfo_IndicatesMajorVersionMismatch()
        {
            VersionNumber mi = new VersionNumber("2017.1.a.2");
            Assert.AreEqual(true, mi.MajorVersionMatches("2017.1.b.1"));
        }

    }
}
