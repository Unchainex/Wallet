using ReactiveUI;
using System.Windows.Input;
using UnchainexWallet.Fluent.Helpers;
using UnchainexWallet.Fluent.ViewModels;
using UnchainexWallet.Fluent.ViewModels.HelpAndSupport;
using UnchainexWallet.Models;
using UnchainexWallet.Helpers;

namespace UnchainexWallet.Fluent.CrashReport.ViewModels;

public class CrashReportWindowViewModel : ViewModelBase
{
	public CrashReportWindowViewModel(SerializableException serializedException)
	{
		SerializedException = serializedException;
		CancelCommand = ReactiveCommand.Create(() => AppLifetimeHelper.Shutdown(withShutdownPrevention: false, restart: true));
		NextCommand = ReactiveCommand.Create(() => AppLifetimeHelper.Shutdown(withShutdownPrevention: false, restart: false));

		OpenGitHubRepoCommand = ReactiveCommand.CreateFromTask(async () => await IoHelpers.OpenBrowserAsync(Link));

		CopyTraceCommand = ReactiveCommand.CreateFromTask(async () =>
		{
			await ApplicationHelper.SetTextAsync(Trace);
		});
	}

	public SerializableException SerializedException { get; }

	public ICommand OpenGitHubRepoCommand { get; }

	public ICommand NextCommand { get; }

	public ICommand CancelCommand { get; }

	public ICommand CopyTraceCommand { get; }

	public string Caption => $"A problem has occurred and Unchainex is unable to continue.";

	public string Link => AboutViewModel.BugReportLink;

	public string Trace => SerializedException.ToString();

	public string Title => "Unchainex has crashed";
}
