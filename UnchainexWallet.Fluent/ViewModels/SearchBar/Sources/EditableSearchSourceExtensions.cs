using System.Collections.Generic;
using System.Linq;
using UnchainexWallet.Fluent.ViewModels.SearchBar.SearchItems;

namespace UnchainexWallet.Fluent.ViewModels.SearchBar.Sources;

public static class EditableSearchSourceExtensions
{
	public static void Toggle(this IEditableSearchSource searchSource, ISearchItem searchItem, bool isDisplayed)
	{
		if (isDisplayed)
		{
			searchSource.Add(searchItem);
		}
		else
		{
			searchSource.Remove(searchItem);
		}
	}

	public static void Toggle(this IEditableSearchSource searchSource, IEnumerable<ISearchItem> searchItems, bool isDisplayed)
	{
		if (isDisplayed)
		{
			searchSource.Add(searchItems.ToArray());
		}
		else
		{
			searchSource.Remove(searchItems.ToArray());
		}
	}
}
