using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScrollRectTwo : UIBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {

    public enum MovementType {
        Unrestricted, // Unrestricted movement -- can scroll forever
        Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
        Clamped, // Restricted movement where it's not possible to go past the edges
    }


    [SerializeField] private Camera m_ScrollCamera;
    [SerializeField] private Vector2 scrollOffset;
    [SerializeField] private float scrollSpeed;

    [SerializeField] private MovementType movementType = MovementType.Elastic;

    Vector3 startCamPos;
    Vector3 currentCamPos;

    Vector2 scrollPosition;
    Vector2 scrollPositionLast;

    bool isDragging;
    Vector2 dragStartPos;
    Vector2 scrollStartPos;
    Bounds viewRectBounds;

    [SerializeField] private RectTransform dragRect;

    protected override void Start() {
        base.Start();
        startCamPos = m_ScrollCamera.transform.position;
        scrollPosition = Vector2.zero;
    }

    public virtual void OnBeginDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!IsActive())
            return;

        dragStartPos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragRect, eventData.position, eventData.pressEventCamera, out dragStartPos);
        scrollStartPos = scrollPosition;
        isDragging = true;
        viewRectBounds = GetBounds();
    }

    Bounds GetBounds() {
        if (dragRect == null)
            return new Bounds();

        Vector3[] corners = new Vector3[4];
        dragRect.GetWorldCorners(corners);

        var viewWorldToLocalMatrix = dragRect.worldToLocalMatrix;

        var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (int i = 0; i < 4; i++) {
            Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[i]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        var bounds = new Bounds(vMin, Vector3.zero);
        bounds.Encapsulate(vMax);
        return bounds;
    }

    public virtual void OnEndDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        isDragging = false;
    }

    public virtual void OnDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!IsActive())
            return;

        Vector2 localCursor;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(dragRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return;
        
        var pointerDelta = localCursor - dragStartPos;

        // substract because we invert the drag direction
        Vector2 position = scrollStartPos - pointerDelta;

        SetCamera(position);
    }

    [SerializeField] private float m_Elasticity = 0.1f; // Only used for MovementType.Elastic
    [SerializeField] private bool m_Inertia = true;
    [SerializeField] private float m_DecelerationRate = 0.135f; // Only used when inertia is enabled
    private Vector2 m_Velocity;

    protected virtual void LateUpdate() {
        float deltaTime = Time.unscaledDeltaTime;
        Vector2 offset = CalculateOffset(Vector2.zero);

        if (!isDragging && (offset != Vector2.zero || m_Velocity != Vector2.zero)) {
            Vector2 position = scrollPosition;

            for (int axis = 0; axis < 2; axis++) {
                // Apply spring physics if movement is elastic and content has an offset from the view.
                if (movementType == MovementType.Elastic && offset[axis] != 0) {
                    float speed = m_Velocity[axis];
                    position[axis] = Mathf.SmoothDamp(scrollPosition[axis], scrollPosition[axis] + offset[axis], ref speed, m_Elasticity, Mathf.Infinity, deltaTime);
                    if (Mathf.Abs(speed) < 1)
                        speed = 0;
                    m_Velocity[axis] = speed;
                }
                // Else move content according to velocity with deceleration applied.
                else if (m_Inertia) {
                    m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
                    if (Mathf.Abs(m_Velocity[axis]) < 1)
                        m_Velocity[axis] = 0;
                    position[axis] += m_Velocity[axis] * deltaTime;
                }
                // If we have neither elaticity or friction, there shouldn't be any velocity.
                else {
                    m_Velocity[axis] = 0;
                }
            }

            if (movementType == MovementType.Clamped) {
                offset = CalculateOffset(position - scrollPosition);
                position += offset;
            }

            SetCamera(position);
        }

        if (isDragging && m_Inertia) {
            Vector3 newVelocity = (scrollPosition - scrollPositionLast) / deltaTime;
            m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
        }

        if (scrollPosition != scrollPositionLast) {
            scrollPositionLast = scrollPosition;
        }
    }

    void SetCamera(Vector2 pos) {
        if (scrollPosition == pos)
            return;

        scrollPosition = pos;

        scrollPosition.x = Mathf.Clamp(pos.x, viewRectBounds.min.x, viewRectBounds.max.x);
        scrollPosition.y = Mathf.Clamp(pos.y, viewRectBounds.min.y, viewRectBounds.max.y);

        Vector2 camOffset = new Vector2(scrollPosition.x / viewRectBounds.size.x * scrollSpeed, scrollPosition.y / viewRectBounds.size.y * scrollSpeed);
        currentCamPos = startCamPos + new Vector3(scrollOffset.x * camOffset.x, scrollOffset.y * camOffset.y);
        
        m_ScrollCamera.transform.position = currentCamPos;
        Debug.Log("Setting to " + scrollPosition + " - " + currentCamPos);
    }

    private void OnDrawGizmos() {
        var pos = dragRect.transform.position;
        pos += new Vector3(scrollPosition.x, scrollPosition.y);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, 5f);
    }


    private Vector2 CalculateOffset(Vector2 delta) {
        if (movementType == MovementType.Unrestricted)
            return delta;

        Vector2 offset = Vector2.zero;

        Vector2 min = viewRectBounds.min;
        Vector2 max = viewRectBounds.max;

        // Horizontal
        min.x += delta.x;
        max.x += delta.x;

        if (min.x > viewRectBounds.min.x)
            offset.x = viewRectBounds.min.x - min.x;
        else if (max.x < viewRectBounds.max.x)
            offset.x = viewRectBounds.max.x - max.x;

        // Vertical
        min.y += delta.y;
        max.y += delta.y;

        if (max.y < viewRectBounds.max.y)
            offset.y = viewRectBounds.max.y - max.y;
        else if (min.y > viewRectBounds.min.y)
            offset.y = viewRectBounds.min.y - min.y;

        return offset;
    }

}
