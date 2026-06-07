# RRWild

simple `/wild` plugin for unturned servers.

players can use `/wild` to teleport to a random wild point set by staff. it also has optional spawn protection so players don't instantly get killed after teleporting.

this was built for live server use, so the setup is pretty simple and the plugin does not do anything heavy in the background.

## commands

| command | permission | description |
| --- | --- | --- |
| `/wild` | `rrwild.wild` | teleports the player to a random wild point |
| `/setwild <index>` | `rrwild.setwild` | saves your current position as a wild point |
| `/removewild <index>` | `rrwild.setwild` | removes a wild point |
| `/clearwild` | `rrwild.setwild` | removes all wild points |

## config

| setting | what it does |
| --- | --- |
| `ProtectionSeconds` | how long spawn protection lasts after teleporting |
| `CancelMoveDistanceMeters` | how far a player can move before protection ends |
| `CancelOnGunEquip` | removes protection when a gun is equipped |
| `CancelOnMeleeEquip` | removes protection when a melee weapon is equipped |
| `CancelOnAnyItemEquip` | removes protection when any item is equipped |
| `BlockDamageTaken` | blocks damage taken while protected |
| `BlockDamageDealt` | blocks damage dealt while protected |
| `PollIntervalSeconds` | how often protection state is checked |
| `TeleportDelay` | delay before teleporting |
| `CooldownSeconds` | cooldown for `/wild`, set to `0` to disable |

## data

wild points are saved in:

```text
WildPoints.json
```

this file is created in the plugin folder after you set your first wild point.

## install

1. build the project or download the compiled dll from releases
2. put `RRWild.dll` in your Rocket `Plugins` folder
3. restart the server
4. give players `rrwild.wild`
5. give staff `rrwild.setwild`
6. use `/setwild <index>` in game to add wild locations

## notes

recommended setup is to add a few wild points around the map so players don't always end up in the same spot.

spawn protection is optional, but i recommend keeping it on if your server has pvp near spawn areas.
