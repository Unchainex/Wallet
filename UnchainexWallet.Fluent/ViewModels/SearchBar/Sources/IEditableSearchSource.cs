using UnchainexWallet.Fluent.ViewModels.SearchBar.SearchItems;

namespace UnchainexWallet.Fluent.ViewModels.SearchBar.Sources;

public interface IEditableSearchSource : ISearchSource
{
	void Remove(params ISearchItem[] searchItems);
	void Add(params ISearchItem[] searchItems);
	void SetQueries(IObservable<string> queries);
}
