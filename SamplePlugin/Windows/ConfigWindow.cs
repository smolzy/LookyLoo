using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly Plugin plugin;

    public ConfigWindow(Plugin plugin)
        : base("LookyLoo - Configuration###LookyLooConfigWindow")
    {
        Flags = ImGuiWindowFlags.NoCollapse;
        Size = new Vector2(360, 480);
        SizeCondition = ImGuiCond.FirstUseEver;

        this.plugin = plugin;
        this.configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 0f);

        if (configuration.IsConfigWindowMovable)
            Flags &= ~ImGuiWindowFlags.NoMove;
        else
            Flags |= ImGuiWindowFlags.NoMove;
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar(7);
    }

    public override void Draw()
    {
        // === Detection ===
        ImGui.TextColored(configuration.TitleColor, "Detection");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text("Maximum detection distance:");
        var maxDist = configuration.MaxDistance;
        if (ImGui.SliderFloat("##MaxDistance", ref maxDist, 5.0f, 200.0f, "%.0f m"))
        {
            configuration.MaxDistance = maxDist;
            configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Spacing();

        // === Behaviour ===
        ImGui.TextColored(configuration.TitleColor, "Behavior");
        ImGui.Separator();
        ImGui.Spacing();

        var openLogin = configuration.OpenOnLogin;
        if (ImGui.Checkbox("Open window on login", ref openLogin))
        {
            configuration.OpenOnLogin = openLogin;
            configuration.Save();
        }

        var autoTarget = configuration.AutoTargetOnLeftClick;
        if (ImGui.Checkbox("Target on left click", ref autoTarget))
        {
            configuration.AutoTargetOnLeftClick = autoTarget;
            configuration.Save();
        }

        var highlight = configuration.HighlightOnHover;
        if (ImGui.Checkbox("Auto-focus on hover", ref highlight))
        {
            configuration.HighlightOnHover = highlight;
            configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Spacing();

        // === List Display Options ===
        ImGui.TextColored(configuration.TitleColor, "List Display Options");
        ImGui.Separator();
        ImGui.Spacing();

        var showJob = configuration.ShowJob;
        if (ImGui.Checkbox("Show Job Abbreviation (e.g. [PLD])", ref showJob)) { configuration.ShowJob = showJob; configuration.Save(); }

        var showLevel = configuration.ShowLevel;
        if (ImGui.Checkbox("Show Level (e.g. Lv.100)", ref showLevel)) { configuration.ShowLevel = showLevel; configuration.Save(); }

        var showWorld = configuration.ShowWorld;
        if (ImGui.Checkbox("Show World / Server (e.g. Moogle)", ref showWorld)) { configuration.ShowWorld = showWorld; configuration.Save(); }

        var showCompany = configuration.ShowCompanyTag;
        if (ImGui.Checkbox("Show Free Company Tag (e.g. <FC>)", ref showCompany)) { configuration.ShowCompanyTag = showCompany; configuration.Save(); }

        var showDistance = configuration.ShowDistance;
        if (ImGui.Checkbox("Show Distance (e.g. 15m)", ref showDistance)) { configuration.ShowDistance = showDistance; configuration.Save(); }

        ImGui.Spacing();
        ImGui.Spacing();

        // === Context Menu Options ===
        ImGui.TextColored(configuration.TitleColor, "Context Menu Options");
        ImGui.Separator();
        ImGui.Spacing();

        var showTarget = configuration.ShowTargetOption;
        if (ImGui.Checkbox("Target Player", ref showTarget)) { configuration.ShowTargetOption = showTarget; configuration.Save(); }

        var showFocus = configuration.ShowFocusTargetOption;
        if (ImGui.Checkbox("Focus Target", ref showFocus)) { configuration.ShowFocusTargetOption = showFocus; configuration.Save(); }

        var showNative = configuration.ShowNativeMenuOption;
        if (ImGui.Checkbox("Open Menu", ref showNative)) { configuration.ShowNativeMenuOption = showNative; configuration.Save(); }

        var showTell = configuration.ShowSendTellOption;
        if (ImGui.Checkbox("Send Tell", ref showTell)) { configuration.ShowSendTellOption = showTell; configuration.Save(); }

        var showInvite = configuration.ShowInvitePartyOption;
        if (ImGui.Checkbox("Invite to Party", ref showInvite)) { configuration.ShowInvitePartyOption = showInvite; configuration.Save(); }

        var showMap = configuration.ShowFindOnMapOption;
        if (ImGui.Checkbox("Find on Map", ref showMap)) { configuration.ShowFindOnMapOption = showMap; configuration.Save(); }

        var showExamine = configuration.ShowExamineOption;
        if (ImGui.Checkbox("Examine", ref showExamine)) { configuration.ShowExamineOption = showExamine; configuration.Save(); }

        var showAdventurePlate = configuration.ShowAdventurePlateOption;
        if (ImGui.Checkbox("View Adventurer Plate", ref showAdventurePlate)) { configuration.ShowAdventurePlateOption = showAdventurePlate; configuration.Save(); }

        var showMoodles = configuration.ShowMoodlesOption;
        if (ImGui.Checkbox("View Moodles (requires Moodles plugin)", ref showMoodles)) { configuration.ShowMoodlesOption = showMoodles; configuration.Save(); }

        var showCopy = configuration.ShowCopyNameOption;
        if (ImGui.Checkbox("Copy Name", ref showCopy)) { configuration.ShowCopyNameOption = showCopy; configuration.Save(); }

        ImGui.Spacing();
        ImGui.Spacing();

        // === Colors ===
        ImGui.TextColored(configuration.TitleColor, "Colors");
        ImGui.Separator();
        ImGui.Spacing();

        var titleColor = configuration.TitleColor;
        if (ImGui.ColorEdit4("Title Color##titleColor", ref titleColor)) { configuration.TitleColor = titleColor; configuration.Save(); }

        var targetingColor = configuration.TargetingMeColor;
        if (ImGui.ColorEdit4("Targeting Me##targetingColor", ref targetingColor)) { configuration.TargetingMeColor = targetingColor; configuration.Save(); }

        var nearbyColor = configuration.NearbyPlayerColor;
        if (ImGui.ColorEdit4("Nearby Player##nearbyColor", ref nearbyColor)) { configuration.NearbyPlayerColor = nearbyColor; configuration.Save(); }

        var unloadedColor = configuration.UnloadedColor;
        if (ImGui.ColorEdit4("Out of Range##unloadedColor", ref unloadedColor)) { configuration.UnloadedColor = unloadedColor; configuration.Save(); }
    }
}
