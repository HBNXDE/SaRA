using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.SaraStandalone.Main;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class strings
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Microsoft.SaraStandalone.Main.strings", typeof(strings).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string CommandLineAcceptEULAMissing => ResourceManager.GetString("CommandLineAcceptEULAMissing", resourceCulture);

	internal static string CommandLineHelpText => ResourceManager.GetString("CommandLineHelpText", resourceCulture);

	internal static string CommandLineSwitches => ResourceManager.GetString("CommandLineSwitches", resourceCulture);

	internal static string DisplayProgress => ResourceManager.GetString("DisplayProgress", resourceCulture);

	internal static string ExpertExperienceAdminTask => ResourceManager.GetString("ExpertExperienceAdminTask", resourceCulture);

	internal static string ExpiredMessage => ResourceManager.GetString("ExpiredMessage", resourceCulture);

	internal static string InvalidOfflineScanParameter => ResourceManager.GetString("InvalidOfflineScanParameter", resourceCulture);

	internal static string InvalidUserFolderPath => ResourceManager.GetString("InvalidUserFolderPath", resourceCulture);

	internal static string M365 => ResourceManager.GetString("M365", resourceCulture);

	internal static string MissingOfficeVersion => ResourceManager.GetString("MissingOfficeVersion", resourceCulture);

	internal static string MissingUserFolderPath => ResourceManager.GetString("MissingUserFolderPath", resourceCulture);

	internal static string OutlookCalendarCheckTask => ResourceManager.GetString("OutlookCalendarCheckTask", resourceCulture);

	internal static string PressAnykeyToContinue => ResourceManager.GetString("PressAnykeyToContinue", resourceCulture);

	internal static string RunningScenario => ResourceManager.GetString("RunningScenario", resourceCulture);

	internal static string RunScenarioMissing => ResourceManager.GetString("RunScenarioMissing", resourceCulture);

	internal static string ScanningOutlook => ResourceManager.GetString("ScanningOutlook", resourceCulture);

	internal static string StartingCommandLineMessage => ResourceManager.GetString("StartingCommandLineMessage", resourceCulture);

	internal static string UnexpectedError => ResourceManager.GetString("UnexpectedError", resourceCulture);

	internal static string UnidentifiedArgumentFound => ResourceManager.GetString("UnidentifiedArgumentFound", resourceCulture);

	internal static string UnknownScenarioName => ResourceManager.GetString("UnknownScenarioName", resourceCulture);

	internal strings()
	{
	}
}
