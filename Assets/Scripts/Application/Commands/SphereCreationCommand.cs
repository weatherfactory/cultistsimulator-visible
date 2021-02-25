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
        public string Id { get; set; }
        public string OwnerSphereIdentifier { get; set; }
        public SphereSpec GoverningSphereSpec { get; set; }
        public List<TokenCreationCommand> Tokens { get; set; }=new List<TokenCreationCommand>();

        public SphereCreationCommand()
        {

        }

        public SphereCreationCommand(SphereSpec governingSphereSpec)
        {
            GoverningSphereSpec = governingSphereSpec;
        }

        public void ExecuteOn(SituationDominion situationDominion,Context context)
        {
            GoverningSphereSpec.SetId(Id); //still some dodginess here. Can we just move Id to spherespec? but what about SphereIdentifier?
            //I think we need SphereSpec on preinstantiated spheres like tabletop

            //ownersphereidentifier and container will also need thought
     var sphere=situationDominion.CreateSphere(GoverningSphereSpec);
         foreach (var t in Tokens)
             t.Execute(context,sphere);
        }
    }
}
 