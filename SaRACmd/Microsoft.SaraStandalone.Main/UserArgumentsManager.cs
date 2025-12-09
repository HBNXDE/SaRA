using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Sara.Framework.Common.Util;
using Microsoft.Sara.Models;

namespace Microsoft.SaraStandalone.Main;

public class UserArgumentsManager
{
	private const string SaraEULAFileName = "SaraEULA.txt";

	private readonly Assembly currentAssembly;

	public UserArgumentsManager(Assembly assembly)
	{
		currentAssembly = assembly;
	}

	public UserArgumentsResult HandleUserArguments(string[] args)
	{
		if (args == null || !args.Any())
		{
			DisplaySwitches(currentAssembly);
			return new UserArgumentsResult(isPassed: false, UserArgumentsType.DisplaySwitches);
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		bool flag = false;
		string text = null;
		string text2 = null;
		bool showProgress = true;
		string officeversion = null;
		bool offlineScan = false;
		bool isScriptInvocation = false;
		string text3 = "Sara";
		string text4 = "English";
		string text5 = "64";
		string newprofilename = null;
		bool skipCreateProfile = false;
		for (int i = 0; i < args.Length; i++)
		{
			if (string.IsNullOrWhiteSpace(args[i]))
			{
				continue;
			}
			if (IsCommandSwitch(args[i]))
			{
				string text6 = args[i].TrimStart('-', '/').ToLower();
				switch (text6)
				{
				case "?":
					DisplaySwitches(currentAssembly);
					return new UserArgumentsResult(isPassed: true, UserArgumentsType.DisplaySwitches);
				case "help":
					DisplayHelp();
					return new UserArgumentsResult(isPassed: true, UserArgumentsType.DisplayHelp);
				case "displayeula":
					DisplayEULA(currentAssembly);
					return new UserArgumentsResult(isPassed: true, UserArgumentsType.DisplayEULA);
				case "accepteula":
					flag = true;
					break;
				case "script":
					isScriptInvocation = true;
					break;
				case "s":
					text = ((args.Length > i + 1) ? args[i + 1] : null);
					if (string.IsNullOrWhiteSpace(text) || IsCommandSwitch(text))
					{
						DisplayScenarioMissing();
						return new UserArgumentsResult(isPassed: false, UserArgumentsType.DisplayScenarioMissing);
					}
					i++;
					break;
				case "logfolder":
				{
					string text10 = ((args.Length > i + 1) ? args[i + 1] : null);
					if (string.IsNullOrEmpty(text10) || IsCommandSwitch(text10))
					{
						DisplayUserFolderPathMissing(text10);
						return new UserArgumentsResult(isPassed: false, UserArgumentsType.DisplayUserFolderPathMissing);
					}
					if (!Directory.Exists(text10) || !ValidateUserFolderPathAccess(text10))
					{
						DisplayInvalidUserFolderPath(text10);
						return new UserArgumentsResult(isPassed: false, UserArgumentsType.DisplayInvalidUserFolderPath);
					}
					text2 = text10;
					dictionary.Add("LogFolder".ToUpperInvariant(), text2);
					i++;
					break;
				}
				case "hideprogress":
					showProgress = false;
					break;
				case "officeversion":
				{
					string text8 = ((args.Length > i + 1) ? args[i + 1] : null);
					if (string.IsNullOrEmpty(text8) || IsCommandSwitch(text8))
					{
						DisplayOfficeVersionMissing();
						return new UserArgumentsResult(isPassed: false, UserArgumentsType.DisplayOfficeVersionMissing);
					}
					officeversion = text8;
					officeversion = (officeversion.Equals("M365", StringComparison.InvariantCultureIgnoreCase) ? strings.ResourceManager.GetString("M365") : officeversion);
					i++;
					break;
				}
				case "offlinescan":
				{
					string text9 = ((args.Length > i + 1) ? args[i + 1] : null);
					if (!string.IsNullOrWhiteSpace(text9) && !IsCommandSwitch(text9))
					{
						DisplayInvalidOfflineScanParameter();
						return new UserArgumentsResult(isPassed: false, UserArgumentsType.InvalidOfflineScanParameter);
					}
					offlineScan = true;
					break;
				}
				case "client":
					text3 = ((args.Length > i + 1) ? args[i + 1] : text3);
					i++;
					break;
				case "language":
					text4 = ((args.Length > i + 1) ? args[i + 1] : text4);
					i++;
					break;
				case "platform":
					text5 = ((args.Length > i + 1) ? args[i + 1] : text5);
					i++;
					break;
				case "newprofilename":
					newprofilename = ((args.Length > i + 1) ? args[i + 1] : text5);
					i++;
					break;
				case "skipcreateprofile":
					skipCreateProfile = true;
					break;
				default:
				{
					string key = text6.ToUpperInvariant();
					string text7 = ((args.Length > i + 1) ? args[i + 1] : null);
					if (string.IsNullOrWhiteSpace(text7) || IsCommandSwitch(text7))
					{
						text7 = null;
					}
					else
					{
						i++;
					}
					dictionary.Add(key, text7);
					break;
				}
				}
				continue;
			}
			DisplayInvalidArguments();
			return new UserArgumentsResult(isPassed: false, UserArgumentsType.DisplayInvalidArguments);
		}
		if (!string.IsNullOrWhiteSpace(text) && !flag)
		{
			DisplayMissingAcceptEULASwitch();
			return new UserArgumentsResult(isPassed: false, UserArgumentsType.DisplayMissingAcceptEULASwitch);
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			DisplayScenarioMissing();
			return new UserArgumentsResult(isPassed: false, UserArgumentsType.DisplayScenarioMissing);
		}
		DisplayScenarioToExecute(text);
		return new UserArgumentsResult(isPassed: true, UserArgumentsType.DisplayScenarioToExecute, text, dictionary, text2, showProgress, officeversion, offlineScan, isScriptInvocation, text3, text4, text5, newprofilename, skipCreateProfile);
	}

	public Session GetCommandLineSession(UserArgumentsResult result)
	{
		Session commandLineSession = SessionHelper.GetCommandLineSession(result.ScenarioToExecute);
		commandLineSession.ClientName = result.ClientName;
		commandLineSession.Language = result.Language;
		commandLineSession.Platform = result.Platform;
		commandLineSession.IsScriptInvocation = result.IsScriptInvocation;
		return commandLineSession;
	}

	private static bool IsCommandSwitch(string arg)
	{
		if (!arg.StartsWith("-", StringComparison.InvariantCulture))
		{
			return arg.StartsWith("/", StringComparison.InvariantCulture);
		}
		return true;
	}

	private static void DisplaySwitches(Assembly currentAssembly)
	{
		string location = currentAssembly.Location;
		string name = currentAssembly.ManifestModule.Name;
		string text = strings.ResourceManager.GetString("CommandLineSwitches");
		LogToConsole.WriteLine(633445, string.Format(text ?? string.Empty, location, name), isError: false);
	}

	private static void DisplayHelp()
	{
		LogToConsole.WriteLine(633446, strings.ResourceManager.GetString("CommandLineHelpText"), isError: false);
	}

	private static void DisplayEULA(Assembly currentAssembly)
	{
		if (Path.GetDirectoryName(currentAssembly.Location) == null)
		{
			_ = string.Empty;
		}
		using StreamReader streamReader = new StreamReader("SaraEULA.txt");
		LogToConsole.WriteLine(633447, streamReader.ReadToEnd() ?? "", isError: false);
	}

	private static void DisplayScenarioMissing()
	{
		LogToConsole.WriteLine(633448, strings.ResourceManager.GetString("RunScenarioMissing") + "\r\n" + strings.ResourceManager.GetString("CommandLineHelpText"), isError: true);
	}

	private static void DisplayInvalidArguments()
	{
		LogToConsole.WriteLine(633449, strings.ResourceManager.GetString("UnidentifiedArgumentFound"), isError: true);
	}

	private static void DisplayMissingAcceptEULASwitch()
	{
		LogToConsole.WriteLine(633449, strings.ResourceManager.GetString("CommandLineAcceptEULAMissing"), isError: true);
	}

	private static void DisplayScenarioToExecute(string scenarioToExecute)
	{
		LogToConsole.WriteLine(633451, strings.ResourceManager.GetString("RunningScenario") + " " + scenarioToExecute + "...", isError: false);
	}

	private static void DisplayUserFolderPathMissing(string userfolderPath)
	{
		LogToConsole.WriteLine(633448, string.Format(strings.ResourceManager.GetString("MissingUserFolderPath"), userfolderPath), isError: true);
	}

	private static void DisplayInvalidUserFolderPath(string userfolderPath)
	{
		LogToConsole.WriteLine(650500, string.Format(strings.ResourceManager.GetString("InvalidUserFolderPath"), userfolderPath), isError: true);
	}

	private static bool ValidateUserFolderPathAccess(string userLogFolderPath)
	{
		try
		{
			Directory.GetAccessControl(userLogFolderPath);
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
		return true;
	}

	private static void DisplayOfficeVersionMissing()
	{
		LogToConsole.WriteLine(650610, strings.ResourceManager.GetString("MissingOfficeVersion"), isError: true);
	}

	private static void DisplayInvalidOfflineScanParameter()
	{
		LogToConsole.WriteLine(650752, strings.ResourceManager.GetString("InvalidOfflineScanParameter"), isError: true);
	}
}
