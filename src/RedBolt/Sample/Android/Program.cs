namespace RedBolt.Sample.Android;

internal static class SampleCatalog
{
    internal static readonly SampleEntry[] Entries =
    [
        new("actions", "Actions", static () => new global::RedBolt.Sample.Actions.ActionsApplication()),
    ];
}
