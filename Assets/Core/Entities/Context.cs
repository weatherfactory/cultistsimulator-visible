
public class Context {
    public enum ActionSource {
        Unknown,
        Loading,
        PlayerDrag,
        PlayerClick,
        PlayerDumpAll,
        SituationStoreStacks,
        SituationEffect,
        SituationResults,
        GreedySlot,
        AnimEnd,
        Retire,
        Debug,
        ChangeTo,
		DoubleClickSend,
        Purge
    }

    public ActionSource actionSource;

    public Context(ActionSource actionSource) {
        this.actionSource = actionSource;
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
