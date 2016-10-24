using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
    public class Recipe
    {
        public string Id { get; set; }
        public string ActionId { get; set; }
        public Dictionary<string, int> Requirements { get; set; }
        public Dictionary<string,int> Effects { get; set; }
        public Boolean Craftable { get; set; }
        public string Label { get; set; }
        public int Warmup { get; set; }
        public string StartDescription { get; set; }
        public string Description { get; set; }

        public Recipe()
        {
            Requirements = new Dictionary<string, int>();
        Effects=new Dictionary<string, int>();
        }

    }

