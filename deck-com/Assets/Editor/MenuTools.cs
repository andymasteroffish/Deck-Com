using UnityEngine;
using UnityEditor;
using System.Xml;
using System;
using System.IO;
using System.Diagnostics;

public class MenuTools {

	[MenuItem("DeckCom/Reset Player Folder")]
	//[MenuItem("Andy's l33t hakz/BLAST THOSE FIL3z")]
	private static void resetPlayerFolder(){
		UnityEngine.Debug.Log ("replacing the current player folder with the default player files");
		string defaultFolder =  Application.dataPath + "/external_data/player_default";
		string folderToReplace = Application.dataPath + "/external_data/player";

		//Debug.Log ("kill " + folderToReplace);
		//delete it
		if (System.IO.Directory.Exists (folderToReplace)) {
			UnityEngine.Debug.Log ("deleting existing player folder");
			System.IO.Directory.Delete (folderToReplace, true);
		} else {
			UnityEngine.Debug.Log ("player folder already gone");
		}

		//make a new one
		System.IO.Directory.CreateDirectory (folderToReplace);

		//copy that shit
		UnityEngine.Debug.Log("copying the contents of "+defaultFolder);
		Copy(defaultFolder, folderToReplace);
	}

	//from https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
	public static void Copy(string sourceDirectory, string targetDirectory)
	{
		DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
		DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

		CopyAll(diSource, diTarget);
	}

	public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
	{
		Directory.CreateDirectory(target.FullName);

		// Copy each file into the new directory.
		foreach (FileInfo fi in source.GetFiles())
		{
			Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
			fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
		}

		// Copy each subdirectory using recursion.
		foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
		{
			DirectoryInfo nextTargetSubDir =
				target.CreateSubdirectory(diSourceSubDir.Name);
			CopyAll(diSourceSubDir, nextTargetSubDir);
		}
	}





}
