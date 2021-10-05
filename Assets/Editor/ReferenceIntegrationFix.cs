using UnityEditor;
public class FIX : AssetPostprocessor
{
    private static string OnGeneratedCSProject(string path, string content)
    {
        return content.Replace("<ReferenceOutputAssembly>false</ReferenceOutputAssembly>", "<ReferenceOutputAssembly>true</ReferenceOutputAssembly>");
    }
}
