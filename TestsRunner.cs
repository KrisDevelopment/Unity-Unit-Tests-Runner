// This is a modified version of the TestRunner.cs file from the KrisDevelopment Asset Store package.

#if UNITY_EDITOR
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestRunner;


[assembly: TestRunCallback(typeof(TesterCallbacks))]
class TesterCallbacks : ITestRunCallback
{
	public void RunStarted(ITest testsToRun)
	{
		Debug.Log($"[TestRunner] RunStarted: {testsToRun.FullName}");
	}

	public void RunFinished(ITestResult result)
	{
		Debug.Log($"[TestRunner] Duration:\n{result.Duration}");
		Debug.Log($"[TestRunner] Message:\n{result.Message}");
		Debug.Log($"[TestRunner] Results output:\n{result.Output}");

		var _failed = new List<ITestResult>();
		var _passed = new List<ITestResult>();
		TestsRunner.CollectFlatResults(result, _failed, _passed);

		if (_passed.Count > 0)
			Debug.Log($"[TestRunner] Passed-OK:\n{string.Join(",\n--\n", _passed.Select(a => $"{a.Test.FullName}; Message: {a.Message}"))}\n----[End of OK]----\n");

		if (_failed.Count > 0)
			Debug.Log($"[TestRunner] Failed:\n{string.Join(",\n--\n", _failed.Select(a => $"{a.Test.FullName}; Message: {a.Message}"))}\n----[End of Filed]----\n");

		if (result.ResultState.Status != NUnit.Framework.Interfaces.TestStatus.Passed)
		{
			Debug.LogError($"[TestRunner] Tests failed with status {result.ResultState.Status} on site {result.ResultState.Site}");
			EditorApplication.Exit(1);
		}
		else
		{
			Debug.Log("[TestRunner] Tests passed :)");
			EditorApplication.Exit(0);
		}
	}

	public void TestStarted(ITest test)
	{

	}

	public void TestFinished(ITestResult result)
	{
		if (result.ResultState.Status == NUnit.Framework.Interfaces.TestStatus.Passed)
		{
			Debug.Log($"[Test Pass] {result.Name}");
		}
		else if (result.ResultState.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
		{
			Debug.LogError($"[Test Fail] {result.Name} on site {result.ResultState.Site}");
		}
		else
		{
			Debug.Log($"[TestRunner] TestFinished: {result.Name} with status {result.ResultState.Status} on site {result.ResultState.Site}");
		}
	}
}


/// <summary>
/// Not the TestsRunner instead of TestRunner. Avoids conflicts with the Unity TestRunner.
/// </summary>
public static class TestsRunner
{
	private static TestRunnerApi runner = null;


	public static void CollectFlatResults(ITestResult node, List<ITestResult> outFailResults, List<ITestResult> outPassResults)
	{
		if (node.ResultState.Status == NUnit.Framework.Interfaces.TestStatus.Passed)
		{
			outPassResults.Add(node);
		}
		else if (node.ResultState.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
		{
			outFailResults.Add(node);
		}

		foreach (var child in node.Children)
		{
			CollectFlatResults(child, outFailResults, outPassResults);
		}
	}

	[MenuItem("Tools/Kris Development/Tests/Run Editor Unit Tests")]
	public static void RunEditorUnitTests()
	{
		Filter filter = new Filter()
		{
			testMode = TestMode.EditMode
		};
		RunTests(filter);
	}

	[MenuItem("Tools/Kris Development/Tests/Run All Unit Tests")]
	public static void RunAllUnitTests()
	{
		Filter filter = new Filter()
		{
			testMode = TestMode.EditMode | TestMode.PlayMode,

		};
		RunTests(filter);
	}

	private static void RunTests(Filter filter)
	{
		Debug.Assert(!System.Environment.GetCommandLineArgs().Contains("-quit"),
			"Use of -quit argument is not tolerated during Unit testing step. The tests will close the editor instance when complete.");

		Debug.Log($"Running with Script Defines: {PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone)}");
		Debug.Log("[TestRunner] Tests starting");

		runner = ScriptableObject.CreateInstance<TestRunnerApi>();
		runner.Execute(filter != null ? new ExecutionSettings(filter) : new ExecutionSettings());
	}
}

#endif