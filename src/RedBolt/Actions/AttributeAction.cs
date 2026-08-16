// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Actions;

using Microsoft.Extensions.Logging;

using RedBolt.Core;
using RedBolt.IO;
using RedBolt.Logging;
using RedBolt.Scene;

public static class AttributeActionConstants
{
    public const string PositionAttribute = "Position";
    public const string RotationAttribute = "Rotation";
    public const string ScaleAttribute = "Scale";
    public const string IsVisibleAttribute = "Is Visible";
    public const string IsEnabledAttribute = "Is Enabled";
}

/// <summary>Finite time action.</summary>
[Reflected]
public partial class AttributeAction : FiniteTimeAction
{
    /// <summary>Construct.</summary>
    public AttributeAction()
    {
    }

    /// <summary>Construct.</summary>
    public AttributeAction(string animatedAttribute)
    {
        _animatedAttribute = animatedAttribute;
    }

    /// <summary>Serialize content from/to archive. May throw ArchiveException.</summary>
    public override void SerializeInBlock(Archive archive)
    {
        base.SerializeInBlock(archive);
        ArchiveSerialization.SerializeOptionalValue(
            archive, "attribute", ref _animatedAttribute, string.Empty, ArchiveSerialization.SerializeValue);
    }

    /// <summary>Set action duration.</summary>
    public void SetAttributeName(string animatedAttribute)
    {
        _animatedAttribute = animatedAttribute;
    }

    /// <summary>Get action duration.</summary>
    public virtual string AttributeName => _animatedAttribute;

    protected AttributeInfo? GetAttribute(RedObject? target)
    {
        var serializable = target as Serializable;
        if (serializable is null)
        {
            Log.GetLogger().LogError(
                "Can animate only serializable class but {TypeName} is not serializable.", target!.GetTypeName());
            return null;
        }

        var attribute = target!.Context.GetReflection(target.GetTypeNameHash())?.GetAttribute(_animatedAttribute);
        if (attribute is null)
            Log.GetLogger().LogError("Attribute {Attribute} not found in {TypeName}.", _animatedAttribute, target.GetTypeName());
        return attribute;
    }

    private string _animatedAttribute = string.Empty;
}
