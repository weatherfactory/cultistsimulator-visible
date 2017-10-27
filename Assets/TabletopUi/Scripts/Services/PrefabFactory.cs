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
using Object = UnityEngine.Object;


namespace Assets.TabletopUi.Scripts.Services
{
    class PrefabFactory : MonoBehaviour
    {
        [Header("Prefabs")]
        public AspectFrame AspectFrame = null;
        public SituationToken SituationToken = null;
        public ElementStackToken ElementStackToken = null;
        public SituationWindow SituationWindow = null;
        public ElementDetailsWindow ElementDetailsWindow = null;
        public SlotDetailsWindow SlotDetailsWindow = null;
        public RecipeSlot RecipeSlot = null;
        public NotificationWindow NotificationWindow = null;
        public SituationOutputNote SituationOutputNote = null;

        [Header("Token Subscribers")]
        [SerializeField] TabletopManager TabletopManager = null;


        public static T CreateToken<T>(Transform destination, string saveLocationInfo = null) where T : DraggableToken
        {
            var token = PrefabFactory.CreateLocally<T>(destination);
            var pf = Instance();
            token.SubscribeNotifier(Registry.Retrieve<INotifier>());
            token.SetContainer(pf.TabletopManager.tabletopContainer);
            if (saveLocationInfo != null)
                token.SaveLocationInfo = saveLocationInfo;

            return token;
        }


        public static T CreateLocally<T>(Transform parent) where T : Component
        {
            var o = GetPrefab<T>();
            try
            { 
            T c = Object.Instantiate(o, parent, false) as T;
                c.transform.localScale = Vector3.one;

                return c;
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't instantiate prefab " + typeof(T) + "\n" + e);
                return null;
            }

        }

        public static T GetPrefab<T>() where T : Component
        {
            var pf = Instance();

            string prefabFieldName = typeof(T).Name;

            FieldInfo field = pf.GetType().GetField(prefabFieldName);
            if (field == null)
                throw new ApplicationException(prefabFieldName +
                                               " not registered in prefab factory; must have field name and type both '" +
                                               prefabFieldName + "', must have field populated in editor");

            T prefab = field.GetValue(pf) as T;
            return prefab;
        }


        private static PrefabFactory Instance()
        {
            var instance = FindObjectOfType<PrefabFactory>();
            if (instance == null)
                throw new ApplicationException("No prefab factory in scene");

            return instance;
        }
    }
}