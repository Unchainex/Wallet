using System.Linq;
using System.Collections.Generic;
using UnchainexWallet.Unchain.Backend.Rounds;

namespace UnchainexWallet.Tests.UnitTests.Unchain.Backend.Rounds.Utils;

public static class ArenaExtensions
{
	public static IEnumerable<Round> GetActiveRounds(this Arena arena)
		=> arena.Rounds.Where(x => x.Phase != Phase.Ended);
}
