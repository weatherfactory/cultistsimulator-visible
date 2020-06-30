using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    //this is a battlefield
   public class FucineEntityFactory
    {

        public static IEntityWithId CreateEntity(Type T, Hashtable importDataForEntity, ContentImportLog log)
        {
            if (T == typeof(BasicVerb))
                return new BasicVerb(importDataForEntity, log);
            else if (T == typeof(DeckSpec))
                return new DeckSpec(importDataForEntity, log);
            else if (T == typeof(Element))
                return new Element(importDataForEntity, log);
            else if (T == typeof(Ending))
                return new Ending(importDataForEntity, log);
            else if (T == typeof(Expulsion))
                return new Expulsion(importDataForEntity, log);
            else if (T == typeof(Legacy))
                return new Legacy(importDataForEntity, log);
            else if (T == typeof(LinkedRecipeDetails))
                return new LinkedRecipeDetails(importDataForEntity, log);
            else if (T == typeof(MorphDetails))
                return new MorphDetails(importDataForEntity, log);
            else if (T == typeof(MutationEffect))
                return new MutationEffect(importDataForEntity, log);
            else if (T == typeof(Recipe))
                return new Recipe(importDataForEntity, log);
            else if (T == typeof(SlotSpecification))
                return new SlotSpecification(importDataForEntity, log);
            else
            {
                throw new ApplicationException("Don't know about" + T.Name);
            }

        }
    }
}
