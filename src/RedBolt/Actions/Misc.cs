// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Actions;

using System;

using RedBolt.Core;
using RedBolt.IO;
using RedBolt.Scene;

/// <summary>Remove self from parent. The Target of the action should be either Node or UIElement.</summary>
[Reflected]
public partial class RemoveSelf : FiniteTimeAction
{
    private sealed class RemoveSelfState : FiniteTimeActionState
    {
        public RemoveSelfState(RemoveSelf action, RedObject? target)
            : base(action, target)
        {
        }

        public override void Update(float time)
        {
            var target = Target;
            if (target is null)
            {
                return;
            }
            if (target is Node node)
            {
                node.Remove();
            }
        }
    }

    /// <summary>Construct.</summary>
    public RemoveSelf()
    {
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        return new RemoveSelfState(this, target);
    }
}

/// <summary>Show target. The Target should have "Is Visible" attribute.</summary>
[Reflected]
public partial class Show : AttributeAction
{
    /// <summary>Construct.</summary>
    public Show()
        : base(AttributeActionConstants.IsVisibleAttribute)
    {
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        using var value = new Variant(true);
        return new SetAttributeState(this, target, GetAttribute(target), value);
    }
}

/// <summary>Hide target. The Target should have "Is Visible" attribute.</summary>
[Reflected]
public partial class Hide : AttributeAction
{
    /// <summary>Construct.</summary>
    public Hide()
        : base(AttributeActionConstants.IsVisibleAttribute)
    {
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        using var value = new Variant(false);
        return new SetAttributeState(this, target, GetAttribute(target), value);
    }
}

/// <summary>Show target. The Target should have "Is Enabled" attribute.</summary>
[Reflected]
public partial class Enable : AttributeAction
{
    /// <summary>Construct.</summary>
    public Enable()
        : base(AttributeActionConstants.IsEnabledAttribute)
    {
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        using var value = new Variant(true);
        return new SetAttributeState(this, target, GetAttribute(target), value);
    }
}

/// <summary>Hide target. The Target should have "Is Enabled" attribute.</summary>
[Reflected]
public partial class Disable : AttributeAction
{
    /// <summary>Construct.</summary>
    public Disable()
        : base(AttributeActionConstants.IsEnabledAttribute)
    {
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        using var value = new Variant(false);
        return new SetAttributeState(this, target, GetAttribute(target), value);
    }
}

/// <summary>Blink target by toggling "Is Enabled" attribute. The Target should have "Is Enabled" attribute.</summary>
[Reflected]
public partial class Blink : AttributeAction
{
    /// <summary>Construct.</summary>
    public Blink()
        : base(AttributeActionConstants.IsEnabledAttribute)
    {
    }

    /// <summary>Get number of blinks.</summary>
    public int NumOfBlinks => _times;

    /// <summary>Set number of blinks.</summary>
    public void SetNumOfBlinks(int times)
    {
        _times = Math.Max(1, times);
    }

    /// <summary>Serialize content from/to archive. May throw ArchiveException.</summary>
    public override void SerializeInBlock(Archive archive)
    {
        base.SerializeInBlock(archive);
        ArchiveSerialization.SerializeOptionalValue(
            archive, "times", ref _times, 1, ArchiveSerialization.SerializeValue);
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        using var from = new Variant(false);
        using var to = new Variant(true);
        return new AttributeBlinkState(this, target, GetAttribute(target), from, to, _times);
    }

    protected int _times = 1;
}

/// <summary>Action that does nothing but waits.</summary>
[Reflected]
public partial class DelayTime : FiniteTimeAction
{
    /// <summary>Construct.</summary>
    public DelayTime()
    {
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        return new FiniteTimeActionState(this, target);
    }
}
