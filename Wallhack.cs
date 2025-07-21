using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using System.Linq;

namespace Wallhack;

[MinimumApiVersion(80)]
public class Wallhack : BasePlugin, IPluginConfig<WallhackConfig>
{
    public override string ModuleName => "Always On Wallhack";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "ServerAdmin";
    public override string ModuleDescription => "Wallhack automatisch für alle Spieler";

    public WallhackConfig Config { get; set; } = new();

    public void OnConfigParsed(WallhackConfig config)
    {
        Config = config;
        Console.WriteLine($"[Wallhack] Config loaded - Plugin enabled: {config.EnablePlugin}");
    }

    public override void Load(bool hotReload)
    {
        if (!Config.EnablePlugin)
        {
            Console.WriteLine("[Wallhack] Wallhack plugin is disabled");
            return;
        }

        Console.WriteLine("[Wallhack] Always-On Wallhack plugin loaded");
        Console.WriteLine("[Wallhack] Using Entity-Flags method for wallhack functionality");
        
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        
        // Kontinuierliche Updates für bessere Sichtbarkeit
        RegisterListener<Listeners.OnTick>(() => 
        {
            if (Config.EnablePlugin)
            {
                ApplyWallhackEffects();
            }
        });
    }

    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        // Wallhack-Effekte beim Spawn aktivieren
        AddTimer(0.1f, () => ApplyWallhackEffects());
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        // Wallhack-Effekte bei Rundenstart erneuern
        AddTimer(0.1f, () => ApplyWallhackEffects());
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        // Cleanup bei Tod
        var deadPlayer = @event.Userid;
        if (deadPlayer?.PlayerPawn?.Value != null)
        {
            AddTimer(0.1f, () => {
                var pawn = deadPlayer.PlayerPawn.Value;
                if (pawn?.IsValid == true)
                {
                    ResetPlayerEffects(pawn);
                }
            });
        }
        return HookResult.Continue;
    }

    private void ApplyWallhackEffects()
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid);

        foreach (var player in allPlayers)
        {
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn?.IsValid != true) continue;

            try 
            {
                // Intensivere Farben für bessere Sichtbarkeit
                var glowColor = player.Team == CsTeam.Terrorist
                    ? Color.FromArgb(255, 255, 0, 0)    // Vollständig rot für T
                    : Color.FromArgb(255, 0, 0, 255);   // Vollständig blau für CT

                // Basis-Rendering setzen
                playerPawn.Render = glowColor;
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");

                // Entity-Flags für Wallhack setzen
                ApplyEntityFlags(playerPawn, player.PlayerName);
                
                // Rendering-Mode für bessere Sichtbarkeit
                ApplyRenderingMode(playerPawn, player.PlayerName);

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[Wallhack] Effect Error for {player.PlayerName}: {ex.Message}");
            }
        }

        if (Config.LogWallhackUsage && Config.DebugMode)
        {
            Console.WriteLine($"[Wallhack] Entity-Flag effects applied to {allPlayers.Count()} players");
        }
    }

    private void ApplyEntityFlags(CCSPlayerPawn playerPawn, string playerName)
    {
        try 
        {
            // Entity-Flags für bessere Sichtbarkeit
            var flagsProperty = playerPawn.GetType().GetProperty("m_fFlags");
            if (flagsProperty != null)
            {
                var currentFlags = (uint)flagsProperty.GetValue(playerPawn);
                // FL_EDICT_ALWAYS_CHECK flag hinzufügen
                var newFlags = currentFlags | 0x40000000;
                flagsProperty.SetValue(playerPawn, newFlags);
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_fFlags");
                
                if (Config.DebugMode)
                    Console.WriteLine($"[Wallhack] Entity flags applied to {playerName}");
            }

            // Effects für Wallhack
            var effectsProperty = playerPawn.GetType().GetProperty("m_fEffects");
            if (effectsProperty != null)
            {
                var currentEffects = (uint)effectsProperty.GetValue(playerPawn);
                // EF_ITEM_BLINK | EF_BRIGHTLIGHT
                var newEffects = currentEffects | 0x020 | 0x004;
                effectsProperty.SetValue(playerPawn, newEffects);
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_fEffects");
                
                if (Config.DebugMode)
                    Console.WriteLine($"[Wallhack] Effects applied to {playerName}");
            }

            // Collision Group ändern
            var collisionProperty = playerPawn.GetType().GetProperty("m_CollisionGroup");
            if (collisionProperty != null)
            {
                collisionProperty.SetValue(playerPawn, 2); // COLLISION_GROUP_DEBRIS
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_CollisionGroup");
            }

        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"[Wallhack] Entity Flag Error for {playerName}: {ex.Message}");
        }
    }

    private void ApplyRenderingMode(CCSPlayerPawn playerPawn, string playerName)
    {
        try 
        {
            // Rendering-Mode für Glow
            var renderModeProperty = playerPawn.GetType().GetProperty("m_nRenderMode");
            if (renderModeProperty != null)
            {
                renderModeProperty.SetValue(playerPawn, 6); // RenderMode: Glow
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_nRenderMode");
                
                if (Config.DebugMode)
                    Console.WriteLine($"[Wallhack] RenderMode set to Glow for {playerName}");
            }

            // Render-FX für Pulse-Effekt
            var renderFXProperty = playerPawn.GetType().GetProperty("m_nRenderFX");
            if (renderFXProperty != null)
            {
                renderFXProperty.SetValue(playerPawn, 14); // kRenderFxPulseSlow
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_nRenderFX");
            }

            // Alpha für Transparenz
            var alphaProperty = playerPawn.GetType().GetProperty("m_clrRender");
            if (alphaProperty != null)
            {
                // Setze Alpha direkt auf Entity
                var currentColor = playerPawn.Render;
                var newColor = Color.FromArgb(200, currentColor.R, currentColor.G, currentColor.B);
                playerPawn.Render = newColor;
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
            }

        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"[Wallhack] Render Mode Error for {playerName}: {ex.Message}");
        }
    }

    private void ResetPlayerEffects(CCSPlayerPawn playerPawn)
    {
        try 
        {
            // Rendering zurücksetzen
            playerPawn.Render = Color.White;
            Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");

            // RenderMode zurück zu Normal
            var renderModeProperty = playerPawn.GetType().GetProperty("m_nRenderMode");
            if (renderModeProperty != null)
            {
                renderModeProperty.SetValue(playerPawn, 0); // Normal
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_nRenderMode");
            }

            // Effects zurücksetzen
            var effectsProperty = playerPawn.GetType().GetProperty("m_fEffects");
            if (effectsProperty != null)
            {
                effectsProperty.SetValue(playerPawn, 0); // Keine Effects
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_fEffects");
            }

        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"[Wallhack] Reset Error: {ex.Message}");
        }
    }

    private bool ShouldShowPlayer(CCSPlayerController wallhackUser, CCSPlayerController target)
    {
        if (!target.IsValid || wallhackUser == target) return false;
        
        // Tote Spieler nicht highlighten
        if (!target.PawnIsAlive || !wallhackUser.PawnIsAlive) return false;

        if (Config.EnemyOnlyMode)
        {
            return target.Team != wallhackUser.Team;
        }

        return true;
    }

    public override void Unload(bool hotReload)
    {
        // Cleanup aller Effekte beim Unload
        try 
        {
            var allPlayers = Utilities.GetPlayers().Where(p => p?.IsValid == true);
            foreach (var player in allPlayers)
            {
                var pawn = player.PlayerPawn.Value;
                if (pawn?.IsValid == true)
                {
                    ResetPlayerEffects(pawn);
                }
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"[Wallhack] Cleanup Error: {ex.Message}");
        }
        
        Console.WriteLine("[Wallhack] Entity-Flags Wallhack plugin unloaded");
    }
}

public class WallhackConfig : BasePluginConfig
{
    public bool EnablePlugin { get; set; } = true;
    public int GlowAlpha { get; set; } = 200;
    public bool EnemyOnlyMode { get; set; } = true;
    public bool LogWallhackUsage { get; set; } = true;
    public bool UseGlowEffect { get; set; } = true;
    public bool DebugMode { get; set; } = false;
}