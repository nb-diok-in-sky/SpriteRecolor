# Changelog

All notable changes to Sprite Batch Recolor are documented in this file.

## [2.1.0] - 2025-05-28

### Added
- **Hue Shift** blend mode — replaces hue only via HSV conversion, preserving original saturation and brightness. The most natural recoloring option for detailed sprites.
- **Colorize** blend mode — replaces hue + saturation via HSV, keeping only brightness. Ideal for painting color onto grayscale art.
- Mode descriptions visible in the UI — each blend mode shows a tooltip explaining what it does.
- ScrollView for the editor window — works properly at any window size.

### Changed
- Reordered blend modes: Hue Shift first (most useful), Flat Replace last (most destructive).
- Refactored GUI into `DrawSettings`, `DrawPreviewSection`, `DrawApplyButton`, `DrawFooter` for cleaner structure.
- Renamed path utilities for clarity: `TraceSourcePath`, `BuildCopyPath`, `EnsureDirectory`.
- Removed the `ColorOnly` mode (replaced by the more capable Hue Shift and Colorize).

## [2.0.0] - 2025-05-28

### Added
- **6 blend modes**: Flat Replace, Multiply, Screen, Overlay, Soft Light, Color.
- **Real-time preview** panel — side-by-side Original / Result display, updates on parameter change.
- **Intensity slider** (0–1) — controls blend strength, like layer opacity in Photoshop.
- **Progress bar** during batch processing via `EditorUtility.DisplayProgressBar`.
- **Sprite Multiple mode support** — copies `spritesheet` slice data from source importer.
- Copies sprite **pivot** and **border** (9-slice) from source importer.
- `FindMatchingSprite` — matches sub-sprites by name for atlas textures.

### Changed
- Wrapped in `namespace SpriteTools` to avoid global name collisions.
- UI language changed from Chinese to English (comments remain bilingual).
- Pixel processing upgraded from `Color[]` to `Color32[]` — ~4× memory bandwidth improvement on large textures.
- `spriteImportMode` is now copied from source instead of being forced to `Single`.
- Minimum window size enforced (300×420).

## [1.1.0] - 2025-05-27

### Fixed
- **"Texture not readable" error** — replaced direct `GetPixels()` with `ReadPixelsViaRT()` that reads pixels through a RenderTexture + `Graphics.Blit`. Completely bypasses the `isReadable` import setting restriction.
- Fixed the same readability issue in the copy-creation step.

### Changed
- Batch import now uses `AssetDatabase.StartAssetEditing()` / `StopAssetEditing()` wrapper to avoid triggering individual import callbacks.

## [1.0.0] - 2025-05-26

### Added
- Initial release.
- Flat color replacement — replaces all pixel RGB with a target color, preserving alpha.
- Batch processing — select multiple GameObjects, recolor all child SpriteRenderers.
- Non-destructive workflow — copies saved in `copy/` subfolder, originals untouched.
- Idempotent — re-running overwrites existing copies without creating duplicates.
- Source path tracing — prevents nested `copy/copy/copy` chains.
- Unity Undo integration with grouped operations.
