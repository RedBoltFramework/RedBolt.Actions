// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Actions;

using RedBolt.Core;
using RedBolt.IO;
using RedBolt.Scene;

/// <summary>Base action state.</summary>
[Reflected]
public partial class BaseAction : Serializable
{
    private sealed class NoActionState : ActionState
    {
        public NoActionState(BaseAction action, RedObject? target)
            : base(action, target)
        {
        }
    }

    /// <summary>Construct.</summary>
    public BaseAction()
    {
    }

    /// <summary>Destruct.</summary>
    public override void Dispose()
    {
        base.Dispose();
    }

    /// <summary>Get action from argument or empty action.</summary>
    public BaseAction GetOrDefault(BaseAction? action)
    {
        if (action is not null)
            return action;
        return GetSubsystem<ActionManager>().GetEmptyAction();
    }

    /// <summary>Serialize content from/to archive. May throw ArchiveException.</summary>
    public override void SerializeInBlock(Archive archive)
    {
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal virtual ActionState StartAction(RedObject? target) => new NoActionState(this, target);
}
