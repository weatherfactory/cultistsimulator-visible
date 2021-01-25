using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Commands;
using SecretHistories.UI;

namespace Assets.Tests.UnitTests
{
    [TestFixture]
    public class ElementStackCreationCommandEncaustmentTests
    {

        [Test]
        public void ElementStackEncaustedToCommand()
        {
            var encaustery=new Encaustery();
            var elementStack=new ElementStack();
            encaustery.EncaustTo<ElementStackCreationCommand>(elementStack);
        }
    }
}
