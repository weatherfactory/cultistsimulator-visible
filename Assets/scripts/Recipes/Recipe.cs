using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    public class Recipe
    {
        public string Id { get; set; }
        public string ActionId { get; set; }
        public Dictionary<string, int> Requirements { get; set; }
        public Boolean Craftable { get; set; }
        public string Label { get; set; }

        public Recipe()
        {
            Requirements = new Dictionary<string, int>();
        }

    }

