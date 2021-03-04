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
        public string Id { get; set; }

        public List<StateEnum> GetStatesCommandIsValidFor() => new List<StateEnum>
        {
            StateEnum.Unstarted, StateEnum.Complete, StateEnum.Halting, StateEnum.Ongoing, StateEnum.RequiringExecution,
            StateEnum.Unknown
        };

        public  List<SphereCreationCommand> Spheres { get; set; } = new List<SphereCreationCommand>();

        public PopulateDominionCommand()
        {

        }


        public PopulateDominionCommand(string id,SphereSpec spec): this(id,new List<SphereSpec>{ spec })
        {}
 

        public PopulateDominionCommand(string id, List<SphereSpec> specs)
        {
            Id = id;

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
                var dominion = situation.Dominions.SingleOrDefault(d => d.Id == Id);
                if (dominion!=null)
                {
                    foreach (var s in Spheres)
                        s.ExecuteOn(dominion as SituationDominion, new Context(Context.ActionSource.Unknown)); 
                    return true;
                }
            }

            return false;
        }
        
    }
}
