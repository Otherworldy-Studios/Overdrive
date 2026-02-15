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
using UnityEngine;
using System.Linq;

namespace PluginMaster
{
    #region DATA & SETTINGS
    [System.Serializable]
    public struct GridOrigin
    {
        [SerializeField] private string _name;
        [SerializeField] private Pose _pose;
        public GridOrigin(string name, Pose point)
        {
            _name = name;
            _pose = point;
        }
        public string name { get => _name; set => _name = value; }
        public Vector3 position { get => _pose.position; set => _pose.position = value; }
        public Quaternion rotation { get => _pose.rotation; set => _pose.rotation = value; }
        public Pose pose { get => _pose; set => _pose = value; }
    }
    [System.Serializable]
    public class SnapSettings : ISerializationCallbackReceiver
    {
        [System.Serializable]
        private struct Bool3
        {
            public bool x, y, z;
            public Bool3(bool x = true, bool y = false, bool z = true) => (this.x, this.y, this.z) = (x, y, z);
        }
        [SerializeField] private bool _snappingEnabled = false;
        [SerializeField] private Bool3 _snappingOn = new Bool3();
        [SerializeField] private bool _visibleGrid = false;
        [SerializeField] private Bool3 _gridOn = new Bool3(false, true, false);
        [SerializeField] private bool _lockedGrid = false;
        [SerializeField] private bool _boundsSnapping = false;
        [SerializeField] private Vector3 _step = Vector3.one;
        [SerializeField] private Vector3 _origin = Vector3.zero;
        [SerializeField] private Quaternion _rotation = Quaternion.identity;
        [SerializeField] private bool _showPositionHandle = true;
        [SerializeField] private bool _showRotationHandle = false;
        [SerializeField] private bool _showScaleHandle = false;
        [SerializeField] private bool _radialGridEnabled = false;
        [SerializeField] private float _radialStep = 1f;
        [SerializeField] private int _radialSectors = 8;
        [SerializeField] private bool _snapToRadius = true;
        [SerializeField] private bool _snapToCircunference = true;
        [SerializeField] private Vector3Int _majorLinesGap = Vector3Int.one * 10;
        [SerializeField] private bool _midpointSnapping = false;
        [SerializeField] private GridOrigin[] _origins = null;
        [SerializeField] private bool _drawGridAsTexture = true;
        private const string DEFAULT_ORIGIN_NAME = "Default";
        [SerializeField] private string _selectedOrigin = DEFAULT_ORIGIN_NAME;
        private System.Collections.Generic.Dictionary<string, Pose> _originsDictionary
            = new System.Collections.Generic.Dictionary<string, Pose>() { { DEFAULT_ORIGIN_NAME, Pose.identity } };

        public System.Action OnGridOriginChange;
        public System.Action OnDataChanged;
        public System.Action OnGridOrientationChange;
        public void DataChanged(bool repaint = true)
        {
            if (!repaint)
            {
                PWBCore.staticData.SetSavePending();
                return;
            }
            PWBCore.SetSavePending();
            if (OnDataChanged != null) OnDataChanged();
            UnityEditor.SceneView.RepaintAll();
        }
        public Vector3 step
        {
            get => _step;
            set
            {
                value = Vector3.Max(value, Vector3.one * 0.1f);
                if (_step == value) return;
                _step = value;
                DataChanged(false);
            }
        }

        public bool snappingEnabled
        {
            get => _snappingEnabled;
            set
            {
                if (_snappingEnabled == value) return;
                _snappingEnabled = value;
                if (_snappingEnabled) visibleGrid = true;
                DataChanged();
            }
        }
        public bool snappingOnX
        {
            get => _snappingOn.x;
            set
            {
                if (_snappingOn.x == value) return;
                _snappingOn.x = value;
                DataChanged();
            }
        }
        public bool snappingOnY
        {
            get => _snappingOn.y;
            set
            {
                if (_snappingOn.y == value) return;
                _snappingOn.y = value;
                DataChanged();
            }
        }
        public bool snappingOnZ
        {
            get => _snappingOn.z;
            set
            {
                if (_snappingOn.z == value) return;
                _snappingOn.z = value;
                DataChanged();
            }
        }

        public Vector3 origin
        {
            get => _origin;
            set
            {
                if (_origin == value) return;
                _origin = value;
                DataChanged(false);
                if (OnGridOriginChange != null) OnGridOriginChange();
            }
        }
        public bool lockedGrid
        {
            get => _lockedGrid;
            set
            {
                if (_lockedGrid == value) return;
                _lockedGrid = value;
                DataChanged();
            }
        }
        public bool visibleGrid
        {
            get => _visibleGrid;
            set
            {
                if (_visibleGrid == value) return;
                _visibleGrid = value;
                DataChanged();
            }
        }
        public bool gridOnX
        {
            get => _gridOn.x;
            set
            {
                if (_gridOn.x == value) return;
                _gridOn.x = value;
                if (value)
                {
                    _gridOn.y = _gridOn.z = false;
                    _snappingOn.x = false;
                    _snappingOn.y = _snappingOn.z = true;
                    if (OnGridOrientationChange != null) OnGridOrientationChange();
                }
                DataChanged();
            }
        }
        public bool gridOnY
        {
            get => _gridOn.y;
            set
            {
                if (_gridOn.y == value) return;
                _gridOn.y = value;
                if (value)
                {
                    _gridOn.x = _gridOn.z = false;
                    _snappingOn.y = false;
                    _snappingOn.x = _snappingOn.z = true;
                    if (OnGridOrientationChange != null) OnGridOrientationChange();
                }
                DataChanged();
            }
        }
        public bool gridOnZ
        {
            get => _gridOn.z;
            set
            {
                if (_gridOn.z == value) return;
                _gridOn.z = value;
                if (value)
                {
                    _gridOn.x = _gridOn.y = false;
                    _snappingOn.z = false;
                    _snappingOn.y = _snappingOn.x = true;
                    if (OnGridOrientationChange != null) OnGridOrientationChange();
                }
                DataChanged();
            }
        }

        public bool boundsSnapping
        {
            get => _boundsSnapping;
            set
            {
                if (_boundsSnapping == value) return;
                _boundsSnapping = value;
                DataChanged();
            }
        }

        public AxesUtils.Axis gridAxis => gridOnX ? AxesUtils.Axis.X : (gridOnY ? AxesUtils.Axis.Y : AxesUtils.Axis.Z);
        public Quaternion rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value) return;
                _rotation = value;
                DataChanged(false);
                if (OnGridOriginChange != null) OnGridOriginChange();
            }
        }
        public bool showPositionHandle
        {
            get => _showPositionHandle;
            set
            {
                if (_showPositionHandle == value) return;
                _showPositionHandle = value;
                if (_showPositionHandle)
                {
                    _showRotationHandle = false;
                    _showScaleHandle = false;
                }
                SnapManager.FrameGridOrigin();
                DataChanged();
            }
        }
        public bool showRotationHandle
        {
            get => _showRotationHandle;
            set
            {
                if (_showRotationHandle == value) return;
                _showRotationHandle = value;
                if (_showRotationHandle)
                {
                    _showPositionHandle = false;
                    _showScaleHandle = false;
                    SnapManager.FrameGridOrigin();
                }
                DataChanged();
            }
        }
        public bool showScaleHandle
        {
            get => _showScaleHandle;
            set
            {
                if (_showScaleHandle == value) return;
                _showScaleHandle = value;
                if (_showScaleHandle)
                {
                    _showPositionHandle = false;
                    _showRotationHandle = false;
                    SnapManager.FrameGridOrigin();
                }
                DataChanged();
            }
        }
        public bool radialGridEnabled
        {
            get => _radialGridEnabled;
            set
            {
                if (_radialGridEnabled == value) return;
                _radialGridEnabled = value;
                DataChanged();
            }
        }
        public float radialStep
        {
            get => _radialStep;
            set
            {
                value = Mathf.Max(value, 0.1f);
                if (_radialStep == value) return;
                _radialStep = value;
                DataChanged();
            }
        }
        public int radialSectors
        {
            get => _radialSectors;
            set
            {
                value = Mathf.Max(value, 3);
                if (_radialSectors == value) return;
                _radialSectors = value;
                DataChanged();
            }
        }
        public bool snapToRadius
        {
            get => _snapToRadius;
            set
            {
                if (_snapToRadius == value) return;
                _snapToRadius = value;
                DataChanged();
            }
        }
        public bool snapToCircunference
        {
            get => _snapToCircunference;
            set
            {
                if (_snapToCircunference == value) return;
                _snapToCircunference = value;
            }
        }

        public Vector3Int majorLinesGap
        {
            get => _majorLinesGap;
            set
            {
                value = Vector3Int.Max(value, Vector3Int.one);
                if (_majorLinesGap == value) return;
                _majorLinesGap = value;
                DataChanged();
            }
        }

        public bool midpointSnapping
        {
            get => _midpointSnapping;
            set
            {
                if (_midpointSnapping == value) return;
                _midpointSnapping = value;
                DataChanged();
            }
        }
        #region ORIGINS
        public string selectedOrigin
        {
            get => _selectedOrigin;
            set
            {
                if (_selectedOrigin == value) return;
                _selectedOrigin = value;
                _origin = _originsDictionary[_selectedOrigin].position;
                _rotation = _originsDictionary[_selectedOrigin].rotation;
                DataChanged();
                if (OnGridOriginChange != null) OnGridOriginChange();
            }
        }

        public bool drawGridAsTexture
        {
            get => _drawGridAsTexture;
            set
            {
                if (_drawGridAsTexture == value) return;
                _drawGridAsTexture = value;
                DataChanged();
            }
        }
        public void SaveGridOrigin(string name)
        {
            if (_originsDictionary.ContainsKey(name)) _originsDictionary[name] = new Pose(origin, rotation);
            else _originsDictionary.Add(name, new Pose(origin, rotation));
            _selectedOrigin = name;
            DataChanged();
        }
        public bool OriginsDictionaryContains(string name) => _originsDictionary.ContainsKey(name);
        public Pose GetOrigin(string name) => _originsDictionary[name];
        public string[] GetOriginNames() => _originsDictionary.Keys.ToArray();
        public void DeleteSelectedOrigin()
        {
            _originsDictionary.Remove(_selectedOrigin);
            selectedOrigin = DEFAULT_ORIGIN_NAME;
        }
        public int GetIndexOfOrigin(string name) => _originsDictionary.Keys.Select((key, index) => new { key, index })
            .FirstOrDefault(pair => pair.key == name)?.index ?? -1;
        public int GetIndexOfSelectedOrigin() => GetIndexOfOrigin(selectedOrigin);
        public string GetOriginAt(int index) => _originsDictionary.Keys.ElementAt(index);
        public void SelectOrigin(int index) => selectedOrigin = GetOriginAt(index);
        public void SetNextOrigin()
        {
            var selectedOriginIdx = GetIndexOfSelectedOrigin();
            if (selectedOriginIdx < _originsDictionary.Count - 1) ++selectedOriginIdx;
            else selectedOriginIdx = 0;
            SelectOrigin(selectedOriginIdx);
        }
        public void ResetOrigin()
        {
            _origin = _originsDictionary[_selectedOrigin].position;
            _rotation = _originsDictionary[_selectedOrigin].rotation;
            DataChanged();
            if (OnGridOriginChange != null) OnGridOriginChange();
        }
        #endregion
        public void SetOriginHeight(Vector3 point, AxesUtils.Axis axis)
        {
            var originPos = origin;
            AxesUtils.SetAxisValue(ref originPos, axis, AxesUtils.GetAxisValue(point, axis));
            origin = originPos;
        }

        public bool IsSnappingEnabledInThisDirection(Vector3 direction)
        {
            bool isParallel(Vector3 other)
                => Vector3.Cross(direction, other).magnitude < 0.0000001;
            if (isParallel(_rotation * Vector3.up) && _snappingOn.y) return true;
            if (isParallel(_rotation * Vector3.right) && _snappingOn.x) return true;
            if (isParallel(_rotation * Vector3.forward) && _snappingOn.z) return true;
            return false;
        }

        public Vector3 TransformToGridDirection(Vector3 direction)
        {
            if (direction == Vector3.zero) return _rotation * Vector3.up;
            var xProjection = Vector3.Project(direction, _rotation * Vector3.right);
            var yProjection = Vector3.Project(direction, _rotation * Vector3.up);
            var zProjection = Vector3.Project(direction, _rotation * Vector3.forward);
            var xProjectionMagnitude = xProjection.magnitude;
            var yProjectionMagnitude = yProjection.magnitude;
            var zProjectionMagnitude = zProjection.magnitude;
            var max = Mathf.Max(xProjectionMagnitude, yProjectionMagnitude, zProjectionMagnitude);
            if (xProjectionMagnitude == max) return xProjection.normalized;
            if (yProjectionMagnitude == max) return yProjection.normalized;
            return zProjection.normalized;
        }

        public void OnBeforeSerialize()
        {
            _origins = _originsDictionary.Select(pair => new GridOrigin(pair.Key, pair.Value)).ToArray();
        }

        public void OnAfterDeserialize()
        {
            if (_origins == null || _origins.Length == 0) return;
            _originsDictionary = _origins.ToDictionary(origin => origin.name, origin => origin.pose);
        }
    }

    [System.Serializable]
    public class SnapManager
    {
        private static SnapSettings _staticSettings = new SnapSettings();
        [SerializeField] SnapSettings _settings = _staticSettings;
        public static SnapSettings settings => _staticSettings;

        public static void FrameGridOrigin()
        {
            var sceneView = (UnityEditor.SceneView)(UnityEditor.SceneView.sceneViews[0]);
            if (sceneView == null) return;
            var viewportPoint = sceneView.camera.WorldToViewportPoint(settings.origin);
            bool originOnScreen = viewportPoint.x > 0 && viewportPoint.y > 0
                && viewportPoint.x < 1 && viewportPoint.y < 1;
            if (originOnScreen) return;
            var activeGO = UnityEditor.Selection.activeGameObject;
            var tempGO = new GameObject();
            tempGO.transform.position = settings.origin;
            UnityEditor.Selection.activeObject = tempGO;
            UnityEditor.SceneView.FrameLastActiveSceneView();
            UnityEditor.Selection.activeGameObject = activeGO;
            GameObject.DestroyImmediate(tempGO);
        }

        public static void ToggleGridPositionHandle()
        {
            if (!settings.lockedGrid) settings.lockedGrid = true;
            settings.showPositionHandle = !settings.showPositionHandle;
            SnapSettingsWindow.RepaintWindow();
        }

        public static void ToggleGridRotationHandle()
        {
            if (!settings.lockedGrid) settings.lockedGrid = true;
            settings.showRotationHandle = !settings.showRotationHandle;
            SnapSettingsWindow.RepaintWindow();
        }

        public static void ToggleGridScaleHandle()
        {
            if (!settings.lockedGrid) settings.lockedGrid = true;
            settings.showScaleHandle = !settings.showScaleHandle;
            SnapSettingsWindow.RepaintWindow();
        }
    }
    #endregion
    #region CAMERA ORIENTATION
    public static class SceneViewCameraOrientation
    {
        public enum Orientation
        {
            Top,
            Bottom,
            Left,
            Right,
            Front,
            Back,
            NotAligned
        }

        public static Orientation GetSceneViewOrientation()
        {
            if (UnityEditor.SceneView.lastActiveSceneView == null) return Orientation.NotAligned;
            var cam = UnityEditor.SceneView.lastActiveSceneView.camera;
            if (cam == null) return Orientation.NotAligned;

            Vector3 forward = cam.transform.forward;

            if (Vector3.Dot(forward, Vector3.down) > 0.99f) return Orientation.Top;
            if (Vector3.Dot(forward, Vector3.up) > 0.99f) return Orientation.Bottom;
            if (Vector3.Dot(forward, Vector3.left) > 0.99f) return Orientation.Right;
            if (Vector3.Dot(forward, Vector3.right) > 0.99f) return Orientation.Left;
            if (Vector3.Dot(forward, Vector3.back) > 0.99f) return Orientation.Front;
            if (Vector3.Dot(forward, Vector3.forward) > 0.99f) return Orientation.Back;

            return Orientation.NotAligned;
        }
    }
    #endregion
    #region PWBIO
    public static partial class PWBIO
    {
        #region SNAP TO GRID
        private static Vector3 SnapPosition(Vector3 position, bool onGrid, bool applySettings,
            Vector3 snapStepFactor, bool ignoreMidpoints = false)
        {
            var result = position;
            if (SnapManager.settings.radialGridEnabled)
            {
                var rotation = SnapManager.settings.rotation;
                if (SnapManager.settings.gridOnX) rotation *= Quaternion.AngleAxis(-90, Vector3.forward);
                else if (SnapManager.settings.gridOnZ) rotation *= Quaternion.AngleAxis(-90, Vector3.right);
                var localPosition = Quaternion.Inverse(rotation) * (position - SnapManager.settings.origin);
                var snappedDirOnPlane = new Vector3(localPosition.x, 0, localPosition.z).normalized;
                if (SnapManager.settings.snapToRadius)
                {
                    var sectorAngleRad = TAU / SnapManager.settings.radialSectors;
                    var angleRad = Mathf.Atan2(localPosition.z, localPosition.x);
                    var snappedAngleRad = Mathf.Round(angleRad / sectorAngleRad) * sectorAngleRad;
                    snappedDirOnPlane = new Vector3(Mathf.Cos(snappedAngleRad), 0, Mathf.Sin(snappedAngleRad));
                    var sizeOnplane = Mathf.Sqrt(localPosition.x * localPosition.x
                        + localPosition.z * localPosition.z);
                    var snappedOnPlane = snappedDirOnPlane * sizeOnplane;
                    var localSnapedPosition = new Vector3(snappedOnPlane.x, localPosition.y, snappedOnPlane.z);
                    result = rotation * localSnapedPosition + SnapManager.settings.origin;
                }
                if (SnapManager.settings.snapToCircunference)
                {
                    var sizeOnplane = Mathf.Sqrt(localPosition.x * localPosition.x
                       + localPosition.z * localPosition.z);
                    var sizeOnPlaneSnapped = Mathf.Round(sizeOnplane / SnapManager.settings.radialStep)
                        * SnapManager.settings.radialStep;
                    var localSnapedPosition = snappedDirOnPlane * sizeOnPlaneSnapped
                        + new Vector3(0, localPosition.y, 0);
                    result = rotation * localSnapedPosition + SnapManager.settings.origin;
                }
            }
            else
            {
                var localPosition = Quaternion.Inverse(SnapManager.settings.rotation)
                * (position - SnapManager.settings.origin);
                float Snap(float step, float value)
                {
                    if (!ignoreMidpoints && SnapManager.settings.midpointSnapping) step *= 0.5f;
                    return Mathf.Round(value / step) * step;
                }
                var localSnappedPosition = new Vector3(
                    Snap(SnapManager.settings.step.x * snapStepFactor.x, localPosition.x),
                    Snap(SnapManager.settings.step.y * snapStepFactor.y, localPosition.y),
                    Snap(SnapManager.settings.step.z * snapStepFactor.z, localPosition.z));
                result = SnapManager.settings.rotation * (applySettings ? new Vector3(
                    SnapManager.settings.snappingOnX ? localSnappedPosition.x : onGrid ? 0 : localPosition.x,
                    SnapManager.settings.snappingOnY ? localSnappedPosition.y : onGrid ? 0 : localPosition.y,
                    SnapManager.settings.snappingOnZ ? localSnappedPosition.z : onGrid ? 0 : localPosition.z)
                    : localSnappedPosition) + SnapManager.settings.origin;
            }
            return result;
        }

        private static Vector3 SnapAndUpdateGridOrigin(Vector3 point, bool snapToGrid,
            bool paintOnPalettePrefabs, bool paintOnMeshesWithoutCollider, bool ignoresceneColliders, bool paintOnTheGrid,
            Vector3 projectionDirection)
        {
            if (snapToGrid)
            {
                point = SnapPosition(point, paintOnTheGrid, applySettings: true, snapStepFactor: Vector3.one);
                var direction = SnapManager.settings.TransformToGridDirection(SnapManager.settings.rotation
                    * projectionDirection);
                if (!paintOnTheGrid && !SnapManager.settings.IsSnappingEnabledInThisDirection(direction))
                {
                    var ray = new Ray(point - direction, direction);
                    if (PWBToolRaycast(ray, out RaycastHit hit, out GameObject collider, float.MaxValue, -1,
                       paintOnPalettePrefabs, paintOnMeshesWithoutCollider, ignoreSceneColliders: ignoresceneColliders))
                        point = hit.point;
                }
            }
            UpdateGridOrigin(point);
            return point;
        }
        private static Vector3 SnapFloorTilePosition(Vector3 position, out Vector3 localPosition)
        {
            var toolSettings = FloorManager.settings;
            var brushOffset = Vector3.zero;
            if (toolSettings.subtractBrushOffset)
            {
                BrushSettings brush = PaletteManager.selectedBrush;
                if (toolSettings.overwriteBrushProperties) brush = toolSettings.brushSettings;
                if (brush != null) brushOffset = brush.localPositionOffset;
                if (FloorManager.quarterTurns > 0)
                    brushOffset = Quaternion.AngleAxis(FloorManager.quarterTurns * 90,
                        FloorManager.settings.upwardAxis) * brushOffset;
            }
            var localOriginOffset = (SnapManager.settings.step - brushOffset) * 0.5f
               - Vector3.up * SnapManager.settings.step.y;
            var origin = SnapManager.settings.origin + SnapManager.settings.rotation * localOriginOffset;
            var localPos = Quaternion.Inverse(SnapManager.settings.rotation) * (position - origin);
            float Snap(float step, float value) => Mathf.Round(value / step) * step;
            var localSnappedPos = new Vector3(Snap(SnapManager.settings.step.x, localPos.x), 0f,
                    Snap(SnapManager.settings.step.z, localPos.z));
            localPosition = localSnappedPos;
            var result = SnapManager.settings.rotation * localSnappedPos + origin;
            return result;
        }


        private enum CellSide { R, L, F, B };
        private static CellSide GetCellSide(Vector3 pointToGridLocal, AxesUtils.Axis axis)
        {
            CellSide cellSide;
            if (axis == AxesUtils.Axis.Z)
                cellSide = pointToGridLocal.x < 0 ? CellSide.L : CellSide.R;
            else cellSide = pointToGridLocal.z < 0 ? CellSide.B : CellSide.F;
            return cellSide;
        }
        private static Vector3 GetWallLocalBrushOffset(CellSide cellSide)
        {
            var toolSettings = WallManager.settings;
            var brushOffset = Vector3.zero;
            if (toolSettings.subtractBrushOffset)
            {
                BrushSettings brush = PaletteManager.selectedBrush;
                if (toolSettings.overwriteBrushProperties) brush = toolSettings.brushSettings;
                if (brush != null) brushOffset = brush.localPositionOffset;
                if (cellSide == CellSide.L || cellSide == CellSide.R)
                {
                    var angle = cellSide == CellSide.L ? -90 : 90;
                    brushOffset = Quaternion.AngleAxis(angle, FloorManager.settings.upwardAxis) * brushOffset;
                }
                else if (cellSide == CellSide.B)
                    brushOffset = Quaternion.AngleAxis(180, FloorManager.settings.upwardAxis) * brushOffset;
            }
            return brushOffset;
        }
        private static Vector3 SnapWallPosition(Vector3 position, out AxesUtils.Axis axis,
            out bool rotateHalfTurn, out Vector3 localPosition)
        {
            var toolSettings = WallManager.settings;

            var snappedPoint = SnapPosition(position, onGrid: true, applySettings: true, snapStepFactor: Vector3.one);
            var localSnappedPoint = Quaternion.Inverse(SnapManager.settings.rotation)
                * (snappedPoint - SnapManager.settings.origin);
            var pointToGrid = snappedPoint - position;
            var pointToGridLocal = Quaternion.Inverse(SnapManager.settings.rotation) * pointToGrid;
            axis = Mathf.Abs(pointToGridLocal.x) < Mathf.Abs(pointToGridLocal.z) ? AxesUtils.Axis.Z : AxesUtils.Axis.X;

            CellSide cellSide = GetCellSide(pointToGridLocal, axis);
            var localBrushOffset = GetWallLocalBrushOffset(cellSide);
            var localOriginOffset = SnapManager.settings.step * 0.5f;
            localOriginOffset.y = 0f;
            localOriginOffset -= localBrushOffset * 0.5f;
            var origin = SnapManager.settings.origin + SnapManager.settings.rotation * localOriginOffset;

            var localPos = Quaternion.Inverse(SnapManager.settings.rotation) * (position - origin);
            float Snap(float step, float value) => Mathf.Round(value / step) * step;
            var xSnappedToCenter = Snap(SnapManager.settings.step.x, localPos.x);
            var zSnappedToCenter = Snap(SnapManager.settings.step.z, localPos.z);

            var xSnappedToBorder = xSnappedToCenter;
            var zSnappedToBorder = zSnappedToCenter;
            rotateHalfTurn = false;
            if (cellSide == CellSide.L || cellSide == CellSide.R)
            {
                if (cellSide == CellSide.L)
                {
                    xSnappedToBorder = localSnappedPoint.x
                        + (WallManager.wallThickness - SnapManager.settings.step.x) * 0.5f;
                    rotateHalfTurn = true;
                }
                else xSnappedToBorder = localSnappedPoint.x
                        - (WallManager.wallThickness + SnapManager.settings.step.x) * 0.5f;
            }
            else
            {
                if (cellSide == CellSide.B)
                {
                    zSnappedToBorder = localSnappedPoint.z
                        + (WallManager.wallThickness - SnapManager.settings.step.z) * 0.5f;
                    rotateHalfTurn = true;
                }
                else zSnappedToBorder = localSnappedPoint.z
                        - (WallManager.wallThickness + SnapManager.settings.step.x) * 0.5f;
            }
            var yOffset = toolSettings.moduleSize.y / 2;

            var localSnappedPos = new Vector3(xSnappedToBorder, yOffset, zSnappedToBorder);

            localPosition = localSnappedPos;
            var result = SnapManager.settings.rotation * localSnappedPos + origin;
            return result;
        }

        private static Vector3 SnapWallPosition(Vector3 startPoint, Vector3 endPoint,
            out AxesUtils.Axis axis, out int cellsCount, out bool rotateHalfTurn, out Vector3 localPosition)
        {
            float Snap(float step, float value) => Mathf.Round(value / step) * step;
            Vector3 SnapToCenter(Vector3 origin, Vector3 point)
            {
                var localPoint = Quaternion.Inverse(SnapManager.settings.rotation) * (point - origin);
                var localXSnappedToCenter = Snap(SnapManager.settings.step.x, localPoint.x);
                var localZSnappedTocenter = Snap(SnapManager.settings.step.z, localPoint.z);
                var localCellCenter = new Vector3(localXSnappedToCenter, 0f, localZSnappedTocenter);
                return localCellCenter;
            }
            var segment = endPoint - startPoint;
            var localSegment = Quaternion.Inverse(SnapManager.settings.rotation) * segment;
            var segmentMagnitudeX = Mathf.Abs(localSegment.x);
            var segmentMagnitudeZ = Mathf.Abs(localSegment.z);

            var localStartGridCenter = SnapToCenter(SnapManager.settings.origin, startPoint);
            var localEndGridCenter = SnapToCenter(SnapManager.settings.origin, endPoint);
            var centerToCenterSegment = localEndGridCenter - localStartGridCenter;
            var centerToCenterMagnitudeX = Mathf.Abs(centerToCenterSegment.x);
            var centerToCenterMagnitudeZ = Mathf.Abs(centerToCenterSegment.z);

            var endPointSnappedToGrid = SnapPosition(endPoint, onGrid: true, applySettings: true,
                snapStepFactor: Vector3.one);
            var localEndPointSnappedToGrid = Quaternion.Inverse(SnapManager.settings.rotation)
               * (endPointSnappedToGrid - SnapManager.settings.origin);
            var pointToGrid = endPointSnappedToGrid - endPoint;
            var pointToGridLocal = Quaternion.Inverse(SnapManager.settings.rotation) * pointToGrid;

            axis = segmentMagnitudeX > segmentMagnitudeZ ? AxesUtils.Axis.X : AxesUtils.Axis.Z;
            if (centerToCenterMagnitudeX < SnapManager.settings.step.x
                && centerToCenterMagnitudeZ < SnapManager.settings.step.z)
                axis = Mathf.Abs(pointToGridLocal.x) < Mathf.Abs(pointToGridLocal.z) ? AxesUtils.Axis.Z : AxesUtils.Axis.X;

            CellSide cellSide = GetCellSide(pointToGridLocal, axis);
            var localBrushOffset = GetWallLocalBrushOffset(cellSide);

            var localOriginOffset = SnapManager.settings.step * 0.5f;
            localOriginOffset.y = 0f;
            localOriginOffset -= localBrushOffset * 0.5f;
            var origin = SnapManager.settings.origin + SnapManager.settings.rotation * localOriginOffset;

            var localStartCellCenter = SnapToCenter(origin, startPoint);
            var localEndCellCenter = SnapToCenter(origin, endPoint);

            var localSnappedSegment = localEndCellCenter - localStartCellCenter;
            var snappedMagnitudeX = Mathf.Abs(localSnappedSegment.x);
            var snappedMagnitudeZ = Mathf.Abs(localSnappedSegment.z);

            var localXSnappedToBorder = localEndCellCenter.x;
            var localZSnappedToBorder = localEndCellCenter.z;
            rotateHalfTurn = false;

            if (cellSide == CellSide.L || cellSide == CellSide.R)
            {
                if (cellSide == CellSide.L)
                {
                    localXSnappedToBorder = localEndPointSnappedToGrid.x
                        + (WallManager.wallThickness - SnapManager.settings.step.x) * 0.5f;
                    rotateHalfTurn = true;
                }
                else localXSnappedToBorder = localEndPointSnappedToGrid.x
                        - (WallManager.wallThickness + SnapManager.settings.step.x) * 0.5f;
                cellsCount = Mathf.RoundToInt(snappedMagnitudeZ / SnapManager.settings.step.z) + 1;
            }
            else
            {
                if (cellSide == CellSide.B)
                {
                    localZSnappedToBorder = localEndPointSnappedToGrid.z
                        + (WallManager.wallThickness - SnapManager.settings.step.z) * 0.5f;
                    rotateHalfTurn = true;
                }
                else localZSnappedToBorder = localEndPointSnappedToGrid.z
                        - (WallManager.wallThickness + SnapManager.settings.step.x) * 0.5f;
                cellsCount = Mathf.RoundToInt(snappedMagnitudeX / SnapManager.settings.step.x) + 1;
            }

            var yOffset = WallManager.settings.moduleSize.y / 2;

            var localSnappedPos = new Vector3(localXSnappedToBorder, yOffset, localZSnappedToBorder);
            localPosition = localSnappedPos;
            var result = SnapManager.settings.rotation * localSnappedPos + origin;
            return result;
        }

        private static void UpdateGridOrigin(Vector3 hitPoint)
        {
            var snapOrigin = SnapManager.settings.origin;
            if (!SnapManager.settings.lockedGrid)
            {
                if (SnapManager.settings.gridOnX) snapOrigin.x = hitPoint.x;
                else if (SnapManager.settings.gridOnY) snapOrigin.y = hitPoint.y;
                else if (SnapManager.settings.gridOnZ) snapOrigin.z = hitPoint.z;
            }
            SnapManager.settings.origin = snapOrigin;
        }
        #endregion
        #region SNAP TO VERTEX
        private static bool _snappedToVertex = false;

        private static bool SnapToVertex(Ray ray, out RaycastHit closestVertexInfo,
            bool in2DMode, GameObject[] selection = null)
        {
            Vector2 origin2D = ray.origin;
            bool snappedToVertex = false;
            float radius = 1f;
            RaycastHit[] hitArray = null;
            Collider2D[] collider2DArray = null;

            do
            {
                var collidingWithOctreeNodes = new System.Collections.Generic.List<GameObject>();
                boundsOctree.GetColliding(collidingWithOctreeNodes, ray, radius, float.PositiveInfinity);
                for (int i = 0; i < collidingWithOctreeNodes.Count; ++i)
                    PWBCore.AddTempCollider(collidingWithOctreeNodes[i]);

                if (selection == null)
                {
                    if (Physics.SphereCast(ray, radius, out RaycastHit hitInfo))
                    {
                        hitArray = new RaycastHit[1];
                        hitArray[0] = hitInfo;
                    }
                    else
                    {
                        hitArray = null;
                    }
                }
                else
                {
                    var allHits = Physics.SphereCastAll(ray, radius);
                    int selLen = selection.Length;
                    int filteredCount = 0;
                    RaycastHit[] tempFiltered = new RaycastHit[allHits.Length];
                    for (int i = 0; i < allHits.Length; ++i)
                    {
                        var colliderObj = allHits[i].collider.gameObject;
                        int hitID = colliderObj.GetInstanceID();
                        if (PWBCore.IsTempCollider(hitID))
                        {
                            colliderObj = PWBCore.GetGameObjectFromTempColliderId(hitID);
                            hitID = colliderObj.GetInstanceID();
                        }
                        for (int j = 0; j < selLen; ++j)
                        {
                            if (hitID == selection[j].GetInstanceID())
                            {
                                tempFiltered[filteredCount++] = allHits[i];
                                break;
                            }
                        }
                    }
                    if (filteredCount > 0)
                    {
                        hitArray = new RaycastHit[filteredCount];
                        for (int i = 0; i < filteredCount; ++i)
                            hitArray[i] = tempFiltered[i];
                    }
                    else
                    {
                        hitArray = null;
                    }
                }

                // visibility filtering
                if (hitArray != null && hitArray.Length > 0)
                {
                    int visibleCount = 0;
                    RaycastHit[] tempVisible = new RaycastHit[hitArray.Length];
                    for (int i = 0; i < hitArray.Length; ++i)
                    {
                        var obj = hitArray[i].collider.gameObject;
                        if (PWBCore.IsTempCollider(obj.GetInstanceID()))
                            obj = PWBCore.GetGameObjectFromTempColliderId(obj.GetInstanceID());
                        if (IsVisible(ref obj))
                        {
                            tempVisible[visibleCount++] = hitArray[i];
                        }
                    }
                    if (visibleCount > 0)
                    {
                        if (visibleCount < hitArray.Length)
                        {
                            RaycastHit[] newArray = new RaycastHit[visibleCount];
                            for (int i = 0; i < visibleCount; ++i)
                                newArray[i] = tempVisible[i];
                            hitArray = newArray;
                        }
                        else
                        {
                            hitArray = tempVisible;
                        }
                        break;
                    }
                    else
                    {
                        hitArray = null;
                    }
                }

                if (in2DMode)
                {
                    var allColliders = Physics2D.OverlapCircleAll(origin2D, radius);
                    int filteredCount = 0;
                    Collider2D[] tempFiltered = new Collider2D[allColliders.Length];
                    for (int i = 0; i < allColliders.Length; ++i)
                    {
                        var colliderObj = allColliders[i].gameObject;
                        int hitID = colliderObj.GetInstanceID();
                        if (PWBCore.IsTempCollider(hitID))
                        {
                            colliderObj = PWBCore.GetGameObjectFromTempColliderId(hitID);
                            hitID = colliderObj.GetInstanceID();
                        }
                        for (int j = 0; selection != null && j < selection.Length; ++j)
                        {
                            if (hitID == selection[j].GetInstanceID())
                            {
                                tempFiltered[filteredCount++] = allColliders[i];
                                break;
                            }
                        }
                    }
                    if (filteredCount > 0)
                    {
                        collider2DArray = new Collider2D[filteredCount];
                        for (int i = 0; i < filteredCount; ++i)
                            collider2DArray[i] = tempFiltered[i];
                        break;
                    }
                    else
                    {
                        collider2DArray = null;
                    }
                }
                radius *= 2f;
            } while (radius <= 1024f);

            if (hitArray != null && hitArray.Length > 0)
            {
                float minDist = float.MaxValue;
                GameObject closestObj = null;
                for (int i = 0; i < hitArray.Length; ++i)
                {
                    if (hitArray[i].distance < minDist)
                    {
                        minDist = hitArray[i].distance;
                        closestObj = hitArray[i].collider.gameObject;
                        if (PWBCore.IsTempCollider(closestObj.GetInstanceID()))
                            closestObj = PWBCore.GetGameObjectFromTempColliderId(closestObj.GetInstanceID());
                    }
                }
                if (closestObj != null && DistanceUtils.FindNearestVertexToMouse(out closestVertexInfo, closestObj.transform))
                    return true;
            }

            snappedToVertex = false;
            closestVertexInfo = new RaycastHit();

            if (in2DMode && collider2DArray != null && collider2DArray.Length > 0)
            {
                float minSqrDistance = float.MaxValue;
                for (int i = 0; i < collider2DArray.Length; ++i)
                {
                    var obj = collider2DArray[i].gameObject;
                    if (PWBCore.IsTempCollider(obj.GetInstanceID()))
                        obj = PWBCore.GetGameObjectFromTempColliderId(obj.GetInstanceID());

                    RaycastHit closestVertexInfo2D;
                    if (DistanceUtils.FindNearestVertexToMouse(out closestVertexInfo2D, obj.transform))
                    {
                        float sqrDistance = ((Vector2)closestVertexInfo2D.point - origin2D).sqrMagnitude;
                        if (sqrDistance < minSqrDistance)
                        {
                            minSqrDistance = sqrDistance;
                            closestVertexInfo = closestVertexInfo2D;
                            snappedToVertex = true;
                        }
                    }
                }
                return snappedToVertex;
            }

#if UNITY_2020_2_OR_NEWER
            return DistanceUtils.FindNearestVertexToMouse(out closestVertexInfo, null);
#else
    return false;
#endif
        }
        #endregion
        #region SNAP TO BOUNDING BOX


        public static Vector3 SnapToBounds(Vector3 mousePos)
        {
            if (!SnapManager.settings.boundsSnapping) return mousePos;
            var sceneView = UnityEditor.SceneView.lastActiveSceneView;
            if (sceneView == null || sceneView.camera == null)
                return mousePos;
            Camera cam = sceneView.camera;
            float maxDistance = float.MaxValue;
            float radius = Mathf.Max(UnityEditor.HandleUtility.GetHandleSize(mousePos) * 0.1f, 0.02f);

            Vector3 SnapToBoundsInDirection(Vector3 position, Vector3 direction)
            {
                (GameObject obj, Bounds bounds)[] objectsColliding = null;
                var ray = new Ray(position, direction);
                boundsOctree.GetCollidingtWithinFrustum(ray, radius, cam, out objectsColliding, maxDistance);
                Vector3 bestPoint = position;
                float bestDistanceToRay = radius;
                Bounds bestBox = new Bounds();
                float bestOriginToPointDistance = float.MaxValue;
                foreach (var colliding in objectsColliding)
                {
                    var b = colliding.bounds;
                    Vector3 min = b.min, max = b.max, mid = b.center;

                    var pts = new Vector3[]
                    {
                        new Vector3(min.x, min.y, min.z),
                        new Vector3(min.x, min.y, mid.z),
                        new Vector3(min.x, min.y, max.z),
                        new Vector3(min.x, mid.y, min.z),
                        new Vector3(min.x, mid.y, mid.z),
                        new Vector3(min.x, mid.y, max.z),
                        new Vector3(min.x, max.y, min.z),
                        new Vector3(min.x, max.y, mid.z),
                        new Vector3(min.x, max.y, max.z),

                        new Vector3(mid.x, min.y, min.z),
                        new Vector3(mid.x, min.y, mid.z),
                        new Vector3(mid.x, min.y, max.z),
                        new Vector3(mid.x, mid.y, min.z),
                        new Vector3(mid.x, mid.y, mid.z),
                        new Vector3(mid.x, mid.y, max.z),
                        new Vector3(mid.x, max.y, min.z),
                        new Vector3(mid.x, max.y, mid.z),
                        new Vector3(mid.x, max.y, max.z),

                        new Vector3(max.x, min.y, min.z),
                        new Vector3(max.x, min.y, mid.z),
                        new Vector3(max.x, min.y, max.z),
                        new Vector3(max.x, mid.y, min.z),
                        new Vector3(max.x, mid.y, mid.z),
                        new Vector3(max.x, mid.y, max.z),
                        new Vector3(max.x, max.y, min.z),
                        new Vector3(max.x, max.y, mid.z),
                        new Vector3(max.x, max.y, max.z),
                    };

                    foreach (var p in pts)
                    {
                        var originToPoint = p - position;
#if UNITY_2021_1_OR_NEWER
                        var distanceToRay = System.MathF.Round(Vector3.Cross(direction, originToPoint).magnitude, 5);
#else
                        var distanceToRay = (float)System.Math.Round(Vector3.Cross(direction, originToPoint).magnitude, 5);
#endif

                        if (distanceToRay > bestDistanceToRay) continue;
                        var originToPointDistance = originToPoint.magnitude;
                        if (distanceToRay == bestDistanceToRay && originToPointDistance > bestOriginToPointDistance) continue;
                        bestDistanceToRay = distanceToRay;
                        bestPoint = p;
                        bestBox = b;
                        bestOriginToPointDistance = originToPointDistance;
                    }
                }

                var plane = new Plane(direction, position);
                if (bestDistanceToRay < radius)
                {
                    var projectedPoint = plane.ClosestPointOnPlane(bestPoint);
                    UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                    UnityEditor.Handles.color = new Color(1f, 0.5f, 0.8f, 1f);
                    UnityEditor.Handles.DrawLine(projectedPoint, bestPoint, thickness: 3f);

                    float worldRadius = UnityEditor.HandleUtility.GetHandleSize(bestPoint) * 0.05f;
                    UnityEditor.Handles.DrawSolidDisc(bestPoint, (cam.transform.position - bestPoint), worldRadius);
                    var TRS = Matrix4x4.TRS(bestBox.center, Quaternion.identity, bestBox.size);
                    Graphics.DrawMesh(cubeMesh, TRS, snapBoxMaterial, layer: 0, cam);

                    return projectedPoint;
                }
                return position;
            }
            var result = SnapToBoundsInDirection(mousePos, Vector3.right);
            result = SnapToBoundsInDirection(result, Vector3.forward);
            result = SnapToBoundsInDirection(result, Vector3.up);
            result = SnapToBoundsInDirection(result, Vector3.left);
            result = SnapToBoundsInDirection(result, Vector3.back);
            result = SnapToBoundsInDirection(result, Vector3.down);
            return result;
        }
        #endregion
        #region GRID
        private static void GridHandles()
        {
            if (!SnapManager.settings.lockedGrid) return;
            var originOffset = SnapManager.settings.origin;
            var rotation = SnapManager.settings.rotation;
            var snapSize = SnapManager.settings.step;
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            var handleSize = UnityEditor.HandleUtility.GetHandleSize(originOffset);

            void DrawSnapGizmos(AxesUtils.Axis forwardAxis, AxesUtils.Axis upwardAxis)
            {
                var fw = rotation * AxesUtils.GetVector(1, forwardAxis);
                var uw = rotation * AxesUtils.GetVector(1, upwardAxis);
                var coneSize = handleSize * 0.15f;
                var stepSize = SnapManager.settings.radialGridEnabled ? SnapManager.settings.radialStep
                    : AxesUtils.GetAxisValue(snapSize, forwardAxis);
                var conePosFw = originOffset + fw * (handleSize * 1.6f);
                var originScreenPos = _sceneViewCamera.WorldToScreenPoint(SnapManager.settings.origin);
                var fwScreenPos = _sceneViewCamera.WorldToScreenPoint(conePosFw);
                var alpha = Mathf.Clamp01((fwScreenPos - originScreenPos).magnitude / 90 - 0.5f);

                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                float distFromMouse = UnityEditor.HandleUtility.DistanceToCircle(conePosFw, coneSize / 2);
                UnityEditor.HandleUtility.AddControl(controlId, distFromMouse);
                bool mouseOver = UnityEditor.HandleUtility.nearestControl == controlId;

                UnityEditor.Handles.color = new Color(1f, 1f, mouseOver ? 1 : 0, alpha);
                UnityEditor.Handles.ConeHandleCap(controlId, conePosFw,
                    Quaternion.LookRotation(fw, uw), coneSize, EventType.Repaint);
                if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && mouseOver)
                    SnapManager.settings.origin += fw * stepSize;

                var conePosBw = originOffset + fw * (handleSize * 1.3f);
                controlId = GUIUtility.GetControlID(FocusType.Passive);
                distFromMouse = UnityEditor.HandleUtility.DistanceToCircle(conePosBw, coneSize / 2);
                UnityEditor.HandleUtility.AddControl(controlId, distFromMouse);
                mouseOver = UnityEditor.HandleUtility.nearestControl == controlId;

                UnityEditor.Handles.color = new Color(1f, 1f, mouseOver ? 1 : 0, alpha);
                UnityEditor.Handles.ConeHandleCap(controlId, conePosBw,
                    Quaternion.LookRotation(-fw, -uw), coneSize, EventType.Repaint);
                if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && mouseOver)
                    SnapManager.settings.origin -= fw * stepSize;
            }
            if (SnapManager.settings.showPositionHandle)
            {
                SnapManager.settings.origin = UnityEditor.Handles.PositionHandle(originOffset, rotation);
                UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.SphereHandleCap(0, originOffset, rotation,
                    UnityEditor.HandleUtility.GetHandleSize(originOffset) * 0.2f, EventType.Repaint);
                UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                DrawSnapGizmos(AxesUtils.Axis.X, AxesUtils.Axis.Y);
                DrawSnapGizmos(AxesUtils.Axis.Y, AxesUtils.Axis.Z);
                DrawSnapGizmos(AxesUtils.Axis.Z, AxesUtils.Axis.X);
            }
            else if (SnapManager.settings.showRotationHandle)
                SnapManager.settings.rotation = UnityEditor.Handles.RotationHandle(rotation, originOffset);
            else if (SnapManager.settings.showScaleHandle)
            {
                if (SnapManager.settings.radialGridEnabled)
                {
                    var step0 = Vector3.one * SnapManager.settings.radialStep;
                    var step = UnityEditor.Handles.ScaleHandle(step0, originOffset,
                        rotation, handleSize);
                    if (step0 != step)
                    {
                        if (step0.x != step.x) SnapManager.settings.radialStep = step.x;
                        else if (step0.y != step.y) SnapManager.settings.radialStep = step.y;
                        else SnapManager.settings.radialStep = step.z;
                    }
                }
                else
                {
                    SnapManager.settings.step = UnityEditor.Handles.ScaleHandle(SnapManager.settings.step,
                    originOffset, rotation, handleSize);
                }
            }
            if (SnapManager.settings.origin != originOffset
                || SnapManager.settings.rotation != rotation
                || SnapManager.settings.step != snapSize)
                SnapSettingsWindow.RepaintWindow();
        }

        private static Mesh _gridMesh = null;
        private static Material _gridMaterial = null;
        private static Material _grid64Material = null;
        private static Material _majorLinesGridMaterial = null;
        private static void DrawGridQuad(AxesUtils.Axis axis, UnityEditor.SceneView sceneView)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (_gridMaterial == null)
                _gridMaterial = new Material(Resources.Load<Material>("Materials/Grid"));
            if (_grid64Material == null)
                _grid64Material = new Material(Resources.Load<Material>("Materials/Grid64"));
            if (_majorLinesGridMaterial == null)
                _majorLinesGridMaterial = new Material(_gridMaterial);
            if (_gridMesh == null)
            {
                _gridMesh = new Mesh();
                _gridMesh.vertices = new Vector3[]
                {
                    new Vector3(-0.5f, -0.5f, 0),
                    new Vector3(0.5f, -0.5f, 0),
                    new Vector3(0.5f, 0.5f, 0),
                    new Vector3(-0.5f, 0.5f, 0)
                };
                _gridMesh.uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                };
                _gridMesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
                _gridMesh.RecalculateNormals();
            }

            var rotation = SnapManager.settings.rotation;
            var origin = SnapManager.settings.origin;
            Camera cam = sceneView.camera;
            Vector3 camPos = cam.transform.position;
            Vector3 camForward = cam.transform.forward;
            Vector3 planeNormal = rotation * (
                axis == AxesUtils.Axis.X ? Vector3.right :
                axis == AxesUtils.Axis.Y ? Vector3.up :
                Vector3.forward
            );
            Plane gridPlane = new Plane(planeNormal, origin);
            float enter;
            Vector3 focusPoint = origin;
            Ray camRay = new Ray(camPos, camForward);
            if (gridPlane.Raycast(camRay, out enter))
                focusPoint = camRay.GetPoint(enter);

            int cellsCount = 10;
            Vector2 sceneViewSize = sceneView.position.size;
            cellsCount = Mathf.Max((int)sceneViewSize.x, (int)sceneViewSize.y);
            if (cellsCount % 2 == 1) ++cellsCount;

            var snapSize = SnapManager.settings.step;
            void DrawMesh(Vector3 snapStepFactor, Material mat, float alpha)
            {
                var gridSize = Vector3.Scale(snapSize, snapStepFactor);
                float quadCellSizeX = 1f;
                float quadCellSizeY = 1f;
                Quaternion localRotation = Quaternion.identity;
                var color = new Color(0.5f, 0.5f, 0.5f, alpha);
                switch (axis)
                {
                    case AxesUtils.Axis.X:
                        quadCellSizeX = gridSize.y;
                        quadCellSizeY = gridSize.z;
                        localRotation = Quaternion.Euler(0, 90, 90);
                        color.r = 1f;
                        break;
                    case AxesUtils.Axis.Y:
                        quadCellSizeX = gridSize.x;
                        quadCellSizeY = gridSize.z;
                        localRotation = Quaternion.Euler(90, 0, 0);
                        color.g = 1f;
                        break;
                    case AxesUtils.Axis.Z:
                        quadCellSizeX = gridSize.x;
                        quadCellSizeY = gridSize.y;
                        color.b = 1f;
                        break;
                }
                Vector3 scale = new Vector3(cellsCount * quadCellSizeX, cellsCount * quadCellSizeY, 1);
                Quaternion quadRotation = rotation * localRotation;
                Vector3 quadPosition = SnapPosition(focusPoint, onGrid: true, applySettings: false,
                    snapStepFactor, ignoreMidpoints: true);

                var trs = Matrix4x4.TRS(quadPosition, quadRotation, scale);
                var tiling = new Vector2(cellsCount, cellsCount);
                mat.SetVector("_Tiling", tiling);
                mat.SetTextureScale("_MainTex", tiling);
                mat.SetColor("_Color", color);
                Graphics.DrawMesh(_gridMesh, trs, mat, 0, cam);

            }
            var majorMat = _majorLinesGridMaterial;
            var f = Mathf.Min(SnapManager.settings.majorLinesGap.x,
                SnapManager.settings.majorLinesGap.y, SnapManager.settings.majorLinesGap.z);
            if (f <= 4) majorMat = _grid64Material;
            DrawMesh(snapStepFactor: SnapManager.settings.majorLinesGap, majorMat, alpha: 0.3f);

            DrawMesh(snapStepFactor: Vector3.one, _gridMaterial, alpha: 0.7f);
        }

        private static void DrawGrid(AxesUtils.Axis axis, Vector3 focusPoint, int maxCells, Vector3 snapSize)
        {
            var rotation = SnapManager.settings.rotation;
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            var focusOffset = Quaternion.Inverse(SnapManager.settings.rotation) * (focusPoint - SnapManager.settings.origin);
            var focusOffsetInt = new Vector3Int(Mathf.RoundToInt(focusOffset.x / snapSize.x),
              Mathf.RoundToInt(focusOffset.y / snapSize.y), Mathf.RoundToInt(focusOffset.z / snapSize.z));
            float GetAlpha(float cell, int majorLinesGap) => (cell % majorLinesGap == 0) ? 0.5f : 0.2f;

            for (int i = 0; i < maxCells; ++i)
            {
                for (int j = 1; j < maxCells; ++j)
                {
                    var p1 = Vector3.zero;
                    var p2 = Vector3.zero;
                    var p3 = Vector3.zero;
                    var p4 = Vector3.zero;

                    var alpha1 = (maxCells - Mathf.Max(i, j - 1)) / (float)maxCells;
                    var alpha2 = alpha1;
                    var alpha3 = alpha1;
                    var alpha4 = alpha1;
                    var alpha1R = alpha1;
                    var alpha2R = alpha1;
                    var alpha4R = alpha1;
                    var alpha3R = alpha1;

                    var color = new Color(0.5f, 1f, 0.5f, 0f);
                    switch (axis)
                    {
                        case AxesUtils.Axis.X:
                            color = new Color(1f, 0.5f, 0.5f, 0f);
                            alpha1 *= GetAlpha(i + focusOffsetInt.y, SnapManager.settings.majorLinesGap.y);
                            alpha2 *= GetAlpha(i - focusOffsetInt.y, SnapManager.settings.majorLinesGap.y);
                            alpha3 *= GetAlpha(i + focusOffsetInt.z, SnapManager.settings.majorLinesGap.z);
                            alpha4 *= GetAlpha(i - focusOffsetInt.z, SnapManager.settings.majorLinesGap.z);
                            alpha1R = alpha1;
                            alpha2R = alpha2;
                            alpha3R = alpha4;
                            alpha4R = alpha3;
                            p1 += rotation * Vector3.Scale(new Vector3(0f, i, j - 1), snapSize);
                            p2 += rotation * Vector3.Scale(new Vector3(0f, i, j), snapSize);
                            p3 += rotation * Vector3.Scale(new Vector3(0f, j - 1, i), snapSize);
                            p4 += rotation * Vector3.Scale(new Vector3(0f, j, i), snapSize);
                            break;
                        case AxesUtils.Axis.Y:
                            alpha1 *= GetAlpha(i + focusOffsetInt.x, SnapManager.settings.majorLinesGap.x);
                            alpha2 *= GetAlpha(i - focusOffsetInt.x, SnapManager.settings.majorLinesGap.x);
                            alpha3 *= GetAlpha(i + focusOffsetInt.z, SnapManager.settings.majorLinesGap.z);
                            alpha4 *= GetAlpha(i - focusOffsetInt.z, SnapManager.settings.majorLinesGap.z);
                            alpha1R = alpha2;
                            alpha2R = alpha1;
                            alpha3R = alpha3;
                            alpha4R = alpha4;
                            p1 += rotation * Vector3.Scale(new Vector3(i, 0f, j - 1), snapSize);
                            p2 += rotation * Vector3.Scale(new Vector3(i, 0f, j), snapSize);
                            p3 += rotation * Vector3.Scale(new Vector3(j - 1, 0f, i), snapSize);
                            p4 += rotation * Vector3.Scale(new Vector3(j, 0f, i), snapSize);
                            break;
                        case AxesUtils.Axis.Z:
                            color = new Color(0.5f, 0.5f, 1f, 0f);
                            alpha1 *= GetAlpha(i + focusOffsetInt.x, SnapManager.settings.majorLinesGap.x);
                            alpha2 *= GetAlpha(i - focusOffsetInt.x, SnapManager.settings.majorLinesGap.x);
                            alpha3 *= GetAlpha(i + focusOffsetInt.y, SnapManager.settings.majorLinesGap.y);
                            alpha4 *= GetAlpha(i - focusOffsetInt.y, SnapManager.settings.majorLinesGap.y);
                            alpha1R = alpha1;
                            alpha2R = alpha2;
                            alpha3R = alpha4;
                            alpha4R = alpha3;
                            p1 += rotation * Vector3.Scale(new Vector3(i, j - 1, 0f), snapSize);
                            p2 += rotation * Vector3.Scale(new Vector3(i, j, 0f), snapSize);
                            p3 += rotation * Vector3.Scale(new Vector3(j - 1, i, 0f), snapSize);
                            p4 += rotation * Vector3.Scale(new Vector3(j, i, 0f), snapSize);
                            break;
                    }
                    UnityEditor.Handles.color = color + new Color(0f, 0f, 0f, alpha1);
                    UnityEditor.Handles.DrawLine(focusPoint + p1, focusPoint + p2);
                    UnityEditor.Handles.color = color + new Color(0f, 0f, 0f, alpha2);
                    UnityEditor.Handles.DrawLine(focusPoint - p1, focusPoint - p2);
                    UnityEditor.Handles.color = color + new Color(0f, 0f, 0f, alpha3);
                    UnityEditor.Handles.DrawLine(focusPoint + p3, focusPoint + p4);
                    UnityEditor.Handles.color = color + new Color(0f, 0f, 0f, alpha4);
                    UnityEditor.Handles.DrawLine(focusPoint - p3, focusPoint - p4);
                    if (i == 0) continue;
                    var r180 = Quaternion.AngleAxis(180, rotation * (axis == AxesUtils.Axis.X ? Vector3.up :
                        axis == AxesUtils.Axis.Y ? Vector3.forward : Vector3.right));
                    UnityEditor.Handles.color = color + new Color(0f, 0f, 0f, alpha1R);
                    UnityEditor.Handles.DrawLine(focusPoint + r180 * p1, focusPoint + r180 * p2);
                    UnityEditor.Handles.color = color + new Color(0f, 0f, 0f, alpha2R);
                    UnityEditor.Handles.DrawLine(focusPoint - r180 * p1, focusPoint - r180 * p2);
                    UnityEditor.Handles.color = color + new Color(0f, 0f, 0f, alpha3R);
                    UnityEditor.Handles.DrawLine(focusPoint + r180 * p3, focusPoint + r180 * p4);
                    UnityEditor.Handles.color = color + new Color(0f, 0f, 0f, alpha4R);
                    UnityEditor.Handles.DrawLine(focusPoint - r180 * p3, focusPoint - r180 * p4);
                }
            }
        }

        private static int GetMaxCells(AxesUtils.Axis axis, Vector3 focusPoint, UnityEditor.SceneView sceneView,
            out Vector3 snapSize)
        {
            snapSize = SnapManager.settings.radialGridEnabled ? Vector3.one * SnapManager.settings.radialStep
                : SnapManager.settings.step;
            var rotation = SnapManager.settings.rotation;

            var guiDistance = (UnityEditor.HandleUtility.WorldToGUIPoint(focusPoint)
                - UnityEditor.HandleUtility.WorldToGUIPoint(focusPoint + rotation * snapSize)).magnitude;

            const int minGuidistance = 30;
            if (guiDistance < minGuidistance) snapSize *= Mathf.Round(minGuidistance / guiDistance);
            int maxCells = 10;

            var halfSize = new Vector3(
                axis == AxesUtils.Axis.X ? 0f : maxCells * snapSize.x,
                axis == AxesUtils.Axis.Y ? 0f : maxCells * snapSize.y,
                axis == AxesUtils.Axis.Z ? 0f : maxCells * snapSize.z);

            var axis1Vector = rotation * (axis == AxesUtils.Axis.X ? Vector3.forward
                : axis == AxesUtils.Axis.Y ? Vector3.right : Vector3.up);
            var axis2Vector = rotation * (axis == AxesUtils.Axis.X ? Vector3.up
                : axis == AxesUtils.Axis.Y ? Vector3.forward : Vector3.right);

            var gridAxes = new Vector2[]
            {
                UnityEditor.HandleUtility.WorldToGUIPoint(focusPoint - Vector3.Scale(halfSize, axis1Vector)),
                UnityEditor.HandleUtility.WorldToGUIPoint(focusPoint + Vector3.Scale(halfSize, axis1Vector)),
                UnityEditor.HandleUtility.WorldToGUIPoint(focusPoint - Vector3.Scale(halfSize, axis2Vector)),
                UnityEditor.HandleUtility.WorldToGUIPoint(focusPoint + Vector3.Scale(halfSize, axis2Vector))
            };

            var gridMax = new Vector2(
                Mathf.Max(gridAxes[0].x, gridAxes[1].x, gridAxes[2].x, gridAxes[3].x),
                Mathf.Max(gridAxes[0].y, gridAxes[1].y, gridAxes[2].y, gridAxes[3].y));
            var gridMin = new Vector2(
                Mathf.Min(gridAxes[0].x, gridAxes[1].x, gridAxes[2].x, gridAxes[3].x),
                Mathf.Min(gridAxes[0].y, gridAxes[1].y, gridAxes[2].y, gridAxes[3].y));

            var gridSizeOnGUI = gridMax - gridMin;
            var diff = sceneView.position.size - gridSizeOnGUI;

            if (diff.x > 0 || diff.y > 0)
            {
                float maxRatio = float.MinValue;
                if (diff.x > 0) maxRatio = sceneView.position.size.x / gridSizeOnGUI.x;
                if (diff.y > 0)
                {
                    float ratio = sceneView.position.size.y / gridSizeOnGUI.y;
                    if (ratio > maxRatio) maxRatio = ratio;
                }
                maxCells = Mathf.CeilToInt((float)maxCells * maxRatio);
                if (maxCells > 30)
                {
                    var maxCellsRatio = Mathf.CeilToInt((float)maxCells / 30f);
                    snapSize = snapSize * maxCellsRatio;
                    maxCells = 30;
                }
            }
            return maxCells;
        }
        private static bool GridRaycast(Ray ray, out RaycastHit hitInfo)
        {
            hitInfo = new RaycastHit();
            var plane = new Plane(SnapManager.settings.rotation * (SnapManager.settings.gridOnX ? Vector3.right
                : SnapManager.settings.gridOnY ? Vector3.up : Vector3.forward), SnapManager.settings.origin);
            if (Vector3.Cross(ray.direction, plane.normal).magnitude < 0.000001)
                plane = new Plane(ray.direction, SnapManager.settings.origin);
            if (plane.Raycast(ray, out float distance))
            {
                hitInfo.normal = plane.normal;
                hitInfo.point = ray.GetPoint(distance);
                return true;
            }
            return false;
        }
        #endregion
        #region RADIAL GRID
        private static void DrawRadialGrid(AxesUtils.Axis axis, UnityEditor.SceneView sceneView, int maxCells, float snapSize)
        {
            var rotation = SnapManager.settings.rotation;
            var otherAxes = AxesUtils.GetOtherAxes(axis);
            var normal = rotation * AxesUtils.GetVector(1, axis);
            var tangent = rotation * AxesUtils.GetVector(1, otherAxes[0]);
            var bitangent = rotation * AxesUtils.GetVector(1, otherAxes[1]);
            float radius = 0f;
            for (int i = 1; i < maxCells; ++i)
            {
                radius += snapSize;
                var alpha = (maxCells - i) * 0.5f / (float)maxCells;
                switch (axis)
                {
                    case AxesUtils.Axis.X:
                        UnityEditor.Handles.color = new Color(1f, 0.5f, 0.5f, alpha);
                        break;
                    case AxesUtils.Axis.Y:
                        UnityEditor.Handles.color = new Color(0.5f, 1f, 0.5f, alpha);
                        break;
                    case AxesUtils.Axis.Z:
                        UnityEditor.Handles.color = new Color(0.5f, 0.5f, 1f, alpha);
                        break;
                }
                DrawGridCricle(SnapManager.settings.origin, normal, tangent, bitangent, radius);

                for (int j = 0; j < SnapManager.settings.radialSectors; ++j)
                {
                    var radians = TAU * j / SnapManager.settings.radialSectors;
                    var tangentDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                    var worldDir = TangentSpaceToWorld(tangent, bitangent, tangentDir);
                    var points = new Vector3[]
                    {
                    SnapManager.settings.origin + (worldDir * (radius - snapSize)),
                    SnapManager.settings.origin + (worldDir * (radius))
                    };
                    UnityEditor.Handles.DrawAAPolyLine(1, points);
                }
            }
        }

        private static void DrawGridCricle(Vector3 center, Vector3 normal,
            Vector3 tangent, Vector3 bitangent, float radius)
        {
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            const float polygonSideSize = 0.3f;
            const int minPolygonSides = 12;
            const int maxPolygonSides = 60;
            var polygonSides = Mathf.Clamp((int)(TAU * radius / polygonSideSize), minPolygonSides, maxPolygonSides);

            var periPoints = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < polygonSides; ++i)
            {
                var radians = TAU * i / (polygonSides - 1f);
                var tangentDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                var worldDir = TangentSpaceToWorld(tangent, bitangent, tangentDir);
                var periPoint = center + (worldDir * (radius));
                periPoints.Add(periPoint);
            }
            UnityEditor.Handles.DrawAAPolyLine(4 * UnityEditor.Handles.color.a, periPoints.ToArray());
        }

        private static bool _gridShorcutEnabled = false;
        public static bool gridShorcutEnabled => _gridShorcutEnabled;
        private static SceneViewCameraOrientation.Orientation _cameraOrientation
            = SceneViewCameraOrientation.Orientation.NotAligned;
        private static AxesUtils.Axis _nonAlignedAxis = AxesUtils.Axis.Y;
        public static void SetAxis(AxesUtils.Axis axis) => _nonAlignedAxis = axis;
        private static void GridDuringSceneGui(UnityEditor.SceneView sceneView)
        {
            if (PWBSettings.shortcuts.gridEnableShortcuts.Check())
            {
                if (!_gridShorcutEnabled)
                {
                    _gridShorcutEnabled = true;
                    Event.current.Use();
                }
            }
            void MoveGridOrigin(AxesUtils.SignedAxis forwardAxis)
            {
                var fw = SnapManager.settings.rotation * forwardAxis;
                var stepSize = SnapManager.settings.radialGridEnabled ? SnapManager.settings.radialStep
                : AxesUtils.GetAxisValue(SnapManager.settings.step, forwardAxis);
                SnapManager.settings.origin += fw * stepSize;
                _gridShorcutEnabled = false;
            }
            if (PWBSettings.shortcuts.gridToggle.Check())
            {
                SnapManager.settings.visibleGrid = !SnapManager.settings.visibleGrid;
                _gridShorcutEnabled = false;
            }
            else if (PWBSettings.shortcuts.gridToggleSnaping.Check()
                && (!PWBSettings.shortcuts.gridToggleSnaping.firstStepEnabled || _gridShorcutEnabled))
            {
                SnapManager.settings.snappingEnabled = !SnapManager.settings.snappingEnabled;
                _gridShorcutEnabled = false;
            }
            else if (PWBSettings.shortcuts.gridToggleLock.Check()
                && (!PWBSettings.shortcuts.gridToggleLock.firstStepEnabled || _gridShorcutEnabled))
            {
                SnapManager.settings.lockedGrid = !SnapManager.settings.lockedGrid;
                _gridShorcutEnabled = false;
            }
            else if (PWBSettings.shortcuts.gridSetOriginPosition.Check() && UnityEditor.Selection.activeTransform != null
                && (!PWBSettings.shortcuts.gridSetOriginPosition.firstStepEnabled || _gridShorcutEnabled))
            {
                SnapManager.settings.origin = UnityEditor.Selection.activeTransform.position;
                SnapManager.settings.showPositionHandle = true;
                _gridShorcutEnabled = false;
            }
            else if (PWBSettings.shortcuts.gridSetOriginRotation.Check() && UnityEditor.Selection.activeTransform != null
                && (!PWBSettings.shortcuts.gridSetOriginRotation.firstStepEnabled || _gridShorcutEnabled))
            {
                SnapManager.settings.rotation = UnityEditor.Selection.activeTransform.rotation;
                SnapManager.settings.showRotationHandle = true;
                _gridShorcutEnabled = false;
            }
            else if (PWBSettings.shortcuts.gridSetSize.Check() && UnityEditor.Selection.activeTransform != null
                && (!PWBSettings.shortcuts.gridSetSize.firstStepEnabled || _gridShorcutEnabled))
            {
                SnapManager.settings.step = BoundsUtils.GetBounds(UnityEditor.Selection.activeTransform,
                    UnityEditor.Selection.activeTransform.rotation).size;
                SnapManager.settings.showScaleHandle = true;
                _gridShorcutEnabled = false;
            }
            else if (PWBSettings.shortcuts.gridFrameOrigin.Check()
                && (!PWBSettings.shortcuts.gridFrameOrigin.firstStepEnabled || _gridShorcutEnabled))
            {
                SnapManager.FrameGridOrigin();
                _gridShorcutEnabled = false;
            }
            else if (PWBSettings.shortcuts.gridTogglePositionHandle.Check()
                && (!PWBSettings.shortcuts.gridTogglePositionHandle.firstStepEnabled || _gridShorcutEnabled))
            {
                SnapManager.ToggleGridPositionHandle();
                _gridShorcutEnabled = false;
            }
            else if (PWBSettings.shortcuts.gridToggleRotationHandle.Check()
                && (!PWBSettings.shortcuts.gridToggleRotationHandle.firstStepEnabled || _gridShorcutEnabled))
            {
                SnapManager.ToggleGridRotationHandle();
                _gridShorcutEnabled = false;
            }
            else if (PWBSettings.shortcuts.gridToggleSpacingHandle.Check()
                && (!PWBSettings.shortcuts.gridToggleSpacingHandle.firstStepEnabled || _gridShorcutEnabled))
            {
                SnapManager.ToggleGridScaleHandle();
                _gridShorcutEnabled = false;
            }
            else if (PWBSettings.shortcuts.gridMoveOriginUp.Check()
                && (!PWBSettings.shortcuts.gridMoveOriginUp.firstStepEnabled || _gridShorcutEnabled))
            {
                MoveGridOrigin(AxesUtils.SignedAxis.UP);
            }
            else if (PWBSettings.shortcuts.gridMoveOriginDown.Check()
                && (!PWBSettings.shortcuts.gridMoveOriginDown.firstStepEnabled || _gridShorcutEnabled))
            {
                MoveGridOrigin(AxesUtils.SignedAxis.DOWN);
            }
            else if (PWBSettings.shortcuts.gridNextOrigin.Check()
                && (!PWBSettings.shortcuts.gridNextOrigin.firstStepEnabled || _gridShorcutEnabled))
            {
                SnapManager.settings.SetNextOrigin();
                SnapSettingsWindow.RepaintWindow();
            }
            else if (PWBSettings.shortcuts.snapToggleBoundsSnapping.Check())
            {
                SnapManager.settings.boundsSnapping = !SnapManager.settings.boundsSnapping;
            }

            if (!SnapManager.settings.visibleGrid)
            {
                return;
            }
            var originOffset = SnapManager.settings.origin;
            var rotation = SnapManager.settings.rotation;
            
            var camOrientation = SceneViewCameraOrientation.GetSceneViewOrientation();

            if (camOrientation == SceneViewCameraOrientation.Orientation.NotAligned)
            {
                switch (_nonAlignedAxis)
                {
                    case AxesUtils.Axis.Y:
                        SnapManager.settings.gridOnY = true;
                        break;
                    case AxesUtils.Axis.X:
                        SnapManager.settings.gridOnX = true;
                        break;
                    case AxesUtils.Axis.Z:
                        SnapManager.settings.gridOnZ = true;
                        break;
                }
            }
            else
            {
                if(_cameraOrientation != camOrientation)
                    _nonAlignedAxis = SnapManager.settings.gridOnX ? AxesUtils.Axis.X
                    : SnapManager.settings.gridOnY ? AxesUtils.Axis.Y : AxesUtils.Axis.Z;
                switch (camOrientation)
                {
                    case SceneViewCameraOrientation.Orientation.Top:
                    case SceneViewCameraOrientation.Orientation.Bottom:
                        SnapManager.settings.gridOnY = true;
                        break;
                    case SceneViewCameraOrientation.Orientation.Left:
                    case SceneViewCameraOrientation.Orientation.Right:
                        SnapManager.settings.gridOnX = true;
                        break;
                    case SceneViewCameraOrientation.Orientation.Front:
                    case SceneViewCameraOrientation.Orientation.Back:
                        SnapManager.settings.gridOnZ = true;
                        break;
                }
            }
            _cameraOrientation = camOrientation;
            var axis = SnapManager.settings.gridOnX ? AxesUtils.Axis.X
                : SnapManager.settings.gridOnY ? AxesUtils.Axis.Y : AxesUtils.Axis.Z;




            var camRay = new Ray(sceneView.camera.transform.position, sceneView.camera.transform.forward);
            var plane = new Plane(rotation * (axis == AxesUtils.Axis.X ? Vector3.right
                : axis == AxesUtils.Axis.Y ? Vector3.up : Vector3.forward), originOffset);
            Vector3 focusPoint;
            if (plane.Raycast(camRay, out float distance)) focusPoint = camRay.GetPoint(distance);
            else return;

            if (!SnapManager.settings.radialGridEnabled && SnapManager.settings.drawGridAsTexture)
                DrawGridQuad(axis, sceneView);
            else
            {
                var snapSize = SnapManager.settings.step;
                var maxCells = GetMaxCells(axis, focusPoint, sceneView, out snapSize);
                var snapStepFactor = new Vector3(
                    snapSize.x / SnapManager.settings.step.x,
                    snapSize.y / SnapManager.settings.step.y,
                    snapSize.z / SnapManager.settings.step.z);
                focusPoint = SnapPosition(focusPoint, SnapManager.settings.snappingEnabled, false, snapStepFactor, true);

                if (SnapManager.settings.radialGridEnabled) DrawRadialGrid(axis, sceneView, maxCells, snapSize.x);
                else DrawGrid(axis, focusPoint, maxCells, snapSize);
            }
            GridHandles();
        }
        #endregion
    }
    #endregion
}