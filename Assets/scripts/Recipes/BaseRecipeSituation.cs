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
    protected Recipe currentRecipe;
    
    public string CurrentRecipeId
    {
        get
        {
            if (currentRecipe == null)
                return null;
            return (currentRecipe.Id);
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
            if(currentRecipe==null)
                return RecipeTimerState.Extinct;
            if (TimeRemaining > 0)
                return RecipeTimerState.Ongoing;
            else
                return RecipeTimerState.Complete;
        }
    }

    public virtual int GetInternalElementQuantity(string forElementId)
    {
        return 0;
    }

    public BaseRecipeSituation(Recipe recipe, float? timeremaining,IElementsContainer inputContainer,Compendium rc)
    {
        currentRecipe = recipe;
        OriginalRecipeId = currentRecipe.Id;
        _compendium = rc;

        if (timeremaining == null)
            TimeRemaining = currentRecipe.Warmup;
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
        SituationInfo info = GetCurrentSituationInfo();
        publishUpdate(info);
    }


    public void Extinguish()
    {
        currentRecipe = null;
    }

 
    public void Subscribe(IRecipeSituationSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
        subscriber.ReceiveSituationUpdate(GetCurrentSituationInfo());
    }

    protected virtual void publishUpdate(SituationInfo info)
    {
        foreach (var s in _subscribers)
            s.ReceiveSituationUpdate(info);
    }

    private SituationInfo GetCurrentSituationInfo()
    {
        SituationInfo info=new SituationInfo();

        info.Label = currentRecipe.Label;
        info.Warmup = currentRecipe.Warmup;
        info.TimeRemaining = TimeRemaining;
        info.State = TimerState;

        return info;
    }




    protected virtual void resetWithRecipe(Compendium compendium)
    {
        currentRecipe = compendium.GetRecipeById(currentRecipe.Loop);
        TimeRemaining = currentRecipe.Warmup;
        
    }


    protected void Complete()
    {
      
        currentRecipe=ProcessRecipe();

        SituationInfo info = new SituationInfo();




        if (currentRecipe.Loop != null)
        {
            resetWithRecipe(_compendium);
            info.Message = currentRecipe.Description;
        }
        else
        {
            info.Message = currentRecipe.Description;
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
            _compendium.GetActualRecipesToExecute(currentRecipe, CharacterContainer);

        foreach (Recipe executingRecipe in recipesToExecute)
        {
            executingRecipe.Do(CharacterContainer);
        }

        //if the first recipe in the list of alternatives is a different recipe, switch it.
        //This can be important if the alternative recipe has a different loop.


        if (recipesToExecute[0].Id != currentRecipe.Id)
            return recipesToExecute[0];
                else
            return currentRecipe;;
    }
}
    