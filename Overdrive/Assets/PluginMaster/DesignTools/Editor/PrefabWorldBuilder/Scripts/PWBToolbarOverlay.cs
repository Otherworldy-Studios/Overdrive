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
#if UNITY_2021_2_OR_NEWER
using UnityEngine;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace PluginMaster
{
    #region TOGGLE MANAGER
    public static class ToggleManager
    {
        private static System.Collections.Generic.Dictionary<ToolManager.PaintTool, IPWBToogle> _toogles = null;
        private static System.Collections.Generic.Dictionary<ToolManager.PaintTool, IPWBToogle> toogles
        {
            get
            {
                if (_toogles == null)
                {
                    _toogles = new System.Collections.Generic.Dictionary<ToolManager.PaintTool, IPWBToogle>()
                    {
                        {ToolManager.PaintTool.PIN,  PinToggle.instance },
                        {ToolManager.PaintTool.BRUSH, BrushToggle.instance},
                        {ToolManager.PaintTool.GRAVITY, GravityToggle.instance},
                        {ToolManager.PaintTool.LINE, LineToggle.instance},
                        {ToolManager.PaintTool.SHAPE, ShapeToggle.instance},
                        {ToolManager.PaintTool.TILING, TilingToggle.instance},
                        {ToolManager.PaintTool.REPLACER, ReplacerToggle.instance},
                        {ToolManager.PaintTool.ERASER, EraserToggle.instance},
                        {ToolManager.PaintTool.SELECTION, SelectionToggle.instance},
                        {ToolManager.PaintTool.CIRCLE_SELECT, CircleSelectToggle.instance},
                        {ToolManager.PaintTool.EXTRUDE, ExtrudeToggle.instance},
                        {ToolManager.PaintTool.MIRROR, MirrorToggle.instance}
                    };
                }
                return _toogles;
            }
        }

        public static void DeselectOthers(string id)
        {
            foreach (var toggle in toogles.Values)
            {
                if (toggle == null) continue;
                if (id != toggle.id && toggle.value) toggle.value = false;
            }
        }

        public static string GetTooltip(string tooltip, string keyCombination) => tooltip + " ... " + keyCombination;

        public static string iconPath => UnityEditor.EditorGUIUtility.isProSkin ? "Sprites/" : "Sprites/LightTheme/";
    }
    #endregion
    #region TOGGLE BASE
    interface IPWBToogle
    {
        public string id { get; }
        public ToolManager.PaintTool tool { get; }
        public bool value { get; set; }
    }

    public class PWBToolbarToggle : UnityEditor.Toolbars.EditorToolbarToggle
    {
        protected string _iconName = string.Empty;
        protected async void DoLoadIcon()
        {
            await System.Threading.Tasks.Task.Delay(1000);
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
        }
    }


    public abstract class ToolToggleBase<T> : PWBToolbarToggle,
        IPWBToogle where T : UnityEditor.Toolbars.EditorToolbarToggle, new()
    {
        private static ToolToggleBase<T> _instance = null;
        public static ToolToggleBase<T> instance => _instance;
        public abstract string id { get; }
        public abstract ToolManager.PaintTool tool { get; }
       
        public ToolToggleBase()
        {
            _instance = this;
            this.RegisterValueChangedCallback(OnValueChange);
            ToolManager.OnToolChange += OnToolChange;
        }

        private void OnToolChange(ToolManager.PaintTool prevTool)
        {
            if (tool == prevTool || tool == ToolManager.tool) PWBIO.OnToolChange(prevTool);
            if (tool == prevTool && tool != ToolManager.tool && value) value = false;
            if (tool == ToolManager.tool && !value) value = true;
        }

        private void OnValueChange(UnityEngine.UIElements.ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                ToolManager.tool = tool;
                ToggleManager.DeselectOthers(id);
            }
            else if (tool == ToolManager.tool) ToolManager.DeselectTool();
        }
    }
    #endregion
    #region TOOLBAR OVERLAY MANAGER
    public static class ToolbarOverlayManager
    {
        public static void OnToolbarDisplayedChanged()
        {
            if (!PWBCore.staticData.closeAllWindowsWhenClosingTheToolbar) return;
            if (PWBPropPlacementToolbarOverlay.IsDisplayed) return;
            if (PWBSelectionToolbarOverlay.IsDisplayed) return;
            if (PWBGridToolbarOverlay.IsDisplayed) return;
            if (ModularEnvironmentsToolbarOverlay.IsDisplayed) return;
            if (SettingsAndDocsToolbarOverlay.IsDisplayed) return;
            PWBIO.CloseAllWindows();
        }
    }
    #endregion
    #region MODULAR ENVIRONMENTS TOOLS
    
    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class WallsToggle : ToolToggleBase<WallsToggle>
    {
        public const string ID = "PWB/WallsToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.WALL;
        public WallsToggle() : base()
        {
            _iconName = "Walls";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Walls", PWBSettings.shortcuts.toolbarWallToggle.combination.ToString());
        }
    }
    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class FloorsToggle : ToolToggleBase<FloorsToggle>
    {
        public const string ID = "PWB/FloorsToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.FLOOR;
        public FloorsToggle() : base()
        {
            _iconName = "Floors";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Floors", PWBSettings.shortcuts.toolbarFloorToggle.combination.ToString());
        }
    }
    [UnityEditor.Overlays.Overlay(typeof(UnityEditor.SceneView), "PWB/Modular", true)]
    public class ModularEnvironmentsToolbarOverlay : UnityEditor.Overlays.ToolbarOverlay
    {
        private static bool _isDisplayed = false;
        ModularEnvironmentsToolbarOverlay() : base(FloorsToggle.ID, WallsToggle.ID)
        {
            displayedChanged += OndisplayedChanged;
#if UNITY_2022_2_OR_NEWER
            collapsedIcon = Resources.Load<Texture2D>(ToggleManager.iconPath + "Floors");
#endif
        }

        private void OndisplayedChanged(bool value)
        {
            _isDisplayed = value;
            ToolbarOverlayManager.OnToolbarDisplayedChanged();
        }

        public static bool IsDisplayed => _isDisplayed;
    }
    #endregion
    #region PROP PLACEMENT TOOLS
    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class PinToggle : ToolToggleBase<PinToggle>
    {
        public const string ID = "PWB/PinToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.PIN;
        public PinToggle() : base()
        {
            _iconName = "Pin";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Pin", PWBSettings.shortcuts.toolbarPinToggle.combination.ToString());
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class BrushToggle : ToolToggleBase<BrushToggle>
    {
        public const string ID = "PWB/BrushToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.BRUSH;
        public BrushToggle() : base()
        {
            _iconName = "Brush";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Brush", PWBSettings.shortcuts.toolbarBrushToggle.combination.ToString());
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class GravityToggle : ToolToggleBase<GravityToggle>
    {
        public const string ID = "PWB/GravityToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.GRAVITY;
        public GravityToggle() : base()
        {
            _iconName = "GravityTool";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Gravity Brush",
                PWBSettings.shortcuts.toolbarGravityToggle.combination.ToString());
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class LineToggle : ToolToggleBase<LineToggle>
    {
        public const string ID = "PWB/LineToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.LINE;
        public LineToggle() : base()
        {
            _iconName = "Line";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Line", PWBSettings.shortcuts.toolbarLineToggle.combination.ToString());
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class ShapeToggle : ToolToggleBase<ShapeToggle>
    {
        public const string ID = "PWB/ShapeToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.SHAPE;
        public ShapeToggle() : base()
        {
            _iconName = "Shape";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Shape", PWBSettings.shortcuts.toolbarShapeToggle.combination.ToString());
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class TilingToggle : ToolToggleBase<TilingToggle>
    {
        public const string ID = "PWB/TilingToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.TILING;
        public TilingToggle() : base()
        {
            _iconName = "Tiling";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Tiling", PWBSettings.shortcuts.toolbarTilingToggle.combination.ToString());
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class ReplacerToggle : ToolToggleBase<ReplacerToggle>
    {
        public const string ID = "PWB/ReplacerToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.REPLACER;
        public ReplacerToggle() : base()
        {
            _iconName = "Replace";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Replacer", PWBSettings.shortcuts.toolbarReplacerToggle.combination.ToString());
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class EraserToggle : ToolToggleBase<EraserToggle>
    {
        public const string ID = "PWB/EraserToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.ERASER;
        public EraserToggle() : base()
        {
            _iconName = "Eraser";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Eraser", PWBSettings.shortcuts.toolbarEraserToggle.combination.ToString());
        }
    }

    
    [UnityEditor.Overlays.Overlay(typeof(UnityEditor.SceneView), "PWB/Prop Placement", true)]
    public class PWBPropPlacementToolbarOverlay : UnityEditor.Overlays.ToolbarOverlay
    {
        private static bool _isDisplayed = false;
        PWBPropPlacementToolbarOverlay() : base(PinToggle.ID, BrushToggle.ID, GravityToggle.ID, LineToggle.ID,
            ShapeToggle.ID, TilingToggle.ID, ReplacerToggle.ID, EraserToggle.ID)
        {
            this.displayedChanged += OndisplayedChanged;
#if UNITY_2022_2_OR_NEWER
            collapsedIcon = Resources.Load<Texture2D>(ToggleManager.iconPath + "Brush");
#endif
        }

        private void OndisplayedChanged(bool value)
        {
            _isDisplayed = value;
            ToolbarOverlayManager.OnToolbarDisplayedChanged();
        }

        public static bool IsDisplayed => _isDisplayed;
    }
    #endregion
    #region SELECTION TOOLS
    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class SelectionToggle : ToolToggleBase<SelectionToggle>
    {
        public const string ID = "PWB/SelectionToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.SELECTION;
        public SelectionToggle() : base()
        {
            _iconName = "Selection";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Selection",
                PWBSettings.shortcuts.toolbarSelectionToggle.combination.ToString());
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class CircleSelectToggle : ToolToggleBase<CircleSelectToggle>
    {
        public const string ID = "PWB/CircleSelectToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.CIRCLE_SELECT;
        public CircleSelectToggle() : base()
        {
            _iconName = "CircleSelect";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Circle Select",
                PWBSettings.shortcuts.toolbarCircleSelectToggle.combination.ToString());
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class ExtrudeToggle : ToolToggleBase<ExtrudeToggle>
    {
        public const string ID = "PWB/ExtrudeToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.EXTRUDE;
        public ExtrudeToggle() : base()
        {
            _iconName = "Extrude";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Extrude", PWBSettings.shortcuts.toolbarExtrudeToggle.combination.ToString());
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class MirrorToggle : ToolToggleBase<MirrorToggle>
    {
        public const string ID = "PWB/MirrorToggle";
        public override string id => ID;
        public override ToolManager.PaintTool tool => ToolManager.PaintTool.MIRROR;
        public MirrorToggle() : base()
        {
            _iconName = "Mirror";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = ToggleManager.GetTooltip("Mirror", PWBSettings.shortcuts.toolbarMirrorToggle.combination.ToString());
        }
    }

    [UnityEditor.Overlays.Overlay(typeof(UnityEditor.SceneView), "PWB/Selection", true)]
    public class PWBSelectionToolbarOverlay : UnityEditor.Overlays.ToolbarOverlay
    {
        private static bool _isDisplayed = false;
        PWBSelectionToolbarOverlay() : base(SelectionToggle.ID, CircleSelectToggle.ID, ExtrudeToggle.ID, MirrorToggle.ID)
        {
            this.displayedChanged += OndisplayedChanged;
#if UNITY_2022_2_OR_NEWER
            collapsedIcon = Resources.Load<Texture2D>(ToggleManager.iconPath + "Selection");
#endif
        }

        private void OndisplayedChanged(bool value)
        {
            _isDisplayed = value;
            ToolbarOverlayManager.OnToolbarDisplayedChanged();
        }

        public static bool IsDisplayed => _isDisplayed;
    }
    #endregion
    #region GRID TOOLS
   

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class GridTypeToggle : UnityEditor.Toolbars.EditorToolbarButton
    {
        public const string ID = "PWB/GridTypeToggle";
        private Texture2D _radialGridIcon = null;
        private Texture2D _rectGridIcon = null;
        private string _radialIconName = string.Empty;
        private string _rectIconName = string.Empty;
        public GridTypeToggle() : base()
        {
            UpdateIcon();
            clicked += OnClick;
            SnapManager.settings.OnDataChanged += UpdateIcon;
        }

        public void UpdateIcon()
        {

            if (_radialGridIcon == null)
            {
                _radialIconName = "RadialGrid";
                _radialGridIcon = Resources.Load<Texture2D>(ToggleManager.iconPath + _radialIconName);
            }
            if (_rectGridIcon == null)
            {
                _rectIconName = "Grid";
                _rectGridIcon = Resources.Load<Texture2D>(ToggleManager.iconPath + _rectIconName);
            }
            if (_radialGridIcon == null || _rectIconName == null) DoLoadIcons();
            icon = SnapManager.settings.radialGridEnabled ? _rectGridIcon : _radialGridIcon;
            tooltip = SnapManager.settings.radialGridEnabled ? "Grid" : "Radial Grid";

        }

        private void OnClick()
        {
            SnapManager.settings.radialGridEnabled = !SnapManager.settings.radialGridEnabled;
            UpdateIcon();
            SnapSettingsWindow.RepaintWindow();
        }

        protected async void DoLoadIcons()
        {
            await System.Threading.Tasks.Task.Delay(1000);
            _radialGridIcon = Resources.Load<Texture2D>(ToggleManager.iconPath + _radialIconName);
            _rectGridIcon = Resources.Load<Texture2D>(ToggleManager.iconPath + _rectIconName);
            if (_radialGridIcon == null || _rectIconName == null) DoLoadIcons();
        }
    }

    public abstract class GridBarToggle : EditorToolbarDropdownToggle
    {
        protected string _iconName = string.Empty;
        public GridBarToggle()
        {
            SnapManager.settings.OnDataChanged += UpdateValue;
            UnityEditor.SceneView.duringSceneGui += UpdateValue;
        }
        protected abstract void UpdateValue();
        private void UpdateValue(UnityEditor.SceneView sceneView) => UpdateValue();

        protected async void DoLoadIcon()
        {
            await System.Threading.Tasks.Task.Delay(1000);
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
        }
    }



    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class SnapToggle : GridBarToggle, UnityEditor.Toolbars.IAccessContainerWindow
    {
        public const string ID = "PWB/SnapToggle";
        public UnityEditor.EditorWindow containerWindow { get; set; }

        public SnapToggle() : base() 
        {
            _iconName = "SnapOn";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = "Enable snapping";
            dropdownClicked += ShowSnapWindow;
            this.RegisterValueChangedCallback(OnValueChange);
        }
        protected override void UpdateValue() => value = SnapManager.settings.snappingEnabled;
        private void OnValueChange(UnityEngine.UIElements.ChangeEvent<bool> evt)
        {
            SnapManager.settings.snappingEnabled = evt.newValue;
            SnapSettingsWindow.RepaintWindow();
        }

        private void ShowSnapWindow()
        {
            var settings = SnapManager.settings;
            var menu = new UnityEditor.GenericMenu();
            if (settings.radialGridEnabled)
            {
                menu.AddItem(new GUIContent("Snap To Radius"), settings.snapToRadius,
                    () => settings.snapToRadius = !settings.snapToRadius);
                menu.AddItem(new GUIContent("Snap To Circunference"), settings.snapToCircunference,
                    () => settings.snapToCircunference = !settings.snapToCircunference);
            }
            else
            {
                menu.AddItem(new GUIContent("X"), settings.snappingOnX, () => settings.snappingOnX = !settings.snappingOnX);
                menu.AddItem(new GUIContent("Y"), settings.snappingOnY, () => settings.snappingOnY = !settings.snappingOnY);
                menu.AddItem(new GUIContent("Z"), settings.snappingOnZ, () => settings.snappingOnZ = !settings.snappingOnZ);
            }
            menu.ShowAsContext();
            SnapSettingsWindow.RepaintWindow();
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class GridToggle : GridBarToggle, UnityEditor.Toolbars.IAccessContainerWindow
    {
        public const string ID = "PWB/GridToggle";
        public UnityEditor.EditorWindow containerWindow { get; set; }

        private void UpdateIcon()
        {
            var settings = SnapManager.settings;
            _iconName = "ShowGrid" + (settings.gridOnY ? "Y" : (settings.gridOnX ? "X" : "Z"));
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
        }
        public GridToggle() : base()
        {
            UpdateIcon();
            tooltip = "Show grid";
            dropdownClicked += ShowGridWindow;
            this.RegisterValueChangedCallback(OnValueChange);
            var settings = SnapManager.settings;
            settings.OnGridOrientationChange += UpdateIcon;
        }

        protected override void UpdateValue() => value = SnapManager.settings.visibleGrid;

        private void OnValueChange(UnityEngine.UIElements.ChangeEvent<bool> evt)
            => SnapManager.settings.visibleGrid = evt.newValue;

        private void ShowGridWindow()
        {
            var settings = SnapManager.settings;
            var menu = new UnityEditor.GenericMenu();
            menu.AddItem(new GUIContent("X"), settings.gridOnX,
                () =>
                {
                    if (settings.gridOnX) return;
                    settings.gridOnX = true;
                    PWBIO.SetAxis(AxesUtils.Axis.X);
                    UpdateIcon();
                });
            menu.AddItem(new GUIContent("Y"), settings.gridOnY,
                () =>
                {
                    if (settings.gridOnY) return;
                    settings.gridOnY = true;
                    PWBIO.SetAxis(AxesUtils.Axis.Y);
                    UpdateIcon();
                });
            menu.AddItem(new GUIContent("Z"), settings.gridOnZ,
                () =>
                {
                    if (settings.gridOnZ) return;
                    settings.gridOnZ = true;
                    PWBIO.SetAxis(AxesUtils.Axis.Z);
                    UpdateIcon();
                });
            menu.ShowAsContext();
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class LockGridToggle : PWBToolbarToggle
    {
        public const string ID = "PWB/LockGridToggle";
        public LockGridToggle()
        {
            UpdteIcon();
            this.RegisterValueChangedCallback(OnValueChange);
            SnapManager.settings.OnDataChanged += UpdateValue;
            UnityEditor.SceneView.duringSceneGui += UpdateValue;
        }
        protected void UpdateValue() => value = SnapManager.settings.lockedGrid;
        private void UpdateValue(UnityEditor.SceneView sceneView) => UpdateValue();

        private void UpdteIcon()
        {
            _iconName = (SnapManager.settings.lockedGrid ? "LockGrid" : "UnlockGrid");
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = SnapManager.settings.lockedGrid ? "Lock the grid origin in place" : "Unlock the grid origin";
        }

        private void OnValueChange(UnityEngine.UIElements.ChangeEvent<bool> evt)
        {
            SnapManager.settings.lockedGrid = evt.newValue;
            UpdteIcon();
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class BoundsSnappingToggle : PWBToolbarToggle
    {
        public const string ID = "PWB/BoundsSnappingToggle";
        public BoundsSnappingToggle()
        {
            UpdteIcon();
            this.RegisterValueChangedCallback(OnValueChange);
            SnapManager.settings.OnDataChanged += UpdateValue;
            UnityEditor.SceneView.duringSceneGui += UpdateValue;
        }
        protected void UpdateValue() => value = SnapManager.settings.boundsSnapping;
        private void UpdateValue(UnityEditor.SceneView sceneView) => UpdateValue();

        private void UpdteIcon()
        {
            _iconName = "BoundsSnapping";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath +  _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = "Bounds Snapping";
        }

        private void OnValueChange(UnityEngine.UIElements.ChangeEvent<bool> evt)
        {
            SnapManager.settings.boundsSnapping = evt.newValue;
        }
    }

    [UnityEditor.Overlays.Overlay(typeof(UnityEditor.SceneView), "PWB/Grid", true)]
    public class PWBGridToolbarOverlay : UnityEditor.Overlays.ToolbarOverlay
    {
        private static bool _isDisplayed = false;
        PWBGridToolbarOverlay() : base(GridTypeToggle.ID, SnapToggle.ID,
            GridToggle.ID, LockGridToggle.ID, BoundsSnappingToggle.ID)
        {
            this.displayedChanged += OndisplayedChanged;
#if UNITY_2022_2_OR_NEWER
            collapsedIcon = Resources.Load<Texture2D>(ToggleManager.iconPath + "Grid");
#endif
        }

        private void OndisplayedChanged(bool value)
        {
            _isDisplayed = value;
            ToolbarOverlayManager.OnToolbarDisplayedChanged();
        }

        public static bool IsDisplayed => _isDisplayed;
    }
    #endregion
    #region SETTINGS & DOCS

    public class PWBToolbarButton : UnityEditor.Toolbars.EditorToolbarButton
    {
        protected string _iconName = string.Empty;
        protected async void DoLoadIcon()
        {
            await System.Threading.Tasks.Task.Delay(1000);
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class PropertiesButton :PWBToolbarButton
    {
        public const string ID = "PWB/PropertiesButton";
        public PropertiesButton()
        {
            _iconName = "ToolProperties";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = "Tool Properties";
            clicked += ToolProperties.ShowWindow;
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class BrushPropertiesButton : PWBToolbarButton
    {
        public const string ID = "PWB/BrushPropertiesButton";
        public BrushPropertiesButton()
        {
            _iconName = "BrushProperties";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = "Brush Properties";
            clicked += BrushProperties.ShowWindow;
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class HelpButton : PWBToolbarButton
    {
        public const string ID = "PWB/HelpButton";
        public HelpButton()
        {
            _iconName = "Help";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = "Documentation";
            clicked += PWBCore.OpenDocFile;
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class GridSettingsButton : PWBToolbarButton
    {
        public const string ID = "PWB/GridSettingsButton";
        public GridSettingsButton()
        {
            _iconName = "SnapSettings";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = "Grid & Snapping Settings";
            clicked += SnapSettingsWindow.ShowWindow;
        }
    }

    [UnityEditor.Toolbars.EditorToolbarElement(ID, typeof(UnityEditor.SceneView))]
    public class PreferencesButton : PWBToolbarButton
    {
        public const string ID = "PWB/PreferencesButton";
        public PreferencesButton()
        {
            _iconName = "Preferences";
            icon = Resources.Load<Texture2D>(ToggleManager.iconPath + _iconName);
            if (icon == null) DoLoadIcon();
            tooltip = "PWB Preferences";
            clicked += PWBPreferences.ShowWindow;
        }
    }

    [UnityEditor.Overlays.Overlay(typeof(UnityEditor.SceneView), "PWB/Settings", true)]
    public class SettingsAndDocsToolbarOverlay : UnityEditor.Overlays.ToolbarOverlay
    {
        private static bool _isDisplayed = false;
        SettingsAndDocsToolbarOverlay()
            : base(PropertiesButton.ID, BrushPropertiesButton.ID, GridSettingsButton.ID, PreferencesButton.ID, HelpButton.ID)
        {
            displayedChanged += OndisplayedChanged;
#if UNITY_2022_2_OR_NEWER
            collapsedIcon = Resources.Load<Texture2D>(ToggleManager.iconPath + "Preferences");
#endif
        }

        private void OndisplayedChanged(bool value)
        {
            _isDisplayed = value;
            ToolbarOverlayManager.OnToolbarDisplayedChanged();
        }

        public static bool IsDisplayed => _isDisplayed;
    }
    #endregion
}
#endif