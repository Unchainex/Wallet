namespace UnchainexWallet.Fluent.Helpers;

public static class ConfirmationTimeLabel
{
	public static string AxisLabel(TimeSpan timeSpan)
	{
		if (timeSpan <= TransactionFeeHelper.CalculateConfirmationTime(UnchainexWallet.Helpers.Constants.FastestConfirmationTarget))
		{
			return "fastest";
		}

		return TimeSpanFormatter.Format(timeSpan, new TimeSpanFormatter.Configuration("day", "hour", "min"));
	}

	public static string SliderLabel(TimeSpan timeSpan)
	{
		if (timeSpan <= TransactionFeeHelper.CalculateConfirmationTime(UnchainexWallet.Helpers.Constants.FastestConfirmationTarget))
		{
			return "fastest";
		}

		return "~" + TimeSpanFormatter.Format(timeSpan, new TimeSpanFormatter.Configuration(" day", " hour", " min"));
	}
}
