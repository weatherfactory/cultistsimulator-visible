using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Logic;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Spheres;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Infrastructure;
using SecretHistories.Logic;
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
        [SerializeField] List<AbstractDominion> Dominions;
        [SerializeField] private OtherworldAnimation EntryAnimation;
        private readonly HashSet<Sphere> _spheres=new HashSet<Sphere>();
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

        public void AttachSphere(Sphere sphere)
        {
            _spheres.Add(sphere);
        }

        public void DetachSphere(Sphere sphere)
        {
            _spheres.Remove(sphere);
        }

        public bool IsOpen { get; }
        
        public string Label { get; }
        public string Description { get; }
        public int Quantity { get; }
        public string UniquenessGroup { get; }
        public bool Unique { get; }
        public string Icon { get; }
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

            if (Dominions.Contains(dominion))
                return false;

            Dominions.Add(dominion);
            foreach(var s in dominion.Spheres)
                AttachSphere(s);
            return true;
        }

        public void RegisterDominions()
        {
            foreach (var d in Dominions)
                RegisterDominion(d);
        }

        public void Show(Transform effectCenter,Ingress ingress)
        {
            _activeIngress = ingress;

            if (EntryAnimation.CanShow() == false)
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

        void OnHideComplete()
        {
            EntryAnimation.onAnimationComplete -= OnShowComplete;

        }

        public void OnArrival()
        {
      
            foreach (var d in Dominions)
                if (String.Equals(d.Identifier, _activeIngress.GetEgressId(), StringComparison.InvariantCultureIgnoreCase)) //Portal identifiers used to be enums, with ToString= eg Wood. Let's be extra forgiving.
                {
                    d.Evoke();
                }
                else
                    d.Dismiss();

            foreach (var c in _activeIngress.GetConsequences())
            {
                var consequenceRecipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(c.Id);
                //we could conceivably use RecipeCompletionEffectCommand here, but that expects a Situation at the moment


                var targetSphere = _spheres.SingleOrDefault(s => s.Id == c.ToPath); //NOTE: we're not actually using the pathing system here. We might want to upgrade to that.

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
