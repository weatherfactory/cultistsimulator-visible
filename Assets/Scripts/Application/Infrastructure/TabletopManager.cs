#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SecretHistories.Core;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.Services;
using Assets.Logic;
using SecretHistories.Constants;
using SecretHistories.UI;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;

using SecretHistories.Enums.Elements;
using SecretHistories.Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

namespace SecretHistories.UI {
    public class TabletopManager : MonoBehaviour
    {

        [Header("Tabletop")] [SerializeField] public TabletopSphere _tabletop;
        [SerializeField] TabletopBackground tabletopBackground;

        [SerializeField] public WindowsSphere WindowsSphere;
        
        [Header("Detail Windows")] [SerializeField]
        private AspectDetailsWindow aspectDetailsWindow;

        [SerializeField] private TokenDetailsWindow tokenDetailsWindow;
        [SerializeField] private CardHoverDetail cardHoverDetail;


        [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("mapContainsTokens")]
        public MapSphere MapSphere;

        [SerializeField] TabletopBackground mapBackground;
        [SerializeField] MapAnimation mapAnimation;

        [Header("Drag & Window")] [SerializeField]
        private RectTransform draggableHolderRectTransform;

        [SerializeField] private ScrollRect tableScroll;
        [SerializeField] public GameObject _dropZoneTemplate;


        [Header("Status Bar & Notes")] [SerializeField]
        private StatusBar StatusBar;

        [SerializeField] private BackgroundMusic backgroundMusic;

        [SerializeField] private Notifier _notifier;
        [SerializeField] private ElementOverview _elementOverview;
        

        void Awake()
        {
            var registry = new Watchman();
            registry.Register(this);
        }





		public void CloseAllDetailsWindows()
		{
			if (aspectDetailsWindow != null)
				aspectDetailsWindow.Hide();
			if (tokenDetailsWindow != null)
				tokenDetailsWindow.Hide();
		}

        public void CloseAllSituationWindowsExcept(string exceptVerbId) {
            var situations = Watchman.Get<SituationsCatalogue>().GetRegisteredSituations();

            foreach (var s in situations)
            {
                if (s.Verb.Id != exceptVerbId)
                    s.Close();
            }
        }

        public bool IsSituationWindowOpen() {
	        var situationControllers = Watchman.Get<SituationsCatalogue>().GetRegisteredSituations();
	        return situationControllers.Any(c => c.IsOpen);
        }

        public void SetHighlightedElement(string elementId, int quantity = 1)
        {
            var enableAccessibleCards =
                Watchman.Get<Config>().GetConfigValueAsInt(NoonConstants.ACCESSIBLECARDS);

            if (enableAccessibleCards==null || enableAccessibleCards==0)
		        return;
	        if (elementId == null)
	        {
		        cardHoverDetail.Hide();
		        return;
	        }
	        cardHoverDetail.Populate(elementId, quantity);
	        cardHoverDetail.Show();
        }


    }


}
