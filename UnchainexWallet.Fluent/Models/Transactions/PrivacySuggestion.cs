using NBitcoin;
using System.Collections.Generic;
using UnchainexWallet.Blockchain.Analysis.Clustering;
using UnchainexWallet.Blockchain.TransactionBuilding;
using UnchainexWallet.Blockchain.TransactionOutputs;
using UnchainexWallet.Fluent.Extensions;

namespace UnchainexWallet.Fluent.Models.Transactions;

// TODO: Case Types should be nested inside the base, and remove the "Suggestion" Suffix
// Avalonia XAML currently does not support {x:Type} references to nested types (https://github.com/AvaloniaUI/Avalonia/issues/2725)
// Revisit this after Avalonia V11 upgrade
public abstract record PrivacySuggestion(BuildTransactionResult? Transaction) : PrivacyItem();

public record LabelManagementSuggestion(BuildTransactionResult? Transaction = null, LabelsArray? NewLabels = null) : PrivacySuggestion(Transaction);

public record FullPrivacySuggestion(BuildTransactionResult Transaction, decimal Difference, string DifferenceText, string DifferenceAmountText, IEnumerable<SmartCoin> Coins, bool IsChangeless) : PrivacySuggestion(Transaction);

public record BetterPrivacySuggestion(BuildTransactionResult Transaction, string DifferenceText, string DifferenceAmountText, IEnumerable<SmartCoin> Coins, bool IsChangeless) : PrivacySuggestion(Transaction);

public record ChangeAvoidanceSuggestion(BuildTransactionResult Transaction, decimal Difference, string DifferenceText, string DifferenceAmountText, bool IsMore, bool IsLess) : PrivacySuggestion(Transaction)
{
    public Money GetAmount(Destination destination) => Transaction!.CalculateDestinationAmount(destination);

    public bool IsSameAmount => !IsMore && !IsLess;
}
