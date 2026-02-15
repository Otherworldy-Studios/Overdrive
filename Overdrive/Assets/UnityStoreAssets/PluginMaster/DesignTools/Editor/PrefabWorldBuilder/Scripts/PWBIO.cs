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
using System.Linq;
using UnityEngine;

namespace PluginMaster
{
    [UnityEditor.InitializeOnLoad]
    public static partial class PWBIO
    {
        #region PWB WINDOWS
        public static void CloseAllWindows(bool closeToolbar = true)
        {
            BrushProperties.CloseWindow();
            ToolProperties.CloseWindow();
            PrefabPalette.CloseWindow();
            if (closeToolbar) PWBToolbar.CloseWindow();
        }
        #endregion

        #region SELECTION
        public static void UpdateSelection()
        {
            if (SelectionManager.topLevelSelection.Length == 0)
            {
                if (tool == ToolManager.PaintTool.EXTRUDE)
                {
                    _initialExtrudePosition = _extrudeHandlePosition = _selectionSize = Vector3.zero;
                    _extrudeDirection = Vector3Int.zero;
                }
                return;
            }
            if (tool == ToolManager.PaintTool.EXTRUDE)
            {
                var selectionBounds = ExtrudeManager.settings.space == Space.World
                    ? BoundsUtils.GetSelectionBounds(SelectionManager.topLevelSelection)
                    : BoundsUtils.GetSelectionBounds(SelectionManager.topLevelSelection,
                    ExtrudeManager.settings.rotationAccordingTo == ExtrudeSettings.RotationAccordingTo.FRIST_SELECTED
                    ? SelectionManager.topLevelSelection.First().transform.rotation
                    : SelectionManager.topLevelSelection.Last().transform.rotation);
                _initialExtrudePosition = _extrudeHandlePosition = selectionBounds.center;
                _selectionSize = selectionBounds.size;
                _extrudeDirection = Vector3Int.zero;
            }
            else if (tool == ToolManager.PaintTool.SELECTION)
            {
                _selectedBoxPointIdx = 10;
                _selectionRotation = Quaternion.identity;
                _selectionChanged = true;
                _editingSelectionHandlePosition = false;
                var rotation = GetSelectionRotation();
                _selectionBounds = BoundsUtils.GetSelectionBounds(SelectionManager.topLevelSelection, rotation);
                _selectionRotation = rotation;
            }
        }
        #endregion

        #region UNSAVED CHANGES
        private const string UNSAVED_CHANGES_TITLE = "Unsaved Changes";
        private const string UNSAVED_CHANGES_MESSAGE = "There are unsaved changes.\nWhat would you like to do?";
        private const string UNSAVED_CHANGES_OK = "Save";
        private const string UNSAVED_CHANGES_CANCEL = "Don't Save";

        private static void DisplaySaveDialog(System.Action Save)
        {
            if (UnityEditor.EditorUtility.DisplayDialog(UNSAVED_CHANGES_TITLE,
                UNSAVED_CHANGES_MESSAGE, UNSAVED_CHANGES_OK, UNSAVED_CHANGES_CANCEL)) Save();
            else repaint = true;
        }
        private static void AskIfWantToSave(ToolManager.ToolState state, System.Action Save)
        {
            switch (PWBCore.staticData.unsavedChangesAction)
            {
                case PWBData.UnsavedChangesAction.ASK:
                    if (state == ToolManager.ToolState.EDIT) DisplaySaveDialog(Save);
                    break;
                case PWBData.UnsavedChangesAction.SAVE:
                    if (state == ToolManager.ToolState.EDIT) Save();
                    BrushstrokeManager.ClearBrushstroke();
                    break;
                case PWBData.UnsavedChangesAction.DISCARD:
                    repaint = true;
                    return;
            }
        }

        #endregion

        #region COMMON
        private const float TAU = Mathf.PI * 2;
        private static int _controlId;
        public static int controlId { set => _controlId = value; }
        private static ToolManager.PaintTool tool => ToolManager.tool;

        private static UnityEditor.Tool _unityCurrentTool = UnityEditor.Tool.None;

        private static Camera _sceneViewCamera = null;

        public static bool repaint { get; set; }

        static PWBIO()
        {
            LineData.SetNextId();
            SelectionManager.selectionChanged += UpdateSelection;
            UnityEditor.Undo.undoRedoPerformed += OnUndoPerformed;
            UnityEditor.SceneView.duringSceneGui += DuringSceneGUI;
            PaletteManager.OnPaletteChanged += OnPaletteChanged;
            PaletteManager.OnBrushSelectionChanged += OnBrushSelectionChanged;
            ToolManager.OnToolModeChanged += OnEditModeChanged;
#if UNITY_2021_1_OR_NEWER
            UnityEditor.SceneManagement.PrefabStage.prefabStageOpened += OnPrefabStageChanged;
            UnityEditor.SceneManagement.PrefabStage.prefabStageClosing += OnPrefabStageChanged;
#endif
            UnityEditor.EditorApplication.delayCall += () =>
            {
                LineInitializeOnLoad();
                ShapeInitializeOnLoad();
                TilingInitializeOnLoad();
                FloorInitializeOnLoad();
                WallInitializeOnLoad();
            };
        }

        private static void OnPaletteChanged()
        {
            ApplySelectionFilters();
            switch (ToolManager.tool)
            {
                case ToolManager.PaintTool.ERASER:
                    if (EraserManager.settings.command == ModifierToolSettings.Command.SELECT_PALETTE_PREFABS)
                        UpdateOctree();
                    break;
                case ToolManager.PaintTool.REPLACER:
                    if (ReplacerManager.settings.command == ModifierToolSettings.Command.SELECT_PALETTE_PREFABS)
                        UpdateOctree();
                    BrushstrokeManager.ClearReplacerDictionary();
                    break;
                case ToolManager.PaintTool.CIRCLE_SELECT:
                    if (CircleSelectManager.settings.command == ModifierToolSettings.Command.SELECT_PALETTE_PREFABS)
                        UpdateOctree();
                    break;
            }
        }

        private static void OnBrushSelectionChanged()
        {
            switch (ToolManager.tool)
            {
                case ToolManager.PaintTool.GRAVITY:
                    InitializeGravityTool();
                    break;
                case ToolManager.PaintTool.LINE:
                    ClearLineStroke();
                    break;
                case ToolManager.PaintTool.SHAPE:
                    ClearShapeStroke();
                    break;
                case ToolManager.PaintTool.TILING:
                    ClearTilingStroke();
                    break;
                case ToolManager.PaintTool.SELECTION:
                    InitializeSelectionToolOnBrushChanged();
                    break;
                case ToolManager.PaintTool.ERASER:
                    if (EraserManager.settings.command == ModifierToolSettings.Command.SELECT_BRUSH_PREFABS)
                        UpdateOctree();
                    break;
                case ToolManager.PaintTool.REPLACER:
                    if (ReplacerManager.settings.command == ModifierToolSettings.Command.SELECT_BRUSH_PREFABS)
                        UpdateOctree();
                    BrushstrokeManager.ClearReplacerDictionary();
                    break;
                case ToolManager.PaintTool.CIRCLE_SELECT:
                    if (CircleSelectManager.settings.command == ModifierToolSettings.Command.SELECT_BRUSH_PREFABS)
                        UpdateOctree();
                    break;
                case ToolManager.PaintTool.FLOOR:
                    UpdateFloorSettingsOnBrushChanged();
                    break;
                case ToolManager.PaintTool.WALL:
                    UpdateWallSettingsOnBrushChanged();
                    break;
            }
        }

        private static bool _mousePressed;
        public static bool mousePressed => _mousePressed;
        public static void HandleMouseEvents()
        {
            if (Event.current.type == EventType.MouseDown) _mousePressed = true;
            else if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseLeaveWindow)
                _mousePressed = false;
        }

        public static void SaveUnityCurrentTool() => _unityCurrentTool = UnityEditor.Tools.current;
        public static bool _wasPickingBrushes = false;

        public static void DuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            _sceneViewCamera = sceneView.camera;
            HandleMouseEvents();
            if (updateStroke) UnityEditor.SceneView.RepaintAll();
            if (sceneView.in2DMode)
            {
                SnapManager.settings.gridOnZ = true;
                PWBToolbar.RepaintWindow();
            }
            if (repaint)
            {
                if (tool == ToolManager.PaintTool.SHAPE) BrushstrokeManager.UpdateShapeBrushstroke();
                sceneView.Repaint();
                repaint = false;
            }
            GizmosInput();
            if (_offsetPicking)
            {
                OffsetPicking(sceneView.camera);
                var labelTexts = new string[] { $"Offset: {_offsetPickingValue.ToString("F5")}" };
                InfoText.Draw(sceneView, labelTexts.ToArray());
                if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
                {
                    _offsetPickingBrush.SetLocalPositionOffset(_offsetPickingValue, _offsetPickingAxis);
                    BrushProperties.RepaintWindow();
                    _offsetPicking = false;
                }
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                    _offsetPicking = false;
                sceneView.Repaint();
            }
            PaletteInput(sceneView);

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape
                && (tool == ToolManager.PaintTool.PIN || tool == ToolManager.PaintTool.BRUSH
                || tool == ToolManager.PaintTool.GRAVITY || tool == ToolManager.PaintTool.ERASER
                || tool == ToolManager.PaintTool.REPLACER || tool == ToolManager.PaintTool.CIRCLE_SELECT
                || tool == ToolManager.PaintTool.FLOOR || tool == ToolManager.PaintTool.WALL))
                ToolManager.DeselectTool();
            var repaintScene = _wasPickingBrushes == PaletteManager.pickingBrushes;
            _wasPickingBrushes = PaletteManager.pickingBrushes;
            if (PaletteManager.pickingBrushes)
            {
                UnityEditor.HandleUtility.AddDefaultControl(_controlId);
                if (repaintScene) UnityEditor.SceneView.RepaintAll();
                if (Event.current.button == 0 && Event.current.type == EventType.MouseDown) Event.current.Use();
                return;
            }
            if (ToolManager.tool != ToolManager.PaintTool.NONE)
            {
                if (PWBSettings.shortcuts.editModeToggle.Check())
                {
                    switch (tool)
                    {
                        case ToolManager.PaintTool.LINE:
                        case ToolManager.PaintTool.SHAPE:
                        case ToolManager.PaintTool.TILING:
                            ToolManager.editMode = !ToolManager.editMode;
                            _persistentItemWasEdited = false;
                            ToolProperties.RepainWindow();
                            break;
                        default: break;
                    }
                }
                if (PaletteManager.selectedBrushIdx == -1 && (tool == ToolManager.PaintTool.PIN
                    || tool == ToolManager.PaintTool.BRUSH || tool == ToolManager.PaintTool.GRAVITY
                    || ((tool == ToolManager.PaintTool.LINE || tool == ToolManager.PaintTool.SHAPE
                    || tool == ToolManager.PaintTool.TILING)
                    && !ToolManager.editMode)))
                {
                    if (tool == ToolManager.PaintTool.LINE && _lineData != null && _lineData.state != ToolManager.ToolState.NONE)
                        ResetLineState();
                    else if (tool == ToolManager.PaintTool.SHAPE
                        && _shapeData != null && _shapeData.state != ToolManager.ToolState.NONE)
                        ResetShapeState();
                    else if (tool == ToolManager.PaintTool.TILING
                        && _tilingData != null && _tilingData.state != ToolManager.ToolState.NONE)
                        ResetTilingState();
                }

                if (Event.current.type == EventType.MouseEnterWindow) _pinned = false;

                if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
                {
                    sceneView.Focus();
                }
                else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.V)
                    _snapToVertex = true;
                else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.V)
                    _snapToVertex = false;
                if (tool == ToolManager.PaintTool.BRUSH || tool == ToolManager.PaintTool.GRAVITY
                    || tool == ToolManager.PaintTool.ERASER || tool == ToolManager.PaintTool.REPLACER
                    || tool == ToolManager.PaintTool.CIRCLE_SELECT)
                {
                    var settings = ToolManager.GetSettingsFromTool(tool);
                    BrushRadiusShortcuts(settings as CircleToolBase);
                }

                switch (tool)
                {
                    case ToolManager.PaintTool.PIN:
                        PinDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.BRUSH:
                        BrushDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.GRAVITY:
                        GravityToolDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.LINE:
                        LineDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.SHAPE:
                        ShapeDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.TILING:
                        TilingDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.ERASER:
                        EraserDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.REPLACER:
                        ReplacerDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.SELECTION:
                        SelectionDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.CIRCLE_SELECT:
                        CircleSelectDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.EXTRUDE:
                        ExtrudeDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.MIRROR:
                        MirrorDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.FLOOR:
                        FloorToolDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.WALL:
                        WallToolDuringSceneGUI(sceneView);
                        break;
                }

                if ((tool != ToolManager.PaintTool.EXTRUDE && tool != ToolManager.PaintTool.SELECTION
                    && tool != ToolManager.PaintTool.MIRROR) && Event.current.type == EventType.Layout
                    && !ToolManager.editMode)
                {
                    UnityEditor.Tools.current = UnityEditor.Tool.None;
                    UnityEditor.HandleUtility.AddDefaultControl(_controlId);
                }
            }
            GridDuringSceneGui(sceneView);
            sceneView.autoRepaintOnSceneChange = true;
        }

        private static Vector3 TangentSpaceToWorld(Vector3 tangent, Vector3 bitangent, Vector2 tangentSpacePos)
            => (tangent * tangentSpacePos.x + bitangent * tangentSpacePos.y);

        private static void UpdateStrokeDirection(Vector3 hitPoint)
        {
            var dir = hitPoint - _prevMousePos;
            if (dir.sqrMagnitude > 0.3f)
            {
                _strokeDirection = hitPoint - _prevMousePos;
                _prevMousePos = hitPoint;
            }
        }

        public static void ResetUnityCurrentTool()
        {
            if (_unityCurrentTool != UnityEditor.Tool.None)
                UnityEditor.Tools.current = _unityCurrentTool;
        }

        private static bool MouseDot(out Vector3 point, out Vector3 normal,
            PaintOnSurfaceToolSettingsBase.PaintMode mode, bool in2DMode,
            bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider, bool snapOnGrid, bool ignoreSceneColliders)
        {
            point = Vector3.zero;
            normal = Vector3.up;
            var mousePos = Event.current.mousePosition;
            if (mousePos.x < 0 || mousePos.x >= Screen.width || mousePos.y < 0 || mousePos.y >= Screen.height) return false;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePos);
            Vector3 SnapPoint(Vector3 hitPoint, ref Vector3 snapNormal)
            {
                if (_snapToVertex)
                {
                    if (SnapToVertex(mouseRay, out RaycastHit snappedHit, in2DMode))
                    {
                        _snappedToVertex = true;
                        hitPoint = snappedHit.point;
                        snapNormal = snappedHit.normal;
                    }
                }
                return hitPoint;
            }

            RaycastHit surfaceHit;
            bool surfaceFound = PWBToolRaycast(mouseRay, out surfaceHit, out GameObject collider,
                float.MaxValue, -1, paintOnPalettePrefabs, castOnMeshesWithoutCollider,
                ignoreSceneColliders: ignoreSceneColliders);
            if (mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE && surfaceFound)
            {
                normal = surfaceHit.normal;
                point = SnapPoint(surfaceHit.point, ref normal);
                return true;
            }
            if (mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE)
            {
                if (surfaceFound)
                {
                    point = SnapPoint(surfaceHit.point, ref normal);
                    var direction = SnapManager.settings.rotation * Vector3.down;
                    var ray = new Ray(point - direction, direction);
                    if (PWBToolRaycast(ray, out RaycastHit hitInfo, out collider, float.MaxValue, -1,
                        paintOnPalettePrefabs, castOnMeshesWithoutCollider, ignoreSceneColliders: ignoreSceneColliders))
                        point = hitInfo.point;
                    UpdateGridOrigin(point);
                    return true;
                }
                if (GridRaycast(mouseRay, out RaycastHit gridHit))
                {
                    point = SnapPoint(gridHit.point, ref normal);
                    return true;
                }
            }
            return false;
        }

        private static bool _updateStroke = false;
        public static bool updateStroke { get => _updateStroke; set => _updateStroke = value; }
        public static void UpdateStroke() => updateStroke = true;

        public static void UpdateSelectedPersistentObject()
        {
            BrushstrokeManager.UpdateBrushstroke(false);
            switch (tool)
            {
                case ToolManager.PaintTool.LINE:
                    if (_selectedPersistentLineData != null) _editingPersistentLine = true;
                    break;
                case ToolManager.PaintTool.SHAPE:
                    if (_selectedPersistentShapeData != null) _editingPersistentShape = true;
                    break;
                case ToolManager.PaintTool.TILING:
                    if (_selectedPersistentTilingData != null) _editingPersistentTiling = true;
                    break;
            }
            repaint = true;
        }
        public static int selectedPointIdx
        {
            get
            {
                switch (ToolManager.tool)
                {
                    case ToolManager.PaintTool.TILING:
                        if (ToolManager.editMode)
                        {
                            if (_selectedPersistentTilingData == null) return -1;
                            return _selectedPersistentTilingData.selectedPointIdx;
                        }
                        else if (_tilingData.state == ToolManager.ToolState.EDIT) return _tilingData.selectedPointIdx;
                        break;
                    case ToolManager.PaintTool.LINE:
                        if (ToolManager.editMode)
                        {
                            if (_selectedPersistentLineData == null) return -1;
                            return _selectedPersistentLineData.selectedPointIdx;
                        }
                        else if (_lineData.state == ToolManager.ToolState.EDIT) return _lineData.selectedPointIdx;
                        break;
                    case ToolManager.PaintTool.SHAPE:
                        if (ToolManager.editMode)
                        {
                            if (_selectedPersistentShapeData == null) return -1;
                            return _selectedPersistentShapeData.selectedPointIdx;
                        }
                        else if (_shapeData.state == ToolManager.ToolState.EDIT) return _shapeData.selectedPointIdx;
                        break;
                }
                return -1;
            }
        }

        private static bool _updateHandlePosition = false;
        private static Vector3 _handlePosition;
        public static void UpdateHandlePosition()
        {
            _updateHandlePosition = true;
            if (tool == ToolManager.PaintTool.TILING && tilingData != null) ApplyTilingHandlePosition(tilingData);
            BrushstrokeManager.UpdateBrushstroke(false);

        }
        public static Vector3 handlePosition { get => _handlePosition; set => _handlePosition = value; }

        private static bool _updateHandleRotation = false;
        private static Quaternion _handleRotation;
        public static void UpdateHandleRotation()
        {
            _updateHandleRotation = true;
            BrushstrokeManager.UpdateBrushstroke(false);
        }
        public static Quaternion handleRotation { get => _handleRotation; set => _handleRotation = value; }

        private static void DrawCircleTool(Vector3 center, Camera camera, Color color, float radius)
        {

            const float polygonSideSize = 0.3f;
            const int minPolygonSides = 8;
            const int maxPolygonSides = 60;
            var polygonSides = Mathf.Clamp((int)(TAU * radius / polygonSideSize),
                minPolygonSides, maxPolygonSides);

            var periPoints = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < polygonSides; ++i)
            {
                var radians = TAU * i / (polygonSides - 1f);
                var tangentDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                var worldDir = TangentSpaceToWorld(camera.transform.right, camera.transform.up, tangentDir);
                periPoints.Add(center + (worldDir * radius));
            }
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 1f);
            UnityEditor.Handles.DrawAAPolyLine(5, periPoints.ToArray());
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawAAPolyLine(5, periPoints.ToArray());
        }

        private static void GetCircleToolTargets(Ray mouseRay, Camera camera, ISelectionBrushTool selectionBrushTool,
            float radius, System.Collections.Generic.HashSet<GameObject> targets)
        {
#if UNITY_2021_1_OR_NEWER
            using (UnityEngine.Pool.ListPool<GameObject>
                .Get(out System.Collections.Generic.List<GameObject> nearbyObjectsList))
#else
            var nearbyObjectsList = new System.Collections.Generic.List<GameObject>();
#endif
            {
                boundsOctree.GetColliding(nearbyObjectsList, mouseRay, radius, maxDistance: float.PositiveInfinity);
                targets.Clear();
                if (selectionBrushTool.outermostPrefabFilter)
                {
                    foreach (var nearby in nearbyObjectsList)
                    {
                        if (nearby == null) continue;
                        var outermost = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(nearby);
                        if (outermost == null) targets.Add(nearby);
                        else if (!targets.Contains(outermost)) targets.Add(outermost);
                    }
                }
                else targets.UnionWith(nearbyObjectsList);
            }
#if UNITY_2021_1_OR_NEWER
             using (UnityEngine.Pool.ListPool<GameObject>
                .Get(out System.Collections.Generic.List<GameObject> toSelectList))
#else
            var toSelectList = new System.Collections.Generic.List<GameObject>();
#endif
            {
                toSelectList.AddRange(targets);
                targets.Clear();

                var closestDistSqr = float.MaxValue;
                int numToSelectListCount = toSelectList.Count;
                for (int i = 0; i < numToSelectListCount; ++i)
                {
                    var obj = toSelectList[i];
                    if (obj == null) continue;
                    var magnitude = BoundsUtils.GetAverageMagnitude(obj.transform);
                    if (radius < magnitude / 2) continue;

                    if (selectionBrushTool.onlyTheClosest)
                    {
                        var pos = obj.transform.position;
                        var distSqr = (pos - camera.transform.position).sqrMagnitude;
                        if (distSqr < closestDistSqr)
                        {
                            closestDistSqr = distSqr;
                            targets.Clear();
                            targets.Add(obj);
                        }
                        continue;
                    }
                    targets.Add(obj);
                }
            }
        }

        private static Quaternion GetRotationFromNormal(Vector3 normal)
        {
            bool GetYOnPlane(out float y)
            {
                y = 0;
                if (Mathf.Approximately(normal.y, 0f)) return false;
                y = -normal.x / normal.y;
                return true;
            }
            bool GetZOnPlane(out float z)
            {
                z = 0f;
                if (Mathf.Approximately(normal.z, 0f)) return false;
                z = -normal.x / normal.z;
                return true;
            }
            bool GetXOnPlane(out float x)
            {
                x = 0f;
                if (Mathf.Approximately(normal.x, 0f)) return false;
                x = -normal.z / normal.x;
                return true;
            }
            var right = Vector3.right;
            if (GetYOnPlane(out float y)) right = new Vector3(1, y, 0);
            else if (GetZOnPlane(out float z)) right = new Vector3(1, 0, z);
            else if (GetXOnPlane(out float x)) right = new Vector3(x, 0, 1);
            var forward = Vector3.Cross(right, normal);
            return Quaternion.LookRotation(forward, normal);
        }

        private static Quaternion GetRotationFromNormal(Vector3 normal, Quaternion currentRotation)
        {
            var rotation = GetRotationFromNormal(normal);
            var currentYRotaion = Quaternion.Euler(0, currentRotation.eulerAngles.y, 0);
            return rotation * currentYRotaion;
        }
        #endregion

        #region PERSISTENT OBJECTS
        public static void OnUndoPerformed()
        {
            _boundsOctree = null;
            if (tool == ToolManager.PaintTool.LINE && UnityEditor.Undo.GetCurrentGroupName() == LineData.COMMAND_NAME)
            {
                OnUndoLine();
                UpdateStroke();
            }
            else if (tool == ToolManager.PaintTool.SHAPE && UnityEditor.Undo.GetCurrentGroupName() == ShapeData.COMMAND_NAME)
            {
                OnUndoShape();
                UpdateStroke();
            }
            else if (tool == ToolManager.PaintTool.TILING && UnityEditor.Undo.GetCurrentGroupName() == TilingData.COMMAND_NAME)
            {
                OnUndoTiling();
                UpdateStroke();
            }
            if (ToolManager.tool == ToolManager.PaintTool.LINE
                || ToolManager.tool == ToolManager.PaintTool.SHAPE
                || ToolManager.tool == ToolManager.PaintTool.TILING)
                PWBCore.staticData.SaveAndUpdateVersion();
            else
            {
                if (ToolManager.tool == ToolManager.PaintTool.REPLACER) BrushstrokeManager.ClearReplacerDictionary();
                BrushstrokeManager.UpdateBrushstroke();
            }
            UnityEditor.SceneView.RepaintAll();
        }

        public static void OnToolChange(ToolManager.PaintTool prevTool)
        {
            switch (prevTool)
            {
                case ToolManager.PaintTool.LINE:
                    ResetLineState();
                    break;
                case ToolManager.PaintTool.SHAPE:
                    ResetShapeState();
                    break;
                case ToolManager.PaintTool.TILING:
                    ResetTilingState();
                    break;
                case ToolManager.PaintTool.EXTRUDE:
                    ResetExtrudeState();
                    break;
                case ToolManager.PaintTool.MIRROR:
                    ResetMirrorState();
                    break;
                default: break;
            }
            _meshesAndRenderers.Clear();
            UnityEditor.SceneView.RepaintAll();
        }

        private static void OnEditModeChanged()
        {
            switch (tool)
            {
                case ToolManager.PaintTool.LINE:
                    OnLineToolModeChanged();
                    break;
                case ToolManager.PaintTool.SHAPE:
                    OnShapeToolModeChanged();
                    break;
                case ToolManager.PaintTool.TILING:
                    OnTilingToolModeChanged();
                    break;
                default: break;
            }
        }

        private static void DeleteDisabledObjects()
        {
            if (_disabledObjects == null) return;
            foreach (var obj in _disabledObjects)
            {
                if (obj == null) continue;
                obj.SetActive(true);
                UnityEditor.Undo.DestroyObjectImmediate(obj);
            }
        }

        private static void ResetSelectedPersistentObject<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>
            (PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA> manager,
            ref bool editingPersistentObject, TOOL_DATA initialPersistentData)
            where TOOL_NAME : IToolName, new()
            where TOOL_SETTINGS : IToolSettings, new()
            where CONTROL_POINT : ControlPoint, new()
            where TOOL_DATA : PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>, new()
            where SCENE_DATA : SceneData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA>, new()
        {
            editingPersistentObject = false;
            if (initialPersistentData == null) return;
            var selectedItem = manager.GetItem(initialPersistentData.id);
            if (selectedItem == null) return;
            selectedItem.ResetPoses(initialPersistentData);
            selectedItem.ClearSelection();
        }

        private static void DeselectPersistentItems<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>
            (PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA> manager)
            where TOOL_NAME : IToolName, new()
            where TOOL_SETTINGS : IToolSettings, new()
            where CONTROL_POINT : ControlPoint, new()
            where TOOL_DATA : PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>, new()
            where SCENE_DATA : SceneData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA>, new()
        {
            var persitentTilings = manager.GetPersistentItems();
            foreach (var i in persitentTilings) i.ClearSelection();
        }

        private static bool ApplySelectedPersistentObject<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>
            (bool deselectPoint, ref bool editingPersistentObject, ref TOOL_DATA initialPersistentData,
            ref TOOL_DATA selectedPersistentData,
            PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA> manager)
            where TOOL_NAME : IToolName, new()
            where TOOL_SETTINGS : IToolSettings, new()
            where CONTROL_POINT : ControlPoint, new()
            where TOOL_DATA : PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>, new()
            where SCENE_DATA : SceneData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA>, new()
        {
            editingPersistentObject = false;
            if (initialPersistentData == null) return false;
            var selected = manager.GetItem(initialPersistentData.id);
            if (selected == null)
            {
                initialPersistentData = null;
                selectedPersistentData = null;
                return false;
            }
            selected.UpdatePoses();
            if (_paintStroke.Count > 0)
            {
                var objDic = Paint(selected.settings as IPaintToolSettings, PAINT_CMD, true, true);
                foreach (var paintedItem in objDic)
                {
                    var persistentItem = manager.GetItem(paintedItem.Key);
                    if (persistentItem == null) continue;
                    persistentItem.AddObjects(paintedItem.Value.ToArray());
                }
            }
            if (deselectPoint)
            {
                DeselectPersistentItems(manager);
            }
            DeleteDisabledObjects();

            _persistentPreviewData.Clear();
            PWBCore.staticData.SaveAndUpdateVersion();
            if (!deselectPoint) return true;
            var persistentObjects = manager.GetPersistentItems();
            foreach (var item in persistentObjects) item.ClearSelection();
            return true;
        }

        static bool _persistentItemWasEdited = false;


        public static void DuplicateItem(long itemId)
        {
            var toolMan = ToolManager.GetCurrentPersistentToolManager();
            var clone = toolMan.Duplicate(itemId);
            ToolManager.editMode = true;
            clone.isSelected = true;
            var allItems = toolMan.GetItems();
            foreach (var item in allItems)
            {
                if (item == clone) continue;
                item.isSelected = false;
                item.ClearSelection();
            }
            var bounds = clone.GetBounds(1.1f);
            UnityEditor.SceneView.lastActiveSceneView.Frame(bounds, false);

            if (ToolManager.tool == ToolManager.PaintTool.LINE)
            {
                LineManager.editModeType = LineManager.EditModeType.LINE_POSE;
                PWBIO.SelectLine(clone as LineData);
            }
            else if (ToolManager.tool == ToolManager.PaintTool.SHAPE) PWBIO.SelectShape(clone as ShapeData);
            else if (ToolManager.tool == ToolManager.PaintTool.TILING) PWBIO.SelectTiling(clone as TilingData);
        }

        public static void PersistentItemContextMenu(UnityEditor.GenericMenu menu,
            IPersistentData data, Vector2 mousePosition)
        {
            void DeleteItem(bool deleteObjects)
            {
                var toolMan = ToolManager.GetCurrentPersistentToolManager();
                toolMan.DeletePersistentItem(data.id, deleteObjects);
                UnityEditor.SceneView.RepaintAll();
            }
            menu.AddItem(new GUIContent("Select parent object ... "
               + PWBSettings.shortcuts.editModeSelectParent.combination.ToString()), on: false, () =>
               {
                   var parent = data.GetParent();
                   if (parent != null) UnityEditor.Selection.activeGameObject = parent;
               });
            menu.AddItem(new GUIContent("Duplicate ... "
                + PWBSettings.shortcuts.editModeDuplicate.combination.ToString()), on: false, () => DuplicateItem(data.id));
            menu.AddItem(new GUIContent("Delete item and its children ... "
                + PWBSettings.shortcuts.editModeDeleteItemAndItsChildren.combination.ToString()),
                on: false, () => DeleteItem(deleteObjects: true));
            menu.AddItem(new GUIContent("Delete item but not its children ... "
                + PWBSettings.shortcuts.editModeDeleteItemButNotItsChildren.combination.ToString()), on: false,
                () => DeleteItem(deleteObjects: false));
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent(data.toolName + " properties..."), on: false,
                           () => ItemPropertiesWindow.ShowItemProperties(data, mousePosition));
        }
        #endregion

        #region OCTREE
        private const float MIN_OCTREE_NODE_SIZE = 0.5f;
        private static BoundsOctree _boundsOctree = new BoundsOctree(initialWorldSize: 10,
            initialWorldPos: Vector3.zero, MIN_OCTREE_NODE_SIZE, MIN_OCTREE_NODE_SIZE);

        private static bool _octreeIsDirty = false;

        public static void SetOctreeDirty() => _octreeIsDirty = true;

        public static BoundsOctree boundsOctree
        {
            get
            {
                if (_boundsOctree == null || _octreeIsDirty) UpdateOctree();
                return _boundsOctree;
            }
        }

        public static void UpdateOctree(GameObject[] allObjects)
        {
            if (tool == ToolManager.PaintTool.ERASER || tool == ToolManager.PaintTool.REPLACER
                || tool == ToolManager.PaintTool.CIRCLE_SELECT)
            {
                var allPrefabsPaths = new System.Collections.Generic.HashSet<string>();
                bool AddPrefabPath(MultibrushItemSettings item)
                {
                    if (item.prefab == null) return false;
                    var path = UnityEditor.AssetDatabase.GetAssetPath(item.prefab);
                    allPrefabsPaths.Add(path);
                    return true;
                }
                ISelectionBrushTool SelectionBrushSettings = EraserManager.settings;
                if (tool == ToolManager.PaintTool.REPLACER) SelectionBrushSettings = ReplacerManager.settings;
                else if (tool == ToolManager.PaintTool.CIRCLE_SELECT) SelectionBrushSettings = CircleSelectManager.settings;
                if (SelectionBrushSettings.command == SelectionBrushToolSettings.Command.SELECT_PALETTE_PREFABS)
                    foreach (var brush in PaletteManager.selectedPalette.brushes)
                        foreach (var item in brush.items) AddPrefabPath(item);
                else if (PaletteManager.selectedBrush != null
                    && SelectionBrushSettings.command == SelectionBrushToolSettings.Command.SELECT_BRUSH_PREFABS)
                    foreach (var item in PaletteManager.selectedBrush.items) AddPrefabPath(item);
                SelectionManager.UpdateSelection();
                bool modifyAll = SelectionBrushSettings.command == SelectionBrushToolSettings.Command.SELECT_ALL;
                bool modifyAllButSelected = false;
                if (tool == ToolManager.PaintTool.ERASER || tool == ToolManager.PaintTool.REPLACER)
                {
                    IModifierTool modifierSettings = tool == ToolManager.PaintTool.ERASER
                        ? EraserManager.settings as IModifierTool : ReplacerManager.settings;
                    modifyAllButSelected = modifierSettings.modifyAllButSelected;
                }
                foreach (var obj in allObjects)
                {
                    if (!obj.activeInHierarchy) continue;
                    if (!modifyAll && !UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(obj)) continue;
                    var prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                    bool isBrush = allPrefabsPaths.Contains(prefabPath);
                    if (!isBrush && !modifyAll) continue;
                    if (modifyAllButSelected && SelectionManager.selection.Contains(obj)) continue;
                    AddObjectToOctree(obj);
                }
            }
            else
            {
                foreach (var obj in allObjects)
                {
                    if (!obj.activeInHierarchy) continue;
                    AddObjectToOctree(obj);
                }
            }
        }

        public static void UpdateOctree()
        {
            if (_boundsOctree != null && _boundsOctree.Count > 0 && !_octreeIsDirty) return;

            _octreeIsDirty = false;
            _boundsOctree = new BoundsOctree(initialWorldSize: 10,
           initialWorldPos: Vector3.zero, MIN_OCTREE_NODE_SIZE, MIN_OCTREE_NODE_SIZE);
            GameObject[] allObjects;
            if (isInPrefabMode)
            {
                var transforms = prefabStage.prefabContentsRoot.GetComponentsInChildren<Transform>();
                allObjects = transforms.Select(t => t.gameObject).ToArray();
                UpdateOctree(allObjects);
            }
            else
            {
#if UNITY_2022_2_OR_NEWER
                allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
                allObjects = GameObject.FindObjectsOfType<GameObject>();
#endif
                var allObjectsRoots = new System.Collections.Generic.HashSet<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj == null) continue;
                    var outermost = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                    if (outermost == null)
                    {
                        var components = obj.GetComponents<Component>();
                        if (components.Length <= 1) continue;
                        var colliders = obj.GetComponents<Collider>();
                        var renderers = obj.GetComponents<Renderer>();
                        var filters = obj.GetComponents<MeshFilter>();
                        if (colliders.Length == 0 && renderers.Length == 0 && filters.Length == 0) continue;
                        allObjectsRoots.Add(obj);
                    }
                    else allObjectsRoots.Add(outermost);
                }
                UpdateOctree(allObjectsRoots.ToArray());
            }
        }

        public static void AddObjectToOctree(GameObject obj)
        {
            if (_boundsOctree == null) _boundsOctree = new BoundsOctree(initialWorldSize: 10,
           initialWorldPos: Vector3.zero, MIN_OCTREE_NODE_SIZE, MIN_OCTREE_NODE_SIZE);
            Bounds bounds;
            if (ToolManager.tool == ToolManager.PaintTool.FLOOR)
                bounds = BoundsUtils.GetBoundsRecursive(obj.transform, SnapManager.settings.rotation);
            else bounds = BoundsUtils.GetBoundsRecursive(obj.transform);
            _boundsOctree.Add(obj, bounds);
        }
        #endregion

        #region SCENE COLLIDERS

        private static System.Collections.Generic.HashSet<int> _sceneColliders
            = new System.Collections.Generic.HashSet<int>();
        public static void UpdateSceneColliderSet()
        {
            Collider[] allColliders;
            if (isInPrefabMode)
            {
                allColliders = prefabStage.prefabContentsRoot.GetComponentsInChildren<Collider>();
            }
            else
            {
#if UNITY_2022_2_OR_NEWER
                allColliders = GameObject.FindObjectsByType<Collider>(FindObjectsSortMode.None);
#else
                allColliders = GameObject.FindObjectsOfType<Collider>();
#endif
            }
            _sceneColliders.Clear();
            foreach (var c in allColliders) _sceneColliders.Add(c.GetInstanceID());
        }
        #endregion

        #region BRUSHTROKE IO & PREVIEW
        private const string PWB_OBJ_NAME = "Prefab World Builder";
        private static Vector3 _prevMousePos = Vector3.zero;
        private static Vector3 _strokeDirection = Vector3.forward;
        private static Transform _autoParent = null;
        private static System.Collections.Generic.Dictionary<string, Transform> _subParents
            = new System.Collections.Generic.Dictionary<string, Transform>();
        private static Mesh quadMesh = null;

        private class PaintStrokeItem
        {
            public readonly GameObject prefab = null;
            public readonly string guid = string.Empty;
            public readonly Vector3 position = Vector3.zero;
            public readonly Quaternion rotation = Quaternion.identity;
            public readonly Vector3 scale = Vector3.one;
            public readonly int layer = 0;
            public readonly bool flipX = false;
            public readonly bool flipY = false;
            public readonly int index = 0;
            private Transform _parent = null;
            private string _persistentParentId = string.Empty;


            private Transform _surface = null;
            public Transform parent { get => _parent; set => _parent = value; }
            public string persistentParentId { get => _persistentParentId; set => _persistentParentId = value; }
            public Transform surface { get => _surface; set => _surface = value; }
            public PaintStrokeItem(GameObject prefab, string guid, Vector3 position, Quaternion rotation,
                Vector3 scale, int layer, Transform parent, Transform surface, bool flipX, bool flipY, int index = -1)
            {
                this.prefab = prefab;
                this.guid = guid;
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
                this.layer = layer;
                this.flipX = flipX;
                this.flipY = flipY;
                this.index = index;
                _parent = parent;
                _surface = surface;
            }
        }
        private static System.Collections.Generic.List<PaintStrokeItem> _paintStroke
            = new System.Collections.Generic.List<PaintStrokeItem>();

        private static void BrushRadiusShortcuts(CircleToolBase settings)
        {
            if (PWBSettings.shortcuts.brushRadius.Check())
            {
                var combi = PWBSettings.shortcuts.brushRadius.combination;
                var delta = Mathf.Sign(combi.delta);
                settings.radius = Mathf.Max(settings.radius * (1f + delta * 0.03f), 0.05f);
                if (settings is BrushToolSettings)
                {
                    if (BrushManager.settings.heightType == BrushToolSettings.HeightType.RADIUS)
                        BrushManager.settings.maxHeightFromCenter = BrushManager.settings.radius;
                }
                ToolProperties.RepainWindow();
            }
        }

        private static void BrushstrokeMouseEvents(BrushToolBase settings)
        {
            if (PaletteManager.selectedBrush == null) return;
            if (Event.current.button == 0 && !Event.current.alt && Event.current.type == EventType.MouseUp
                && PaletteManager.selectedBrush.patternMachine != null
                && PaletteManager.selectedBrush.restartPatternForEachStroke)
            {
                PaletteManager.selectedBrush.patternMachine.Reset();
                BrushstrokeManager.UpdateBrushstroke();
            }
            else if (PWBSettings.shortcuts.brushUpdatebrushstroke.Check())
            {
                BrushstrokeManager.UpdateBrushstroke();
                repaint = true;
            }
            else if (PWBSettings.shortcuts.brushResetRotation.Check()) _brushAngle = 0;
            else if (PWBSettings.shortcuts.brushDensity.Check()
                && settings.brushShape != BrushToolBase.BrushShape.POINT)
            {
                settings.density += (int)Mathf.Sign(PWBSettings.shortcuts.brushDensity.combination.delta);
                ToolProperties.RepainWindow();
            }
            else if (PWBSettings.shortcuts.brushRotate.Check())
                _brushAngle -= PWBSettings.shortcuts.brushRotate.combination.delta * 1.8f; //180deg/100px
            if (Event.current.button == 1)
            {
                if (Event.current.type == EventType.MouseDown && (Event.current.control || Event.current.shift))
                {
                    _pinned = true;
                    _pinMouse = Event.current.mousePosition;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp && !Event.current.control && !Event.current.shift)
                    _pinned = false;
            }
            if ((Event.current.keyCode == KeyCode.LeftControl || Event.current.keyCode == KeyCode.RightControl
                || Event.current.keyCode == KeyCode.RightShift || Event.current.keyCode == KeyCode.LeftShift)
                && Event.current.type == EventType.KeyUp) _pinned = false;
        }

        private static Mesh ReversedMesh(Mesh mesh, int subMeshCount)
        {
            var reversed = (Mesh)GameObject.Instantiate(mesh);
            reversed.name = mesh.name + "_reversed";

            if (mesh.normals != null && mesh.normals.Length == mesh.vertexCount
                && mesh.tangents != null && mesh.tangents.Length == mesh.vertexCount)
            {
                for (int i = 0; i < reversed.normals.Length; ++i)
                {
                    reversed.normals[i] = -reversed.normals[i];
                    reversed.tangents[i].x = -reversed.tangents[i].x;
                }
            }

            reversed.subMeshCount = subMeshCount;
            for (int i = 0; i < subMeshCount; ++i)
            {
                var triangles = mesh.GetTriangles(i);
                for (int t = 0; t < triangles.Length; t += 3)
                {
                    int tmp = triangles[t + 1];
                    triangles[t + 1] = triangles[t + 2];
                    triangles[t + 2] = tmp;
                }
                reversed.SetTriangles(triangles, i);
            }
            return reversed;
        }
        private struct MeshAndRenderer
        {
            public Mesh mesh;
            public Mesh reversedMesh;
            public Matrix4x4 localToWorldMatrix;
            public Material[] materials;
            public int subMeshCount;
            public Renderer renderer;
            public MeshAndRenderer(Mesh mesh, Mesh reversedMesh, Matrix4x4 localToWorldMatrix, Material[] materials,
                int subMeshCount, Renderer renderer)
            {
                this.mesh = mesh;
                this.reversedMesh = reversedMesh;
                this.localToWorldMatrix = localToWorldMatrix;
                this.materials = materials;
                this.subMeshCount = subMeshCount;
                this.renderer = renderer;
            }
        }
        private static System.Collections.Generic.Dictionary<int, MeshAndRenderer[]> _meshesAndRenderers
            = new System.Collections.Generic.Dictionary<int, MeshAndRenderer[]>();

        private struct SpriteAnBounds
        {
            public SpriteRenderer spriteRenderer;
            public Bounds bounds;
            public MaterialPropertyBlock mpb;
            public SpriteAnBounds(SpriteRenderer spriteRenderer, Bounds bounds, MaterialPropertyBlock mpb)
            {
                this.spriteRenderer = spriteRenderer;
                this.bounds = bounds;
                this.mpb = mpb;
            }
        }

        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<SpriteAnBounds>>
            _spriteRenderers = new System.Collections.Generic.Dictionary<int,
                System.Collections.Generic.HashSet<SpriteAnBounds>>();

        public static void ClearPreviewDictionaries()
        {
            _meshesAndRenderers.Clear();
            _spriteRenderers.Clear();
        }
        private static void PreviewBrushItem(GameObject prefab, Matrix4x4 rootToWorld, int layer,
            Camera camera, bool redMaterial, bool reverseTriangles, bool flipX, bool flipY)
        {
            if (Event.current.type != EventType.Repaint) return;
            var id = prefab.GetInstanceID();

            if (!_meshesAndRenderers.ContainsKey(id))
            {
                var meshesAndRenderers = new System.Collections.Generic.List<MeshAndRenderer>();
                var renderers = prefab.GetComponentsInChildren<MeshRenderer>();
                foreach (var renderer in renderers)
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null) continue;
                    var mesh = filter.sharedMesh;
                    if (mesh == null || mesh.subMeshCount == 0) continue;
                    var materials = renderer.sharedMaterials;
                    if (materials == null || materials.Length == 0) continue;
                    var submeshCount = Mathf.Min(mesh.subMeshCount, materials.Length);
                    Mesh reversedMesh = reverseTriangles ? ReversedMesh(mesh, submeshCount) : null;
                    meshesAndRenderers.Add(new MeshAndRenderer(mesh, reversedMesh,
                        filter.transform.localToWorldMatrix, materials, submeshCount, renderer));
                }
                var skinedMeshRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var renderer in skinedMeshRenderers)
                {
                    var mesh = renderer.sharedMesh;
                    if (mesh == null) continue;
                    var materials = renderer.sharedMaterials;
                    if (materials == null || materials.Length == 0) continue;
                    var submeshCount = Mathf.Min(mesh.subMeshCount, materials.Length);
                    Mesh reversedMesh = null;
                    if (reverseTriangles) reversedMesh = ReversedMesh(mesh, submeshCount);
                    meshesAndRenderers.Add(new MeshAndRenderer(mesh, reversedMesh,
                        renderer.transform.localToWorldMatrix, materials, submeshCount, renderer));
                }
                _meshesAndRenderers.Add(id, meshesAndRenderers.ToArray());
            }

            for (int i = 0; i < _meshesAndRenderers[id].Length; ++i)
            {
                var item = _meshesAndRenderers[id][i];
                var mesh = item.mesh;
                var childToWorld = rootToWorld * item.localToWorldMatrix;

                if (!redMaterial)
                {
                    if (item.renderer is SkinnedMeshRenderer)
                    {
                        var smr = (SkinnedMeshRenderer)item.renderer;
                        var rootBone = smr.rootBone;
                        if (rootBone != null)
                        {
                            while (rootBone.parent != null && rootBone.parent != prefab.transform) rootBone = rootBone.parent;
                            var rotation = rootBone.rotation;
                            var position = rootBone.position;
                            position.y = 0f;
                            var scale = rootBone.localScale;
                            childToWorld = rootToWorld * Matrix4x4.TRS(position, rotation, scale);
                        }
                    }

                    for (int subMeshIdx = 0; subMeshIdx < item.subMeshCount; ++subMeshIdx)
                    {
                        var material = item.materials[subMeshIdx];
                        if (reverseTriangles)
                        {
                            if (item.reversedMesh == null) item.reversedMesh = ReversedMesh(mesh, item.subMeshCount);
                            Graphics.DrawMesh(item.reversedMesh, childToWorld, material, layer, camera, subMeshIdx);
                        }
                        else Graphics.DrawMesh(mesh, childToWorld, material, layer, camera, subMeshIdx);
                    }
                }
                else
                {
                    for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; ++subMeshIdx)
                        Graphics.DrawMesh(mesh, childToWorld, transparentRedMaterial, layer, camera, subMeshIdx);
                }
            }
            System.Collections.Generic.HashSet<SpriteAnBounds> spritesAndBounds = null;
            if (!_spriteRenderers.ContainsKey(id))
            {
                var spriteRenderersArray = prefab.GetComponentsInChildren<SpriteRenderer>();
                for (int i = 0; i < spriteRenderersArray.Length; ++i)
                {
                    var spriteRenderer = spriteRenderersArray[i];
                    if (spriteRenderer == null || !spriteRenderer.enabled || spriteRenderer.sprite == null
                        || !spriteRenderer.gameObject.activeSelf) continue;
                    var bounds = BoundsUtils.GetBoundsRecursive(prefab.transform);
                    var mpb = new MaterialPropertyBlock();
                    mpb.SetTexture("_MainTex", spriteRenderer.sprite.texture);
                    mpb.SetColor("_Color", spriteRenderer.color);
                    if (spritesAndBounds == null) spritesAndBounds = new System.Collections.Generic.HashSet<SpriteAnBounds>();
                    spritesAndBounds.Add(new SpriteAnBounds(spriteRenderer, bounds, mpb));
                }
                _spriteRenderers[id] = spritesAndBounds;
            }
            else spritesAndBounds = _spriteRenderers[id];
            if (spritesAndBounds != null && spritesAndBounds.Count > 0)
            {
                foreach (var snb in spritesAndBounds)
                    DrawSprite(snb.spriteRenderer, rootToWorld, camera, snb.bounds, flipX, flipY, snb.mpb);
            }
        }
        private static void DrawSprite(SpriteRenderer renderer, Matrix4x4 matrix,
            Camera camera, Bounds objectBounds, bool flipX, bool flipY, MaterialPropertyBlock mpb)
        {
            if (quadMesh == null)
            {
                quadMesh = new Mesh
                {
                    vertices = new[] { new Vector3(-.5f, .5f, 0), new Vector3(.5f, .5f, 0),
                      new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0) },
                    normals = new[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward },
                    triangles = new[] { 0, 2, 3, 3, 1, 0 }
                };
            }
            var minUV = new Vector2(float.MaxValue, float.MaxValue);
            var maxUV = new Vector2(float.MinValue, float.MinValue);
            foreach (var uv in renderer.sprite.uv)
            {
                minUV = Vector2.Min(minUV, uv);
                maxUV = Vector2.Max(maxUV, uv);
            }
            var uvs = new Vector2[] { new Vector2(minUV.x, maxUV.y),  new Vector2(maxUV.x, maxUV.y),
                new Vector2(minUV.x, minUV.y), new Vector2(maxUV.x, minUV.y)};
            void ToggleFlip(ref bool flip) => flip = !flip;
            if (renderer.flipX) ToggleFlip(ref flipX);
            if (renderer.flipY) ToggleFlip(ref flipY);
            if (flipX)
            {
                uvs[0].x = maxUV.x;
                uvs[1].x = minUV.x;
                uvs[2].x = maxUV.x;
                uvs[3].x = minUV.x;
            }
            if (flipY)
            {
                uvs[0].y = minUV.y;
                uvs[1].y = minUV.y;
                uvs[2].y = maxUV.y;
                uvs[3].y = maxUV.y;
            }
            quadMesh.uv = uvs;
            var pivotToCenter = (renderer.sprite.rect.size / 2 - renderer.sprite.pivot) / renderer.sprite.pixelsPerUnit;
            if (renderer.flipX) pivotToCenter.x = -pivotToCenter.x;
            if (renderer.flipY) pivotToCenter.y = -pivotToCenter.y;
            matrix *= Matrix4x4.Translate(pivotToCenter);
            matrix *= renderer.transform.localToWorldMatrix;
            matrix *= Matrix4x4.Scale(new Vector3(
                renderer.sprite.textureRect.width / renderer.sprite.pixelsPerUnit,
                renderer.sprite.textureRect.height / renderer.sprite.pixelsPerUnit, 1));
            Graphics.DrawMesh(quadMesh, matrix, renderer.sharedMaterial, 0, camera, 0, mpb);
        }
        private static bool IsVisible(ref GameObject obj)
        {
            if (obj == null) return false;
            var parentRenderer = obj.GetComponentInParent<Renderer>();
            var parentTerrain = obj.GetComponentInParent<Terrain>();
            if (parentRenderer != null) obj = parentRenderer.gameObject;
            else if (parentTerrain != null) obj = parentTerrain.gameObject;
            else
            {
                var parent = obj.transform.parent;
                if (parent != null)
                {
                    var siblingRenderer = parent.GetComponentInChildren<Renderer>();
                    var siblingTerrain = parent.GetComponentInChildren<Terrain>();
                    if (siblingRenderer != null) obj = parent.gameObject;
                    else if (siblingTerrain != null) obj = parent.gameObject;

                }
            }
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                foreach (var renderer in renderers)
                    if (renderer.enabled) return true;
            }
            var terrains = obj.GetComponentsInChildren<Terrain>();
            if (terrains.Length > 0)
            {
                foreach (var terrain in terrains)
                    if (terrain.enabled) return true;
            }
            return false;
        }
        private static bool IsVisible(GameObject obj)
        {
            obj = PWBCore.GetGameObjectFromTempCollider(obj);
            return IsVisible(ref obj);
        }
        private struct TerrainDataSimple
        {
            public float[,,] alphamaps;
            public Vector3 size;
            public TerrainLayer[] layers;
            public TerrainDataSimple(float[,,] alphamaps, Vector3 size, TerrainLayer[] layers)
                => (this.alphamaps, this.size, this.layers) = (alphamaps, size, layers);
        }
        private static System.Collections.Generic.Dictionary<int, TerrainDataSimple> _terrainAlphamaps
            = new System.Collections.Generic.Dictionary<int, TerrainDataSimple>();


        private static BrushstrokeItem[] _brushstroke = null;
        private struct PreviewData
        {
            public readonly GameObject prefab;
            public readonly Matrix4x4 rootToWorld;
            public readonly int layer;
            public readonly bool flipX;
            public readonly bool flipY;
            public PreviewData(GameObject prefab, Matrix4x4 rootToWorld, int layer, bool flipX, bool flipY)
            {
                this.prefab = prefab;
                this.rootToWorld = rootToWorld;
                this.layer = layer;
                this.flipX = flipX;
                this.flipY = flipY;
            }
        }
        private static System.Collections.Generic.List<PreviewData> _previewData
            = new System.Collections.Generic.List<PreviewData>();

        private static bool PreviewIfBrushtrokestaysTheSame(out BrushstrokeItem[] brushstroke,
            Camera camera, bool forceUpdate)
        {
            brushstroke = BrushstrokeManager.brushstroke;
            if (!forceUpdate && _brushstroke != null && BrushstrokeManager.BrushstrokeEqual(brushstroke, _brushstroke))
            {
                foreach (var previewItemData in _previewData)
                    PreviewBrushItem(previewItemData.prefab, previewItemData.rootToWorld,
                        previewItemData.layer, camera, false, false, previewItemData.flipX, previewItemData.flipY);
                return true;
            }
            _brushstroke = BrushstrokeManager.brushstrokeClone;
            _previewData.Clear();
            return false;
        }

        private static System.Collections.Generic.Dictionary<long, PreviewData[]> _persistentPreviewData
            = new System.Collections.Generic.Dictionary<long, PreviewData[]>();
        private static System.Collections.Generic.Dictionary<long, BrushstrokeItem[]> _persistentLineBrushstrokes
            = new System.Collections.Generic.Dictionary<long, BrushstrokeItem[]>();

        private static void PreviewPersistent(Camera camera)
        {
            foreach (var previewDataArray in _persistentPreviewData.Values)
                foreach (var previewItemData in previewDataArray)
                    PreviewBrushItem(previewItemData.prefab, previewItemData.rootToWorld,
                        previewItemData.layer, camera, false, false, previewItemData.flipX, previewItemData.flipY);
        }
        #endregion

        #region PAINT
        public static bool painting { get; set; }
        private const string PAINT_CMD = "Paint";

        private static System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<(GameObject, int)>>
            Paint(IPaintToolSettings settings, string commandName = PAINT_CMD,
            bool addTempCollider = true, bool persistent = false, string toolObjectId = "")
        {
            painting = true;
            var paintedObjects = new System.Collections.Generic.Dictionary<string,
                System.Collections.Generic.List<(GameObject, int)>>();
            if (_paintStroke.Count == 0)
            {
                if (BrushstrokeManager.brushstroke.Length == 0) BrushstrokeManager.UpdateBrushstroke();
                return paintedObjects;
            }

            foreach (var item in _paintStroke)
            {
                if (item.prefab == null) continue;
                var persistentParentId = persistent ? item.persistentParentId : toolObjectId;
                var type = UnityEditor.PrefabUtility.GetPrefabAssetType(item.prefab);
                GameObject obj = type == UnityEditor.PrefabAssetType.NotAPrefab ? GameObject.Instantiate(item.prefab)
                    : (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab
                    (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(item.prefab)
                    ? item.prefab : UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(item.prefab));
                if (PWBCore.staticData.addEnumerationToName)
                    obj.name = obj.name + "_" + PWBCore.staticData.GetPrefabCount(item.guid);
                if (settings.overwritePrefabLayer) obj.layer = settings.layer;
                obj.transform.SetPositionAndRotation(item.position, item.rotation);
                obj.transform.localScale = item.scale;
                var root = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                item.parent = GetParent(settings, item.prefab.name,
                    true, item.surface, persistentParentId);
                if (addTempCollider) PWBCore.AddTempCollider(obj);
                if (!paintedObjects.ContainsKey(persistentParentId))
                    paintedObjects.Add(persistentParentId, new System.Collections.Generic.List<(GameObject, int)>());
                paintedObjects[persistentParentId].Add((obj, item.index));
                var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();

                foreach (var spriteRenderer in spriteRenderers)
                {
                    var flipX = spriteRenderer.flipX;
                    var flipY = spriteRenderer.flipY;
                    if (item.flipX) flipX = !flipX;
                    if (item.flipY) flipY = !flipY;
                    spriteRenderer.flipX = flipX;
                    spriteRenderer.flipY = flipY;
                    var center = BoundsUtils.GetBoundsRecursive(spriteRenderer.transform,
                        spriteRenderer.transform.rotation).center;
                    var pivotToCenter = center - spriteRenderer.transform.position;
                    var delta = Vector3.zero;
                    if (item.flipX) delta.x = pivotToCenter.x * -2;
                    if (item.flipY) delta.y = pivotToCenter.y * -2;
                    spriteRenderer.transform.position += delta;
                }
                AddObjectToOctree(obj);
                UnityEditor.Undo.RegisterCreatedObjectUndo(obj, commandName);

                if (isInPrefabMode)
                {
                    if (item.parent == null) UnityEditor.Undo.SetTransformParent(obj.transform,
                            prefabStage.prefabContentsRoot.transform, commandName);
                    else UnityEditor.Undo.SetTransformParent(obj.transform, item.parent, commandName);
                }
                else if (root != null) UnityEditor.Undo.SetTransformParent(root.transform, item.parent, commandName);
                else UnityEditor.Undo.SetTransformParent(obj.transform, item.parent, commandName);
            }
            if (_paintStroke.Count > 0) BrushstrokeManager.UpdateBrushstroke();
            _paintStroke.Clear();
            return paintedObjects;
        }
        #endregion

        #region PARENTING
        public static void ResetAutoParent() => _autoParent = null;

        private const string NO_PALETTE_NAME = "<#PALETTE@>";
        private const string NO_TOOL_NAME = "<#TOOL@>";
        private const string NO_OBJ_ID = "<#ID@>";
        private const string NO_BRUSH_NAME = "<#BRUSH@>";
        private const string NO_PREFAB_NAME = "<#PREFAB@>";
        private const string PARENT_KEY_SEPARATOR = "<#@>";
        public static Transform GetParent(IPaintToolSettings settings, string prefabName,
            bool create, Transform surface, string toolObjectId = "")
        {
            IToolParentingSettings parentingSettings = PWBCore.staticData.globalParentingSettings;
            if (settings.overwriteParentingSettings) parentingSettings = settings.GetParentingSettings();
            if (!create) return parentingSettings.parent;

            var isPersistent = ToolManager.IsCurrentToolPersistent();
            if (parentingSettings.autoCreateParent)
            {
                if (isInPrefabMode)
                {
                    var root = prefabStage.prefabContentsRoot;
                    var pwbObj = root.transform.Find(PWB_OBJ_NAME);
                    if (pwbObj == null)
                    {
                        _autoParent = new GameObject(PWB_OBJ_NAME).transform;
                        _autoParent.SetParent(root.transform);
                    }
                    else _autoParent = pwbObj.transform;
                }
                else
                {
                    var pwbObj = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                        .GetRootGameObjects().FirstOrDefault(o => o.name == PWB_OBJ_NAME);
                    if (pwbObj == null) _autoParent = new GameObject(PWB_OBJ_NAME).transform;
                    else _autoParent = pwbObj.transform;
                }
            }
            else if (parentingSettings.setLastSelectedAsParent) _autoParent = UnityEditor.Selection.activeTransform;
            else if (parentingSettings.setSurfaceAsParent) _autoParent = surface;
            else _autoParent = parentingSettings.parent;

            if (!parentingSettings.createSubparentPerPalette && !parentingSettings.createSubparentPerTool && !isPersistent
                && !parentingSettings.createSubparentPerBrush && !parentingSettings.createSubparentPerPrefab)
                return _autoParent;

            var _autoParentId = _autoParent == null ? -1 : _autoParent.gameObject.GetInstanceID();
            string GetSubParentKey(int parentId = -1, string palette = NO_PALETTE_NAME,
                string tool = NO_TOOL_NAME, string id = NO_OBJ_ID,
                string brush = NO_BRUSH_NAME, string prefab = NO_PREFAB_NAME)
                => parentId + PARENT_KEY_SEPARATOR + palette + PARENT_KEY_SEPARATOR
                + tool + PARENT_KEY_SEPARATOR + id + PARENT_KEY_SEPARATOR + brush
                + PARENT_KEY_SEPARATOR + prefab;

            string subParentKey = GetSubParentKey(_autoParentId,
                parentingSettings.createSubparentPerPalette ? PaletteManager.selectedPalette.name : NO_PALETTE_NAME,
                parentingSettings.createSubparentPerTool
                ? ToolManager.GetToolFromSettings(settings as IToolSettings).ToString() : NO_TOOL_NAME,
                string.IsNullOrEmpty(toolObjectId) ? NO_OBJ_ID : toolObjectId,
                parentingSettings.createSubparentPerBrush ? PaletteManager.selectedBrush.name : NO_BRUSH_NAME,
                parentingSettings.createSubparentPerPrefab ? prefabName : NO_PREFAB_NAME);

            create = !(_subParents.ContainsKey(subParentKey));
            if (!create && _subParents[subParentKey] == null) create = true;
            if (!create) return _subParents[subParentKey];

            Transform CreateSubParent(string key, string name, Transform transformParent)
            {
                Transform subParentTransform = null;
                var subParentIsEmpty = true;
                if (transformParent != null)
                {
                    subParentTransform = transformParent.Find(name);
                    if (subParentTransform != null)
                        subParentIsEmpty = subParentTransform.GetComponents<Component>().Length == 1;
                    if (isInPrefabMode && transformParent != prefabStage.prefabContentsRoot.transform
                        && transformParent.parent == null)
                        transformParent.SetParent(prefabStage.prefabContentsRoot.transform);
                }
                else if (isInPrefabMode) transformParent = prefabStage.prefabContentsRoot.transform;

                if (subParentTransform == null || !subParentIsEmpty)
                {
                    var obj = new GameObject(name);
                    var subParent = obj.transform;
                    subParent.SetParent(transformParent);
                    subParent.localPosition = Vector3.zero;
                    subParent.localRotation = Quaternion.identity;
                    subParent.localScale = Vector3.one;
                    if (_subParents.ContainsKey(key)) _subParents[key] = subParent;
                    else _subParents.Add(key, subParent);
                    return subParent;
                }
                return subParentTransform;
            }

            var parent = _autoParent;
            void CreateSubParentIfDoesntExist(string name, string palette = NO_PALETTE_NAME,
                string tool = NO_TOOL_NAME, string id = NO_OBJ_ID, string brush = NO_BRUSH_NAME,
                string prefab = NO_PREFAB_NAME)
            {
                var key = GetSubParentKey(_autoParentId, palette, tool, id, brush, prefab);
                var keyExist = _subParents.ContainsKey(key);
                var subParent = keyExist ? _subParents[key] : null;
                if (subParent != null) parent = subParent;
                if (!keyExist || subParent == null) parent = CreateSubParent(key, name, parent);
            }

            var keySplitted = subParentKey.Split(new string[] { PARENT_KEY_SEPARATOR },
                System.StringSplitOptions.None);
            var keyPlaletteName = keySplitted[1];
            var keyToolName = keySplitted[2];
            var keyToolObjId = keySplitted[3];
            var keyBrushName = keySplitted[4];
            var keyPrefabName = keySplitted[5];

            if (keyPlaletteName != NO_PALETTE_NAME)
                CreateSubParentIfDoesntExist(keyPlaletteName, keyPlaletteName);
            if (keyToolName != NO_TOOL_NAME)
                CreateSubParentIfDoesntExist(keyToolName, keyPlaletteName, keyToolName);
            if (keyToolObjId != NO_OBJ_ID)
                CreateSubParentIfDoesntExist(keyToolObjId, keyPlaletteName, keyToolName, keyToolObjId);
            if (keyBrushName != NO_BRUSH_NAME)
                CreateSubParentIfDoesntExist(keyBrushName, keyPlaletteName, keyToolName,
                    keyToolObjId, keyBrushName);
            if (keyPrefabName != NO_PREFAB_NAME)
                CreateSubParentIfDoesntExist(keyPrefabName, keyPlaletteName,
                    keyToolName, keyToolObjId, keyBrushName, keyPrefabName);
            return parent;
        }
        #endregion

        #region RAYCAST & DISTANCE TO SURFACE
        public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask,
            QueryTriggerInteraction queryTriggerInteraction)
        {
            if (isInPrefabMode)
            {
                var physScene = prefabStage.scene.GetPhysicsScene();
                return physScene.Raycast(
                    ray.origin,
                    ray.direction,
                    out hitInfo,
                    maxDistance,
                    layerMask,
                    queryTriggerInteraction);
            }
            else return Physics.Raycast(ray, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }
        private static readonly System.Collections.Generic.List<RaycastHit> _raycastAllHits
            = new System.Collections.Generic.List<RaycastHit>();
        public static RaycastHit[] RaycastAll(
        Ray ray,
        float maxDistance,
        int layerMask,
        QueryTriggerInteraction queryTriggerInteraction)
        {
            if (isInPrefabMode)
            {
                bool useGlobal = queryTriggerInteraction == QueryTriggerInteraction.UseGlobal;
                bool hitTriggers = useGlobal
                    ? Physics.queriesHitTriggers
                    : queryTriggerInteraction == QueryTriggerInteraction.Collide;

                _raycastAllHits.Clear();
                var root = prefabStage.prefabContentsRoot;
                var colliders = root.GetComponentsInChildren<Collider>(false);
                for (int i = 0; i < colliders.Length; i++)
                {
                    var col = colliders[i];
                    if (((1 << col.gameObject.layer) & layerMask) == 0) continue;
                    if (!hitTriggers && col.isTrigger) continue;
                    if (col.Raycast(ray, out RaycastHit hit, maxDistance))
                        _raycastAllHits.Add(hit);
                }
                _raycastAllHits.Sort((a, b) => a.distance.CompareTo(b.distance));
                return _raycastAllHits.ToArray();
            }
            return Physics.RaycastAll(ray, maxDistance, layerMask, queryTriggerInteraction);
        }

        private static readonly System.Collections.Generic.Dictionary<int, TerrainDataSimple> _terrainCache
            = new System.Collections.Generic.Dictionary<int, TerrainDataSimple>();

        public static bool PWBToolRaycast(
            Ray mouseRay,
            out RaycastHit mouseHit,
            out GameObject collider,
            float maxDistance,
            LayerMask layerMask,
            bool paintOnPalettePrefabs,
            bool castOnMeshesWithoutCollider,
            string[] tags = null,
            TerrainLayer[] terrainLayers = null,
            System.Collections.Generic.HashSet<GameObject> exceptions = null,
            bool sameOriginAsRay = true,
            Vector3 origin = default,
            bool createTempColliders = false,
            bool ignoreSceneColliders = false)
        {
            mouseHit = new RaycastHit();
            collider = null;

            System.Collections.Generic.HashSet<string> tagSet = (tags != null && tags.Length > 0)
                ? new System.Collections.Generic.HashSet<string>(tags)
                : null;

            Plane originPlane = default;
            if (!sameOriginAsRay) originPlane = new Plane(mouseRay.direction, origin);

            bool validHit = false;
            bool valid = false;
#if UNITY_2021_1_OR_NEWER
            using (UnityEngine.Pool.DictionaryPool<GameObject, RaycastHit>
                .Get(out System.Collections.Generic.Dictionary<GameObject, RaycastHit> hitDictionary))
#else
            var hitDictionary = new System.Collections.Generic.Dictionary<GameObject, RaycastHit>();
#endif
            {
#if UNITY_2021_1_OR_NEWER
                using (UnityEngine.Pool.ListPool<GameObject>
                    .Get(out System.Collections.Generic.List<GameObject> nearbyObjectsList))
#else
                var nearbyObjectsList = new System.Collections.Generic.List<GameObject>();
#endif

                {
                    void MeshRaycast()
                    {
                        boundsOctree.GetColliding(nearbyObjectsList, mouseRay, maxDistance);
#if UNITY_2021_1_OR_NEWER
                        using (UnityEngine.Pool.ListPool<RaycastHit>
                            .Get(out System.Collections.Generic.List<RaycastHit> hitResultsList))
#else
                        var hitResultsList = new System.Collections.Generic.List<RaycastHit>();
#endif
#if UNITY_2021_1_OR_NEWER
                        using (UnityEngine.Pool.ListPool<GameObject>
                            .Get(out System.Collections.Generic.List<GameObject> collidersResultsList))
#else
                        var collidersResultsList = new System.Collections.Generic.List<GameObject>();
#endif
                        {
                            if (createTempColliders && !isInPrefabMode)
                            {
                                foreach (var obj in nearbyObjectsList) PWBCore.AddTempCollider(obj);
                                PhysicsRaycast();
                            }
                            else if (MeshUtils.RaycastAll(mouseRay, hitResultsList, collidersResultsList,
                                nearbyObjectsList, maxDistance, sameOriginAsRay, origin))
                            {
                                for (int i = 0; i < hitResultsList.Count; i++)
                                {
                                    var obj = collidersResultsList[i];
                                    float dist = sameOriginAsRay ? hitResultsList[i].distance
                                        : Mathf.Abs(originPlane.GetDistanceToPoint(hitResultsList[i].point));

                                    if (hitDictionary.TryGetValue(obj, out var existing))
                                    {
                                        if (dist < (sameOriginAsRay
                                            ? existing.distance : Mathf.Abs(originPlane.GetDistanceToPoint(existing.point))))
                                            hitDictionary[obj] = hitResultsList[i];
                                    }
                                    else
                                    {
                                        hitDictionary.Add(obj, hitResultsList[i]);
                                    }
                                }
                            }
                        }
                    }

                    bool PhysicsRaycast()
                    {
                        validHit = Raycast(mouseRay, out RaycastHit hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
                        if (validHit)
                        {
                            var allHits = RaycastAll(mouseRay, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
                            for (int i = 0; i < allHits.Length; i++)
                            {
                                var obj = allHits[i].collider.gameObject;
                                if (ignoreSceneColliders && _sceneColliders.Contains(allHits[i].collider.GetInstanceID()))
                                    continue;
                                float dist = sameOriginAsRay ? allHits[i].distance
                                    : Mathf.Abs(originPlane.GetDistanceToPoint(allHits[i].point));

                                if (hitDictionary.TryGetValue(obj, out var existing))
                                {
                                    if (dist < (sameOriginAsRay
                                        ? existing.distance : Mathf.Abs(originPlane.GetDistanceToPoint(existing.point))))
                                        hitDictionary[obj] = allHits[i];
                                }
                                else
                                {
                                    hitDictionary.Add(obj, allHits[i]);
                                }
                            }
                        }
                        return validHit;
                    }

                    if (castOnMeshesWithoutCollider)
                    {
                        if (ignoreSceneColliders) MeshRaycast();
                        else if (!PhysicsRaycast()) MeshRaycast();
                    }
                    else
                    {
                        if (ignoreSceneColliders)
                        {
                            foreach (var id in _sceneColliders)
                            {
                                var c = UnityEditor.EditorUtility.InstanceIDToObject(id) as Collider;
                                if (c != null) PWBCore.AddTempCollider(c.gameObject);
                            }
                            PhysicsRaycast();
                        }
                        else PhysicsRaycast();
                    }
                }
                float minDistance = float.MaxValue;

                foreach (var pair in hitDictionary)
                {
                    var obj = ResolveOriginal(pair.Key);
                    var hitInfo = pair.Value;

                    float dist = sameOriginAsRay ? hitInfo.distance : originPlane.GetDistanceToPoint(hitInfo.point);

                    if (Mathf.Abs(dist) < minDistance && FiltersPassed(obj, hitInfo.point, paintOnPalettePrefabs, tagSet,
                        terrainLayers, exceptions))
                    {
                        minDistance = dist;
                        collider = obj;
                        mouseHit = hitInfo;
                        mouseHit.distance = dist;
                        valid = true;
                    }
                }
            }
            return valid;
        }

        private static bool ResolveTemp(GameObject obj)
        {
            var parent = obj.transform.parent;
            return parent != null && parent.gameObject.GetInstanceID() == PWBCore.parentColliderId;
        }

        private static GameObject ResolveOriginal(GameObject obj)
        {
            return ResolveTemp(obj) ? PWBCore.GetGameObjectFromTempColliderId(obj.GetInstanceID()) : obj;
        }

        private static bool FiltersPassed(
            GameObject obj,
            Vector3 hitPoint,
            bool paintOnPalettePrefabs,
            System.Collections.Generic.HashSet<string> tagSet,
            TerrainLayer[] terrainLayers,
            System.Collections.Generic.HashSet<GameObject> exceptions)
        {
            if (obj == null || !IsVisible(obj))
                return false;

            if (exceptions != null && exceptions.Count > 0 && exceptions.Contains(obj))
                return false;

            if (tagSet != null && obj.tag != "untagged" && !tagSet.Contains(obj.tag))
                return false;

            if (!paintOnPalettePrefabs && PaletteManager.selectedPalette.ContainsSceneObject(obj))
                return false;

            if (terrainLayers != null && terrainLayers.Length > 0)
                return TerrainCheck(obj, hitPoint, terrainLayers);
            return true;
        }

        private static bool TerrainCheck(GameObject obj, Vector3 hitPoint, TerrainLayer[] terrainLayers)
        {
            var terrain = obj.GetComponent<Terrain>();
            if (terrain == null) return true;

            int id = terrain.GetInstanceID();
            if (!_terrainCache.TryGetValue(id, out var data))
            {
                var td = terrain.terrainData;
                if (td == null) return false;
                data = new TerrainDataSimple(td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight),
                    td.size, td.terrainLayers);
                _terrainCache[id] = data;
            }

            Vector3 local = terrain.transform.InverseTransformPoint(hitPoint);
            int x = Mathf.Clamp(Mathf.RoundToInt(local.x / data.size.x * data.alphamaps.GetLength(1)),
                0, data.alphamaps.GetLength(1) - 1);
            int z = Mathf.Clamp(Mathf.RoundToInt(local.z / data.size.z * data.alphamaps.GetLength(0)),
                0, data.alphamaps.GetLength(0) - 1);

            int layerIdx = 0;
            for (int k = 1; k < data.alphamaps.GetLength(2); k++)
                if (data.alphamaps[z, x, k] > 0.5f)
                {
                    layerIdx = k;
                    break;
                }

            foreach (var layer in terrainLayers)
                if (layer == data.layers[layerIdx])
                    return true;

            return false;
        }

        public static float GetDistanceToSurface(Vector3[] vertices, Matrix4x4 TRS, Vector3 direction, float magnitude,
          bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider, bool ignoreSceneColliders, out Transform surface,
          GameObject prefab, System.Collections.Generic.HashSet<GameObject> exceptions = null, bool createTemColliders = true)
        {
            surface = null;
            var distance = 0f;
            void GetDistance(float height, Vector3 direction, out GameObject collider)
            {
                collider = null;
                var positiveDistance = float.MinValue;
                var negativeDistance = float.MinValue;
                foreach (var vertex in vertices)
                {
                    var origin = TRS.MultiplyPoint(vertex);
                    var ray = new Ray(origin - (direction * height), direction);
                    if (PWBToolRaycast(ray, out RaycastHit hitInfo, out GameObject rayCollider,
                        float.MaxValue, -1, paintOnPalettePrefabs, castOnMeshesWithoutCollider,
                        tags: null, terrainLayers: null, exceptions, sameOriginAsRay: false, origin, createTemColliders,
                        ignoreSceneColliders: ignoreSceneColliders))
                    {
                        var prevPosDistance = positiveDistance;
                        var prevNegDistance = negativeDistance;
                        if (hitInfo.distance >= 0) positiveDistance = Mathf.Max(hitInfo.distance, positiveDistance);
                        else negativeDistance = Mathf.Max(hitInfo.distance, negativeDistance);
                        if (Mathf.Approximately(prevPosDistance, positiveDistance)
                            && Mathf.Approximately(prevNegDistance, negativeDistance)) continue;
                        distance = positiveDistance >= 0 ? positiveDistance : negativeDistance;
                        collider = rayCollider;
                    }
                }
                if (Mathf.Approximately(distance, float.MinValue) || Mathf.Approximately(distance, float.MaxValue))
                    distance = 0;
            }
            var scale = TRS.lossyScale;
            var scaleMult = Mathf.Max(scale.x + scale.y + scale.z, 1) * 9;
            float hMult = magnitude * scaleMult;
            GetDistance(hMult, direction, out GameObject surfaceCollider);
            if (surfaceCollider != null) surface = surfaceCollider.transform;
            return distance;
        }

        public static float GetBottomDistanceToSurface(Vector3[] bottomVertices, Matrix4x4 TRS,
            float magnitude, bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider, bool ignoreSceneColliders,
            out Transform surface, System.Collections.Generic.HashSet<GameObject> exceptions = null)
        {
            surface = null;
            var distance = 0f;
            void GetDistance(float height, Vector3 direction, out GameObject collider)
            {
                collider = null;
                var positiveDistance = float.MinValue;
                var negativeDistance = float.MinValue;
                foreach (var vertex in bottomVertices)
                {
                    var origin = TRS.MultiplyPoint(vertex);
                    var ray = new Ray(origin - (direction * height), direction);
                    if (PWBToolRaycast(ray, out RaycastHit hitInfo, out GameObject rayCollider, float.MaxValue, -1,
                        paintOnPalettePrefabs, castOnMeshesWithoutCollider, tags: null, terrainLayers: null, exceptions,
                        sameOriginAsRay: false, origin, createTempColliders: true,
                        ignoreSceneColliders: ignoreSceneColliders))
                    {
                        var prevPosDistance = positiveDistance;
                        var prevNegDistance = negativeDistance;
                        if (hitInfo.distance >= 0) positiveDistance = Mathf.Max(hitInfo.distance, positiveDistance);
                        else negativeDistance = Mathf.Max(hitInfo.distance, negativeDistance);
                        if (Mathf.Approximately(prevPosDistance, positiveDistance)
                            && Mathf.Approximately(prevNegDistance, negativeDistance)) continue;
                        distance = positiveDistance >= 0 ? positiveDistance : negativeDistance;
                        collider = rayCollider;
                    }
                }
                if (Mathf.Approximately(distance, float.MinValue) || Mathf.Approximately(distance, float.MaxValue))
                    distance = 0;
            }
            var scale = TRS.lossyScale;
            var scaleMult = Mathf.Max(scale.x + scale.y + scale.z, 1) * 9;
            float hMult = magnitude * scaleMult;
            var down = (TRS.rotation * Vector3.down).normalized;
            GetDistance(hMult, down, out GameObject surfaceCollider);
            if (surfaceCollider != null) surface = surfaceCollider.transform;
            return distance;
        }

        public static float GetBottomDistanceToSurfaceSigned(Vector3[] bottomVertices, Matrix4x4 TRS,
            float maxDistance, bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider, bool ignoreSceneColliders)
        {
            float distance = 0f;
            var down = Vector3.down;
            foreach (var vertex in bottomVertices)
            {
                var origin = TRS.MultiplyPoint(vertex);
                var ray = new Ray(origin - down * maxDistance, down);
                if (PWBToolRaycast(ray, out RaycastHit hitInfo, out GameObject collider,
                    float.MaxValue, -1, paintOnPalettePrefabs, castOnMeshesWithoutCollider,
                    ignoreSceneColliders: ignoreSceneColliders))
                {
                    var d = hitInfo.distance - maxDistance;
                    if (Mathf.Abs(d) > Mathf.Abs(distance)) distance = d;
                }
            }
            return distance;
        }

        public static float GetPivotDistanceToSurfaceSigned(Vector3 pivot,
            float maxDistance, bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider, bool ignoreSceneColliders,
            out Transform surface, System.Collections.Generic.HashSet<GameObject> exceptions = null)
        {
            surface = null;
            var ray = new Ray(pivot + Vector3.up * maxDistance, Vector3.down);
            if (PWBToolRaycast(ray, out RaycastHit hitInfo, out GameObject collider, float.MaxValue, -1, paintOnPalettePrefabs,
                castOnMeshesWithoutCollider, exceptions: exceptions, ignoreSceneColliders: ignoreSceneColliders))
            {
                surface = collider.transform;
                return hitInfo.distance - maxDistance;
            }
            return 0;
        }
        #endregion

        #region BRUSH SHAPE INDICATOR
        private const float NormalOffset = 0.01f;
        private const float PolygonSideSize = 0.3f;
        private const int MinPolygonSides = 12;
        private const int MaxPolygonSides = 36;
        private const float ShadowOffset = 0.2f;

        private static readonly System.Collections.Generic.List<Vector3> _periPoints
            = new System.Collections.Generic.List<Vector3>();
        private static readonly System.Collections.Generic.List<Vector3> _dropAreaPeriPoints
            = new System.Collections.Generic.List<Vector3>();

        public static void DrawCircleIndicator(
            Vector3 hitPoint,
            Vector3 hitNormal,
            float radius,
            float height,
            Vector3 tangent,
            Vector3 bitangent,
            Vector3 normal,
            bool paintOnPalettePrefabs,
            bool castOnMeshesWithoutCollider,
            int layerMask = -1,
            string[] tags = null,
            bool drawDropArea = false)
        {
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            int polygonSides = Mathf.Clamp((int)(TAU * radius / PolygonSideSize), MinPolygonSides, MaxPolygonSides);

            Vector3 center = hitPoint + hitNormal * NormalOffset;
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.5f);
            UnityEditor.Handles.DrawWireDisc(center, hitNormal, radius, 3);
            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.5f);
            UnityEditor.Handles.DrawWireDisc(center, hitNormal, radius + ShadowOffset, 3);

            if (drawDropArea)
            {
                var heightOffset = normal * height;
                UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.5f);
                UnityEditor.Handles.DrawWireDisc(center + heightOffset, hitNormal, radius, 3);
            }
        }

        private const int MinSideSegments = 4;
        private const int MaxSideSegments = 15;
        private const float SegmentDivisor = 0.3f;

        public static void DrawSquareIndicator(
        Vector3 hitPoint,
        float radius,
        float height,
        Vector3 tangent,
        Vector3 bitangent,
        Vector3 normal,
        bool drawDropArea = false)
        {
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            int segments = Mathf.Clamp((int)(radius * 2f / SegmentDivisor), MinSideSegments, MaxSideSegments);
            int segmentCount = segments * 4;
            float segmentSize = radius * 2f / segments;

            _periPoints.Clear();
            _dropAreaPeriPoints.Clear();

            Vector3 heightOffset = normal * height;
            for (int i = 0; i < segmentCount; i++)
            {
                int side = i / segments;
                int idx = i % segments;
                Vector3 peri = hitPoint;
                switch (side)
                {
                    case 0: peri += tangent * (segmentSize * idx - radius) + bitangent * radius; break;
                    case 1: peri += bitangent * (radius - segmentSize * idx) + tangent * radius; break;
                    case 2: peri += tangent * (radius - segmentSize * idx) - bitangent * radius; break;
                    default: peri += bitangent * (segmentSize * idx - radius) - tangent * radius; break;
                }

                _periPoints.Add(peri);
                if (drawDropArea) _dropAreaPeriPoints.Add(peri + heightOffset);
            }

            if (_periPoints.Count > 0)
            {
                _periPoints.Add(_periPoints[0]);
                UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
                UnityEditor.Handles.DrawAAPolyLine(8, _periPoints.ToArray());

                UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.7f);
                UnityEditor.Handles.DrawAAPolyLine(4, _periPoints.ToArray());
            }

            if (drawDropArea && _dropAreaPeriPoints.Count > 0)
            {
                _dropAreaPeriPoints.Add(_dropAreaPeriPoints[0]);
                UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.5f);
                UnityEditor.Handles.DrawAAPolyLine(3, _dropAreaPeriPoints.ToArray());
            }
        }
        #endregion

        #region HANDLES
        private static float _blinkingDelta = 0.05f;
        private static float _blinkingValue = 1f;
        private static void DrawDotHandleCap(Vector3 point, float alpha = 1f,
            float scale = 1f, bool selected = false, bool isPivot = false)
        {
            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f * alpha);
            var handleSize = UnityEditor.HandleUtility.GetHandleSize(point);
            var sizeDelta = handleSize * 0.0125f;
            UnityEditor.Handles.DotHandleCap(0, point, Quaternion.identity,
                handleSize * 0.0325f * scale * PWBCore.staticData.controPointSize, EventType.Repaint);
            var fillColor = selected ? PWBCore.staticData.selectedContolPointColor
                : (isPivot ? Color.green : UnityEditor.Handles.preselectionColor);
            fillColor.a *= alpha;
            if (selected && PWBCore.staticData.selectedControlPointBlink)
            {
                fillColor.a *= _blinkingValue;
                if (_blinkingValue >= 1) _blinkingDelta = -Mathf.Abs(_blinkingDelta);
                else if (_blinkingValue <= 0) _blinkingDelta = Mathf.Abs(_blinkingDelta);
                _blinkingValue += _blinkingDelta;
            }
            UnityEditor.Handles.color = fillColor;
            UnityEditor.Handles.DotHandleCap(0, point, Quaternion.identity,
                (handleSize * 0.0325f * scale - sizeDelta) * PWBCore.staticData.controPointSize, EventType.Repaint);
        }
        #endregion

        #region DRAG AND DROP
        public class SceneDragReceiver : ISceneDragReceiver
        {
            private int _brushID = -1;
            public int brushId { get => _brushID; set => _brushID = value; }
            public void PerformDrag(Event evt) { }
            public void StartDrag() { }
            public void StopDrag() { }
            public UnityEditor.DragAndDropVisualMode UpdateDrag(Event evt, EventType eventType)
            {
                PrefabPalette.instance.DeselectAllButThis(_brushID);
                ToolManager.tool = ToolManager.PaintTool.PIN;
                return UnityEditor.DragAndDropVisualMode.Generic;
            }
        }
        private static SceneDragReceiver _sceneDragReceiver = new SceneDragReceiver();
        public static SceneDragReceiver sceneDragReceiver => _sceneDragReceiver;




        #endregion

        #region PALETTE
        public static void ReplaceSelected()
        {
            var replacerSettings = new ReplacerSettings();
            _paintStroke.Clear();
            SelectionManager.UpdateSelection();
            var targets = SelectionManager.topLevelSelection;
            BrushstrokeManager.UpdateReplacerBrushstroke(clearDictionary: true, targets);
            ReplacePreview(UnityEditor.SceneView.lastActiveSceneView.camera, replacerSettings, targets);
            var newObjects = new System.Collections.Generic.HashSet<GameObject>();
            Replace(newObjects);
            if (newObjects != null)
                if (newObjects.Count > 0) UnityEditor.Selection.objects = newObjects.ToArray();
        }
        private static void PaletteInput(UnityEditor.SceneView sceneView)
        {
            void Repaint()
            {
                PrefabPalette.RepaintWindow();
                sceneView.Repaint();
                repaint = true;
                AsyncRepaint();
            }
            if (PWBSettings.shortcuts.palettePreviousBrush.Check())
            {
                PaletteManager.SelectPreviousBrush();
                Repaint();
            }
            else if (PWBSettings.shortcuts.paletteNextBrush.Check())
            {
                PaletteManager.SelectNextBrush();
                Repaint();
            }
            if (PWBSettings.shortcuts.paletteNextBrushScroll.Check())
            {
                Event.current.Use();
                if (PWBSettings.shortcuts.paletteNextBrushScroll.combination.delta > 0) PaletteManager.SelectNextBrush();
                else PaletteManager.SelectPreviousBrush();
                Repaint();
            }
            if (PWBSettings.shortcuts.paletteNextPaletteScroll.Check())
            {
                Event.current.Use();
                if (PWBSettings.shortcuts.paletteNextPaletteScroll.combination.delta > 0) PaletteManager.SelectNextPalette();
                else PaletteManager.SelectPreviousPalette();
                Repaint();
            }
            if (PWBSettings.shortcuts.palettePreviousPalette.Check())
            {
                PaletteManager.SelectPreviousPalette();
                Repaint();
            }
            else if (PWBSettings.shortcuts.paletteNextPalette.Check())
            {
                PaletteManager.SelectNextPalette();
                Repaint();
            }
            if (PWBSettings.shortcuts.paletteReplaceSceneSelection.Check())
            {
                ReplaceSelected();
            }
            var pickShortcutOn = PWBSettings.shortcuts.palettePickBrush.Check();
            var pickBrush = PaletteManager.pickingBrushes && Event.current.button == 0
                && Event.current.type == EventType.MouseDown;
            if (pickShortcutOn || pickBrush)
            {
                var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (PWBToolRaycast(mouseRay, out RaycastHit mouseHit, out GameObject collider, float.MaxValue, layerMask: -1,
                    paintOnPalettePrefabs: true, castOnMeshesWithoutCollider: true, ignoreSceneColliders: true))
                {
                    var target = collider.gameObject;
                    var outermostPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(target);
                    if (outermostPrefab != null) target = outermostPrefab;
                    var brushIdx = PaletteManager.selectedPalette.FindBrushIdx(target);
                    if (brushIdx >= 0) PaletteManager.SelectBrush(brushIdx);
                    else if (outermostPrefab != null)
                    {
                        var prefabAsset = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(outermostPrefab);
                        PrefabPalette.instance.CreateBrushFromSelection(prefabAsset);
                    }
                }
                Event.current.Use();
                if (!pickShortcutOn && pickBrush) PaletteManager.pickingBrushes = false;
            }
            if (PaletteManager.pickingBrushes
                && Event.current.type == EventType.KeyDown
                && Event.current.keyCode == KeyCode.Escape)
            {
                PaletteManager.pickingBrushes = false;
            }
            if (PaletteManager.pickingBrushes)
            {
                if (boundsOctree.Count == 0) UpdateOctree();
                var labelTexts = new string[] { $"Brush Picker", "Object: " };
                var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                var objName = "None";
                if (PWBToolRaycast(mouseRay, out RaycastHit mouseHit, out GameObject collider, float.MaxValue, layerMask: -1,
                    paintOnPalettePrefabs: true, castOnMeshesWithoutCollider: true, ignoreSceneColliders: true))
                {
                    var target = collider.gameObject;
                    var outermostPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(target);
                    if (outermostPrefab != null) objName = outermostPrefab.name;
                }
                labelTexts[1] += objName;
                InfoText.Draw(sceneView, labelTexts.ToArray());
            }
            if (PWBSettings.shortcuts.palettePickBrush.holdKeysAndClickCombination.holdingChanged)
                PaletteManager.pickingBrushes = PWBSettings.shortcuts.palettePickBrush.holdKeysAndClickCombination.holdingKeys;
        }
        async static void AsyncRepaint()
        {
            await System.Threading.Tasks.Task.Delay(500);
            repaint = true;
        }
        #endregion

        #region TOOLBAR
        public static void ToogleTool(ToolManager.PaintTool tool)
        {
#if UNITY_2021_2_OR_NEWER
#else
            if (PWBToolbar.instance == null) PWBToolbar.ShowWindow();
#endif
            ToolManager.tool = ToolManager.tool == tool ? ToolManager.PaintTool.NONE : tool;
            PWBToolbar.RepaintWindow();
        }
        #endregion

        #region MODULAR
        private static bool _modularDeleteMode = false;
        private static Vector3 GetCenterToPivot(GameObject prefab, Vector3 scaleMult, Quaternion rotation)
        {
            var itemBounds = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation);
            var centerToPivotGlobal = prefab.transform.position - itemBounds.center;
            var centerToPivotLocal = Quaternion.Inverse(prefab.transform.rotation) * centerToPivotGlobal;
            var result = rotation * Vector3.Scale(centerToPivotLocal, scaleMult);
            return result;
        }
        #endregion

        #region GIZMOS
        private static void GizmosInput()
        {
            if (PWBSettings.shortcuts.gizmosToggleInfotext.Check())
            {
                PWBCore.staticData.ToggleInfoText();
            }
        }
        #endregion

        #region BRUSH PROPERTIES
        private static bool _offsetPicking = false;
        private static AxesUtils.Axis _offsetPickingAxis;
        private static float _offsetPickingValue = 0f;
        private static BrushSettings _offsetPickingBrush = null;
        public static void EnableOffsetPicking(AxesUtils.Axis axis, BrushSettings brush)
        {
            _offsetPickingBrush = brush;
            ToolManager.tool = ToolManager.PaintTool.NONE;
            _offsetPicking = true;
            _offsetPickingAxis = axis;
            _offsetPickingValue = 0f;
            UpdateOctree();
            if (UnityEditor.SceneView.sceneViews.Count > 0)
                ((UnityEditor.SceneView)UnityEditor.SceneView.sceneViews[0]).Focus();
        }

        public static bool OffsetRaycast(out RaycastHit mouseHit, out GameObject collider)
        {
            mouseHit = new RaycastHit();
            collider = null;
            if (boundsOctree == null) return false;

            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            GameObject[] nearbyObjects = null;

            nearbyObjects = boundsOctree.GetColliding(mouseRay);
            if (nearbyObjects.Length == 0) return false;
            nearbyObjects = nearbyObjects.Where(o => o != null).ToArray();
            if (nearbyObjects.Length == 0) return false;

            var validHit = false;
            var minDistance = float.MaxValue;

            foreach (var obj in nearbyObjects)
            {
                if (!MeshUtils.RayIntersectsGameObject(mouseRay, obj, includeInactive: false, out Vector3 hitPoint,
                    out float distance, out Vector3 hitNormal)) continue;
                if (distance >= minDistance) continue;
                minDistance = distance;
                mouseHit.point = hitPoint;
                mouseHit.distance = distance;
                collider = obj;
                validHit = true;
            }
            return validHit;
        }

        private static void OffsetPicking(Camera sceneCamera)
        {
            _offsetPickingValue = 0;
            if (!OffsetRaycast(out RaycastHit hit, out GameObject obj)) return;
            if (obj == null) return;
            var bounds = BoundsUtils.GetBoundsRecursive(obj.transform, obj.transform.rotation);
            var localHit = obj.transform.InverseTransformPoint(hit.point);

            var localCenter = obj.transform.InverseTransformPoint(bounds.center);
            var halfLocalSize = new Vector3(bounds.size.x / obj.transform.lossyScale.x,
                bounds.size.y / obj.transform.lossyScale.y,
                bounds.size.z / obj.transform.lossyScale.z) * 0.5f;
            var localMin = localCenter - halfLocalSize;
            var localMax = localCenter + halfLocalSize;

            var minShift = AxesUtils.GetAxisValue(localMin, _offsetPickingAxis);
            var maxShift = AxesUtils.GetAxisValue(localMax, _offsetPickingAxis);
            var hitShift = AxesUtils.GetAxisValue(localHit, _offsetPickingAxis);

            _offsetPickingValue = Mathf.Abs(hitShift >= 0f ? maxShift - hitShift : hitShift - minShift);
#if UNITY_2021_1_OR_NEWER
            _offsetPickingValue = System.MathF.Round(_offsetPickingValue, digits: 5);
#else
            _offsetPickingValue = (float)System.Math.Round(_offsetPickingValue, digits: 5);
#endif
        }
        #endregion

        #region MATERIALS & MESHES
        private static Material _transparentRedMaterial = null;
        public static Material transparentRedMaterial
        {
            get
            {
                if (_transparentRedMaterial == null)
                    _transparentRedMaterial = new Material(Shader.Find("PluginMaster/TransparentRed"));
                return _transparentRedMaterial;
            }
        }

        private static Material _transparentRedMaterial2 = null;
        public static Material transparentRedMaterial2
        {
            get
            {
                if (_transparentRedMaterial2 == null)
                    _transparentRedMaterial2 = new Material(Shader.Find("PluginMaster/TransparentRed2"));
                return _transparentRedMaterial2;
            }
        }

        private static Material _snapBoxMaterial = null;
        public static Material snapBoxMaterial
        {
            get
            {
                if (_snapBoxMaterial == null)
                    _snapBoxMaterial = new Material(Shader.Find("PluginMaster/SnapBox"));
                return _snapBoxMaterial;
            }
        }

        private static Mesh _cubeMesh = null;
        private static Mesh cubeMesh
        {
            get
            {
                if (_cubeMesh == null) _cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                return _cubeMesh;
            }
        }
        #endregion

        #region PREFAB STAGE
#if UNITY_2021_1_OR_NEWER
        public static bool isInPrefabMode => prefabStage != null;

        public static UnityEditor.SceneManagement.PrefabStage prefabStage
            => UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();

        private static void OnPrefabStageChanged(UnityEditor.SceneManagement.PrefabStage stage)
        {
            if (ToolManager.tool == ToolManager.PaintTool.NONE) return;
            UpdateOctree();
        }
#else
        public static bool isInPrefabMode => false;
        public class PrefabStage
        {
            public string assetPath = null;
            public GameObject prefabContentsRoot = null;
            public UnityEngine.SceneManagement.Scene scene;
        }
        public static PrefabStage prefabStage => null;
#endif
        #endregion
    }
}