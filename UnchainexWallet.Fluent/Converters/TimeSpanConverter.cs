using Avalonia.Data.Converters;
using UnchainexWallet.Fluent.Helpers;

namespace UnchainexWallet.Fluent.Converters;

public class TimeSpanConverter
{
	public static readonly IValueConverter ToEstimatedConfirmationTime =
		new FuncValueConverter<TimeSpan?, string?>(ts =>
		{
			if (ts is { } t)
			{
				var friendlyString = TextHelpers.TimeSpanToFriendlyString(t);
				if (friendlyString == "")
				{
					return "";
				}

				return $"≈ {friendlyString}";
			}

			return "";
		});
}
