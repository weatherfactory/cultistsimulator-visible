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
using SecretHistories.Constants;
using SecretHistories.TokenContainers;

using Object = UnityEngine.Object;


namespace SecretHistories.Services
{
   public class PrefabFactory : MonoBehaviour
    {
       // [Header("Prefabs")]
        
        //public ElementFrame ElementFrame = null;
        //public Token Token = null;
        //public CardManifestation CardManifestation = null;
        //public DropzoneManifestation DropzoneManifestation = null;
        //public StoredManifestation StoredManifestation = null;
        //public MinimalManifestation MinimalManifestation = null;
        //public VerbManifestation VerbManifestation = null;
        //public PortalManifestation PortalManifestation = null;
        //public PickupManifestation PickupManifestation = null;
        //public ThresholdSphere ThresholdSphere = null;
        //public NotificationWindow NotificationWindow = null;

        //public SituationNote SituationNote = null;

        private readonly string prefabPath = "prefabs/";


        public T Create<T>() where T : Component
        {
            // var o = GetPrefabObject<T>();

             var o = GetPrefabObjectFromResources<T>();
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
            var o = GetPrefabObjectFromResources<T>();

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

        public T GetPrefabObjectFromResources<T>() where T : Component
        {
            string loadFromPath = prefabPath + typeof(T).Name;
            var prefab= Resources.Load<T>(loadFromPath);
            if(prefab==null)
              NoonUtility.LogWarning($"Can't find prefab of type {typeof(T).Name} at {loadFromPath}. Returning null.");

            return prefab;
            //var candidates=Resources.FindObjectsOfTypeAll(typeof(T));
            //if(candidates.Length==0)
            //{

            //}

            //if (candidates.Length >1)
            //    NoonUtility.LogWarning($"Multiple types of prefab {typeof(T)} in Resources. Using the first candidate.");

            //return candidates.First() as T;

        }

        public IManifestation CreateManifestationPrefab(Type manifestationType,Transform parent)
        {

            string loadFromPath = prefabPath + manifestationType.Name;
            var prefab = Resources.Load(loadFromPath);
            if (prefab == null)
                NoonUtility.LogWarning($"Can't find prefab of type {manifestationType.Name} at {loadFromPath}. Returning null.");


            var instantiatedPrefab = Instantiate(prefab, parent) as GameObject;

            return instantiatedPrefab.GetComponent(manifestationType) as IManifestation;
        }


    }
}