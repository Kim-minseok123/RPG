using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MultiplayersBuildAndRun
{
	[MenuItem("Tools/Run Multiplayer/2 Players")]
	static void PerformWin64Build2()
	{
		PerformWin64Build(2);
	}

	[MenuItem("Tools/Run Multiplayer/3 Players")]
	static void PerformWin64Build3()
	{
		PerformWin64Build(3);
	}

	[MenuItem("Tools/Run Multiplayer/4 Players")]
	static void PerformWin64Build4()
	{
		PerformWin64Build(4);
	}
    [MenuItem("Tools/BuildDedicatedServer")]
    static void BuildDedicatedServer()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

        buildPlayerOptions.target = BuildTarget.StandaloneWindows;
        buildPlayerOptions.scenes = GetScenePaths(); 
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
        buildPlayerOptions.options = BuildOptions.None;
        buildPlayerOptions.locationPathName = "Builds/Win64/DedicatedServer/DedicatedServer.exe";
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Server built successfully.");
        }
        else
        {
            Debug.LogError("Server build failed.");
        }
    }
    static void PerformWin64Build(int playerCount)
	{
        EditorUserBuildSettings.SwitchActiveBuildTarget(
        BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.scenes = GetScenePaths();
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Player;  // 클라이언트용 빌드 서브타겟 설정
        buildPlayerOptions.options = BuildOptions.None;

        for (int i = 1; i <= playerCount; i++)
        {
            buildPlayerOptions.locationPathName = "Builds/Win64/" + GetProjectName() + i.ToString() + "/" + GetProjectName() + i.ToString() + ".exe";
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Client " + i + " built successfully.");
            }
            else
            {
                Debug.LogError("Client " + i + " build failed.");
            }
        }
    }

	static string GetProjectName()
	{
		string[] s = Application.dataPath.Split('/');
		return s[s.Length - 2];
	}

	static string[] GetScenePaths()
	{
		string[] scenes = new string[EditorBuildSettings.scenes.Length];

		for (int i = 0; i < scenes.Length; i++)
		{
			scenes[i] = EditorBuildSettings.scenes[i].path;
		}

		return scenes;
	}
}
