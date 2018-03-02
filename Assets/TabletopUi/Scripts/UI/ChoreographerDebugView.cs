using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoreographerDebugView : MonoBehaviour {

    public bool showCurrent = false;

    public Transform tabletop;
    public Rect targetRect;
    public bool tokenOverlaps;
    public List<Vector2> checkedPoints;
    public Rect finalRect;
    public List<Rect> checkedRects;

    public bool hasDebugData;

    //public List<string> debugLog = new List<string>();

    // DESTROY

    public void InitKill(float duration) {
        Invoke("Kill", duration);
    }

    void Kill() {
        Destroy(gameObject);
    }

    // DRAWING

    private void OnDrawGizmosSelected() {
        if (showCurrent) {
            Gizmos.color = Color.cyan;

            var container = tabletop.GetComponent<TabletopTokenContainer>();

            foreach (var item in container.GetTokens()) {
                DrawToken(GetCenterPosRect(item.RectTransform.anchoredPosition, item.RectTransform.rect.size));
            }
        }
        if (hasDebugData) { 
            DrawSpawnPreview();
            DrawCheckPoints();
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            DrawCheckedRects();
            Gizmos.color = Color.green;
            DrawToken(finalRect);
        }
    }

    // Tokens have their pos in their center, rects in the bottom right
    Rect GetCenterPosRect(Vector2 centerPos, Vector2 size) {
        return new Rect(centerPos - size / 2f, size);
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
        if (checkedPoints != null)
            foreach (var item in checkedPoints)
                DrawCheckPoint(item, Color.cyan);
    }

    void DrawCheckPoint(Vector2 pos, Color color) {
        Gizmos.color = color;
        Gizmos.DrawSphere(tabletop.position + Vector3.Scale(pos, tabletop.lossyScale), 8f * tabletop.lossyScale.x);
    }
}
