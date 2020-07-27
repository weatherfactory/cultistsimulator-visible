using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Noon;
using UnityEngine.Analytics;


public interface ICompendium
{
    Recipe GetFirstMatchingRecipe(AspectsInContext aspectsInContext, string verb, Character character,bool getHintRecipes);
    List<T> GetEntitiesAsList<T>() where T : class, IEntityWithId;
    T GetEntityById<T>(string entityId) where  T: class,IEntityWithId;
    bool TryAddEntity(IEntityWithId entity);
    bool EntityExists<T>(string entityId) where T : class, IEntityWithId;

    void SupplyLevers(IGameEntityStorage populatedCharacter);
    string GetVerbIconOverrideFromAspects(IAspectsDictionary currentAspects);


    /// <summary>
    /// Run all second-stage populations that occur between / across entities
    /// </summary>
    void OnPostImport(ContentImportLog log);

    void InitialiseForEntityTypes(IEnumerable<Type> entityTypes);
    IEnumerable<Type> GetEntityTypes();

    void LogFnords(ContentImportLog log);
    void CountWords(ContentImportLog log);
    void LogMissingImages(ContentImportLog log);
    void SupplyElementIdsForValidation(object validateThis);


}

public class Compendium : ICompendium
{
    
    private Dictionary<Type, EntityStore> entityStores;

 private Dictionary<string, string> _pastLevers;

 private readonly List<string> elementIdsToValidate=new List<string>();

    private EntityStore EntityStoreFor(Type type)
    {
        return entityStores[type];
    }

    /// <summary>
    /// Very forgiving method that accepts strings or collections in a variety of formats to log element  ids for later validation
    /// This could really be very generic and validate any kind of id we like, but I'm leaving that for now until I've taken more of a look at mods
    /// </summary>
    /// <param name="validateThis"></param>
    public void SupplyElementIdsForValidation(object validateThis)
    {
        if (validateThis is Dictionary<string, int> di)
            elementIdsToValidate.AddRange(di.Keys);
        else if (validateThis is Dictionary<string, string> ds)
            elementIdsToValidate.AddRange(ds.Keys);
        else if(validateThis is IAspectsDictionary a)
            elementIdsToValidate.AddRange(a.KeysAsList());
        else if(validateThis is List<string> l)
            elementIdsToValidate.AddRange(l);
        else if(validateThis is string s)
            elementIdsToValidate.Add(s);
        else
           throw new ArgumentException("Unknown argument for element validation: " + validateThis.ToString());
    }



    /// <summary>
    /// A Compendium is initialised with all the entity types it'll contain
    /// </summary>
    /// <param name="entityTypes"></param>
    public void InitialiseForEntityTypes(IEnumerable<Type> entityTypes)
    {
        entityStores=new Dictionary<Type,EntityStore>();
        
        foreach(Type t in entityTypes)
        {
            if(!typeof(IEntityWithId).IsAssignableFrom(t))
                throw new ApplicationException($"Type {t.Name} doesn't implement {nameof(IEntityWithId)} - it shouldn't be specified to compendium as an entity type");
            entityStores.Add(t,new EntityStore());
        }
        
    }

    public IEnumerable<Type> GetEntityTypes()
    {
        var allEntityTypes = entityStores.Keys;
        return new List<Type>(allEntityTypes);
    }

    public bool TryAddEntity(IEntityWithId entityToAdd)
    {
        var type = entityToAdd.GetType();
        var entitiesStore = EntityStoreFor(type);
        return entitiesStore.TryAddEntity(entityToAdd);

    }


    public void OnPostImport(ContentImportLog log)
    {



      //run OnPostImport for every entity in every store
        foreach (var entityStore in entityStores.Values)
        {
            var entityList = new List<IEntityWithId>(entityStore.GetAllAsList()); //we might modify the collection as it gets refined, so we need to copy it first

            foreach (var e in entityList)
                e.OnPostImport(log, this);
        }


        ValidateAspectIds(log);
    }

    private void ValidateAspectIds(ContentImportLog log)
    {
        var elements = entityStores[typeof(Element)].GetAll();

        var missingAspects = elementIdsToValidate.Except(elements.Keys);

        foreach (var missingAspect in missingAspects)
        {
            //  if(!IsKnownElement(missingAspect))//double-checking that it is a genuinely missing element: there's extra logic to check if e.g. it's a lever or other token
            log.LogWarning("unknown element id specified: " + missingAspect);
        }
    }







    public List<T> GetEntitiesAsList<T>() where T: class, IEntityWithId
    {
        EntityStore entityStore = entityStores[typeof(T)];

        return entityStore.GetAllAsList<T>();
    }



    public bool EntityExists<T>(string entityId) where T : class, IEntityWithId
    {
        EntityStore entityStore = entityStores[typeof(T)];

        return (entityStore.TryGetById(entityId, out T entity));

    }

    public T GetEntityById<T>(string entityId) where T : class, IEntityWithId
    {
        EntityStore entityStore = entityStores[typeof(T)];

        if (entityStore.TryGetById(entityId, out T entity))
        {
                if (!string.IsNullOrEmpty(entity.Lever) && _pastLevers.ContainsKey(entity.Lever))
                {
                    entity= GetEntityById<T>(_pastLevers[entity.Lever]);

                }

                return entity;


        }
        else
        {

            NoonUtility.Log("Can't find entity id '" + entityId + "' of type " + typeof(T));
            return null;
        }


    }

    ///<summary>
    ///<param name="character">supplied so we can check maxexecutions (ugh) and possibly other contextual lmitations</param>
    /// <param name="getHintRecipes">If true, get recipes with hintonly=true (and *only* hintonly=true)</param>
    /// </summary>
    public Recipe GetFirstMatchingRecipe(AspectsInContext aspectsInContext, string verb, Character character, bool getHintRecipes)
    {

        aspectsInContext.ThrowErrorIfNotPopulated(verb);

        //note: we *either* get craftable recipes *or* if we're getting hint recipes we don't care if they're craftable
        var _recipes = EntityStoreFor(typeof(Recipe)).GetAllAsList<Recipe>();
        List<Recipe> candidateRecipes = _recipes.Where(r => r.ActionId == verb && (r.Craftable || getHintRecipes) && r.HintOnly == getHintRecipes && !character.HasExhaustedRecipe(r)).ToList();
        foreach (var recipe in candidateRecipes)
        {
            if (recipe.RequirementsSatisfiedBy(aspectsInContext))
                return recipe;
        }

        return null;
    }




    /// <summary>
    /// allow the character to specify levers (legacy event records)
    /// replace tokens with lever values, and also store the levers for later use
    /// if we want to retrieve the actual levered elements.
    /// </summary>
    /// <param name="populatedCharacter"></param>
    public void SupplyLevers(IGameEntityStorage populatedCharacter)
    {
        _pastLevers = populatedCharacter.GetAllPastLegacyEventRecords();
        TokenReplacer tr = new TokenReplacer(populatedCharacter,this);

        EntityStore recipesStore = entityStores[typeof(Recipe)];

        foreach (var r in recipesStore.GetAllAsList<Recipe>())
        {

            r.Label = tr.ReplaceTextFor(r.Label);
            r.StartDescription = tr.ReplaceTextFor(r.StartDescription);
            r.Description = tr.ReplaceTextFor(r.Description);
        }

        var elements = entityStores[typeof(Element)].GetAll();

        foreach (var k in elements.Keys)
        {
            var e = elements[k] as Element;
            e.Label = tr.ReplaceTextFor(e.Label);
            e.Description = tr.ReplaceTextFor(e.Description);

        }

    }
    /// <summary>
    /// If any of the aspects in currentAspects specify a verb icon override, return that icon override value
    /// </summary>
    /// <param name="currentAspects"></param>
    /// <returns></returns>
    public string GetVerbIconOverrideFromAspects(IAspectsDictionary currentAspects)
    {
        if (currentAspects != null)
        {
            foreach (var a in currentAspects.Where(a=>a.Value>0))
            { 
                var e = entityStores[typeof(Element)].GetById<Element>(a.Key);
                try
                {
                    //assume only one override, but out after
                    if (!string.IsNullOrEmpty(e.VerbIcon))
                        return e.VerbIcon;
                }
                catch (Exception)
                {
                   throw new ApplicationException("Couldn't find VerbIcon for element" + a.Key  + " - does that element exist?");
                }
            }
        }

        return null;
    }


    public void LogFnords(ContentImportLog log)
    {
        const string FNORD = "FNORD";

        var allElements = entityStores[typeof(Element)].GetAllAsList<Element>();
        string elementFnords = "";
        int elementFnordCount = 0;
        foreach (var e in allElements)
        {
            if (e.Label.ToUpper().Contains(FNORD)
                || e.Description.ToUpper().Contains(FNORD)
            )
            {
                elementFnords += (" " + e.Id);
                elementFnordCount++;
            }
        }

        var allRecipes = EntityStoreFor(typeof(Recipe)).GetAllAsList<Recipe>();
        string recipeFnords = "";
        int recipeFnordCount = 0;
        foreach (var r in allRecipes)
        {

            if (r.Label.ToUpper().Contains(FNORD)
                || r.StartDescription.ToUpper().Contains(FNORD)
                || r.Description.ToUpper().Contains(FNORD)

            )
            {

                recipeFnords += (" " + r.Id);
                recipeFnordCount++;
            }
        }


        if (elementFnords != "") 
            log.LogInfo(elementFnordCount + "  fnords for elements:" + elementFnords);
        else
            log.LogInfo("No fnords found for elements.");
        if (recipeFnords != "")
            log.LogInfo(recipeFnordCount + "  fnords for recipes:" + recipeFnords);
        else
            log.LogInfo("No fnords found for recipes.");

    }

    public void CountWords(ContentImportLog log)
    {
        int words = 0;
        foreach (var r in EntityStoreFor(typeof(Recipe)).GetAllAsList<Recipe>())
        {
            words += (r.Label.Count(char.IsWhiteSpace) + 1);
            words += (r.StartDescription.Count(char.IsWhiteSpace) + 1);
            words += (r.Description.Count(char.IsWhiteSpace) + 1);
        }



        foreach (var e in  entityStores[typeof(Element)].GetAllAsList<Element>())
        {
            words += (e.Label.Count(char.IsWhiteSpace) + 1);
            words += (e.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var v in entityStores[typeof(BasicVerb)].GetAllAsList<BasicVerb>())
        {
            var verb = (BasicVerb) v;

            words += (verb.Label.Count(char.IsWhiteSpace) + 1);
            words += (verb.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var l in EntityStoreFor(typeof(Legacy)).GetAllAsList<Legacy>())
        {
            words += (l.Label.Count(char.IsWhiteSpace) + 1);
            words += (l.StartDescription.Count(char.IsWhiteSpace) + 1);
            words += (l.Description.Count(char.IsWhiteSpace) + 1);
        }

        log.LogInfo("Words (based on spaces +1 count): " + words);

    }

    public void LogMissingImages(ContentImportLog log)
    {
        //check for missing images
        var allElements = entityStores[typeof(Element)].GetAllAsList<Element>();
        string missingAspectImages = "";
        int missingAspectImageCount = 0;
        string missingElementImages = "";
        int missingElementImageCount = 0;
        foreach (var e in allElements)
        {

            if (e.IsAspect)
            {
                if ((!e.NoArtNeeded && !e.IsHidden) && (ResourcesManager.GetSpriteForAspect(e.Icon) == null || ResourcesManager.GetSpriteForAspect(e.Icon).name == ResourcesManager.PLACEHOLDER_IMAGE_NAME))
                {
                    missingAspectImages += (" " + e.Id);
                    missingAspectImageCount++;
                }
            }
            else
            {
                if (!e.NoArtNeeded && ResourcesManager.GetSpriteForElement(e.Icon).name == ResourcesManager.PLACEHOLDER_IMAGE_NAME)
                {
                    missingElementImages += (" " + e.Id);
                    missingElementImageCount++;
                }
            }
        }

        if (missingAspectImages != "")
            log.LogInfo("Missing " + missingAspectImageCount + " images for aspects:" + missingAspectImages);
        else
            log.LogInfo("No missing aspect images found.");

        if (missingElementImages != "")
            log.LogInfo("Missing " + missingElementImageCount + " images for elephants:" + missingElementImages);
        else
            log.LogInfo("No missing elephant images found.");
    }


}
