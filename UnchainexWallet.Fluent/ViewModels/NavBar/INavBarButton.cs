#pragma warning disable IDE0130

using System.Threading.Tasks;

namespace UnchainexWallet.Fluent;

public interface INavBarButton : INavBarItem
{
	Task Activate();
}
