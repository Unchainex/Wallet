using Microsoft.CodeAnalysis;
using UnchainexWallet.Fluent.Generators.Abstractions;

namespace UnchainexWallet.Fluent.Generators.Generators;

[Generator]
internal class MainGenerator : CombinedGenerator
{
	public MainGenerator()
	{
		AddStaticFileGenerator<AutoInterfaceAttributeGenerator>();
		AddStaticFileGenerator<AutoNotifyAttributeGenerator>();
		Add<AutoNotifyGenerator>();
		Add<AutoInterfaceGenerator>();
		Add<UiContextConstructorGenerator>();
		Add<FluentNavigationGenerator>();
	}
}
