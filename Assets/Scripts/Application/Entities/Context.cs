
using SecretHistories.Entities;
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
        SituationStoreStacks,
        SituationEffect,
        SituationResults,
        GreedyGrab,
        Retire,
        Debug,
        ChangeTo,
		DoubleClickSend,
        Purge,
        Merge,
        TravelFailed,
        UI,
        SpawningAnchor,
        PushedAside,
        ContainingSphereRetired
    }

    public ActionSource actionSource;


    public TokenLocation TokenDestination { get; set; }

    public Source StackSource { get; set; }

    public Context(ActionSource actionSource) {
        this.actionSource = actionSource;
    }

    public Context(ActionSource actionSource,TokenLocation tokenDestination)
    {
        this.actionSource = actionSource;
        TokenDestination = tokenDestination;
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
