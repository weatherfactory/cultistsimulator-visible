
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
        FlushingTokens, //Tokens are being flushed from one sphere (or >1 spheres) to another sphere, via a situation command
        SituationEffect,
        GreedyGrab,
        Retire,
        Shroud,
        Unshroud,
        Debug,
        ChangeTo,
		DoubleClickSend,
        Purge,
        Merge,
        SituationCreated,
        SituationReset,
        TravelFailed,
        UI,
        JustSpawned,
        PushedAside,
        ContainingSphereRetired,
        SphereReferenceLocationChanged
    }

    public ActionSource actionSource;
    public TokenLocation TokenDestination { get; set; }
    public FucinePath OccurringAt { get; set; }
    public Context(ActionSource actionSource) {
        this.actionSource = actionSource;
    }

    public Context(Context fromContext):this(fromContext.actionSource,fromContext.TokenDestination)
    {
        
    }

    public Context(ActionSource actionSource,TokenLocation tokenDestination)
    {
        this.actionSource = actionSource;
        TokenDestination = tokenDestination;
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
