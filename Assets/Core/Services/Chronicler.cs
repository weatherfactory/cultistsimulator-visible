using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Noon;

namespace Assets.Core.Services
{
    /// <summary>
    /// meta responses to significant in game events
    /// </summary>
    
    
    public class Chronicler
    {
        private IGameEntityStorage _storage;
        private ICompendium _compendium;
        private const string BOOK_ASPECT = "text";
        private const string DESIRE_ASPECT = "desire";
        private const string TOOL_ASPECT = "tool";
        private const string CULT_ASPECT = "cult";
        private const string HQ_ASPECT = "hq";
        private const string POWER_ASPECT="powermarks";
        private const string SENSATION_ASPECT="sensationmarks";
        private const string ENLIGHTENMENT_ASPECT = "enlightenmentmarks";
        private const string EXALTED_ASPECT = "exalted";
        private const string DISCIPLE_ASPECT = "disciple";
        private const string FOLLOWER_ASPECT = "follower";
        private const string MORTAL_ASPECT = "mortal";




        public Chronicler(IGameEntityStorage storage,ICompendium compendium)
        {
            _storage = storage;
            _compendium = compendium;
        }

        public void CharacterNameChanged(string newName)
        {
            _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastCharacterName,newName);
        }

        public void TokenPlacedOnTabletop(ElementStackToken token)
        {
            IAspectsDictionary tokenAspects = token.GetAspects();

            
            TryUpdateBookLever(token, tokenAspects);

            TryUpdateDesireLever(tokenAspects);

            TryUpdateToolLever(token, tokenAspects);
            
            TryUpdateCultLever(token, tokenAspects);
            
            TryUpdateHqLever(token, tokenAspects);

            

        }

        public void ChronicleGameEnd(List<SituationController> situations, List<IElementStacksManager> stacksManagers)
        {
            //a lot of the stuff in TokenPlacedOnTabletop might be better here, actually

            List<IElementStack> allStacksInGame=new List<IElementStack>();

            foreach (var situation in situations)
            {
                allStacksInGame.AddRange(situation.GetStartingStacks());
                allStacksInGame.AddRange(situation.GetOutputStacks());
                allStacksInGame.AddRange(situation.GetStoredStacks());
                allStacksInGame.AddRange(situation.GetOngoingStacks());
             
            }

            foreach (var sm in stacksManagers)
            {
                allStacksInGame.AddRange(sm.GetStacks());
            }

            var rnd=new Random();

            allStacksInGame=allStacksInGame.OrderBy(x => rnd.Next()).ToList(); //shuffle them, to prevent eg most recent follower consistently showing up at top.

            TryUpdateBestFollower(allStacksInGame);
        }

        private void TryUpdateBestFollower(List<IElementStack> stacks)
        {
            //var pastFollowerId = _storage.GetFutureLegacyEventRecord(LegacyEventRecordId.LastFollower);

            //var currentFollower = _compendium.GetElementById(pastFollowerId);
            //if (currentFollower == null)
            //{
            //    NoonUtility.Log("Couldn't find a matching follower for CurrentFollowerId lever when trying to update follower");
            //    return;
            //}

            Element currentFollower=null;

            foreach (var stack in stacks.Where(s=>s.GetAspects().ContainsKey(FOLLOWER_ASPECT) && s.GetAspects().ContainsKey(MORTAL_ASPECT)))
            {
                var aspects = stack.GetAspects();
                //if the follower is Exalted, update it.
                if (aspects.ContainsKey(EXALTED_ASPECT))
                {
                    currentFollower = _compendium.GetElementById(stack.Id);
                }

                else if (aspects.ContainsKey(DISCIPLE_ASPECT) && currentFollower!=null && !currentFollower.Aspects.ContainsKey(EXALTED_ASPECT))
                    {
                        currentFollower = _compendium.GetElementById(stack.Id);
                    }
                else if (currentFollower==null || (!currentFollower.Aspects.ContainsKey(EXALTED_ASPECT) &&
                         !currentFollower.Aspects.ContainsKey(DISCIPLE_ASPECT)))
                {
                    currentFollower = _compendium.GetElementById(stack.Id);

                }

            }

            if(currentFollower!=null)

            _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastFollower, currentFollower.Id);



        }

        private void TryUpdateHqLever(ElementStackToken token, IAspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(HQ_ASPECT))
                _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastHeadquarters, token.Id);
        }

        private void TryUpdateCultLever(ElementStackToken token, IAspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(CULT_ASPECT))
                _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastCult, token.Id);
        }

        private void TryUpdateToolLever(ElementStackToken token, IAspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(TOOL_ASPECT))
                _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastTool, token.Id);
        }

        private void TryUpdateDesireLever(IAspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(DESIRE_ASPECT))
            {
                if (tokenAspects.Keys.Contains(POWER_ASPECT))
                    _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastDesire, "ascensionpowera");

                else if (tokenAspects.Keys.Contains(SENSATION_ASPECT))
                    _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastDesire, "ascensionsensationa");

                else if (tokenAspects.Keys.Contains(ENLIGHTENMENT_ASPECT))

                    _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastDesire, "ascensionenlightenmenta");
            }
        }

        private void TryUpdateBookLever(ElementStackToken token, IAspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(BOOK_ASPECT))
                _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.LastBook, token.Id);
        }
    }
}
