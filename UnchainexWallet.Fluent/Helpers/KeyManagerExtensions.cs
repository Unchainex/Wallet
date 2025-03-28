using System.Collections.Generic;
using System.Linq;
using UnchainexWallet.Blockchain.Keys;

namespace UnchainexWallet.Fluent.Helpers;

public static class KeyManagerExtensions
{
	public static (List<string>, List<string>) GetLabels(this KeyManager km)
	{
		var (changeKeys, receiveKeys) = km.GetKeys().Partition(x => x.IsInternal);
		return (changeKeys.SelectMany(x => x.Labels).ToList(), receiveKeys.SelectMany(x => x.Labels).ToList());
	}
}
