# Contributing to Tremors

Thank you for helping. Tremors exists for people with shaky hands, so the most valuable
contribution is often not code but experience: what works, what is still hard, which
setting helped.

## Reporting a problem or an idea

Open an [issue](https://github.com/JeroenJonckheer/Tremors/issues/new/choose). Plain
language is fine, in English or Dutch. Mention the strength setting and the type of
mouse if you can.

## Changing the code

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Fork and clone the repository.
3. `dotnet test` must pass, without warnings (warnings are treated as errors).
4. Keep pull requests small and focused, and explain *why* in the description.

Guidelines:

- **Simple first.** The people using Tremors should never have to understand it.
  New settings need a very good reason.
- **The hook must stay fast.** Windows removes a low-level hook that responds too
  slowly. No I/O, allocations in loops or locks in the hook path.
- **Filter changes need tests.** `Smoother` has no Windows dependencies; add a case to
  [`SmootherTests.cs`](tests/Tremors.Tests/SmootherTests.cs) and describe what you
  measured.
- **Text goes in [`Text.cs`](src/Tremors/Text.cs)**, in every language. New languages
  are welcome.
- **No network, no telemetry.** Tremors stays private.

See [docs/how-it-works.md](docs/how-it-works.md) for the design.

## Releasing (maintainers)

1. Update [CHANGELOG.md](CHANGELOG.md).
2. `git tag v1.2.3 && git push origin v1.2.3`.
3. GitHub Actions builds `Tremors.exe`, runs the tests and publishes the release.
