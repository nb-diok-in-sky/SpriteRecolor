# Sprite Batch Recolor — Sprite 批量染色工具

一个无损的 Unity 编辑器批量 Sprite 染色工具，支持 Photoshop 风格的混合模式和实时预览。

![Unity](https://img.shields.io/badge/Unity-2021.3%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)

## 功能

- **7 种混合模式** — 包含基于 HSV 的色相替换和上色模式，保留原图明暗细节
- **实时预览** — 左右对比原图和效果图，调色时即时刷新
- **无损工作流** — 原图永远不会被修改，副本保存在 `copy` 子文件夹中
- **批量处理** — 选中多个物体，一键染色所有 SpriteRenderer
- **强度控制** — 0%（无变化）到 100%（完全混合）之间自由调节
- **Undo 支持** — 完整的 Unity 撤销集成，Ctrl+Z 一键回退
- **无需 Read/Write** — 通过 RenderTexture 读取像素，不依赖纹理的 isReadable 设置
- **图集兼容** — 完整保留 Multiple 模式的切片数据、Pivot、Border 和 PPU

## 混合模式说明

| 模式 | 效果 | 适合场景 |
|------|------|---------|
| **Hue Shift（色相替换）** | 只换色相，保留饱和度和明暗 | 给有细节的 Sprite 换色（红衣服→蓝衣服），最自然 |
| **Colorize（上色）** | 换色相+饱和度，只保留明暗 | 给灰度或低饱和度素材上色，像水彩铺底色 |
| **Multiply（正片叠底）** | 原色 × 目标色，整体变暗 | 给浅色 Sprite 加深色调 |
| **Screen（滤色）** | 提亮效果 | 给深色 Sprite 加亮色调、发光效果 |
| **Overlay（叠加）** | 亮的更亮暗的更暗，增加对比度 | 需要强烈色彩冲击的场景 |
| **Soft Light（柔光）** | 比叠加更柔和 | 微妙的色调调整 |
| **Flat Replace（纯色替换）** | 把所有像素替换成目标色，只保留透明度 | 纯色图标、剪影 |

## 安装

### 方法一 — 直接复制

1. 把 `Editor/` 文件夹复制到你 Unity 项目的 `Assets/` 目录下
2. 工具出现在 **Tools → Sprite Batch Recolor**

### 方法二 — Unity Package Manager（Git URL）

1. 打开 **Window → Package Manager**
2. 点击 **+** → **Add package from git URL...**
3. 粘贴本仓库的 URL

## 使用方法

1. 打开 **Tools → Sprite Batch Recolor**
2. 在 Hierarchy 或 Scene 中选中带有 SpriteRenderer 的物体
3. 选择目标颜色（可以用吸管从屏幕取色）
4. 选择混合模式 — 推荐 **Hue Shift**，效果最自然
5. 调节 Intensity 控制混合强度
6. 在预览面板查看效果
7. 点击 **Apply Recolor**

### 副本存储方式

```
Assets/Sprites/
├── player.png              ← 原图（不会被修改）
└── copy/
    └── player_copy.png     ← 染色后的副本
```

重复操作会覆盖已有副本，不会产生多余文件。

### 使用建议

- **Hue Shift** 几乎适用于所有场景，它只改变颜色而保留所有阴影、高光和纹理细节
- **Colorize** 适合原始素材是灰度或低饱和度的情况
- **Multiply / Screen** 适合对纯色或扁平化风格的 Sprite 做简单色调调整
- 用 **Intensity** 滑块可以做出微妙的色彩偏移效果

## 环境要求

- Unity 2021.3 LTS 或更高版本
- 2D Sprite 包（Unity 默认包含）

## 开源协议

MIT License — 详见 [LICENSE](LICENSE)
