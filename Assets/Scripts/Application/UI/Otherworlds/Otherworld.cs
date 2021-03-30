using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Logic;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Assets.Scripts.Application.UI.Otherworlds;
using SecretHistories.Constants.Events;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.Logic;
using SecretHistories.Otherworlds;
using SecretHistories.Spheres;
using SecretHistories.Tokens.TokenPayloads;
using SecretHistories.UI;
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

        private readonly HashSet<Sphere> _spheres=new HashSet<Sphere>();
        private readonly List<AbstractOtherworldAttendant> _attendants=new List<AbstractOtherworldAttendant>();

        private Ingress _activeIngress;

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
            throw new NotImplementedException();
        }

        public Sphere GetWindowsSphere()
        {
            throw new NotImplementedException();
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

            EntryAnimation.onAnimationComplete += OnShowComplete;
            EntryAnimation.SetCenterForEffect(effectCenter);
            EntryAnimation.Show(); // starts coroutine that calls OnShowComplete when done
        }

        void OnShowComplete()
        {
            EntryAnimation.onAnimationComplete -= OnShowComplete;
            OnArrival();
        }

        public void Hide()
        {
            if (!EntryAnimation.CanHide())


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
        }

        public void OnArrival()
        {
            RegisterAttendant(new AttendantThereCanBeOnlyOne(this));
            

            DisplayOrHideDominions();
            foreach (var d in _dominions)
            {
                var closeOnChoice = new AttendantCloseOnChoice(this, d.doorSlot);
                RegisterAttendant(closeOnChoice);
            }

            EnactConsequences();
        }

        private void DisplayOrHideDominions()
        {
            foreach (var d in Dominions)
                if (String.Equals(d.Identifier, _activeIngress.GetEgressId(), StringComparison.InvariantCultureIgnoreCase)
                ) //Portal identifiers used to be enums, with ToString= eg Wood. Let's be extra forgiving.
                {
                    d.Evoke();

                }
                else
                    d.Dismiss();
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
