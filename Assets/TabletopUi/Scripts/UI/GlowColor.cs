using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIStyle {
    public enum TokenGlowColor { HighlightPink, Default, Hover }

    public static Color hoverWhite = new Color32(0xFF, 0xFF, 0xFF, 0xFF);

    public static Color brightPink = new Color32(0xFF, 0xA8, 0xEA, 0xFF);
    public static Color lightBlue = new Color32(0x94, 0xE2, 0xEF, 0xFF);

    public static Color slotPink = new Color32(0xFF, 0xA8, 0xEA, 0xFF); // new Color32(0x8E, 0x5D, 0x82, 0xFF) // DARKER HIGHLIGHT VARIANT
    public static Color slotDefault = new Color32(0x1C, 0x43, 0x62, 0xFF);

    public static Color GetGlowColor(TokenGlowColor colorType) {
        switch (colorType) {
            case TokenGlowColor.HighlightPink:
                return brightPink;
            case TokenGlowColor.Hover:
                return hoverWhite;
            case TokenGlowColor.Default:
            default:
                return lightBlue;
        }
    }

    public static Color GetSlotGlowColor(TokenGlowColor colorType) {
        switch (colorType) {
            case TokenGlowColor.HighlightPink:
                return slotPink;
            case TokenGlowColor.Hover:
                return hoverWhite;
            case TokenGlowColor.Default:
            default:
                return slotDefault;
        }
    }

}
