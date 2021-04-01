using System;
using System.Collections.Generic;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI.Scripts;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories.UI {
    [ExecuteInEditMode]
    public class EgressThreshold : ThresholdSphere{

        public event System.Action<Token> onCardDropped;

        public Image activeGlow;
        public Color defaultBackgroundColor;
        public Image doorColor;
        bool isActive;

        const float glowDefaultFactor = 0.65f;

        public override bool AllowDrag { get { return false; } }
        public override bool AllowStackMerge { get { return false; } }
        private Sphere _evictionDestination;


        //public void Start()
        //{
        //    ShowGlow(false, true);
        //    slotGlow.Hide(true);
        //}
        
        
        public void SetEvictionDestination(Sphere destinationSphere)
        {
            _evictionDestination = destinationSphere;
        }


        public override void EvictToken(Token token, Context context)
        {
            if(_evictionDestination!=null)
                _evictionDestination.AcceptToken(token,context);
            else
            base.EvictToken(token,context);

        }

        public void SetAsActive(bool active) {
            isActive = active;
            activeGlow.gameObject.SetActive(active);
            doorColor.color = defaultBackgroundColor;
            activeGlow.color = defaultBackgroundColor;
     //       ShowGlow(false, true);
        }





    }
}
