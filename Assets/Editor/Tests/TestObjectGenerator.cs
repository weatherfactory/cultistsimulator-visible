using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    public class TestObjectGenerator
    {
        public static Element CreateElement(int key)
        {
            return new Element(key.ToString(), "label" + key, "description" + key);
        }

        public static string GeneratedElementId(int index)
        {
            return index.ToString();
        }

        public static Dictionary<string,Element> ElementDictionary(int minKey, int MaxKey)
        {
            Dictionary<string,Element> d =new Dictionary<string, Element>();
            for (int k = minKey; k <= MaxKey; k++)
            {
                Element e = CreateElement(k);
            d.Add(k.ToString(),e);
            }

        return d;

        }

        public static void AddUniqueAspectsToElements(Dictionary<string, Element> elements)
        {
            foreach (string k in elements.Keys)
            {
                elements[k].Aspects.Add("a" + k,1);
            }
        }
    }

