using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;




public abstract class AbstractVerb : IVerb,IEntity
{
    
    [FucineId]
    public string Id { get; set; }
        
    [FucineValue(".")]
     public string Label { get; set; }

    [FucineValue(".")]
    public string Description { get; set; }
    
    [FucineSubEntity(typeof(SlotSpecification))]
    public SlotSpecification Slot { get; set; }


    public abstract bool Transient { get; }

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

