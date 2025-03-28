using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using UnchainexWallet.Fluent.ViewModels.SearchBar.Patterns;
using UnchainexWallet.Fluent.ViewModels.SearchBar.SearchItems;

namespace UnchainexWallet.Fluent.ViewModels.SearchBar.Sources;

public class CompositeSearchSource : ISearchSource
{
	private readonly ISearchSource[] _sources;

	public CompositeSearchSource(params ISearchSource[] sources)
	{
		_sources = sources;
	}

	public IObservable<IChangeSet<ISearchItem, ComposedKey>> Changes => _sources.Select(r => r.Changes).Merge();
}
