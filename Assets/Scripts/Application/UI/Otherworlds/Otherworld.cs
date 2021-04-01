using Assets.Logic;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Assets.Scripts.Application.UI.Otherworlds;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.Logic;
using SecretHistories.Otherworlds;
using SecretHistories.Spheres;
using SecretHistories.Tokens.TokenPayloads;
using SecretHistories.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.UI
{
    public class Otherworld: MonoBehaviour,IManifestable
    {
        //Otherworlds are NOT currently encausted or saved
        //also, we've broken the model for simplicity - this is a Manifestable which is also its own manifestation
         List<AbstractDominion> Dominions=>new List<AbstractDominion>(_dominions);
         [SerializeField] private List<OtherworldDominion> _dominions;
        [SerializeField] private OtherworldAnimation EntryAnimation;
        [SerializeField] private EnRouteSphere otherworldSpecificEnRouteSphere;
        [SerializeField] private string EntrySfxName;
        [SerializeField] private string ExitSfxName;


        private readonly HashSet<Sphere> _spheres=new HashSet<Sphere>();
        private readonly List<AbstractOtherworldAttendant> _attendants=new List<AbstractOtherworldAttendant>();

        private Ingress _activeIngress;
        private EgressThreshold _activeEgress;

        public string Id => gameObject.name;
        public string EntityId => editableId;
        [SerializeField] private string editableId;
        public FucinePath GetAbsolutePath()
        {
            var pathAbove = FucinePath.Root();
            var absolutePath = pathAbove.AppendToken(this.Id);
            return absolutePath;
        }

        public RectTransform GetRectTransform()
        {
            return gameObject.GetComponent<RectTransform>();
        }

        public AspectsDictionary GetAspects(bool includeSelf)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, int> Mutations { get; }
        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
            throw new NotImplementedException();
        }

        public string GetSignature()
        {
            throw new NotImplementedException();
        }

        public Sphere GetEnRouteSphere()
        {
            if (otherworldSpecificEnRouteSphere != null)
                return otherworldSpecificEnRouteSphere;

            return FucineRoot.Get().GetEnRouteSphere();
        }

        public Sphere GetWindowsSphere()
        {
            return FucineRoot.Get().GetWindowsSphere();
        }

        public List<Sphere> GetSpheres()
        {
            return new List<Sphere>(_spheres);
        }

        public void AttachSphere(Sphere sphere)
        {
            _spheres.Add(sphere);
            foreach(var a in _attendants)
                sphere.Subscribe(a);
        }

        public void DetachSphere(Sphere sphere)
        {
            _spheres.Remove(sphere);
            foreach (var a in _attendants)
                sphere.Unsubscribe(a);
        }

        public void RegisterAttendant(AbstractOtherworldAttendant a)
        {
            //ultimately, then, we should have some sort of external service locator to set up these attendants
            if(!_attendants.Contains(a))
                _attendants.Add(a);

            foreach(var s in _spheres)
                s.Subscribe(a);
        }

        public void UnregisterAttendant(AbstractOtherworldAttendant a)
        {
            if (_attendants.Contains(a))
                _attendants.Remove(a);

            foreach (var s in _spheres)
                s.Unsubscribe(a);
        }

        public void UnregisterAllAttendants()
        {
            var attendantsToUnregister=new List<AbstractOtherworldAttendant>(_attendants);

            foreach(var a in attendantsToUnregister)
                UnregisterAttendant(a);

            _attendants.Clear();
        }

        public bool IsOpen { get; }
        
        public string Label { get; }
        public string Description { get; }
        public int Quantity { get; }
        public string UniquenessGroup { get; }
        public bool Unique { get; }
        public string Icon { get; }

        /// <summary>
        /// Register dominions, summon attendants
        /// </summary>
        public void Prepare()
        {
            foreach (var d in Dominions)
                d.RegisterFor(this);
            
            //I really might need to rethink this approach
            var permanentSphereSpec = otherworldSpecificEnRouteSphere.GetComponent<PermanentSphereSpec>();
            permanentSphereSpec.ApplySpecToSphere(otherworldSpecificEnRouteSphere);
            otherworldSpecificEnRouteSphere.SetContainer(this);
            
        }


        public string GetIllumination(string key)
        {
            throw new NotImplementedException();
        }

        public void SetIllumination(string key, string value)
        {
            throw new NotImplementedException();
        }

        public Timeshadow GetTimeshadow()
        {
            return Timeshadow.CreateTimelessShadow();
        }

        public bool RegisterDominion(AbstractDominion dominion)
        {
            dominion.OnSphereAdded.AddListener(AttachSphere);
            dominion.OnSphereRemoved.AddListener(DetachSphere);

            //we set dominions associated with an otherworld in the editor. But we might later want to add one at run time.
            if(!Dominions.Contains(dominion))
                Dominions.Add(dominion);

            return true;
        }

        public void Show(Transform effectCenter,Ingress ingress)
        {
            _activeIngress = ingress;
            
            if (!EntryAnimation.CanShow())
                return;

            SoundManager.PlaySfx(EntrySfxName);

            EntryAnimation.onAnimationComplete += OnShowComplete;
            EntryAnimation.SetCenterForEffect(effectCenter);
            EntryAnimation.Show(); // starts coroutine that calls OnShowComplete when done
        }

        void OnShowComplete()
        {
            EntryAnimation.onAnimationComplete -= OnShowComplete;
            OnArrival();
        }

        /// <summary>
        /// call this from Numa, not directly
        /// </summary>
        public void Hide()
        {
            if (!EntryAnimation.CanHide())
                return;

            SoundManager.PlaySfx(ExitSfxName);


            EntryAnimation.onAnimationComplete += OnHideComplete;

            EntryAnimation.Hide();
        }

        void OnHideComplete()
        {
            EntryAnimation.onAnimationComplete -= OnShowComplete;
            OnLeave();
        }

        private void OnLeave()
        {
            UnregisterAllAttendants();
            if(_activeEgress!=null)
            {
                _activeEgress.EvictAllTokens(Context.Unknown());
                _activeEgress = null;
                otherworldSpecificEnRouteSphere.SetOverridingNextStop(_activeEgress);
            }
        }

        public void OnArrival()
        {
            RegisterAttendant(new AttendantThereCanBeOnlyOne(this));
            
            ActivateDominionsAndDoors();

            foreach (var d in _dominions)
            {
                d.EgressSphere.SetEvictionDestination(_activeIngress.GetEgressOutputSphere());
                var closeOnChoice = new AttendantCloseOnChoice(this, d.EgressSphere);
                RegisterAttendant(closeOnChoice);
            }

            EnactConsequences();
        }

        private void ActivateDominionsAndDoors()
        {
            foreach (var d in _dominions)
            {
                if(d.VisibleFor(_activeIngress.GetEgressId()))
                    d.Evoke();
                else
                    d.Dismiss();

                if (d.MatchesEgress(_activeIngress.GetEgressId()))
                {
                    d.EgressSphere.RemoveBlock(new SphereBlock(BlockDirection.Inward, BlockReason.Inactive));
                    _activeEgress = d.EgressSphere;
                    otherworldSpecificEnRouteSphere.SetOverridingNextStop(_activeEgress);
                }
                else
                    d.EgressSphere.AddBlock(new SphereBlock(BlockDirection.Inward, BlockReason.Inactive));
            }
        }

        private void EnactConsequences()
        {
            foreach (var c in _activeIngress.GetConsequences())
            {
                var consequenceRecipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(c.Id);
                //we could conceivably use RecipeCompletionEffectCommand here, but that expects a Situation at the moment


                var targetSphere =
                    _spheres.SingleOrDefault(s =>
                        s.Id == c.ToPath); //NOTE: we're not actually using the pathing system here. We might want to upgrade to that.

                var fader = targetSphere.gameObject.GetComponent<CanvasGroupFader>();
                if(fader!=null)
                    fader.Show();


                if (targetSphere != null) //if we were using pathing we wouldn't have to do this
                {
                    var dealer = new Dealer(Watchman.Get<DealersTable>());

                    foreach (var deckId in consequenceRecipe.DeckEffects.Keys)

                        for (int i = 1; i <= consequenceRecipe.DeckEffects[deckId]; i++)
                        {
                            {
                                var drawnCard = dealer.Deal(deckId);
                                targetSphere.AcceptToken(drawnCard, Context.Unknown());
                            }
                        }
                }
            }
        }
    }
}
