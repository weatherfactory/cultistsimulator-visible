
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;

public class Context {
    public enum ActionSource {
        Unknown,
        Loading,
        PlayerDrag,
        PushToThresholddUsurpedThisStack,
        CalvedStack,
        PlayerClick,
        PlayerDumpAll,
TokenMigration,
        SituationEffect,
        GreedyGrab,
        Retire,
        Debug,
        ChangeTo,
		DoubleClickSend,
        Purge,
        Merge,
        SituationCreated,
        SituationReset,
        TravelFailed,
        UI,
        SpawningAnchor,
        PushedAside,
        ContainingSphereRetired
    }

    public ActionSource actionSource;
    public bool Metafictional { get; set; }
    public TokenLocation TokenDestination { get; set; }
    public FucinePath OccurringAt { get; set; }
    public Context(ActionSource actionSource) {
        this.actionSource = actionSource;
        Metafictional = false;
    }

    public Context(Context fromContext)
    {

    }

    public Context(ActionSource actionSource,TokenLocation tokenDestination)
    {
        this.actionSource = actionSource;
        TokenDestination = tokenDestination;
        Metafictional = false;
    }

    public static Context Unknown()
    {
        return new Context(ActionSource.Unknown);
    }

    
    public bool IsManualAction() {
        switch (actionSource) {
            case ActionSource.DoubleClickSend:
            case ActionSource.PlayerDrag:
            case ActionSource.PlayerClick:
            case ActionSource.PlayerDumpAll:
                return true;

            default:
                return false;
        }
    }

}
