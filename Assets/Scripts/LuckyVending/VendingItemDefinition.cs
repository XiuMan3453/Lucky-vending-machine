using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class VendingItemDefinition
{
    public string id;
    public string displayName;
    public string shortCode;
    public string rarity;
    public int baseValue;
    public string[] tags;
    public Color32 color;

    public VendingItemDefinition(string id, string displayName, string shortCode, string rarity, int baseValue, string[] tags, Color32 color)
    {
        this.id = id;
        this.displayName = displayName;
        this.shortCode = shortCode;
        this.rarity = rarity;
        this.baseValue = baseValue;
        this.tags = tags;
        this.color = color;
    }

    public bool HasTag(string tag)
    {
        return tags != null && tags.Contains(tag);
    }
}
