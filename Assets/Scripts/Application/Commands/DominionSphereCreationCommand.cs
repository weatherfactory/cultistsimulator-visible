using System.Collections.Generic;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class DominionSphereCreationCommand: IEncaustment
    {
        public SphereSpec GoverningSphereSpec { get; set; }
        public FucinePath Path { get; protected set; }
        public List<TokenCreationCommand> Tokens { get; set; }=new List<TokenCreationCommand>();

        public DominionSphereCreationCommand()
        {

        }
    }
}
 