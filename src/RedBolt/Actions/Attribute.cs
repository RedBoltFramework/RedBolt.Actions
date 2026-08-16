// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Actions;

using System;

using RedBolt.Core;
using RedBolt.IO;
using RedBolt.Scene;

/// <summary>Animate attribute between two values.</summary>
[Reflected]
public partial class AttributeFromTo : AttributeAction
{
    private sealed class AttributeFromToState : AttributeActionState
    {
        private Variant _from;
        private Variant _to;

        public AttributeFromToState(AttributeFromTo action, RedObject? target, AttributeInfo? attribute)
            : base(action, target, attribute)
        {
            _from = action.From.Clone();
            _to = action.To.Clone();
        }

        public override void Dispose()
        {
            _to.Dispose();
            _from.Dispose();
            base.Dispose();
        }

        protected override void Update(float time, ref Variant value)
        {
            var result = _from.Lerp(_to, time);
            value.Dispose();
            value = result;
        }
    }

    /// <summary>Construct.</summary>
    public AttributeFromTo()
    {
    }

    // Set "from" value.
    public void SetFrom(Variant variant)
    {
        var value = variant.Clone();
        _from.Dispose();
        _from = value;
    }

    // Get "to" value.
    public void SetTo(Variant variant)
    {
        var value = variant.Clone();
        _to.Dispose();
        _to = value;
    }

    // Get "from" value.
    public ref readonly Variant From => ref _from;

    // Get "to" value.
    public ref readonly Variant To => ref _to;

    /// <summary>Create reversed action.</summary>
    public override FiniteTimeAction Reverse()
    {
        var result = new AttributeFromTo();
        result.SetDuration(Duration);
        result.SetAttributeName(AttributeName);
        result.SetFrom(_to);
        result.SetTo(_from);
        return result;
    }

    /// <summary>Serialize content from/to archive. May throw ArchiveException.</summary>
    public override void SerializeInBlock(Archive archive)
    {
        base.SerializeInBlock(archive);
        ArchiveSerialization.SerializeOptionalValue(
            archive, "from", ref _from, Variant.Empty, ArchiveSerialization.SerializeValue);
        ArchiveSerialization.SerializeOptionalValue(
            archive, "to", ref _to, Variant.Empty, ArchiveSerialization.SerializeValue);
    }

    public override void Dispose()
    {
        _to.Dispose();
        _from.Dispose();
        base.Dispose();
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        return new AttributeFromToState(this, target, GetAttribute(target));
    }

    private Variant _from;
    private Variant _to;
}

/// <summary>Animate attribute between current and provided value.</summary>
[Reflected]
public partial class AttributeTo : AttributeAction
{
    private sealed class AttributeToState : AttributeActionState
    {
        private Variant _from;
        private Variant _to;

        public AttributeToState(AttributeTo action, RedObject? target, AttributeInfo? attribute)
            : base(action, target, attribute)
        {
            _to = action.To.Clone();
            if (_attribute is not null)
            {
                _attribute.Accessor!.Get((Serializable)target!, ref _from);
            }
        }

        public override void Dispose()
        {
            _to.Dispose();
            _from.Dispose();
            base.Dispose();
        }

        protected override void Update(float time, ref Variant value)
        {
            var result = _from.Lerp(_to, time);
            value.Dispose();
            value = result;
        }
    }

    /// <summary>Construct.</summary>
    public AttributeTo()
    {
    }

    // Get "to" value.
    public void SetTo(Variant variant)
    {
        var value = variant.Clone();
        _to.Dispose();
        _to = value;
    }

    // Get "to" value.
    public ref readonly Variant To => ref _to;

    /// <summary>Serialize content from/to archive. May throw ArchiveException.</summary>
    public override void SerializeInBlock(Archive archive)
    {
        base.SerializeInBlock(archive);
        ArchiveSerialization.SerializeOptionalValue(
            archive, "to", ref _to, Variant.Empty, ArchiveSerialization.SerializeValue);
    }

    public override void Dispose()
    {
        _to.Dispose();
        base.Dispose();
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        return new AttributeToState(this, target, GetAttribute(target));
    }

    private Variant _to;
}

/// <summary>Animate attribute between two values.</summary>
[Reflected]
public partial class AttributeBlink : AttributeAction
{
    /// <summary>Construct.</summary>
    public AttributeBlink()
    {
    }

    // Set "from" value.
    public void SetFrom(Variant variant)
    {
        var value = variant.Clone();
        _from.Dispose();
        _from = value;
    }

    // Get "to" value.
    public void SetTo(Variant variant)
    {
        var value = variant.Clone();
        _to.Dispose();
        _to = value;
    }

    /// <summary>Set number of blinks.</summary>
    public void SetNumOfBlinks(int times)
    {
        _times = Math.Max(1, times);
    }

    // Get "from" value.
    public ref readonly Variant From => ref _from;

    // Get "to" value.
    public ref readonly Variant To => ref _to;

    /// <summary>Get number of blinks.</summary>
    public int NumOfBlinks => _times;

    /// <summary>Create reversed action.</summary>
    public override FiniteTimeAction Reverse()
    {
        var result = new AttributeFromTo();
        result.SetDuration(Duration);
        result.SetAttributeName(AttributeName);
        result.SetFrom(_to);
        result.SetTo(_from);
        return result;
    }

    /// <summary>Serialize content from/to archive. May throw ArchiveException.</summary>
    public override void SerializeInBlock(Archive archive)
    {
        base.SerializeInBlock(archive);
        ArchiveSerialization.SerializeOptionalValue(
            archive, "from", ref _from, Variant.Empty, ArchiveSerialization.SerializeValue);
        ArchiveSerialization.SerializeOptionalValue(
            archive, "to", ref _to, Variant.Empty, ArchiveSerialization.SerializeValue);
    }

    public override void Dispose()
    {
        _to.Dispose();
        _from.Dispose();
        base.Dispose();
    }

    /// <summary>Create new action state from the action.</summary>
    protected internal override ActionState StartAction(RedObject? target)
    {
        return new AttributeBlinkState(this, target, GetAttribute(target), _from, _to, _times);
    }

    private Variant _from;
    private Variant _to;
    private int _times = 1;
}
