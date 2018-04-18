using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;


/// <summary>
/// just a bundle of properties
/// </summary>
    public abstract class AbstractVerb : IVerb
{
        private string _id;
        private string _label;
        private string _description;
    private bool _atStart;
    private SlotSpecification _primarySlotSpecification;

    

    public string Id
        {
            get { return _id; }
        }

        public string Label
        {
            get { return _label; }
        }

        public string Description
        {
            get { return _description; }
        }

    public bool AtStart
    {
        get { return _atStart; }
        set { _atStart = value; }
    }

    public abstract bool Transient { get; }

    public SlotSpecification PrimarySlotSpecification
    {
        get { return _primarySlotSpecification; }
        set { _primarySlotSpecification = value; }

    }

    public AbstractVerb(string id, string label, string description)
        {
            _id = id;
            _label = label;
            _description = description;
        }
    }

