using Avalonia;

namespace UnchainexWallet.Fluent.Controls.Rendering;

internal record struct DrawPayload(
	HandlerCommand HandlerCommand,
	IDrawHandler? Handler = null,
	Rect Bounds = default);
