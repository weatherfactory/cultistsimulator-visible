using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using Noon;
using OrbCreationExtensions;
using UnityEngine.Rendering;
using UnityEngine.Analytics;

public interface ICompendium
{
    void UpdateRecipes(List<Recipe> allRecipes);
    void UpdateElements(Dictionary<string, Element> elements);
    void UpdateVerbs(Dictionary<string, IVerb> verbs);
    void UpdateLegacies(Dictionary<string, Legacy> legacies);
    void UpdateDeckSpecs(Dictionary<string, IDeckSpec> deckSpecs);
    Recipe GetFirstRecipeForAspectsWithVerb(IAspectsDictionary aspects, string verb, Character character,bool getHintRecipes);
    List<Recipe> GetAllRecipesAsList();
    Recipe GetRecipeById(string recipeId);
    Dictionary<string,Element> GetAllElementsAsDictionary();
    Element GetElementById(string elementId);
    Boolean IsKnownElement(string elementId);
    List<IVerb> GetAllVerbs();
    IVerb GetVerbById(string verbId);
    Ending GetEndingById(string endingFlag);
    IVerb GetOrCreateVerbForCommand(ISituationEffectCommand command);

    List<Legacy> GetAllLegacies();
    Legacy GetLegacyById(string legacyId);

    List<IDeckSpec> GetAllDeckSpecs();
    IDeckSpec GetDeckSpecById(string id);
    void SupplyLevers(IGameEntityStorage populatedCharacter);
}

public class Compendium : ICompendium
{
    private List<Recipe> _recipes;
    private Dictionary<string, Recipe> _recipeDict;
    private Dictionary<string, Element> _elements;
    private Dictionary<string, IVerb> _verbs;
    private Dictionary<string, Legacy> _legacies;
    private Dictionary<string, IDeckSpec> _decks;
    private Dictionary<LegacyEventRecordId, string> _pastLevers;

    // -- Update Collections ------------------------------

    public void UpdateRecipes(List<Recipe> allRecipes)
    {
        _recipes = allRecipes;
        _recipeDict = new Dictionary<string, Recipe>();

        foreach (var item in allRecipes) {
            if (_recipeDict.ContainsKey(item.Id)) {
                #if UNITY_EDITOR
                UnityEngine.Debug.LogWarning("Duplicate Recipe Id " + item.Id + "! Skipping...");
                #endif
                continue;
            }

            _recipeDict.Add(item.Id, item);
        }
    }

    public void UpdateElements(Dictionary<string, Element> elements)
    {
        _elements = elements;
    }

    public void UpdateVerbs(Dictionary<string, IVerb> verbs)
    {
        _verbs = verbs;
    }

    public void UpdateDeckSpecs(Dictionary<string, IDeckSpec> deckSpecs)
    {
        _decks = deckSpecs;
    }

    public void UpdateLegacies(Dictionary<string, Legacy> legacies)
    {
        _legacies = legacies;
    }


    // -- Misc Getters ------------------------------

    /// <summary>

    /// </summary>
    /// <param name="aspects"></param>
    /// <param name="verb"></param>
    /// <param name="character"></param>
    /// <param name="getHintRecipes">If true, get recipes with hintonly=true (and *only* hintonly=true)</param>
    /// <returns></returns>
    public Recipe GetFirstRecipeForAspectsWithVerb(IAspectsDictionary aspects, string verb, Character character, bool getHintRecipes)
    {
        //for each recipe,
        //note: we *either* get craftable recipes *or* if we're getting hint recipes we don't care if they're craftable
        List<Recipe> candidateRecipes=_recipes.Where(r => r.ActionId == verb && ( r.Craftable || getHintRecipes) && r.HintOnly==getHintRecipes && !character.HasExhaustedRecipe(r)).ToList();
        foreach (var recipe in candidateRecipes )
        {
            //for each requirement in recipe, check if that aspect does *not* exist at that level in Aspects

            if (recipe.RequirementsSatisfiedBy(aspects))
                return recipe;
            //Why wasn't the code using RequirementsSatisfiedBy? I think because it's very old code; but I've left it
            //here for now in case there was a good reason for special case behaviour (like not honouring -1/NOT) - AK
            //bool matches = true;
            //foreach (string requirementId in recipe.Requirements.Keys)
            //{

            //    if (!aspects.Any(a => a.Key == requirementId && a.Value >= recipe.Requirements[requirementId]))
            //        matches = false;
            //}
            //if none fail, return that recipe
            //if (matches) return recipe;
            //if any fail, continue
        }

        return null;
    }

    public Boolean IsKnownElement(string elementId) {
        return _elements.ContainsKey(elementId);
    }

    // -- Get All ------------------------------

    public List<Recipe> GetAllRecipesAsList() {
        return _recipes;
    }

    public Dictionary<string, Element> GetAllElementsAsDictionary() {
        return _elements;
    }

    public List<IVerb> GetAllVerbs() {
        return new List<IVerb>(_verbs.Values);
    }

    public List<IDeckSpec> GetAllDeckSpecs() {
        return new List<IDeckSpec>(_decks.Values);
    }

    public List<Legacy> GetAllLegacies() {
        return new List<Legacy>(_legacies.Values);
    }

    // -- Get By Id ------------------------------

    public Recipe GetRecipeById(string recipeId)
    {
        if (recipeId == null || recipeId=="NULL") //sometimes this gets parsed out of the save data?
            return null;
        if (_recipeDict.ContainsKey(recipeId))
        {
            var recipe = _recipeDict[recipeId];
            return recipe;
        }
        else
        throw new ApplicationException("Can't find recipe id " + recipeId);
        
    }

    public Element GetElementById(string elementId) {
        Element element;
        if (elementId.StartsWith(NoonConstants.LEVER_PREFIX))
        {
            
            string leverId = elementId.Replace(NoonConstants.LEVER_PREFIX, "");
            if (!Enum.IsDefined(typeof(LegacyEventRecordId), leverId))
                return null;
            else
            { 
            LegacyEventRecordId leverEnum = (LegacyEventRecordId) Enum.Parse(typeof(LegacyEventRecordId), leverId);
                if (!_pastLevers.ContainsKey(leverEnum))
                    return null;
                else
                elementId = _pastLevers[leverEnum];

            }

        }


        _elements.TryGetValue(elementId, out element);

        return element;
    }

    public IVerb GetVerbById(string verbId) {
        IVerb verb;
        _verbs.TryGetValue(verbId, out verb);

        return verb;
    }

    public IDeckSpec GetDeckSpecById(string id) {
        IDeckSpec deck;
        _decks.TryGetValue(id, out deck);

        return deck;
    }

    public Legacy GetLegacyById(string legacyId) {
        Legacy legacy;
        _legacies.TryGetValue(legacyId, out legacy);

        return legacy;
    }

    // -- Assorted Methods ------------------------------

    public IVerb GetOrCreateVerbForCommand(ISituationEffectCommand command)
    {
        var candidateVerb = GetVerbById(command.Recipe.ActionId);

        if (candidateVerb != null)
            return candidateVerb;

        var createdVerb = new CreatedVerb(command.Recipe.ActionId, command.Recipe.Label,command.Recipe.Description);

        return createdVerb;
    }

    public Ending GetEndingById(string endingFlag)
    {
		Analytics.CustomEvent( "A_ENDING", new Dictionary<string,object>{ {"id",endingFlag} } );

        if (endingFlag == "minorforgevictory")
            return new Ending(endingFlag, "THE CONFLAGRATION OF THE HEART",
                "For a little while I dwell in the high rooms of the Mansus, and then I return through the Tricuspid Gate, and my body stirs in the ashes. I am hairless and imperishable as marble, and the Forge's fire still burns within me. " +
                "I carry the Shaping Strength. I will not grow old. Perhaps I will rebel. Perhaps, one day, I will rise even higher." +
                " [Congratulations on a standard Power victory. You have wrestled the game to its knees. There are other paths.] ", "forgeofdays", EndingFlavour.Grand, "DramaticLightCool","A_ENDING_MINORFORGEVICTORY");

        if (endingFlag== "minorgrailvictory")
            return new Ending(endingFlag,"THE FEAST OF THE TRUE BIRTH",
                "For a little while I dwell in the high rooms of the Mansus, and then I return through the Tricuspid Gate, and I tear free of the sticky rags of my old flesh. My new body is smooth without and red within like a sweet fruit." +
                " My limbs are strong as cables. My senses are knives. I will not grow old. I will walk the world in the service of the Grail, feasting, growing. Perhaps I will rebel. Perhaps, one day, I will rise even higher." +
                " [Congratulations on a standard Sensation victory. You have wrestled the game to its knees. There are other paths.] ", "redgrail", EndingFlavour.Grand, "DramaticLightCool", "A_ENDING_MINORGRAILVICTORY");

        if (endingFlag == "minorlanternvictory")
            return new Ending(endingFlag, "THE INCURSUS",
                "I have passed through the Tricuspid Gate, and entered the high rooms of the Mansus. I will not live, but neither will I die. The Glory is very close here. It leaks through the fabric of the House to contribute its light. " +
                " One day - perhaps one day soon - the Pilgrimage will conclude, and the Watchman will permit seven souls to ascend further. The Hour called Vagabond will be the first. Perhaps I will be the seventh." +
                " [Congratulations on a standard Enlightenment victory. You have wrestled the game to its knees. There are other paths.] ", "doorintheeye", EndingFlavour.Grand, "DramaticLightCool", "A_ENDING_MINORLANTERNVICTORY");

        if (endingFlag == "minorforgevictory_withrisen")
            return new Ending(endingFlag, "THE CONFLAGRATION OF THE HEART",
                "For a little while I dwell in the high rooms of the Mansus, and then I return through the Tricuspid Gate, and my body stirs in the ashes. I am hairless and imperishable as marble, and the Forge's fire still burns within me. " +
                "I carry the Shaping Strength... and something else besides. The flesh of my dead beloved was consumed in the flames, and now they will always be a part of me, like the tin that hardens the bronze. We will not grow old. Perhaps we shall rebel. Perhaps, one day, we will rise even higher." +
                " [Congratulations on a standard Power victory. You have wrestled the game to its knees. There are other paths.] ", "forgeofdays", EndingFlavour.Grand, "DramaticLightCool", "A_ENDING_MINORFORGEVICTORY_WITHRISEN");

        if (endingFlag == "minorgrailvictory_withrisen")
            return new Ending(endingFlag, "THE FEAST OF THE TRUE BIRTH",
                "For a little while I dwell in the high rooms of the Mansus, and then I return through the Tricuspid Gate, and I tear free of the sticky rags of my old flesh. My new body is smooth without and red within like a sweet fruit." +
                " My limbs are strong as cables. My senses are knives. I will not grow old. I will walk the world in the service of the Grail, feasting, growing, and my dead beloved will walk beside me, sharing in my feasts, until they become something altogether new. Perhaps we shall rebel. Perhaps, one day, we will rise even higher." +
                " [Congratulations on a standard Sensation victory. You have wrestled the game to its knees. There are other paths.] ", "redgrail", EndingFlavour.Grand, "DramaticLightCool", "A_ENDING_MINORGRAILVICTORY_WITHRISEN");

        if (endingFlag == "minorlanternvictory_withrisen")
            return new Ending(endingFlag, "THE INCURSUS",
                "I have passed through the Tricuspid Gate, and entered the high rooms of the Mansus. I will not live, but neither will I die. My dead beloved follows me like a shadow - and in the heart of this light, shadows burn all the deeper. The Glory is very close here. It leaks through the fabric of the House to contribute its light. " +
                " One day - perhaps one day soon - the Pilgrimage will conclude, and the Watchman will permit seven souls to ascend further. The Hour called Vagabond will be the first.  Perhaps my companion and I will join her" +
                " [Congratulations on a standard Enlightenment victory. You have wrestled the game to its knees. There are other paths.] ", "doorintheeye", EndingFlavour.Grand, "DramaticLightCool", "A_ENDING_MINORLANTERNVICTORY_WITHRISEN");



        if (endingFlag=="deathofthebody")
            return new Ending(endingFlag, "MY BODY IS DEAD",
                "Where will they find me? I am not here. In the end, my strength was insufficient to sustain my failing heart. [I was starving, and I had no Health remaining. I should have " +
                "ensured I had money to purchase essentials; I could have used Dream to rest and recover from my weakness.]", "suninrags", EndingFlavour.Melancholy, "DramaticLightEvil", "A_ENDING_DEATHOFTHEBODY");

        if (endingFlag == "despairending")
            return new Ending(endingFlag, "NO MORE",
                "Despair, the wolf that devours thought. Am I alive, or am I dead? It no longer matters. [I allowed the Despair token to reach 3 Dread or Injury.]", "despair", EndingFlavour.Melancholy, "DramaticLightEvil", "A_ENDING_DESPAIRENDING");
        if (endingFlag == "visionsending")
            return new Ending(endingFlag, "GLORY",
                "First it was the dreams. Then it was the visions. Now it's everything. I no longer have any idea what is real, and what is not. [I allowed the Visions token to reach 3 Fascination.]", "madness", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_VISIONSENDING");

        if (endingFlag == "wintersacrifice")
            return new Ending(endingFlag, "GOING QUIETLY",
                "In the upper room of the house where I am taken, my breath fogs and my eyes grow soft. The light in the room is the light at the end of the sun. I am a beautiful ending.", "suninrags", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_WINTERSACRIFICE");

        if (endingFlag == "arrest") 
            return new Ending(endingFlag, "BARS ACROSS THE SUN",
                "The nature of my crimes was vague, and the trial contentious. But there is a consensus that I have done something I should not. I wish it could have been different. I wish " +
                " that I could have done <i>everything</i> I should not. [Many Hunters have specific weaknesses. Perhaps you can use those weaknesses to stop them before they bring you to trial.]", "notorious", EndingFlavour.Melancholy, "DramaticLightEvil", "A_ENDING_ARREST"
                );

        if (endingFlag == "rivalascension")
            return new Ending(endingFlag, "NOT LONG ENOUGH",
                "Perhaps I waited too long. Or perhaps this victory was never meant for me. Another has ascended in my place. Where they have risen, " +
                "I will diminish, until I am only a memory. Perhaps another will avenge my defeat. ['Heaven hath no rage like love to hatred turned.' Your one-time ally turned against you, and surpassed you.]", "lionsmith", EndingFlavour.Melancholy, "DramaticLightEvil", "A_RIVAL_ASCENSION"
            );


        if (endingFlag == "workvictory")
            return new Ending(endingFlag, "EVENING FALLS",
                "I have my fire, my books, my clock, my window on the world where they do other things. I could have been unhappy. I'm not unhappy. This was a successful life, and when it is " +
                "over the sweet earth will fill my mouth, softer than splinters. [This might be considered a victory.]", "insomnia", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_WORKVICTORY");

        if (endingFlag == "workvictoryb")
            return new Ending(endingFlag, "AMBITION'S TIDE",
                "I will rise high, and higher yet. My affairs will prosper. I will fill a fine house with elegant things. I will be honoured by my peers and slandered by my  rivals. I will grow " +
                "used to the sound of my name. Then one day, I will die, and some time after that, my name will be heard no more. [This should be considered a minor victory.]", "finehouse", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_WORKVICTORYB");
        if (endingFlag == "workvictorymarriage")
            return new Ending(endingFlag, "A NEW LIFE",
                "I was poor, and now I am rich. I was something common, and now I am something rare. On summer mornings the hills outside my bedroom window are green with sun and on winter mornings they are white with snow and over all of it is the sound of " +
                "the laughter of our heirs, and the sound of the wind. The wind rattles the casements and runs into the hills and is gone. [This should be considered a minor victory.]", "moonlitcastle", EndingFlavour.Grand, "DramaticLight", "A_ENDING_WORKVICTORYMARRIAGE");

        if(endingFlag=="minorheartvictory")
            return new Ending(endingFlag,"LIFE, UNENDING",
                "As the pain fades, so does my voice, but now my heart will speak for me. In the scales of Time shall it be weighed against a feather, and it shall not be found wanting. In my final shape I shall pass the Tricuspid Gate, and add my heart's beat to the Thunderskin's chorus. " +
                "Our rhythm is the rhythm of the Hours: and the Hours have promised that we shall endure with the world unceasing. I move eternally through the Mansus, and in eternity is my constancy assured. " +
                "[Congratulations on a standard Change: Feather victory. You have wrestled the game to its knees. There are other paths.] ", "thunderskin",EndingFlavour.Grand,"DramaticLight","A_ENDING_MINORHEARTVICTORY");

        if (endingFlag == "minormothvictory")
            return new Ending(endingFlag, "THINGS WITH WINGS",
                "The Carapace Cross is gone, extinct as the dodo or the dragon. Only humans remain. But still my wings unfurl, and still my skin has hardened to scales, and still the facets of my eyes are shining anthracite. I have not passed " +
                "the Tricuspid Gate; I have gone down into death and returned alive. Here in the dark we will remain, where we cannot be seen, until at last we can no longer reject the Glory. Into the fire we fly. The Cross is imaginary; the change is not. " +
                "[Congratulations on a standard Change: Scale victory. You have wrestled the game to its knees. There are other paths.] ", "moth", EndingFlavour.Grand, "DramaticLight", "A_ENDING_MINORMOTHVICTORY");

        if (endingFlag == "turnasidevictory")
            return new Ending(endingFlag, "AFTER HAPPILY",
                "FNORD","moth", EndingFlavour.Grand, "DramaticLight", "A_ENDING_MINORMOTHVICTORY");

        if (endingFlag == "tristanvictory")
            return new Ending(endingFlag, "EVER AFTER: TRISTAN",
                "Our flesh may tire. We may grow old. But I will not regret. He is seared into my memory: the sure, strong strokes of handwritten notes; the shape he leaves in pillows; the supple swell of his arms by the fire. Brightest of all is his body beside mine, flush with the flames of the dawn." +
                " [This might be considered a victory. But the House is no place for lovers.]", "tristan", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_TRISTANVICTORY");

        if (endingFlag == "valcianevictory")
            return new Ending(endingFlag, "EVER AFTER: VALCIANE",
                "She fills our lives with shapely things: treasures of porcelain, nacre, blown glass. Some are the products of her work. Some are tokens from trips abroad. They collect in piles around our home, forming curious passageways. But I find her among the mazes she builds. She wants very much to be found." +
                " [This might be considered a victory. But the House is no place for lovers.]", "valciane", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_VALCIANEVICTORY");

        if (endingFlag == "laidlawvictory")
            return new Ending(endingFlag, "EVER AFTER: LAIDLAW",
                "We have our patterns. Our visits to the auction house, the flickering films we watch together, the rhythm we know in the night. But he is unpredictable as the sparking of wood in the hearth. When next will he spit embers? When next will his fires cool? I shape myself around his flares. We find, we think, a way." +
                " [This might be considered a victory. But the House is no place for lovers.]", "laidlaw", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_LAIDLAWVICTORY");

        if (endingFlag == "elridgevictory")
            return new Ending(endingFlag, "EVER AFTER: ELRIDGE",
                "Our home is ordered, polished, controlled. He knows always where the tools are that we need. He knows afterwards to clean, to put them back once more. Sometimes I must wait, while he does the things he must do. But when he is finished, he may look up at me, and I may give him his reward." +
                " [This might be considered a victory. But the House is no place for lovers.]", "elridge", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_ELRIDGEVICTORY");

        if (endingFlag == "rosevictory")
            return new Ending(endingFlag, "EVER AFTER: ROSE",
                "Our lives are happy ones. We seem a suited pair. There are, of course, those visits from her brother - but she is lonely when I travel, and needs the company. When I return her eyes are softer than before, and her kisses wine and caramel." +
                " [This might be considered a victory. But the House is no place for lovers.]", "rose", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_ROSEVICTORY");

        if (endingFlag == "victorvictory")
            return new Ending(endingFlag, "EVER AFTER: VICTOR",
                "Our evenings together are elegant things. The sweep of the stairwell all glittered with gold. The sighs of the audience close to the stage. The chink of our knives, further back, through fillet. We often run into his sister, whose tastes mirror our own. But when we are alone again, he lavishes his gaze on me. He offers kisses, caresses, caramel. We are happy, I think." +
                " [This might be considered a victory. But the House is no place for lovers.]", "victor", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_VICTORVICTORY");

        if (endingFlag == "salibavictory")
            return new Ending(endingFlag, "EVER AFTER: SALIBA",
                "I would not say it ended badly. But there is little of me, now. Our outside activities waned over time, and so, I know, have I. As I dwindle, my lover takes greater and greater interest in me: these days he hardly leaves my side, and I, hardly, his room. Soon, he tells me, a smear of something wet beneath his eye. Soon." +
                " [This might be considered a victory. But the House is no place for lovers.]", "saliba", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_SALIBAVICTORY");

        if (endingFlag == "reniravictory")
            return new Ending(endingFlag, "EVER AFTER: RENIRA",
                "I am drunk with her. Bare shoulders at the opera, a hundred ruby smiles across a crowded restaurant. Her smile dims if I am caught, even for the shortest of times, admiring another pretty thing. But she knows she is the beating heart of me, the lifeblood of my work. I am lost in her lovely arms, her mouth as sweet as myrrh. Hers is my heart. My goddess. She." +
                " [This might be considered a victory. But the House is no place for lovers.]", "renira", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_RENIRAVICTORY");

        if (endingFlag == "violetvictory")
            return new Ending(endingFlag, "EVER AFTER: VIOLET",
                "The house is alive with candles. But they never last that long. She lies with me in the guttering dark, as our windows frost with breath, with ice, with the silence of the night. One by one we douse the flames and listen to the sound of nothingness, clasped in each other's arms." +
                " [This might be considered a victory. But the House is no place for lovers.]", "violet", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_VIOLETVICTORY");

        if (endingFlag == "auclairvictory")
            return new Ending(endingFlag, "EVER AFTER: AUCLAIR",
                "My mind is crowded with her. The catechisms over soup, the dialogues of daily life. What once were strolls beneath the moon are now our cherished memories, of her and I before we truly knew ourselves. Now we know. Now we are scholars of the heart. Now we are each other's." +
                " [This might be considered a victory. But the House is no place for lovers.]", "auclair", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_AUCLAIRVICTORY");

        if (endingFlag == "enidvictory")
            return new Ending(endingFlag, "EVER AFTER: ENID",
                "She does not talk, when she is sad. But she is often sad. I know now to place myself, know to leave my hand just so, and coax that lovely neck upon my shoulder. Our lives are mottled with moments like this, unspoken, unseen, us. And in the morning, when the day before her lies open and wide, she smiles the smile only I know and I am well repaid." +
                " [This might be considered a victory. But the House is no place for lovers.]", "enid", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_ENIDVICTORY");

        if (endingFlag == "nevillevictory")
            return new Ending(endingFlag, "EVER AFTER: NEVILLE",
                "I find notes among my papers, chocolates at my desk. When we dine, he sits close beside me, no matter the remonstrations of the waiter. I have sometimes caught him, blue in the dawn, stroking a lock of my hair. I kiss him, then; I make him mine. He thanks me afterwards." +
                " [This might be considered a victory. But the House is no place for lovers.]", "neville", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_NEVILLEVICTORY");

        if (endingFlag == "catvictory")
            return new Ending(endingFlag, "EVER AFTER: CAT",
                "Our house is ever stocked with what we need, before I know we need it. It is hard to know whether my desires are anticipated before I am aware of them, or whether she molds them from the start. But I sometimes catch her lost in thought, haloed in lamp-light and smoke. I know to move quietly, and chink our cups together when I carry them, so she is not startled. She laughs at me for this, but ensures I have sweet dreams." +
                " [This might be considered a victory. But the House is no place for lovers.]", "cat", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_CATVICTORY");

        if (endingFlag == "cliftonvictory")
            return new Ending(endingFlag, "EVER AFTER: CLIFTON",
                "It is hard to tell, with him, whether to laugh or cry. Sometimes he stumbles in at dawn, dripping Turkish cigarettes. Other times he drapes our home with hanging lamps, and may or may not persuade a battered gramophone to play. Those nights, we sample food, and then each other, in the glittering gloom of the night." +
                " [This might be considered a victory. But the House is no place for lovers.]", "clifton", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_CLIFTONVICTORY");

        if (endingFlag == "sleevictory")
            return new Ending(endingFlag, "EVER AFTER: SLEE",
                "Our home is littered with his notes. They touch upon the grandnesses of love, of birth, of everything. Among them we laugh at limericks and fall drunkenly to bed. When we wake, we are cocooned, warm in the crooks of each other. Sometimes I nudge a scrap of paper out from the coverlet. Sometimes I let it stay." +
                " [This might be considered a victory. But the House is no place for lovers.]", "slee", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_SLEEVICTORY");

        if (endingFlag == "portervictory")
            return new Ending(endingFlag, "EVER AFTER: PORTER",
                "He is very specific about what he does, and does not, enjoy. I learn his ways: to draw in circles by his ear, to vex until he lights his pipe, to sweeten his tempers with sugared delights. I will never fully grasp this man, as he will not grasp me. But our years are speckled with what some might call love, and that is enough." +
                " [This might be considered a victory. But the House is no place for lovers.]", "porter", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_PORTERVICTORY");

        if (endingFlag == "ysabetvictory")
            return new Ending(endingFlag, "EVER AFTER: YSABET",
                "Our lives are rich, chaotic days. I note appointments in her diary several hours before we're due to meet, but still she is rarely on time. I lie in bed, awaiting her, only to wake in the early dawn as she slides, cold, beside me. But her smile is warm against my neck. Her eyes are hot with love for me. Our house is home to fluttering things, and she must beat her wings." +
                " [This might be considered a victory. But the House is no place for lovers.]", "ysabet", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_YSABETVICTORY");

        if (endingFlag == "sylviavictory")
            return new Ending(endingFlag, "EVER AFTER: SYLVIA",
                "Life is not easy with her. There are arguments. There are times when I roam the streets, seeking whichever bright window -  that night - frames her and another. But she is unmercifully charming when she returns. There is always cause. We are drawn back together, again and again, and blot out the world with desire." +
                " [This might be considered a victory. But the House is no place for lovers.]", "sylvia", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_SYLVIAVICTORY");

        if (endingFlag == "clovettevictory")
            return new Ending(endingFlag, "EVER AFTER: CLOVETTE",
                "My life follows the path of her joy - powerful, merciless, hers. I must sometimes remind her of our vows, but she is always regretful, and always irresistible. I am rich in the glow of her attention, once I have pulled her from the parties we attend each night. And when she is mine, oh, I am hers, and our heartbeats quicken together." +
                " [This might be considered a victory. But the House is no place for lovers.]", "clovette", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_CLOVETTEVICTORY");

        if (endingFlag == "dorothyvictory")
            return new Ending(endingFlag, "EVER AFTER: DOROTHY",
                "There are lulls in the music between us, but there are far more crescendos than falls. We live together, we travel as one, we fall naturally into perfect step. Before we sleep we often dance, to the low, slow rhythm of the streets outside. Those nights we dream the same pink dreams and waken, flushed, in the dawn." +
                " [This might be considered a victory. But the House is no place for lovers.]", "dorothy", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_DOROTHYVICTORY");

        if (endingFlag == "leovictory")
            return new Ending(endingFlag, "EVER AFTER: LEO",
                "His love is irrepressible. It sprouts like weeds in sunny spots and cannot be constrained. I was not entirely certain, to begin with, what was the moon, and what the man. But he has sown our home with richness. He has filled it with trinkets from our shared life. He is the melody of love, and I hear him now." +
                " [This might be considered a victory. But the House is no place for lovers.]", "leo", EndingFlavour.Melancholy, "DramaticLight", "A_ENDING_LEOVICTORY");

        return Ending.DefaultEnding();
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

        foreach (var r in _recipes)
        {
            
            r.Label = tr.ReplaceTextFor(r.Label);
            r.StartDescription = tr.ReplaceTextFor(r.StartDescription);
            r.Description = tr.ReplaceTextFor(r.Description);
        }

        foreach (var k in _elements.Keys)
        {
            var e = _elements[k];
            e.Label = tr.ReplaceTextFor(e.Label);
            e.Description = tr.ReplaceTextFor(e.Description);

        }
       
    }
}
