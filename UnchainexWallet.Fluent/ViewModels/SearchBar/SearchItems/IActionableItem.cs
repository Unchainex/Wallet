using System.Threading.Tasks;

namespace UnchainexWallet.Fluent.ViewModels.SearchBar.SearchItems;

public interface IActionableItem : ISearchItem
{
	Func<Task> Activate { get; }
}
