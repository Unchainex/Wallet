using System.Reactive.Linq;
using ReactiveUI;
using UnchainexWallet.Blockchain.Keys;
using UnchainexWallet.Fluent.Validation;
using UnchainexWallet.Fluent.ViewModels.Dialogs.Base;
using UnchainexWallet.Models;

namespace UnchainexWallet.Fluent.ViewModels.Dialogs;

[NavigationMetaData(Title = "Advanced Recovery Options", NavigationTarget = NavigationTarget.CompactDialogScreen)]
public partial class AdvancedRecoveryOptionsViewModel : DialogViewModelBase<int?>
{
	[AutoNotify] private string _minGapLimit;

	public AdvancedRecoveryOptionsViewModel(int minGapLimit)
	{
		_minGapLimit = minGapLimit.ToString();

		this.ValidateProperty(x => x.MinGapLimit, ValidateMinGapLimit);

		SetupCancel(enableCancel: true, enableCancelOnEscape: true, enableCancelOnPressed: true);
		EnableBack = false;

		NextCommand = ReactiveCommand.Create(
			() => Close(result: int.Parse(MinGapLimit)),
			this.WhenAnyValue(x => x.MinGapLimit).Select(_ => !Validations.Any));
	}

	private void ValidateMinGapLimit(IValidationErrors errors)
	{
		if (!int.TryParse(MinGapLimit, out var minGapLimit) ||
			minGapLimit is < KeyManager.AbsoluteMinGapLimit or > KeyManager.MaxGapLimit)
		{
			errors.Add(
				ErrorSeverity.Error,
				$"Must be a number between {KeyManager.AbsoluteMinGapLimit} and {KeyManager.MaxGapLimit}.");
		}
	}
}
