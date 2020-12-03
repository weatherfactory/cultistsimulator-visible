
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;

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
        GreedySlot,
        Retire,
        Debug,
        ChangeTo,
		DoubleClickSend,
        Purge,
        Split,
        Merge,
        TravelArrived,
        TravelFailed,
        UI
    }

    public ActionSource actionSource;

    public TokenLocation TokenLocation { get; set; }


    public Source StackSource { get; set; }

    public Context(ActionSource actionSource) {
        this.actionSource = actionSource;
    }

    public Context(ActionSource actionSource,TokenLocation tokenLocation)
    {
        this.actionSource = actionSource;
        TokenLocation = tokenLocation;
    }



    
    public bool IsManualAction() {
        switch (actionSource) {
            case ActionSource.PlayerDrag:
            case ActionSource.PlayerClick:
            case ActionSource.PlayerDumpAll:
                return true;

            default:
                return false;
        }
    }

}
