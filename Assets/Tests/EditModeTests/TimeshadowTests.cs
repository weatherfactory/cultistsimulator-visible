using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Logic;

namespace Assets.Tests.EditModeTests
{
    [TestFixture]
   public class TimeshadowTests
    {
        [Test]
        public void SpendTime_ReducesTimeRemaining()
        {
            var ts=new Timeshadow(10,9,false);
            ts.SpendTime(5);
            Assert.AreEqual(4,ts.LifetimeRemaining);
        }

        [Test]
        public void LifeTimeRemainingIsMinimumZero()
        {
            var ts = new Timeshadow(10, 9, false);
            ts.SpendTime(90);
            Assert.AreEqual(0, ts.LifetimeRemaining);
        }

        [Test]
        public void LifetimeSpent_IsLifetime_MinusLifetimeRemaining()
        {
            var ts = new Timeshadow(10, 3, false);
            Assert.AreEqual(7,ts.LifetimeSpent);
        }
    }
}
