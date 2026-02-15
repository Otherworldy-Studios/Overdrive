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
    #region DATA & SETTINGS
    [System.Serializable]
    public class TilingSettings : PaintOnSurfaceToolSettings, IPaintToolSettings
    {
        #region TILING SETTINGS

        [SerializeField] private TilesUtils.SizeType _cellSizeType = TilesUtils.SizeType.SMALLEST_OBJECT;
        [SerializeField] private Vector2 _cellSize = Vector2.one;
        [SerializeField] private Quaternion _rotation = Quaternion.identity;
        [SerializeField] private Vector2 _spacing = Vector2.zero;
        [SerializeField] private AxesUtils.SignedAxis _axisAlignedWithNormal = AxesUtils.SignedAxis.UP;
        [SerializeField] private bool _showPreview = true;
        public Quaternion rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value) return;
                var prevRotation = _rotation;
                _rotation = value;
                OnDataChanged();
            }
        }

        public TilesUtils.SizeType cellSizeType
        {
            get => _cellSizeType;
            set
            {
                if (_cellSizeType == value) return;
                _cellSizeType = value;
                UpdateCellSize();
            }
        }
        public Vector2 cellSize
        {
            get => _cellSize;
            set
            {
                if (_cellSize == value) return;
                _cellSize = value;
                OnDataChanged();
            }
        }
        public Vector2 spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                OnDataChanged();
            }
        }
        public AxesUtils.SignedAxis axisAlignedWithNormal
        {
            get => _axisAlignedWithNormal;
            set
            {
                if (_axisAlignedWithNormal == value) return;
                _axisAlignedWithNormal = value;
                UpdateCellSize();
                OnDataChanged();
            }
        }
        public bool showPreview
        {
            get => _showPreview;
            set
            {
                if (_showPreview == value) return;
                _showPreview = value;
                OnDataChanged();
            }
        }
        public void UpdateCellSize()
        {
            if (ToolManager.tool != ToolManager.PaintTool.TILING) return;

            if (_cellSizeType != TilesUtils.SizeType.CUSTOM)
            {
                var toolSettings = TilingManager.settings;
                BrushSettings brush = PaletteManager.selectedBrush;
                if (ToolManager.editMode && brush == null) brush = brushSettings;
                else if (toolSettings.overwriteBrushProperties) brush = toolSettings.brushSettings;
                if (brush == null) return;
                AxesUtils.SignedAxis forwardAxis = AxesUtils.SignedAxis.FORWARD;
                if (_axisAlignedWithNormal == AxesUtils.SignedAxis.DOWN) forwardAxis = AxesUtils.SignedAxis.BACK;
                else if (_axisAlignedWithNormal == AxesUtils.SignedAxis.RIGHT) forwardAxis = AxesUtils.SignedAxis.UP;
                else if (_axisAlignedWithNormal == AxesUtils.SignedAxis.LEFT) forwardAxis = AxesUtils.SignedAxis.DOWN;
                else if (_axisAlignedWithNormal == AxesUtils.SignedAxis.FORWARD) forwardAxis = AxesUtils.SignedAxis.RIGHT;
                else if (_axisAlignedWithNormal == AxesUtils.SignedAxis.BACK) forwardAxis = AxesUtils.SignedAxis.LEFT;
                _cellSize = TilesUtils.GetCellSize(_cellSizeType, brush, _axisAlignedWithNormal,
                    forwardAxis, _cellSize, tangentSpace: true, quarterTurns: 0, subtractBrushOffset: false);
                ToolProperties.RepainWindow();
                UnityEditor.SceneView.RepaintAll();
            }
            OnDataChanged();
        }
        #endregion

        #region ON DATA CHANGED
        public TilingSettings() : base()
        {
            _paintTool.OnDataChanged += DataChanged;
            _paintTool.brushSettings.OnDataChangedAction += DataChanged;
        }

        public override void DataChanged()
        {
            base.DataChanged();
            PWBIO.UpdateStroke();
        }
        #endregion

        #region PAINT TOOL
        [SerializeField] private PaintToolSettings _paintTool = new PaintToolSettings();
        public Transform parent { get => _paintTool.parent; set => _paintTool.parent = value; }
        public bool overwritePrefabLayer
        {
            get => _paintTool.overwritePrefabLayer;
            set => _paintTool.overwritePrefabLayer = value;
        }
        public int layer { get => _paintTool.layer; set => _paintTool.layer = value; }
        public bool autoCreateParent { get => _paintTool.autoCreateParent; set => _paintTool.autoCreateParent = value; }
        public bool setSurfaceAsParent { get => _paintTool.setSurfaceAsParent; set => _paintTool.setSurfaceAsParent = value; }
        public bool setLastSelectedAsParent
        {
            get => _paintTool.setLastSelectedAsParent;
            set => _paintTool.setLastSelectedAsParent = value;
        }
        public bool createSubparentPerPalette
        {
            get => _paintTool.createSubparentPerPalette;
            set => _paintTool.createSubparentPerPalette = value;
        }
        public bool createSubparentPerTool
        {
            get => _paintTool.createSubparentPerTool;
            set => _paintTool.createSubparentPerTool = value;
        }
        public bool createSubparentPerBrush
        {
            get => _paintTool.createSubparentPerBrush;
            set => _paintTool.createSubparentPerBrush = value;
        }
        public bool createSubparentPerPrefab
        {
            get => _paintTool.createSubparentPerPrefab;
            set => _paintTool.createSubparentPerPrefab = value;
        }
        public bool overwriteBrushProperties
        {
            get => _paintTool.overwriteBrushProperties;
            set
            {
                _paintTool.overwriteBrushProperties = value;
                OnDataChanged();
            }
        }
        public BrushSettings brushSettings => _paintTool.brushSettings;
        public bool overwriteParentingSettings
        {
            get => _paintTool.overwriteParentingSettings;
            set => _paintTool.overwriteParentingSettings = value;
        }
        public IToolParentingSettings GetParentingSettings() => _paintTool as ToolParentingSettings;
        #endregion

        public override void Copy(IToolSettings other)
        {
            var otherTilingSettings = other as TilingSettings;
            base.Copy(other);
            _paintTool.Copy(otherTilingSettings._paintTool);
            _cellSizeType = otherTilingSettings._cellSizeType;
            _cellSize = otherTilingSettings._cellSize;
            _rotation = otherTilingSettings._rotation;
            _spacing = otherTilingSettings._spacing;
            _axisAlignedWithNormal = otherTilingSettings._axisAlignedWithNormal;

        }

        public TilingSettings Clone()
        {
            var clone = new TilingSettings();
            clone.Copy(this);
            return clone;
        }
    }

    public class TilingToolName : IToolName { public string value => "Tiling"; }

    [System.Serializable]
    public class TilingData : PersistentData<TilingToolName, TilingSettings, ControlPoint>
    {
        [System.NonSerialized]
        private System.Collections.Generic.List<Vector3> _tilingCenters
            = new System.Collections.Generic.List<Vector3>();
        public System.Collections.Generic.List<Vector3> tilingCenters => _tilingCenters;
        public TilingData() : base() { }
        public TilingData((GameObject, int)[] objects, long initialBrushId, TilingData tilingData)
        : base(objects, initialBrushId, tilingData) { }

        private static TilingData _instance = null;
        public static TilingData instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TilingData();
                    _instance._settings = TilingManager.settings;
                }
                return _instance;
            }
        }
        protected override void Initialize()
        {
            base.Initialize();
            const int pointCount = 9;
            for (int i = 0; i < pointCount; i++) _controlPoints.Add(new ControlPoint());
            _pointPositions = new Vector3[pointCount];
        }
        public TilingData Clone()
        {
            var clone = new TilingData();
            base.Clone(clone);
            clone._tilingCenters = _tilingCenters.ToList();
            return clone;
        }

        public Vector3 GetCenter() => GetPoint(8);
    }

    [System.Serializable]
    public class TilingSceneData : SceneData<TilingToolName, TilingSettings, ControlPoint, TilingData>
    {
        public TilingSceneData() : base() { }
        public TilingSceneData(string sceneGUID) : base(sceneGUID) { }
    }

    [System.Serializable]
    public class TilingManager
        : PersistentToolManagerBase<TilingToolName, TilingSettings, ControlPoint, TilingData, TilingSceneData>
    { }
    #endregion

    #region PWBIO
    public static partial class PWBIO
    {
        #region HANDLERS
        private static void TilingInitializeOnLoad()
        {
            TilingManager.settings.OnDataChanged += OnTilingSettingsChanged;
            BrushSettings.OnBrushSettingsChanged += PreviewSelectedPersistentTilings;
        }
        private static void OnUndoTiling() => ClearTilingStroke();
        private static void OnTilingToolModeChanged()
        {
            DeselectPersistentItems(TilingManager.instance);
            if (!ToolManager.editMode)
            {
                ToolProperties.RepainWindow();
                return;
            }
            ResetTilingState();
            ResetSelectedPersistentObject(TilingManager.instance, ref _editingPersistentTiling, _initialPersistentTilingData);
        }
        private static void OnTilingSettingsChanged()
        {
            repaint = true;
            if (!ToolManager.editMode)
            {
                _tilingData.settings = TilingManager.settings;
                updateStroke = true;
                return;
            }
            if (_selectedPersistentTilingData == null) return;
            _selectedPersistentTilingData.settings.Copy(TilingManager.settings);
            PreviewPersistentTiling(_selectedPersistentTilingData);
        }
        #endregion

        #region SPAWN MODE
        public static void InitializeTilingTool()
        {
            TilingManager.instance.DeselectAllItems();
            ResetTilingState(false);
            if (TilingManager.settings.paintOnMeshesWithoutCollider)
            {
                if (TilingManager.settings.ignoreSceneColliders) UpdateSceneColliderSet();
                UpdateOctree();
            }
        }
        public static void ResetTilingState(bool askIfWantToSave = true)
        {
            _initialPersistentTilingData = null;
            _selectedPersistentTilingData = null;
            _editingPersistentTiling = false;
            if (askIfWantToSave)
            {
                void Save()
                {
                    if (UnityEditor.SceneView.lastActiveSceneView != null)
                        TilingStrokePreview(UnityEditor.SceneView.lastActiveSceneView.camera, TilingData.nextHexId, true);
                    CreateTiling();
                }
                AskIfWantToSave(_tilingData.state, Save);
            }
            _snappedToVertex = false;
            _tilingData.Reset();
            _paintStroke.Clear();
        }
        private static void TilingStateNone(bool in2DMode)
        {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                _tilingData.state = ToolManager.ToolState.PREVIEW;
                TilingManager.settings.UpdateCellSize();
            }
            if (MouseDot(out Vector3 point, out Vector3 normal, TilingManager.settings.mode, in2DMode,
                TilingManager.settings.paintOnPalettePrefabs, TilingManager.settings.paintOnMeshesWithoutCollider, false,
                ignoreSceneColliders: TilingManager.settings.ignoreSceneColliders))
            {
                point = SnapToBounds(point);
                point = SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                   TilingManager.settings.paintOnPalettePrefabs, TilingManager.settings.paintOnMeshesWithoutCollider,
                   TilingManager.settings.ignoreSceneColliders, paintOnTheGrid: false,
                   TilingManager.settings.rotation * Vector3.down);
                _tilingData.SetPoint(2, point, registerUndo: false, selectAll: false);
                _tilingData.SetPoint(0, point, registerUndo: false, selectAll: false);
            }
            if (_tilingData.pointsCount > 0) DrawDotHandleCap(_tilingData.GetPoint(0));
        }

        private static void TilingStateRectangle(UnityEditor.SceneView sceneView)
        {
            var settings = TilingManager.settings;
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                UpdateMidpoints(_tilingData);
                _tilingData.state = ToolManager.ToolState.EDIT;
                updateStroke = true;
            }

            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var plane = new Plane(settings.rotation * Vector3.up, _tilingData.GetPoint(0));

            if (plane.Raycast(mouseRay, out float distance))
            {
                var point = mouseRay.GetPoint(distance);
                point = SnapToBounds(point);
                point = SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                   TilingManager.settings.paintOnPalettePrefabs, TilingManager.settings.paintOnMeshesWithoutCollider,
                   TilingManager.settings.ignoreSceneColliders, paintOnTheGrid: false,
                   TilingManager.settings.rotation * Vector3.down);
                _tilingData.SetPoint(2, point, registerUndo: false, selectAll: false);
                var diagonal = point - _tilingData.GetPoint(0);
                var tangent = Vector3.Project(diagonal, settings.rotation * Vector3.right);
                var bitangent = Vector3.Project(diagonal, settings.rotation * Vector3.forward);
                _tilingData.SetPoint(1, _tilingData.GetPoint(0) + tangent, registerUndo: false, selectAll: false);
                _tilingData.SetPoint(3, _tilingData.GetPoint(0) + bitangent, registerUndo: false, selectAll: false);
                DrawTilingGrid(_tilingData);
                TilingInfoText(sceneView);
                for (int i = 0; i < 4; ++i) DrawDotHandleCap(_tilingData.GetPoint(i));
                return;
            }
            DrawDotHandleCap(_tilingData.GetPoint(0));

        }
        private static void TilingInfoText(UnityEditor.SceneView sceneView)
        {
            if (!PWBCore.staticData.showInfoText) return;
            if (_tilingSize == Vector2Int.zero) return;
            var labelTexts = new string[]
            { $"{_tilingSize.x} x {_tilingSize.y}" };
            InfoText.Draw(sceneView, labelTexts);
        }
        private static void TilingStateEdit(Camera camera)
        {
            bool mouseDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;
            TilingShortcuts(_tilingData);
            if (_rotateTiling90)
            {
                var rotation = _tilingData.settings.rotation * Quaternion.AngleAxis(90, _rotateTilingAxis);
                SetTilingRotation(_tilingData, rotation);
                _rotateTiling90 = false;
            }
            var forceStrokeUpdate = updateStroke;
            if (updateStroke)
            {
                BrushstrokeManager.UpdateTilingBrushstroke(_tilingData.tilingCenters.ToArray());
                updateStroke = false;
            }
            if (TilingManager.settings.showPreview) TilingStrokePreview(camera, TilingData.nextHexId, forceStrokeUpdate);

            DrawTilingGrid(_tilingData);
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                if (!TilingManager.settings.showPreview) TilingStrokePreview(camera, TilingData.nextHexId, forceStrokeUpdate);
                CreateTiling();
                ResetTilingState(false);
            }
            DrawTilingControlPoints(_tilingData, out bool clickOnPoint, out bool wasEdited, out Vector3 delta);
        }
        private static void CreateTiling()
        {
            var nextTilingId = TilingData.nextHexId;
            var objDic = Paint(TilingManager.settings, PAINT_CMD, true, false, nextTilingId);
            if (objDic.Count != 1) return;
            var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            var sceneGUID = UnityEditor.AssetDatabase.AssetPathToGUID(scenePath);
            if (isInPrefabMode)
                sceneGUID = UnityEditor.AssetDatabase.AssetPathToGUID(prefabStage.assetPath);
            var initialBrushId = PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.id : -1;
            var objs = objDic[nextTilingId].ToArray();
            var persistentData = new TilingData(objs, initialBrushId, _tilingData);
            TilingManager.instance.AddPersistentItem(sceneGUID, persistentData);
            PWBItemsWindow.RepainWindow();
        }
        private static void TilingStrokePreview(Camera camera, string hexId, bool forceUpdate)
        {
            BrushstrokeItem[] brushstroke;
            if (PreviewIfBrushtrokestaysTheSame(out brushstroke, camera, forceUpdate)) return;
            _paintStroke.Clear();
            var toolSettings = TilingManager.settings;
            float maxSurfaceHeight = 0f;
            for (int i = 0; i < brushstroke.Length; ++i)
            {
                var strokeItem = brushstroke[i];

                var prefab = strokeItem.settings.prefab;
                if (prefab == null) continue;
                Bounds bounds = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation);

                BrushSettings brushSettings = strokeItem.settings;
                if (toolSettings.overwriteBrushProperties) brushSettings = toolSettings.brushSettings;
                if (brushSettings == null) brushSettings = new BrushSettings();

                var additionalRotation = Quaternion.Euler(strokeItem.additionalAngle);
                var scaleMult = brushSettings.GetScaleMultiplier();

                var size = additionalRotation * Vector3.Scale(bounds.size, scaleMult);
                size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
                var pivotToCenter = prefab.transform.InverseTransformDirection(bounds.center - prefab.transform.position);
                pivotToCenter = Vector3.Scale(pivotToCenter, scaleMult);
                pivotToCenter = additionalRotation * pivotToCenter;
                var itemPosition = strokeItem.tangentPosition;

                var height = size.x + size.y + size.z + maxSurfaceHeight
                    + Vector3.Distance(itemPosition, tilingData.GetCenter())
                    + Vector3.Distance(tilingData.GetPoint(0), tilingData.GetPoint(2));

                var normal = toolSettings.rotation * Vector3.up;
                var axisDirection = Vector3.up;
                if (toolSettings.axisAlignedWithNormal == AxesUtils.Axis.Z)
                {
                    size.x = bounds.size.y;
                    size.y = bounds.size.z;
                    size.z = bounds.size.x;
                    axisDirection = Vector3.forward;
                }
                else if (toolSettings.axisAlignedWithNormal == AxesUtils.Axis.X)
                {
                    size.x = bounds.size.z;
                    size.y = bounds.size.x;
                    size.z = bounds.size.y;
                    axisDirection = Vector3.right;
                }

                var ray = new Ray(itemPosition + normal * height, -normal);
                Transform surface = null;
                if (toolSettings.mode != TilingSettings.PaintMode.ON_SHAPE)
                {
                    if (PWBToolRaycast(ray, out RaycastHit itemHit, out GameObject collider, height * 2f, -1,
                        toolSettings.paintOnPalettePrefabs, toolSettings.paintOnMeshesWithoutCollider,
                        createTempColliders: toolSettings.paintOnMeshesWithoutCollider,
                        ignoreSceneColliders: toolSettings.ignoreSceneColliders))

                    {
                        itemPosition = itemHit.point;
                        if (brushSettings.rotateToTheSurface) normal = itemHit.normal;
                        var colObj = PWBCore.GetGameObjectFromTempCollider(collider);
                        if (colObj != null) surface = colObj.transform;
                        var surfObj = PWBCore.GetGameObjectFromTempCollider(collider);
                        var surfSize = BoundsUtils.GetBounds(surfObj.transform).size;
                        var h = surfSize.x + surfSize.y + surfSize.z;
                        maxSurfaceHeight = Mathf.Max(h, maxSurfaceHeight);
                    }
                    else if (toolSettings.mode == TilingSettings.PaintMode.ON_SURFACE) continue;
                }
                var itemRotation = toolSettings.rotation;
                Vector3 itemTangent = itemRotation * Vector3.forward;

                if (brushSettings.rotateToTheSurface
                    && toolSettings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    itemRotation = Quaternion.LookRotation(itemTangent, normal);
                    itemPosition += normal * strokeItem.surfaceDistance;
                }
                else itemPosition += normal * strokeItem.surfaceDistance;
                var axisAlignedWithNormal = (Vector3)toolSettings.axisAlignedWithNormal;

                itemRotation *= Quaternion.FromToRotation(Vector3.up, axisAlignedWithNormal);

                itemRotation *= additionalRotation;

                if (brushSettings.rotateToTheSurface && brushSettings.alwaysOrientUp)
                {
                    var fw = itemRotation * Vector3.forward;
                    const float minMag = 1e-6f;
                    fw.y = 0;
                    if (Mathf.Abs(fw.x) < minMag && Mathf.Abs(fw.z) < minMag) fw = Quaternion.Euler(0, 90, 0) * normal;
                    itemRotation = Quaternion.LookRotation(fw, Vector3.up);
                }

                itemPosition += itemRotation * (brushSettings.localPositionOffset);

                itemPosition -= itemRotation * pivotToCenter;
                if (brushSettings.embedInSurface)
                {
                    if (brushSettings.embedAtPivotHeight)
                        itemPosition += normal * AxesUtils.GetAxisValue(pivotToCenter, toolSettings.axisAlignedWithNormal);
                    else
                        itemPosition += normal * (AxesUtils.GetAxisValue(size, toolSettings.axisAlignedWithNormal) / 2);
                }
                if (brushSettings.embedInSurface
                && toolSettings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (!brushSettings.embedAtPivotHeight)
                    {
                        var TRS = Matrix4x4.TRS(itemPosition, itemRotation,
                            Vector3.Scale(prefab.transform.localScale, scaleMult));
                        var bottomDistanceToSurfce = GetBottomDistanceToSurface(strokeItem.settings.bottomVertices,
                            TRS, Mathf.Abs(strokeItem.settings.bottomMagnitude), toolSettings.paintOnPalettePrefabs,
                            toolSettings.paintOnMeshesWithoutCollider, toolSettings.ignoreSceneColliders,
                            out Transform surfaceTransform);
                        itemPosition += itemRotation * (axisDirection * -bottomDistanceToSurfce);
                    }
                }

                var itemScale = Vector3.Scale(prefab.transform.localScale, scaleMult);
                var layer = toolSettings.overwritePrefabLayer ? toolSettings.layer : prefab.layer;
                Transform parentTransform = toolSettings.parent;

                var paintItem = new PaintStrokeItem(prefab, strokeItem.settings.guid, itemPosition, itemRotation,
                    itemScale, layer, parentTransform, surface, strokeItem.flipX, strokeItem.flipY);
                paintItem.persistentParentId = hexId;

                _paintStroke.Add(paintItem);
                var previewRootToWorld = Matrix4x4.TRS(itemPosition, itemRotation, scaleMult)
                    * Matrix4x4.Rotate(Quaternion.Inverse(prefab.transform.rotation))
                    * Matrix4x4.Translate(-prefab.transform.position);
                PreviewBrushItem(prefab, previewRootToWorld, layer, camera, false, false, strokeItem.flipX, strokeItem.flipY);
                _previewData.Add(new PreviewData(prefab, previewRootToWorld, layer, strokeItem.flipX, strokeItem.flipY));
            }
        }
        #endregion

        #region COMMON
        private static TilingData _tilingData = TilingData.instance;
        private static void ClearTilingStroke()
        {
            _paintStroke.Clear();
            BrushstrokeManager.ClearBrushstroke();
            updateStroke = true;
            if (ToolManager.editMode)
            {
                if (!_editingPersistentLine) return;
                _selectedPersistentTilingData.UpdatePoses();
                PreviewPersistentTiling(_selectedPersistentTilingData);
                UnityEditor.SceneView.RepaintAll();
            }
            else
            {
                UpdateCellCenters(_tilingData, false);
                TilingStrokePreview(UnityEditor.SceneView.lastActiveSceneView.camera, TilingData.nextHexId, true);
            }
        }
        private static void TilingDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                if (_tilingData.state == ToolManager.ToolState.EDIT && _tilingData.selectedPointIdx > 0)
                    _tilingData.selectedPointIdx = -1;
                else if (_tilingData.state == ToolManager.ToolState.NONE) ToolManager.DeselectTool();
                else ResetTilingState(false);
            }
            if (ToolManager.editMode || TilingManager.instance.showPreexistingElements) TilingToolEditMode(sceneView);
            if (ToolManager.editMode) return;
            switch (_tilingData.state)
            {
                case ToolManager.ToolState.NONE:
                    TilingStateNone(sceneView.in2DMode);
                    break;
                case ToolManager.ToolState.PREVIEW:
                    TilingStateRectangle(sceneView);
                    break;
                case ToolManager.ToolState.EDIT:
                    TilingStateEdit(sceneView.camera);
                    break;
            }
        }
        private static void DrawTilingRectangle(TilingData data)
        {
            var settings = data.settings;
            var cornerPoints = new Vector3[] { data.GetPoint(0), data.GetPoint(1),
                data.GetPoint(2), data.GetPoint(3), data.GetPoint(0) };
            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(8, cornerPoints);
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(4, cornerPoints);
        }
        private static void UpdateMidpoints(TilingData data)
        {
            for (int i = 0; i < 4; ++i)
            {
                var nextI = (i + 1) % 4;
                var point = data.GetPoint(i);
                var nextPoint = data.GetPoint(nextI);
                data.SetPoint(i + 4, point + (nextPoint - point) / 2, registerUndo: false, selectAll: false);
            }
            data.SetPoint(8, data.GetPoint(0)
                + (data.GetPoint(2) - data.GetPoint(0)) / 2, registerUndo: false, selectAll: false);
        }
        private static void DrawCells(TilingData data) => UpdateCellCenters(data, true);
        private static void DrawTilingGrid(TilingData data)
        {
            DrawCells(data);
            DrawTilingRectangle(data);
        }
        public static TilingData tilingData => ToolManager.editMode ? _selectedPersistentTilingData : _tilingData;
        private static void ApplyTilingHandlePosition(TilingData data) => SetTilingSelectedPoint(data, _handlePosition);
        private static bool SetTilingSelectedPoint(TilingData data, Vector3 position)
        {
            if (data.selectedPointIdx < 0) return false;
            _handlePosition = position;
            var prevPosition = data.selectedPoint;
            var snappedPoint = SnapToBounds(_handlePosition);
            snappedPoint = SnapAndUpdateGridOrigin(snappedPoint, SnapManager.settings.snappingEnabled,
               data.settings.paintOnPalettePrefabs, data.settings.paintOnMeshesWithoutCollider,
               data.settings.ignoreSceneColliders, paintOnTheGrid: false, Vector3.down);
            data.SetPoint(data.selectedPointIdx, snappedPoint, registerUndo: true, selectAll: false);
            _handlePosition = data.selectedPoint;
            if (prevPosition == data.selectedPoint) return false;

            updateStroke = true;
            var delta = data.selectedPoint - prevPosition;
            if (data.selectedPointIdx < 4)
            {
                var nextCornerIdx = (data.selectedPointIdx + 1) % 4;
                var oppositeCornerIdx = (data.selectedPointIdx + 2) % 4;
                var prevCornerIdx = (data.selectedPointIdx + 3) % 4;

                var nextVector = data.GetPoint(nextCornerIdx) - prevPosition;
                var prevVector = data.GetPoint(prevCornerIdx) - prevPosition;
                var deltaNext = Vector3.Project(delta, nextVector);
                var deltaPrev = Vector3.Project(delta, prevVector);
                var deltaNormal = delta - deltaNext - deltaPrev;
                data.AddValue(nextCornerIdx, deltaPrev + deltaNormal);
                data.AddValue(prevCornerIdx, deltaNext + deltaNormal);
                data.AddValue(oppositeCornerIdx, deltaNormal);
            }
            else if (data.selectedPointIdx < 8)
            {
                var prevCornerIdx = data.selectedPointIdx - 4;
                var nextCornerIdx = (data.selectedPointIdx - 3) % 4;
                var oppositeSideIdx = (data.selectedPointIdx - 2) % 4 + 4;
                var parallel = data.GetPoint(nextCornerIdx) - data.GetPoint(prevCornerIdx);
                var perpendicular = data.GetPoint(oppositeSideIdx) - prevPosition;
                var deltaParallel = Vector3.Project(delta, parallel);
                var deltaPerpendicular = Vector3.Project(delta, perpendicular);
                var deltaNormal = delta - deltaParallel - deltaPerpendicular;
                for (int i = 0; i < 4; ++i) data.AddValue(i, deltaParallel + deltaNormal);
                data.AddValue(prevCornerIdx, deltaPerpendicular);
                data.AddValue(nextCornerIdx, deltaPerpendicular);
            }
            else for (int i = 0; i < 4; ++i) data.AddValue(i, delta);
            UpdateMidpoints(data);
            UpdateCellCenters(data, false);
            return true;
        }
        private static bool SetTilingRotation(TilingData data, Quaternion rotation)
        {
            var prevRotation = data.settings.rotation;
            data.settings.rotation = rotation;
            if (data.settings.rotation == prevRotation) return false;

            ToolProperties.RegisterUndo(TilingData.COMMAND_NAME);
            updateStroke = true;
            var delta = rotation * Quaternion.Inverse(prevRotation);
            for (int i = 0; i < 8; ++i)
            {
                var centerToPoint = data.GetPoint(i) - data.GetPoint(8);
                var rotatedPos = (delta * centerToPoint) + data.GetPoint(8);
                data.SetPoint(i, rotatedPos, registerUndo: false, selectAll: false);
            }
            DrawCells(data);
            ToolProperties.RepainWindow();
            UpdateCellCenters(data, false);
            return true;
        }
        public static void UpdateCellSize()
        {
            if (ToolManager.editMode)
            {
                if (_selectedPersistentTilingData == null) return;
                _selectedPersistentTilingData.settings.UpdateCellSize();
                UpdateCellCenters(_selectedPersistentTilingData, true);
            }
            _tilingData.settings.UpdateCellSize();
            UpdateCellCenters(_tilingData, true);
        }
        private static Vector2Int _tilingSize = Vector2Int.zero;
        private static void UpdateCellCenters(TilingData data, bool DrawCells)
        {
            if (!ToolManager.editMode && data.state == ToolManager.ToolState.NONE) return;
            data.tilingCenters.Clear();
            var settings = data.settings;
            var tangentDir = data.GetPoint(1) - data.GetPoint(0);
            var tangentSize = tangentDir.magnitude;
            tangentDir.Normalize();
            var bitangentDir = data.GetPoint(3) - data.GetPoint(0);
            var bitangentSize = bitangentDir.magnitude;
            bitangentDir.Normalize();
            var cellTangent = tangentDir * Mathf.Abs(settings.cellSize.x);
            var cellBitangent = bitangentDir * Mathf.Abs(settings.cellSize.y);
            var vertices = new Vector3[] { Vector3.zero, cellTangent, cellTangent + cellBitangent, cellBitangent };
            var offset = data.GetPoint(0);
            void SetTileCenter()
            {
                var linePoints = new Vector3[5];
                for (int i = 0; i <= 4; ++i) linePoints[i] = vertices[i % 4] + offset;
                var cellCenter = linePoints[0] + (linePoints[2] - linePoints[0]) / 2;
                data.tilingCenters.Add(cellCenter);
                if (!DrawCells) return;
                UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.3f);

                UnityEditor.Handles.DrawAAPolyLine(6, linePoints);
                UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.3f);
                UnityEditor.Handles.DrawAAPolyLine(2, linePoints);

            }
            var minCellSize = settings.cellSize + settings.spacing;
            minCellSize = Vector2.Max(minCellSize, Vector2.one * 0.001f);
            var cellSize = minCellSize - settings.spacing;
            float tangentOffset = 0;
            _tilingSize = Vector2Int.zero;
            while (Mathf.Abs(tangentOffset) + Mathf.Abs(cellSize.x) <= tangentSize)
            {
                float bitangentOffset = 0;
                ++_tilingSize.x;
                var sizeY = 0;
                while (Mathf.Abs(bitangentOffset) + Mathf.Abs(cellSize.y) <= bitangentSize)
                {
                    SetTileCenter();
                    bitangentOffset += minCellSize.y;
                    offset = data.GetPoint(0) + tangentDir * Mathf.Abs(tangentOffset)
                        + bitangentDir * Mathf.Abs(bitangentOffset);
                    ++sizeY;
                }
                _tilingSize.y = Mathf.Max(_tilingSize.y, sizeY);
                tangentOffset += minCellSize.x;
                offset = data.GetPoint(0) + tangentDir * Mathf.Abs(tangentOffset);
            }
        }

        private static Vector3 _rotateTilingAxis = Vector3.zero;

        private static bool _rotateTiling90 = false;
        public static void ShowTilingContextMenu(TilingData data, Vector2 mousePosition)
        {
            if (!ToolManager.editMode) return;
            void Rotate90(Vector3 axis)
            {
                if (ToolManager.editMode) SelectTiling(data);
                _rotateTiling90 = true;
                _rotateTilingAxis = axis;
            }
            var menu = new UnityEditor.GenericMenu();
            menu.AddItem(new GUIContent("Rotate 90º around Y ... "
                + PWBSettings.shortcuts.selectionRotate90YCW.combination.ToString()), on: false,
                () => Rotate90(Vector3.down));
            menu.AddItem(new GUIContent("Rotate -90º around Y ... "
                + PWBSettings.shortcuts.selectionRotate90YCCW.combination.ToString()), on: false,
                () => Rotate90(Vector3.up));
            menu.AddItem(new GUIContent("Rotate 90º around X ... "
                + PWBSettings.shortcuts.selectionRotate90XCW.combination.ToString()), on: false,
                () => Rotate90(Vector3.left));
            menu.AddItem(new GUIContent("Rotate -90º around X ... "
                + PWBSettings.shortcuts.selectionRotate90XCCW.combination.ToString()), on: false,
                () => Rotate90(Vector3.right));
            menu.AddItem(new GUIContent("Rotate 90º around Z ... "
                + PWBSettings.shortcuts.selectionRotate90ZCW.combination.ToString()), on: false,
                () => Rotate90(Vector3.back));
            menu.AddItem(new GUIContent("Rotate -90º around Z ... "
                + PWBSettings.shortcuts.selectionRotate90ZCCW.combination.ToString()), on: false,
                () => Rotate90(Vector3.forward));
            menu.AddSeparator(string.Empty);
            PersistentItemContextMenu(menu, data, mousePosition);
            menu.ShowAsContext();
        }
        private static bool DrawTilingControlPoints(TilingData data,
            out bool clickOnPoint, out bool wasEdited, out Vector3 delta)
        {
            delta = Vector3.zero;
            clickOnPoint = false;
            wasEdited = false;

            for (int i = 0; i < 9; ++i)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                if (!clickOnPoint)
                {
                    float distFromMouse
                        = UnityEditor.HandleUtility.DistanceToRectangle(data.GetPoint(i), Quaternion.identity, 0f);
                    UnityEditor.HandleUtility.AddControl(controlId, distFromMouse);
                    if (Event.current.button == 0 && Event.current.type == EventType.MouseDown
                        && UnityEditor.HandleUtility.nearestControl == controlId)
                    {
                        data.selectedPointIdx = i;
                        clickOnPoint = true;
                        Event.current.Use();
                    }
                    if (Event.current.button == 1 && Event.current.type == EventType.MouseDown
                      && !Event.current.control && !Event.current.shift && !Event.current.alt
                          && UnityEditor.HandleUtility.nearestControl == controlId)
                    {
                        ShowTilingContextMenu(data,
                            UnityEditor.EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition));
                        Event.current.Use();
                    }
                }
                if (Event.current.type != EventType.Repaint) continue;
                DrawDotHandleCap(data.GetPoint(i));
            }
            if (clickOnPoint) ToolProperties.RepainWindow();
            if (data.selectedPointIdx < 0) return false;
            var prevPoint = data.selectedPoint;
            wasEdited = SetTilingSelectedPoint(data,
                UnityEditor.Handles.PositionHandle(data.selectedPoint, data.settings.rotation));
            if (prevPoint != data.selectedPoint) ToolProperties.RepainWindow();
            if (data.selectedPointIdx == 8)
            {
                var prevRotation = data.settings.rotation;
                wasEdited = wasEdited || SetTilingRotation(data,
                    UnityEditor.Handles.RotationHandle(data.settings.rotation, data.GetPoint(8)));
                if (prevRotation != data.settings.rotation) ToolProperties.RepainWindow();
            }
            return clickOnPoint || wasEdited;
        }
        private static bool TilingShortcuts(TilingData data)
        {
            if (data == null) return false;
            var keyCode = Event.current.keyCode;

            var spacing1 = PWBSettings.shortcuts.tilingEditSpacing1.Check();
            var spacing2 = PWBSettings.shortcuts.tilingEditSpacing2.Check();
            if (spacing1 || spacing2)
            {
                var delta = spacing1 ? PWBSettings.shortcuts.tilingEditSpacing1.combination.delta
                    : -PWBSettings.shortcuts.tilingEditSpacing2.combination.delta;
                var deltaSign = -Mathf.Sign(delta);
                var otherAxes = AxesUtils.GetOtherAxes(AxesUtils.Axis.Y);
                var spacing = Vector3.zero;
                AxesUtils.SetAxisValue(ref spacing, otherAxes[0], data.settings.spacing.x);
                AxesUtils.SetAxisValue(ref spacing, otherAxes[1], data.settings.spacing.y);
                var axisIdx = spacing1 ? 1 : 0;
                var size = data.GetPoint(2) - data.GetPoint(axisIdx);
                var axisSize = AxesUtils.GetAxisValue(size, otherAxes[axisIdx]);
                AxesUtils.AddValueToAxis(ref spacing, otherAxes[axisIdx], axisSize * deltaSign * 0.005f);
                data.settings.spacing = new Vector2(AxesUtils.GetAxisValue(spacing, otherAxes[0]),
                    AxesUtils.GetAxisValue(spacing, otherAxes[1]));
                ToolProperties.RepainWindow();
                Event.current.Use();
                return true;
            }
            void Rotate90(Vector3 axis)
            {
                _rotateTiling90 = true;
                _rotateTilingAxis = axis;
            }
            if (PWBSettings.shortcuts.selectionRotate90XCCW.Check())
            {
                Rotate90(Vector3.right);
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90XCW.Check())
            {
                Rotate90(Vector3.left);
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90YCCW.Check())
            {
                Rotate90(Vector3.up);
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90YCW.Check())
            {
                Rotate90(Vector3.down);
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90ZCCW.Check())
            {
                Rotate90(Vector3.forward);
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90ZCW.Check())
            {
                Rotate90(Vector3.back);
                return true;
            }
            return false;
        }
        public static void UpdateTilingRotation(Quaternion rotation)
        {
            if (tilingData == null) return;
            updateStroke = true;
            SetTilingRotation(tilingData, rotation);
        }
        #endregion

        #region EDIT MODE
        private static TilingData _initialPersistentTilingData = null;
        private static TilingData _selectedPersistentTilingData = null;
        private static bool _editingPersistentTiling = false;
        public static TilingData selectedPersistentTilingData
        {
            get
            {
                if (!_editingPersistentTiling) return null;
                return _selectedPersistentTilingData;
            }
        }

        public static void SelectTiling(TilingData data)
        {
            ApplySelectedPersistentTiling(true);
            _editingPersistentTiling = true;
            data.ClearSelection();
            data.selectedPointIdx = 8;
            data.isSelected = true;
            _selectedPersistentTilingData = data;
            if (_initialPersistentTilingData == null) _initialPersistentTilingData = data.Clone();
            TilingManager.instance.CopyToolSettings(data.settings);
        }

        private static void TilingToolEditMode(UnityEditor.SceneView sceneView)
        {
            var persistentItems = TilingManager.instance.GetPersistentItems();
            var deselectedItems = new System.Collections.Generic.List<TilingData>(persistentItems);
            bool clickOnAnyPoint = false;
            bool selectedItemWasEdited = false;
            foreach (var itemData in persistentItems)
            {
                DrawCells(itemData);
                if (!ToolManager.editMode) continue;
                DrawTilingRectangle(itemData);

                var selectedTilingId = _initialPersistentTilingData == null ? -1 : _initialPersistentTilingData.id;
                if (DrawTilingControlPoints(itemData, out bool clickOnPoint, out bool wasEdited, out Vector3 delta))
                {
                    if (clickOnPoint)
                    {
                        clickOnAnyPoint = true;
                        _editingPersistentTiling = true;
                        if (selectedTilingId != itemData.id)
                        {
                            ApplySelectedPersistentTiling(false);
                            if (selectedTilingId == -1)
                                _createProfileName = TilingManager.instance.selectedProfileName;
                            TilingManager.instance.CopyToolSettings(itemData.settings);
                            itemData.isSelected = true;
                            _selectedPersistentTilingData = itemData;

                            _editingPersistentTiling = true;
                            UpdateCellSize();
                        }
                        if (_initialPersistentTilingData == null) _initialPersistentTilingData = itemData.Clone();
                        else if (_initialPersistentTilingData.id != itemData.id)
                            _initialPersistentTilingData = itemData.Clone();
                        deselectedItems.Remove(itemData);
                    }
                    if (wasEdited)
                    {
                        _editingPersistentTiling = true;
                        selectedItemWasEdited = true;
                        _persistentItemWasEdited = true;
                    }
                }
            }
            if (clickOnAnyPoint)
                foreach (var itemData in deselectedItems) itemData.ClearSelection();
            if (!ToolManager.editMode) return;
            bool skipPreview = _selectedPersistentTilingData != null
                && _selectedPersistentTilingData.objectCount > PWBCore.staticData.maxPreviewCountInEditMode;
            if (!skipPreview)
            {
                if (selectedItemWasEdited) PreviewPersistentTiling(_selectedPersistentTilingData);
                else if (_editingPersistentTiling && _selectedPersistentTilingData != null)
                {
                    var forceStrokeUpdate = updateStroke;
                    if (updateStroke)
                    {
                        PreviewPersistentTiling(_selectedPersistentTilingData);
                        updateStroke = false;
                        PWBCore.SetSavePending();
                    }
                    if (_brushstroke != null
                        && !BrushstrokeManager.BrushstrokeEqual(BrushstrokeManager.brushstroke, _brushstroke))
                        _paintStroke.Clear();
                    TilingStrokePreview(sceneView.camera, _selectedPersistentTilingData.hexId, forceStrokeUpdate);
                }
            }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                if (skipPreview)
                {
                    PreviewPersistentTiling(_selectedPersistentTilingData);
                    TilingStrokePreview(sceneView.camera, _selectedPersistentTilingData.hexId, forceUpdate: true);
                }
                _persistentItemWasEdited = true;
                ApplySelectedPersistentTiling(deselectPoint: true);

                ToolProperties.RepainWindow();
            }
            else if (PWBSettings.shortcuts.editModeSelectParent.Check()
                && _selectedPersistentTilingData != null)
            {
                var parent = _selectedPersistentTilingData.GetParent();
                if (parent != null) UnityEditor.Selection.activeGameObject = parent;
            }
            else if (PWBSettings.shortcuts.editModeDeleteItemButNotItsChildren.Check())
                TilingManager.instance.DeletePersistentItem(_selectedPersistentTilingData.id, false);
            else if (PWBSettings.shortcuts.editModeDeleteItemAndItsChildren.Check())
                TilingManager.instance.DeletePersistentItem(_selectedPersistentTilingData.id, true);
            else if (PWBSettings.shortcuts.editModeDuplicate.Check()) DuplicateItem(_selectedPersistentTilingData.id);
            if (TilingShortcuts(_selectedPersistentTilingData))
            {
                DrawCells(_selectedPersistentTilingData);
                PreviewPersistentTiling(_selectedPersistentTilingData);
                repaint = true;
            }
            if (_rotateTiling90)
            {
                var rotation = _selectedPersistentTilingData.settings.rotation * Quaternion.AngleAxis(90, _rotateTilingAxis);
                SetTilingRotation(_selectedPersistentTilingData, rotation);
                PreviewPersistentTiling(_selectedPersistentTilingData);
                repaint = true;
                _rotateTiling90 = false;
            }
        }

        public static void PreviewSelectedPersistentTilings()
        {
            if (ToolManager.tool != ToolManager.PaintTool.TILING) return;
            var persistentTilings = TilingManager.instance.GetPersistentItems();
            foreach (var tilingData in persistentTilings)
            {
                if (!tilingData.isSelected) continue;
                PreviewPersistentTiling(tilingData);
            }
        }

        private static void PreviewPersistentTiling(TilingData data)
        {
            Vector3[] objPos = null;
            var objList = data.objectList;
            var toolSettings = data.settings;
            BrushstrokeManager.UpdatePersistentTilingBrushstroke(data.tilingCenters.ToArray(),
                toolSettings, objList, out objPos, out Vector3[] strokePos);
            _disabledObjects.Clear();
            _disabledObjects.UnionWith(objList);
            var objSet = data.objectSet;
            float maxSurfaceHeight = 0f;
            for (int objIdx = 0; objIdx < objPos.Length; ++objIdx)
            {
                var obj = objList[objIdx];
                if (obj == null)
                {
                    data.RemovePose(objIdx);
                    continue;
                }
                obj.SetActive(true);
                var itemPosition = objPos[objIdx];

                BrushSettings brushSettings = TilingManager.instance.applyBrushToExisting
                    ? (toolSettings.overwriteBrushProperties ? toolSettings.brushSettings : PaletteManager.selectedBrush)
                    : PaletteManager.GetBrushById(data.initialBrushId);
                if (brushSettings == null) brushSettings = new BrushSettings();

                var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);

                var bounds = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation, ignoreDissabled: true,
                    BoundsUtils.ObjectProperty.BOUNDING_BOX, recursive: true, useDictionary: true);

                var scaleMult = brushSettings.GetScaleMultiplier();
                var size = Vector3.Scale(bounds.size, scaleMult);

                var height = size.x + size.y + size.z + maxSurfaceHeight
                    + Vector3.Distance(itemPosition, data.GetCenter()) + Vector3.Distance(data.GetPoint(0), data.GetPoint(2));
                var normal = toolSettings.rotation * Vector3.up;

                var ray = new Ray(itemPosition + normal * height, -normal);
                if (toolSettings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (PWBToolRaycast(ray, out RaycastHit itemHit, out GameObject collider, height * 2f, -1,
                        toolSettings.paintOnPalettePrefabs, toolSettings.paintOnMeshesWithoutCollider,
                        tags: null, terrainLayers: null, exceptions: objSet,
                        createTempColliders: toolSettings.paintOnMeshesWithoutCollider,
                        ignoreSceneColliders: toolSettings.ignoreSceneColliders))
                    {
                        itemPosition = itemHit.point;
                        if (brushSettings.rotateToTheSurface) normal = itemHit.normal;
                        var surfObj = PWBCore.GetGameObjectFromTempCollider(collider);
                        var surfSize = BoundsUtils.GetBounds(surfObj.transform).size;
                        var h = surfSize.x + surfSize.y + surfSize.z;
                        maxSurfaceHeight = Mathf.Max(h, maxSurfaceHeight);
                    }
                    else if (toolSettings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE) continue;
                }
                var itemRotation = toolSettings.rotation;
                Vector3 itemTangent = itemRotation * Vector3.forward;

                if (brushSettings.rotateToTheSurface
                    && toolSettings.mode != PaintOnSurfaceToolSettings.PaintMode.ON_SHAPE)
                {
                    itemRotation = Quaternion.LookRotation(itemTangent, normal);
                    itemPosition += normal * brushSettings.surfaceDistance;
                }
                else itemPosition += normal * brushSettings.surfaceDistance;
                var axisAlignedWithNormal = (Vector3)toolSettings.axisAlignedWithNormal;

                itemRotation *= Quaternion.FromToRotation(Vector3.up, axisAlignedWithNormal);

                if (brushSettings.rotateToTheSurface && brushSettings.alwaysOrientUp)
                {
                    var fw = itemRotation * Vector3.forward;
                    const float minMag = 1e-6f;
                    fw.y = 0;
                    if (Mathf.Abs(fw.x) < minMag && Mathf.Abs(fw.z) < minMag) fw = Quaternion.Euler(0, 90, 0) * normal;
                    itemRotation = Quaternion.LookRotation(fw, Vector3.up);
                }

                itemPosition += itemRotation * brushSettings.localPositionOffset;

                UnityEditor.Undo.RecordObject(obj.transform, TilingData.COMMAND_NAME);
                obj.transform.rotation = Quaternion.identity;
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = itemRotation;


                var pivotToCenter = prefab.transform.InverseTransformDirection(bounds.center - prefab.transform.position);
                pivotToCenter = Vector3.Scale(pivotToCenter, scaleMult);
                pivotToCenter = itemRotation * pivotToCenter;


                itemPosition -= pivotToCenter;
                if (brushSettings.embedInSurface)
                {
                    if (brushSettings.embedAtPivotHeight)
                        itemPosition += normal * AxesUtils.GetAxisValue(pivotToCenter, toolSettings.axisAlignedWithNormal);
                    else
                        itemPosition += normal * (AxesUtils.GetAxisValue(size, toolSettings.axisAlignedWithNormal) / 2);
                }

                var axisDirection = Vector3.up;
                if (toolSettings.axisAlignedWithNormal == AxesUtils.Axis.Z)
                {
                    size.x = bounds.size.y;
                    size.y = bounds.size.z;
                    size.z = bounds.size.x;
                    axisDirection = Vector3.forward;
                }
                else if (toolSettings.axisAlignedWithNormal == AxesUtils.Axis.X)
                {
                    size.x = bounds.size.z;
                    size.y = bounds.size.x;
                    size.z = bounds.size.y;
                    axisDirection = Vector3.right;
                }

                if (brushSettings.embedInSurface
                    && toolSettings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    var bottomMagnitude = BoundsUtils.GetBottomMagnitude(obj.transform);
                    if (brushSettings.embedAtPivotHeight)
                        itemPosition += itemRotation * (axisDirection * bottomMagnitude);
                    else
                    {
                        var TRS = Matrix4x4.TRS(itemPosition, itemRotation, obj.transform.lossyScale);
                        var bottomVertices = BoundsUtils.GetBottomVertices(obj.transform);
                        var bottomDistanceToSurfce = GetBottomDistanceToSurface(bottomVertices, TRS,
                            Mathf.Abs(bottomMagnitude), toolSettings.paintOnPalettePrefabs,
                            toolSettings.paintOnMeshesWithoutCollider, toolSettings.ignoreSceneColliders,
                            out Transform surfaceTransform);
                        itemPosition += itemRotation * (axisDirection * -bottomDistanceToSurfce);
                    }
                }
                obj.transform.position = itemPosition;
                
                if (TilingManager.instance.applyBrushToExisting)
                {
                    brushSettings = TilingManager.instance.applyBrushToExisting
                    ? (toolSettings.overwriteBrushProperties ? toolSettings.brushSettings : PaletteManager.selectedBrush)
                    : PaletteManager.GetBrushById(data.initialBrushId);
                    if (brushSettings == null) brushSettings = new BrushSettings();

                    obj.transform.localScale = Vector3.Scale(prefab.transform.localScale, scaleMult);
                    obj.transform.localRotation *= Quaternion.Euler(brushSettings.GetAdditionalAngle());
                    obj.transform.position += itemRotation * (axisDirection * brushSettings.GetSurfaceDistance());
                    var flipX = brushSettings.GetFlipX();
                    var flipY = brushSettings.GetFlipY();
                    if (flipX || flipY)
                    {
                        var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();
                        foreach (var spriteRenderer in spriteRenderers)
                        {
                            UnityEditor.Undo.RecordObject(spriteRenderer, TilingData.COMMAND_NAME);
                            spriteRenderer.flipX = flipX;
                            spriteRenderer.flipY = flipY;
                        }
                    }
                }
                _disabledObjects.Remove(obj);
            }
            foreach (var obj in _disabledObjects) if (obj != null) obj.SetActive(false);
        }
        private static void ApplySelectedPersistentTiling(bool deselectPoint)
        {
            if (!_persistentItemWasEdited) return;
            _persistentItemWasEdited = false;
            if (!ApplySelectedPersistentObject(deselectPoint, ref _editingPersistentTiling, ref _initialPersistentTilingData,
                ref _selectedPersistentTilingData, TilingManager.instance)) return;
            if (_initialPersistentTilingData == null) return;
            var selectedTiling = TilingManager.instance.GetItem(_initialPersistentTilingData.id);
            _initialPersistentTilingData = selectedTiling.Clone();

        }
        #endregion
    }
    #endregion
}
