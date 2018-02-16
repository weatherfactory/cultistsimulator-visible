using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GoldenMaterialEditor : MaterialEditor
{
    public override void OnInspectorGUI()
    {
        if (!isVisible)
            return;

        Material material = target as Material;

        MaterialProperty[] properties = GetMaterialProperties(targets);

        string[] keys = material.shaderKeywords;

        bool effectsLayer1Enabled = keys.Contains("EFFECTS_LAYER_1_ON");
        bool effectsLayer2Enabled = keys.Contains("EFFECTS_LAYER_2_ON");
        bool effectsLayer3Enabled = keys.Contains("EFFECTS_LAYER_3_ON");
        bool effectsLayer4Enabled = keys.Contains("EFFECTS_LAYER_4_ON");

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < 3; i++)
            TexturePropertySingleLine(new GUIContent(properties[i].displayName), properties[i]);

        EditorGUILayout.Separator();

        effectsLayer1Enabled = EditorGUILayout.Toggle("Effects Layer 1", effectsLayer1Enabled);
        if (effectsLayer1Enabled)
            DrawEffectsLayer(properties, 1);

        effectsLayer2Enabled = EditorGUILayout.Toggle("Effects Layer 2", effectsLayer2Enabled);
        if (effectsLayer2Enabled)
            DrawEffectsLayer(properties, 2);

        effectsLayer3Enabled = EditorGUILayout.Toggle("Effects Layer 3", effectsLayer3Enabled);
        if (effectsLayer3Enabled)
            DrawEffectsLayer(properties, 3);

        effectsLayer4Enabled = EditorGUILayout.Toggle("Effects Layer 4", effectsLayer4Enabled);
        if (effectsLayer4Enabled)
            DrawEffectsLayer(properties, 4);

        if (EditorGUI.EndChangeCheck())
        {
            string[] newKeys = new string[] {
                effectsLayer1Enabled ? "EFFECTS_LAYER_1_ON" : "EFFECTS_LAYER_1_OFF",
                effectsLayer2Enabled ? "EFFECTS_LAYER_2_ON" : "EFFECTS_LAYER_2_OFF",
                effectsLayer3Enabled ? "EFFECTS_LAYER_3_ON" : "EFFECTS_LAYER_3_OFF",
                effectsLayer4Enabled ? "EFFECTS_LAYER_4_ON" : "EFFECTS_LAYER_4_OFF",
            };

            material.shaderKeywords = newKeys;
            EditorUtility.SetDirty(material);
        }
    }

    void DrawEffectsLayer(MaterialProperty[] properties, int layer)
    {
        GUIStyle style = EditorStyles.helpBox;
        style.margin = new RectOffset(20, 20, 0, 0);  

        EditorGUILayout.BeginVertical(style);
        {
            TexturePropertySingleLine(new GUIContent("Effect Texture"), properties.GetByName(EffectName(layer, "Tex")));
            TexturePropertySingleLine(new GUIContent("Motion Texture"), properties.GetByName(EffectName(layer, "Motion")));

            ColorProperty(properties.GetByName(EffectName(layer, "Color")), "Tint Color");

            FloatProperty(properties.GetByName(EffectName(layer, "MotionSpeed")), "Motion Speed");
            FloatProperty(properties.GetByName(EffectName(layer, "Rotation")), "Rotation Speed");

            Vector4 translation = properties.GetByName(EffectName(layer, "Translation")).vectorValue;
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Positon");
                translation.x = EditorGUILayout.FloatField(translation.x);
                translation.y = EditorGUILayout.FloatField(translation.y);
            }
            EditorGUILayout.EndHorizontal();
            properties.GetByName(EffectName(layer, "Translation")).vectorValue = translation;

            Vector4 pivotScale = properties.GetByName(EffectName(layer, "PivotScale")).vectorValue;
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Pivot");
                pivotScale.x = EditorGUILayout.FloatField(pivotScale.x);
                pivotScale.y = EditorGUILayout.FloatField(pivotScale.y);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Scale");
                pivotScale.z = EditorGUILayout.FloatField(pivotScale.z);
                pivotScale.w = EditorGUILayout.FloatField(pivotScale.w);
            }
            EditorGUILayout.EndHorizontal();
            properties.GetByName(EffectName(layer, "PivotScale")).vectorValue = pivotScale;

            BoolProperty(properties.GetByName(EffectName(layer, "Foreground")), "Foreground");
        }
        EditorGUILayout.EndVertical();
    }

    bool BoolProperty(MaterialProperty property, string name)
    {
        bool toggle = property.floatValue == 0 ? false : true;
        toggle = EditorGUILayout.Toggle(name, toggle);
        property.floatValue = toggle ? 1 : 0;

        return toggle;
    }

    string EffectName(int layer, string property)
    {
        return string.Format("_EffectsLayer{0}{1}", layer.ToString(), property);
    }
}