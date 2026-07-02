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

### Infrastructure Required
-10 mood after 30 days without road building. 30-day grace period for new colonies.

Completing roads inspires followers (+2 to +30 mood depending on road quality and tile length). Each quality tier stacks; same tier replaces.

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

### Ability

#### Coordinate Works

Cooldown: 3 days

Duration: 24 hours

Radius: 19.8 tiles (double Production Command)

The Surveyor inspires nearby workers to build faster.

Nearby colonists receive:

* Construction speed +75%

Caravan support: casting before departing doubles road-building work output.

### Disabled Work Types

* Cooking
* Plants
* Animals
* Artistic

---

# Required Precepts

### Infrastructure Required (WM_Infrastructure_Required)

Forced precept (requireOne). Triggers a -10 mood penalty on all Waymakers colonists after 30 days without building a road or railway.

New colonies get a 30-day grace period starting from the settlement date.

---

# Required Rituals

None. Both rituals are unlocked by the meme but optional.

---

# Unlocked Rituals

## Groundbreaking Ceremony

Celebrates the beginning of a major project. Led by the Surveyor.

Ritual behavior:
- Target: GatheringSpot or Altar (vanilla)
- Required role: Surveyor (substitutable if none assigned)
- Duration: ~6,250 ticks (~4 hours)
- Always available (canStartAnytime + alwaysStartAnytime)
- Outcome quality: ParticipantCount curve + Surveyor Construction skill (PawnSkill)

Surveyor skill curve (Construction level → quality bonus):
| Skill | Bonus |
|-------|-------|
| 4     | +0%   |
| 6     | +25%  |
| 8     | +35%  |
| 10    | +45%  |
| 14    | +55%  |
| 18    | +65%  |
| 20    | +80%  |

Participant count curve:
| Count | Bonus |
|-------|-------|
| 1     | +0%   |
| 2     | +10%  |
| 4     | +25%  |
| 7     | +40%  |
| 10    | +50%  |
| 13    | +60%  |

Outcome tiers (mood memory):
| Tier | Mood | Duration |
|------|------|----------|
| Terrible | -6 | 3 days |
| Lackluster | -2 | 5 days |
| Good | +4 | 8 days |
| Glorious | +8 | 12 days |

> **Note:** Construction speed buff implemented via custom worker (sets `severity = quality` for 25/35/50% stages). Inspiration chance not yet implemented.

## Opening of the Line

Celebrates the completion of a road or railway. Led by the Surveyor.

Ritual behavior:
- Target: GatheringSpot or Altar (vanilla)
- Required role: Surveyor (substitutable if none assigned)
- Duration: ~6,250 ticks (~4 hours)
- Always available (canStartAnytime + alwaysStartAnytime)
- Outcome quality: ParticipantCount + Construction skill (PawnSkill, ~70% weight) + SocialImpact (PawnStatScaled, ~30% weight)

Outcome tiers (mood memory):
| Tier | Mood | Duration |
|------|------|----------|
| Terrible | -6 | 3 days |
| Lackluster | -2 | 5 days |
| Good | +4 | 8 days |
| Glorious | +8 | 12 days |

> **Note:** Visitor spawning (TravelerGroup/TraderCaravanArrival) implemented via custom worker. Caravan movement speed buff and faction goodwill not yet implemented.

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

| Tier         | baseMood | Example roads                                     | 1-leg | 6-leg | 30-leg |
|--------------|----------|---------------------------------------------------|-------|-------|--------|
| Crude        | +2       | DirtPath, DirtRoad                                | +2    | +4    | +12    |
| Basic        | +5       | StoneRoad                                         | +5    | +7    | +15    |
| Engineered   | +9       | Asphalt, Railroad, RailTunnel, RailOverpass       | +9    | +11   | +19    |
| Advanced     | +13      | GlitterRoad, Glitterrail, GlitterOverpass         | +13   | +15   | +23    |

Tiers stack , building all 4 gives cumulative mood bonuses.

---

## Isolated Settlement (SCRAPPED)

**Mood:** -3

Triggered by hostile relations, lack of visitors, or no road built. Scrapped as too generic and overlapping with Vanilla Memes Expanded mechanics.

---

## Route Destroyed (SCRAPPED)

**Mood:** -6 , Duration: 10 days

Built roads in RotR are stored as `SurfaceTile.RoadLink` entries in the world grid. RotR has no "destroy road" feature, roads are only superseded, never removed. No clean hook exists. Scrapped.

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
  - [x] Raider incompatibility: vanilla Raider has no `exclusionTags`, needs Harmony check
  - [x] VFEA_Isolationist incompatibility: needs XML patch to add `Waymaker` tag

## Block 2b: Forced Precepts

- [x] `WM_Infrastructure_Required` , -10 mood after 30 days without road building
- [x] Custom `ThoughtWorker_Precept_RecentRoadBuild` (30-day threshold, settleTick grace period)
- [x] Dummy invisible stage 0 (within threshold) + stage 1 (-10 penalty)
- [x] `{DAYSSINCELASTROADTHRESHOLD}` template in `thoughtStageDescriptions`
- [x] `tooltipShowMoodRange` shows the -10 mood range
- [x] Positive road moods described in precept description text
- [x] Forced via `<requireOne>` on `WM_Waymakers`

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

- [x] Create `Ritual_Patterns_Waymakers.xml` , 2 RitualPatternDefs
- [x] Create `Ritual_Behaviors_Waymakers.xml` , Surveyor role, speech + socialize stages
- [x] Create `Ritual_Outcomes_Waymakers.xml` , 4 quality tiers each, mood memories
- [x] Create `Precepts_Rituals_Waymakers.xml` , links both rituals to `WM_Waymakers`
- [x] Outcome quality: PawnSkill (Construction) for both; PawnStatScaled (SocialImpact) for Opening
- [x] Participant count curve: +10% at 2 pawns, scaling to +60% at 13
- [x] Using vanilla components only (no custom C# for outcomes)
- [x] Visitor spawning on Good/Glorious outcomes (Opening of the Line) via custom worker

## Block 6: Mood Thoughts

- [x] Tier-based road thoughts (`WM_RoadBuilt_Crude/Basic/Engineered/Advanced`) , applied via EndConstruction prefix
- [x] Scaling: `mood = base[tier] + legs/3`, duration `5+legs` capped per tier
- [x] Same tier replaces; different tiers stack
- [x] ~~Isolated Settlement (-3)~~ SCRAPPED (too generic, overlaps with VME)
- [x] ~~Route Destroyed (-6, 10d)~~ SCRAPPED (no clean RotR hook)
- [x] Trimmed: Recently Traveled, Bustling Trade, Long Time Stationary, New Ally , too generic/overlapping with VME

## Block 7: RotR Integration

- [x] Patch `EndConstruction` (prefix) to detect road completions
- [x] Patch `FinaliseConstructionSite` to count legs in chain
- [x] Leg tracking via `RuntimeHelpers.GetHashCode` + `CountLegs` walking `previous` chain
- [x] Caravan work speed: Patch `WorldObjectComp_Caravan.AmountOfWork` (×1.75 when caster hediff active)
- [x] ~~Route destruction trigger, **back burner**~~ SCRAPPED (no clean RotR hook)

## Block 8: C# Assembly

- [x] Create `Source/` with `.csproj` (net472, Krafs.Rimworld.Ref, Harmony, <ExcludeAssets>runtime)
- [x] Harmony bootstrapper with manual `harmony.Patch()` calls
- [x] RotR integration patches (EndConstruction prefix + FinaliseConstructionSite postfix + caravan work)
- [x] Reflection-based access (`Traverse`, `AccessTools.Method`), no compile-time RotR dep
- [x] `ThoughtWorker_Precept_RecentRoadBuild` (Infrastructure Required, 30-day threshold, grace period)
- [x] ~~ThoughtWorkers for conditional thoughts (Isolated Settlement)~~ SCRAPPED

## Block 9: Assets

- [x] Meme icon (`Textures/UI/Memes/Waymakers.png`, 128x128)
- [x] Role icon (`Textures/UI/Roles/Surveyor.png`)
- [x] Ability icon (`Textures/UI/Abilities/CoordinateWorks.png`)
- [x] Ritual icons (`WM_GroundbreakingCeremony`, `WM_OpeningOfTheLine`)

## Block 10: Localization

- [x] `Languages/Italian/DefInjected/` translations (all def types covered)
- [x] English labels are inline in Defs, no separate English Keyed needed

---

## Nice-to-haves

- [x] RulePackDef for Surveyor name maker
- [x] Coordinate Works caravan support , patched `AmountOfWork` (×1.75 when caster hediff active)
- [ ] Custom Waymakers Stonecutter Table , faster stone cutting, gated behind meme via `<addDesignators>`. Reuses vanilla sprite, inherits from `TableStonecutter` with `WorkSpeedGlobal` or `CraftingSpeed` bonus.
- [x] **FIX**: Groundbreaking ceremony ConstructionSpeed buff (`WM_GroundbreakingBuff`) now sets `severity = quality` so higher stages unlock at 0.33 (+35%) and 0.66 (+50%).
- [x] Opening of the Line spawns TravelerGroup / TraderCaravanArrival on Good/Glorious outcomes (social buff, not stat buff)
