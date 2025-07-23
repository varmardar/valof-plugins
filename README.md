<br />
<div align="center">
  <a href="https://github.com/othneildrew/Best-README-Template">
    <img src="images/logo.png" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">CS2 Valof-PowerUps</h3>

  <p align="center">
    PowerUps which make your internal games more fun
  </p>
</div>

<div>
  <h3>First installation</h3>

  <p>
    To install this plugin you need Metamod and Counterstrikesharp on your server, a tutorial can be found on this page:
  </p>
  <a href="https://www.ghostcap.com/how-to-install-cs2-plugins">Install Metamod and Counterstrike Sharp</a>
</div>

<div>
  <h3>Mod installation / Updating</h3>

  <p>
    To install the mode in counterstrikesharp, download the latest release and unzip it and move the files in this folder into the folder <b>/game/csgo/addons/counterstrikesharp/plugins/Valof-Powerups</B>. The name of the folder cannot be freely selected, otherwise the mod will not work.
  </p>

  <h3>Changing gamedata.json</h3>
  <p>For all PowerUps to work without errors you have to update your <b>/game/csgo/addons/counterstrikesharp/gamedata/gamedata.json</b>, just add the following lines in json a</p>
  ```
  "GameTraceManager": {
    "signatures": {
      "library": "server",
      "windows": "48 8B 0D ? ? ? ? 48 8D 45 ? 48 89 44 24 ? 4C 8D 44 24 ? C7 44 24 ? ? ? ? ? 48 8D 54 24 ? 4C 8B CB",
      "linux": "48 8D 05 ? ? ? ? F3 0F 58 8D ? ? ? ? 31 FF"
    }
  },
  "TraceFunc": {
    "signatures": {
      "library": "server",
      "windows": "4C 8B DC 49 89 5B ? 49 89 6B ? 49 89 73 ? 57 41 56 41 57 48 81 EC ? ? ? ? 0F 57 C0",
      "linux": "48 B8 ? ? ? ? ? ? ? ? 55 48 89 E5 41 57 41 56 49 89 D6 41 55"
    }
  },
  "CTraceFilterVtable": {
    "signatures": {
      "library": "server",
      "windows": "48 8D 05 ? ? ? ? 66 0F 7F 45 ? 48 89 45 ? 41 0F B6 F1",
      "linux": "48 8D 05 ? ? ? ? F3 0F 11 95 ? ? ? ? F3 0F 11 85 ? ? ? ? 48 C7 85"
    }
  },
  "TraceShape": {
    "signatures": {
      "library": "server",
      "windows": "48 89 5C 24 ?? 48 89 4C 24 ?? 55 56 41 55",
      "linux": "55 48 89 E5 41 57 41 56 49 89 CE 41 55 4D 89 C5 41 54 49 89 D4 53 4C 89 CB"
    }
  ```


</div>

<div>
  <h3>Available Powerups</h3>

| PowerUp Name | PowerUp Description | Implemented |
| :---         |     :---      |          :---: |
| Anti Flash PowerUp   | Player can not be flashed     | ✅    |
| Big Helmet PowerUp   | Player is immune to headshots     | ✅    |
| Body Kevlar PowerUp     | Player is immune to all hits butheadshots       | ✅       |
| Bunnyhop Model PowerUp     | Allows player to bunnyhop without restrictions      | ✅       |
| Chicken Model PowerUp     | Player model is changed with a chicken       | ✅       |
| Damage Reduction PowerUp     | All bodyshots deal 10 damage and all headshots 20 damage       | ✅       |
| Death Bomb PowerUp     | After player is killed, he will explode and deal damage to nearby players       | ✅       |
| Decoy PowerUp     | Player is able to place limited decoys of his own model       | ✅       |
| ExtraHealth PowerUp     | At the beginning of the round the player receives between 100-200 Extra Health       | ✅       |
| Fly PowerUp     | Player can fly while holding his Jump Key      | ✅       |
| Infinite Ammo PowerUp    | Player has infinite ammo on all his weapons in this round       | ✅       |
| Invisibility PowerUp     | Player is invisible while holding his knife       | ✅       |
| LowGravity PowerUp     | Player gravity gets decreased in this round alowing higher jumps       | ✅       |
| OneShotDeagle PowerUp     | Player is given a Deagle with 10 Bullets, thats able to one shot every player       | ✅       |
| OP Zeus PowerUp     | Player is given a Zeus, with unlimited range and decreased reload time      | ✅       |
| SpeedBoost PowerUp     | Players speed is doubled       | ✅       |
| Swap with Enemy PowerUp     | Player is allowed to swap places with a random enemy       | ✅       |
| TeleportOnKill PowerUp      | After killing an player, the player is automatically teleported to the position of the victim       | ✅       |
| Teleport to Enemy Spawn PowerUp     | After a cooldown timer the player is allowed to teleport to the enemy spawn      | ✅       |
| Teleport to own Spawn PowerUp     | After a cooldown timer the player is allowed to teleport back to the own spawn      | ✅       |
| Wallhack PowerUp     | Player is able to see other players glow through walls      | ✅       |

</div>



