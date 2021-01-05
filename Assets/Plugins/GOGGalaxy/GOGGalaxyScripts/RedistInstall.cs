#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;
using System;

namespace Galaxy {

	// This copys various files into their required locations when Unity is launched to make installation a breeze.
	// Note: Implemented as a direct mirror of the dll copy process implemeneted in the Steamworks.net library (https://github.com/rlabrecque/Steamworks.NET).
	[InitializeOnLoad]
	public class RedistInstall {
		const string galaxyAPIRelativeLoc = "Assets/Plugins/GoGGalaxy";

		static RedistInstall() {
			#if UNITY_EDITOR_WIN
				#if UNITY_EDITOR_64
					CopyFile ("Win64/Galaxy64.dll", "Galaxy64.dll", true);
					CopyFile ("Win64/GalaxyCSharpGlue.dll", "GalaxyCSharpGlue.dll", true);
				#else
					CopyFile ("Win32/Galaxy.dll", "Galaxy.dll", true);
					CopyFile ("Win32/GalaxyCSharpGlue.dll", "GalaxyCSharpGlue.dll", true);
				#endif
			#elif UNITY_EDITOR_OSX
					CopyFile ("OSXUniversal/Galaxy.bundle/Contents/MacOS/libGalaxy.dylib", "libGalaxy.dylib", true);
					CopyFile ("OSXUniversal/Galaxy.bundle/Contents/MacOS/libGalaxyCSharpGlue.dylib", "libGalaxyCSharpGlue.dylib", true);
			#endif
		}

		static void CopyFile(string sourceFilename, string targetFilename, bool bCheckDifference) {
			string strCWD = Directory.GetCurrentDirectory();
			string strSource = Path.Combine(Path.Combine(strCWD, galaxyAPIRelativeLoc), sourceFilename);
			string strDest = Path.Combine(strCWD, targetFilename);

			if (!File.Exists(strSource)) {
				Debug.LogWarning(string.Format("[GoGGalaxy RedistInstall] Could not copy {0} into the project root. {0} could not be found in '{1}'. Place {0} from the GoG Galaxy SDK in the project root manually.", sourceFilename, Path.Combine(strCWD, galaxyAPIRelativeLoc)));
				return;
			}

			if (File.Exists(strDest)) {
				if (!bCheckDifference)
					return;

				if (FilesAreEqual(strSource, strDest)) {
					return;
				}

				Debug.Log(string.Format("[GoGGalaxy RedistInstall] {0} in the project root differs from the GoG Galaxy redistributable. Updating.... Please relaunch Unity.", sourceFilename));
			}
			else {
				Debug.Log(string.Format("[GoGGalaxy RedistInstall] {0} is not present in the project root. Copying...", sourceFilename));
			}

			File.Copy(strSource, strDest, true);
			File.SetAttributes(strDest, File.GetAttributes(strDest) & ~FileAttributes.ReadOnly);

			if (File.Exists(strDest)) {
				Debug.Log(string.Format("[GoGGalaxy RedistInstall] Successfully copied {0} into the project root. Please relaunch Unity.", sourceFilename));
			}
			else {
				Debug.LogWarning(string.Format("[GoGGalaxy RedistInstall] Could not copy {0} into the project root. File.Copy() Failed. Place {0} from the GoG Galaxy SDK in the project root manually.", sourceFilename));
			}
		}

		private static bool FilesAreEqual (string filename1, string filename2) {
			FileInfo file1Info = new FileInfo (filename1);
			FileInfo file2Info = new FileInfo (filename2);
			return FilesAreEqual (file1Info, file2Info);
		}

		private const int kNumBytesPerChunk = sizeof(Int64);
		private static bool FilesAreEqual(FileInfo first, FileInfo second)
		{
			if (first.Length != second.Length)
				return false;

			int iterations = (int)Math.Ceiling((double)first.Length / kNumBytesPerChunk);

			using (FileStream fs1 = first.OpenRead())
				using (FileStream fs2 = second.OpenRead())
			{
				byte[] one = new byte[kNumBytesPerChunk];
				byte[] two = new byte[kNumBytesPerChunk];

				for (int i = 0; i < iterations; i++)
				{
					fs1.Read(one, 0, kNumBytesPerChunk);
					fs2.Read(two, 0, kNumBytesPerChunk);

					if (BitConverter.ToInt64(one,0) != BitConverter.ToInt64(two,0))
						return false;
				}
			}

			return true;
		}

		private static int CalculateFileDataHash (string filename) {
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(filename))
				{
					return md5.ComputeHash(stream).GetHashCode ();
				}
			}
		}
	}
}

#endif
