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
    Recipe GetFirstRecipeForAspectsWithVerb(AspectsInContext aspectsInContext, string verb, Character character,bool getHintRecipes);
    List<T> GetEntitiesAsList<T>() where T : class, IEntityWithId;
    T GetEntityById<T>(string entityId) where  T: class,IEntityWithId;
    bool TryAddEntity(IEntityWithId entity);
    void AddEntity(string id, Type type, IEntityWithId entity);
    bool EntityExists<T>(string entityId) where T : class, IEntityWithId;

    void SupplyLevers(IGameEntityStorage populatedCharacter);
    string GetVerbIconOverrideFromAspects(IAspectsDictionary currentAspects);


    /// <summary>
    /// Run all second-stage populations that occur between / across entities
    /// </summary>
    void OnPostImport(ContentImportLog log);

    void InitialiseForTypes(IEnumerable<Type> entityTypes);

    void LogFnords(ContentImportLog log);
    void CountWords(ContentImportLog log);
    void LogMissingImages(ContentImportLog log);
    void SupplyElementIdsForValidation(object validateThis);
    
}

public class Compendium : ICompendium
{
    
    private Dictionary<Type, EntityStore> allEntityStores;

 private Dictionary<string, string> _pastLevers;


    private List<string> aspectIdsToValidate=new List<string>();

    private EntityStore entityStoreFor(Type type)
    {
        return allEntityStores[type];
    }


    public void SupplyElementIdsForValidation(object validateThis)
    {
        if (validateThis is Dictionary<string, int> di)
            aspectIdsToValidate.AddRange(di.Keys);
        else if (validateThis is Dictionary<string, string> ds)
            aspectIdsToValidate.AddRange(ds.Keys);
        else if(validateThis is IAspectsDictionary a)
            aspectIdsToValidate.AddRange(a.KeysAsList());
        else if(validateThis is List<string> l)
            aspectIdsToValidate.AddRange(l);
        else if(validateThis is string s)
            aspectIdsToValidate.Add(s);
        else
           throw new ArgumentException("Unknown argument for element validation: " + validateThis.ToString());
    }



    /// <summary>
    /// A Compendium is initialised with all the entity types it'll contain
    /// </summary>
    /// <param name="entityTypes"></param>
    public void InitialiseForTypes(IEnumerable<Type> entityTypes)
    {

        allEntityStores=new Dictionary<Type,EntityStore>();


        foreach(Type t in entityTypes)
        {
            allEntityStores.Add(t,new EntityStore());
        }
        //allEntityStores.Add(typeof(BasicVerb), new EntityStore());
        //allEntityStores.Add(typeof(DeckSpec), new EntityStore());
        //allEntityStores.Add(typeof(Element), new EntityStore());
        //allEntityStores.Add(typeof(Ending), new EntityStore());
        //allEntityStores.Add(typeof(Legacy), new EntityStore());
        //allEntityStores.Add(typeof(Recipe),new EntityStore());


    }

    public bool TryAddEntity(IEntityWithId entityToAdd)
    {
        var type = entityToAdd.GetType();
        var entitiesStore = entityStoreFor(type);
        return entitiesStore.TryAddEntity(entityToAdd);

    }

    public void AddEntity(string id, Type type, IEntityWithId entity)
    {
           var entitiesStore = entityStoreFor(type);
            entitiesStore.AddEntity(entity);

    }

    public void OnPostImport(ContentImportLog log)
    {

        var elements = allEntityStores[typeof(Element)].GetAll();

      
        foreach (var d in allEntityStores.Values)
        {
            var entityList = new List<IEntityWithId>(d.GetAllAsList()); //we might modify the collection as it gets refined, so we need to copy it first

            foreach (var e in entityList)
                e.OnPostImport(log, this);


            var missingAspects = aspectIdsToValidate.Except(elements.Keys);
            foreach (var missingAspect in missingAspects)
            {
                //  if(!IsKnownElement(missingAspect))//double-checking that it is a genuinely missing element: there's extra logic to check if e.g. it's a lever or other token
                log.LogWarning("unknown element id specified: " + missingAspect);
            }

        }
    }

    
    /// </summary>
    /// <param name="aspects"></param>
    /// <param name="verb"></param>
    /// <param name="character"></param>
    /// <param name="getHintRecipes">If true, get recipes with hintonly=true (and *only* hintonly=true)</param>
    /// <returns></returns>
    public Recipe GetFirstRecipeForAspectsWithVerb(AspectsInContext aspectsInContext, string verb, Character character, bool getHintRecipes)
    {

        aspectsInContext.ThrowErrorIfNotPopulated(verb);
        //for each recipe,
        //note: we *either* get craftable recipes *or* if we're getting hint recipes we don't care if they're craftable
        var _recipes = entityStoreFor(typeof(Recipe)).GetAllAsList<Recipe>();
        List<Recipe> candidateRecipes= _recipes.Where(r => r.ActionId == verb && ( r.Craftable || getHintRecipes) && r.HintOnly==getHintRecipes && !character.HasExhaustedRecipe(r)).ToList();
        foreach (var recipe in candidateRecipes )
        {
            //for each requirement in recipe, check if that aspect does *not* exist at that level in Aspects

            if (recipe.RequirementsSatisfiedBy(aspectsInContext) )
                return recipe;

        }

        return null;
    }



    public List<T> GetEntitiesAsList<T>() where T: class, IEntityWithId
    {
        EntityStore entityStore = allEntityStores[typeof(T)];

        return entityStore.GetAllAsList<T>();
    }



    public bool EntityExists<T>(string entityId) where T : class, IEntityWithId
    {
        EntityStore entityStore = allEntityStores[typeof(T)];

        return (entityStore.TryGetById(entityId, out T entity));

    }

    public T GetEntityById<T>(string entityId) where T : class, IEntityWithId
    {
        EntityStore entityStore = allEntityStores[typeof(T)];

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

        EntityStore recipesStore = allEntityStores[typeof(Recipe)];

        foreach (var r in recipesStore.GetAllAsList<Recipe>())
        {

            r.Label = tr.ReplaceTextFor(r.Label);
            r.StartDescription = tr.ReplaceTextFor(r.StartDescription);
            r.Description = tr.ReplaceTextFor(r.Description);
        }

        var elements = allEntityStores[typeof(Element)].GetAll();

        foreach (var k in elements.Keys)
        {
            var e = elements[k] as Element;
            e.Label = tr.ReplaceTextFor(e.Label);
            e.Description = tr.ReplaceTextFor(e.Description);

        }

    }

    public string GetVerbIconOverrideFromAspects(IAspectsDictionary currentAspects)
    {
        if (currentAspects != null)
        {
            foreach (var a in currentAspects.Where(a=>a.Value>0))
            { 
                var e = allEntityStores[typeof(Element)].GetById<Element>(a.Key);
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

        var allElements = allEntityStores[typeof(Element)].GetAllAsList<Element>();
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

        var allRecipes = entityStoreFor(typeof(Recipe)).GetAllAsList<Recipe>();
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
        foreach (var r in entityStoreFor(typeof(Recipe)).GetAllAsList<Recipe>())
        {
            words += (r.Label.Count(char.IsWhiteSpace) + 1);
            words += (r.StartDescription.Count(char.IsWhiteSpace) + 1);
            words += (r.Description.Count(char.IsWhiteSpace) + 1);
        }



        foreach (var e in  allEntityStores[typeof(Element)].GetAllAsList<Element>())
        {
            words += (e.Label.Count(char.IsWhiteSpace) + 1);
            words += (e.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var v in allEntityStores[typeof(BasicVerb)].GetAllAsList<BasicVerb>())
        {
            var verb = (BasicVerb) v;

            words += (verb.Label.Count(char.IsWhiteSpace) + 1);
            words += (verb.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var l in entityStoreFor(typeof(Legacy)).GetAllAsList<Legacy>())
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
        var allElements = allEntityStores[typeof(Element)].GetAllAsList<Element>();
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
