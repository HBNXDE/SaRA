using System;
using System.Timers;
using Microsoft.Sara.Framework.Common.Util;

namespace Microsoft.SaraStandalone.Main;

public class DisplayScenarioProgress
{
	private const int ElapsedTime = 60000;

	public Timer ProgressTimer { get; }

	public string Scenario { get; }

	public bool ShowProgress { get; }

	public DisplayScenarioProgress(string scenario, bool showProgress = true)
	{
		Scenario = scenario;
		ShowProgress = showProgress;
		if (ShowProgress && !string.IsNullOrEmpty(Scenario) && IsScenarioEnabledforShowProgress())
		{
			ProgressTimer = new Timer();
		}
	}

	public void StartProgressDisplay()
	{
		if (ProgressTimer != null)
		{
			ProgressTimer.Elapsed += OnTimedEvent;
			ProgressTimer.AutoReset = true;
			ProgressTimer.Enabled = true;
		}
	}

	public void StopProgressDisplay()
	{
		if (ProgressTimer != null)
		{
			ProgressTimer.Stop();
		}
	}

	private void OnTimedEvent(object source, ElapsedEventArgs e)
	{
		ProgressTimer.Interval = 60000.0;
		LogToConsole.WriteLine(650535, strings.ResourceManager.GetString("ScanningOutlook") + "\r\n" + strings.ResourceManager.GetString("DisplayProgress"), isError: false);
	}

	private bool IsScenarioEnabledforShowProgress()
	{
		if (!Scenario.Equals(strings.ResourceManager.GetString("ExpertExperienceAdminTask"), StringComparison.InvariantCultureIgnoreCase))
		{
			return Scenario.Equals(strings.ResourceManager.GetString("OutlookCalendarCheckTask"), StringComparison.InvariantCultureIgnoreCase);
		}
		return true;
	}
}
