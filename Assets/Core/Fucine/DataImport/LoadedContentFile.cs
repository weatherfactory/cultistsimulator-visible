using Newtonsoft.Json.Linq;

public class LoadedContentFile
{
    private readonly string _entityTag;

    /// <summary>
    /// Path of the original file
    /// </summary>
    public string Path { get; private set; }
    /// <summary>
    /// JSON.NET object containing json for entities of the tagged type
    /// </summary>
    public JProperty EntityContainer { get; private set; }

    /// <summary>
    /// eg recipes, elements...
    /// </summary>
    public string EntityTag => _entityTag.ToLower();

    public LoadedContentFile(string path, JProperty entityContainer, string entityTag)
    {
        Path = path;
        EntityContainer = entityContainer;
        _entityTag = entityTag;
    }
}