using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using UnchainexWallet.Fluent.Models.UI;
using UnchainexWallet.Fluent.ViewModels.Navigation;
using UnchainexWallet.Fluent.ViewModels.SearchBar.Patterns;
using UnchainexWallet.Fluent.ViewModels.SearchBar.SearchItems;

namespace UnchainexWallet.Fluent.ViewModels.SearchBar.Sources;

public class ActionsSearchSource : ISearchSource
{
	public ActionsSearchSource(UiContext uiContext, IObservable<string> query)
	{
		UiContext = uiContext;

		var filter = query.Select(SearchSource.DefaultFilter);

		Changes = GetItemsFromMetadata()
			.ToObservable()
			.ToObservableChangeSet(x => x.Key)
			.Filter(filter);
	}

	public IObservable<IChangeSet<ISearchItem, ComposedKey>> Changes { get; }

	public UiContext UiContext { get; }

	private IEnumerable<ISearchItem> GetItemsFromMetadata()
	{
		return NavigationManager.MetaData
			.Where(m => m.Searchable)
			.Select(m =>
			{
				var onActivate = CreateOnActivateFunction(m);
				var searchItem = new ActionableItem(m.Title, m.Caption, onActivate, m.Category ?? "No category", m.Keywords)
				{
					Icon = m.IconName,
					IsDefault = true,
					Priority = m.Order,
				};
				return searchItem;
			});
	}

	private Func<Task> CreateOnActivateFunction(NavigationMetaData navigationMetaData)
	{
		return async () =>
		{
			var vm = await NavigationManager.MaterializeViewModelAsync(navigationMetaData);
			if (vm is null)
			{
				return;
			}

			if (vm is INavBarButton navBarButton)
			{
				await navBarButton.Activate();
			}
			else if (vm is TriggerCommandViewModel triggerCommandViewModel && triggerCommandViewModel.TargetCommand.CanExecute(default))
			{
				triggerCommandViewModel.TargetCommand.Execute(default);
			}
			else
			{
				UiContext.Navigate(vm.DefaultTarget).To(vm);
			}
		};
	}
}
