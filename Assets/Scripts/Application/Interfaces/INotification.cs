namespace SecretHistories.Fucine
{
    public interface INotification
    {
        string Title { get; }
        string Description { get; }
        bool Additive { get; }
        //0 is standard. negative is less emphatic, positive is more emphatic. 
        //initially used only for indicating hint (less emphatic) predictions, but could allow us
        //to layer priorities later.
        int EmphasisLevel { get; }
    }
}