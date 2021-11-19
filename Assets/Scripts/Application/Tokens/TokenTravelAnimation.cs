using System;
using System.Collections;
using System.Collections.Generic;
using SecretHistories.Constants.Events;
using SecretHistories.Entities;
using SecretHistories.Events;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using UnityEngine;
using SecretHistories.UI;
using SecretHistories.States.TokenStates;


public class TokenTravelAnimation : MonoBehaviour,ISphereEventSubscriber {

	public event Action<Token,Context> OnTokenArrival;
    public event Func<SphereBlock,int> OnBlockRedundant;

	protected Token _token;
    protected Context _context;

    public float GetDurationElapsed()
    {
        return _travelTimeElapsed;
    }

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

    public bool Defunct { get; private set;}
    


    public SphereBlock AppliesSphereBlock()
    {
        return new SphereBlock(BlockDirection.Inward,
            BlockReason.InboundTravellingStack);
    }

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

        _token.SetLocalScale(Vector3.one * _scaleStart);
        _token.MakeNonInteractable();
        transform.SetAsLastSibling();
	}

public void ExecuteHeartbeat(float seconds, float metaseconds)
{
    if (Defunct)
        return;

        if (_token.PauseAnimations)
            return;
		if (_travelDuration < 0)
			return;
		else if (_travelTimeElapsed < _travelDuration) 
			Continue( metaseconds);
		else
			Complete();
	}

	void Continue( float metaseconds) {

        //	_travelTimeElapsed += Time.deltaTime;
        _travelTimeElapsed += metaseconds;

        float completion = _travelTimeElapsed / _travelDuration;

		_token.TokenRectTransform.position = GetPos(Easing.Circular.Out(completion));

		if (_scaleStart != 1f && completion < _scalePercentage)
			_token.SetLocalScale(Vector3.Lerp(Vector3.one * _scaleStart, Vector3.one, Easing.Quartic.Out(completion / _scalePercentage)));
		else if (_scaleEnd != 1f  && completion > (1f - _scalePercentage))
            _token.SetLocalScale(Vector3.Lerp(Vector3.one * _scaleEnd, Vector3.one, Easing.Quadratic.Out((1f - completion) / _scalePercentage)));
		else
            _token.SetLocalScale(Vector3.one);
	}

    Vector3 GetPos(float lerp) {
        return new Vector3(Mathf.Lerp(StartPosition.x, EndPosition.x, lerp), Mathf.Lerp(StartPosition.y, EndPosition.y, lerp), EndPosition.z);
    }

	void Complete() {;
		_token.TokenRectTransform.position = new Vector3(EndPosition.x, EndPosition.y, EndPosition.z);
		_token.SetLocalScale(Vector3.one * _scaleEnd);
 
        _token.MakeInteractable();
        OnTokenArrival?.Invoke(_token,_context);
    }

    public void OnSphereChanged(SphereChangedArgs args)
    {
        if(Defunct)
        {
            args.Sphere.Unsubscribe(this);
            return;
        }
        //destination sphere has been changed, eg its reference point has moved
        var destinationSphere = args.Sphere;
        var sphereTokenLocation = args.Context.TokenDestination;

        var newItinerary = destinationSphere.GetItineraryFor(_token);

        newItinerary.Divert(_token,args.Context);

    }

    public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
    {
        //tokens in destination sphere have been changed
    }

    public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
    {
        //someone's done something else in the destination sphere
    }

    public void Retire()
    {
        
        Defunct = true;
        Destroy(this);
    }

    public void OnDestroy()
    {
        OnBlockRedundant?.Invoke(AppliesSphereBlock());
    }
}
