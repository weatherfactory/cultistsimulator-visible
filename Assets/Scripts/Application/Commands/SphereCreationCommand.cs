using System.Collections.Generic;
using System.Diagnostics;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class SphereCreationCommand: IEncaustment
    {
        public SphereSpec GoverningSphereSpec { get; set; }
        public FucinePath Path { get; set; }
        public List<TokenCreationCommand> Tokens { get; set; }=new List<TokenCreationCommand>();

        public SphereCreationCommand()
        {

        }

        public SphereCreationCommand(SphereSpec governingSphereSpec)
        {
            GoverningSphereSpec = governingSphereSpec;
        }

        public void ExecuteOn(Dominion dominion,Context context)
        {
         dominion.CreateSphere(GoverningSphereSpec);
         foreach (var t in Tokens)
             t.Execute(context);
        }
    }
}
 