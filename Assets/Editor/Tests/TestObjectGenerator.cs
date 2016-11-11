﻿using System;
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

    public static FakeElementStack CreateElementCard(string key, int quantity)
    {
        FakeElementStack c=new FakeElementStack() {ElementId = key,Quantity = 1};
        return c;
    }

        public static List<IElementStack> CardsForElements(Dictionary<string, Element> elements)
        {
        List<IElementStack> cards=new List<IElementStack>();
            foreach (string k in elements.Keys)
            {
                FakeElementStack stack = CreateElementCard(k, 1);
                stack.Aspects = elements[k].AspectsIncludingSelf;
                cards.Add(stack);
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

