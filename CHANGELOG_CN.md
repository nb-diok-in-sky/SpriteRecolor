# 更新日志

## [2.1.0] - 2025-05-28

### 新增
- **Hue Shift（色相替换）** 混合模式 — 通过 HSV 转换只替换色相，保留原图的饱和度和明暗。对有细节的 Sprite 效果最自然。
- **Colorize（上色）** 混合模式 — 替换色相和饱和度，只保留明暗。适合给灰度素材上色。
- 每个混合模式在 UI 中显示功能说明，hover 查看详细 tooltip。
- 编辑器窗口加入 ScrollView，任意窗口大小都能正常使用。

### 变更
- 重新排列混合模式顺序：Hue Shift 排第一（最常用），Flat Replace 排最后（最粗暴）。
- 重构 GUI 代码，拆分为 `DrawSettings`、`DrawPreviewSection`、`DrawApplyButton`、`DrawFooter` 四个方法。
- 路径工具方法重命名：`TraceSourcePath`、`BuildCopyPath`、`EnsureDirectory`。
- 移除旧的 `ColorOnly` 模式（被更强的 Hue Shift 和 Colorize 替代）。

## [2.0.0] - 2025-05-28

### 新增
- **6 种混合模式**：纯色替换、正片叠底、滤色、叠加、柔光、颜色。
- **实时预览面板** — 左右对比原图和效果图，参数变化时即时刷新。
- **强度滑块**（0~1）— 控制混合程度，相当于 Photoshop 中的图层不透明度。
- 批量处理时显示**进度条**（`EditorUtility.DisplayProgressBar`）。
- 支持 **Sprite Multiple 模式** — 从源 importer 复制 `spritesheet` 切片数据。
- 复制源图的 **Pivot** 和 **Border**（9-slice 切片信息）。
- `FindMatchingSprite` — 按名字匹配图集中的子 Sprite。

### 变更
- 包裹在 `namespace SpriteTools` 中，避免全局命名冲突。
- UI 语言从中文改为英文（代码注释保留中英双语）。
- 像素处理从 `Color[]` 升级到 `Color32[]`，大纹理处理速度提升约 4 倍。
- `spriteImportMode` 从源图复制，不再强制设为 `Single`。
- 设定最小窗口尺寸（300×420）。

## [1.1.0] - 2025-05-27

### 修复
- **修复 "Texture not readable" 报错** — 废弃直接 `GetPixels()` 的方式，改用 `ReadPixelsViaRT()` 通过 RenderTexture + `Graphics.Blit` 读取像素，完全绕开 `isReadable` 导入设置的限制。
- 修复副本创建步骤中的同一可读性问题。

### 变更
- 批量导入改用 `AssetDatabase.StartAssetEditing()` / `StopAssetEditing()` 包裹，避免逐个触发导入回调。

## [1.0.0] - 2025-05-26

### 新增
- 初始版本。
- 纯色替换 — 将所有像素的 RGB 替换为目标颜色，保留 Alpha。
- 批量处理 — 选中多个 GameObject，一键染色所有子级 SpriteRenderer。
- 无损工作流 — 副本保存在 `copy/` 子文件夹中，原图不受影响。
- 幂等性 — 重复执行会覆盖已有副本，不会产生多余文件。
- 源路径追溯 — 防止 `copy/copy/copy` 嵌套链。
- Unity Undo 集成，操作按组合并。
