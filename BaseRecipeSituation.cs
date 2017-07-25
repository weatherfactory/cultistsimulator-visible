using System;
using System.Collections.Generic;
using System.Linq;

using Noon;
using UnityEngine.Assertions;

public abstract class BaseRecipeSituation:IRecipeSituation
{
    protected float _timeRemaining;
    protected List<IRecipeSituationSubscriber> _subscribers=new List<IRecipeSituationSubscriber>();
    protected Compendium _compendium;
    private Recipe _currentRecipe;
    private bool _fresh;

    
    public string CurrentRecipeId
    {
        get
        {
            if (CurrentRecipe == null)
                return null;
            return (CurrentRecipe.Id);
        }
    }

    protected IElementsContainer CharacterContainer;

    ///this is the id of the *originating* recipe, although the recipe inside may change later.
    ///the original recipe may be important for ongoing situations
    public string OriginalRecipeId { get; set; }

    public float TimeRemaining
    {
        get
        {
            if (_timeRemaining < 0)
                return 0;
            return _timeRemaining;
        }
        set { _timeRemaining = value; }
    }

    public RecipeTimerState TimerState
    {
        get
        {
            if (_fresh)
            {
                _fresh = false;
                return RecipeTimerState.Fresh;
            }


            if(CurrentRecipe==null)
                return RecipeTimerState.Extinct;
            if (TimeRemaining > 0)
                return RecipeTimerState.Ongoing;
            
                return RecipeTimerState.Complete;
        }
    }

    protected Recipe CurrentRecipe
    {
        get { return _currentRecipe; }
        set
        {
            _fresh = true;
            _currentRecipe = value;
        }
    }



    public virtual int GetInternalElementQuantity(string forElementId)
    {
        return 0;
    }

    public BaseRecipeSituation(Recipe recipe, float? timeremaining,IElementsContainer inputContainer,Compendium rc)
    {
        CurrentRecipe = recipe;
        OriginalRecipeId = CurrentRecipe.Id;
        _compendium = rc;

        if (timeremaining == null)
            TimeRemaining = CurrentRecipe.Warmup;
        else
            TimeRemaining = timeremaining.Value;

         CharacterContainer = inputContainer;


    }

    public void DoHeartbeat()
    {
        _timeRemaining--;
        if (_timeRemaining <= 0)
        
            Complete();
        else
            Continue();
    }

    private void Continue()
    {
        SituationInfo info = new SituationInfo();

        info.Label = CurrentRecipe.Label;
        info.Warmup = CurrentRecipe.Warmup;
        info.TimeRemaining = TimeRemaining;
        info.State = TimerState;

        publishUpdate(info);
    }


    public void Extinguish()
    {
        _currentRecipe = null;
    }

 
    public void Subscribe(IRecipeSituationSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
        SituationInfo info = new SituationInfo();

        info.Label = CurrentRecipe.Label;
        info.Warmup = CurrentRecipe.Warmup;
        info.TimeRemaining = TimeRemaining;
        info.State = TimerState;
        subscriber.SituationUpdated(info);
    }

    public abstract bool IsInteractive();

    protected virtual void publishUpdate(SituationInfo info)
    {
        foreach (var s in _subscribers)
            if(info.State==RecipeTimerState.Fresh)
                s.SituationBegins(info,this);
        else
            s.SituationUpdated(info);
    }




    protected virtual void resetWithRecipe(Compendium compendium)
    {
        CurrentRecipe = compendium.GetRecipeById(CurrentRecipe.Loop);
        TimeRemaining = CurrentRecipe.Warmup;
        
    }


    protected void Complete()
    {

        CurrentRecipe = ProcessRecipe();

        SituationInfo info = new SituationInfo();

        if (CurrentRecipe.Loop != null)
        {
            resetWithRecipe(_compendium);
            info.Message = CurrentRecipe.Description;
        }
        else
        {
            info.Message = CurrentRecipe.Description;
            Extinguish();
        }

        info.OriginalActionId = _compendium.GetRecipeById(OriginalRecipeId).ActionId;
        info.State = TimerState;
        info.TimeRemaining = TimeRemaining;

        publishUpdate(info);

    }

    protected virtual Recipe ProcessRecipe()
    {
        //find alternative recipe(s) - there may be additional recipes
        List<Recipe> recipesToExecute =
            _compendium.GetActualRecipesToExecute(CurrentRecipe, CharacterContainer);

        foreach (Recipe executingRecipe in recipesToExecute)
        {
            executingRecipe.Do(CharacterContainer);
        }

        //if the first recipe in the list of alternatives is a different recipe, switch it.
        //This can be important if the alternative recipe has a different loop.


        if (recipesToExecute[0].Id != CurrentRecipe.Id)
            return recipesToExecute[0];
                else
            return CurrentRecipe;;
    }
}
    