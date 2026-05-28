# Sprite Batch Recolor

A non-destructive batch sprite recoloring tool for Unity Editor, with Photoshop-style blend modes and real-time preview.

![Unity](https://img.shields.io/badge/Unity-2021.3%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

- **7 Blend Modes** — including HSV-based Hue Shift and Colorize that preserve shading detail
- **Real-time Preview** — side-by-side original vs. result, updates as you tweak settings
- **Non-destructive** — original textures are never modified; copies live in a `copy` subfolder
- **Batch Processing** — select multiple objects, recolor all their SpriteRenderers at once
- **Intensity Control** — blend between 0% (no change) and 100% (full effect)
- **Undo Support** — full Unity Undo integration, Ctrl+Z to revert
- **No Read/Write Required** — reads pixels via RenderTexture, works with any texture import setting
- **Sprite Atlas Safe** — preserves Multiple-mode slice data, pivot, border, and PPU

## Blend Modes

| Mode | What it does | Best for |
|------|-------------|----------|
| **Hue Shift** | Replaces hue only, keeps saturation & brightness | Recoloring detailed sprites naturally (red shirt → blue shirt) |
| **Colorize** | Replaces hue + saturation, keeps brightness | Painting color onto grayscale or desaturated art |
| **Multiply** | Darkens by multiplying with target color | Tinting light sprites with a darker tone |
| **Screen** | Lightens the image with target color | Adding glow or tinting dark sprites |
| **Overlay** | Boosts contrast while tinting | Dramatic color shifts with enhanced contrast |
| **Soft Light** | Gentle version of Overlay | Subtle tinting without harsh contrast |
| **Flat Replace** | Replaces all RGB with target color, keeps alpha only | Solid-color icons or silhouettes |

## Installation

### Option A — Copy into project

1. Copy the `Editor/` folder into your Unity project's `Assets/` directory
2. The tool appears under **Tools → Sprite Batch Recolor**

### Option B — Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL...**
3. Paste this repo's URL

## Usage

1. Open **Tools → Sprite Batch Recolor**
2. Select one or more GameObjects with SpriteRenderers in the Hierarchy or Scene view
3. Pick a target color (use the eyedropper to sample from screen)
4. Choose a blend mode — **Hue Shift** recommended for most use cases
5. Adjust intensity if needed
6. Check the preview panel to see before/after
7. Click **Apply Recolor**

### How copies are stored

```
Assets/Sprites/
├── player.png              ← original (never touched)
└── copy/
    └── player_copy.png     ← recolored copy
```

Re-applying overwrites existing copies — no duplicate files are created.

### Tips

- **Hue Shift** is almost always what you want. It changes the color while keeping all shading, highlights, and texture detail intact.
- **Colorize** is useful when your source sprite is grayscale or very desaturated.
- **Multiply/Screen** work best for simple tinting effects on flat-colored sprites.
- Use **Intensity** to dial back the effect for subtle color shifts.

## Requirements

- Unity 2021.3 LTS or later
- 2D Sprite package (included in Unity by default)

## License

MIT License — see [LICENSE](LICENSE) for details.
