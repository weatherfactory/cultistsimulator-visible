using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using Object = UnityEngine.Object;


namespace Assets.TabletopUi.Scripts.Services
{
   public class PrefabFactory : MonoBehaviour
    {
        [Header("Prefabs")]
        public ElementFrame ElementFrame = null;
        public VerbAnchor VerbAnchor = null;
        public BookshelfAnchor BookshelfAnchor = null;
        public ElementStackToken ElementStackToken = null;
        public CardManifestation CardManifestation = null;
        public MinimalManifestation MinimalManifestation = null;
        public SituationWindow SituationWindow = null;
        public RecipeSlot RecipeSlot = null;
        public NotificationWindow NotificationWindow = null;
        public SituationNote SituationNote = null;


        public ISituationAnchor CreateSituationAnchorForVerb(IVerb verb,Transform t)
        {
            if(verb.Species==NoonConstants.ANCHOR_BOOKSHELF)
                return CreateLocally<BookshelfAnchor>(t);
            else
                return CreateLocally<VerbAnchor>(t);
        }


        public T CreateLocally<T>(Transform parent) where T : Component
        {
            var o = GetPrefab<T>();
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

        public T GetPrefab<T>() where T : Component
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


    }
}