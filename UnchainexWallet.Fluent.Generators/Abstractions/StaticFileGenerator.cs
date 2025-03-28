using System.Collections.Generic;

namespace UnchainexWallet.Fluent.Generators.Abstractions;

internal abstract class StaticFileGenerator
{
	public abstract IEnumerable<(string FileName, string Source)> Generate();
}
