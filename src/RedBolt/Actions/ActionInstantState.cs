// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Actions;

using RedBolt.Core;

/// <summary>Finite time action state.</summary>
public class ActionInstantState : FiniteTimeActionState
{
    /// <summary>Construct.</summary>
    public ActionInstantState(ActionInstant action, RedObject? target)
        : base(action, target)
    {
    }

    /// <summary>Destruct.</summary>
    public override void Dispose()
    {
        base.Dispose();
    }

    /// <summary>Gets a value indicating whether this instance is done.</summary>
    public override bool IsDone => true;

    /// <summary>Called every frame with it's delta time.</summary>
    protected internal override void Step(float dt)
    {
        Update(1.0f);
    }
}
