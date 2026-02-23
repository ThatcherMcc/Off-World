# Terrain MVP: Bringing the World to Life with Terrain Types

> Goal: Get a minimum viable product / demo with a good world showing **different terrain types** (sand/beach, grass, rock/cliffs). This doc covers what to change in **ChunkGeneration** and **TerrainGeneration** (and how they interact with your existing terrain material).

---

## Current State

- **TerrainGeneration.cs**: Builds mesh from noise (continentalness, peaks/valleys, erosion). Single UV set `(x, z)`. No vertex colors. Trees spawn by noise above water. Height drives land vs water.
- **ChunkGeneration.cs**: Picks a seed with valid land/water ratio, sets `waterLevel = Perlin(seed, seed) * 15`, spawns chunks and a water plane.
- **Terrain.mat**: Already has **Grass**, **Sand**, **CliffRock** textures and shader params:
  - `_Sand_Height` (13), `_Sand_Threshold` (0.4) — sand near that height.
  - `_Cliff_Rock_Height` (-500), `_Cliff_Rock_Threshold` (0.58) — rock on steep slopes.

The shader blends by **height** and **slope** (world/object position + normals). So terrain types can already appear **if** your height range and thresholds line up.

---

## What You Need to Do

### 1. Align Height Ranges with the Shader (ChunkGeneration + Material)

- **waterLevel** is `Perlin(seed, seed) * 15` (0–15). Your terrain height goes from ~0 (water) to much higher (mountains).
- The material’s **sand** is keyed to `_Sand_Height` (currently 13). For a clear “beach” band:
  - Sand should sit **just above water**.
- **Action**: Drive sand height from water level so it’s consistent for every seed.
  - In **ChunkGeneration**, after you know `waterLevel`, set the terrain material’s sand height (e.g. `_Sand_Height = waterLevel + 1` or `waterLevel + 2`). Use a **material property block** or a **material instance** per chunk so you don’t change the shared asset.
  - Alternatively, expose `waterLevel` (or a “shore height”) from ChunkGeneration and have TerrainGeneration apply it when assigning the material (e.g. set `_Sand_Height` on the material instance used by that chunk).
- Tune **thresholds** in the material so transitions look good:
  - `_Sand_Threshold`: controls how wide the sand band is (e.g. 0.3–0.5).
  - `_Cliff_Rock_Threshold`: controls how steep before rock shows (e.g. 0.5–0.65).

Result: **Beaches (sand)** at shore, **grass** on flatter land, **rock** on steep slopes — all visible in the demo.

---

### 2. TerrainGeneration: Better UVs and Optional Vertex Data

- **UVs**: You currently set `uv[i] = (vertices[i].x, vertices[i].z)`. That’s world-space XZ; scale can be wrong for texture density.
  - **Action**: Scale UVs by a constant (e.g. divide by chunk size or a “texel size”) so grass/sand/rock textures don’t stretch or tile oddly. Example: `uv[i] = new Vector2(vertices[i].x / 128f, vertices[i].z / 128f)` or use world position and a scale factor you expose from ChunkGeneration.
- **Normals**: You already call `mesh.RecalculateNormals()`. The shader uses normals for cliff/rock (slope). No change needed unless you add custom normal logic later.
- **Optional (later)**: Add a second UV channel or **vertex color** (e.g. a “moisture” or “biome” value from noise) so you can blend in more terrain types (dirt, dry grass) without changing the shader much. For MVP, height + slope is enough.

Result: **Consistent texture scale** and correct **sand/grass/rock** from the existing shader.

---

### 3. ChunkGeneration: Pass Water Level (or Terrain Config) to Terrain

- **Action**: So that sand height follows water:
  - Either pass `waterLevel` (and optionally shore offset) into **TerrainGeneration** (e.g. set on the chunk’s TerrainGeneration component or via a shared “terrain config” scriptable object / static).
  - Or, in ChunkGeneration after `waterLevel` is set, create/use a **MaterialPropertyBlock** or **material instance** and set `_Sand_Height = waterLevel + 1` (and optionally `_Sand_Threshold`) before or when chunks are generated. Then assign that material/block to the terrain renderers.
- Keep **seed and noise logic** identical between ChunkGeneration’s `SimulateTerrain` / `CalculateHeight` and TerrainGeneration’s `Noise` / `BaseNoise` so that terrain types (and land/water) align at chunk borders. You already do this; don’t change noise order or scales without updating both.

Result: **Sand band follows water** for any seed; no manual material tweaking per world.

---

### 4. Demo-Friendly Tuning (ChunkGeneration + TerrainGeneration)

- **Land/water ratio**: You already use `landThresholdMin` / `landThresholdMax`. Tune so the playable area has a mix of shore, grass, and some hills (e.g. 0.35–0.55 land ratio).
- **Noise scales**: Slightly **larger** continentalness/erosion scale (e.g. multiply the input to Perlin by 0.0003–0.0008) can make “regions” (beach vs inland) more readable and less speckly. Prefer one place to define these (e.g. ChunkGeneration or a shared config) so TerrainGeneration and SimulateTerrain stay in sync.
- **Expose in Inspector**: In ChunkGeneration, expose:
  - `waterLevel` (or “shore height offset” for sand),
  - optionally “sand height offset” (e.g. +1 or +2 above water),

so you can tune the demo without code changes.

Result: **Clear, readable terrain types** and a **stable spawn area** with variety.

---

## Summary Checklist

| Where | What to do | Status |
|-------|------------|--------|
| **ChunkGeneration** | After `waterLevel` is set, set terrain material’s `_Sand_Height` (e.g. `waterLevel + 1`) via MaterialPropertyBlock or material instance; set terrain material _Sand_Height (waterLevel + sandHeightOffset) via runtime material instance. | Done |
| **ChunkGeneration** | Expose `sandHeightOffset` and `terrainUVScale` in Inspector for demo tuning. | Done |
| **TerrainGeneration** | Scale UVs by world position and `terrainUVScale` for consistent texture density across chunks. | Done |
| **Terrain.mat** | Tune `_Sand_Threshold` and `_Cliff_Rock_Threshold` so sand band and cliff blending look good. | Manual (Inspector) |
| **Both** | Keep noise/height logic identical so terrain types align at chunk edges. | Unchanged (already in sync) |

---

## Optional Next Steps (Post-MVP)

- **Vertex color biome**: Add a second noise (e.g. “moisture”) in TerrainGeneration, write to vertex color, and use it in the shader to blend a 4th texture (e.g. dirt or dry grass).
- **ChunkManager / spawn tables**: Use height (and later biome) to pick spawn tables (e.g. “low/beach”, “mid/grass”, “high/rock”) so critters and props match the terrain type.
- **Snow at high elevation**: In the shader, add a 4th layer that blends in above a `_Snow_Height`; set that in ChunkGeneration from your max terrain height or a fixed value.

Once sand height is driven by `waterLevel` and UVs are scaled, your existing Grass/Sand/CliffRock shader will give you a world with **distinct terrain types** for the MVP/demo.
