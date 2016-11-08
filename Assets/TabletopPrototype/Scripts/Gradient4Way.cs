using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class Gradient4Way : BaseMeshEffect {

	public enum ColorMode { Overwrite, Additive, Multiply, Ignore };
	public ColorMode colorMode = ColorMode.Overwrite;
	public float gradientStrength = 1f;
	public bool applyAlpha = true;

	public Color32 colorTopLeft = Color.white;
	public Color32 colorTopRight = Color.black;
	public Color32 colorBottomLeft = Color.black;
	public Color32 colorBottomRight = Color.white;

	public override void ModifyMesh( VertexHelper vh ) {
		int count = vh.currentVertCount;

		if (!IsActive() || count == 0)
			return;

		var vert = new UIVertex();

		float minX = Mathf.Infinity;
		float maxX = Mathf.NegativeInfinity;
		float minY = Mathf.Infinity;
		float maxY = Mathf.NegativeInfinity;

		for (int i = 0; i < count; i++) {
			vh.PopulateUIVertex(ref vert, i);

			minX = Mathf.Min(vert.position.x, minX);
			maxX = Mathf.Max(vert.position.x, maxX);
			minY = Mathf.Min(vert.position.y, minY);
			maxY = Mathf.Max(vert.position.y, maxY);
		}

		Vector2 relativePos;

		float uiElementWidth = minX - maxX;
		float uiElementHeight = minY - maxY;

		for (int i = 0; i < count; i++) {
			vh.PopulateUIVertex(ref vert, i);

			relativePos.x = (vert.position.x - maxX) / uiElementWidth;
			relativePos.y = (vert.position.y - maxY) / uiElementHeight;

			vert.color = GetColor(vert.color, relativePos, colorMode);

			vh.SetUIVertex(vert, i);
		}
	}

	private Color GetColor(Color vertColor, Vector2 vertexPos, ColorMode colorMode) {
		if (Mathf.Approximately(gradientStrength, 0f))
			return vertColor;			

		var color = CombineColor(vertColor, GetColorForPosition(vertexPos), colorMode);

		if (!applyAlpha)
			color.a = vertColor.a;

		return color;
	}

	private Color GetColorForPosition( Vector2 vertexPos ) {
		Color leftColor;
		Color rightColor;

		leftColor = Color.Lerp(colorTopLeft, colorBottomLeft, vertexPos.y);
		rightColor = Color.Lerp(colorTopRight, colorBottomRight, vertexPos.y);

		return Color.Lerp(leftColor, rightColor, 1f - vertexPos.x);
	}

	private Color CombineColor(Color vertColor, Color newColor, ColorMode colorMode) {
		if (Mathf.Approximately(gradientStrength, 0f))
			return vertColor;			

		switch (colorMode) {
		case ColorMode.Ignore:
			return vertColor;
		case ColorMode.Additive:
			return vertColor + newColor * gradientStrength;
		case ColorMode.Multiply:
			return vertColor * newColor * gradientStrength;
		case ColorMode.Overwrite:
		default:
			return newColor * gradientStrength;
		}
	}
}