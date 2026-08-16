// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Actions;

using System;

using RedBolt.Core;

/// <summary>Finite time action state.</summary>
public class FiniteTimeActionState : ActionState, IDisposable
{
    /// <summary>Construct.</summary>
    public FiniteTimeActionState(FiniteTimeAction action, RedObject? target)
        : base(action, target)
    {
        _duration = action.Duration;
    }

    /// <summary>Destruct.</summary>
    public virtual void Dispose()
    {
    }

    /// <summary>Gets a value indicating whether this instance is done.</summary>
    public override bool IsDone => _elapsed >= _duration;

    /// <summary>Called every frame with it's delta time.</summary>
    protected internal override void Step(float dt)
    {
        if (_firstTick)
        {
            _firstTick = false;
            _elapsed = 0.0f;
        }
        else
        {
            _elapsed += dt;
        }

        Update(Math.Clamp(_elapsed / Math.Max(_duration, 1.1920929E-07f), 0.0f, 1.0f));
    }

    /// <summary>Get action duration.</summary>
    public float Duration => _duration;

    /// <summary>Get action elapsed time.</summary>
    public float Elapsed => _elapsed;

    /// <summary>Call StartAction on an action.</summary>
    protected FiniteTimeActionState? StartAction(FiniteTimeAction? action, RedObject? target)
    {
        if (action is not null)
            return base.StartAction(action, target) as FiniteTimeActionState;
        return null;
    }

    private readonly float _duration;
    private float _elapsed;
    private bool _firstTick = true;
}
