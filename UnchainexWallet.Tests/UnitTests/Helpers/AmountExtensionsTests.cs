using NBitcoin;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Fluent.Models.Wallets;
using UnchainexWallet.Services;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Helpers;

public class AmountExtensionsTests
{
	[Fact]
	public void DifferenceShouldBeExpected()
	{
		UnchainexWallet.Fluent.Services.EventBus = new EventBus();
		var p = new AmountProvider();
		var previous = new Amount(Money.FromUnit(221, MoneyUnit.Satoshi), p);
		var current = new Amount(Money.FromUnit(110, MoneyUnit.Satoshi), p);

		var result = current.Diff(previous);

		var expected = -0.5m;
		decimal tolerance = 0.01m;
		var areApproximatelyEqual = Math.Abs((decimal)result - expected) < tolerance;
		Assert.True(areApproximatelyEqual, $"Result is not the expected by the given tolerance. Result: {result}, Expected: {expected}, Tolerance: {tolerance}");
	}
}
