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
using SecretHistories.Enums;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.UI
{
    public class Otherworld: MonoBehaviour,IManifestable
    {
        //Otherworlds are NOT currently encausted or saved
        //also, we've broken the model for simplicity - this is a Manifestable which is also its own manifestation
        //This is probably the way to go with things like Otherworlds and Rooms which can't actually be moved and manipulated.

      public   List<AbstractDominion> Dominions=>new List<AbstractDominion>(_dominions);
       public bool Metafictional => false;
       public bool Retire(RetirementVFX VFX)
       {
           //do nothing: can't currently retire an otherworld
           return false;
       }

       [SerializeField] private List<OtherworldDominion> _dominions;

        [SerializeField] private OtherworldTransitionFX _transitionFx;
        [SerializeField] private EnRouteSphere otherworldSpecificEnRouteSphere;

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
            var absolutePath = pathAbove.AppendingToken(this.Id);
            return absolutePath;
        }

        public FucinePath GetWildPath()
        {
            var wildCardPath = FucinePath.Wild();
            return wildCardPath.AppendingToken(this.Id);
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
                    if(d!=null)
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
            
            if (!_transitionFx.CanShow())
                return;


            RegisterAttendant(new AttendantThereCanBeOnlyOne(this));

            Watchman.Get<LocalNexus>().DisablePlayerInput(10f); //adding a timeout in case we get stuck
            _transitionFx.Show(_activeIngress, OnShowComplete); // starts coroutine that calls OnShowComplete when done
            

            ActivateEgress();
            ActivateHeralds();

        }

        private void ActivateHeralds()
        {
           //none as yet
        }

        void OnShowComplete()
        {
            OnArrival();
        }

        /// <summary>
        /// call this from Numa, not directly
        /// </summary>
        public void Hide()
        {
            if (!_transitionFx.CanHide())
                return;
            RemoveAllEgressSphereBlocks();
            UnregisterAllAttendants();

            foreach (var d in _dominions)
            {
                {
                    foreach (var s in d.Spheres)
                        if (s != d.EgressSphere)
                            s.RetireAllTokens();
                }
                d.Dismiss();
            }



            Watchman.Get<CamOperator>().PointCameraAtTableLevelVector2(_activeIngress.GetRectTransform().position,3f);
            _transitionFx.Hide(OnHideComplete);
        }

        private void RemoveAllEgressSphereBlocks()
        {
            foreach (var d in _dominions)
            {
                d.EgressSphere.RemoveMatchingBlocks(BlockDirection.Inward, BlockReason.Inactive);
                
            }
        }


        void OnHideComplete()
        {
            OnLeave();
        }

        private void OnLeave()
        {
            if(_activeEgress!=null)
            {
                _activeEgress.EvictAllTokens(Context.Unknown());
                _activeEgress = null;
                otherworldSpecificEnRouteSphere.SetOverridingNextStop(_activeEgress);
            }

            Watchman.Get<BackgroundMusic>().PlayRandomClip();
        }

        public void OnArrival()
        {
            float targetHeight = Watchman.Get<CamOperator>().ZOOM_Z_FAR;
                Watchman.Get<CamOperator>().PointAtTableLevelAtHeight(_activeEgress.GetRectTransform().position, targetHeight,0.2f,null);


            foreach (var d in _dominions)
            {
                if (d.VisibleFor(_activeIngress.GetEgressId()))
                    d.Evoke();
                else
                    d.Dismiss();
            }


            EnactConsequences();
            Watchman.Get<LocalNexus>().EnablePlayerInput();
        }

        private void ActivateEgress()
        {
            foreach (var d in _dominions)
            {
                if (d.MatchesEgress(_activeIngress.GetEgressId()))
                {
                    d.EgressSphere.RemoveMatchingBlocks(BlockDirection.Inward, BlockReason.Inactive);
                    _activeEgress = d.EgressSphere;
                    otherworldSpecificEnRouteSphere.SetOverridingNextStop(_activeEgress);
                }
                else
                    d.EgressSphere.AddBlock(BlockDirection.Inward, BlockReason.Inactive);
            }

            foreach (var d in _dominions)
            {
                d.EgressSphere.SetEvictionDestination(_activeIngress.GetEgressOutputSphere());
                var closeOnChoice = new AttendantCloseOnChoice(this, d.EgressSphere);
                RegisterAttendant(closeOnChoice);
            }
        }



        private void EnactConsequences()
        {
            foreach (var c in _activeIngress.GetConsequences())
            {
                var consequenceRecipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(c.Id);
                //we could conceivably use RecipeCompletionEffectCommand here, but that expects a Situation at the moment

                string targetSphereId = c.ToPath.GetEndingPathPart().TrimSpherePrefix();

                var targetSphere =
                    _spheres.SingleOrDefault(s =>
                        s.Id == targetSphereId); //NOTE: we're not actually using the pathing system here. This is because a Mansus otherworld breaks the model; it's a token which exists in the root.
                //either it shouldn't be, or tokens in root should be allowed.

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
