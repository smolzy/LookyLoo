using Dalamud.Configuration;
using System;
using System.Numerics;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;

    // Detection settings
    public float MaxDistance { get; set; } = 100.0f;

    // Notification / Behavior settings
    public bool OpenOnLogin { get; set; } = true;
    public bool AutoTargetOnLeftClick { get; set; } = true;
    public bool HighlightOnHover { get; set; } = true;

    // List Display Toggles
    public bool ShowJob { get; set; } = true;
    public bool ShowLevel { get; set; } = false;
    public bool ShowWorld { get; set; } = false;
    public bool ShowCompanyTag { get; set; } = false;
    public bool ShowDistance { get; set; } = false;

    // Right-click context menu options (shown/hidden)
    public bool ShowTargetOption { get; set; } = true;
    public bool ShowFocusTargetOption { get; set; } = true;
    public bool ShowNativeMenuOption { get; set; } = true;
    public bool ShowExamineOption { get; set; } = true;
    public bool ShowAdventurePlateOption { get; set; } = true;
    public bool ShowMoodlesOption { get; set; } = true;
    public bool ShowCopyNameOption { get; set; } = true;

    // Color settings (inspired by Peeping Tim)
    public Vector4 TitleColor { get; set; } = new Vector4(0.6f, 0.8f, 1.0f, 1.0f);
    public Vector4 TargetingMeColor { get; set; } = new Vector4(0.04f, 0.96f, 0.18f, 1.0f);
    public Vector4 NearbyPlayerColor { get; set; } = new Vector4(1f, 1f, 1f, 1f);
    public Vector4 UnloadedColor { get; set; } = new Vector4(0.5f, 0.5f, 0.5f, 1f);

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
