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
            VersionNumber vn = new VersionNumber(matchingversion);
            VersionNumber vn2= new VersionNumber(matchingversion);
            Assert.AreEqual(true,vn.MajorVersionMatches(vn2));
        }

        [Test]
        public void MetaInfo_IndicatesMajorVersionMismatch()
        {
            VersionNumber vn = new VersionNumber("2017.1.a.2");

            VersionNumber vn2 = new VersionNumber("2017.1.b.1");
            Assert.AreEqual(true, vn.MajorVersionMatches(vn2));
        }

    }
}
