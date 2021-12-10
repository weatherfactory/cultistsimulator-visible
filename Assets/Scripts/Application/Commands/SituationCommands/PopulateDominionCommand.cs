using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.Commands.SituationCommands
{
    /// <summary>
    /// Populates spheres for specified dominion. Will overwrite (and gracefully retire) any existing spheres, but only if there are SphereCreationCommands specified.
    /// If there are no spherecreationcommands there, it'll do nothing - use ClearDominionCommand instead
    /// </summary>
  public class PopulateDominionCommand: ISituationCommand,IEncaustment
    {

        public string Identifier { get; set; }

        public bool IsValidForState(StateEnum forState)
        {
            return true;
        }

        public bool IsObsoleteInState(StateEnum forState)
        {
            return false;
        }


        public  List<SphereCreationCommand> Spheres { get; set; } = new List<SphereCreationCommand>();

        public PopulateDominionCommand()
        {

        }


        public PopulateDominionCommand(string identifier,SphereSpec spec): this(identifier, new List<SphereSpec>{ spec })
        {}
 

        public PopulateDominionCommand(string identifier, List<SphereSpec> specs)
        {
            Identifier = identifier;

            foreach (var s in specs)
            {
                var newCommand = new SphereCreationCommand(s);
                Spheres.Add(newCommand);

            }
        }

        public bool Execute(AbstractDominion dominion)
        {
           if (dominion != null)
           {
               foreach (var s in Spheres)
                   s.ExecuteOn(dominion, new Context(Context.ActionSource.Unknown));
               return true;
           }

           return false;
        }

        public bool Execute(Situation situation)
        {
            return Execute(situation as ITokenPayload);
        }

        public bool Execute(ITokenPayload payload)
        {
            if (Spheres.Any())
            {
                var dominion = payload.Dominions.SingleOrDefault(d => d.Identifier == Identifier);
                if (dominion!=null)
                {
                    //we're going to replace any existing spheres
                    var sphereIdsToRemove = new List<string>(dominion.Spheres.Select(s => s.Id));
                    foreach (var s in sphereIdsToRemove)
                        dominion.RemoveSphere(s, SphereRetirementType.Graceful);

                    foreach (var s in Spheres)
                        s.ExecuteOn(dominion as SituationDominion, new Context(Context.ActionSource.Unknown)); 
                    return true;
                }
                else
                
                //store the dominion creation for later. If a payload keeps its spheres in a manifestation, it
                //may not yet be manifested, so the dominion may not yet exist.
                    payload.StorePopulateDominionCommand(this);
                
            }

            return false;
        }


    }
}
