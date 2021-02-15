using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.UI;

namespace Assets.Tests.PlayModeTests
{
    [TestFixture]
   public class NoteElementTests
    {
        [Test]
        public void GenerateSituation()
        {
            Verb verb = Watchman.Get<Compendium>().GetEntityById<Verb>("work");
            var sc = new SituationCreationCommand(verb.Id, NullRecipe.Create(verb).Id, new SituationPath(verb.Id),
                StateEnum.Unstarted);
            var tc = new TokenCreationCommand(sc, TokenLocation.Default());
            tc.Execute(Context.Unknown());

        }
    }
}
