// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Actions;

using RedBolt.Core;
using RedBolt.IO;

/// <summary>Finite time action.</summary>
[Reflected]
public partial class FiniteTimeAction : BaseAction
{
    private sealed class NoAction : FiniteTimeAction
    {
        /// <summary>Construct.</summary>
        public NoAction(FiniteTimeAction reversed)
        {
            _reversed = reversed;
        }

        /// <summary>Destruct.</summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>Create reversed action.</summary>
        public override FiniteTimeAction Reverse() => _reversed;

        private readonly FiniteTimeAction _reversed;
    }

    /// <summary>Construct.</summary>
    public FiniteTimeAction()
    {
    }

    /// <summary>Set action duration.</summary>
    public void SetDuration(float duration)
    {
        // Prevent division by 0
        if (duration < 1.1920929E-07f)
            duration = 1.1920929E-07f;

        _duration = duration;
    }

    /// <summary>Get action duration.</summary>
    public virtual float Duration => _duration;

    /// <summary>Serialize content from/to archive. May throw ArchiveException.</summary>
    public override void SerializeInBlock(Archive archive)
    {
        base.SerializeInBlock(archive);
        ArchiveSerialization.SerializeOptionalValue(
            archive, "duration", ref _duration, 1.1920929E-07f, ArchiveSerialization.SerializeValue);
    }

    /// <summary>Get action from argument or empty action.</summary>
    public FiniteTimeAction GetOrDefault(FiniteTimeAction? action)
    {
        if (action is not null)
            return action;
        return GetSubsystem<ActionManager>().GetEmptyAction();
    }

    /// <summary>Create reversed action.</summary>
    public virtual FiniteTimeAction Reverse() => new NoAction(this);

    private float _duration = 1.1920929E-07f;
}
