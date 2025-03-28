using System;
using UnchainexWallet.Logging;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace UnchainexWallet.Daemon;

public class Program
{
	public static async Task<int> Main(string[] args)
	{
		var app = UnchainexAppBuilder
			.Create("Unchainex Daemon", args)
			.EnsureSingleInstance()
			.OnUnhandledExceptions(LogUnhandledException)
			.OnUnobservedTaskExceptions(LogUnobservedTaskException)
			.Build();

		var exitCode = await app.RunAsConsoleAsync().ConfigureAwait(false);
		return (int)exitCode;
	}

	private static void LogUnobservedTaskException(object? sender, AggregateException e)
	{
		ReadOnlyCollection<Exception> innerExceptions = e.Flatten().InnerExceptions;

		switch (innerExceptions)
		{
			case [SocketException { SocketErrorCode: SocketError.OperationAborted }]:
			// Source of this exception is NBitcoin library.
			case [OperationCanceledException { Message: "The peer has been disconnected" }]:
				// Until https://github.com/MetacoSA/NBitcoin/pull/1089 is resolved.
				Logger.LogTrace(e);
				break;
			default:
				Logger.LogDebug(e);
				break;
		}
	}

	private static void LogUnhandledException(object? sender, Exception e) =>
		Logger.LogWarning(e);
}
