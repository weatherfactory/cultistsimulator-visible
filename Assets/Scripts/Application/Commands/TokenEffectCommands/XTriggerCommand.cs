using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Interfaces;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
  public  class XTriggerCommand: IAffectsTokenCommand
    {

        public AspectsDictionary AspectsPresent { get; set; }
        public IDice UseDice { get; set; }
        public Sphere InSphere { get; set; }

        public XTriggerCommand(AspectsDictionary aspectsPresent,IDice useDice, Sphere inSphere)
        {
            AspectsPresent = aspectsPresent;
            UseDice = useDice;
            InSphere = inSphere;
        }


        public bool ExecuteOn(Token token)
        {
            return false;
        }

        public bool ExecuteOn(ITokenPayload payload)
        {
           RunXTriggersOnMutationsForStack(payload);
           RunXTriggersOnStackItself(payload);
           return true;
        }



        private void RunXTriggersOnMutationsForStack(ITokenPayload onStack)
        {
            foreach (var eachStackMutation in onStack.Mutations)
            {
                //first we apply xtriggers to mutations - but not to the default stack aspects.
                //i.e., mutations can get replaced by other mutations thanks to xtrigger morphs
                //we do this first in the expectation that the stack will generally get repopulated next, if there's a relevant xtrigger
                var stackMutationBaseAspect = Watchman.Get<Compendium>().GetEntityById<Element>(eachStackMutation.Key);
                if (stackMutationBaseAspect == null)
                {
                    NoonUtility.Log("Mutation aspect id doesn't exist: " + eachStackMutation.Key);
                }
                else
                {
                    foreach (var mutationXTrigger in stackMutationBaseAspect.XTriggers)
                    {
                        if (AspectsPresent.ContainsKey(mutationXTrigger.Key))
                        {
                            foreach (var morph in mutationXTrigger.Value)
                            {
                                if (morph.Chance >= UseDice.Rolld100())
                                {
                                    string newElementId = morph.Id;
                                    string currentMutationId = eachStackMutation.Key;
                                    int existingLevel = eachStackMutation.Value;

                                    if (morph.MorphEffect == MorphEffectType.Transform)
                                    {
                                        onStack.SetMutation(currentMutationId, 0, false);
                                        onStack.SetMutation(newElementId, existingLevel * morph.Level,
                                            true); //make it additive rather than overwriting, just in case
                                    }
                                    else if (morph.MorphEffect == MorphEffectType.Spawn)
                                    {
                                        ElementStackCreationCommand spawnedElementCommand =
                                            new ElementStackCreationCommand(newElementId, morph.Level);

                                        InSphere.ProvisionElementStackToken(spawnedElementCommand,
                                            new Context(Context.ActionSource.ChangeTo));

                                        NoonUtility.Log(
                                            "xtrigger aspect marked additional=true " + mutationXTrigger + " caused " +
                                            currentMutationId + " to spawn a new " + newElementId);
                                    }
                                    else if (morph.MorphEffect == MorphEffectType.Mutate)
                                    {
                                        onStack.SetMutation(newElementId, morph.Level, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RunXTriggersOnStackItself(ITokenPayload payload)
        {
            if (!payload.IsValidElementStack())
                return;
            
            var onStack = payload as ElementStack; //bleagh

            var xTriggers = onStack.GetXTriggers();

            foreach (var triggerKey in xTriggers.Keys)
            {
                //for each XTrigger in the stack, check if any of the aspects present in all the recipe's stacks match the trigger key
                if (AspectsPresent.ContainsKey(triggerKey))
                {
                    foreach (var morph in xTriggers[triggerKey])
                    {
                        Element effectElement = Watchman.Get<Compendium>().GetEntityById<Element>(morph.Id);
                        if (effectElement == null)
                        {
                            NoonUtility.Log(
                                "Tried to run an xtrigger with an element effect id that doesn't exist: " +
                                xTriggers[triggerKey]);
                        }

                        else if (morph.Chance >= UseDice.Rolld100())
                        {
                            string newElementId = morph.Id;
                            string oldElementId = onStack.Id;
                            int existingQuantity = onStack.Quantity;
                            if (morph.MorphEffect == MorphEffectType.Transform)
                            {
                                onStack.ChangeTo(newElementId);
                                NoonUtility.Log(
                                    "Transform xtrigger " + triggerKey + " caused " + oldElementId +
                                    " to transform into " + newElementId);
                            }
                            else if (morph.MorphEffect == MorphEffectType.Spawn)
                            {
                                var elementStackCreationCommand = new ElementStackCreationCommand(newElementId, morph.Level);
                                InSphere.ProvisionElementStackToken(elementStackCreationCommand,
                                    new Context(Context.ActionSource.ChangeTo));
                                NoonUtility.Log(
                                      "Spawn xtrigger " + triggerKey + " caused " +
                                      oldElementId + " to spawn a new " + newElementId);
                            }
                            else if (morph.MorphEffect == MorphEffectType.Mutate)
                            {
                                onStack.SetMutation(newElementId, morph.Level, true);
                            }
                        }
                    }
                }
            }
        }
    }
}
