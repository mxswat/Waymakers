# Changelog

## 1.1.0

- VFEC road work integration: Coordinate Works buff now doubles VFEC road building speed
- RotR is now optional (removed from modDependencies, kept in loadOnly), VFEC also optional
- Readme, workshop description, and About.xml updated to clarify that at least one road-building mod is needed
- Codebase split: monolithic Waymakers.cs split into 6 files with Patches/ directory
- Deduplicated pawn-collection pattern into CaravanUtils helpers (`GetAllCaravanPawns`, `HasCoordinateWorksBuff`)

## 1.0.1

- Complete Italian translations for all thought descriptions and buff status text
- Italian Keyed translation entries added

## 1.0.0 - Initial Release

- Waymakers meme (Normal, Impact 2) with trait preferences (Industrious, Jogger, Ascetic)
- Surveyor specialist role (+50% Construction, +20% Mining)
- Coordinate Works ability (+75% construction aura, 3d cooldown)
- Coordinate Works caravan gizmo (cast from world map)
- 2x caravan road building speed when Coordinate Works active
- Infrastructure Required precept (-10 mood after 30 days without roads)
- Four road completion mood thoughts (Crude +2, Basic +5, Engineered +9, Advanced +13)
- Groundbreaking Ceremony ritual (mood + construction speed buff)
- Opening of the Line ritual (mood + trader caravan arrival)
- Reflection-based RotR and VVE integration
- Buff status displayed on RotR road construction site
- Performance optimizations (cached reflection lookups)
- Groundbreaking ceremony severity now scales with ritual quality
