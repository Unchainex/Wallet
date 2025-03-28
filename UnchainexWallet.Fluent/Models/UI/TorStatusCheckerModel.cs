using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using UnchainexWallet.Fluent.Extensions;
using UnchainexWallet.Services;
using UnchainexWallet.Tor.StatusChecker;

namespace UnchainexWallet.Fluent.Models.UI;

[AutoInterface]
public partial class TorStatusCheckerModel
{
	public TorStatusCheckerModel()
	{
		Issues =
			Services.EventBus.AsObservable<TorNetworkStatusChanged>()
				.Select(e => e.ReportedIssues.ToList());
	}

	public IObservable<IList<Issue>> Issues { get; }
}
