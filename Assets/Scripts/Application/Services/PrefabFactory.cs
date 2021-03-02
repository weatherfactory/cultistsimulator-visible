using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Assets.Scripts.Application.Abstract;
using Assets.Scripts.Application.Fucine;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Fucine;
using SecretHistories.UI;
using UnityEngine;
using SecretHistories.UI.Scripts;
using SecretHistories.Elements;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;

using Object = UnityEngine.Object;


namespace SecretHistories.Services
{
    [ImmanenceAttribute(typeof(PrefabFactory))]
   public class PrefabFactory
    {

        private readonly string prefabPath = "prefabs/";

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
        }

        public IManifestation CreateManifestationPrefab(Type manifestationType,Transform parent)
        {

            string loadFromPath = prefabPath + manifestationType.Name;
            var prefab = Resources.Load(loadFromPath);
            if (prefab == null)
                NoonUtility.LogWarning($"Can't find prefab of type {manifestationType.Name} at {loadFromPath}. Returning null.");


            var instantiatedPrefab = Object.Instantiate(prefab, parent) as GameObject;

            return instantiatedPrefab.GetComponent(manifestationType) as IManifestation;
        }

        public Sphere InstantiateSphere(SphereSpec spec)
        {
            return InstantiateSphere(spec, FucineRoot.Get());
        }

        public Sphere InstantiateSphere(SphereSpec spec, IHasAspects container)
            {
                string loadFromPath = prefabPath + spec.SphereType.Name;
                var prefab = Resources.Load(loadFromPath);
                GameObject instantiatedPrefab;
                try
                {
                    instantiatedPrefab = Object.Instantiate(prefab) as GameObject;
                }
                catch (Exception e)
                {
                    NoonUtility.Log($"Can't instantiate a sphere prefab at path {loadFromPath}; instantiating a generic game object and adding a thresholdsphere component to it. Error specifics: {e.Message}");
                    instantiatedPrefab = new GameObject();
                    instantiatedPrefab.AddComponent<ThresholdSphere>();
                }
                instantiatedPrefab.name = $"{spec.SphereType.Name}";
                var newSphere = instantiatedPrefab.GetComponent(spec.SphereType) as Sphere;
                container.AttachSphere(newSphere);
                
                newSphere.ApplySpec(spec);

                Watchman.Get<HornedAxe>().RegisterSphere(newSphere);

                return newSphere;
            }


        }
}