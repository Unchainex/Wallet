using DynamicData;
using UnchainexWallet.Fluent.ViewModels.SearchBar.Patterns;
using UnchainexWallet.Fluent.ViewModels.SearchBar.SearchItems;

namespace UnchainexWallet.Fluent.ViewModels.SearchBar.Sources;

public interface ISearchSource
{
	IObservable<IChangeSet<ISearchItem, ComposedKey>> Changes { get; }
}
