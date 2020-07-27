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


public class EntityStore
{
    private Dictionary<string, IEntityWithId> _entities=new Dictionary<string, IEntityWithId>();


    public bool TryAddEntity(IEntityWithId entityToAdd)
    {
        if (!_entities.ContainsKey(entityToAdd.Id))
        {
            AddEntity(entityToAdd);
            return true;
        }

        return false;
    }


    public void AddEntity(IEntityWithId entityToAdd)
    {
            _entities.Add(entityToAdd.Id, entityToAdd);

    }

    public bool TryGetEntityById<T>(string entityId, out T entity) where T : class, IEntityWithId
    {
        IEntityWithId retrievedEntity;
        if(_entities.TryGetValue(entityId, out retrievedEntity))
        {
            entity = retrievedEntity as T;
            return true;
        }
        else
        {
            entity = null;
            return false;
        }


    }


    public T GetEntityById<T>(string entityId) where T : class, IEntityWithId
    {
        return _entities[entityId] as T;
    }


    public List<IEntityWithId> GetAllAsList()
    {
        return new List<IEntityWithId>(_entities.Values);
    }


    public List<T> GetAllAsList<T>() where T: class, IEntityWithId
    {
        
        return new List<T>(_entities.Values.Cast<T>().ToList());
    }

    public Dictionary<string, IEntityWithId> GetAll()
    {
        return new Dictionary<string, IEntityWithId>(_entities);
    }
}

public interface ICompendium
{
    Recipe GetFirstRecipeForAspectsWithVerb(AspectsInContext aspectsInContext, string verb, Character character,bool getHintRecipes);
    List<T> GetEntitiesAsList<T>() where T : class, IEntityWithId;
    T GetEntityById<T>(string entityId) where  T: class,IEntityWithId;
   bool TryGetEntityById<T>(string entityId, out T entity) where T : class, IEntityWithId;
    Dictionary<string,Element> GetAllElementsAsDictionary();
    Element GetElementById(string elementId);
    Boolean IsKnownElement(string elementId);

    List<Ending> GetAllEndings();
    Ending GetEndingById(string endingFlag);

    List<Legacy> GetAllLegacies();
    Legacy GetLegacyById(string legacyId);

    List<IDeckSpec> GetAllDeckSpecs();
    DeckSpec GetDeckSpecById(string id);
    void SupplyLevers(IGameEntityStorage populatedCharacter);
    string GetVerbIconOverrideFromAspects(IAspectsDictionary currentAspects);

    bool TryAddDeckSpec(DeckSpec deck);

    void AddEntity(string id, Type type, IEntityWithId entity);
    /// <summary>
    /// Run all second-stage populations that occur between / across entities
    /// </summary>
    void OnPostImport(ContentImportLog log);

    void Reset();

    void LogFnords(ContentImportLog log);
    void CountWords(ContentImportLog log);
    void LogMissingImages(ContentImportLog log);
    void SupplyElementIdsForValidation(object validateThis);
}

public class Compendium : ICompendium
{
    private Dictionary<Type, IDictionary> allEntities;
    private Dictionary<Type, EntityStore> allEntityStores;

   // private List<Recipe> _recipes=new List<Recipe>();
    private Dictionary<string, string> _pastLevers;

  //  private Dictionary<string, Recipe> _recipeDict;
    private Dictionary<string, Element> _elements;
    private Dictionary<string, Legacy> _legacies;
    private Dictionary<string, Ending> _endings;
    private Dictionary<string, DeckSpec> _decks;

    private List<string> aspectIdsToValidate=new List<string>();

    private EntityStore getEntitiesStoreOfType(Type type)
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



    public Compendium()
    {
    Reset(); //a little inelegant to call this twice - we call it explicitly in the content importer too
    }


    public void Reset()
    {
        allEntities= new Dictionary<Type, IDictionary>();

        allEntityStores=new Dictionary<Type,EntityStore>();
        
    //    _recipeDict = new Dictionary<string, Recipe>();
    _elements = new Dictionary<string, Element>();
    _legacies = new Dictionary<string, Legacy>();
    _endings = new Dictionary<string, Ending>();
    _decks = new Dictionary<string, DeckSpec>();

       //  allEntities.Add(typeof(Recipe), _recipeDict);
        allEntities.Add(typeof(Element), _elements);
        allEntities.Add(typeof(Legacy), _legacies);
        allEntities.Add(typeof(Ending), _endings);
        allEntities.Add(typeof(DeckSpec), _decks);


        var verbStore = new EntityStore();
        allEntityStores.Add(typeof(BasicVerb), verbStore);

        var recipeStore = new EntityStore();
        allEntityStores.Add(typeof(Recipe),recipeStore);

    }


    public void AddEntity(string id, Type type, IEntityWithId entity)
    {
        if (type == typeof(Recipe) || type==typeof(BasicVerb))
        {
            //       _recipes.Add(entity as Recipe);
            var recipesStore = allEntityStores[type];
            recipesStore.AddEntity(entity);
        }
        else
        {
         
        var entityStore = allEntities[type];
        entityStore.Add(id,entity);

        }

    }

    public void OnPostImport(ContentImportLog log)
    {
        foreach (var d in allEntities.Values)
        {
            HashSet <IEntityWithId> entities= new HashSet<IEntityWithId>((IEnumerable<IEntityWithId>) d.Values); //we might modify the collection as it gets refined, so we need to copy it first

            foreach (var e in entities)
                e.OnPostImport(log,this);


            var missingAspects = aspectIdsToValidate.Except(_elements.Keys);
            foreach (var missingAspect in missingAspects)
            {
              //  if(!IsKnownElement(missingAspect))//double-checking that it is a genuinely missing element: there's extra logic to check if e.g. it's a lever or other token
                    log.LogWarning("unknown element id specified: " + missingAspect);
            }

        }

        foreach (var d in allEntityStores.Values)
        {
            var entityList = new List<IEntityWithId>(d.GetAllAsList()); //we might modify the collection as it gets refined, so we need to copy it first

            foreach (var e in entityList)
                e.OnPostImport(log, this);


            var missingAspects = aspectIdsToValidate.Except(_elements.Keys);
            foreach (var missingAspect in missingAspects)
            {
                //  if(!IsKnownElement(missingAspect))//double-checking that it is a genuinely missing element: there's extra logic to check if e.g. it's a lever or other token
                log.LogWarning("unknown element id specified: " + missingAspect);
            }

        }
    }

    // -- Update Collections ------------------------------

    //public void UpdateRecipes(List<Recipe> allRecipes)
    //{
    //    _recipes = allRecipes;
        
    //    foreach (var item in allRecipes) {
    //        if (_recipeDict.ContainsKey(item.Id)) {
    //            #if UNITY_EDITOR
    //            UnityEngine.Debug.LogWarning("Duplicate Recipe Id " + item.Id + "! Skipping...");
    //            #endif
    //            continue;
    //        }

    //        _recipeDict.Add(item.Id, item);
    //    }
    //}

    /// <summary>

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
        var _recipes = getEntitiesStoreOfType(typeof(Recipe)).GetAllAsList<Recipe>();
        List<Recipe> candidateRecipes= _recipes.Where(r => r.ActionId == verb && ( r.Craftable || getHintRecipes) && r.HintOnly==getHintRecipes && !character.HasExhaustedRecipe(r)).ToList();
        foreach (var recipe in candidateRecipes )
        {
            //for each requirement in recipe, check if that aspect does *not* exist at that level in Aspects

            if (recipe.RequirementsSatisfiedBy(aspectsInContext) )
                return recipe;

        }

        return null;
    }


    // -- Get All ------------------------------

    public List<T> GetEntitiesAsList<T>() where T: class, IEntityWithId
    {
        EntityStore entityStore = allEntityStores[typeof(T)];

        return entityStore.GetAllAsList<T>();
    }



    public Dictionary<string, Element> GetAllElementsAsDictionary() {
        return _elements;
    }


    public List<IDeckSpec> GetAllDeckSpecs() {
        return new List<IDeckSpec>(_decks.Values);
    }

    public List<Legacy> GetAllLegacies() {
        return new List<Legacy>(_legacies.Values);
    }

    public List<Ending> GetAllEndings() {
        return new List<Ending>(_endings.Values);
    }


    public T GetEntityById<T> (string entityId) where T: class, IEntityWithId
    {
        EntityStore entityStore = allEntityStores[typeof(T)];

        T entity;

        if (entityStore.TryGetEntityById(entityId, out entity))
        {
            return entity;
        }
        else
        {
            NoonUtility.Log("Can't find entity id '" + entityId + "' of type " + typeof(T));
            return null;
        }
    }


    public bool TryGetEntityById<T>(string entityId, out T entity) where T : class, IEntityWithId
    {
        EntityStore entityStore = allEntityStores[typeof(T)];
        return entityStore.TryGetEntityById(entityId, out entity);
    }

    public Boolean IsKnownElement(string elementId)
    {
        //return _elements.ContainsKey(elementId);

        return (GetElementById(elementId) != null);
    }

    public Element GetElementById(string elementId) {
        
        Element element;
        _elements.TryGetValue(elementId, out element);

        if (element == null)
            return null;

        if (!string.IsNullOrEmpty(element.Lever))
        {
            if (!_pastLevers.ContainsKey(element.Lever))
                return null;
            else
                return GetElementById(_pastLevers[element.Lever]);

        }

        return element;
    }


    public DeckSpec GetDeckSpecById(string id) {
        DeckSpec deck;
        _decks.TryGetValue(id, out deck);

        return deck;
    }


    public bool TryAddDeckSpec(DeckSpec deck)
    {
        if (!_decks.ContainsKey(deck.Id))
        {
            _decks.Add(deck.Id,deck);
            return true;
        }

        return false;
    }



    public Legacy GetLegacyById(string legacyId) {
        Legacy legacy;
        _legacies.TryGetValue(legacyId, out legacy);

        return legacy;
    }

    public Ending GetEndingById(string endingId)
	{
		Analytics.CustomEvent( "A_ENDING", new Dictionary<string,object>{ {"id",endingId} } );

        Ending ending;
		if (_endings.TryGetValue(endingId, out ending))
		{
			return ending;
		}

		return Ending.DefaultEnding();
    }

    // -- Assorted Methods ------------------------------


	
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

        foreach (var k in _elements.Keys)
        {
            var e = _elements[k] as Element;
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
                var e = GetElementById(a.Key);
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

        var allElements = GetAllElementsAsDictionary();
        string elementFnords = "";
        int elementFnordCount = 0;
        foreach (var k in allElements.Keys)
        {
            var thisElement = allElements[k];

            if (thisElement.Label.ToUpper().Contains(FNORD)
                || thisElement.Description.ToUpper().Contains(FNORD)
            )
            {
                elementFnords += (" " + k);
                elementFnordCount++;
            }
        }

        var allRecipes = getEntitiesStoreOfType(typeof(Recipe)).GetAllAsList<Recipe>();
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
        foreach (var r in getEntitiesStoreOfType(typeof(Recipe)).GetAllAsList<Recipe>())
        {
            words += (r.Label.Count(char.IsWhiteSpace) + 1);
            words += (r.StartDescription.Count(char.IsWhiteSpace) + 1);
            words += (r.Description.Count(char.IsWhiteSpace) + 1);
        }



        foreach (var e in _elements.Values)
        {
            words += (e.Label.Count(char.IsWhiteSpace) + 1);
            words += (e.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var v in allEntityStores[typeof(BasicVerb)].GetAllAsList())
        {
            var verb = (BasicVerb) v;

            words += (verb.Label.Count(char.IsWhiteSpace) + 1);
            words += (verb.Description.Count(char.IsWhiteSpace) + 1);
        }

        foreach (var l in _legacies.Values)
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
        var allElements = GetAllElementsAsDictionary();
        string missingAspectImages = "";
        int missingAspectImageCount = 0;
        string missingElementImages = "";
        int missingElementImageCount = 0;
        foreach (var k in allElements.Keys)
        {
            var thisElement = allElements[k];

            if (thisElement.IsAspect)
            {
                if ((!thisElement.NoArtNeeded && !thisElement.IsHidden) && (ResourcesManager.GetSpriteForAspect(thisElement.Icon) == null || ResourcesManager.GetSpriteForAspect(thisElement.Icon).name == ResourcesManager.PLACEHOLDER_IMAGE_NAME))
                {
                    missingAspectImages += (" " + k);
                    missingAspectImageCount++;
                }
            }
            else
            {
                if (!thisElement.NoArtNeeded && ResourcesManager.GetSpriteForElement(thisElement.Icon).name == ResourcesManager.PLACEHOLDER_IMAGE_NAME)
                {
                    missingElementImages += (" " + k);
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
