using System.Linq;
using Avalonia.Data.Converters;

namespace UnchainexWallet.Fluent.Converters;

public static class BooleanConverters
{
	public static readonly IMultiValueConverter Or = new FuncMultiValueConverter<bool, bool>(n => n.Any(b => b));
}
