using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using UnityEngine;
using SecretHistories.UI.Scripts;
using SecretHistories.Elements;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Infrastructure;
using SecretHistories.TokenContainers;

using Object = UnityEngine.Object;


namespace SecretHistories.Services
{
   public class PrefabFactory : MonoBehaviour
    {
        [Header("Prefabs")]
        
        public ElementFrame ElementFrame = null;

        public Token Token = null;
        public CardManifestation CardManifestation = null;
        public DropzoneManifestation DropzoneManifestation = null;
        public StoredManifestation StoredManifestation = null;
        public MinimalManifestation MinimalManifestation = null;
        public VerbManifestation VerbManifestation = null;
        public PortalManifestation PortalManifestation = null;
        public PickupManifestation PickupManifestation = null;
        public Threshold Threshold = null;
        public NotificationWindow NotificationWindow = null;
        public SituationNote SituationNote = null;


        public T Create<T>() where T : Component
        {
            var o = GetPrefabObject<T>();
            try
            {
                var c = Object.Instantiate(o) as T;
                c.transform.localScale = Vector3.one;

                return c;
            }
            catch (Exception e)
            {
                NoonUtility.Log("Couldn't instantiate prefab " + typeof(T) + "\n" + e);
                return null;
            }

        }

        public T CreateLocally<T>(Transform parent) where T : Component
        {
            var o = GetPrefabObject<T>();
            try
            { 
                var c = Object.Instantiate(o, parent, false) as T;
                c.transform.localScale = Vector3.one;

                return c;
            }
            catch (Exception e)
            {
                NoonUtility.Log("Couldn't instantiate prefab " + typeof(T) + "\n" + e);
                return null;
            }

        }

        public T GetPrefabObject<T>() where T : Component
        {
            string prefabFieldName = typeof(T).Name;

            FieldInfo field = GetType().GetField(prefabFieldName);
            if (field == null)
                throw new ApplicationException(prefabFieldName +
                                               " not registered in prefab factory; must have field name and type both '" +
                                               prefabFieldName + "', must have field populated in editor");

            T prefab = field.GetValue(this) as T;
            return prefab;
        }

        public IManifestation CreateManifestationPrefab(Type prefabFieldType,Transform parent)
        {
            
            FieldInfo field = GetType().GetField(prefabFieldType.Name);
            if (field == null)
                throw new ApplicationException(prefabFieldType.Name +
                                               " not registered in prefab factory; must have field name and type both '" +
                                               prefabFieldType.Name + "', must have field populated in editor");

            var prefabObject = field.GetValue(this) as UnityEngine.Object;

            var instantiatedPrefab = Instantiate(prefabObject,parent);
            return instantiatedPrefab as IManifestation;
        }




    }
}