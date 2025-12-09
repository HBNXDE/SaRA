#define TRACE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Online.CSE.HRC.Analysis.Analyzers.Common;
using Microsoft.Sara.AppPlugins.Common;
using Microsoft.Sara.AppPlugins.Office;
using Microsoft.Sara.AppPlugins.Outlook;
using Microsoft.Sara.Framework.Common;
using Microsoft.Sara.Framework.Common.Context;
using Microsoft.Sara.Framework.Common.CredentialManager;
using Microsoft.Sara.Framework.Common.Persistence;
using Microsoft.Sara.Framework.Common.Telemetry;
using Microsoft.Sara.Framework.Common.UICore.BuiltIns;
using Microsoft.Sara.Framework.Common.UICore.BuiltIns.Helpers;
using Microsoft.Sara.Framework.Common.Util;
using Microsoft.Sara.Framework.TaskEngine.Contract;
using Microsoft.Sara.Framework.TaskEngine.Engine;
using Microsoft.Sara.Models;
using Microsoft.Support.PortableDiagnostics.Infrastructure.Services;
using Microsoft.Support.PortableDiagnostics.OfficeActivationInteractive;

namespace Microsoft.SaraStandalone.Main;

public class Program
{
	private const int GenericPassedExitCode = 0;

	private const int GenericErrorExitCode = 1;

	private const int ExpiredSaraCommandLineExitCode = 11;

	private const int UnknownScenarioErrorCode = -777;

	private const int WaitTimeBeforeExit = 10000;

	private const string WorkflowName = "Microsoft.Sara.Plugins.Common.AnalyzerWorkflow";

	private static Assembly currentAssembly;

	public static int Main(string[] args)
	{
		if (TestHook.GetValue("SupportAttachProcess") == "true")
		{
			LogToConsole.WriteLineInDeveloperMode(633433, "Press any key to start the scenario.\r\n\r\n");
			Console.ReadKey();
		}
		string value = TestHook.GetValue("HeadlessClientArguments");
		if (!string.IsNullOrWhiteSpace(value))
		{
			args = value.Split(' ').ToArray();
		}
		TestHook.GetValue<bool>("DeveloperMode", out var returnValue);
		if (returnValue)
		{
			LogToConsole.WriteLineInDeveloperMode(650644, "DeveloperMode Enabled.\r\n\r\n");
		}
		int num = 1;
		try
		{
			string text = "prod";
			string value2 = TestHook.GetValue("SaraFlight");
			AppSetting appSetting;
			if (!string.IsNullOrWhiteSpace(value2))
			{
				text = value2;
			}
			else if (ConfigurationManager.SaraConfig != null && ConfigurationManager.TryGetAppSettingFromSaraConfig("UserGroup", out appSetting) && appSetting != null)
			{
				text = appSetting.Value;
			}
			Session commandLineSession = SessionHelper.GetCommandLineSession();
			InitializeTelemetryUploader();
			DisplayIntroMessage(args);
			if (returnValue)
			{
				LogToConsole.WriteLine(633436, "Creating a new session with SessionId: [" + commandLineSession.ClientSessionId + "] for [" + text + "].", isError: false);
			}
			else
			{
				LogToConsole.WriteLine(633436, "Creating a new session with SessionId: [" + commandLineSession.ClientSessionId + "] for [" + text + "].", isError: false, isLogToTelemetryOnly: true);
			}
			DateTime dateTime = new FileInfo(currentAssembly.Location).CreationTimeUtc.Date.AddMonths(120); //Time ;(
			TestHook.GetValue<bool>("ExpireSaraCommandLine", out var returnValue2);
			UserArgumentsManager userArgumentsManager = new UserArgumentsManager(currentAssembly);
			UserArgumentsResult userArgumentsResult = userArgumentsManager.HandleUserArguments(args);
			if ((string.IsNullOrEmpty(userArgumentsResult.ClientName) || !(userArgumentsResult.ClientName.ToLower() == "gethelp")) && (dateTime <= DateTime.UtcNow.ToLocalTime() || returnValue2))
			{
				LogToConsole.WriteLine(633437, strings.ResourceManager.GetString("ExpiredMessage"), isError: true);
				num = 11;
			}
			else
			{
				BootManager.Initialize(args);
				commandLineSession = userArgumentsManager.GetCommandLineSession(userArgumentsResult);
				if (userArgumentsResult.UserArgumentsType == UserArgumentsType.DisplayScenarioToExecute)
				{
					if (string.IsNullOrWhiteSpace(userArgumentsResult.ScenarioToExecute))
					{
						throw new InvalidOperationException("ScenarioToExecute is null or empty while trying to DisplayScenarioToExecute.");
					}
					RemoteTrace.Source.TraceData(TraceEventType.Information, 633440, new Session(commandLineSession));
					DisplayScenarioProgress displayScenarioProgress = new DisplayScenarioProgress(userArgumentsResult.ScenarioToExecute, userArgumentsResult.ShowProgress);
					displayScenarioProgress.StartProgressDisplay();
					num = (IsLegacyScenario(userArgumentsResult.ScenarioToExecute) ? RunLegacyScenario(userArgumentsResult.ScenarioToExecute, userArgumentsResult.UserLogFolderPath, userArgumentsResult.OfficeVersion, userArgumentsResult.OfflineScan, IsFullScanScenario(userArgumentsResult.ScenarioToExecute), userArgumentsResult.Platform, userArgumentsResult.Language, userArgumentsResult.NewProfileName, userArgumentsResult.SkipCreateProfile, userArgumentsResult.ClientName) : ((!IsInteractiveScenario(userArgumentsResult.ScenarioToExecute)) ? RunScenario(returnValue, text, commandLineSession.ClientSessionId, userArgumentsResult.ScenarioToExecute, userArgumentsResult.CustomArgs) : RunInteractiveScenario(returnValue, text, commandLineSession.ClientSessionId)));
					displayScenarioProgress.StopProgressDisplay();
				}
				else if (userArgumentsResult.IsPassed)
				{
					num = 0;
				}
			}
		}
		catch (Exception ex)
		{
			LogToConsole.WriteLine(633443, strings.ResourceManager.GetString("UnexpectedError"), isError: true);
			if (returnValue)
			{
				LogToConsole.WriteLine(633444, $"Unexpected Error: [{ex}]", isError: true);
			}
			else
			{
				LogToConsole.WriteLine(633444, "Unexpected Error: [" + Pii.ScrubExceptions(ex) + "]", isError: true, isLogToTelemetryOnly: true);
			}
		}
		LogToConsole.WriteLine(633442, $"\r\nScenario finished with exit code: [{num}].", ConsoleColor.Blue);
		LogToConsole.WriteLine(624345, "\r\nSaRA Command Line is closing, Please wait...", ConsoleColor.Magenta);
		Thread.Sleep(10000);
		RemoteTrace.Source.Flush();
		Trace.Flush();
		if (returnValue)
		{
			LogToConsole.WriteInDeveloperMode(650470, strings.ResourceManager.GetString("PressAnykeyToContinue"));
			Console.ReadKey();
		}
		return num;
	}

	private static void InitializeTelemetryUploader()
	{
		string name = "https://logging.diagnostics.office.com/log/SaraClient";
		if (ConfigurationManager.TryGetAppSetting("RemoteTelemetryUrl", out var appSetting))
		{
			name = appSetting.Value;
		}
		RemoteTrace.Source.Listeners.Add(new RemoteTraceListener(name));
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string text4 = "prod";
		if (ConfigurationManager.SaraConfig != null && ConfigurationManager.TryGetAppSettingFromSaraConfig("UserGroup", out var appSetting2) && appSetting2 != null)
		{
			text4 = appSetting2.Value;
		}
		if (text4.IndexOf("prod", StringComparison.OrdinalIgnoreCase) != -1)
		{
			if (ConfigurationManager.TryGetAppSetting("SaraServiceAPIMUrlPROD", out var appSetting3))
			{
				text = appSetting3.Value;
			}
			if (ConfigurationManager.TryGetAppSetting("SaraServiceAPIMSubsKeyPROD", out var appSetting4))
			{
				text3 = appSetting4.Value;
			}
			if (ConfigurationManager.TryGetAppSetting("SaraClientConfigApplicationIdPROD", out var appSetting5))
			{
				text2 = appSetting5.Value;
			}
		}
		else
		{
			if (ConfigurationManager.TryGetAppSetting("SaraServiceAPIMUrlPPE", out var appSetting6))
			{
				text = appSetting6.Value;
			}
			if (ConfigurationManager.TryGetAppSetting("SaraServiceAPIMSubsKeyPPE", out var appSetting7))
			{
				text3 = appSetting7.Value;
			}
			if (ConfigurationManager.TryGetAppSetting("SaraClientConfigApplicationIdPPE", out var appSetting8))
			{
				text2 = appSetting8.Value;
			}
		}
		if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(text2) || string.IsNullOrWhiteSpace(text3))
		{
			throw new InvalidConfigurationException("Error getting Sara service settings for log and metrics from configuration");
		}
		RemoteTrace.Source.Listeners.Add(new RemoteAppInsightsTraceListener(text, text3, text2));
		Log.Info("633636", "Initialize the FileLogging...");
	}

	private static void DisplayIntroMessage(string[] args)
	{
		currentAssembly = typeof(Program).Assembly;
		string fileVersion = FileVersionInfo.GetVersionInfo(currentAssembly.Location).FileVersion;
		string message = strings.ResourceManager.GetString("StartingCommandLineMessage") + " (" + fileVersion + ").\r\n";
		LogToConsole.WriteLine(633434, message, isError: false);
		string text = string.Join(" ", args);
		LogToConsole.WriteLine(633435, "Executing SaRACmd.exe " + text, isError: false, isLogToTelemetryOnly: true);
	}

	private static bool IsLegacyScenario(string scenarioName)
	{
		string[] source = new string[12]
		{
			"expertexperienceadmintask", "officescrubscenario", "officeroiscannormalscenario", "officeroiscanfullscenario", "officesigninscenario", "outlookconnectivityscenario", "outlookauthcheckscenario", "officesetupscenario", "outlookemailsetupscenario", "teamspresencescenario",
			"outlookhangcrashscenario", "officeactivationscenario"
		};
		scenarioName = scenarioName.ToLowerInvariant();
		return source.Contains(scenarioName);
	}

	private static bool IsAuthScenario(string scenarioName)
	{
		string[] source = new string[7] { "officesigninscenario", "outlookconnectivityscenario", "outlookauthcheckscenario", "officesetupscenario", "outlookemailsetupscenario", "outlookhangcrashscenario", "officeactivationscenario" };
		scenarioName = scenarioName.ToLowerInvariant();
		return source.Contains(scenarioName);
	}

	private static bool RequiresConsentScenario(string scenarioName)
	{
		return new string[2] { "outlookauthcheckscenario", "outlookemailsetupscenario" }.Contains(scenarioName.ToLowerInvariant());
	}

	private static bool IsFullScanScenario(string scenarioName)
	{
		return scenarioName.ToLowerInvariant() == "officeroiscanfullscenario".ToLowerInvariant();
	}

	private static bool IsInteractiveScenario(string scenarioName)
	{
		return scenarioName.ToUpperInvariant() == "OfficeActivationInteractiveScenario".ToUpperInvariant();
	}

	private static int RunLegacyScenario(string scenarioName, string userFolderPath = null, string officeversion = null, bool isOfflineScan = false, bool isFullScan = false, string platform = "64", string language = "English", string newProfileName = null, bool skipCreateProfile = false, string clientName = "")
	{
		scenarioName = scenarioName.ToLowerInvariant();
		Microsoft.Sara.Framework.Common.Symptom selectedSymptom = ConfigurationManager.SaraConfig.Symptoms.Single((Microsoft.Sara.Framework.Common.Symptom x) => x.HeadlessCommandName?.ToLowerInvariant() == scenarioName);
		List<ApplicationDescription> list = selectedSymptom.Products.Select((PlatformProduct platformProduct) => new ApplicationDescription(platformProduct.Product, platformProduct.Platform, selectedSymptom.Name, selectedSymptom.Description, selectedSymptom.AdminTask, platformProduct.SupportChannel)).ToList();
		BuiltInContext orCreateContext = ContextManager.Instance.GetOrCreateContext<BuiltInContext>();
		orCreateContext.IsCommandLineMode.Value = true;
		orCreateContext.SelectedSymptom.Value = selectedSymptom;
		orCreateContext.SelectedApplication.Value = ((list.Count > 1) ? list.SingleOrDefault((ApplicationDescription x) => x.PlatformEnum == Platform.WindowsDesktop) : list.First());
		orCreateContext.UserLogFolder.Value = userFolderPath;
		orCreateContext.OfficeVersion.Value = officeversion;
		orCreateContext.IsOfflineScan.Value = isOfflineScan;
		orCreateContext.SkipCreateProfile.Value = skipCreateProfile;
		OfficeSetupDiagRecoveryContext orCreateContext2 = ContextManager.Instance.GetOrCreateContext<OfficeSetupDiagRecoveryContext>();
		orCreateContext2.FullRoiScan.Value = isFullScan;
		orCreateContext2.RecoveryApplication.Value = Office365RecoverProducts.Office;
		DiagSharedContext orCreateContext3 = ContextManager.Instance.GetOrCreateContext<DiagSharedContext>();
		if (RequiresConsentScenario(scenarioName) && orCreateContext3 != null)
		{
			orCreateContext3.TargetPage.Value = TargetPages.EnterProfile;
			orCreateContext3.UserSelectedYes.Value = true;
			orCreateContext3.UserResponse.Value = true;
			orCreateContext3.SwitchPage.Value = false;
			orCreateContext3.TaskProgressList.Value = new ObservableCollection<GroupTaskResult>(new List<GroupTaskResult>());
		}
		orCreateContext3.Language.Value = language;
		orCreateContext3.Platform.Value = platform;
		if (!string.IsNullOrEmpty(newProfileName))
		{
			ContextManager.Instance.GetOrCreateContext<OlkDiagProfilesContext>().NewProfileName.Value = newProfileName;
		}
		WorkflowManager.Instance.OnAppLaunch();
		IAnalyzerWorkflow orCreateWorkflow = WorkflowManager.Instance.GetOrCreateWorkflow<IAnalyzerWorkflow>("Microsoft.Sara.Plugins.Common.AnalyzerWorkflow", "Microsoft.Sara.Plugins.Common.AnalyzerWorkflow");
		orCreateWorkflow.AnalyzerName.Value = selectedSymptom.HeadlessCommandScenario;
		if (IsAuthScenario(scenarioName))
		{
			if (!string.IsNullOrEmpty(clientName) && clientName.ToLower() == "gethelp")
			{
				Process process = Process.GetProcessesByName("GetHelp").FirstOrDefault();
				if (process != null)
				{
					AuthManager.Instance.MainWindow = process.MainWindowHandle;
				}
			}
			AuthenticationHelper.VerifyCredential();
		}
		if (!orCreateWorkflow.TryExecute() && orCreateContext.CommandLineExitCode.Value == 0)
		{
			throw new InvalidOperationException("Unexpected error while executing [" + scenarioName + "] scenario.");
		}
		return orCreateContext.CommandLineExitCode.Value;
	}

	private static int RunScenario(bool isInDevMode, string runEnvironment, string clientSessionId, string scenarioName, Dictionary<string, object> args)
	{
		int num = new PortableDiagnosticsUnattended(runEnvironment, clientSessionId, isInDevMode).ExecutePortableDiagnostic(scenarioName, args);
		if (num == -777)
		{
			LogToConsole.WriteLine(633441, strings.ResourceManager.GetString("UnknownScenarioName"), isError: true);
			return 1;
		}
		return num;
	}

	private static int RunInteractiveScenario(bool isInDevMode, string runEnvironment, string clientSessionId)
	{
		new OfficeActivationInteractiveDiagnostic(runEnvironment, clientSessionId, isInDevMode).ExecuteInteractiveDiagnostic();
		return 0;
	}
}
