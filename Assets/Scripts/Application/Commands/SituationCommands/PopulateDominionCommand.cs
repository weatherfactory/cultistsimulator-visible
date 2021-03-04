using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.Commands.SituationCommands
{
  public  class PopulateDominionCommand: ISituationCommand,IEncaustment
    {

        public DominionEnum Identifier { get; set; }

        public bool IsValidForState(StateEnum forState)
        {
            return true;
        }


        public  List<SphereCreationCommand> Spheres { get; set; } = new List<SphereCreationCommand>();

        public PopulateDominionCommand()
        {

        }


        public PopulateDominionCommand(DominionEnum identifier,SphereSpec spec): this(identifier, new List<SphereSpec>{ spec })
        {}
 

        public PopulateDominionCommand(DominionEnum identifier, List<SphereSpec> specs)
        {
            Identifier = identifier;

            foreach (var s in specs)
            {
                var newCommand = new SphereCreationCommand(s);
                Spheres.Add(newCommand);

            }
        }

        
        public bool Execute(Situation situation)
        {
            if (Spheres.Any())
            {
                var dominion = situation.Dominions.SingleOrDefault(d => d.Identifier == Identifier);
                if (dominion!=null)
                {
                    foreach (var s in Spheres)
                        s.ExecuteOn(dominion as SituationDominion, new Context(Context.ActionSource.Unknown)); 
                    return true;
                }
                else
                {NoonUtility.LogWarning($"Tried to populate dominion {Identifier} in situation {situation.Id}, but can't find that dominion identifier");
                    return false;
                }
            }

            return false;
        }


    }
}
