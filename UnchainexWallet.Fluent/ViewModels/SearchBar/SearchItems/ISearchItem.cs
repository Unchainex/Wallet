using System.Collections.Generic;
using UnchainexWallet.Fluent.ViewModels.SearchBar.Patterns;

namespace UnchainexWallet.Fluent.ViewModels.SearchBar.SearchItems;

public interface ISearchItem
{
	public string Name { get; }
	public string Description { get; }
	public ComposedKey Key { get; }
	public string? Icon { get; set; }
	public string Category { get; }
	public IEnumerable<string> Keywords { get; }
	public bool IsDefault { get; }
	public int Priority { get; }
}
