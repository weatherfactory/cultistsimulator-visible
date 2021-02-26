using System.Collections.Generic;
using System.Diagnostics;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Commands
{
    public class SphereCreationCommand: IEncaustment
    {
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

        public void ExecuteOn(FucineRoot root, Context context)
        {
            var sphere = root.GetSphereById(this.GoverningSphereSpec.Id);
         if(sphere!=null)
         {
             //this will diskard data from spheres that can't be found because they haven't become active/registered yet - like ElementsMalleary
             //worth caching the commands for when they appear?
            sphere.OwnerSphereIdentifier = OwnerSphereIdentifier; //should never be relevant, but let's play safe

            foreach (var t in Tokens)
                t.Execute(context, sphere);
         }
        }

        public void ExecuteOn(SituationDominion situationDominion,Context context)
        {
            
            //ownersphereidentifier and container will also need thought
     var sphere=situationDominion.CreateSphere(GoverningSphereSpec);
     sphere.OwnerSphereIdentifier = OwnerSphereIdentifier;
         foreach (var t in Tokens)
             t.Execute(context,sphere);
        }
    }
}
 