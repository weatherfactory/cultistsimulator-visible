using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Interfaces;
using Assets.Editor.Tests;


public class TestObjectGenerator
    {
        public static Element CreateElement(int key)
        {
            return new Element(key.ToString(), "label" + key, "description" + key);
        }

    public static FakeElementCard CreateElementCard(string key, int quantity)
    {
        FakeElementCard c=new FakeElementCard() {ElementId = key,Quantity = 1};
        return c;
    }

        public static List<IElementCard> CardsForElements(Dictionary<string, Element> elements)
        {
        List<IElementCard> cards=new List<IElementCard>();
            foreach (string k in elements.Keys)
            {
                FakeElementCard card = CreateElementCard(k, 1);
                card.Aspects = elements[k].AspectsIncludingSelf;
                cards.Add(card);
            }
            return cards;
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

        public static void AddUniqueAspectsToEachElement(Dictionary<string, Element> elements)
        {
            foreach (string k in elements.Keys)
            {
                elements[k].Aspects.Add("a" + k,1);
            }
        }
    }

