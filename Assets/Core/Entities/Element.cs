using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Fucine;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;
using UnityEngine;

namespace Assets.Core.Entities
{
    ///this is a reference object stored in Compendium where we indicate aspects, child slots and other properties#
    [FucineImportable("elements")]
    public class Element: AbstractEntity<Element>, IEntityWithId
    {

        [FucineValue(DefaultValue = "", Localise = true)]
        public string Label { get; set; }

        [FucineValue(DefaultValue = "", Localise = true)]
        public string Description { get; set;}

        [FucineValue("")]
        public string Comments { get; set; }

        [FucineValue("")]
        public string Icon
        {
            get
            {
                if (string.IsNullOrEmpty(_icon))
                    return Id;
                return _icon;
            }
            set => _icon = value;
        }

        [FucineValue("")]
        public string VerbIcon { get; set; }

        [FucineValue("")]
        public string DecayTo { get; set; }

        [FucineValue("")]
        public string UniquenessGroup { get; set; }

        //if true, when the card decays it should become more, rather than less saturated with colour (eg Fatigue->Health)
        [FucineValue(false)]
        public bool Resaturate { get; set; }

        [FucineValue(false)]
        public bool IsAspect { get; set; }

        [FucineValue(false)]
        public bool IsHidden { get; set; } //use with caution! this is intended specifically for uniqueness group aspects. It will only work on aspect displays, anyhow

        [FucineValue(false)]
        public bool NoArtNeeded { get; set; }


        /// <summary>
        /// If a Unique element is created and another one exists in games, the first one should be quietly removed. When a unique element is created, all references to it should be removed from all decks.
        /// [Note: if a deck resets on exhaustion, the rest will add a new element. So ideally, whenever a card is drawn from a deck, it should be checked for existing uniqueness. Chris' Mansus-management deck is a good place to enforce this if it doesn't already do it..]
        /// </summary>
        [FucineValue(false)]
        public bool Unique { get; set; }

        [FucineValue(0)]
        public float Lifetime { get; set; }

        [FucineValue("")]
        public string Inherits { get; set; }

        [FucineAspects(ValidateAsElementId = true)]
        public AspectsDictionary Aspects { get; set; }

        [FucineList(Localise = true)]
        public List<SlotSpecification> Slots { get; set; }

        /// <summary>
        /// Inductions ONLY OCCUR WHEN A RECIPE COMPLETES. This ensures we don't get inductions spamming over and over.
        /// Note: the 'additional' value here currently does nothing, but we might later use it to determine whether quantity of an aspect increases chance of induction
        /// </summary>
        [FucineList]
        public List<LinkedRecipeDetails> Induces { get; set; }

        /// <summary>
        /// XTriggers allow the triggering aspect to transform the element into something else. For example, if the Knock aspect were present, and the element was a locked_box with Knock:open_box,
        /// then the box would become an open_box regardless of what else happened in the recipe.
        /// XTriggers run *before* the rest of the recipe (so if the recipe affected open_box elements but not locked_box elements, those effects would take place if there was a Knock in the mix).
        /// </summary>
        [FucineDict]
        public Dictionary<string, List<MorphDetails>> XTriggers { get; set; }

        private string _icon;



        /// <summary>
        /// all aspects the element has, *including* the aspect itself as an element
        /// </summary>
        public IAspectsDictionary AspectsIncludingSelf
        {
            get
            {
                IAspectsDictionary aspectsIncludingElementItself =new AspectsDictionary();

                foreach(string k in Aspects.Keys)
                    aspectsIncludingElementItself.Add(k,Aspects[k]);

                if (!aspectsIncludingElementItself.ContainsKey(Id))
                    aspectsIncludingElementItself.Add(Id,1);
            
                return aspectsIncludingElementItself;
            }
        }


        public Element(EntityData importDataForEntity, ContentImportLog log):base(importDataForEntity, log)
        {

        }

        public Element()
        {
            Slots = new List<SlotSpecification>();
            Aspects = new AspectsDictionary();
            XTriggers = new Dictionary<string, List<MorphDetails>>();
            Induces = new List<LinkedRecipeDetails>();
        }

        public Boolean HasChildSlotsForVerb(string forVerb)
        {
            return Slots.Any(cs => cs.ActionId == forVerb || cs.ActionId == String.Empty);



        }

        /// <summary>
        /// inherit certain specific behaviours.
        /// Use with care. This is intended to be used in content import only, before these behaviours are added for the actual element, and it can't cope with duplicate keys.
        /// </summary>
        /// <param name="inheritFromElement"></param>
        public void InheritFrom(Element inheritFromElement)
        {
            Aspects.CombineAspects(inheritFromElement.Aspects);
            //no mutations: base elements don't have mutations
            foreach (string k in inheritFromElement.XTriggers.Keys)
            {
                XTriggers.Add(k,inheritFromElement.XTriggers[k]);
            }

            foreach (SlotSpecification s in inheritFromElement.Slots)
            {
                Slots.Add(s);
            }

            foreach (LinkedRecipeDetails i in inheritFromElement.Induces)
            {
                Induces.Add(i);
            }

            UniquenessGroup = inheritFromElement.UniquenessGroup; //we would probably want to do this anyway, but also we are already inheriting aspects so we have to for tidiness

        }

        protected override void OnPostImportForSpecificEntity(ContentImportLog log, ICompendium populatedCompendium)
        {
 
           
            //Apply inherits
            if (!string.IsNullOrEmpty(Inherits))
            {
                if (!populatedCompendium.EntityExists<Element>(Inherits))
                    log.LogProblem($"{Id} is trying to inherit from a nonexistent element, {Inherits}");
                else
                    InheritFrom(populatedCompendium.GetEntityById<Element>(Inherits));
            }

            //if the element is a member of a uniqueness group, add it as an aspect also.
            if (!string.IsNullOrEmpty(UniquenessGroup))
                if (!populatedCompendium.EntityExists<Element>(UniquenessGroup))
                    log.LogProblem($"{Id} has {UniquenessGroup} specified as a UniquenessGroup, but there's no aspect of that name");
                else
                    Aspects.Add(UniquenessGroup, 1);

         
            foreach (var i in Induces)
                i.OnPostImport(log, populatedCompendium);

        }


    }


}