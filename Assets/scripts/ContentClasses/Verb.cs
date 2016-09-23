
using System;
using System.Collections.Generic;

namespace ContentClasses
{
    [Serializable]
    public class Verb
    {
        public string Id;
        public string Label;
        public string Description;
    }

    [Serializable]
    public class VerbList
    {
        public List<Verb> Actions =new List<Verb> ();
    }
}