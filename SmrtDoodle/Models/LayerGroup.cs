using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SmrtDoodle.Models;

/// <summary>
/// A logical folder/group for organizing layers in the panel.
/// Groups support nested visibility toggle, blend mode, and collapse/expand.
/// </summary>
public class LayerGroup
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsExpanded { get; set; } = true;
    public float Opacity { get; set; } = 1.0f;
    public BlendMode BlendMode { get; set; } = BlendMode.Normal;

    /// <summary>
    /// Ordered child layer IDs belonging to this group. Order matches display order.
    /// </summary>
    public ObservableCollection<string> ChildLayerIds { get; } = new();

    public LayerGroup(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Toggle visibility for the group. When a group is hidden, all child layers
    /// should also be hidden during rendering (regardless of their individual visibility).
    /// </summary>
    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
    }

    public override string ToString() => $"📁 {Name} ({ChildLayerIds.Count} layers)";
}
