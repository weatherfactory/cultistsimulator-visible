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
        public static Dictionary<Type, IEntityFactory> CachedEntityFactories = new Dictionary<Type, IEntityFactory>();
        public static Dictionary<Type,INonEntityFactory> CachedNonEntityFactories=new Dictionary<Type, INonEntityFactory>();

        public static IEntityWithId CreateEntity(Type T, Hashtable importDataForEntity, ContentImportLog log)
        {
            IEntityFactory entityFactory;

            if (CachedEntityFactories.ContainsKey(T))
                entityFactory = CachedEntityFactories[T];
            else
            {
                Type factoryType = typeof(EntityFactory<>);
                Type factoryTypeConstructed = factoryType.MakeGenericType(T); //we need a generic in order to be able to compile-time type the lamba
                entityFactory = Activator.CreateInstance(factoryTypeConstructed) as IEntityFactory;
                CachedEntityFactories.Add(T, entityFactory);
            }

            return entityFactory.ConstructorFastInvoke(importDataForEntity, log) as IEntityWithId;

            //uncomment below to use native new() if perf is an issue. It's not showing up as that, though
            //if (T == typeof(BasicVerb))
            //    return new BasicVerb(importDataForEntity, log);
            //else if (T == typeof(DeckSpec))
            //    return new DeckSpec(importDataForEntity, log);
            //else if (T == typeof(Element))
            //    return new Element(importDataForEntity, log);
            //else if (T == typeof(Ending))
            //    return new Ending(importDataForEntity, log);
            //else if (T == typeof(Expulsion))
            //    return new Expulsion(importDataForEntity, log);
            //else if (T == typeof(Legacy))
            //    return new Legacy(importDataForEntity, log);
            //else if (T == typeof(LinkedRecipeDetails))
            //    return new LinkedRecipeDetails(importDataForEntity, log);
            //else if (T == typeof(MorphDetails))
            //    return new MorphDetails(importDataForEntity, log);
            //else if (T == typeof(MutationEffect))
            //    return new MutationEffect(importDataForEntity, log);
            //else if (T == typeof(Recipe))
            //    return new Recipe(importDataForEntity, log);
            //else if (T == typeof(SlotSpecification))
            //    return new SlotSpecification(importDataForEntity, log);
            //else
            //{
            //    throw new ApplicationException("whut?");
            //}

        }



        public static object CreateObjectWithDefaultConstructor(Type typeToCreate)
        {
            INonEntityFactory nonEntityFactory;

            if (CachedNonEntityFactories.ContainsKey(typeToCreate))
                nonEntityFactory = CachedNonEntityFactories[typeToCreate];
            else
            { 
                Type factoryType = typeof(NonEntityFactory<>);
                Type factoryTypeConstructed = factoryType.MakeGenericType(typeToCreate); //we need a generic in order to be able to compile-time type the lamba
                nonEntityFactory = Activator.CreateInstance(factoryTypeConstructed) as INonEntityFactory;
                CachedNonEntityFactories.Add(typeToCreate,nonEntityFactory);
            }

            return nonEntityFactory.ConstructorFastInvoke();

        }


    }



}
