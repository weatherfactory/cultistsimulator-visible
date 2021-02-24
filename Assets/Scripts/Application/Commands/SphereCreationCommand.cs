using System.Collections.Generic;
using System.Diagnostics;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class SphereCreationCommand: IEncaustment
    {
        public SphereSpec GoverningSphereSpec { get; set; }
        public List<TokenCreationCommand> Tokens { get; set; }=new List<TokenCreationCommand>();

        public SphereCreationCommand()
        {
            NoonUtility.Log("In here");
        }

        public SphereCreationCommand(SphereSpec governingSphereSpec)
        {
            GoverningSphereSpec = governingSphereSpec;
        }
    }
}
 