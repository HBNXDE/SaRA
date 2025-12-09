using System.Collections.Generic;

namespace Microsoft.SaraStandalone.Main;

public class UserArgumentsResult
{
	public bool IsPassed { get; }

	public UserArgumentsType UserArgumentsType { get; }

	public string ScenarioToExecute { get; }

	public Dictionary<string, object> CustomArgs { get; }

	public string UserLogFolderPath { get; }

	public bool ShowProgress { get; }

	public string OfficeVersion { get; }

	public bool OfflineScan { get; }

	public bool IsScriptInvocation { get; }

	public string ClientName { get; }

	public string Language { get; set; }

	public string Platform { get; set; }

	public string NewProfileName { get; set; }

	public bool SkipCreateProfile { get; set; }

	public UserArgumentsResult(bool isPassed, UserArgumentsType userArgumentsType, string scenarioToExecute = null, Dictionary<string, object> customArgs = null, string userLogFolderPath = null, bool showProgress = true, string officeversion = null, bool offlineScan = false, bool isScriptInvocation = false, string clientName = "Sara", string language = "English", string platform = "64", string newprofilename = null, bool skipCreateProfile = false)
	{
		IsPassed = isPassed;
		UserArgumentsType = userArgumentsType;
		ScenarioToExecute = scenarioToExecute;
		CustomArgs = customArgs;
		UserLogFolderPath = userLogFolderPath;
		ShowProgress = showProgress;
		OfficeVersion = officeversion;
		OfflineScan = offlineScan;
		IsScriptInvocation = isScriptInvocation;
		ClientName = clientName;
		Language = language;
		Platform = platform;
		NewProfileName = newprofilename;
		SkipCreateProfile = skipCreateProfile;
	}
}
