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
    public class ClearDominionCommand : ISituationCommand, IEncaustment
    {

        public string Identifier { get; set; }
        public SphereRetirementType RetirementType { get; set; }

        public bool IsValidForState(StateEnum forState)
        {
            return true;
        }

        public bool IsObsoleteInState(StateEnum forState)
        {
            return false;
        }


        public ClearDominionCommand()
        {

        }

        
        public ClearDominionCommand(string identifier,SphereRetirementType retirementType)
        {
            Identifier = identifier;
            RetirementType = retirementType;
        }

    
        public bool Execute(Situation situation)
        {
      
                var dominion = situation.Dominions.SingleOrDefault(d => d.Identifier == Identifier);
                if (dominion != null)
                {
                    var sphereIdsToRemove = new List<string>(dominion.Spheres.Select(s => s.Id));
                    foreach (var s in sphereIdsToRemove)
                        dominion.RemoveSphere(s,RetirementType);

                    return true;
                }
                
                NoonUtility.LogWarning($"Tried to populate dominion {Identifier} in situation {situation.Id}, but can't find that dominion identifier");
                return false;
                
            
        }


    }
}
