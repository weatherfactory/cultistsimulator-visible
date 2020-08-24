using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine.Analytics;
using Noon;

namespace Assets.Core.Services
{
    /// <summary>
    /// meta responses to significant in game events
    /// </summary>
    
    
    public class Chronicler
    {
        private Character _storage;
        private ICompendium _compendium;
        private const string BOOK_ASPECT = "text";
        private const string DESIRE_ASPECT = "desire";
        private const string TOOL_ASPECT = "tool";
        private const string CULT_ASPECT = "society";
        private const string HQ_ASPECT = "hq";
        private const string POWER_ASPECT="powermarks";
        private const string SENSATION_ASPECT="sensationmarks";
        private const string ENLIGHTENMENT_ASPECT = "enlightenmentmarks";
        private const string EXALTED_ASPECT = "exalted";
        private const string DISCIPLE_ASPECT = "disciple";
        private const string FOLLOWER_ASPECT = "follower";
        private const string MORTAL_ASPECT = "mortal";
        private const string SUMMONED_ASPECT = "summoned";
        private const string HIRELING_ASPECT = "hireling";
        private const string EDGE = "edge";
        private const string FORGE = "forge";
        private const string GRAIL = "grail";
        private const string HEART = "heart";
        private const string KNOCK = "knock";
        private const string LANTERN = "lantern";
        private const string MOTH = "moth";
        private const string SECRETHISTORIES = "secrethistories";
        private const string WINTER = "winter";





        public Chronicler(Character storage,ICompendium compendium)
        {
            _storage = storage;
            _compendium = compendium;
        }

        public void CharacterNameChanged(string newName)
        {
            _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.lastcharactername.ToString(), newName);
        }

        public void TokenPlacedOnTabletop(ElementStackToken token)
        {

            if (token.PlacementAlreadyChronicled)
                return;

            IAspectsDictionary tokenAspects = token.GetAspects();

            var storefrontServicesProvider = Registry.Get<StorefrontServicesProvider>();

            TryChronicleBookPlaced(token, tokenAspects);

            TryChronicleDesirePlaced(token, tokenAspects);

            TryChronicleFollowerPlaced(token, tokenAspects, storefrontServicesProvider);

            TryChronicleToolPlaced(token, tokenAspects);
            
            TryChronicleCultPlaced(token, tokenAspects, storefrontServicesProvider);
            
            TryCHronicleHQPlaced(token, tokenAspects);

            token.PlacementAlreadyChronicled = true;

        }

        
        private void SetAchievementsForEnding(Ending ending)
        {
            if (string.IsNullOrEmpty(ending.Achievement))
                return;

            var storefrontServicesProvider = Registry.Get<StorefrontServicesProvider>();
            storefrontServicesProvider.SetAchievementForCurrentStorefronts(ending.Achievement, true);

            if (ending.Achievement == NoonConstants.A_ENDING_MAJORFORGEVICTORY ||
				ending.Achievement == NoonConstants.A_ENDING_MAJORGRAILVICTORY ||
				ending.Achievement == NoonConstants.A_ENDING_MAJORLANTERNVICTORY)
			{
				storefrontServicesProvider.SetAchievementForCurrentStorefronts(NoonConstants.A_ENDING_MAJORVICTORYGENERIC, true);
			}
		}

        public void ChronicleGameEnd(List<SituationController> situations, List<IElementStacksManager> stacksManagers,Ending ending)
        {
            //a lot of the stuff in TokenPlacedOnTabletop might be better here, actually
            SetAchievementsForEnding(ending);

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

            Element currentFollower=null;

            foreach (var stack in stacks.Where(s=>s.GetAspects().ContainsKey(FOLLOWER_ASPECT) && !s.GetAspects().ContainsKey(HIRELING_ASPECT) && !s.GetAspects().ContainsKey(SUMMONED_ASPECT)))
            {
                var aspects = stack.GetAspects();
                //if the follower is Exalted, update it.
                if (aspects.ContainsKey(EXALTED_ASPECT))
                {
                    currentFollower = _compendium.GetEntityById<Element>(stack.EntityId);

                }

                else if (aspects.ContainsKey(DISCIPLE_ASPECT) && currentFollower!=null && !currentFollower.Aspects.ContainsKey(EXALTED_ASPECT))
                    {
                        currentFollower = _compendium.GetEntityById<Element>(stack.EntityId);
                    }
                else if (currentFollower==null || (!currentFollower.Aspects.ContainsKey(EXALTED_ASPECT) &&
                         !currentFollower.Aspects.ContainsKey(DISCIPLE_ASPECT)))
                {
                    currentFollower = _compendium.GetEntityById<Element>(stack.EntityId);

                }

            }

            if(currentFollower!=null)

            _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.lastfollower.ToString(), currentFollower.Id);

        }

        private void TryChronicleFollowerPlaced(ElementStackToken token, IAspectsDictionary tokenAspects, StorefrontServicesProvider storefrontServicesProvider)
        {
            if (tokenAspects.ContainsKey(SUMMONED_ASPECT))
			{
				Analytics.CustomEvent( "A_SUMMON_GENERIC", new Dictionary<string,object>{ {"id",token.EntityId} } );
                storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_SUMMON_GENERIC", true);
			}

            if (tokenAspects.ContainsKey(EXALTED_ASPECT))
            {
                const int EXALT_MINIMUM_ASPECT_LEVEL = 10;

                if (tokenAspects.Keys.Contains(EDGE) && tokenAspects[EDGE]>= EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_EDGE" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_EDGE", true);
				}
                if (tokenAspects.Keys.Contains(FORGE) && tokenAspects[FORGE] >=EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_FORGE" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_FORGE", true);
				}
                if (tokenAspects.Keys.Contains(GRAIL) && tokenAspects[GRAIL] >= EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_GRAIL" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_GRAIL", true);
				}
                if (tokenAspects.Keys.Contains(HEART) && tokenAspects[HEART] >= EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_HEART" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_HEART", true);
				}
                if (tokenAspects.Keys.Contains(KNOCK) && tokenAspects[KNOCK] >=EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_KNOCK" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_KNOCK", true);
				}
                if (tokenAspects.Keys.Contains(LANTERN) && tokenAspects[LANTERN] >=EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_LANTERN" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_LANTERN", true);
				}
                if (tokenAspects.Keys.Contains(MOTH) && tokenAspects[MOTH] >=EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_MOTH" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_MOTH", true);
				}
                if (tokenAspects.Keys.Contains(WINTER) && tokenAspects[WINTER] >=EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_WINTER" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_WINTER", true);
				}
            }

        }


        private void TryCHronicleHQPlaced(ElementStackToken token, IAspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(HQ_ASPECT))
			{
				Analytics.CustomEvent( "A_HQ_PLACED", new Dictionary<string,object>{ {"id",token.EntityId} } );
                _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.lastheadquarters.ToString(), token.EntityId);
			}
        }

        private void TryChronicleCultPlaced(ElementStackToken token, IAspectsDictionary tokenAspects, StorefrontServicesProvider storefrontServicesProvider)
        {
            if (tokenAspects.Keys.Contains(CULT_ASPECT))
            {
				Analytics.CustomEvent( "A_CULT_PLACED", new Dictionary<string,object>{ {"id",token.EntityId} } );
                _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.lastcult.ToString(), token.EntityId);

                if (tokenAspects.Keys.Contains("cultsecrethistories_1"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_SECRETHISTORIES", true);
				}
                else if (tokenAspects.Keys.Contains("venerationedge"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_EDGE",true);
				}
                else if (tokenAspects.Keys.Contains("venerationforge"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_FORGE", true);
				}
                else if (tokenAspects.Keys.Contains("venerationgrail"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_GRAIL", true);
				}
                else if (tokenAspects.Keys.Contains("venerationheart"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_HEART", true);
				}
                else if (tokenAspects.Keys.Contains("venerationknock"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_KNOCK", true);
				}
                else if (tokenAspects.Keys.Contains("venerationlantern"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_LANTERN", true);
				}
                else if (tokenAspects.Keys.Contains("venerationmoth"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_MOTH", true);
                }
                else if (tokenAspects.Keys.Contains("venerationwinter"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_WINTER", true);
				}
            }
        }

        private void TryChronicleToolPlaced(ElementStackToken token, IAspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(TOOL_ASPECT))
			{
				Analytics.CustomEvent( "A_TOOL_PLACED", new Dictionary<string,object>{ {"id",token.EntityId} } );
                _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.lasttool.ToString(), token.EntityId);
			}
        }

        private void TryChronicleDesirePlaced(ElementStackToken token, IAspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(DESIRE_ASPECT))
            {
				Analytics.CustomEvent( "A_DESIRE_PLACED", new Dictionary<string,object>{ {"id",token.EntityId} } );

                if (tokenAspects.Keys.Contains(POWER_ASPECT))
				{
					Analytics.CustomEvent( "A_DESIRE_POWER" );
                    _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.lastdesire.ToString(), "ascensionpowera");
				}
                else if (tokenAspects.Keys.Contains(SENSATION_ASPECT))
                {
					Analytics.CustomEvent( "A_DESIRE_SENSATION" );
				    _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.lastdesire.ToString(), "ascensionsensationa");
				}
                else if (tokenAspects.Keys.Contains(ENLIGHTENMENT_ASPECT))
				{
					Analytics.CustomEvent( "A_DESIRE_ENLIGHTENMENT" );
                    _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.lastdesire.ToString(), "ascensionenlightenmenta");
				}
            }
        }

        private void TryChronicleBookPlaced(ElementStackToken token, IAspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(BOOK_ASPECT))
			{
				Analytics.CustomEvent( "A_BOOK_PLACED", new Dictionary<string,object>{ {"id",token.EntityId} } );
                _storage.SetFutureLegacyEventRecord(LegacyEventRecordId.lastbook.ToString(), token.EntityId);
			}
        }

        public void ChronicleMansusEntry(PortalEffect portalEffect)
        {
            var storefrontServicesProvider = Registry.Get<StorefrontServicesProvider>();

            switch (portalEffect)
            {
                case PortalEffect.Wood:
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_WOOD",true);
                    break;
                case PortalEffect.WhiteDoor:
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_WHITEDOOR", true);
                    break;
                case PortalEffect.StagDoor:
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_STAGDOOR", true);
                    break;
                case PortalEffect.SpiderDoor:
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_SPIDERDOOR", true);
                    break;
                case PortalEffect.PeacockDoor:
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_PEACOCKDOOR", true);
                    break;
                case PortalEffect.TricuspidGate:
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_TRICUSPIDGATE", true);
                    break;
            }
        }
    }
}
