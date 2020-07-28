using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ContentFileLoader
{
    public readonly string ContentFolder;
    private List<string> _contentFilePaths=new List<string>();
    private List<LoadedContentFile> _loadedContentFiles=new List<LoadedContentFile>();


    private List<string> GetContentFilesRecursive(string path)
    {
        List<string> contentFilePaths = new List<string>();
        //find all the content files
        if (Directory.Exists(path))
        {
            contentFilePaths.AddRange(Directory.GetFiles(path).ToList().FindAll(f => f.EndsWith(".json")));
            foreach (var subdirectory in Directory.GetDirectories(path))
                contentFilePaths.AddRange(GetContentFilesRecursive(subdirectory));
        }
        return contentFilePaths;
    }


    public IEnumerable<LoadedContentFile> GetLoadedContentFilesContainingEntityTag(string entityTag)
    {
        IEnumerable<LoadedContentFile> matchingFiles = _loadedContentFiles.Where(lcf => lcf.EntityTag == entityTag.ToLower());

        return matchingFiles;
    }

    public ContentFileLoader(string contentFolder)
    {
        ContentFolder = contentFolder;
    }

    public void LoadContentFiles()
    {
        //find all the content files
        _contentFilePaths = GetContentFilesRecursive(ContentFolder);

        if (_contentFilePaths.Any())
            _contentFilePaths.Sort();

        foreach (var contentFilePath in _contentFilePaths)
        {
            using (StreamReader file = File.OpenText(contentFilePath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {

                var topLevelObject = (JObject)JToken.ReadFrom(reader);
                var containerProperty =
                    topLevelObject.Properties().First(); //there should be exactly one property, which contains all the relevant entities

                LoadedContentFile loadedFile=new LoadedContentFile(contentFilePath,containerProperty,containerProperty.Name);

                _loadedContentFiles.Add(loadedFile);

            }

        }
    }


    public List<string> ContentFilePaths()
    {
        return new List<string>(_contentFilePaths);
    }

}