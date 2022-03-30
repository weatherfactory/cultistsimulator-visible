﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Ghosts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SecretHistories.Assets.Scripts.Application.Tokens.Ghosts
{
    public class ThingGhost: AbstractGhost
    {
        [SerializeField] private Image artwork;
        [SerializeField] public TextMeshProUGUI text;

        public override void UpdateVisuals(IManifestable manifestable)
        {
            text.text = manifestable.Label;
            Sprite sprite = ResourcesManager.GetSpriteForElement(manifestable.Icon);
            artwork.sprite = sprite;
        }
    }
}
