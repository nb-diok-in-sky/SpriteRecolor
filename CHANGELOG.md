# Changelog / 更新日志

## [3.0.0] - 2025-05-28

### Added / 新增
- **UI Image support** — now works with both `SpriteRenderer` and `UnityEngine.UI.Image`. Selection info shows count of each type.
- **Preset system** — save/load color + mode + intensity combos. Stored in `EditorPrefs`, persists across sessions. Color swatch preview in preset list.

### Changed / 变更
- `GatherTargets()` replaces separate renderer collection — unified pipeline for SpriteRenderer and Image.
- Selection info now shows breakdown (e.g. "3 SpriteRenderer(s) + 2 UI Image(s)").
- README merged into single bilingual file (Chinese first, English below).

---

## [2.1.0] - 2025-05-28

### Added
- **Hue Shift** blend mode — replaces hue only via HSV, preserving saturation and brightness.
- **Colorize** blend mode — replaces hue + saturation via HSV, keeping only brightness.
- Mode descriptions visible in UI with tooltips.
- ScrollView for the editor window.

### Changed
- Reordered blend modes: Hue Shift first, Flat Replace last.
- Refactored GUI into `DrawSettings`, `DrawPreviewSection`, `DrawApplyButton`, `DrawFooter`.
- Removed `ColorOnly` mode (replaced by Hue Shift and Colorize).

## [2.0.0] - 2025-05-28

### Added
- **6 blend modes**: Flat Replace, Multiply, Screen, Overlay, Soft Light, Color.
- **Real-time preview** panel with side-by-side comparison.
- **Intensity slider** (0-1).
- **Progress bar** during batch processing.
- **Sprite Multiple mode support** — copies spritesheet slice data.
- Copies sprite pivot and border (9-slice).

### Changed
- Wrapped in `namespace SpriteTools`.
- UI language: English (comments bilingual).
- Pixel processing: `Color[]` to `Color32[]` (~4x bandwidth improvement).
- `spriteImportMode` copied from source instead of forced to Single.

## [1.1.0] - 2025-05-27

### Fixed
- "Texture not readable" error — replaced `GetPixels()` with `ReadPixelsViaRT()` using RenderTexture + Graphics.Blit.

### Changed
- Batch import uses `StartAssetEditing` / `StopAssetEditing` wrapper.

## [1.0.0] - 2025-05-26

### Added
- Initial release: flat color replacement, batch processing, non-destructive copy workflow, undo support.
