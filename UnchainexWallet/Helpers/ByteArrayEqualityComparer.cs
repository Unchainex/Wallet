using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnchainexWallet.Crypto;

namespace UnchainexWallet.Helpers;

public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
{
	public bool Equals([AllowNull] byte[] x, [AllowNull] byte[] y) => ByteHelpers.CompareFastUnsafe(x, y);

	public int GetHashCode([DisallowNull] byte[] obj) => HashHelpers.ComputeHashCode(obj);
}
