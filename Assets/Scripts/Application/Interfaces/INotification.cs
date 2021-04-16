namespace SecretHistories.Fucine
{
    public interface INotification
    {
        string Title { get; }
        string Description { get; }
        bool Additive { get; }
    }
}