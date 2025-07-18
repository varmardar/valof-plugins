
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API.Modules.Events;
using Valof_Powerups.PowerUps;





namespace Valof_Powerups;
[MinimumApiVersion(129)]
public class ValofPowerups : BasePlugin
{
    public override string ModuleName => "PowerUp Plugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "YourName";
    public override string ModuleDescription => "PowerUp System mit dauerhafter UI";

    private Timer? _uiTimer;
    private CCSGameRules? _gameRules;
    
    
    // PowerUp System
    private readonly Dictionary<int, IPowerUp> _playerPowerUps = new();
    private readonly List<IPowerUp> _availablePowerUps = new();
    private readonly Random _random = new();
    private readonly HashSet<int> _playersAssignedThisRound = new();
    private bool _roundStartProcessed = false;
    private readonly Dictionary<int, bool> _playerAnimating = new();
    private readonly Dictionary<int, int> _animationStep = new();
    private readonly Dictionary<int, IPowerUp> _finalPowerUp = new();
    private bool _isFirstRound = true;

    public override void Load(bool hotReload)
    {
        Console.WriteLine($"[{ModuleName}] Plugin geladen!");
        
        // HTML Flash Fix
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnMapStart>(OnMapStartHandler);
        
        // UI Timer
        _uiTimer = AddTimer(0.1f, UpdateUI, TimerFlags.REPEAT);
        
        // Event Handler
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
        RegisterEventHandler<EventRoundPrestart>(OnRoundPrestart);
        
        RegisterEventHandler<EventRoundFreezeEnd>(OnRoundFreezeEnd);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
        RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);

        

        // Commands
        AddCommand("use", "PowerUp verwenden", OnUseCommand);
        
        // PowerUps laden
        LoadPowerUps();
    }

    public override void Unload(bool hotReload)
    {
        _uiTimer?.Kill();
        Console.WriteLine($"[{ModuleName}] Plugin entladen!");
    }

    private void LoadPowerUps()
    {   
        //_availablePowerUps.Clear();
        //_availablePowerUps.Add(new SpeedBoostPowerUp());
        //_availablePowerUps.Add(new LowGravityPowerUp());
        //_availablePowerUps.Add(new TeleportToEnemySpawnPowerUp());
        //_availablePowerUps.Add(new TeleportToOwnSpawnPowerUp());
        //_availablePowerUps.Add(new DecoyPowerUp());
        //_availablePowerUps.Add(new SwapWithEnemyPowerUp());
        //_availablePowerUps.Add(new TeleportOnKillPowerUp());
        //_availablePowerUps.Add(new OverpoweredZeusPowerUp());
        //_availablePowerUps.Add(new ExtraHealthPowerUp());
        //_availablePowerUps.Add(new InfiniteAmmoPowerUp());
        //_availablePowerUps.Add(new OneShotDeaglePowerUp());
        //_availablePowerUps.Add(new InvisibilityPowerUp());
        _availablePowerUps.Add(new DamageReductionPowerUp());
        //_availablePowerUps.Add(new ChickenModelPowerUp());



        Console.WriteLine($"[{ModuleName}] {_availablePowerUps.Count} PowerUps geladen!");
    }

    private void OnMapStartHandler(string mapName)
    {
        _gameRules = null;
        _playerPowerUps.Clear();
        _playerAnimating.Clear();
        _animationStep.Clear();
        _finalPowerUp.Clear();
        _isFirstRound = true; // Bei neuer Map wieder auf true setzen
    }

    private void InitializeGameRules()
    {
        var gameRulesProxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
        _gameRules = gameRulesProxy?.GameRules;
    }

    private void OnTick()
    {
        if (_gameRules == null)
        {
            InitializeGameRules();
        }
        else
        {
            _gameRules.GameRestart = _gameRules.RestartRoundTime < Server.CurrentTime;
        }
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
    var player = @event.Userid;
    if (player != null && player.IsValid)
    {
        // NUR UI anzeigen beim Connect, KEIN PowerUp
        ShowPersistentUI(player);
    }
    return HookResult.Continue;
    }
[GameEventHandler]
public HookResult OnRoundPrestart(EventRoundPrestart @event, GameEventInfo info)
{
    // Nur beim ersten Aufruf pro Runde verarbeiten
    if (_roundStartProcessed) return HookResult.Continue;
    _roundStartProcessed = true;
    
    // Timer um Flag nach kurzer Zeit zurückzusetzen
    AddTimer(1.0f, () => _roundStartProcessed = false);
    
    // ERST alle PowerUps löschen
    _playerPowerUps.Clear();
    _playersAssignedThisRound.Clear();
    _playerAnimating.Clear();
    _animationStep.Clear();
    _finalPowerUp.Clear();
    
    // PowerUps zurücksetzen
    foreach (var powerUp in _availablePowerUps)
    {
        powerUp.ResetForNewRound();
    }
    
    // Delay nur bei der ersten Runde
    var delay = _isFirstRound ? 8.0f : 0.0f;
    _isFirstRound = false; 
    
    AddTimer(delay, () =>
    {
        var players = Utilities.GetPlayers();
        foreach (var player in players)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                AssignRandomPowerUp(player);
            }
        }
    });
    
    return HookResult.Continue;
}
[GameEventHandler]
public HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
{
    // Automatisch für alle PowerUps aufrufen
    var players = Utilities.GetPlayers();
    foreach (var player in players)
    {
        if (player != null && player.IsValid && !player.IsBot && _playerPowerUps.ContainsKey(player.Slot))
        {
            _playerPowerUps[player.Slot].OnRoundStart(player);
        }
    }
    
    return HookResult.Continue;
}

    [CommandHelper]
    public void OnUseCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || !player.IsValid) return;

        if (_playerPowerUps.ContainsKey(player.Slot))
        {
            var powerUp = _playerPowerUps[player.Slot];
            if (powerUp.CanUse(player))
            {
                powerUp.Use(player);
            }
            else
            {
                player.PrintToChat($"❌ {powerUp.Name} kann gerade nicht verwendet werden!");
            }
        }
        else
        {
            player.PrintToChat("❌ Du hast kein PowerUp!");
        }
    }

    private void AssignRandomPowerUp(CCSPlayerController player)
{
    if (_availablePowerUps.Count == 0) return;
    
    // Finales PowerUp auswählen
    var randomPowerUp = _availablePowerUps[_random.Next(_availablePowerUps.Count)];
    _finalPowerUp[player.Slot] = randomPowerUp;

    Console.WriteLine($"[DEBUG-POWERUP] {player.PlayerName} bekommt PowerUp: {randomPowerUp.Name}");
    
    
    // Animation starten
        _playerAnimating[player.Slot] = true;
    _animationStep[player.Slot] = 0;
    
    // Sound abspielen
    player.ExecuteClientCommand("play sounds/ui/armsrace_demoted.wav");
    
    // Animation Timer (alle 0.2 Sekunden für 3 Sekunden = 15 Schritte)
    var animationTimer = AddTimer(0.2f, () =>
    {
        if (!_playerAnimating.ContainsKey(player.Slot) || !_playerAnimating[player.Slot])
            return;
            
        _animationStep[player.Slot]++;
        
        // Animation nach 15 Schritten beenden
        if (_animationStep[player.Slot] >= 15)
        {
            _playerAnimating[player.Slot] = false;
            _playerPowerUps[player.Slot] = _finalPowerUp[player.Slot];
            
            // Final Sound
            player.ExecuteClientCommand("play sounds/ui/armsrace_level_up.wav");
            player.PrintToChat($"🎁 Du hast das PowerUp '{_finalPowerUp[player.Slot].Name}' erhalten!");
            
            // Bei permanent PowerUps automatisch aktivieren
            if (_finalPowerUp[player.Slot].Type == PowerUpType.Permanent)
            {
                _finalPowerUp[player.Slot].Use(player);
            }
        }
        
    }, TimerFlags.REPEAT);
    
    // Timer nach 4 Sekunden stoppen
    AddTimer(4.0f, () => animationTimer?.Kill());
}



    private void UpdateUI()
{
    var players = Utilities.GetPlayers();
    
    foreach (var player in players)
    {
        if (player == null || !player.IsValid )
            continue;

        // Automatisches Update für alle PowerUps
        if (_playerPowerUps.ContainsKey(player.Slot))
        {
            _playerPowerUps[player.Slot].OnUpdate(player);
        }

        ShowPersistentUI(player);
    }
}

    private void ShowPersistentUI(CCSPlayerController player)
    {
        var uiText = BuildUIText(player);
        player.PrintToCenterHtml(uiText);
    }

    private string BuildUIText(CCSPlayerController player)
{
    var playerName = player.PlayerName ?? "Unknown";
    var powerUpName = "Kein PowerUp";
    var statusText = "---";
    var useText = "---";

    // Animation läuft
    if (_playerAnimating.ContainsKey(player.Slot) && _playerAnimating[player.Slot])
    {
        // Zufälligen PowerUp Namen für Animation anzeigen
        var randomIndex = _random.Next(_availablePowerUps.Count);
        powerUpName = $"🎲 {_availablePowerUps[randomIndex].Name} 🎲";
        statusText = "🎰 AUSWAHL LÄUFT... 🎰";
        useText = "⏳ Bitte warten...";
    }
    // Normaler Zustand
    else if (_playerPowerUps.ContainsKey(player.Slot))
    {
        var powerUp = _playerPowerUps[player.Slot];
        powerUpName = powerUp.Name;
        statusText = powerUp.GetStatusText(player);
        
        useText = powerUp.Type switch
        {
            PowerUpType.Permanent => powerUp.CanUse(player) ? "!use zum aktivieren" : "Bereits aktiv",
            PowerUpType.Cooldown => powerUp.CanUse(player) ? "!use zum benutzen" : $"Cooldown: {statusText}",
            PowerUpType.Limited => powerUp.CanUse(player) ? "!use zum benutzen" : "Aufgebraucht",
            _ => "!use zum benutzen"
        };
    }
    
    var uiText = $@"<div style='
        position: absolute;
        top: 50px;
        left: 50px;
        background: rgba(0,0,0,0.9);
        padding: 20px;
        border-radius: 10px;
        font-size: 18px;
        color: white;
        border: 2px solid #FFD700;
        z-index: 9999;
        width: 350px;
        font-family: Arial;
        text-align: center;
    '>
<div style='color: #FFFFFF; font-size: 22px; font-weight: bold; margin-bottom: 10px;'>{playerName}</div>
<br>
<div style='color: #FFFF00; font-size: 20px; margin-bottom: 10px;'>{powerUpName}</div>
<br>
<div style='color: #FF8800; font-size: 20px; margin-bottom: 10px;'>{statusText}</div>
<br>
<div style='color: #00FFFF; font-size: 20px;'>{useText}</div>
</div>";
    
    return uiText;
}

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var victim = @event.Userid;
        var attacker = @event.Attacker;

        // Angreifer muss gültig sein
        if (attacker == null || !IsPlayerValid(attacker))
            return HookResult.Continue;

        // Prüfen ob Angreifer TeleportOnKill PowerUp hat
        if (_playerPowerUps.ContainsKey(attacker.Slot) &&
            _playerPowerUps[attacker.Slot] is TeleportOnKillPowerUp teleportPowerUp)
        {
            if (teleportPowerUp.HasPowerUp(attacker) && victim != null)
            {
                teleportPowerUp.OnPlayerKill(attacker, victim);
            }
        }

        return HookResult.Continue;
    }

[GameEventHandler]
public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
{
    var attacker = @event.Attacker;
    var victim = @event.Userid;

    Console.WriteLine($"[DEBUG-HURT] PlayerHurt Event: {victim?.PlayerName} nahm {@event.DmgHealth} Schaden (Hitgroup: {@event.Hitgroup})");

    // Victim Validierung
    if (victim?.IsValid != true || victim.PlayerPawn?.Value?.IsValid != true) 
    {
        Console.WriteLine($"[DEBUG-HURT] Victim ungültig - Abbruch");
        return HookResult.Continue;
    }

    // NEUE LOGIK: Damage Reduction PowerUp
    if (_playerPowerUps.TryGetValue(victim.Slot, out var victimPowerUp))
    {
        Console.WriteLine($"[DEBUG-HURT] {victim.PlayerName} hat PowerUp: {victimPowerUp.Name}");
        
        if (victimPowerUp is DamageReductionPowerUp damageReduction &&
            damageReduction.HasDamageReduction(victim))
        {
            Console.WriteLine($"[DEBUG-HURT] DamageReduction PowerUp erkannt - verarbeite...");
            
            // Verarbeite die Damage Reduction SOFORT
            Server.NextFrame(() =>
            {
                damageReduction.ProcessDamageReduction(victim, @event.DmgHealth, @event.Hitgroup);
            });
        }
        else
        {
            Console.WriteLine($"[DEBUG-HURT] {victim.PlayerName} hat KEIN DamageReduction PowerUp");
        }
    }
    else
    {
        Console.WriteLine($"[DEBUG-HURT] {victim.PlayerName} hat GAR KEIN PowerUp");
    }

    // Rest bleibt gleich...
    return HookResult.Continue;
}

    private bool IsPlayerValid(CCSPlayerController? player)
    {
        return player?.IsValid == true &&
               player.PlayerPawn?.Value?.IsValid == true &&
               player.PlayerPawn.Value.Health > 0;
    }

[GameEventHandler]
public HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
{   
    

    var player = @event.Userid;
    var weapon = @event.Weapon;
    Console.WriteLine($"[DEBUG] Weapon fired: {@event.Weapon} by {player?.PlayerName}");
    if (player == null || weapon != "weapon_taser") return HookResult.Continue;

    if (_playerPowerUps.ContainsKey(player.Slot) && 
        _playerPowerUps[player.Slot] is OverpoweredZeusPowerUp zeusPowerUp)
    {
        if (zeusPowerUp.HasPowerUp(player))
        {
            if (zeusPowerUp.CanShootZeus(player))
            {
                zeusPowerUp.OnZeusShot(player);
            }
            else
            {
                player.PrintToChat("⚡ Zeus im Cooldown!");
            }
        }
    }

    return HookResult.Continue;
}

}