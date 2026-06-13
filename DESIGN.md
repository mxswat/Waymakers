# Waymakers

## Description

The prosperity of civilization depends on connection. Roads, railways, and commerce bind isolated peoples together and allow knowledge and culture to flourish. To create paths is a noble calling.

## Impact

Medium

---

# Incompatible Memes

### Isolationist

Waymakers believe settlements should be connected.

Isolationists believe settlements should stand apart.

These philosophies are fundamentally opposed.

### Raider

Raiders prey upon travelers and disrupt commerce.

Waymakers build and protect routes.

These philosophies are fundamentally opposed.

---

# Required Precepts

None.

---

# Precepts More Likely To Appear

### Trading

* Respected

### Hospitality

* Welcomed

### Charity

* Respected

### Diversity of Thought

* Moderate Bigotry
* Diversity Appreciated

### Research Speed

* Faster research preferred

### Mining Yield

* Increased

---

# Disabled Precepts

None.

---

# Unlocked Role

## Surveyor Specialist

### Requirements

* Construction 6+
* Intellectual 4+

### Passive Effects

* Construction speed +50%
* Mining speed +20%
* Caravan packing speed +50%

### Ability

#### Coordinate Works

Cooldown: 3 days

Duration: 24 hours

Nearby colonists receive:

* Construction speed +75%
* Move speed +10%

> **Open design:** `GeneralLaborSpeed` was considered for hauling but dropped (Move Speed covers the walking portion). Should the buff persist after leaving the aura radius (enabling road building)? Currently uses a standard link like Production Command.

### Disabled Work Types

* Cooking
* Plants
* Animals
* Artistic

---

# Required Rituals

None.

---

# Unlocked Rituals

## Groundbreaking Ceremony

Celebrates the beginning of a major project.

Possible outcomes:

* Mood bonus.
* Inspiration chance.
* Temporary construction speed bonus.

---

## Opening of the Line

Celebrates the completion of a road or railway.

Possible outcomes:

* Mood bonus.
* Friendly visitors.
* Temporary caravan movement speed bonus.

---

# New Buildables

None.

---

# Unlocked Craftables

None.

---

# Starting Research

None.

---

# Agreeing Traits

* Industrious
* Jogger
* Ascetic

---

# Conflicting Traits

* Lazy

---

# Design Intent

Waymakers are builders, traders, and explorers.

Their purpose is not conquest, nor isolation.

They believe civilization advances when people, goods, and ideas are allowed to move freely.

No settlement should stand alone.

# Mood Thoughts

> Note: Several mood concepts were considered but trimmed as too generic or overlapping with Vanilla Memes Expanded. Only road/connection-themed thoughts remain.

## New Connection Established

**Mood:** scaling per tier

**Duration:** 5 + legs (capped per tier)

A new road or railway strengthens civilization and brings hope for the future.

Granted after:
- Completing a road.
- Completing a railway.

### Scaling

Four tier-based mood thoughts, one per road quality. Same tier replaces itself; different tiers stack.

```
mood = baseMood[tier] + (legs / 3)
durationDays = 5 + legs  (capped per tier: Crude 15d, Basic 20d, Engineered 25d, Advanced 30d)
```

| Tier         | baseMood | Example roads                                     | 1-leg | 6-leg |
|--------------|----------|---------------------------------------------------|-------|-------|
| Crude        | +2       | DirtPath, DirtRoad                                | +2    | +4    |
| Basic        | +5       | StoneRoad                                         | +5    | +7    |
| Engineered   | +9       | Asphalt, Railroad, RailTunnel, RailOverpass       | +9    | +11   |
| Advanced     | +13      | GlitterRoad, Glitterrail, GlitterOverpass         | +13   | +15   |

---

## Isolated Settlement

**Mood:** -3

Triggered by:

* Hostile relations with most factions.
* Lack of visitors or traders for an extended period.
* No road built in a long time.

> **Open design.** Could use a custom ThoughtWorker checking faction relations, visitor history, or time since last road completion. Lower priority , implement after core features.

---

## Route Destroyed

> **Back burner.** Built roads in RotR are stored as `SurfaceTile.RoadLink` entries in the world grid. RotR has no "destroy road" feature , roads are only superseded by better roads, never removed. No straightforward hook exists for this thought. Revisit later; it may need a custom road-damage event or hook into colony-abandonment / Vehicle Framework loss events.

**Mood:** -6

Duration: 10 days

Followers mourn the loss of an important connection.

Triggered by:

* Destruction of a road or railway.
* Loss of a transport vehicle.
* Abandonment of a colony.

---

## Trimmed concepts

### Recently Traveled (+4, 10d)

After caravan journey, visiting settlement, returning from expedition. Too close to Traveler meme (VME) and generic travel mood.

### Bustling Trade (+3, 5d)

After successful trade caravan, orbital trade, large transaction. Overlaps with Trader meme (VME).

### Long Time Stationary (-5)

After 30 days without caravan/expedition/visiting. Too broad , Waymakers care about roads, not just movement.

### New Ally (+4, 15d)

Forming alliance, restoring friendly relations. Generic diplomacy mood, not road-specific.

---

# Design Philosophy

Waymakers derive happiness from movement, commerce, and cooperation.

They become uneasy when isolated or stagnant.

To followers of Waymakers, civilization is not defined by walls or territory, but by the roads that connect people together.

---

# Implementation Checklist

## Block 1: Mod Skeleton

- [x] Create `Waymakers/` directory under `RimWorld/Mods/`
- [x] Create `About/About.xml` (name, packageId, dependencies: Harmony, Ideology, RotR)
- [x] Create `1.6/` version folder with `Assemblies/`, `Defs/`, `Patches/` subfolders

## Block 2: MemeDef

- [x] Create `1.6/Defs/MemeDefs/Memes_Waymakers.xml`
  - [x] `defName=WM_Waymakers`, label, description, iconPath
  - [x] `impact=2` (Medium), `category=Normal`
  - [x] `exclusionTags`: `Waymaker` tag defined
  - [x] `agreeableTraits`: Industriousness degree 2, SpeedOffset degree 2, Ascetic (using tag-name-as-def workaround)
  - [x] `disagreeableTraits`: Industriousness degree -1 (lazy)
  - [ ] Raider incompatibility: vanilla Raider has no `exclusionTags`, needs Harmony check
  - [ ] VFEA_Isolationist incompatibility: needs XML patch to add `Waymaker` tag

## Block 3: Surveyor Specialist Role

- [x] Create `1.6/Defs/PreceptDefs/Role_Surveyor.xml`
  - [x] Role def (`PreceptRoleSingleBase` parent)
  - [x] `requiredMemes`: link to `WM_Waymakers`
  - [x] Skill requirements: Construction 6+, Intellectual 4+ (`RoleRequirement_MinSkillAny`)
  - [x] `roleDisabledWorkTags`: Cooking, PlantWork, Animals, Artistic
  - [x] Passive stat offsets: ConstructionSpeed +0.50, MiningSpeed +0.20 (`RoleEffect_PawnStatOffset`)
  - [x] `grantedAbilities`: `WM_CoordinateWorks`
  - [x] RulePackDef for role name generation (Surveyor, Pathfinder, Waymaker, etc.)

## Block 4: Coordinate Works Ability

- [x] Create `1.6/Defs/AbilityDefs/Abilities_Waymakers.xml`
  - [x] Cooldown 3 days, duration 24 hours (via `RoleAuraBuffBase` parent)
  - [x] Aura applies +75% ConstructionSpeed to nearby colonists (via HediffComp_GiveHediffsInRange)
  - [x] Ability effect radius doubled to 19.8 (preview circle + aura + link lines)
  - [x] Caravan support: patch on `AmountOfWork` applies ×1.75 multiplier when caster hediff active
  - [x] Uses vanilla comps, no custom C# needed

## Block 5: Rituals

- [ ] Create `1.6/Defs/Rituals/GroundbreakingCeremony.xml`
- [ ] Create `1.6/Defs/Rituals/OpeningOfTheLine.xml`
- [ ] Create PreceptDefs linking rituals to `WM_Waymakers`

## Block 6: Mood Thoughts

- [x] Tier-based road thoughts (`WM_RoadBuilt_Crude/Basic/Engineered/Advanced`) , applied via EndConstruction prefix
- [x] Scaling: `mood = base[tier] + legs/3`, duration `5+legs` capped per tier
- [x] Same tier replaces; different tiers stack
- [ ] Isolated Settlement (-3) , ThoughtWorker checking faction relations + time since last road (later phase)
- [ ] Route Destroyed (-6, 10d) , **back burner**
- [x] Trimmed: Recently Traveled, Bustling Trade, Long Time Stationary, New Ally , too generic/overlapping with VME

## Block 7: RotR Integration

- [x] Patch `EndConstruction` (prefix) to detect road completions
- [x] Patch `FinaliseConstructionSite` to count legs in chain
- [x] Leg tracking via `RuntimeHelpers.GetHashCode` + `CountLegs` walking `previous` chain
- [ ] Work speed bonus: Patch `WorldObjectComp_Caravan.AmountOfWork` when meme active
- [ ] Route destruction trigger , **back burner**

## Block 8: C# Assembly

- [x] Create `Source/` with `.csproj` (net472, Krafs.Rimworld.Ref, Harmony, <ExcludeAssets>runtime)
- [x] Harmony bootstrapper with manual `harmony.Patch()` calls
- [x] RotR integration patches (EndConstruction prefix + FinaliseConstructionSite postfix)
- [x] Reflection-based access (`Traverse`, `AccessTools.Method`), no compile-time RotR dep
- [ ] ThoughtWorkers for conditional thoughts (Isolated Settlement)

## Block 9: Assets

- [x] Meme icon (`Textures/UI/Memes/Waymakers.png`, 128x128)
- [x] Role icon (`Textures/UI/Roles/Surveyor.png`)
- [x] Ability icon (`Textures/UI/Abilities/CoordinateWorks.png`)
- [ ] Ritual icons

## Block 10: Localization

- [ ] `Languages/English/Keyed/` translations
- [x] `Languages/English/DefInjected/` def translations (all def types covered)

---

## Nice-to-haves

- [x] RulePackDef for Surveyor name maker
- [x] Coordinate Works caravan support — patched `AmountOfWork` (×1.75 when caster hediff active)
