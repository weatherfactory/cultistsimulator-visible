using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Elements.Manifestations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.Elements
{
    public class CardManifestation: MonoBehaviour,IElementManifestation
    {

        [SerializeField] Image artwork;
        [SerializeField] Image backArtwork;
        [SerializeField] Image textBackground;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] ElementStackBadge stackBadge;
        [SerializeField] TextMeshProUGUI stackCountText;
        [SerializeField] GameObject decayView;
        [SerializeField] TextMeshProUGUI decayCountText;
        [SerializeField] Sprite spriteDecaysTextBG;
        [SerializeField] Sprite spriteUniqueTextBG;
        [SerializeField] GameObject shadow;

        [SerializeField] CardVFX defaultRetireFX = CardVFX.CardBurn;
    }
}
