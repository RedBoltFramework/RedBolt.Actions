// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Actions;

using System;

using RedBolt.Core;

public class ActionState
{
    protected ActionState(BaseAction action, RedObject? target)
    {
        _action = action;
        _target = target is not null ? new WeakReference<RedObject>(target) : null;
        _originalTarget = target is not null ? new WeakReference<RedObject>(target) : null;
    }

    /// <summary>Called once per frame.</summary>
    /// <remarks>
    /// Time value is between 0 and 1.
    /// For example:
    /// 0 means that the action just started.
    /// 0.5 means that the action is in the middle.
    /// 1 means that the action is over.
    /// </remarks>
    public virtual void Update(float time)
    {
    }

    /// <summary>Gets the target.</summary>
    /// <remarks>
    /// Will be set with the 'StartAction' method of the corresponding Action.
    /// When the 'Stop' method is called, Target will be set to null.
    /// </remarks>
    public RedObject? Target => _target is not null && _target.TryGetTarget(out var target) ? target : null;

    /// <summary>Gets the original target.</summary>
    public RedObject? OriginalTarget =>
        _originalTarget is not null && _originalTarget.TryGetTarget(out var target) ? target : null;

    /// <summary>Gets the action.</summary>
    public BaseAction Action => _action;

    /// <summary>Gets a value indicating whether this instance is done.</summary>
    public virtual bool IsDone => true;

    /// <summary>Called after the action has finished.</summary>
    /// <remarks>
    /// It will set the 'Target' to null.
    /// IMPORTANT: You should never call this method manually. Instead, use: "target.StopAction(actionState);"
    /// </remarks>
    public virtual void Stop()
    {
        _target = null;
    }

    /// <summary>Called every frame with it's delta time.</summary>
    /// <remarks>DON'T override unless you know what you are doing.</remarks>
    protected internal virtual void Step(float dt)
    {
    }

    /// <summary>Call StartAction on an action.</summary>
    protected ActionState? StartAction(BaseAction? action, RedObject? target) =>
        action is not null ? action.StartAction(target) : null;

    private readonly BaseAction _action;
    /// <summary>Active target reference. Set to null when action is complete.</summary>
    private WeakReference<RedObject>? _target;
    /// <summary>Original target reference. To track target references in ActionManager.</summary>
    private readonly WeakReference<RedObject>? _originalTarget;
}
