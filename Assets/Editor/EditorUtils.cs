using System.Reflection;
using UnityEditor;

namespace Assets.Editor
{
    public class EditorUtils
    {
        public static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            if (method != null)
                method.Invoke(new object(), null);
        }
    }
}
