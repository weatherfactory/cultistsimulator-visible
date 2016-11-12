using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;
using Assets.TabletopUi.Scripts;
using Object = UnityEngine.Object;


namespace Assets.TabletopUi.Scripts.Services
{
    class PrefabFactory: MonoBehaviour
    {
        [Header("Prefabs")]
        public AspectFrame AspectFrame=null;
        public VerbBox VerbBox = null;
        public ElementStack ElementStack = null;
        public SituationWindow SituationWindow = null;
        public ElementDetailsWindow ElementDetailsWindow = null;
        public Notification Notification = null;
        public RecipeSlot RecipeSlot = null;

        [Header("Token Subscribers")]
        [SerializeField]
        TabletopManager TabletopManager = null;
        [SerializeField]
        Notifier Notifier = null;


        public static T CreateTokenWithSubscribers<T>(Transform destination) where T : DraggableToken
        {
            var token = PrefabFactory.CreateLocally<T>(destination);
            var pf = Instance();
            token.Subscribe(pf.TabletopManager);
            token.Subscribe(pf.Notifier);

            return token;
        }

        public static SituationWindow CreateSituationWindowWithSubscribers(Transform destination)
        {
            var sw = PrefabFactory.CreateLocally<SituationWindow>(destination);
            var pf = Instance();
            sw.Subscribe(pf.TabletopManager);
            return sw;
        }


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
