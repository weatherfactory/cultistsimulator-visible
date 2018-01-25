using System.Collections;
using System.Collections.Generic;
using Assets.Core.Entities;
using UnityEngine;
using Assets.CS.TabletopUI;

public class TokenAnimation : MonoBehaviour {

	public event System.Action<SituationToken> onAnimDone;

	protected DraggableToken token;

	private RectTransform startToken;
	private Vector3 startPosition;

	private RectTransform endToken;
	private Vector3 endPosition;

	private float zPos;

	private float scaleStart = 1f;
	private float scaleEnd = 1f;

	private float scalePercentage = 0.8f; // needs to be below 0.5 if both scales are active

	private float duration = -1f; // do not animate while this is < 0
	private float timeSpent = 0f;

	public bool IsRunning { private get; set; }

	private Vector3 startPos {
		get {
			if (startToken != null)
				return startToken.anchoredPosition3D;
			else
				return startPosition;
		}
	}

	private Vector3 endPos {
		get {
			if (endToken != null)
				return endToken.anchoredPosition3D;
			else
				return endPosition;
		}
	}

	public void SetPositions(Vector3 startPos, Vector3 endPos, float zPos = 0f) {
		this.startPosition = startPos;
		this.endPosition = endPos;
		this.zPos = zPos;
	}

	public void SetPositions(RectTransform startPos, Vector3 endPos, float zPos = 0f) {
		this.startToken = startPos;
		this.endPosition = endPos;
		this.zPos = zPos;
	}

	public void SetPositions(Vector3 startPos, RectTransform endPos, float zPos = 0f) {
		this.startPosition = startPos;
		this.endToken = endPos;
		this.zPos = zPos;
	}

	public void SetPositions(RectTransform startPos, RectTransform endPos, float zPos = 0f) {
		this.startToken = startPos;
		this.endToken = endPos;
		this.zPos = zPos;
	}

	public void SetScaling(float scaleStart, float scaleEnd, float scaleDuration = 1f) {
		this.scaleStart = scaleStart;
		this.scaleEnd = scaleEnd;
		this.scalePercentage = Mathf.Clamp01(scaleDuration) * ((scaleStart != 1f && scaleEnd != 1f) ? 0.5f : 1f); // may not be bigger than 0.5 for dual scaling
	}

	public void StartAnim(float duration = 1f) {
		this.duration = duration;
		this.timeSpent = 0f;

		token = GetComponent<DraggableToken>();
		token.enabled = false;
        token.IsBeingAnimated = true;
        token.RectTransform.localScale = Vector3.one * scaleStart;
		IsRunning = true;
	}

	void Update () {
		if (!IsRunning || duration < 0)
			return;
		else if (timeSpent < duration) 
			DoAnim();
		else
			CompleteAnim();
	}

	void DoAnim() {
		timeSpent += Time.deltaTime;

		float completion = timeSpent / duration;

		token.RectTransform.anchoredPosition3D = GetPos(Easing.Circular.Out(completion));

		if (scaleStart != 1f && completion < scalePercentage)
			transform.localScale = Vector3.Lerp(Vector3.one * scaleStart, Vector3.one, Easing.Quadratic.Out(completion / scalePercentage));
		else if (scaleEnd != 1f  && completion > (1f - scalePercentage))
			transform.localScale = Vector3.Lerp(Vector3.one * scaleEnd, Vector3.one, Easing.Quadratic.Out((1f - completion) / scalePercentage));
		else 
			transform.localScale = Vector3.one;
	}

    Vector3 GetPos(float lerp) {
        return new Vector3(Mathf.Lerp(startPos.x, endPos.x, lerp), Mathf.Lerp(startPos.y, endPos.y, lerp), zPos);
    }

	void CompleteAnim() {
		IsRunning = false;
		token.RectTransform.anchoredPosition3D = new Vector3(endPos.x, endPos.y, zPos);
		token.RectTransform.localScale = Vector3.one * scaleEnd;
		token.enabled = true;
        token.IsBeingAnimated = false;

		FireCompleteEvent();
		Destroy(this);
	}

	protected virtual void FireCompleteEvent() {
        if (onAnimDone != null) {
            var sitToken = token as SituationToken;

            if (sitToken != null)
                onAnimDone(sitToken);
        }
	}

}
