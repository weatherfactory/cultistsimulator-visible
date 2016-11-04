public interface IRecipeSituation
{
    string CurrentRecipeId { get; }

    ///this is the id of the *originating* recipe, although the recipe inside may change later.
    ///the original recipe may be important for ongoing situations
    string OriginalRecipeId { get; set; }

    float TimeRemaining { get; set; }
    RecipeTimerState TimerState { get; }
    int GetInternalElementQuantity(string forElementId);
    void DoHeartbeat();
    void Extinguish();
    void Subscribe(IRecipeSituationSubscriber subscriber);
    bool IsInteractive();

}