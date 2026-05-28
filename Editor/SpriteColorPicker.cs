using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace SpriteTools
{
    /// <summary>
    /// Non-destructive batch sprite recolor tool with Photoshop-style blend modes.
    /// Supports SpriteRenderer and UI Image. Copies are saved alongside originals.
    /// </summary>
    public class SpriteColorPicker : EditorWindow
    {
        // ──────────────────────────────────────
        //  Blend Mode Definition
        // ──────────────────────────────────────

        private enum BlendMode
        {
            HueShift,
            Colorize,
            Multiply,
            Screen,
            Overlay,
            SoftLight,
            FlatReplace,
        }

        private struct BlendModeInfo
        {
            public string label;
            public string tooltip;
        }

        private static readonly BlendModeInfo[] ModeInfo =
        {
            new BlendModeInfo {
                label   = "Hue Shift",
                tooltip = "Only replaces the hue — keeps original brightness and saturation.\nMost natural result. Best for recoloring detailed sprites."
            },
            new BlendModeInfo {
                label   = "Colorize",
                tooltip = "Replaces hue + saturation, keeps only brightness.\nLike painting color onto a grayscale image."
            },
            new BlendModeInfo {
                label   = "Multiply",
                tooltip = "Darkens by multiplying original with target color.\nGood for tinting light sprites with a darker tone."
            },
            new BlendModeInfo {
                label   = "Screen",
                tooltip = "Lightens the image. Good for adding glow or tinting dark sprites."
            },
            new BlendModeInfo {
                label   = "Overlay",
                tooltip = "Boosts contrast while tinting.\nBright areas get brighter, dark areas darker."
            },
            new BlendModeInfo {
                label   = "Soft Light",
                tooltip = "Gentle version of Overlay. Subtle tinting without harsh contrast."
            },
            new BlendModeInfo {
                label   = "Flat Replace",
                tooltip = "Replaces all pixel colors with the target.\nOnly preserves alpha. Useful for solid-color icons."
            },
        };

        // ──────────────────────────────────────
        //  Preset
        // ──────────────────────────────────────

        [System.Serializable]
        private class Preset
        {
            public string name;
            public Color color;
            public BlendMode mode;
            public float intensity;
        }

        private List<Preset> _presets = new List<Preset>();
        private string _newPresetName = "";
        private bool _showPresets;
        private const string PresetEditorPrefsKey = "SpriteRecolor_Presets";

        // ──────────────────────────────────────
        //  State
        // ──────────────────────────────────────

        private Color     _targetColor = Color.red;
        private BlendMode _blendMode   = BlendMode.HueShift;
        private float     _intensity   = 1f;
        private Vector2   _scrollPos;
        private bool      _showPreview = true;

        // Preview cache
        private Texture2D _previewTex;
        private Color32[] _previewSrcPixels;
        private int       _previewW, _previewH;
        private int       _cachedSrcId;
        private Color     _cachedColor;
        private BlendMode _cachedMode;
        private float     _cachedIntensity;

        private const int PreviewMax = 128;

        // ──────────────────────────────────────
        //  Window Lifecycle
        // ──────────────────────────────────────

        [MenuItem("Tools/Sprite Batch Recolor")]
        private static void Open()
        {
            var win = GetWindow<SpriteColorPicker>("Sprite Recolor");
            win.minSize = new Vector2(300, 480);
        }

        private void OnEnable()
        {
            Selection.selectionChanged += MarkPreviewDirty;
            LoadPresets();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= MarkPreviewDirty;
            DestroyPreview();
            SavePresets();
        }

        private void MarkPreviewDirty() { _cachedSrcId = 0; Repaint(); }

        // ──────────────────────────────────────
        //  GUI
        // ──────────────────────────────────────

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawSettings();
            DrawPresetSection();
            DrawSelectionInfo();
            DrawPreviewSection();
            DrawApplyButton();
            DrawFooter();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSettings()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Recolor Settings", EditorStyles.boldLabel);

            _targetColor = EditorGUILayout.ColorField(
                new GUIContent("Target Color", "Click the eyedropper to pick from screen"),
                _targetColor, true, false, false);

            int modeIdx = (int)_blendMode;
            string[] labels = new string[ModeInfo.Length];
            for (int i = 0; i < ModeInfo.Length; i++) labels[i] = ModeInfo[i].label;

            modeIdx = EditorGUILayout.Popup(
                new GUIContent("Blend Mode", ModeInfo[modeIdx].tooltip),
                modeIdx, labels);
            _blendMode = (BlendMode)modeIdx;

            EditorGUILayout.HelpBox(ModeInfo[modeIdx].tooltip, MessageType.None);

            _intensity = EditorGUILayout.Slider(
                new GUIContent("Intensity", "0 = no effect, 1 = full blend"),
                _intensity, 0f, 1f);
        }

        // ──────────────────────────────────────
        //  Presets GUI
        // ──────────────────────────────────────

        private void DrawPresetSection()
        {
            EditorGUILayout.Space(4);
            _showPresets = EditorGUILayout.Foldout(_showPresets, $"Presets ({_presets.Count})", true);
            if (!_showPresets) return;

            // Save current as preset
            EditorGUILayout.BeginHorizontal();
            _newPresetName = EditorGUILayout.TextField(_newPresetName, GUILayout.ExpandWidth(true));
            GUI.enabled = !string.IsNullOrWhiteSpace(_newPresetName);
            if (GUILayout.Button("Save", GUILayout.Width(50)))
            {
                _presets.Add(new Preset
                {
                    name = _newPresetName.Trim(),
                    color = _targetColor,
                    mode = _blendMode,
                    intensity = _intensity,
                });
                _newPresetName = "";
                SavePresets();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // List presets
            int removeIdx = -1;
            for (int i = 0; i < _presets.Count; i++)
            {
                var p = _presets[i];
                EditorGUILayout.BeginHorizontal();

                // Color swatch
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(16, 16, GUILayout.Width(16)), p.color);
                GUILayout.Space(4);

                // Load button
                if (GUILayout.Button(p.name, EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(true)))
                {
                    _targetColor = p.color;
                    _blendMode = p.mode;
                    _intensity = p.intensity;
                }

                // Mode label
                GUILayout.Label(ModeInfo[(int)p.mode].label, EditorStyles.miniLabel, GUILayout.Width(70));

                // Delete
                if (GUILayout.Button("×", EditorStyles.miniButtonRight, GUILayout.Width(20)))
                    removeIdx = i;

                EditorGUILayout.EndHorizontal();
            }

            if (removeIdx >= 0)
            {
                _presets.RemoveAt(removeIdx);
                SavePresets();
            }
        }

        private void SavePresets()
        {
            var wrapper = new PresetListWrapper { items = _presets };
            EditorPrefs.SetString(PresetEditorPrefsKey, JsonUtility.ToJson(wrapper));
        }

        private void LoadPresets()
        {
            string json = EditorPrefs.GetString(PresetEditorPrefsKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<PresetListWrapper>(json);
                if (wrapper != null && wrapper.items != null)
                    _presets = wrapper.items;
            }
        }

        [System.Serializable]
        private class PresetListWrapper
        {
            public List<Preset> items = new List<Preset>();
        }

        // ──────────────────────────────────────
        //  Selection Info
        // ──────────────────────────────────────

        private void DrawSelectionInfo()
        {
            EditorGUILayout.Space(4);
            CountSelectedTargets(out int spriteCount, out int imageCount);
            int total = spriteCount + imageCount;

            if (total > 0)
            {
                string msg = "";
                if (spriteCount > 0) msg += $"{spriteCount} SpriteRenderer(s)";
                if (imageCount > 0)
                {
                    if (msg.Length > 0) msg += " + ";
                    msg += $"{imageCount} UI Image(s)";
                }
                EditorGUILayout.HelpBox($"Selected: {msg}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Select objects with SpriteRenderer or UI Image", MessageType.Warning);
            }
        }

        private void DrawApplyButton()
        {
            EditorGUILayout.Space(8);
            CountSelectedTargets(out int sc, out int ic);
            GUI.enabled = sc + ic > 0;
            if (GUILayout.Button("Apply Recolor", GUILayout.Height(32)))
                ApplyRecolor();
            GUI.enabled = true;
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Original textures are never modified.\n" +
                "Copies are saved in a 'copy' subfolder.\n" +
                "Re-applying overwrites existing copies (no duplicates).",
                MessageType.Info);
        }

        // ──────────────────────────────────────
        //  Preview
        // ──────────────────────────────────────

        private void DrawPreviewSection()
        {
            _showPreview = EditorGUILayout.Foldout(_showPreview, "Preview", true);
            if (!_showPreview) return;

            Texture2D srcTex = GetFirstSelectedTexture(out int srcId);
            if (srcTex == null)
            {
                EditorGUILayout.HelpBox("Select a SpriteRenderer or UI Image to preview.", MessageType.None);
                return;
            }

            if (srcId != _cachedSrcId)
            {
                CachePreviewSource(srcTex);
                _cachedSrcId = srcId;
                _cachedColor = default;
            }

            bool paramsDirty = _targetColor != _cachedColor
                            || _blendMode  != _cachedMode
                            || !Mathf.Approximately(_intensity, _cachedIntensity);

            if (_previewTex == null || paramsDirty)
            {
                RebuildPreviewTexture();
                _cachedColor     = _targetColor;
                _cachedMode      = _blendMode;
                _cachedIntensity = _intensity;
            }

            if (_previewTex == null) return;

            float halfW = Mathf.Min(position.width * 0.42f, PreviewMax);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var r1 = GUILayoutUtility.GetRect(halfW, halfW);
            GUILayout.Space(8);
            var r2 = GUILayoutUtility.GetRect(halfW, halfW);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUI.DrawTextureTransparent(r1, srcTex, ScaleMode.ScaleToFit);
            EditorGUI.DrawTextureTransparent(r2, _previewTex, ScaleMode.ScaleToFit);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Original", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(halfW));
            GUILayout.Space(8);
            GUILayout.Label("Result",   EditorStyles.centeredGreyMiniLabel, GUILayout.Width(halfW));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private Texture2D GetFirstSelectedTexture(out int id)
        {
            id = 0;
            if (Selection.gameObjects == null) return null;
            foreach (var go in Selection.gameObjects)
            {
                // SpriteRenderer
                var sr = go.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.sprite != null && sr.sprite.texture != null)
                {
                    id = sr.sprite.texture.GetInstanceID();
                    return sr.sprite.texture;
                }
                // UI Image
                var img = go.GetComponentInChildren<UnityEngine.UI.Image>();
                if (img != null && img.sprite != null && img.sprite.texture != null)
                {
                    id = img.sprite.texture.GetInstanceID();
                    return img.sprite.texture;
                }
            }
            return null;
        }

        private void CachePreviewSource(Texture2D tex)
        {
            float scale = Mathf.Min(1f, (float)PreviewMax / Mathf.Max(tex.width, tex.height));
            _previewW = Mathf.Max(1, Mathf.RoundToInt(tex.width  * scale));
            _previewH = Mathf.Max(1, Mathf.RoundToInt(tex.height * scale));

            var rt = RenderTexture.GetTemporary(_previewW, _previewH, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(tex, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;

            var tmp = new Texture2D(_previewW, _previewH, TextureFormat.RGBA32, false);
            tmp.ReadPixels(new Rect(0, 0, _previewW, _previewH), 0, 0);
            tmp.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            _previewSrcPixels = tmp.GetPixels32();
            Object.DestroyImmediate(tmp);
        }

        private void RebuildPreviewTexture()
        {
            if (_previewSrcPixels == null) return;

            if (_previewTex == null || _previewTex.width != _previewW || _previewTex.height != _previewH)
            {
                DestroyPreview();
                _previewTex = new Texture2D(_previewW, _previewH, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Point };
            }

            var result = new Color32[_previewSrcPixels.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = Blend(_previewSrcPixels[i], _targetColor, _blendMode, _intensity);

            _previewTex.SetPixels32(result);
            _previewTex.Apply();
        }

        private void DestroyPreview()
        {
            if (_previewTex != null) { Object.DestroyImmediate(_previewTex); _previewTex = null; }
            _previewSrcPixels = null;
        }

        // ──────────────────────────────────────
        //  Blend Core
        // ──────────────────────────────────────

        private static Color32 Blend(Color32 src, Color target, BlendMode mode, float intensity)
        {
            if (src.a < 3) return src;

            float sr = src.r / 255f, sg = src.g / 255f, sb = src.b / 255f;
            float tr = target.r,     tg = target.g,     tb = target.b;
            float rr, rg, rb;

            switch (mode)
            {
                case BlendMode.HueShift:
                    HueShift(sr, sg, sb, target, out rr, out rg, out rb);
                    break;

                case BlendMode.Colorize:
                    Colorize(sr, sg, sb, target, out rr, out rg, out rb);
                    break;

                case BlendMode.Multiply:
                    rr = sr * tr; rg = sg * tg; rb = sb * tb;
                    break;

                case BlendMode.Screen:
                    rr = 1f - (1f - sr) * (1f - tr);
                    rg = 1f - (1f - sg) * (1f - tg);
                    rb = 1f - (1f - sb) * (1f - tb);
                    break;

                case BlendMode.Overlay:
                    rr = sr < 0.5f ? 2f * sr * tr : 1f - 2f * (1f - sr) * (1f - tr);
                    rg = sg < 0.5f ? 2f * sg * tg : 1f - 2f * (1f - sg) * (1f - tg);
                    rb = sb < 0.5f ? 2f * sb * tb : 1f - 2f * (1f - sb) * (1f - tb);
                    break;

                case BlendMode.SoftLight:
                    rr = (1f - 2f * tr) * sr * sr + 2f * tr * sr;
                    rg = (1f - 2f * tg) * sg * sg + 2f * tg * sg;
                    rb = (1f - 2f * tb) * sb * sb + 2f * tb * sb;
                    break;

                default: // FlatReplace
                    rr = tr; rg = tg; rb = tb;
                    break;
            }

            if (intensity < 1f)
            {
                float inv = 1f - intensity;
                rr = sr * inv + rr * intensity;
                rg = sg * inv + rg * intensity;
                rb = sb * inv + rb * intensity;
            }

            return new Color32(
                (byte)(Mathf.Clamp01(rr) * 255f),
                (byte)(Mathf.Clamp01(rg) * 255f),
                (byte)(Mathf.Clamp01(rb) * 255f),
                src.a);
        }

        private static void HueShift(float sr, float sg, float sb, Color target,
                                     out float rr, out float rg, out float rb)
        {
            Color.RGBToHSV(new Color(sr, sg, sb), out _, out float srcS, out float srcV);
            Color.RGBToHSV(target,                out float tH, out _, out _);

            Color result = Color.HSVToRGB(tH, srcS, srcV);
            rr = result.r; rg = result.g; rb = result.b;
        }

        private static void Colorize(float sr, float sg, float sb, Color target,
                                     out float rr, out float rg, out float rb)
        {
            Color.RGBToHSV(new Color(sr, sg, sb), out _, out _, out float srcV);
            Color.RGBToHSV(target,                out float tH, out float tS, out _);

            Color result = Color.HSVToRGB(tH, tS, srcV);
            rr = result.r; rg = result.g; rb = result.b;
        }

        // ──────────────────────────────────────
        //  Apply — collect targets
        // ──────────────────────────────────────

        private struct RecolorTarget
        {
            public Component component;  // SpriteRenderer or Image
            public Sprite sprite;
            public Texture2D texture;
        }

        private static void CountSelectedTargets(out int spriteCount, out int imageCount)
        {
            spriteCount = 0;
            imageCount = 0;
            foreach (var go in Selection.gameObjects)
            {
                spriteCount += go.GetComponentsInChildren<SpriteRenderer>().Length;
                imageCount  += go.GetComponentsInChildren<UnityEngine.UI.Image>().Length;
            }
        }

        private static List<RecolorTarget> GatherTargets()
        {
            var targets = new List<RecolorTarget>();
            foreach (var go in Selection.gameObjects)
            {
                foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (sr.sprite != null && sr.sprite.texture != null)
                        targets.Add(new RecolorTarget { component = sr, sprite = sr.sprite, texture = sr.sprite.texture });
                }
                foreach (var img in go.GetComponentsInChildren<UnityEngine.UI.Image>())
                {
                    if (img.sprite != null && img.sprite.texture != null)
                        targets.Add(new RecolorTarget { component = img, sprite = img.sprite, texture = img.sprite.texture });
                }
            }
            return targets;
        }

        // ──────────────────────────────────────
        //  Apply — main
        // ──────────────────────────────────────

        private void ApplyRecolor()
        {
            var targets = GatherTargets();
            if (targets.Count == 0) return;

            var processed = new Dictionary<string, string>();
            int texCount = 0;

            Undo.SetCurrentGroupName("Sprite Batch Recolor");
            int undoGroup = Undo.GetCurrentGroup();

            // ── Write recolored PNGs ──
            for (int i = 0; i < targets.Count; i++)
            {
                string srcPath = TraceSourcePath(AssetDatabase.GetAssetPath(targets[i].texture));
                if (string.IsNullOrEmpty(srcPath) || processed.ContainsKey(srcPath)) continue;

                EditorUtility.DisplayProgressBar("Recoloring",
                    Path.GetFileName(srcPath), (float)i / targets.Count);

                string copyPath = BuildCopyPath(srcPath);
                EnsureDirectory(copyPath);
                WriteBlendedPNG(srcPath, copyPath);
                processed[srcPath] = copyPath;
                texCount++;
            }
            EditorUtility.ClearProgressBar();

            if (texCount == 0) return;

            // ── Batch import ──
            AssetDatabase.StartAssetEditing();
            try { foreach (var kv in processed) AssetDatabase.ImportAsset(kv.Value, ImportAssetOptions.ForceUpdate); }
            finally { AssetDatabase.StopAssetEditing(); }

            foreach (var kv in processed) CopyImportSettings(kv.Key, kv.Value);

            // ── Assign sprites ──
            int srCount = 0, imgCount = 0;
            foreach (var t in targets)
            {
                string srcPath = TraceSourcePath(AssetDatabase.GetAssetPath(t.texture));
                if (!processed.TryGetValue(srcPath, out string copyPath)) continue;

                Sprite match = FindMatchingSprite(t.sprite, copyPath);
                if (match == null)
                {
                    Debug.LogWarning($"[Recolor] Failed to load sprite from: {copyPath}");
                    continue;
                }

                if (t.component is SpriteRenderer sr)
                {
                    Undo.RecordObject(sr, "Recolor Sprite");
                    sr.sprite = match;
                    sr.color = Color.white;
                    EditorUtility.SetDirty(sr);
                    srCount++;
                }
                else if (t.component is UnityEngine.UI.Image img)
                {
                    Undo.RecordObject(img, "Recolor Image");
                    img.sprite = match;
                    img.color = Color.white;
                    EditorUtility.SetDirty(img);
                    imgCount++;
                }
            }

            Undo.CollapseUndoOperations(undoGroup);

            string log = $"[Recolor] Done — {texCount} texture(s)";
            if (srCount > 0) log += $", {srCount} SpriteRenderer(s)";
            if (imgCount > 0) log += $", {imgCount} UI Image(s)";
            Debug.Log(log);
        }

        // ──────────────────────────────────────
        //  Texture I/O
        // ──────────────────────────────────────

        private void WriteBlendedPNG(string srcPath, string dstPath)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(srcPath);
            if (tex == null) return;

            Color32[] px = ReadPixels32ViaRT(tex);
            for (int i = 0; i < px.Length; i++)
                px[i] = Blend(px[i], _targetColor, _blendMode, _intensity);

            var output = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            output.SetPixels32(px);
            output.Apply();

            string fullPath = Path.Combine(Application.dataPath, "..", dstPath).Replace("\\", "/");
            File.WriteAllBytes(fullPath, output.EncodeToPNG());
            Object.DestroyImmediate(output);
        }

        private static Color32[] ReadPixels32ViaRT(Texture2D source)
        {
            var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);

            var prev = RenderTexture.active;
            RenderTexture.active = rt;

            var tmp = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            tmp.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            tmp.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            Color32[] pixels = tmp.GetPixels32();
            Object.DestroyImmediate(tmp);
            return pixels;
        }

        // ──────────────────────────────────────
        //  Import Settings
        // ──────────────────────────────────────

        private static void CopyImportSettings(string srcPath, string dstPath)
        {
            var src = AssetImporter.GetAtPath(srcPath) as TextureImporter;
            var dst = AssetImporter.GetAtPath(dstPath) as TextureImporter;
            if (dst == null) { Debug.LogWarning($"[Recolor] Importer not found: {dstPath}"); return; }

            dst.textureType        = TextureImporterType.Sprite;
            dst.textureCompression = TextureImporterCompression.Uncompressed;

            if (src != null)
            {
                dst.spriteImportMode    = src.spriteImportMode;
                dst.spritePixelsPerUnit = src.spritePixelsPerUnit;
                dst.spritePivot         = src.spritePivot;
                dst.spriteBorder        = src.spriteBorder;
                dst.filterMode          = src.filterMode;

                if (src.spriteImportMode == SpriteImportMode.Multiple)
                    dst.spritesheet = src.spritesheet;
            }

            dst.SaveAndReimport();
        }

        private static Sprite FindMatchingSprite(Sprite original, string copyPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(copyPath);
            if (assets == null) return null;

            foreach (var a in assets)
                if (a is Sprite sp && sp.name == original.name) return sp;

            foreach (var a in assets)
                if (a is Sprite sp) return sp;

            return null;
        }

        // ──────────────────────────────────────
        //  Path Utilities
        // ──────────────────────────────────────

        private static string TraceSourcePath(string path)
        {
            string dir  = Path.GetDirectoryName(path).Replace("\\", "/");
            string name = Path.GetFileNameWithoutExtension(path);
            string ext  = Path.GetExtension(path);

            int idx = name.IndexOf("_copy");
            string baseName = idx >= 0 ? name.Substring(0, idx) : name;

            while (dir.EndsWith("/copy") || Path.GetFileName(dir) == "copy")
                dir = Path.GetDirectoryName(dir).Replace("\\", "/");

            string candidate = $"{dir}/{baseName}{ext}";
            string full = Path.Combine(Application.dataPath, "..", candidate).Replace("\\", "/");
            return File.Exists(full) ? candidate : path;
        }

        private static string BuildCopyPath(string srcPath)
        {
            string dir  = Path.GetDirectoryName(srcPath).Replace("\\", "/");
            string name = Path.GetFileNameWithoutExtension(srcPath);
            string ext  = Path.GetExtension(srcPath);
            return $"{dir}/copy/{name}_copy{ext}";
        }

        private static void EnsureDirectory(string assetPath)
        {
            string dir = Path.GetDirectoryName(assetPath).Replace("\\", "/");
            string full = Path.Combine(Application.dataPath, "..", dir).Replace("\\", "/");
            if (!Directory.Exists(full)) Directory.CreateDirectory(full);
        }
    }
}
