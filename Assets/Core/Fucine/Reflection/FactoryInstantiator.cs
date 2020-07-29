using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Fucine.DataImport;
using Assets.Core.Interfaces;

namespace Assets.Core.Fucine
{
    public class FactoryInstantiator
    {
        public static Dictionary<Type, IImportedEntityFactory> CachedEntityFactories = new Dictionary<Type, IImportedEntityFactory>();
        public static Dictionary<Type,IFastInvokableObjectFactory> CachedNonEntityFactories=new Dictionary<Type, IFastInvokableObjectFactory>();

        public static IEntityWithId CreateEntity(Type T, EntityData importDataForEntity, ContentImportLog log)
        {
            IImportedEntityFactory importedEntityFactory;

            if (CachedEntityFactories.ContainsKey(T))
                importedEntityFactory = CachedEntityFactories[T];
            else
            {
                Type factoryType = typeof(ImportedEntityFactory<>);
                Type factoryTypeConstructed = factoryType.MakeGenericType(T); //we need a generic in order to be able to compile-time type the lamba
                importedEntityFactory = Activator.CreateInstance(factoryTypeConstructed) as IImportedEntityFactory;
                CachedEntityFactories.Add(T, importedEntityFactory);
            }

            return importedEntityFactory.ConstructorFastInvoke(importDataForEntity, log) as IEntityWithId;

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
            IFastInvokableObjectFactory fastInvokableObjectFactory;

            if (CachedNonEntityFactories.ContainsKey(typeToCreate))
                fastInvokableObjectFactory = CachedNonEntityFactories[typeToCreate];
            else
            { 
                Type factoryType = typeof(FastInvokableObjectFactory<>);
                Type factoryTypeConstructed = factoryType.MakeGenericType(typeToCreate); //we need a generic in order to be able to compile-time type the lambda
                fastInvokableObjectFactory = Activator.CreateInstance(factoryTypeConstructed) as IFastInvokableObjectFactory;
                CachedNonEntityFactories.Add(typeToCreate,fastInvokableObjectFactory);
            }

            return fastInvokableObjectFactory.ConstructorFastInvoke();

        }


    }



}
