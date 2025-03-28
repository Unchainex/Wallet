using System.Windows.Input;
using ReactiveUI;
using UnchainexWallet.Fluent.Extensions;
using UnchainexWallet.Fluent.Models.UI;

namespace UnchainexWallet.Fluent.ViewModels.OpenDirectory;

public abstract class OpenFileViewModel : TriggerCommandViewModel
{
	public OpenFileViewModel(UiContext uiContext)
	{
		UiContext = uiContext;
	}

	public abstract string FilePath { get; }

	public override ICommand TargetCommand =>
		ReactiveCommand.CreateFromTask(async () =>
		{
			try
			{
				await UiContext.FileSystem.OpenFileInTextEditorAsync(FilePath);
			}
			catch (Exception ex)
			{
				await ShowErrorAsync("Open", ex.ToUserFriendlyString(), "Unchainex was unable to open the file");
			}
		});
}
