using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using UnityEngine;
using Assets.TabletopUi.Scripts;
using Object = UnityEngine.Object;


namespace Assets.TabletopUi.Scripts.Services
{
    class PrefabFactory: MonoBehaviour
    {
        public AspectFrame AspectFrame;
        public VerbBox VerbBox;
        public ElementCard ElementCard;
        public SituationWindow SituationWindow;
        public ElementDetailsWindow ElementDetailsWindow;
        public Notification Notification;
        public RecipeSlot RecipeSlot;

        public static T CreateLocally<T>(Transform parent) where T : Component
        {
            
            var o = GetPrefab<T>();
            T c = Object.Instantiate(o, parent, false) as T;
            c.transform.localScale = Vector3.one;
            return c;
        }

        public static T GetPrefab<T>() where T : Component
        {
            var pf = Instance();

            string prefabFieldName = typeof(T).Name;

            FieldInfo field = pf.GetType().GetField(prefabFieldName);
            if(field==null)
                throw new ApplicationException(prefabFieldName + " not registered in prefab factory; must have field name and type both '"+ prefabFieldName+ "', must have field populated in editor" );

            T prefab = field.GetValue(pf) as T;
            return prefab;
        }

        private static PrefabFactory Instance()
        {
            var instance = FindObjectOfType<PrefabFactory>();
                if(instance==null)
                throw new ApplicationException("No prefab factory in scene");

            return instance;
        }
    }
}
