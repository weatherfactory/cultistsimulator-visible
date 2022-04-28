// Comment this out to disable copying
//#define DISABLEREDISTCOPY

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Galaxy 
{

	// Copy GOG dlls to the appropriate place when building standalone players.
	// Note: Implemented as a direct mirror of the dll copy process implemented in the Steamworks.net library (https://github.com/rlabrecque/Steamworks.NET).
	public class CopyGalaxyLibraries {
		private const string GALAXY_API_RELATIVE_LOC = "Assets/Plugins/GoGGalaxy";

		public static void Copy(BuildTarget target, string pathToBuiltProject) 
		{
			#if !DISABLEREDISTCOPY
			if (target == BuildTarget.StandaloneWindows64) {
				Debug.Log("BUILD: copying Galaxy libraries 64 to: " + pathToBuiltProject);
				CopyFile("Win64/Galaxy64.dll", Path.Combine(pathToBuiltProject, "Galaxy64.dll"));
				CopyFile("Win64/GalaxyCSharpGlue.dll", Path.Combine(pathToBuiltProject, "GalaxyCSharpGlue.dll"));
			}
			else if (target == BuildTarget.StandaloneWindows) {
				Debug.Log("BUILD: copying Galaxy libraries 32 to: " + pathToBuiltProject);
				CopyFile("Win32/Galaxy.dll", Path.Combine(pathToBuiltProject, "Galaxy.dll"));
				CopyFile("Win32/GalaxyCSharpGlue.dll", Path.Combine(pathToBuiltProject, "GalaxyCSharpGlue.dll"));
			}
			else if (target == BuildTarget.StandaloneOSX)
			{
				//copy code as provided seems to be bobbins - it uses a source path that doesn't match the Unity bundle contents, and it assumes libgalaxy not libgalaxy64, although libgalaxy no longer appears to exist
            pathToBuiltProject = pathToBuiltProject + "/OSX.app/"; // Ensure the .app directory name is actually treated as a directory.
            string outputLibLocation = Path.Combine (pathToBuiltProject, "Contents/Frameworks/MonoEmbedRuntime/osx/");
         //   CopyFile ("OSXUniversal/libGalaxy.dylib", Path.Combine (outputLibLocation, "libGalaxy.dylib"));
            CopyFile("OSXUniversal/libGalaxy64.dylib", Path.Combine(outputLibLocation, "libGalaxy64.dylib"));
            CopyFile("OSXUniversal/libGalaxyCSharpGlue.dylib", Path.Combine (outputLibLocation, "libGalaxyCSharpGlue.dylib"));
			}
#endif
		}

		private static void CopyFile(string sourceFilePath, string outputFilePath) 
		{
			string strCWD = Directory.GetCurrentDirectory();
			string strSource = Path.Combine(Path.Combine(strCWD, GALAXY_API_RELATIVE_LOC), sourceFilePath);
			string strFileDest = outputFilePath;

			if (!File.Exists(strSource))
            {
                string warning =
                    $"[GoGGalaxy RedistCopy] Could not copy {sourceFilePath} to {outputFilePath}. {sourceFilePath} could not be found in '{GALAXY_API_RELATIVE_LOC}'. Place {sourceFilePath} from the redist into the project root manually.";
				Debug.LogWarning(warning);
				return;
			}

			if (File.Exists(strFileDest)) {
				if (File.GetLastWriteTime(strSource) == File.GetLastWriteTime(strFileDest)) {
					FileInfo fInfo = new FileInfo(strSource);
					FileInfo fInfo2 = new FileInfo(strFileDest);
					if (fInfo.Length == fInfo2.Length) {
						return;
					}
				}
			}

			(new FileInfo(strFileDest)).Directory.Create();

			File.Copy(strSource, strFileDest, true);
			File.SetAttributes(strFileDest, File.GetAttributes(strFileDest) & ~FileAttributes.ReadOnly);

			if (!File.Exists(strFileDest)) {
				Debug.LogWarning(string.Format("[GoGGalaxy RedistCopy] Could not copy {0} into the built project. File.Copy() Failed. Place {0} from the redist folder into the output dir manually.", sourceFilePath));
			}
		}
	}
}

#endif
