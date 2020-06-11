using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;


/// <summary>
/// just a bundle of properties
/// </summary>
    public abstract class AbstractVerb : IVerb,IEntity
{
    private SlotSpecification _primarySlotSpecification;

    [FucineId]
    public string Id { get; set; }
        
    [FucineString]
     public string Label { get; set; }

    [FucineString]
    public string Description { get; set; }

    public abstract bool Transient { get; }

    public SlotSpecification PrimarySlotSpecification
    {
        get { return _primarySlotSpecification; }
        set { _primarySlotSpecification = value; }

    }

    public AbstractVerb(string id, string label, string description)
        {
            Id= id;
            Label = label;
            Description = description;
        }

    public AbstractVerb()
    {

    }
    }

