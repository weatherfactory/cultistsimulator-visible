using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    //this is a battlefield
    public class WIPFactory
    {
        public static Dictionary<Type,IFactory> Factories=new Dictionary<Type, IFactory>();

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
                return Activator.CreateInstance(T, importDataForEntity, log) as IEntityWithId;
            }

        }

        //public static IEntityWithId CreateEntityTest(Type T,Hashtable h, ContentImportLog log)
        //{

        //    Type cachedEntityType = typeof(EntityFactory<>);
        //    Type cachedEntityTypeConstructed = cachedEntityType.MakeGenericType(T);
        //    dynamic cachedEntityInfo = Activator.CreateInstance(cachedEntityTypeConstructed);

        //    return cachedEntityInfo.ConstructorFastInvoke(h,log);

        //}

        public static object CreateObjectWithDefaultConstructor(Type typeToCreate)
        {
            IFactory factory;

            if (Factories.ContainsKey(typeToCreate))
                factory = Factories[typeToCreate];
            else
            { 
                Type factoryType = typeof(NonEntityFactory<>);
                Type factoryTypeConstructed = factoryType.MakeGenericType(typeToCreate); //we need a generic in order to be able to compile-time type the lamba
                factory = Activator.CreateInstance(factoryTypeConstructed) as IFactory;
                Factories.Add(typeToCreate,factory);
            }

            return factory.ConstructorFastInvoke();

        }


    }



}
