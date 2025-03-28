#pragma warning disable IDE0130

using System.ComponentModel;

namespace UnchainexWallet.Fluent;

public interface INavBarItem : INotifyPropertyChanged
{
	string Title { get; }
	string IconName { get; }
	string IconNameFocused { get; }
}
