using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Runtime;

namespace Assets.Core.Services
{

    public class Refinement
    {
        private string _refinementString;
        public string ForAspectId { get; private set;}
        public string Text { get; private set; }


        public Refinement(string refinementString)
        {
            _refinementString = refinementString;
            string[] _refinementParts = _refinementString.Split('|');
            if (_refinementParts.Length != 2)
            {
                throw new ApplicationException("Malformed refinement candidate " + _refinementString);
            }

            ForAspectId = _refinementParts[0];
            Text = _refinementParts[1];

        }



    }

    /// <summary>
    /// replaces tokens in a string based on the aspects available in a context (eg a situation) 
    /// </summary>
   public class TextRefiner
   {
       private IAspectsDictionary _aspectsInContext;

        public TextRefiner(IAspectsDictionary aspectsInContext)
        {
            _aspectsInContext = aspectsInContext;
        }

       public string RefineString(string stringToRefine)
       {
            //refinements follow this pattern: @#benefactorm|Timothy#benefactorf|Nicole#|Bozo@
            //must begin @ and end @
            //#[aspectid]|[text]
            //#| means 'default'
            //if default is not included and no aspects match, nothing should be displayed
            //CURRENTLY this code assumes max of one refinement set per string.

            //simplest most frequent: no refinements.
           if (!stringToRefine.Contains('@'))
            return stringToRefine;

           int indexBegin = stringToRefine.IndexOf('@');
           int indexTermination = stringToRefine.LastIndexOf('@');
           if (indexBegin == indexTermination)
               return flagNoTerminatingAt(stringToRefine);
           //+1 below because we're counting the first element of the substring, but indexes are zero based.
           //Yes it's obvious to *you* but this shit gets me every time so this note's for me. -AK
           string replaceableSegment = stringToRefine.Substring(indexBegin,(indexTermination - indexBegin)+1);

            //take the @s out (if we have more than two, this breaks, but dw for now)
           //and then split into #-headed segments
           string replaceableSegmentInternal = replaceableSegment.Replace("@", string.Empty);
           List<string> candidateRefinements = new List<string>(replaceableSegmentInternal.Split('#'));
           candidateRefinements.RemoveAll(cr => cr.Length == 0); //there's  an empty string in there from the bit before the first #

           foreach (var candidate in candidateRefinements)
           {
               try
               {
                   Refinement r = new Refinement(candidate);

                   if (r.ForAspectId == string.Empty)
                        //we've got as far as the default candidate. Stop worrying, return this, go on with your life.
                       return stringToRefine.Replace(replaceableSegment, r.Text);

                    if (_aspectsInContext.ContainsKey(r.ForAspectId))
                   {
                       //we've found a matching candidate. Stop worrying, return this, go on with your life.
                       return stringToRefine.Replace(replaceableSegment, r.Text);
                   }
               }

               catch (Exception e)
               {
                   return e.Message;
               }
           }

           //no candidate matches found. Remove the replaceable segment.

           return stringToRefine.Replace(replaceableSegment,String.Empty);
       }

       private string flagNoTerminatingAt(string s)
       {
           return s + "[Looks like there's a refinement with no @ terminator here]";
       }



    }
}
