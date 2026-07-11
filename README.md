# Waymakers

A RimWorld meme mod optionally integrating with [Rails and Roads of the Rim](https://steamcommunity.com/sharedfiles/filedetails/?id=3271115410) and [Vanilla Factions Expanded - Classical](https://steamcommunity.com/sharedfiles/filedetails/?id=3014906877).

## What it does

Adds the **Waymakers** meme, a Normal meme (impact 2) for builders, traders, and explorers who believe civilization advances through connection.

### Meme features
- Trait preferences: Industrious, Jogger, Ascetic; dislikes Lazy
- Incompatible with Raider and Isolationist memes

### Surveyor Specialist
- Requires Construction 6+ and Intellectual 4+
- Passive: +50% Construction Speed, +20% Mining Speed
- Disabled work: Cooking, Plants, Animals, Art
- **Coordinate Works** ability: +75% build speed aura (19.8 tile radius), caravan road building ×2

### Rituals
- **Groundbreaking Ceremony** , led by the Surveyor, grants mood bonus and construction speed buff scaled by quality
- **Opening of the Line** , led by the Surveyor, grants mood bonus and attracts trader caravans on positive outcomes

### Forced Precept
- **Infrastructure Required** , -10 mood after 30 days without building a road (30-day grace period for new colonies)

### Mood thoughts
Four tier-based stacking thoughts triggered by road/rail completion:

| Tier | Roads | Base mood |
|------|-------|-----------|
| Crude | Dirt paths | +2 |
| Basic | Stone roads | +5 |
| Engineered | Asphalt, Railroads | +9 |
| Advanced | Glitter roads/rails | +13 |

Mood scales with road length and duration scales with legs.

## Dependencies
- Harmony
- RimWorld - Ideology

**Road-building mod (one required for core features):**
- [Rails and Roads of the Rim (Continued)](https://steamcommunity.com/sharedfiles/filedetails/?id=3271115410) — full integration (recommended)
- [Vanilla Factions Expanded - Classical](https://steamcommunity.com/sharedfiles/filedetails/?id=3014906877) — Coordinate Works buff accelerates VFEC road projects

Without at least one road-building mod, the infrastructure precept and road completion mood thoughts have nothing to trigger on.

## Build

```powershell
dotnet build "Source\Waymakers.csproj"
```

Targets .NET Framework 4.7.2. Uses reflection-based Harmony patches against RotR and a direct reference to VFEC.dll (both optional at runtime).

## Credits

- Art by Nekl

## License

GPL-3.0
