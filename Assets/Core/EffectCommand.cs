using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core
{
    public class EffectCommand
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Dictionary<string, int> ElementChanges;

        public EffectCommand(Dictionary<string, int> elementChanges)
        {
            ElementChanges = elementChanges;
            Title = "default title";
            Description = "default message";
        }
    }
}
