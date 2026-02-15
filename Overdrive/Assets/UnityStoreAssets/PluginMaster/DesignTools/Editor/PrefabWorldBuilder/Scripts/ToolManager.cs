/*
Copyright (c) Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;

namespace PluginMaster
{
    [UnityEditor.InitializeOnLoad]
    public static class ToolManager
    {
        public enum PaintTool
        {
            NONE,
            PIN,
            BRUSH,
            GRAVITY,
            LINE,
            SHAPE,
            TILING,
            REPLACER,
            ERASER,
            SELECTION,
            CIRCLE_SELECT,
            EXTRUDE,
            MIRROR,
            FLOOR,
            WALL
        }

        private static PaintTool _tool = ToolManager.PaintTool.NONE;
        public enum ToolState { NONE, PREVIEW, EDIT, PERSISTENT }

        private static bool _editMode = false;
        public static Action<PaintTool> OnToolChange;
        public static Action OnToolModeChanged;
        public static bool _triggerToolChangeEvent = true;
        static ToolManager()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            PaletteManager.OnBrushSelectionChanged += TilingManager.settings.UpdateCellSize;
        }

        public static bool editMode
        {
            get => _editMode;
            set
            {
                if (_editMode == value) return;
                _editMode = value;
                if (OnToolModeChanged != null) OnToolModeChanged();
            }
        }
        public static ToolManager.PaintTool tool
        {
            get => _tool;
            set
            {
                if (_tool == value) return;
                var prevTool = _tool;
                _tool = value;
                if (_tool != prevTool)
                {
                    BoundsUtils.ClearBoundsDictionaries();
                    if (_triggerToolChangeEvent && OnToolChange != null) OnToolChange(prevTool);
                    _editMode = false;
                    _triggerToolChangeEvent = true;
                    PWBPreferences.SelectToolCategory(_tool);
                    PWBItemsWindow.RepainWindow();
                }

                switch (_tool)
                {
                    case PaintTool.PIN:
                        PWBIO.UpdateOctree();
                        PWBIO.ResetPinValues();
                        if (PinManager.settings.ignoreSceneColliders) PWBIO.UpdateSceneColliderSet();
                        break;
                    case PaintTool.BRUSH:
                        PWBIO.UpdateOctree();
                        if (BrushManager.settings.ignoreSceneColliders) PWBIO.UpdateSceneColliderSet();
                        break;
                    case PaintTool.GRAVITY:
                        PWBIO.InitializeGravityTool();
                        break;
                    case PaintTool.ERASER:
                        PWBIO.UpdateOctree();
                        break;
                    case PaintTool.REPLACER:
                        PWBIO.UpdateOctree();
                        PWBIO.ResetReplacer();
                        break;
                    case PaintTool.EXTRUDE:
                        SelectionManager.UpdateSelection();
                        PWBIO.ResetUnityCurrentTool();
                        PWBIO.ResetExtrudeState(false);
                        break;
                    case PaintTool.LINE:
                        PWBIO.InitializeLineTool();
                        break;
                    case PaintTool.SHAPE:
                        PWBIO.InitializeShapeTool();
                        break;
                    case PaintTool.TILING:
                        PWBIO.InitializeTilingTool();
                        break;
                    case PaintTool.SELECTION:
                        PWBIO.InitializeSelectionTool();
                        break;
                    case PaintTool.CIRCLE_SELECT:
                        PWBIO.UpdateOctree();
                        break;
                    case PaintTool.MIRROR:
                        SelectionManager.UpdateSelection();
                        PWBIO.InitializeMirrorPose();
                        break;
                    case PaintTool.FLOOR:
                        PWBIO.OnFloorEnabled();
                        break;
                    case PaintTool.WALL:
                        PWBIO.OnWallEnabled();
                        break;
                    case PaintTool.NONE:
                        PWBIO.ResetUnityCurrentTool();
                        PWBIO.ResetReplacer();
                        PWBCore.DestroyTempColliders();
                        ApplicationEventHandler.hierarchyChangedWhileUsingTools = false;
                        break;
                    default: break;
                }

                if (_tool != PaintTool.NONE)
                {
                    PWBIO.SaveUnityCurrentTool();
                    if (PWBCore.staticData.openToolPropertiesWhenAToolIsSelected) ToolProperties.ShowWindow();
                    PaletteManager.pickingBrushes = false;
                }

                if (_tool == PaintTool.BRUSH || _tool == PaintTool.PIN || _tool == PaintTool.GRAVITY
                    || _tool == PaintTool.REPLACER || _tool == PaintTool.ERASER || _tool == PaintTool.LINE
                    || _tool == PaintTool.SHAPE || _tool == PaintTool.TILING || _tool == PaintTool.FLOOR
                    || _tool == PaintTool.WALL)
                {
                    PrefabPalette.ShowWindow();
                    SelectionManager.UpdateSelection();
                    if (_tool == PaintTool.BRUSH || _tool == PaintTool.PIN || _tool == PaintTool.GRAVITY
                        || _tool == PaintTool.REPLACER || _tool == PaintTool.FLOOR || _tool == PaintTool.WALL)
                        BrushstrokeManager.UpdateBrushstroke();
                    PWBIO.ResetAutoParent();
                }

                if (_tool == PaintTool.LINE || _tool == PaintTool.SHAPE
                    || _tool == PaintTool.TILING || _tool == PaintTool.NONE)
                    PWBItemsWindow.RepainWindow();

                ToolProperties.RepainWindow();
                if (BrushProperties.instance != null) BrushProperties.instance.Repaint();
                if (UnityEditor.SceneView.sceneViews.Count > 0) ((UnityEditor.SceneView)
                        UnityEditor.SceneView.sceneViews[0]).Focus();
            }
        }

        public static void DeselectTool(bool triggerToolChangeEvent = true)
        {
            _triggerToolChangeEvent = triggerToolChangeEvent;
            if (tool == PaintTool.REPLACER) PWBIO.ResetReplacer();
            tool = PaintTool.NONE;
            PWBIO.ResetUnityCurrentTool();
            PWBToolbar.RepaintWindow();
        }

        private static void OnSceneClosing(UnityEngine.SceneManagement.Scene scene, bool removingScene)
        {
            PWBCore.staticData.SaveAndUpdateVersion();
            DeselectTool();
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            DeselectTool();
            PWBCore.DestroyTempColliders();
        }

        public static void OnPaletteClosed()
        {
            if (tool != PaintTool.ERASER && tool != PaintTool.EXTRUDE)
                tool = PaintTool.NONE;
        }

        public static PaintTool GetToolFromSettings(IToolSettings settings)
        {
            if (settings is PinSettings) return PaintTool.PIN;
            if (settings is GravityToolSettings) return PaintTool.GRAVITY;
            if (settings is BrushToolSettings) return PaintTool.BRUSH;
            if (settings is ShapeSettings) return PaintTool.SHAPE;
            if (settings is LineSettings) return PaintTool.LINE;
            if (settings is TilingSettings) return PaintTool.TILING;
            if (settings is ReplacerSettings) return PaintTool.REPLACER;
            if (settings is EraserSettings) return PaintTool.ERASER;
            if (settings is SelectionToolSettings) return PaintTool.SELECTION;
            if (settings is CircleSelectSettings) return PaintTool.CIRCLE_SELECT;
            if (settings is ExtrudeSettings) return PaintTool.EXTRUDE;
            if (settings is MirrorSettings) return PaintTool.MIRROR;
            if (settings is FloorSettings) return PaintTool.FLOOR;
            if (settings is WallSettings) return PaintTool.WALL;
            return PaintTool.NONE;
        }
        public static PaintTool GetToolFromSettings(IPaintToolSettings settings)
        {
            if (settings is PinSettings) return PaintTool.PIN;
            if (settings is GravityToolSettings) return PaintTool.GRAVITY;
            if (settings is BrushToolSettings) return PaintTool.BRUSH;
            if (settings is ShapeSettings) return PaintTool.SHAPE;
            if (settings is LineSettings) return PaintTool.LINE;
            if (settings is TilingSettings) return PaintTool.TILING;
            if (settings is ReplacerSettings) return PaintTool.REPLACER;
            if (settings is EraserSettings) return PaintTool.ERASER;
            if (settings is SelectionToolSettings) return PaintTool.SELECTION;
            if (settings is CircleSelectSettings) return PaintTool.CIRCLE_SELECT;
            if (settings is ExtrudeSettings) return PaintTool.EXTRUDE;
            if (settings is MirrorSettings) return PaintTool.MIRROR;
            if (settings is FloorSettings) return PaintTool.FLOOR;
            if (settings is WallSettings) return PaintTool.WALL;
            return PaintTool.NONE;
        }

        public static IToolSettings GetSettingsFromTool(PaintTool tool)
        {
            switch (tool)
            {
                case PaintTool.PIN: return PinManager.settings;
                case PaintTool.BRUSH: return BrushManager.settings;
                case PaintTool.GRAVITY: return GravityToolManager.settings;
                case PaintTool.REPLACER: return ReplacerManager.settings;
                case PaintTool.ERASER: return EraserManager.settings;
                case PaintTool.EXTRUDE: return ExtrudeManager.settings;
                case PaintTool.LINE: return LineManager.settings;
                case PaintTool.SHAPE: return ShapeManager.settings;
                case PaintTool.TILING: return TilingManager.settings;
                case PaintTool.SELECTION: return SelectionToolManager.settings;
                case PaintTool.CIRCLE_SELECT: return CircleSelectManager.settings;
                case PaintTool.MIRROR: return MirrorManager.settings;
                case PaintTool.FLOOR: return FloorManager.settings;
                case PaintTool.WALL: return WallManager.settings;
                default: return null;
            }
        }

        public static IPersistentToolManager GetCurrentPersistentToolManager()
        {
            switch (tool)
            {
                case PaintTool.LINE: return LineManager.instance;
                case PaintTool.SHAPE: return ShapeManager.instance;
                case PaintTool.TILING: return TilingManager.instance;
                default: return null;
            }
        }

        public static IPersistentData[] GetCurrentPersistentToolData()
        {
            var manager = GetCurrentPersistentToolManager();
            if (manager == null) return null;
            return manager.GetItems();
        }

        public static bool IsCurrentToolPersistent()
            => tool == PaintTool.LINE || tool == PaintTool.SHAPE || tool == PaintTool.TILING;
    }
}