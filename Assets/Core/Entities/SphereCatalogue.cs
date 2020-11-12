using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.TokenContainers;
using Noon;
using UnityEngine;

namespace Assets.Core.Entities {

    public class SphereCatalogue:ISphereEventSubscriber {

        public bool EnableAspectCaching = true;
        private readonly HashSet<Sphere> _spheres;
        private readonly HashSet<ISphereEventSubscriber> _subscribers;
        private AspectsDictionary _tabletopAspects = null;
        private AspectsDictionary _allAspectsExtant = null;
        private bool _tabletopAspectsDirty = true;
        private bool _allAspectsExtantDirty = true;

        public const string EN_ROUTE_PATH = "enroute";
        
        public const string STORAGE_PATH = "storage";


        public SphereCatalogue() {
            _spheres = new HashSet<Sphere>();
            _subscribers = new HashSet<ISphereEventSubscriber>();
        }

        public HashSet<Sphere> GetSpheres() {
            return _spheres;
        }

        public void RegisterSphere(Sphere sphere) {
            
            _spheres.Add(sphere);
        }

        public void DeregisterSphere(Sphere sphere) {
            
            _spheres.Remove(sphere);
        }


        public void Reset()
        {
            foreach(var c in new List<Sphere>(_spheres))
                _spheres.RemoveWhere(tc => !tc.PersistBetweenScenes);
            
            _subscribers.Clear();
        }


        public void Subscribe(ISphereEventSubscriber subscriber) {
            _subscribers.Add(subscriber);
         }

        public void Unsubscribe(ISphereEventSubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }


        public Sphere GetContainerByPath(SpherePath containerPath)
        {

            try
            {
                var specifiedContainer = _spheres.SingleOrDefault(c => c.GetPath() == containerPath);
                if (specifiedContainer == null)
                    return Registry.Get<NullContainer>();

                return specifiedContainer;

            }
            catch (Exception e)
            {
                NoonUtility.LogWarning($"Error retrieving container with path {containerPath}: {e.Message}");
                return Registry.Get<NullContainer>();
            }

        }


        public void NotifyStacksChangedForContainer(TokenEventArgs args)
        {
            NotifyAspectsDirty();

            foreach(var s in _subscribers)
                s.NotifyStacksChangedForContainer(args);
        }

        public void OnTokenClicked(TokenEventArgs args)
        {
            foreach (var s in _subscribers)
                s.OnTokenClicked(args);
        }

        public void OnTokenReceivedADrop(TokenEventArgs args)
        {
            foreach (var s in _subscribers)
                s.OnTokenReceivedADrop(args);
        }

        public void OnTokenPointerEntered(TokenEventArgs args)
        {
            foreach (var s in _subscribers)
                s.OnTokenDoubleClicked(args);
        }

        public void OnTokenPointerExited(TokenEventArgs args)
        {
            foreach (var s in _subscribers)
                s.OnTokenPointerEntered(args);
        }

        public void OnTokenDoubleClicked(TokenEventArgs args)
        {
            foreach (var s in _subscribers)
                s.OnTokenPointerExited(args);
        }

        public void OnTokenDragged(TokenEventArgs args)
        {
            foreach(var s in _subscribers)
                s.OnTokenDragged(args);
        }


        public AspectsInContext GetAspectsInContext(IAspectsDictionary aspectsInSituation)
        {
            if (!EnableAspectCaching)
            {
                _tabletopAspectsDirty = true;
                _allAspectsExtantDirty = true;
            }

            if (_tabletopAspectsDirty)
            {
                if (_tabletopAspects == null)
                    _tabletopAspects = new AspectsDictionary();
                else
                    _tabletopAspects.Clear();

                List<ElementStackToken> tabletopStacks=new List<ElementStackToken>();


                var tabletopContainers =
                    _spheres.Where(tc => tc.SphereCategory == SphereCategory.World);

                foreach(var tc in tabletopContainers)
                    tabletopStacks.AddRange(tc.GetStacks());

                
                foreach (var tabletopStack in tabletopStacks)
                {
                    IAspectsDictionary stackAspects = tabletopStack.GetAspects();
                    IAspectsDictionary multipliedAspects = new AspectsDictionary();
                    //If we just count aspects, a stack of 10 cards only counts them once. I *think* this is the only place we need to worry about this rn,
                    //but bear it in mind in case there's ever a similar issue inside situations <--there is! if multiple cards are output, they stack.
                    //However! To complicate matters, if we're counting elements rather than aspects, there is already code in the stack to multiply aspect * quality, and we don't want to multiply it twice
                    foreach (var aspect in stackAspects)
                    {

                        if (aspect.Key == tabletopStack.EntityId)
                            multipliedAspects.Add(aspect.Key, aspect.Value);
                        else
                            multipliedAspects.Add(aspect.Key, aspect.Value * tabletopStack.Quantity);
                    }
                    _tabletopAspects.CombineAspects(multipliedAspects);
                }


                if (EnableAspectCaching)
                {
                    _tabletopAspectsDirty = false;		// If left dirty the aspects will recalc every frame
                    _allAspectsExtantDirty = true;      // Force the aspects below to recalc
                }
            }

            if (_allAspectsExtantDirty)
            {
                if (_allAspectsExtant == null)
                    _allAspectsExtant = new AspectsDictionary();
                else
                    _allAspectsExtant.Clear();

                var allSituations = Registry.Get<SituationsCatalogue>();
                foreach (var s in allSituations.GetRegisteredSituations())
                {
                    var stacksInSituation = s.GetAllStacksInSituation();
                    foreach (var situationStack in stacksInSituation)
                    {
                        IAspectsDictionary stackAspects = situationStack.GetAspects();
                        IAspectsDictionary multipliedAspects = new AspectsDictionary();
                        //See notes above. We need to multiply aspects to take account of stack quantities here too.
                        foreach (var aspect in stackAspects)
                        {

                            if (aspect.Key == situationStack.EntityId)
                                multipliedAspects.Add(aspect.Key, aspect.Value);
                            else
                                multipliedAspects.Add(aspect.Key, aspect.Value * situationStack.Quantity);
                        }
                        _allAspectsExtant.CombineAspects(multipliedAspects);
                    }

                }
                _allAspectsExtant.CombineAspects(_tabletopAspects);

                if (EnableAspectCaching)
                    _allAspectsExtantDirty = false;     // If left dirty the aspects will recalc every frame
            }

            AspectsInContext aspectsInContext = new AspectsInContext(aspectsInSituation, _tabletopAspects, _allAspectsExtant);

            return aspectsInContext;

        }

        public void NotifyAspectsDirty()
        {
            _tabletopAspectsDirty = true;
        }

    }
}
