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

namespace SecretHistories.Commands.SituationCommands
{
  public  class PopulateDominionCommand: ISituationCommand,IEncaustment
    {
        public List<StateEnum> GetStatesCommandIsValidFor() => new List<StateEnum>
        {
            StateEnum.Unstarted, StateEnum.Complete, StateEnum.Halting, StateEnum.Ongoing, StateEnum.RequiringExecution,
            StateEnum.Unknown
        };

        public  List<SphereCreationCommand> Spheres { get; set; } = new List<SphereCreationCommand>();

        public PopulateDominionCommand()
        {

        }

        private Type getSphereType()
        {
            return Spheres.First().GoverningSphereSpec.SphereType;
        }

                public PopulateDominionCommand(SphereSpec spec)
        {
            var newCommand=new SphereCreationCommand(spec);
            Spheres.Add(newCommand);
        }

        public PopulateDominionCommand(List<SphereSpec> specs)
        {
            foreach (var s in specs)
            {
                var newCommand = new SphereCreationCommand(s);
                Spheres.Add(newCommand);
            }
        }

        
        public bool Execute(Situation situation)
        {
            if (Spheres.Count > 0) //This means we don't clear existing slots unless there are sl
            {
                var dominion = situation.GetRelevantDominions(situation.State.RehydrationValue, getSphereType()).FirstOrDefault();
                if(dominion!=null)
                {
                    dominion.RemoveAllSpheres();
                    foreach (var s in Spheres)
                        dominion.CreatePrimarySphere(s.GoverningSphereSpec); //I KNOW. But it's been a long day. 
                    return true;
                }
            }

            return false;
        }
        
    }
}
