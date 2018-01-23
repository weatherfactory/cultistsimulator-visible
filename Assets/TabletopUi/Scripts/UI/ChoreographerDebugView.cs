using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoreographerDebugView : MonoBehaviour {

    public Transform tabletop;
    public Rect targetRect;
    public bool tokenOverlaps;
    public Vector2[] currentPoints;
    public Rect finalRect;
    public List<Rect> checkedRects;

    public void Init(Rect targetRect, bool tokenOverlaps, Vector2[] currentPoints, Rect finalRect) {
        this.targetRect = targetRect;
        this.tokenOverlaps = tokenOverlaps;
        this.currentPoints = currentPoints;
        this.finalRect = finalRect;
    }

    // DRAWING

    private void OnDrawGizmosSelected() {
        DrawSpawnPreview();
        DrawCheckPoints();
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        DrawCheckedRects();
        Gizmos.color = Color.green;
        DrawToken(finalRect);
    }

    void DrawToken(Rect token) {
        DrawWireCube(token.position + token.size / 2f, token.size);
    }

    void DrawWireCube(Vector3 pos, Vector3 size) {
        Gizmos.DrawWireCube(tabletop.position + Vector3.Scale(pos, tabletop.lossyScale), Vector3.Scale(size, tabletop.lossyScale));
    }

    void DrawTokenSolid(Rect token) {
        DrawCube(token.position + token.size / 2f, token.size);
    }

    void DrawCube(Vector3 pos, Vector3 size) {
        Gizmos.DrawCube(tabletop.position + Vector3.Scale(pos, tabletop.lossyScale), Vector3.Scale(size, tabletop.lossyScale));
    }

    void DrawCheckedRects() {
        if (checkedRects != null)
            foreach (var item in checkedRects)
                DrawTokenSolid(item);
    }

    void DrawSpawnPreview() {
        Gizmos.color = tokenOverlaps ? Color.red : Color.grey;
        DrawToken(targetRect);
    }

    void DrawCheckPoints() {
        if (currentPoints != null)
            foreach (var item in currentPoints)
                DrawCheckPoint(item, Color.cyan);
    }

    void DrawCheckPoint(Vector2 pos, Color color) {
        Gizmos.color = color;
        Gizmos.DrawSphere(tabletop.position + Vector3.Scale(pos, tabletop.lossyScale), 2f * tabletop.lossyScale.x);
    }
}
