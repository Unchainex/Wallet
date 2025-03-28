using Microsoft.CodeAnalysis;

namespace UnchainexWallet.Fluent.Generators.Abstractions;

internal record GeneratorStepContext(GeneratorExecutionContext Context, Compilation Compilation);
