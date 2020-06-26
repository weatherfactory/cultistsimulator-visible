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
   public class FucineEntityFactory<T> where T:AbstractEntity<T>
    {

        public static AbstractEntity<T> CreateEntity(Hashtable importDataForEntity, ContentImportLog log)
        {
             if (typeof(T) == typeof(BasicVerb))
                return new BasicVerb(importDataForEntity,log) as AbstractEntity<T>;
           else if (typeof(T)==typeof(DeckSpec))
                return new DeckSpec(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(Element))
                return new Element(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(Ending))
                return new Ending(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(Expulsion))
                return new Expulsion(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(Legacy))
                return new Legacy(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(LinkedRecipeDetails))
                return new LinkedRecipeDetails(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(MorphDetails))
                return new MorphDetails(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(MutationEffect))
                return new MutationEffect(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(Recipe))
                return new Recipe(importDataForEntity, log) as AbstractEntity<T>;
            else if (typeof(T) == typeof(SlotSpecification))
                return new SlotSpecification(importDataForEntity, log) as AbstractEntity<T>;
             else
             {
                 throw new ApplicationException("Don't know about" + typeof(T).Name);
             }

        }
    }
}
