using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SamplePlugin.Plugin;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private string searchFilter = string.Empty;
    private int selectedTab = 0;
    private string? selectedPlayerKey = null;

    // Aetherlove Colors
    private static readonly Vector4 ColorBody = new(0.85f, 0.85f, 0.85f, 1f);
    private static readonly Vector4 ColorSubtle = new(0.70f, 0.70f, 0.74f, 1f);
    private static readonly Vector4 ColorLiveGreen = new(0.35f, 0.85f, 0.45f, 1f);
    private static readonly Vector4 ColorWindowBg = new(0.08f, 0.08f, 0.09f, 0.95f);
    private static readonly Vector4 ColorCardBg = new(0.15f, 0.15f, 0.16f, 1f);
    private static readonly Vector4 ColorCardHover = new(0.20f, 0.20f, 0.22f, 1f);
    private static readonly Vector4 ColorCardActive = new(0.25f, 0.25f, 0.28f, 1f);

    public MainWindow(Plugin plugin)
        : base("LookyLoo", ImGuiWindowFlags.NoScrollbar)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(180, 200),
            MaximumSize = new Vector2(800, float.MaxValue)
        };

        Size = new Vector2(200, 320);
        SizeCondition = ImGuiCond.FirstUseEver;
        BgAlpha = 0.95f;

        this.plugin = plugin;
    }

    public void Dispose() { }

    public void SetSelectedPlayer(string key) => selectedPlayerKey = key;

    public override void PreDraw()
    {
        // Square UI styling (0 rounding)
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8f, 8f));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 0f);
        
        ImGui.PushStyleColor(ImGuiCol.WindowBg, ColorWindowBg);
        ImGui.PushStyleColor(ImGuiCol.Text, ColorBody);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, ColorCardBg);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, ColorCardHover);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, ColorCardActive);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor(5);
        ImGui.PopStyleVar(8);
    }

    public override void Draw()
    {
        var config = plugin.Configuration;
        var allPlayers = plugin.GetNearbyPlayers();
        int targetingMeCount = allPlayers.Count(p => p.IsActive);

        // Search bar and settings
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 28f * ImGuiHelpers.GlobalScale);
        ImGui.InputTextWithHint("##SearchFilter", "Search...", ref searchFilter, 64);
        
        ImGui.SameLine();
        if (Dalamud.Interface.Components.ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
            plugin.ToggleConfigUi();
            
        ImGui.Spacing();

        // Modern Tabs (Custom drawn or heavily styled)
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 4f));
        using (var tabBar = ImRaii.TabBar("LookyLooTabs##tabs", ImGuiTabBarFlags.NoTooltip))
        {
            if (tabBar.Success)
            {
                if (ImGui.BeginTabItem("All"))
                {
                    selectedTab = 0;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Targeting Me"))
                {
                    selectedTab = 1;
                    ImGui.EndTabItem();
                }
            }
        }
        ImGui.PopStyleVar();

        var filteredList = allPlayers.Where(p =>
        {
            if (selectedTab == 1 && !p.HasEverTargetedMe) return false;
            if (string.IsNullOrWhiteSpace(searchFilter)) return true;

            return p.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                || p.World.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                || p.JobAbbreviation.Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrEmpty(p.CompanyTag) && p.CompanyTag.Contains(searchFilter, StringComparison.OrdinalIgnoreCase));
        }).ToList();

        // Sort history by time for the Targeting Me tab
        if (selectedTab == 1)
        {
            filteredList = filteredList
                .OrderByDescending(p => p.IsActive)
                .ThenByDescending(p => p.LastTargetedMeTime)
                .ToList();
        }

        // Custom Player Cards List
        using var listChild = ImRaii.Child("PlayerList", new Vector2(0, 0), false, ImGuiWindowFlags.None);
        if (!listChild.Success) return;

        // Get the DRAW LIST for the CHILD window so clipping works correctly!
        var childDrawList = ImGui.GetWindowDrawList();

        float cardHeight = 28f * ImGuiHelpers.GlobalScale;
        float rounding = 0f; // Square / No rounding

        foreach (var player in filteredList)
        {
            string rowKey = $"{player.Name}@{player.World}";
            bool isCurrentTarget = Plugin.TargetManager.Target?.GameObjectId == player.GameObjectId;
            bool isSelected = selectedPlayerKey == rowKey;
            
            var cursorPos = ImGui.GetCursorScreenPos();
            var availWidth = ImGui.GetContentRegionAvail().X;
            var rectMin = cursorPos;
            var rectMax = new Vector2(rectMin.X + availWidth, rectMin.Y + cardHeight);
            
            // Invisible button for interaction
            ImGui.InvisibleButton($"##btn_{rowKey}", new Vector2(availWidth, cardHeight));
            bool isHovered = ImGui.IsItemHovered();
            bool isActive = ImGui.IsItemActive();
            
            // Interaction logic
            if (isHovered && config.HighlightOnHover && player.IsLoaded)
            {
                foreach (var p in allPlayers)
                {
                    if (p.IsFocused && ($"{p.Name}@{p.World}" != rowKey))
                        plugin.FocusTargetPlayer(p);
                }
                if (!player.IsFocused)
                    plugin.FocusTargetPlayer(player);
            }
            else if (!isHovered && player.IsFocused)
            {
                plugin.FocusTargetPlayer(player);
            }
            
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && config.AutoTargetOnLeftClick)
            {
                plugin.TargetPlayer(player);
                selectedPlayerKey = rowKey;
            }
            
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup($"ContextMenu_{rowKey}");
                selectedPlayerKey = rowKey;
            }

            // Draw Card Background
            Vector4 bgCol = ColorCardBg;
            if (isActive) bgCol = ColorCardActive;
            else if (isHovered) bgCol = ColorCardHover;
            else if (isSelected) bgCol = ColorCardActive;

            childDrawList.AddRectFilled(rectMin, rectMax, ImGui.ColorConvertFloat4ToU32(bgCol), rounding);
            
            // Draw Target Indicator (Green square) if targeting me
            var textPos = new Vector2(rectMin.X + 12f, rectMin.Y + 8f);
            if (player.IsActive)
            {
                var sqMin = new Vector2(rectMin.X + 12f, rectMin.Y + (cardHeight - 7f) / 2f);
                var sqMax = new Vector2(rectMin.X + 19f, rectMin.Y + (cardHeight + 7f) / 2f);
                childDrawList.AddRectFilled(sqMin, sqMax, ImGui.ColorConvertFloat4ToU32(ColorLiveGreen), 0f);
                textPos.X += 14f;
            }
            else if (isCurrentTarget)
            {
                var sqMin = new Vector2(rectMin.X + 12f, rectMin.Y + (cardHeight - 7f) / 2f);
                var sqMax = new Vector2(rectMin.X + 19f, rectMin.Y + (cardHeight + 7f) / 2f);
                childDrawList.AddRectFilled(sqMin, sqMax, 0xFF00D8FFu, 0f); // Gold/Yellow
                textPos.X += 14f;
            }

            // Draw Name
            Vector4 nameColor = ColorBody;
            if (player.IsActive || isCurrentTarget) nameColor = new Vector4(1, 1, 1, 1);
            else if (player.HasEverTargetedMe) nameColor = ColorSubtle; // Grayed out for history
            if (!player.IsLoaded) nameColor = config.UnloadedColor;
            
            // Adjust vertical centering
            textPos.Y = rectMin.Y + (cardHeight - ImGui.GetFontSize()) / 2f;
            childDrawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(nameColor), player.Name);
            
            float curX = textPos.X + ImGui.CalcTextSize(player.Name).X;
            uint subtleColorU32 = ImGui.ColorConvertFloat4ToU32(ColorSubtle);

            if (config.ShowLevel && player.Level > 0)
            {
                string lvlStr = $" Lv.{player.Level}";
                childDrawList.AddText(new Vector2(curX, textPos.Y), subtleColorU32, lvlStr);
                curX += ImGui.CalcTextSize(lvlStr).X;
            }

            if (config.ShowJob && !string.IsNullOrEmpty(player.JobAbbreviation))
            {
                string jobStr = $" [{player.JobAbbreviation}]";
                childDrawList.AddText(new Vector2(curX, textPos.Y), subtleColorU32, jobStr);
                curX += ImGui.CalcTextSize(jobStr).X;
            }

            if (config.ShowCompanyTag && !string.IsNullOrEmpty(player.CompanyTag))
            {
                string fcStr = $" <{player.CompanyTag}>";
                childDrawList.AddText(new Vector2(curX, textPos.Y), subtleColorU32, fcStr);
                curX += ImGui.CalcTextSize(fcStr).X;
            }

            if (config.ShowWorld && !string.IsNullOrEmpty(player.World))
            {
                string worldStr = $" @{player.World}";
                childDrawList.AddText(new Vector2(curX, textPos.Y), subtleColorU32, worldStr);
                curX += ImGui.CalcTextSize(worldStr).X;
            }

            // Draw right side info (Distance and/or History Time)
            string rightStr = string.Empty;
            if (config.ShowDistance && player.IsLoaded)
            {
                rightStr = $"{player.Distance:0}m";
            }

            if (!player.IsActive && player.HasEverTargetedMe && player.LastTargetedMeTime.HasValue)
            {
                string timeStr = player.LastTargetedMeTime.Value.ToString("HH:mm");
                rightStr = string.IsNullOrEmpty(rightStr) ? timeStr : $"{rightStr}  {timeStr}";
            }

            if (!string.IsNullOrEmpty(rightStr))
            {
                var rightSize = ImGui.CalcTextSize(rightStr);
                var rightPos = new Vector2(rectMax.X - rightSize.X - 10f, textPos.Y);
                childDrawList.AddText(rightPos, subtleColorU32, rightStr);
            }

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2f); // Compact spacing

            DrawContextMenu(plugin, player, rowKey, config);
        }
    }

    private static void DrawContextMenu(Plugin plugin, NearbyPlayerInfo player, string rowKey, Configuration config)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(10, 6));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, ColorWindowBg);
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.25f, 0.25f, 0.28f, 1f));
        
        if (!ImGui.BeginPopup($"ContextMenu_{rowKey}"))
        {
            ImGui.PopStyleColor(2);
            ImGui.PopStyleVar(5);
            return;
        }

        ImGui.TextColored(new Vector4(1,1,1,1), $"{player.Name}");
        ImGui.TextColored(ColorSubtle, $"seen at {player.LastSeen:HH:mm}");
        ImGui.Separator();
        ImGui.Spacing();

        if (config.ShowTargetOption && ImGui.Selectable("Target Player"))
            plugin.TargetPlayer(player);

        if (config.ShowFocusTargetOption && player.IsLoaded)
        {
            string focusLabel = player.IsFocused ? "Remove Focus Target" : "Set Focus Target";
            if (ImGui.Selectable(focusLabel))
                plugin.FocusTargetPlayer(player);
        }

        if (config.ShowNativeMenuOption && player.IsLoaded)
        {
            if (ImGui.Selectable("Native Subcommand"))
                plugin.OpenNativeContextMenu(player);
        }

        if (config.ShowExamineOption && player.IsLoaded)
        {
            if (ImGui.Selectable("Examine"))
                plugin.OpenExamine(player);
        }

        if (config.ShowAdventurePlateOption && player.IsLoaded)
        {
            if (ImGui.Selectable("View Adventurer Plate"))
                plugin.OpenAdventurePlate(player);
        }

        ImGui.Separator();

        if (config.ShowCopyNameOption && ImGui.Selectable("Copy Name"))
        {
            ImGui.SetClipboardText(player.Name);
            Plugin.ChatGui.Print($"[LookyLoo] Name copied: {player.Name}");
        }

        ImGui.EndPopup();
        ImGui.PopStyleColor(2);
        ImGui.PopStyleVar(5);
    }
}
