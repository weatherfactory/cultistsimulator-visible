using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIStyle {
    public enum TokenGlowColor { Pink, Blue }

    public static Color brightPink = new Color32(0xFF, 0xA8, 0xEA, 0xFF);
    public static Color lightBlue = new Color32(0x94, 0xE2, 0xEF, 0xFF);
    
    public static Color slotPink = new Color32(0xFF, 0xA8, 0xEA, 0xFF); // new Color32(0x8E, 0x5D, 0x82, 0xFF) // DARKER HIGHLIGHT VARIANT
    public static Color slotDefault = new Color32(0x1C, 0x43, 0x62, 0xFF);

    public static Color GetGlowColor(TokenGlowColor colorType) {
        return colorType == TokenGlowColor.Pink ? brightPink : lightBlue;
    }

    public static Color GetSlotGlowColor(TokenGlowColor colorType) {
        return colorType == TokenGlowColor.Pink ? slotPink : slotDefault;
    }

}
