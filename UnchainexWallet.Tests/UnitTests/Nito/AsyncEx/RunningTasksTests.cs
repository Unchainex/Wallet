using System.Threading.Tasks;
using UnchainexWallet.Nito.AsyncEx;
using Xunit;

namespace UnchainexWallet.Tests.UnitTests.Nito.AsyncEx;

/// <summary>
/// Tests for <see cref="RunningTasks"/> class.
/// </summary>
public class RunningTasksTests
{
	[Fact]
	public async Task RemeberWithTestAsync()
	{
		AbandonedTasks processingEvents = new();

		using (RunningTasks.RememberWith(processingEvents))
		{
			Assert.Equal(1, processingEvents.Count);
		}

		Assert.Equal(0, processingEvents.Count);

		using (RunningTasks.RememberWith(processingEvents))
		{
			Assert.Equal(1, processingEvents.Count);
		}

		Assert.Equal(0, processingEvents.Count);
		await processingEvents.WhenAllAsync();
	}
}
