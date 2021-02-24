using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using SecretHistories.UI;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;

 namespace SecretHistories.Entities
{

public class SphereSpec: AbstractEntity<SphereSpec>
{

    [FucineValue(DefaultValue = "", Localise = true)]
    public string Label { get; set; }

    [FucineValue("")]
    public string ActionId { get; set; }

    /// <summary>
    /// currently, this is only used by the primary slot specification
    /// </summary>
    [FucineValue(DefaultValue = "", Localise = true)]
    public string Description { get; set; }
    /// <summary>
    /// The element in this slot must possess at least one of these aspects
    /// </summary>
    [FucineAspects(ValidateAsElementId = true)]
    public AspectsDictionary Required { get; set; }
    /// <summary>
    /// The element in this slot cannot possess any of these aspects
    /// </summary>
    [FucineAspects(ValidateAsElementId = true)]
    public AspectsDictionary Forbidden { get; set; }

    /// <summary>
    /// A Greedy slot will find a card on the desktop that matches its specification, and insert it.
    /// </summary>
    [FucineValue(false)]
    public bool Greedy { get; set; }
    /// <summary>
    /// A Consuming slot will destroy its contents when a recipe begins
    /// </summary>
    [FucineValue(false)]
    public bool Consumes { get; set; }

    [FucineList]
   public List<AngelSpecification> Angels { get; set; }


      public Type SphereType { get; set; }
    public FucinePath RelativePath => new FucinePath(_id);


        public bool AllowAnyToken { get; set; }

    private readonly HashSet<StateEnum> _activeInStates=new HashSet<StateEnum>();

    public bool IsActiveInState(StateEnum state)
    {
        return (_activeInStates.Contains(state));
    }


    public void MakeActiveInState(StateEnum state)
    {
        _activeInStates.Add(state);
    }

    protected SphereSpec()
    {
        //setting here as default.
        SphereType = typeof(ThresholdSphere);
    }

    /// <summary>
    /// for this constructor, ID and type have already been determined, and are just being set and/or deserialised from the parameters
    /// </summary>
    [JsonConstructor]
    public SphereSpec(Type sphereType, string id)
    {
        _id = id;
        SphereType = sphereType;
        Required = new AspectsDictionary();
        Forbidden = new AspectsDictionary();
        ActionId = string.Empty;
        Angels = new List<AngelSpecification>();
        }


    public SphereSpec(Type sphereType, AbstractSphereSpecIdentifierStrategy sphereSpecIdentifierStrategy): this(sphereType,sphereSpecIdentifierStrategy.GetIdentifier())
    {
    }


    public SphereSpec(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
    {
        SphereType = typeof(ThresholdSphere); //ultimately we will want to import the spheretype, but that'll do for now
    }

    public List<IAngel> MakeAngels()
    {
        return new List<IAngel>();
    }

    public ContainerMatchForStack CheckPayloadAllowedHere(ITokenPayload payload)
    {
        if (!payload.IsValidElementStack() && !AllowAnyToken)
            return new ContainerMatchForStack(new List<string>(), SlotMatchForAspectsType.ForbiddenAspectPresent);

        var aspects = payload.GetAspects(true);

        foreach (string k in Forbidden.Keys)
        {
            if(aspects.ContainsKey(k))
            {
                return new ContainerMatchForStack(new List<string>() {k}, SlotMatchForAspectsType.ForbiddenAspectPresent);
            }
        }
        
        //passed the forbidden check
        //if there are no specific requirements, then we're now okay
        if(Required.Keys.Count==0)
            return new ContainerMatchForStack(null,SlotMatchForAspectsType.Okay);


        foreach (string k in Required.Keys) //only one needs to match
        {
            if (aspects.ContainsKey(k))
            { 
                int aspectAtValue = aspects[k];
                if (aspectAtValue >= Required[k])
                    return new ContainerMatchForStack(null, SlotMatchForAspectsType.Okay);
            }
        }

        return new ContainerMatchForStack(Required.Keys, SlotMatchForAspectsType.RequiredAspectMissing);


    }

    protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
    {
        
    }
}



public enum SlotMatchForAspectsType
{
Okay,
    RequiredAspectMissing,
    ForbiddenAspectPresent
}
}