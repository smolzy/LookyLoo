using Dalamud.Game.Command;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using SamplePlugin.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Lumina.Excel.Sheets;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IContextMenu ContextMenu { get; private set; } = null!;

    private const string CommandName = "/looky";
    private const string CommandNameConfig = "/lookyconfig";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("LookyLoo");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    // === Viewer tracking (inspired by Peeping Tim) ===
    // Key = "Name@World"
    private readonly Dictionary<string, NearbyPlayerInfo> nearbyPlayers = new();

    // World name cache
    private readonly Dictionary<uint, string> worldNames = new();

    // Throttle update
    private long lastUpdateTick = 0;
    private const int UpdateIntervalMs = 150;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Cache world names from Lumina
        var worldSheet = DataManager.GetExcelSheet<World>();
        if (worldSheet != null)
        {
            foreach (var world in worldSheet)
            {
                worldNames[world.RowId] = world.Name.ExtractText();
            }
        }

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle the LookyLoo window (/looky)"
        });
        CommandManager.AddHandler(CommandNameConfig, new CommandInfo(OnConfig)
        {
            HelpMessage = "Open LookyLoo configuration (/lookyconfig)"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        // Hook into the game's native context menu
        ContextMenu.OnMenuOpened += OnNativeMenuOpened;

        // Hook into framework update for background scanning
        Framework.Update += OnFrameworkUpdate;

        Log.Information($"[LookyLoo] Plugin loaded.");
    }

    public void Dispose()
    {
        Framework.Update -= OnFrameworkUpdate;
        ContextMenu.OnMenuOpened -= OnNativeMenuOpened;

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        CommandManager.RemoveHandler(CommandNameConfig);
    }

    // === Framework Update - background scan every 150ms ===
    private void OnFrameworkUpdate(IFramework framework)
    {
        long currentTick = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (currentTick - lastUpdateTick < UpdateIntervalMs)
            return;
        lastUpdateTick = currentTick;

        var localPlayer = ObjectTable.LocalPlayer;
        if (localPlayer == null) return;

        ulong localId = localPlayer.GameObjectId;
        var foundKeys = new HashSet<string>();

        foreach (var obj in ObjectTable)
        {
            if (obj is IPlayerCharacter pc && pc.GameObjectId != localId)
            {
                float dist = System.Numerics.Vector3.Distance(localPlayer.Position, pc.Position);
                if (dist > Configuration.MaxDistance)
                    continue;

                string key = GetPlayerKey(pc);
                foundKeys.Add(key);
                bool targetingMe = pc.TargetObjectId == localId;

                if (!nearbyPlayers.TryGetValue(key, out var info))
                {
                    info = CreatePlayerInfo(pc, dist, targetingMe);
                    nearbyPlayers[key] = info;
                }
                else
                {
                    info.IsActive = targetingMe;
                    info.Distance = dist;
                    info.LastSeen = DateTime.Now;
                    info.IsLoaded = true;
                    info.GameObjectId = pc.GameObjectId;
                    info.JobId = pc.ClassJob.IsValid ? pc.ClassJob.RowId : 0;
                    info.JobAbbreviation = pc.ClassJob.IsValid ? pc.ClassJob.Value.Abbreviation.ToString() : "?";

                    unsafe
                    {
                        var chara = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)pc.Address;
                        if (chara != null)
                        {
                            var foray = chara->GetForayInfo();
                            info.Level = (foray != null && foray->Level != 0) ? foray->Level : pc.Level;
                            info.CompanyTag = chara->FreeCompanyTagString;
                            info.IsFriend = chara->IsFriend;
                            info.IsPartyMember = chara->IsPartyMember;
                        }
                    }

                    if (targetingMe)
                    {
                        info.HasEverTargetedMe = true;
                        info.LastTargetedMeTime = DateTime.Now;
                    }
                }

                // Keep track of who has targeted us
                info.WasTargetingMe = targetingMe;
            }
        }

        // Mark players no longer in range as inactive
        foreach (var kv in nearbyPlayers)
        {
            if (!foundKeys.Contains(kv.Key))
            {
                kv.Value.IsActive = false;
                kv.Value.IsLoaded = false;
                kv.Value.WasTargetingMe = false;
            }
        }

        // Remove players not seen for > 5 minutes (keep history up to 60 mins for those who targeted us)
        var toRemove = nearbyPlayers
            .Where(kv => !kv.Value.IsLoaded && (DateTime.Now - kv.Value.LastSeen).TotalMinutes > (kv.Value.HasEverTargetedMe ? 60 : 5))
            .Select(kv => kv.Key)
            .ToList();

        foreach (var k in toRemove)
            nearbyPlayers.Remove(k);
    }

    // === Native context menu integration (like Peeping Tim) ===
    private void OnNativeMenuOpened(IMenuOpenedArgs args)
    {
        if (args.MenuType == ContextMenuType.Inventory)
            return;

        try
        {
            if (args.Target is MenuTargetDefault target &&
                target.TargetObject is IPlayerCharacter pc)
            {
                args.AddMenuItem(new MenuItem()
                {
                    Name = "View in LookyLoo",
                    Prefix = SeIconChar.BoxedLetterL,
                    OnClicked = _ => OpenAndSelectPlayer(pc),
                    Priority = 50
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Ultimate Sight] Context menu error: {ex.Message}");
        }
    }

    private void OpenAndSelectPlayer(IPlayerCharacter pc)
    {
        MainWindow.IsOpen = true;
        string key = GetPlayerKey(pc);

        if (!nearbyPlayers.TryGetValue(key, out var info))
        {
            float dist = 0;
            var local = ObjectTable.LocalPlayer;
            if (local != null) dist = System.Numerics.Vector3.Distance(local.Position, pc.Position);

            info = CreatePlayerInfo(pc, dist, false);
            nearbyPlayers[key] = info;
        }

        MainWindow.SetSelectedPlayer(key);
    }

    // === Public API for MainWindow ===

    public List<NearbyPlayerInfo> GetNearbyPlayers()
    {
        return nearbyPlayers.Values
            .OrderByDescending(p => p.IsActive)
            .ThenBy(p => p.Distance)
            .ToList();
    }

    public List<NearbyPlayerInfo> GetViewersTargetingMe()
    {
        return nearbyPlayers.Values
            .Where(p => p.HasEverTargetedMe)
            .OrderByDescending(p => p.IsActive)
            .ThenByDescending(p => p.LastTargetedMeTime)
            .ToList();
    }

    public void TargetPlayer(NearbyPlayerInfo info)
    {
        var obj = ObjectTable.SearchById(info.GameObjectId);
        if (obj != null)
        {
            TargetManager.Target = obj;
        }
        else
        {
            ChatGui.PrintError($"[Ultimate Sight] Impossible de cibler {info.Name}.");
        }
    }

    public void FocusTargetPlayer(NearbyPlayerInfo info)
    {
        var obj = ObjectTable.SearchById(info.GameObjectId);
        if (obj != null)
        {
            if (info.IsFocused)
            {
                TargetManager.FocusTarget = null;
            }
            else
            {
                TargetManager.FocusTarget = obj;
            }
            info.IsFocused = !info.IsFocused;
        }
    }

    public void OpenNativeContextMenu(NearbyPlayerInfo info)
    {
        var obj = ObjectTable.SearchById(info.GameObjectId);
        if (obj is IPlayerCharacter pc)
        {
            unsafe
            {
                Framework.RunOnTick(() =>
                {
                    FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentHUD.Instance()->OpenContextMenuFromTarget(
                        (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)pc.Address
                    );
                });
            }
        }
    }

    public void OpenExamine(NearbyPlayerInfo info)
    {
        var obj = ObjectTable.SearchById(info.GameObjectId);
        if (obj is IPlayerCharacter pc)
        {
            unsafe
            {
                Framework.RunOnTick(() =>
                {
                    FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentInspect.Instance()->ExamineCharacter((uint)pc.GameObjectId);
                });
            }
        }
    }

    public void OpenAdventurePlate(NearbyPlayerInfo info)
    {
        var obj = ObjectTable.SearchById(info.GameObjectId);
        if (obj is IPlayerCharacter pc)
        {
            unsafe
            {
                Framework.RunOnTick(() =>
                {
                    // Note: OpenCharaCard usually takes ContentId (ulong) if we have it, or GameObject*
                    // PeepingTim uses: AgentCharaCard.Instance()->OpenCharaCard(pc.Struct());
                    FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentCharaCard.Instance()->OpenCharaCard(
                        (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)pc.Address
                    );
                });
            }
        }
    }

    public void SendChatCommand(string command, NearbyPlayerInfo info)
    {
        string fullCommand = $"/{command} {info.Name}@{info.World}";
        CommandManager.ProcessCommand(fullCommand);
    }

    // === Helpers ===

    public string GetWorldName(uint rowId)
    {
        return worldNames.TryGetValue(rowId, out var name) ? name : "Unknown";
    }

    public string GetPlayerKey(IPlayerCharacter pc)
    {
        return $"{pc.Name.TextValue}@{GetWorldName(pc.HomeWorld.RowId)}";
    }

    private NearbyPlayerInfo CreatePlayerInfo(IPlayerCharacter pc, float distance, bool targetingMe)
    {
        byte level = pc.Level;
        string companyTag = string.Empty;
        bool isFriend = false;
        bool isParty = false;

        unsafe
        {
            var chara = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)pc.Address;
            if (chara != null)
            {
                var foray = chara->GetForayInfo();
                if (foray != null && foray->Level != 0)
                    level = foray->Level;

                companyTag = chara->FreeCompanyTagString;
                isFriend = chara->IsFriend;
                isParty = chara->IsPartyMember;
            }
        }

        return new NearbyPlayerInfo
        {
            Name = pc.Name.TextValue,
            World = GetWorldName(pc.HomeWorld.RowId),
            JobId = pc.ClassJob.IsValid ? pc.ClassJob.RowId : 0,
            JobAbbreviation = pc.ClassJob.IsValid ? pc.ClassJob.Value.Abbreviation.ToString() : "?",
            Level = level,
            CompanyTag = companyTag,
            IsFriend = isFriend,
            IsPartyMember = isParty,
            Distance = distance,
            IsActive = targetingMe,
            WasTargetingMe = targetingMe,
            HasEverTargetedMe = targetingMe,
            LastTargetedMeTime = targetingMe ? DateTime.Now : null,
            IsLoaded = true,
            IsFocused = false,
            FirstSeen = DateTime.Now,
            LastSeen = DateTime.Now,
            GameObjectId = pc.GameObjectId
        };
    }

    // === Commands ===
    private void OnCommand(string command, string args) => ToggleMainUi();
    private void OnConfig(string command, string args) => ToggleConfigUi();

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();

    // === Data Model ===
    public class NearbyPlayerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string World { get; set; } = string.Empty;
        public uint JobId { get; set; }
        public string JobAbbreviation { get; set; } = string.Empty;
        public byte Level { get; set; }
        public string CompanyTag { get; set; } = string.Empty;
        public bool IsFriend { get; set; }
        public bool IsPartyMember { get; set; }
        public float Distance { get; set; }

        /// <summary>True if currently targeting the local player.</summary>
        public bool IsActive { get; set; }

        /// <summary>Used to detect NEW targeting events for alerts.</summary>
        public bool WasTargetingMe { get; set; }
        
        /// <summary>History tracking.</summary>
        public bool HasEverTargetedMe { get; set; }
        public DateTime? LastTargetedMeTime { get; set; }
        
        public bool IsLoaded { get; set; }
        public bool IsFocused { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public ulong GameObjectId { get; set; }
    }
}
