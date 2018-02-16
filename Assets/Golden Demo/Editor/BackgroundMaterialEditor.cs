using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BackgroundMaterialEditor : MaterialEditor
{
    public override void OnInspectorGUI()
    {
        if (!isVisible)
            return;

        Material material = target as Material;

        MaterialProperty[] properties = GetMaterialProperties(targets);

        string[] keys = material.shaderKeywords;

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < 1; i++)
            TexturePropertySingleLine(new GUIContent(properties[i].displayName), properties[i]);

        EditorGUILayout.Separator();

        DrawEffectsLayer(properties, 1);

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(material);
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