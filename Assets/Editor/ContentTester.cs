using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Noon;
using UnityEditor;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace Assets.Editor
{
    public class ContentTester : ISituationSimulatorSubscriber
    {
        private readonly SituationSimulator _simulator;
        private ContentTest _currentContentTest;
        private List<SituationRecipeSpec> _remainingExpectedRecipes;
        private Recipe _currentRecipe;

        private ContentTester()
        {
            _simulator = new SituationSimulator(this);
        }

        [MenuItem("Tools/Validate Content Assertions %#g")]
        private static void ValidateContentAssertions()
        {
            // Clear the console of previous messages to reduce confusion
            // Also disable all logging so that the console isn't polluted with other messages
            EditorUtils.ClearConsole();
            NoonUtility.CurrentVerbosity = -1;

            // Load all the content tests
            List<string> contentFilePaths = Directory.GetFiles(
                Path.Combine(Application.dataPath, "Editor/ContentTests")).ToList().FindAll(f => f.EndsWith(".yaml"));
            List<ContentTest> contentTests = new List<ContentTest>();
            foreach (var path in contentFilePaths)
            {
                var contentFilePath = Path.GetFullPath(path);
                NoonUtility.Log("Loading content tests in " + contentFilePath, -1);

                // Read the YAML
                var reader = new StringReader(File.ReadAllText(contentFilePath));
                var yaml = new YamlStream();
                yaml.Load(reader);

                // Load the list of content tests
                var testIdx = 0;
                foreach (var document in yaml.Documents)
                {
                    var root = (YamlSequenceNode) document.RootNode;
                    contentTests.AddRange(
                        root.Children.Select(node => ContentTest.FromYaml(contentFilePath, ++testIdx, (YamlMappingNode) node)));
                }
            }

            // Start the simulator
            ContentTester tester = new ContentTester();
            NoonUtility.Log("Running " + contentTests.Count + " content tests.", -1);
            int numFailedTests = 0;
            foreach (var test in contentTests)
            {
                try
                {
                    tester.RunTest(test);
                }
                catch (SituationSimulatorException e)
                {
                    NoonUtility.Log("Failed content test: " + test.Id + "\n" + e.Message, -1, messageLevel: 2);
                    numFailedTests++;
                }
            }
            NoonUtility.Log("Testing complete. " + numFailedTests + " failed tests.", -1, messageLevel: numFailedTests > 0 ? 1 : 0);
        }

        private void RunTest(ContentTest test)
        {
            // Set up our assertions
            _currentContentTest = test;
            _remainingExpectedRecipes = _currentContentTest.ExpectedRecipes != null ?
                new List<SituationRecipeSpec>(_currentContentTest.ExpectedRecipes) : null;

            // Run the test, which will call back to the event functions
            Dictionary<string, SimulatedElementStack> additionalSlots = null;
            if (test.StartingAdditionalSlotElements != null)
                additionalSlots = test.StartingAdditionalSlotElements.ToDictionary(
                    p => p.Key, p => BuildSlotStack(p.Value));
            _simulator.RunSituation(
                test.ActionId,
                BuildSlotStack(test.StartingPrimarySlotElement),
                additionalSlots);
        }

        public void OnRecipeStarted(Recipe recipe, SimulatedSlotsManager ongoingSlotsManager)
        {
            _currentRecipe = recipe;  // Save it so that the expulsion can be checked against its spec, if any
            SituationRecipeSpec expectedRecipe = GetExpectedRecipeSpecIfNext(recipe);
            if (expectedRecipe == null)
                return;

            // Try to fill in the slot if this is expected
            if (expectedRecipe.Slot == null)
                return;

            // Create the stack, with all appropriate mutations
            SimulatedElementStack stack = new SimulatedElementStack();
            stack.Populate(expectedRecipe.Slot.ElementId, 1, Source.Existing());
            foreach (var mutation in expectedRecipe.Slot.Mutations)
                stack.SetMutation(mutation.Key, mutation.Value, false);

            if (!ongoingSlotsManager.TryAddStackToSlot(null, stack))
                throw new SituationSimulatorException(
                    "Failed to add '" + stack.EntityId + "' to ongoing slot for recipe '" + recipe.Id + "'");
        }

        public int OnRecipeRollRequested(Recipe recipe)
        {
            if (recipe == null)
                return 0;
            SituationRecipeSpec expectedRecipe = GetExpectedRecipeSpecIfNext(recipe);
            return expectedRecipe == null ? 0 : expectedRecipe.Roll;
        }

        public void OnRecipeExecuted(Recipe recipe)
        {
            SituationRecipeSpec expectedRecipe = GetExpectedRecipeSpecIfNext(recipe);
            if (expectedRecipe == null)
                return;
            _remainingExpectedRecipes.Remove(expectedRecipe);
        }

        public void OnRecipeExpulsion(IVerb verb, Recipe recipe, List<IElementStack> stacks)
        {
            SituationRecipeSpec expectedRecipe = GetExpectedRecipeSpecIfNext(_currentRecipe);
            if (expectedRecipe == null || expectedRecipe.Expulsions == null)
                return;

            // Get the first expulsion that matches the verb and recipe
            // This might be a problem if there are two expected expulsions of the same recipe in two different,
            // unspecified verbs, but this seems like an edge-case and is unlikely to occur
            SituationExpulsionSpec expectedExpulsion = null;
            foreach (var expulsionSpec in expectedRecipe.Expulsions)
            {
                if (expulsionSpec.ActionId != null && expulsionSpec.ActionId != verb.Id)
                    continue;
                if (expulsionSpec.RecipeId != null && expulsionSpec.RecipeId != recipe.Id)
                    continue;

                expectedExpulsion = expulsionSpec;
            }
            if (expectedExpulsion == null)
                return;

            // Check for the presence of the expected stacks
            CheckResults(expectedExpulsion.Stacks, stacks);
        }

        public void OnSituationCompleted(IEnumerable<IElementStack> outputStacks, string outputText)
        {
            // Check that all the expected recipes were encountered
            if (_remainingExpectedRecipes != null)
                foreach (var remainingRecipe in _remainingExpectedRecipes)
                    throw new SituationSimulatorException(
                        "Expected '" + remainingRecipe.RecipeId + "' but was never encountered");

            // Check that the output elements are as expected, with the right quantities
            // Since quantities might be split across multiple stacks, we first compile quantities by element,
            // then run comparisons against them
            if (_currentContentTest.ExpectedResults != null)
            {
                CheckResults(_currentContentTest.ExpectedResults, outputStacks);
            }

            // Check that the output text matches
            if (_currentContentTest.ExpectedResultText != null && !_currentContentTest.ExpectedResultText.Contains(outputText))
                throw new SituationSimulatorException(
                    "Expected output text to contain '" + _currentContentTest.ExpectedResultText + "' but got '" + outputText + "'");
        }

        private SituationRecipeSpec GetExpectedRecipeSpecIfNext(Recipe recipe)
        {
            if (_remainingExpectedRecipes == null || _remainingExpectedRecipes.Count == 0)
                return null;

            // Only consider the topmost recipe as a candidate, since every previous recipe needs to have been
            // successfully executed
            SituationRecipeSpec expectedRecipe = _remainingExpectedRecipes.First();
            if (expectedRecipe == null)
                return null;
            return expectedRecipe.RecipeId != recipe.Id ? null : expectedRecipe;
        }

        private static void CheckResults(List<SituationResultSpec> expectedResults, IEnumerable<IElementStack> stacks)
        {
            Dictionary<SituationResultSpec, int> encounteredQuantities = new Dictionary<SituationResultSpec, int>();
            foreach (var outputStack in stacks)
            {
                // Find a suitable candidate, based on both entity ID and present mutations
                // The first matching candidate is always selected
                SituationResultSpec expectedResult = null;
                Dictionary<string, int> mutations = outputStack.GetCurrentMutations();
                foreach (var er in expectedResults)
                {
                    // Filter by element ID first
                    if (er.ElementId != outputStack.EntityId)
                        continue;
                    expectedResult = er;

                    // Filter by mutations, setting the result to null if any of the mutation criteria are not
                    // fulfilled
                    foreach (var mutationSpec in er.Mutations)
                    {
                        string mutElId = mutationSpec.Key;
                        int mutQuantity = mutationSpec.Value;
                        mutations.TryGetValue(mutElId, out var actualQuantity);  // Will set a default of 0 if not found
                        if (mutQuantity == actualQuantity)
                            continue;
                        expectedResult = null;
                        break;
                    }
                }
                if (expectedResult == null)
                    continue;  // Ignore any unexpected results
                int oldQuantity;
                encounteredQuantities.TryGetValue(expectedResult, out oldQuantity);
                encounteredQuantities[expectedResult] = oldQuantity + outputStack.Quantity;
            }

            foreach (var expectedResult in expectedResults)
            {
                int quantity = 0;
                if (encounteredQuantities.ContainsKey(expectedResult))
                {
                    quantity = encounteredQuantities[expectedResult];
                }

                bool test = false;
                string message = null;
                switch (expectedResult.Op)
                {
                    case ComparisonOperator.LessThan:
                        test = quantity < expectedResult.Quantity;
                        message = "less than";
                        break;
                    case ComparisonOperator.LessThanOrEqual:
                        test = quantity <= expectedResult.Quantity;
                        message = "less than or equal to";
                        break;
                    case ComparisonOperator.Equal:
                        test = quantity == expectedResult.Quantity;
                        message = "equal to";
                        break;
                    case ComparisonOperator.GreaterThan:
                        test = quantity > expectedResult.Quantity;
                        message = "greater than";
                        break;
                    case ComparisonOperator.GreaterThanOrEqual:
                        test = quantity >= expectedResult.Quantity;
                        message = "greater than or equal";
                        break;
                }
                if (!test)
                    throw new SituationSimulatorException(
                        "Was expecting '" + expectedResult.ElementId + "' "
                        + (expectedResult.Mutations.Count > 0 ? "(with mutations) " : "")
                        + "to be " + message + " " + expectedResult.Quantity + " but was " + quantity);
            }
        }

        private static SimulatedElementStack BuildSlotStack(SituationSlotSpec spec)
        {
            Element element = Registry.Retrieve<ICompendium>().GetElementById(spec.ElementId);
            if (element == null)
                throw new SituationSimulatorException("Unknown element '" + spec.ElementId + "' in slot");
            SimulatedElementStack stack = new SimulatedElementStack();
            stack.Populate(element, 1, Source.Existing());
            foreach (var mutation in spec.Mutations)
                stack.SetMutation(mutation.Key, mutation.Value, false);
            return stack;
        }
    }

    internal class ContentTest
    {
        public string Id { get; private set; }

        public string ActionId { get; private set; }

        public SituationSlotSpec StartingPrimarySlotElement { get; private set; }

        public Dictionary<string, SituationSlotSpec> StartingAdditionalSlotElements { get; private set; }

        public List<SituationRecipeSpec> ExpectedRecipes { get; private set; }

        public List<SituationResultSpec> ExpectedResults { get; private set; }

        public string ExpectedResultText { get; private set; }

        public static ContentTest FromYaml(string filePath, int entryNum, YamlMappingNode data)
        {
            ContentTest test = new ContentTest
            {
                Id = Path.GetFileNameWithoutExtension(filePath) + "_" + entryNum,
                ActionId = data.Children["action"].ToString(),
                StartingPrimarySlotElement = SituationSlotSpec.FromYaml(data.Children["slot"])
            };

            if (data.Children.ContainsKey("aSlots"))
                test.StartingAdditionalSlotElements = ((YamlMappingNode) data.Children["aSlots"]).ToDictionary(
                    p => p.Key.ToString(), p => SituationSlotSpec.FromYaml(p.Value));

            if (data.Children.ContainsKey("recipes"))
                test.ExpectedRecipes = ((YamlSequenceNode) data.Children["recipes"]).Children.Select(
                    SituationRecipeSpec.FromYaml).ToList();

            if (data.Children.ContainsKey("output"))
                test.ExpectedResults = ((YamlSequenceNode) data.Children["output"]).Children.Select(
                    SituationResultSpec.FromYaml).ToList();

            if (data.Children.ContainsKey("text"))
                test.ExpectedResultText = data.Children["text"].ToString();

            return test;
        }
    }

    internal class SituationSlotSpec
    {
        public string ElementId { get; private set; }

        public Dictionary<string, int> Mutations { get; private set; }

        public static SituationSlotSpec FromYaml(YamlNode data)
        {
            // Is it a dictionary?
            var mappingNode = data as YamlMappingNode;
            if (mappingNode != null)
                return FromYaml(mappingNode);

            // Is it just the element ID?
            var node = data as YamlScalarNode;
            return node != null ? FromYaml(node) : null;
        }

        private static SituationSlotSpec FromYaml(YamlMappingNode data)
        {
            var slotSpec = new SituationSlotSpec()
            {
                ElementId = data.Children["id"].ToString(),
                Mutations = new Dictionary<string, int>(),
            };
            if (data.Children.ContainsKey("mutations"))
                slotSpec.Mutations =
                    ((YamlMappingNode) data.Children["mutations"]).ToDictionary(
                        p => p.Key.ToString(),
                        p => int.Parse(p.Value.ToString()));
            return slotSpec;
        }

        private static SituationSlotSpec FromYaml(YamlScalarNode data)
        {
            var slotSpec = new SituationSlotSpec()
            {
                ElementId = data.ToString(),
                Mutations = new Dictionary<string, int>()
            };
            return slotSpec;
        }
    }

    internal class SituationRecipeSpec
    {
        public string RecipeId { get; private set; }

        public SituationSlotSpec Slot { get; private set; }

        public int Roll { get; private set; }

        public List<SituationExpulsionSpec> Expulsions { get; private set; }

        public static SituationRecipeSpec FromYaml(YamlNode data)
        {
            // Is it a dictionary?
            var mappingNode = data as YamlMappingNode;
            if (mappingNode != null)
                return FromYaml(mappingNode);

            // Is it just the recipe ID?
            var node = data as YamlScalarNode;
            return node != null ? FromYaml(node) : null;
        }

        private static SituationRecipeSpec FromYaml(YamlMappingNode data)
        {
            var recipeSpec = new SituationRecipeSpec()
            {
                RecipeId = data.Children["id"].ToString()
            };

            if (data.Children.ContainsKey("slot"))
                recipeSpec.Slot = SituationSlotSpec.FromYaml(data.Children["slot"]);

            if (data.Children.ContainsKey("roll"))
                recipeSpec.Roll = int.Parse(data.Children["roll"].ToString());

            if (data.Children.ContainsKey("expulsions"))
                recipeSpec.Expulsions = ((YamlSequenceNode) data.Children["expulsions"]).Children.Select(
                    SituationExpulsionSpec.FromYaml).ToList();

            return recipeSpec;
        }

        private static SituationRecipeSpec FromYaml(YamlScalarNode data)
        {
            var recipeSpec = new SituationRecipeSpec()
            {
                RecipeId = data.ToString(),
                Roll = 0
            };
            return recipeSpec;
        }
    }

    internal class SituationExpulsionSpec
    {
        public string ActionId { get; private set; }

        public string RecipeId { get; private set; }

        public List<SituationResultSpec> Stacks { get; private set; }

        public static SituationExpulsionSpec FromYaml(YamlNode data)
        {
            // Is it a dictionary?
            var mappingNode = data as YamlMappingNode;
            if (mappingNode != null)
                return FromYaml(mappingNode);

            // Is it just the element ID?
            var node = data as YamlScalarNode;
            return node != null ? FromYaml(node) : null;
        }

        private static SituationExpulsionSpec FromYaml(YamlMappingNode data)
        {
            var expulsionSpec = new SituationExpulsionSpec()
            {
                ActionId = null,
                RecipeId = null,
                Stacks = new List<SituationResultSpec>()
            };

            if (data.Children.ContainsKey("action"))
                expulsionSpec.ActionId = data.Children["action"].ToString();

            if (data.Children.ContainsKey("recipe"))
                expulsionSpec.RecipeId = data.Children["recipe"].ToString();

            if (data.Children.ContainsKey("expelled"))
                expulsionSpec.Stacks = ((YamlSequenceNode) data.Children["expelled"]).Children.Select(
                    SituationResultSpec.FromYaml).ToList();

            return expulsionSpec;
        }

        private static SituationExpulsionSpec FromYaml(YamlScalarNode data)
        {
            var expulsionSpec = new SituationExpulsionSpec()
            {
                ActionId = null,
                RecipeId = data.ToString(),
                Stacks = new List<SituationResultSpec>()
            };
            return expulsionSpec;
        }
    }

    internal class SituationResultSpec
    {
        private static readonly Regex QuantityRegex = new Regex(@"^\s*([<>=]+)?\s*(\d+)\s*$");

        public string ElementId { get; private set; }

        public Dictionary<string, int> Mutations { get; private set; }

        public ComparisonOperator Op { get; private set; }

        public int Quantity { get; private set; }

        public static SituationResultSpec FromYaml(YamlNode data)
        {
            // Is it a dictionary?
            var mappingNode = data as YamlMappingNode;
            if (mappingNode != null)
                return FromYaml(mappingNode);

            // Is it just the element ID?
            var node = data as YamlScalarNode;
            return node != null ? FromYaml(node) : null;
        }

        private static SituationResultSpec FromYaml(YamlMappingNode data)
        {
            var resultSpec = new SituationResultSpec()
            {
                ElementId = data.Children["id"].ToString(),
                Mutations = new Dictionary<string, int>(),
                Quantity = 1,
                Op = ComparisonOperator.Equal
            };


            if (data.Children.ContainsKey("mutations"))
                resultSpec.Mutations =
                    ((YamlMappingNode) data.Children["mutations"]).ToDictionary(
                        p => p.Key.ToString(),
                        p => int.Parse(p.Value.ToString()));

            // Check if the quantity is specified a simple number or a comparison
            if (data.Children.ContainsKey("quantity"))
            {
                string quantityField = data.Children["quantity"].ToString();
                Match match = QuantityRegex.Match(quantityField);
                if (!match.Success)
                    throw new SituationSimulatorException("Invalid quantity format: '" + quantityField + "'");
                if (match.Groups[1].Length > 0)
                    switch (match.Groups[1].Value)
                    {
                        case "<":
                            resultSpec.Op = ComparisonOperator.LessThan;
                            break;
                        case "<=":
                            resultSpec.Op = ComparisonOperator.LessThanOrEqual;
                            break;
                        case "==":
                            resultSpec.Op = ComparisonOperator.Equal;
                            break;
                        case ">":
                            resultSpec.Op = ComparisonOperator.GreaterThan;
                            break;
                        case ">=":
                            resultSpec.Op = ComparisonOperator.GreaterThanOrEqual;
                            break;
                        default:
                            throw new SituationSimulatorException("Unexpected quantity operator: '" + match.Groups[1].Value + "' in '" + quantityField + "'");
                    }
                resultSpec.Quantity = int.Parse(match.Groups[2].Value);
            }
            return resultSpec;
        }

        private static SituationResultSpec FromYaml(YamlScalarNode data)
        {
            var resultSpec = new SituationResultSpec()
            {
                ElementId = data.ToString(),
                Mutations = new Dictionary<string, int>(),
                Quantity = 1,
                Op = ComparisonOperator.Equal
            };
            return resultSpec;
        }
    }

    internal enum ComparisonOperator
    {
        LessThan,
        LessThanOrEqual,
        Equal,
        GreaterThan,
        GreaterThanOrEqual,
    }
}
