// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Actions;

using RedBolt.Core;
using RedBolt.IO;

/// <summary>Finite time action.</summary>
[Reflected]
public partial class ActionInstant : FiniteTimeAction
{
    /// <summary>Construct.</summary>
    public ActionInstant()
    {
    }

    /// <summary>Get action duration.</summary>
    public override float Duration => 1.1920929E-07f;

    /// <summary>Serialize content from/to archive. May throw ArchiveException.</summary>
    public override void SerializeInBlock(Archive archive)
    {
        // Skip FiniteTimeAction::SerializeInBlock because duration is always 0
    }

    /// <summary>Create reversed action.</summary>
    public override FiniteTimeAction Reverse()
    {
        return new ActionInstant();
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        return new ActionInstantState(this, target);
    }
}
