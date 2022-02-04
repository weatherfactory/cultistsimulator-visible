﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations
{
    public enum HighlightType
    {
        WillInteract,
        All,
        Hover,
        AttentionPls,
        PotentiallyRelevant
    }

    public interface IManifestation
    {
        Transform Transform { get; }
       RectTransform RectTransform { get; }
        void Retire(RetirementVFX retirementVfx, Action callback);
        bool CanAnimateIcon();
        void BeginIconAnimation();
        
        void Initialise(IManifestable manifestable);

        void UpdateVisuals(IManifestable manifestable);
        void UpdateLocalScale(Vector3 newScale);
        void OnBeginDragVisuals();
        void OnEndDragVisuals();

        void Highlight(HighlightType highlightType, IManifestable manifestable);
        void Unhighlight(HighlightType highlightType, IManifestable manifestable);
        bool NoPush { get; }
        void Unshroud(bool instant);
        void Shroud(bool instant);
        void Emphasise();
        void Understate();
        bool RequestingNoDrag { get; }
        bool RequestingNoSplit { get; }
        void DoMove(RectTransform tokenRectTransform);

        bool HandlePointerClick(PointerEventData eventData, Token token);

        IGhost CreateGhost();


    }
}
