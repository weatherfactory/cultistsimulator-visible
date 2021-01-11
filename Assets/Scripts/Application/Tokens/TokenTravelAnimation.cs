using System.Collections;
using System.Collections.Generic;
using SecretHistories.Entities;
using UnityEngine;
using SecretHistories.UI;
using SecretHistories.States.TokenStates;


public class TokenTravelAnimation : MonoBehaviour {

	public event System.Action<Token,Context> OnTokenArrival;

	protected Token _token;
    protected Context _context;

    [SerializeField]
	private Vector3 _startPosition;
    [SerializeField]
    private Vector3 _endPosition;

    [SerializeField]
    private float _scaleStart = 1f;
    
    [SerializeField]
    private float _scaleEnd = 1f;

    [SerializeField]
    private float _scalePercentage = 0.8f; // needs to be below 0.5 if both scales are active

    [SerializeField]
    private float _travelDuration = -1f; // do not animate while this is < 0
    
    [SerializeField]
    private float _travelTimeElapsed = 0f;




    protected virtual Vector3 StartPosition => _startPosition;

    protected virtual Vector3 EndPosition => _endPosition;

    public void SetPositions(Vector3 startPos, Vector3 endPos) { //0f default
		this._startPosition = startPos;
		this._endPosition = endPos;
	}

	public void SetScaling(float scaleStart, float scaleEnd, float scaleDuration) { 
		this._scaleStart = scaleStart;
		this._scaleEnd = scaleEnd;

        float _scalePercentageMultiplier;

      //  if (Math.Abs(scaleStart - 1f) > 0.01f && Math.Abs(scaleEnd - 1f) > 0.01f) //maybe later
			if (scaleStart != 1f && scaleEnd != 1f)
            _scalePercentageMultiplier = 0.5f;// may not be bigger than 0.5 for dual scaling
		else
            _scalePercentageMultiplier = 1f;

        this._scalePercentage = Mathf.Clamp01(scaleDuration) * _scalePercentageMultiplier;

	//	this._scalePercentage = Mathf.Clamp01(scaleDuration) * ((scaleStart != 1f && scaleEnd != 1f) ? 0.5f : 1f); // may not be bigger than 0.5 for dual scaling
	}

	public virtual void Begin(Token token,Context context, float duration) {
		this._travelDuration = duration;
		this._travelTimeElapsed = 0f;
        
        _token = token;
        _context = context;

        _token.ManifestationRectTransform.localScale = Vector3.one * _scaleStart;
        transform.SetAsLastSibling();
	}

	void Update ()
    {
        if (_token.PauseAnimations)
            return;
		if (_travelDuration < 0)
			return;
		else if (_travelTimeElapsed < _travelDuration) 
			Continue();
		else
			Complete();
	}

	void Continue() {

		_travelTimeElapsed += Time.deltaTime;


		float completion = _travelTimeElapsed / _travelDuration;

		_token.TokenRectTransform.position = GetPos(Easing.Circular.Out(completion));

		if (_scaleStart != 1f && completion < _scalePercentage)
			_token.TokenRectTransform.localScale = Vector3.Lerp(Vector3.one * _scaleStart, Vector3.one, Easing.Quartic.Out(completion / _scalePercentage));
		else if (_scaleEnd != 1f  && completion > (1f - _scalePercentage))
            _token.TokenRectTransform.localScale = Vector3.Lerp(Vector3.one * _scaleEnd, Vector3.one, Easing.Quadratic.Out((1f - completion) / _scalePercentage));
		else
            _token.TokenRectTransform.localScale = Vector3.one;
	}

    Vector3 GetPos(float lerp) {
        return new Vector3(Mathf.Lerp(StartPosition.x, EndPosition.x, lerp), Mathf.Lerp(StartPosition.y, EndPosition.y, lerp), EndPosition.z);
    }

	void Complete() {;
		_token.TokenRectTransform.position = new Vector3(EndPosition.x, EndPosition.y, EndPosition.z);
		_token.TokenRectTransform.localScale = Vector3.one * _scaleEnd;
        NoonUtility.LogWarning(
            "we used to disable the token while it was travelling. We don't in fact want to do that, but we should probably disable raycasts");

        OnTokenArrival?.Invoke(_token,_context);
		Destroy(this);
	}

}
