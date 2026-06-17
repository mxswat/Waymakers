# Waymakers

A RimWorld meme mod integrated with [Rails and Roads of the Rim](https://steamcommunity.com/sharedfiles/filedetails/?id=3271115410).

## What it does

Adds the **Waymakers** meme, a Normal meme (impact 2) for builders, traders, and explorers who believe civilization advances through connection.

### Meme features
- Trait preferences: Industrious, Jogger, Ascetic; dislikes Lazy
- Incompatible with Isolationist memes

### Surveyor Specialist
- Requires Construction 6+ and Intellectual 4+
- Passive: +50% Construction Speed, +20% Mining Speed
- Disabled work: Cooking, Plants, Animals, Art
- **Coordinate Works** ability: aura buff to nearby colonists (+50% build, +25% haul, +10% move)

### Mood thoughts
Four tier-based stacking thoughts triggered by road/rail completion:

| Tier | Roads | Base mood |
|------|-------|-----------|
| Crude | Dirt paths | +2 |
| Basic | Stone roads | +5 |
| Engineered | Asphalt, Railroads | +9 |
| Advanced | Glitter roads/rails | +13 |

Mood scales with road length (legs / 3) and duration scales with legs.

## Dependencies
- Harmony
- RimWorld - Ideology
- [Rails and Roads of the Rim (Continued)](https://steamcommunity.com/sharedfiles/filedetails/?id=3271115410)

## Build

```powershell
dotnet build "Source\Waymakers.csproj"
```

Targets .NET Framework 4.7.2. Uses reflection-based Harmony patches against RotR, no compile-time dependency on the RotR DLL.

## Credits

- Art by Nekl

## License

GPL-3.0
