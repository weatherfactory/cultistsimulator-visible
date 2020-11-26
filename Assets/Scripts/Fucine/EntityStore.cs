using System.Collections.Generic;
using System.Linq;
using Assets.Core.Interfaces;
using Noon;

public class EntityStore
{
    private Dictionary<string, IEntityWithId> _entities=new Dictionary<string, IEntityWithId>();


    public bool TryAddEntity(IEntityWithId entityToAdd)
    {
        if (!_entities.ContainsKey(entityToAdd.Id))
        {
            AddEntity(entityToAdd);
            return true;
        }

        return false;
    }


    public void AddEntity(IEntityWithId entityToAdd)
    {
        _entities.Add(entityToAdd.Id, entityToAdd);

    }

    public bool TryGetById<T>(string entityId, out T entity) where T : class, IEntityWithId
    {
        IEntityWithId retrievedEntity;
        if(entityId!=null && _entities.TryGetValue(entityId, out retrievedEntity))
        {
            entity = retrievedEntity as T;
            return true;
        }
        else
        {
            entity = null;
            return false;
        }


    }
    
    public T GetById<T>(string entityId) where T : class, IEntityWithId
    {
        if (entityId == null)
            return null;
        return _entities[entityId] as T;
    }

    public T GetSingleInstance<T>() where T : class, IEntityWithId
    {
        if (_entities.Count > 1)
        {
            NoonUtility.LogWarning($"Single instance requested of {nameof(T)}, but there's {_entities.Count} of them. Returning the first. ");
            return _entities.Values.First() as T;
        }
        if(_entities.Count<1)
        {
            NoonUtility.LogWarning($"Single instance requested of {nameof(T)}, but there aren't any. Returning null.");
            return null;
        }

        return _entities.Values.Single() as T;
    }

    public List<IEntityWithId> GetAllAsList()
    {
        return new List<IEntityWithId>(_entities.Values);
    }


    public List<T> GetAllAsList<T>() where T: class, IEntityWithId
    {
        
        return new List<T>(_entities.Values.Cast<T>().ToList());
    }

    public List<T> GetAllAsAlphabetisedList<T>() where T : class, IEntityWithId
    {
        //if we order here, we don't have to cast to list twice

        return new List<T>(_entities.Values.Cast<T>().OrderBy(e=>e.Id).ToList());
    }

    public List<T> GetFilteredList<T>() where T : class, IEntityWithId
    {
        //if we order here, we don't have to cast to list twice

        return new List<T>(_entities.Values.Cast<T>().OrderBy(e => e.Id).ToList());
    }


    public Dictionary<string, IEntityWithId> GetAll()
    {
        return new Dictionary<string, IEntityWithId>(_entities);
    }
}