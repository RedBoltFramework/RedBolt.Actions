# RedBolt.Actions workspace instructions

- RedBolt is expected at `../RedBolt` unless `RedBoltRoot` is supplied.
- Tests remain in `../RedBolt.Tests`.
- Preserve the public `RedBolt.Actions` assembly and namespace.
- Keep desktop, WebAssembly, and Android builds working.
- Follow the monorepo `doc/VERIFICATION.md`: build affected projects and run focused tests; validate affected platform samples and packages when their behavior or integration changes. Run the full supported matrix for release validation or when requested.
