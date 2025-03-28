using System.Collections.Generic;
using UnchainexWallet.Blockchain.Analysis.Clustering;

namespace UnchainexWallet.Fluent.Helpers;

public class LabelsComparer : IEqualityComparer<LabelsArray>
{
	private static LabelsComparer? ComparerInstance;

	public static IEqualityComparer<LabelsArray> Instance => ComparerInstance ??= new LabelsComparer();

	public bool Equals(LabelsArray x, LabelsArray y)
	{
		return x.Equals(y, StringComparer.OrdinalIgnoreCase);
	}

	public int GetHashCode(LabelsArray obj)
	{
		return 0;
	}
}
