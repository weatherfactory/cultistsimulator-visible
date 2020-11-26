using UnityEditor;

namespace Assets.Core.Utility
{
    public class CopySteamLibraries
    {
        public static void Copy(BuildTarget target, string pathToBuiltProject)
        {
            if (!target.ToString().StartsWith("Standalone"))
                return;

            FileUtil.ReplaceFile("steam_appid.txt", pathToBuiltProject + "/steam_appid.txt");
        }

    }
}