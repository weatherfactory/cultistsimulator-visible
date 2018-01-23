using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TestTokenChoreographer : MonoBehaviour {

    public Rect table = new Rect(0f, 0f, 100f, 50f);
    public int numStartingItems = 20;
    public Vector3 verbSize = new Vector3(6f, 6f, 0f);
    public Vector3 cardSize = new Vector3(4f, 5f, 0f);

    Token[] tokens;

    int spawnStage = 0;

    Vector3 targetSpawnPos;
    Token targetToken;
    bool tokenOverlaps;

    List<Vector3> checkedPoints;
    Vector3[] currentPoints;

    Token finalToken;

    Vector2 tablePadding = new Vector2(2f, 2f);

    float checkPointPerArcLength = 70f;
    float radiusBase = 50f;
    float radiusIncrement = 35f;
    float radiusMaxSize = 250f;

    [Header("Display")]
    public bool showOverlapBoundaries;
    
    public struct Token {
        public Vector3 pos;
        public Vector2 size;
    }

	// Use this for initialization
	void OnEnable () {
        Init();
	}

    public void Init() {
        spawnStage = 0;
        tokenOverlaps = false;
        checkedPoints = null;
        currentPoints = null;
        BuildRandomItems(numStartingItems);
    }

    void BuildRandomItems(int num) {
        tokens = new Token[num];

        for (int i = 0; i < num; i++) {
            tokens[i] = new Token();
            tokens[i].pos = new Vector3(
                Mathf.Lerp(table.x + cardSize.x / 2f, table.x + table.width - cardSize.x / 2f, Random.value),
                Mathf.Lerp(table.y + cardSize.y / 2f, table.y + table.height - cardSize.y / 2f, Random.value),
                0f);
            tokens[i].size = Random.value < 0.2f ? verbSize : cardSize;
        }
    }

    public void ShowSpawnPreviewAtPos(Vector3 pos) {
        targetSpawnPos = GetPosClampedToTable(pos);
        targetToken = new Token();
        targetToken.pos = targetSpawnPos;
        targetToken.size = cardSize;

        tokenOverlaps = !IsLegalPosition(targetToken);
        spawnStage = 1;
    }

    Vector3 GetPosClampedToTable(Vector3 pos) {
        const float padding = .2f;

        pos.x = Mathf.Clamp(pos.x, table.x + padding, table.x + table.width - padding);
        pos.y = Mathf.Clamp(pos.y, table.y + padding, table.y + table.height - padding);
        return pos;
    }

    public void Spawn() {
        spawnStage = 2;

        if (IsLegalPosition(targetToken)) {
            checkedPoints = null;
            currentPoints = null;
            SetFinalPos(targetToken.pos);
            return;
        }

        float radius = radiusBase;
        checkedPoints = new List<Vector3>();

        while (radius < radiusMaxSize) {
            currentPoints = GetTestPoints(targetToken.pos, radius);

            foreach (var point in currentPoints) {
                if (IsLegalPosition(GetRect(point, targetToken.size))) {
                    Debug.Log("Possible Pos! " + checkedPoints.Count);
                    SetFinalPos(point);
                    return;
                }

                checkedPoints.Add(point);
            }

            radius += radiusIncrement;
        }

        Debug.LogWarning("No position found! Rechecking starting point with more tolerance.");

        if (IsLegalPosition(GetRect(targetToken.pos, targetToken.size / 3f))) {
            SetFinalPos(targetToken.pos);
            return;
        }

        Debug.LogWarning("No position found! Rechecking all points with more tolerance.");

        foreach (var point in checkedPoints) {
            if (IsLegalPosition(GetRect(point, targetToken.size / 3f))) {
                Debug.Log("Possible Pos! " + checkedPoints.Count);
                SetFinalPos(point);
                return;
            }
        }

        Debug.LogWarning("No position found! Dumping in middle.");
        SetFinalPos(new Vector3(table.x + table.width * 0.5f, table.y + table.height * 0.5f));
    }

    void SetFinalPos(Vector3 pos) {
        spawnStage = 3;
        finalToken = new Token();
        finalToken.pos = pos;
        finalToken.size = targetToken.size;
    }

    // POSITIONING

    bool IsLegalPosition(Token token) {
        return IsLegalPosition(GetRectFromToken(targetToken));
    }

    bool IsLegalPosition(Rect rect) {
        if (table.Contains(rect.position + rect.size / 2f) == false)
            return false;

        Rect compareRect;

        foreach (var item in tokens) {
            compareRect = GetRectFromToken(item);

            if (compareRect.Overlaps(rect))
                return false;
        }

        return true;
    }

    Rect GetTableRectWithPadding() {
        return new Rect(table.x + tablePadding.x, table.y + tablePadding.y, table.width - tablePadding.x - tablePadding.x, table.height - tablePadding.y - tablePadding.y );
    }

    Rect GetRectFromToken(Token token) {
        return GetRect(token.pos, token.size);
    }

    Rect GetRect(Vector3 pos, Vector3 size) {
        return new Rect(new Vector2(pos.x - size.x / 2f, pos.y - size.y / 2f), size);
    }

    Vector3[] GetTestPoints(Vector3 pos, float radius) {
        float circumference = 2f * Mathf.PI * radius;
        int numPoints = Mathf.FloorToInt(circumference / checkPointPerArcLength);

        var points = new Vector3[numPoints];
        float angleSteps = Mathf.Deg2Rad * 360f / points.Length;

        for (int i = 0; i < points.Length; i++) 
            points[i] = GetPointOnCircle(pos, radius, -i * angleSteps);
        
        return points;
    }

    Vector2 GetPointOnCircle(Vector3 origin, float radius, float angle) {
        return new Vector2(origin.x + radius * Mathf.Cos(angle),
                           origin.y + radius * Mathf.Sin(angle));
    }

    // DRAWING

    private void OnDrawGizmos() {
        DrawTable();
        DrawItems();

        if (spawnStage >= 1) 
            DrawSpawnPreview();
        if (spawnStage >= 2) 
            DrawCheckPoints();
        if (spawnStage >= 3) {
            Gizmos.color = Color.green;
            DrawToken(finalToken);
        }
    }

    void DrawTable() {
        Gizmos.color = new Color(0f, 0f, 0f, 0.5f);
        Gizmos.DrawCube(table.position + table.size / 2f, table.size);
    }

    void DrawItems() {
        foreach (var item in tokens) {
            Gizmos.color = Color.yellow;
            DrawToken(item);
        }
    }
    
    void DrawToken(Token token) {
        DrawWireCube(token.pos, token.size);
    }

    void DrawWireCube(Vector3 pos, Vector3 size) {
        Gizmos.DrawWireCube(pos, size);
    }

    void DrawSpawnPreview() {
        Gizmos.color = tokenOverlaps ? Color.red : Color.grey;
        DrawToken(targetToken);
    }

    void DrawCheckPoints() {
        if (currentPoints != null)
            foreach (var item in currentPoints)
                DrawCheckPoint(item, Color.cyan);

        if (checkedPoints != null)
            foreach (var item in checkedPoints)
                DrawCheckPoint(item, Color.red);
    } 

    void DrawCheckPoint(Vector3 pos, Color color) {
        Gizmos.color = color;
        Gizmos.DrawSphere(pos, 2f);
    }

}

[CustomEditor(typeof(TestTokenChoreographer))]
public class TestTokenChoreographerEditor : Editor {

    [SerializeField]
    Vector3 spawnPos = new Vector3(50f, 25f);

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var choreo = target as TestTokenChoreographer;

        if (GUILayout.Button("Re-Init")) { 
            choreo.Init();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Spawn")) {
            choreo.Spawn();
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI() {
        var choreo = target as TestTokenChoreographer;

        EditorGUI.BeginChangeCheck();

        Vector3 newTargetPosition = Handles.PositionHandle(spawnPos, Quaternion.identity);

        if (EditorGUI.EndChangeCheck()) {
            spawnPos = newTargetPosition;
            choreo.ShowSpawnPreviewAtPos(spawnPos);
        }
    }

}