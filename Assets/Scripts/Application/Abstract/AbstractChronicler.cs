using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Ghosts;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using Random = System.Random;


namespace SecretHistories.Abstract
{
    public abstract class AbstractChronicler: MonoBehaviour, ICharacterSubscriber
    {
        protected Character _chroniclingCharacter;
        protected Compendium _compendium;

        public void Awake()
        {
            var w = new Watchman();
            w.Register(this);

            _compendium = Watchman.Get<Compendium>();

        }
        public void ChronicleCharacter(Character characterToChronicle)
        {
            if (_chroniclingCharacter != null)
                _chroniclingCharacter.Unsubscribe(this);

            _chroniclingCharacter = characterToChronicle;
            characterToChronicle.Subscribe(this);

        }
        public abstract void TokenPlacedInWorld(Token token);
        public virtual void ChronicleGameEnd(List<Situation> situations, HashSet<Sphere> tokenContainers, Ending ending)
        {
            //a lot of the stuff in TokenPlacedOnTabletop might be better here, actually
            SetAchievementsForEnding(ending);

            List<Token> allStacksInGame = new List<Token>();


            foreach (var tc in tokenContainers)
            {
                allStacksInGame.AddRange(tc.GetElementTokens());
            }

            var rnd = new Random();

            allStacksInGame = allStacksInGame.OrderBy(x => rnd.Next()).ToList(); //shuffle them, to prevent eg most recent follower consistently showing up at top.

            ChronicleSpecificsForElementStacksAtGameEnd(allStacksInGame);

        }

        public abstract void SetAchievementsForEnding(Ending ending);

        public abstract void ChronicleSpecificsForElementStacksAtGameEnd(List<Token> elementTokens);

        public void CharacterNameUpdated(string newName)
        {
            _chroniclingCharacter.SetFutureLegacyEventRecord(LegacyEventRecordId.lastcharactername.ToString(), newName);

        }

        public void CharacterProfessionUpdated(string newProfession)
        {
            //
        }
        public abstract void ChronicleOtherworldEntry(string portal);
    }
}
