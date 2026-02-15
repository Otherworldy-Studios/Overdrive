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
    public class LineSettings : PaintOnSurfaceToolSettings, IPaintToolSettings
    {
        public enum SpacingType { BOUNDS, CONSTANT }

        [SerializeField] private Vector3 _projectionDirection = Vector3.down;
        [SerializeField] protected bool _objectsOrientedAlongTheLine = true;
        [SerializeField] private AxesUtils.Axis _axisOrientedAlongTheLine = AxesUtils.Axis.X;
        [SerializeField] private SpacingType _spacingType = SpacingType.BOUNDS;
        [SerializeField] private float _gapSize = 0f;
        [SerializeField] private float _spacing = 10f;


        public Vector3 projectionDirection
        {
            get => _projectionDirection;
            set
            {
                if (_projectionDirection == value) return;
                _projectionDirection = value;
                OnDataChanged();
            }
        }
        public void UpdateProjectDirection(Vector3 value) => _projectionDirection = value;

        public virtual bool objectsOrientedAlongTheLine
        {
            get => _objectsOrientedAlongTheLine;
            set
            {
                if (_objectsOrientedAlongTheLine == value) return;
                _objectsOrientedAlongTheLine = value;
                OnDataChanged();
            }
        }

        public AxesUtils.Axis axisOrientedAlongTheLine
        {
            get => _axisOrientedAlongTheLine;
            set
            {
                if (_axisOrientedAlongTheLine == value) return;
                _axisOrientedAlongTheLine = value;
                OnDataChanged();
            }
        }

        public SpacingType spacingType
        {
            get => _spacingType;
            set
            {
                if (_spacingType == value) return;
                _spacingType = value;
                OnDataChanged();
            }
        }

        public float spacing
        {
            get => _spacing;
            set
            {
                value = Mathf.Max(value, 0.01f);
                if (_spacing == value) return;
                _spacing = value;
                OnDataChanged();
            }
        }

        public float gapSize
        {
            get => _gapSize;
            set
            {
                if (_gapSize == value) return;
                _gapSize = value;
                OnDataChanged();
            }
        }

        [SerializeField] private PaintToolSettings _paintTool = new PaintToolSettings();
        public Transform parent { get => _paintTool.parent; set => _paintTool.parent = value; }
        public bool overwritePrefabLayer
        { get => _paintTool.overwritePrefabLayer; set => _paintTool.overwritePrefabLayer = value; }
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
        { get => _paintTool.overwriteBrushProperties; set => _paintTool.overwriteBrushProperties = value; }
        public BrushSettings brushSettings => _paintTool.brushSettings;
        public bool overwriteParentingSettings
        {
            get => _paintTool.overwriteParentingSettings;
            set => _paintTool.overwriteParentingSettings = value;
        }
        public IToolParentingSettings GetParentingSettings() => _paintTool as ToolParentingSettings;
        public LineSettings() : base() => _paintTool.OnDataChanged += DataChanged;

        public override void DataChanged()
        {
            base.DataChanged();
            UpdateStroke();
            UnityEditor.SceneView.RepaintAll();
        }

        protected virtual void UpdateStroke() => PWBIO.UpdateStroke();

        public override void Copy(IToolSettings other)
        {
            var otherLineSettings = other as LineSettings;
            if (otherLineSettings == null) return;
            base.Copy(other);
            _projectionDirection = otherLineSettings._projectionDirection;
            _objectsOrientedAlongTheLine = otherLineSettings._objectsOrientedAlongTheLine;
            _axisOrientedAlongTheLine = otherLineSettings._axisOrientedAlongTheLine;
            _spacingType = otherLineSettings._spacingType;
            _spacing = otherLineSettings._spacing;
            _paintTool.Copy(otherLineSettings._paintTool);
            _gapSize = otherLineSettings._gapSize;
        }
    }

    [System.Serializable]
    public class LineSegment
    {
        public enum SegmentType { STRAIGHT, CURVE }
        public SegmentType type = SegmentType.CURVE;
        [SerializeField]
        private System.Collections.Generic.List<LinePoint> _linePoints = new System.Collections.Generic.List<LinePoint>();

        public Vector3[] points => _linePoints.Select(p => p.position).ToArray();
        public float[] scales => _linePoints.Select(p => p.scale).ToArray();

        public void AddPoint(Vector3 position, float scale = 0.25f) => _linePoints.Add(new LinePoint(position, scale));
    }

    [System.Serializable]
    public class LinePoint : ControlPoint
    {
        public LineSegment.SegmentType type = LineSegment.SegmentType.CURVE;
        public float scale = 0.25f;
        public LinePoint() { }
        public LinePoint(Vector3 position = new Vector3(), float scale = 0.25f,
             LineSegment.SegmentType type = LineSegment.SegmentType.CURVE)
            : base(position) => (this.type, this.scale) = (type, scale);
        public LinePoint(LinePoint other) : base((ControlPoint)other) => Copy(other);
        public override void Copy(ControlPoint other)
        {
            base.Copy(other);
            var otherLinePoint = other as LinePoint;
            if (otherLinePoint == null) return;
            type = otherLinePoint.type;
            scale = otherLinePoint.scale;
        }
    }

    [System.Serializable]
    public class LineData : PersistentData<LineToolName, LineSettings, LinePoint>
    {
        [SerializeField] private bool _closed = false;

        private float _lenght = 0f;
        private System.Collections.Generic.List<Vector3> _midpoints = new System.Collections.Generic.List<Vector3>();
        private System.Collections.Generic.List<Vector3> _pathPoints = new System.Collections.Generic.List<Vector3>();
        private System.Collections.Generic.List<Vector3> _onSurfacePathPoints = new System.Collections.Generic.List<Vector3>();
        public override ToolManager.ToolState state
        {
            get => base.state;
            set
            {
                if (state == value) return;
                base.state = value;
                UpdatePath(forceUpdate: false, updateOnSurfacePoints: false);
            }
        }
        public override bool SetPoint(int idx, Vector3 value, bool registerUndo, bool selectAll, bool moveSelection = true)
        {
            if (base.SetPoint(idx, value, registerUndo, selectAll, moveSelection))
            {
                UpdatePath(forceUpdate: false, updateOnSurfacePoints: true);
                return true;
            }
            return false;
        }

        public void SetRotatedPoint(int idx, Vector3 value, bool registerUndo)
            => base.SetPoint(idx, value, registerUndo, selectAll: false, moveSelection: false);
        public void AddPoint(Vector3 point, bool registerUndo = true)
        {
            var linePoint = new LinePoint(point);
            base.AddPoint(linePoint, registerUndo);
            UpdatePath(forceUpdate: false, updateOnSurfacePoints: true);
        }

        protected override void UpdatePoints(bool deserializing = false)
        {
            base.UpdatePoints();
            UpdatePath(forceUpdate: false, updateOnSurfacePoints: !deserializing);
            if (!deserializing && ToolManager.editMode) PWBIO.ApplyPersistentLine(this);
        }
        public void ToggleSegmentType()
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            for (int i = 0; i < _selection.Count; ++i)
            {
                var idx = _selection[i];
                _controlPoints[idx].type = _controlPoints[idx].type == LineSegment.SegmentType.CURVE
                    ? LineSegment.SegmentType.STRAIGHT : LineSegment.SegmentType.CURVE;
            }
        }
        public LineSegment[] GetSegments()
        {
            var segments = new System.Collections.Generic.List<LineSegment>();
            if (_controlPoints == null || _controlPoints.Count == 0) return segments.ToArray();
            var type = _controlPoints[0].type;
            for (int i = 0; i < pointsCount; ++i)
            {
                var segment = new LineSegment();
                segments.Add(segment);
                segment.type = type;
                segment.AddPoint(_controlPoints[i].position);

                do
                {
                    ++i;
                    if (i >= pointsCount) break;
                    type = _controlPoints[i].type;
                    if (type == segment.type) segment.AddPoint(_controlPoints[i].position);
                } while (type == segment.type);
                if (i >= pointsCount) break;
                i -= 2;
            }
            if (_closed)
            {
                if (_controlPoints[0].type == _controlPoints.Last().type)
                    segments.Last().AddPoint(_controlPoints[0].position);
                else
                {
                    var segment = new LineSegment();
                    segment.type = _controlPoints[0].type;
                    segment.AddPoint(_controlPoints.Last().position);
                    segment.AddPoint(_controlPoints[0].position);
                    segments.Add(segment);
                }
            }
            return segments.ToArray();
        }

        public void ToggleClosed()
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            _closed = !_closed;
        }

        public bool closed
        {
            get => _closed;
            set => _closed = value;
        }

        public override void ToggleSelection()
        {
            base.ToggleSelection();
            if (isSelected) SelectAll();
            else ClearSelection();
            UnityEditor.SceneView.RepaintAll();
        }
        protected override void Initialize()
        {
            base.Initialize();
            for (int i = 0; i < 2; ++i) _controlPoints.Add(new LinePoint(Vector3.zero));
            deserializing = true;
            UpdatePoints(deserializing);
            deserializing = false;
        }
        public LineData() : base() { }
        public LineData((GameObject, int)[] objects, long initialBrushId, LineData lineData)
            : base(objects, initialBrushId, lineData) { }

        private static LineData _instance = null;
        public static LineData instance
        {
            get
            {
                if (_instance == null) _instance = new LineData();
                if (_instance.points == null || _instance.points.Length == 0)
                {
                    _instance.Initialize();
                    _instance._settings = LineManager.settings;
                }
                return _instance;
            }
        }

        private void CopyLineData(LineData other)
        {
            _closed = other._closed;
            _lenght = other.lenght;
            _midpoints = other._midpoints.ToList();
            _pathPoints = other._pathPoints.ToList();
        }

        public LineData Clone()
        {
            var clone = new LineData();
            base.Clone(clone);
            clone.CopyLineData(this);
            return clone;
        }
        public override void Copy(PersistentData<LineToolName, LineSettings, LinePoint> other)
        {
            base.Copy(other);
            var otherLineData = other as LineData;
            if (otherLineData == null) return;
            CopyLineData(otherLineData);
        }
        private float GetLineLength(Vector3[] points, out float[] lengthFromFirstPoint)
        {
            float lineLength = 0f;
            lengthFromFirstPoint = new float[points.Length];
            var segmentLength = new float[points.Length];
            lengthFromFirstPoint[0] = 0f;
            for (int i = 1; i < points.Length; ++i)
            {
                segmentLength[i - 1] = (points[i] - points[i - 1]).magnitude;
                lineLength += segmentLength[i - 1];
                lengthFromFirstPoint[i] = lineLength;
            }
            return lineLength;
        }

        private Vector3[] GetLineMidpoints(Vector3[] points)
        {
            if (points.Length == 0) return new Vector3[0];
            var midpoints = new System.Collections.Generic.List<Vector3>();
            var subSegments = new System.Collections.Generic.List<System.Collections.Generic.List<Vector3>>();
            var pathPoints = _pointPositions;
            bool IsAPathPoint(Vector3 point) => pathPoints.Contains(point);
            subSegments.Add(new System.Collections.Generic.List<Vector3>());
            subSegments.Last().Add(points[0]);
            for (int i = 1; i < points.Length - 1; ++i)
            {
                var point = points[i];
                subSegments.Last().Add(point);
                if (IsAPathPoint(point))
                {
                    subSegments.Add(new System.Collections.Generic.List<Vector3>());
                    subSegments.Last().Add(point);
                }
            }
            subSegments.Last().Add(points.Last());
            Vector3 GetLineMidpoint(Vector3[] subSegmentPoints)
            {
                var midpoint = subSegmentPoints[0];
                float[] lengthFromFirstPoint = null;
                var halfLineLength = GetLineLength(subSegmentPoints, out lengthFromFirstPoint) / 2f;
                for (int i = 1; i < subSegmentPoints.Length; ++i)
                {
                    if (lengthFromFirstPoint[i] < halfLineLength) continue;
                    var dir = (subSegmentPoints[i] - subSegmentPoints[i - 1]).normalized;
                    var localLength = halfLineLength - lengthFromFirstPoint[i - 1];
                    midpoint = subSegmentPoints[i - 1] + dir * localLength;
                    break;
                }
                return midpoint;
            }
            foreach (var subSegment in subSegments) midpoints.Add(GetLineMidpoint(subSegment.ToArray()));
            return midpoints.ToArray();
        }

        public void UpdatePath(bool forceUpdate, bool updateOnSurfacePoints)
        {
            if (!forceUpdate && !ToolManager.editMode && state != ToolManager.ToolState.EDIT) return;
            _lenght = 0;
            _pathPoints.Clear();
            _midpoints.Clear();
            _onSurfacePathPoints.Clear();
            var segments = GetSegments();
            void AddSegmentPoints(System.Collections.Generic.List<Vector3> pointList, Vector3[] newPoints)
            {
                if (pointList.Count > 0 && pointList.Last() == newPoints[0] && newPoints.Length > 1)
                    for (int i = 1; i < newPoints.Length; ++i) pointList.Add(newPoints[i]);
                else pointList.AddRange(newPoints);
            }
            foreach (var segment in segments)
            {
                var segmentPoints = new Vector3[] { };
                if (segment.type == LineSegment.SegmentType.STRAIGHT) segmentPoints = segment.points.ToArray();
                else segmentPoints = (BezierPath.GetBezierPoints(segment.points, segment.scales)).ToArray();
                AddSegmentPoints(_pathPoints, segmentPoints);
                if (segmentPoints.Length == 0) continue;
                var midpoints = GetLineMidpoints(segmentPoints);
                AddSegmentPoints(_midpoints, midpoints);
            }
            if (!updateOnSurfacePoints) return;
            var objSet = objectSet;
            for (int i = 0; i < _pathPoints.Count; ++i)
            {
                float distance = 10000f;
                if (ToolManager.tool == ToolManager.PaintTool.LINE && !deserializing)
                {
                    var ray = new Ray(_pathPoints[i] - settings.projectionDirection * distance, settings.projectionDirection);
                    var onSurfacePoint = _pathPoints[i];
                    if (PWBIO.PWBToolRaycast(ray, out RaycastHit hit, out GameObject collider, distance * 2, -1,
                        paintOnPalettePrefabs: false, castOnMeshesWithoutCollider: true, tags: null, terrainLayers: null,
                        exceptions: objSet, sameOriginAsRay: false, origin: _pathPoints[i],
                        createTempColliders: settings.paintOnMeshesWithoutCollider,
                        ignoreSceneColliders: settings.ignoreSceneColliders))
                    {
                        onSurfacePoint = hit.point;
                    }
                    _onSurfacePathPoints.Add(onSurfacePoint);
                }
                if (i == 0) continue;
                _lenght += (_pathPoints[i] - _pathPoints[i - 1]).magnitude;
            }
        }

        public static bool SphereSegmentIntersection(Vector3 segmentStart, Vector3 segmentEnd,
            Vector3 sphereCenter, float sphereRadius, out Vector3 intersection)
        {
            var r = sphereRadius;
            var d = segmentEnd - segmentStart;
            var f = segmentStart - sphereCenter;
            var a = Vector3.Dot(d, d);
            var b = 2 * Vector3.Dot(f, d);
            var c = Vector3.Dot(f, f) - r * r;
            float discriminant = b * b - 4 * a * c;
            float t = -1;
            intersection = segmentStart;
            if (discriminant < 0) return false;
            else
            {
                discriminant = Mathf.Sqrt(discriminant);
                var t1 = (-b - discriminant) / (2 * a);
                var t2 = (-b + discriminant) / (2 * a);
                if (t1 >= 0 && t1 <= 1 && t1 > t2) t = t1;
                else if (t2 >= 0 && t2 <= 1 && t2 > t1) t = t2;
            }
            if (t == -1) return false;
            intersection += d * t;
            return true;
        }
        public static Vector3 NearestPathPoint(int startSegmentIdx, Vector3 startPoint, float minPathLenght,
            Vector3[] pathPoints, out int nearestPointIdx, out float distanceFromNearestPoint)
        {
            nearestPointIdx = pathPoints.Length - 1;
            var result = pathPoints.Last();
            distanceFromNearestPoint = 0f;
            startSegmentIdx = Mathf.Max(startSegmentIdx, 1);
            for (int i = startSegmentIdx; i < pathPoints.Length; ++i)
            {
                var start = pathPoints[i - 1];
                var end = pathPoints[i];
                if(i == pathPoints.Length -1)
                {
                    end = (end - start) * 1000 + start;
                }
                if (SphereSegmentIntersection(start, end, startPoint, minPathLenght, out Vector3 intersection))
                {
                    result = intersection;
                    nearestPointIdx = i - 1;
                    distanceFromNearestPoint = (intersection - pathPoints[nearestPointIdx]).magnitude;
                    return result;
                }
            }

            return result;
        }

        public float lenght => _lenght;
        public Vector3[] pathPoints => _pathPoints.ToArray();
        public Vector3[] onSurfacePathPoints => _onSurfacePathPoints.ToArray();
        public Vector3 lastPathPoint => _pathPoints.Last();
        public Vector3[] midpoints => _midpoints.ToArray();
        public Vector3 lastTangentPos { get; set; }

        public bool showHandles { get; set; }
    }

    public class LineToolName : IToolName { public string value => "Line"; }

    [System.Serializable]
    public class LineSceneData : SceneData<LineToolName, LineSettings, LinePoint, LineData>
    {
        public LineSceneData() : base() { }
        public LineSceneData(string sceneGUID) : base(sceneGUID) { }
    }

    [System.Serializable]
    public class LineManager : PersistentToolManagerBase<LineToolName, LineSettings, LinePoint, LineData, LineSceneData>
    {
        public enum EditModeType
        {
            NODES,
            LINE_POSE
        }
        public static EditModeType editModeType { get; set; }
        public static void ToggleEditModeType()
        {
            editModeType = editModeType == EditModeType.NODES ? EditModeType.LINE_POSE : EditModeType.NODES;
            ToolProperties.RepainWindow();
        }
    }
    #endregion

    #region PWBIO
    public static partial class PWBIO
    {
        #region HANDLERS
        private static void LineInitializeOnLoad()
        {
            LineManager.settings.OnDataChanged += OnLineSettingsChanged;
            BrushSettings.OnBrushSettingsChanged += PreviewSelectedPersistentLines;
        }
        private static void OnLineToolModeChanged()
        {
            DeselectPersistentLines();
            if (!ToolManager.editMode)
            {
                ToolProperties.RepainWindow();
                return;
            }
            ResetLineState();
            ResetSelectedPersistentLine();
            LineManager.editModeType = LineManager.EditModeType.NODES;
        }
        private static void OnLineSettingsChanged()
        {
            repaint = true;
            if (!ToolManager.editMode)
            {
                _lineData.settings = LineManager.settings;
                updateStroke = true;
                return;
            }
            if (_selectedPersistentLineData == null) return;
            _selectedPersistentLineData.settings.Copy(LineManager.settings);
            PreviewPersistentLine(_selectedPersistentLineData);
        }
        private static void OnUndoLine() => ClearLineStroke();
        #endregion

        #region SPAWN MODE
        public static void InitializeLineTool()
        {
            LineManager.instance.DeselectAllItems();
            ResetLineState(false);
            PWBCore.staticData.VersionUpdate();
            if (LineManager.settings.paintOnMeshesWithoutCollider)
            {
                if (LineManager.settings.ignoreSceneColliders) UpdateSceneColliderSet();
                UpdateOctree();
            }
        }
        public static void ResetLineState(bool askIfWantToSave = true)
        {
            if (_lineData.state == ToolManager.ToolState.NONE) return;
            if (askIfWantToSave)
            {
                void Save()
                {
                    if (UnityEditor.SceneView.lastActiveSceneView != null)
                        LineStrokePreview(UnityEditor.SceneView.lastActiveSceneView, _lineData,
                            persistent: false, forceUpdate: true, initialIdx: 0);
                    CreateLine();
                }
                AskIfWantToSave(_lineData.state, Save);
            }
            _snappedToVertex = false;
            selectingLinePoints = false;
            _lineData.Reset();
            OnLineSettingsChanged();
        }

        private static void LineStateNone(bool in2DMode)
        {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                _lineData.name = LineData.nextHexId;
                _lineData.closed = false;
                _lineData.state = ToolManager.ToolState.PREVIEW;
                Event.current.Use();
            }
            if (MouseDot(out Vector3 point, out Vector3 normal, LineManager.settings.mode, in2DMode,
                LineManager.settings.paintOnPalettePrefabs, LineManager.settings.paintOnMeshesWithoutCollider, false,
                ignoreSceneColliders: LineManager.settings.ignoreSceneColliders))
            {
                point = SnapToBounds(point);
                point = _snapToVertex ? LinePointSnapping(point)
                    : SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                    LineManager.settings.paintOnPalettePrefabs, LineManager.settings.paintOnMeshesWithoutCollider,
                    LineManager.settings.ignoreSceneColliders, paintOnTheGrid: false, Vector3.down);
                _lineData.SetPoint(0, point, registerUndo: false, selectAll: false);
                _lineData.SetPoint(1, point, registerUndo: false, selectAll: false);
            }
            DrawDotHandleCap(_lineData.GetPoint(0));
        }

        private static void LineStateStraightLine(bool in2DMode)
        {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                _lineData.state = ToolManager.ToolState.EDIT;
                updateStroke = true;
            }
            if (MouseDot(out Vector3 point, out Vector3 normal, LineManager.settings.mode, in2DMode,
                LineManager.settings.paintOnPalettePrefabs, LineManager.settings.paintOnMeshesWithoutCollider, false,
                ignoreSceneColliders: LineManager.settings.ignoreSceneColliders))
            {
                point = SnapToBounds(point);
                point = _snapToVertex ? LinePointSnapping(point)
                    : SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                    LineManager.settings.paintOnPalettePrefabs, LineManager.settings.paintOnMeshesWithoutCollider,
                    LineManager.settings.ignoreSceneColliders, paintOnTheGrid: false, Vector3.down);
                _lineData.SetPoint(1, point, registerUndo: false, selectAll: false);
            }

            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(8, new Vector3[] { _lineData.GetPoint(0), _lineData.GetPoint(1) });
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(4, new Vector3[] { _lineData.GetPoint(0), _lineData.GetPoint(1) });
            DrawDotHandleCap(_lineData.GetPoint(0));
            DrawDotHandleCap(_lineData.GetPoint(1));
        }

        private static void LineStateBezier(UnityEditor.SceneView sceneView)
        {
            var pathPoints = _lineData.pathPoints;
            var forceStrokeUpdate = updateStroke;
            if (updateStroke)
            {
                _lineData.UpdatePath(forceUpdate: false, updateOnSurfacePoints: false);
                pathPoints = _lineData.pathPoints;
                BrushstrokeManager.UpdateLineBrushstroke(pathPoints);
                updateStroke = false;
            }
            LineStrokePreview(sceneView, _lineData, persistent: false, forceStrokeUpdate, 0);
            DrawLine(_lineData, drawSurfacePath: true);
            DrawSelectionRectangle();
            LineInput(false, sceneView, false);

            if (selectingLinePoints && !Event.current.control) _lineData.ClearSelection();

            bool clickOnPoint, wasEdited;
            DrawLineControlPoints(_lineData, isPersistent: false, showHandles: true,
                out clickOnPoint, out bool multiSelection, out bool addToselection,
                out bool removeFromSelection, out wasEdited, out Vector3 delta);
            if (wasEdited) updateStroke = true;
            SelectionRectangleInput(clickOnPoint);
        }

        private static void CreateLine()
        {
            var nextLineId = LineData.nextHexId;
            var objDic = Paint(LineManager.settings, PAINT_CMD, addTempCollider: true,
                persistent: false, toolObjectId: nextLineId);
            if (objDic.Count != 1) return;

            var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            var sceneGUID = UnityEditor.AssetDatabase.AssetPathToGUID(scenePath);
            if (isInPrefabMode)
                sceneGUID = UnityEditor.AssetDatabase.AssetPathToGUID(prefabStage.assetPath);
            var initialBrushId = PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.id : -1;
            var objs = objDic[nextLineId].ToArray();
            var persistentData = new LineData(objs, initialBrushId, _lineData);
            LineManager.instance.AddPersistentItem(sceneGUID, persistentData);
            PWBItemsWindow.RepainWindow();
        }

        private static void LineStrokePreview(UnityEditor.SceneView sceneView,
            LineData lineData, bool persistent, bool forceUpdate, int initialIdx)
        {
            var settings = lineData.settings;
            var lastPoint = lineData.lastPathPoint;
            var objectCount = lineData.objectCount;
            var lastObjectTangentPosition = lineData.lastTangentPos;

            BrushstrokeItem[] brushstroke = null;

            if (PreviewIfBrushtrokestaysTheSame(out brushstroke, sceneView.camera, forceUpdate)) return;

            if (!persistent) _paintStroke.Clear();
            var idx = initialIdx;
            float maxSurfaceHeight = 0f;
            for (int i = 0; i < brushstroke.Length; ++i)
            {
                var strokeItem = brushstroke[i];
                var prefab = strokeItem.settings.prefab;
                if (prefab == null) continue;
                var bounds = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation);
                BrushSettings brushSettings = strokeItem.settings;
                if (LineManager.settings.overwriteBrushProperties) brushSettings = LineManager.settings.brushSettings;

                var size = Vector3.Scale(bounds.size, strokeItem.scaleMultiplier);

                var pivotToCenter = Vector3.Scale(
                    prefab.transform.InverseTransformDirection(bounds.center - prefab.transform.position),
                    strokeItem.scaleMultiplier);
                var height = size.x + size.y + size.z + maxSurfaceHeight;
                Vector3 segmentDir = Vector3.zero;

                if (settings.objectsOrientedAlongTheLine && brushstroke.Length > 1)
                {
                    segmentDir = i < brushstroke.Length - 1
                        ? strokeItem.nextTangentPosition - strokeItem.tangentPosition
                        : lastPoint - strokeItem.tangentPosition;
                }
                if (brushstroke.Length == 1)
                {
                    segmentDir = lastPoint - brushstroke[0].tangentPosition;
                    if (persistent && objectCount > 0)
                        segmentDir = lastPoint - lastObjectTangentPosition;
                }
                if (i == brushstroke.Length - 1)
                {
                    var onLineSize = AxesUtils.GetAxisValue(size, settings.axisOrientedAlongTheLine)
                        + settings.gapSize;
                    var segmentSize = segmentDir.magnitude;
                    if (segmentSize > onLineSize) segmentDir = segmentDir.normalized
                            * (settings.spacingType == LineSettings.SpacingType.BOUNDS ? onLineSize : settings.spacing);
                }

                var perpendicularToTheSurface = settings.perpendicularToTheSurface
                    || (brushSettings.rotateToTheSurface && !brushSettings.alwaysOrientUp);

                if (settings.objectsOrientedAlongTheLine && !perpendicularToTheSurface)
                {
                    var projectionAxis = ((AxesUtils.SignedAxis)(settings.projectionDirection)).axis;
                    segmentDir -= AxesUtils.GetVector(AxesUtils.GetAxisValue(segmentDir, projectionAxis), projectionAxis);
                }
                var normal = -settings.projectionDirection;
                var otherAxes = AxesUtils.GetOtherAxes((AxesUtils.SignedAxis)(-settings.projectionDirection));
                var tangetAxis = otherAxes[settings.objectsOrientedAlongTheLine ? 0 : 1];
                Vector3 itemTangent = (AxesUtils.SignedAxis)(tangetAxis);
                var itemRotation = Quaternion.LookRotation(itemTangent, normal);
                var lookAt = Quaternion.LookRotation((Vector3)(AxesUtils.SignedAxis)
                    (settings.axisOrientedAlongTheLine), Vector3.up);

                var itemPosition = strokeItem.tangentPosition + segmentDir / 2;

                var ray = new Ray(itemPosition + normal * height, -normal);
                Transform surface = null;
                if (settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (PWBToolRaycast(ray, out RaycastHit itemHit,
                        out GameObject collider, maxDistance: float.MaxValue, layerMask: -1,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider,
                        sameOriginAsRay: false, origin: itemPosition,
                        createTempColliders: settings.paintOnMeshesWithoutCollider,
                        ignoreSceneColliders: settings.ignoreSceneColliders))
                    {
                        itemPosition = itemHit.point;
                        if (perpendicularToTheSurface) normal = itemHit.normal;
                        var colObj = PWBCore.GetGameObjectFromTempCollider(collider);
                        if (colObj != null) surface = colObj.transform;
                        var surfObj = PWBCore.GetGameObjectFromTempCollider(collider);
                        var surfSize = BoundsUtils.GetBounds(surfObj.transform).size;
                        var h = surfSize.x + surfSize.y + surfSize.z;
                        maxSurfaceHeight = Mathf.Max(h, maxSurfaceHeight);
                    }
                    else if (settings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE) continue;
                }

                if (perpendicularToTheSurface && segmentDir != Vector3.zero)
                {
                    if (settings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                    {
                        var bitangent = Vector3.Cross(segmentDir, normal);
                        var lineNormal = Vector3.Cross(bitangent, segmentDir);
                        itemRotation = Quaternion.LookRotation(segmentDir, lineNormal) * lookAt;
                    }
                    else
                    {
                        var plane = new Plane(normal, itemPosition);
                        var tangent = plane.ClosestPointOnPlane(segmentDir + itemPosition) - itemPosition;
                        itemRotation = Quaternion.LookRotation(tangent, normal) * lookAt;
                    }
                }
                else if (!perpendicularToTheSurface && segmentDir != Vector3.zero)
                    itemRotation = Quaternion.LookRotation(segmentDir, normal) * lookAt;
                itemRotation *= Quaternion.Euler(strokeItem.additionalAngle);

                if (!settings.perpendicularToTheSurface && brushSettings.rotateToTheSurface && brushSettings.alwaysOrientUp)
                {
                    var fw = itemRotation * Vector3.forward;
                    const float minMag = 1e-6f;
                    fw.y = 0;
                    if (Mathf.Abs(fw.x) < minMag && Mathf.Abs(fw.z) < minMag) fw = Quaternion.Euler(0, 90, 0) * normal;
                    itemRotation = Quaternion.LookRotation(fw, Vector3.up);
                }

                itemPosition += normal * strokeItem.surfaceDistance;

                itemPosition += itemRotation * brushSettings.localPositionOffset;
                itemPosition -= itemRotation * (pivotToCenter - Vector3.up * (size.y / 2));


                if (brushSettings.embedInSurface
                    && settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (brushSettings.embedAtPivotHeight)
                        itemPosition += itemRotation * new Vector3(0f, strokeItem.settings.bottomMagnitude, 0f);
                    else
                    {
                        var TRS = Matrix4x4.TRS(itemPosition, itemRotation,
                            Vector3.Scale(prefab.transform.localScale, strokeItem.scaleMultiplier));
                        float magnitudeInDirection;
                        var localDirection = Quaternion.Inverse(itemRotation) * -normal;
                        var furthestVertices = strokeItem.settings.GetFurthestVerticesInDirection(localDirection,
                            out magnitudeInDirection);
                        var distanceTosurface = GetDistanceToSurface(furthestVertices, TRS, -normal,
                            Mathf.Abs(magnitudeInDirection), LineManager.settings.paintOnPalettePrefabs,
                            LineManager.settings.paintOnMeshesWithoutCollider, LineManager.settings.ignoreSceneColliders,
                            out Transform surfaceTransform, prefab);
                        itemPosition -= normal * distanceTosurface;
                    }
                }

                var rootToWorld = Matrix4x4.TRS(itemPosition, itemRotation, strokeItem.scaleMultiplier)
                    * Matrix4x4.Rotate(Quaternion.Inverse(prefab.transform.rotation))
                    * Matrix4x4.Translate(-prefab.transform.position);
                var itemScale = Vector3.Scale(prefab.transform.localScale, strokeItem.scaleMultiplier);
                var layer = settings.overwritePrefabLayer ? settings.layer : prefab.layer;

                Transform parentTransform = settings.parent;
                var paintItem = new PaintStrokeItem(prefab, strokeItem.settings.guid, itemPosition, itemRotation,
                    itemScale, layer, parentTransform, surface, strokeItem.flipX, strokeItem.flipY, idx++);
                paintItem.persistentParentId = persistent ? lineData.hexId : LineData.nextHexId;
                _paintStroke.Add(paintItem);
                PreviewBrushItem(prefab, rootToWorld, layer, sceneView.camera,
                    false, false, strokeItem.flipX, strokeItem.flipY);
                var prevData = new PreviewData(prefab, rootToWorld, layer, strokeItem.flipX, strokeItem.flipY);
                _previewData.Add(prevData);
            }
            if (_persistentPreviewData.ContainsKey(lineData.id)) _persistentPreviewData[lineData.id] = _previewData.ToArray();
            else _persistentPreviewData.Add(lineData.id, _previewData.ToArray());
        }
        #endregion

        #region COMMON
        private static LineData _lineData = LineData.instance;
        private static bool _selectingLinePoints = false;
        private static Rect _selectionRect = new Rect();

        private static string _createProfileName = ToolProfile.DEFAULT;
        public static LineData lineData
            => (ToolManager.editMode && _selectedPersistentLineData != null) ? _selectedPersistentLineData : _lineData;
        public static bool selectingLinePoints
        {
            get => _selectingLinePoints;
            set
            {
                if (value == _selectingLinePoints) return;
                _selectingLinePoints = value;
            }
        }
        private static void ClearLineStroke()
        {
            _paintStroke.Clear();
            BrushstrokeManager.ClearBrushstroke();
            if (ToolManager.editMode && _selectedPersistentLineData != null)
            {
                _selectedPersistentLineData.UpdatePath(forceUpdate: true, updateOnSurfacePoints: false);
                PreviewPersistentLine(_selectedPersistentLineData);
                UnityEditor.SceneView.RepaintAll();
                repaint = true;
            }
        }

        private static void LineDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                if (_lineData.state == ToolManager.ToolState.EDIT && _lineData.selectedPointIdx > 0)
                    _lineData.ClearSelection();
                else if (_lineData.state == ToolManager.ToolState.NONE && !ToolManager.editMode)
                    ToolManager.DeselectTool();
                else if (ToolManager.editMode)
                {
                    if (_editingPersistentLine) ResetSelectedPersistentLine();
                    else ToolManager.DeselectTool();
                    DeselectPersistentLines();
                    _initialPersistentLineData = null;
                    _selectedPersistentLineData = null;
                    ToolProperties.RepainWindow();
                    ToolManager.editMode = false;
                }
                else ResetLineState(false);
                OnUndoLine();
                UpdateStroke();
                BrushstrokeManager.ClearBrushstroke();
            }

            LineToolEditMode(sceneView);
            if (ToolManager.editMode) return;

            switch (_lineData.state)
            {
                case ToolManager.ToolState.NONE:
                    LineStateNone(sceneView.in2DMode);
                    break;
                case ToolManager.ToolState.PREVIEW:
                    LineStateStraightLine(sceneView.in2DMode);
                    break;
                case ToolManager.ToolState.EDIT:
                    LineStateBezier(sceneView);
                    break;
            }
        }

        private static Quaternion _lineRotation = Quaternion.identity;

        private static void RotateLineAround(int idx, Quaternion rotation, LineData lineData)
        {
            var pivotPosition = lineData.GetPoint(idx);
            for (int i = 0; i < lineData.pointsCount; ++i)
            {
                if (i == idx) continue;
                var localPositionUnrotated = Quaternion.Inverse(_lineRotation) * (lineData.GetPoint(i) - pivotPosition);
                var localPosition = rotation * localPositionUnrotated;
                lineData.SetRotatedPoint(i, pivotPosition + localPosition, true);
            }
            _lineRotation = rotation;
            lineData.UpdatePath(forceUpdate: false, updateOnSurfacePoints: true);
        }

        public static void ResetLineRotation() => _lineRotation = Quaternion.identity;

        public static void UpdateLinePathAndStroke(LineData data)
        {
            data.UpdatePath(forceUpdate: true, updateOnSurfacePoints: true);
            PWBIO.PreviewPersistentLine(data);
        }

        public static void ApplyPersistentLineAndReset(LineData data)
        {
            data.UpdatePath(forceUpdate: true, updateOnSurfacePoints: true);
            PreviewPersistentLine(data);
            DeleteDisabledObjects();
            ApplyPersistentLine(data);
            _initialPersistentLineData = null;
            _selectedPersistentLineData = null;
            UnityEditor.SceneView.RepaintAll();
        }
        public static void DeleteLinePoints(LineData data, int[] indexes, bool isPersistent)
        {
            if (isPersistent && data.pointsCount - indexes.Length <= 1)
            {
                LineManager.instance.DeletePersistentItem(data.id, deleteObjects: true);
                UnityEditor.SceneView.RepaintAll();
                return;
            }
            data.RemovePoints(indexes);
            if (isPersistent) ApplyPersistentLineAndReset(data);
            if (data.pointsCount >= 2) updateStroke = true;
        }
        public static void ShowLineContextMenu(LineData data, bool isPersistent, Vector2 mousePosition, int pointIdx)
        {
            if (isPersistent && !ToolManager.editMode) return;
            var menu = new UnityEditor.GenericMenu();
           
            menu.AddItem(new GUIContent("Delete point ... Delete"), on: false, () =>
            {
                if (isPersistent && data.pointsCount <= 2)
                {
                    LineManager.instance.DeletePersistentItem(data.id, deleteObjects: true);
                    UnityEditor.SceneView.RepaintAll();
                    return;
                }
                data.RemovePoint(pointIdx);
                if (isPersistent)
                {
                    data.UpdatePath(forceUpdate: true, updateOnSurfacePoints: true);
                    PreviewPersistentLine(data);
                    DeleteDisabledObjects();
                    ApplyPersistentLine(data);
                    _initialPersistentLineData = null;
                    _selectedPersistentLineData = null;
                }
                if (data.pointsCount >= 2) updateStroke = true;
            });
            menu.AddItem(new GUIContent("Delete selected points ... Delete"), on: false, () =>
            {
                if (isPersistent && data.pointsCount - data.selectionCount <= 1)
                {
                    LineManager.instance.DeletePersistentItem(data.id, deleteObjects: true);
                    UnityEditor.SceneView.RepaintAll();
                    return;
                }
                data.RemoveSelectedPoints();
                if (isPersistent)
                {
                    data.UpdatePath(forceUpdate: true, updateOnSurfacePoints: true);
                    PreviewPersistentLine(data);
                    DeleteDisabledObjects();
                    ApplyPersistentLine(data);
                    _initialPersistentLineData = null;
                    _selectedPersistentLineData = null;
                }
                if (data.pointsCount >= 2) updateStroke = true;
            });
            menu.AddItem(new GUIContent("Select all points ... "
                + PWBSettings.shortcuts.lineSelectAllPoints.combination.ToString()), on: false, () => data.SelectAll());
            menu.AddItem(new GUIContent("Deselect all points ... "
                + PWBSettings.shortcuts.lineDeselectAllPoints.combination.ToString()), on: false,
                () => data.ClearSelection());
            menu.AddItem(new GUIContent("Set prev segment as straight or curved ... "
                + PWBSettings.shortcuts.lineToggleCurve.combination.ToString()), on: false, () =>
                {
                    data.ToggleSegmentType();
                    updateStroke = true;
                });
            menu.AddItem(new GUIContent("Close or open the path ... "
                + PWBSettings.shortcuts.lineToggleClosed.combination.ToString()), on: false, () =>
                {
                    data.ToggleClosed();
                    updateStroke = true;
                });
 
            menu.AddSeparator(string.Empty);
            PersistentItemContextMenu(menu, data, mousePosition);
            menu.ShowAsContext();
        }

        private static bool DrawLineControlPoints(LineData lineData, bool isPersistent, bool showHandles,
            out bool clickOnPoint, out bool multiSelection, out bool addToSelection,
            out bool removedFromSelection, out bool wasEdited, out Vector3 delta)
        {
            delta = Vector3.zero;
            clickOnPoint = false;
            wasEdited = false;
            multiSelection = false;
            addToSelection = false;
            removedFromSelection = false;
            bool leftMouseDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;
            bool selectAll = ToolManager.editMode && LineManager.editModeType == LineManager.EditModeType.LINE_POSE;
            bool selectionChanged = false;
            for (int i = 0; i < lineData.pointsCount; ++i)
            {
                if (selectingLinePoints)
                {
                    var GUIPos = UnityEditor.HandleUtility.WorldToGUIPoint(lineData.GetPoint(i));
                    var rect = _selectionRect;
                    if (_selectionRect.size.x < 0 || _selectionRect.size.y < 0)
                    {
                        var max = Vector2.Max(_selectionRect.min, _selectionRect.max);
                        var min = Vector2.Min(_selectionRect.min, _selectionRect.max);
                        var size = max - min;
                        rect = new Rect(min, size);
                    }
                    if (rect.Contains(GUIPos))
                    {
                        if (!Event.current.control && lineData.selectedPointIdx < 0) lineData.selectedPointIdx = i;
                        lineData.AddToSelection(i);
                        clickOnPoint = true;
                        multiSelection = true;
                        selectionChanged = true;
                    }
                }
                else
                {
                    var controlId = GUIUtility.GetControlID(FocusType.Passive);
                    float distFromMouse = UnityEditor.HandleUtility.DistanceToRectangle(lineData.GetPoint(i),
                        Quaternion.identity, 0f);
                    UnityEditor.HandleUtility.AddControl(controlId, distFromMouse);

                    if (!clickOnPoint && showHandles && leftMouseDown
                    && UnityEditor.HandleUtility.nearestControl == controlId)
                    {
                        if (!Event.current.control)
                        {
                            lineData.ClearSelection();
                            lineData.selectedPointIdx = i;
                            selectionChanged = true;
                        }
                        if ((!ToolManager.editMode
                            || (ToolManager.editMode && LineManager.editModeType == LineManager.EditModeType.NODES))
                            && (Event.current.control || lineData.selectionCount == 0))
                        {
                            if (lineData.ControlPointIsSelected(i))
                            {
                                lineData.RemoveFromSelection(i);
                                lineData.selectedPointIdx = -1;
                                removedFromSelection = true;
                            }
                            else
                            {
                                lineData.AddToSelection(i);
                                lineData.showHandles = true;
                                lineData.selectedPointIdx = i;
                                if (Event.current.control) addToSelection = true;
                            }
                            selectionChanged = true;
                        }
                        clickOnPoint = true;
                        Event.current.Use();
                    }
                    if (Event.current.button == 1 && Event.current.type == EventType.MouseDown
                        && !Event.current.control && !Event.current.shift && !Event.current.alt
                            && UnityEditor.HandleUtility.nearestControl == controlId)
                    {
                        ShowLineContextMenu(lineData, isPersistent,
                            UnityEditor.EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition), pointIdx: i);
                        Event.current.Use();
                    }
                }
                if (Event.current.type != EventType.Repaint) continue;
                DrawDotHandleCap(lineData.GetPoint(i), 1, 1, lineData.ControlPointIsSelected(i));
            }
            if (selectionChanged) ResetLineRotation();
            var midpoints = lineData.midpoints;
            for (int i = 0; i < midpoints.Length; ++i)
            {
                var point = midpoints[i];

                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                if (showHandles)
                {
                    float distFromMouse
                           = UnityEditor.HandleUtility.DistanceToRectangle(point, Quaternion.identity, 0f);
                    UnityEditor.HandleUtility.AddControl(controlId, distFromMouse);
                }
                DrawDotHandleCap(point, 0.4f);
                if (showHandles && UnityEditor.HandleUtility.nearestControl == controlId)
                {
                    DrawDotHandleCap(point);
                    if (leftMouseDown)
                    {
                        lineData.InsertPoint(i + 1, new LinePoint(point));
                        lineData.ClearSelection();
                        lineData.selectedPointIdx = i + 1;
                        updateStroke = true;
                        clickOnPoint = true;
                        Event.current.Use();
                    }
                }
            }
            if (showHandles && lineData.showHandles && lineData.selectedPointIdx >= 0)
            {
                var selectedPoint = lineData.selectedPoint;
                if (_updateHandlePosition)
                {
                    selectedPoint = _handlePosition;
                    _updateHandlePosition = false;
                }
                var prevPosition = lineData.selectedPoint;
                lineData.SetPoint(lineData.selectedPointIdx,
                    UnityEditor.Handles.PositionHandle(selectedPoint, Quaternion.identity),
                    registerUndo: true, selectAll);
                var point = SnapToBounds(lineData.selectedPoint);
                point = _snapToVertex ? LinePointSnapping(point)
                    : SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                        LineManager.settings.paintOnPalettePrefabs, LineManager.settings.paintOnMeshesWithoutCollider,
                        LineManager.settings.ignoreSceneColliders, paintOnTheGrid: false, Vector3.down);
                lineData.SetPoint(lineData.selectedPointIdx, point, registerUndo: false, selectAll);
                _handlePosition = lineData.selectedPoint;
                if (prevPosition != lineData.selectedPoint)
                {
                    wasEdited = true;
                    updateStroke = true;
                    delta = lineData.selectedPoint - prevPosition;
                    ToolProperties.RepainWindow();
                }
                if (LineManager.editModeType == LineManager.EditModeType.LINE_POSE)
                {
                    var prevRotation = _lineRotation;
                    var handleRotation = UnityEditor.Handles.RotationHandle(_lineRotation, lineData.selectedPoint);
                    if (prevRotation != handleRotation)
                    {
                        RotateLineAround(lineData.selectedPointIdx, handleRotation, lineData);
                        wasEdited = true;
                        updateStroke = true;
                        ToolProperties.RepainWindow();
                    }
                }
            }
            if (!showHandles) return false;
            return clickOnPoint || wasEdited;
        }

        private static Vector3 LinePointSnapping(Vector3 point)
        {
            const float snapSqrDistance = 400f;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var persistentLines = LineManager.instance.GetPersistentItems();
            var result = point;
            var minSqrDistance = snapSqrDistance;
            foreach (var lineData in persistentLines)
            {
                var controlPoints = lineData.points;
                foreach (var controlPoint in controlPoints)
                {
                    var intersection = mouseRay.origin + Vector3.Project(controlPoint - mouseRay.origin, mouseRay.direction);
                    var GUIControlPoint = UnityEditor.HandleUtility.WorldToGUIPoint(controlPoint);
                    var intersectionGUIPoint = UnityEditor.HandleUtility.WorldToGUIPoint(intersection);
                    var sqrDistance = (GUIControlPoint - intersectionGUIPoint).sqrMagnitude;
                    if (sqrDistance > 0 && sqrDistance < snapSqrDistance && sqrDistance < minSqrDistance)
                    {
                        minSqrDistance = sqrDistance;
                        result = controlPoint;
                    }
                }
            }
            return result;
        }

        private static void DrawLine(LineData lineData, bool drawSurfacePath)
        {
            var pathPoints = lineData.pathPoints;
            var surfacePathPoints = lineData.onSurfacePathPoints;
            if (pathPoints.Length == 0 || (drawSurfacePath && surfacePathPoints.Length == 0))
                lineData.UpdatePath(forceUpdate: true, updateOnSurfacePoints: drawSurfacePath);
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            if (drawSurfacePath)
            {
                UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
                UnityEditor.Handles.DrawAAPolyLine(8, surfacePathPoints);
                UnityEditor.Handles.color = new Color(0f, 1f, 1f, 0.5f);
                UnityEditor.Handles.DrawAAPolyLine(4, surfacePathPoints);
            }

            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(8, pathPoints);
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(4, pathPoints);
        }

        private static void DrawSelectionRectangle()
        {
            if (!selectingLinePoints) return;
            var rays = new Ray[]
            {
                UnityEditor.HandleUtility.GUIPointToWorldRay(_selectionRect.min),
                UnityEditor.HandleUtility.GUIPointToWorldRay(new Vector2(_selectionRect.xMax, _selectionRect.yMin)),
                UnityEditor.HandleUtility.GUIPointToWorldRay(_selectionRect.max),
                UnityEditor.HandleUtility.GUIPointToWorldRay(new Vector2(_selectionRect.xMin, _selectionRect.yMax))
            };
            var verts = new Vector3[4];
            for (int i = 0; i < 4; ++i) verts[i] = rays[i].origin + rays[i].direction;
            UnityEditor.Handles.DrawSolidRectangleWithOutline(verts,
            new Color(0f, 0.5f, 0.5f, 0.3f), new Color(0f, 0.5f, 0.5f, 1f));
        }

        private static void SelectionRectangleInput(bool clickOnPoint)
        {
            bool leftMouseDown = Event.current.button == 1 && Event.current.type == EventType.MouseDown;
            if (!selectingLinePoints && Event.current.shift && leftMouseDown && !clickOnPoint)
            {
                selectingLinePoints = true;
                _selectionRect = new Rect(Event.current.mousePosition, Vector2.zero);
                Event.current.Use();
            }
            if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseMove)
                && selectingLinePoints)
            {
                _selectionRect.size = Event.current.mousePosition - _selectionRect.position;
            }
            if (Event.current.button == 0 && (Event.current.type == EventType.MouseUp
                || Event.current.type == EventType.Ignore || Event.current.type == EventType.KeyUp))
                selectingLinePoints = false;
        }

        public static void ApplyPersistentLine(LineData data)
        {
            data.UpdatePoses();
            DeleteDisabledObjects();
            PWBCore.staticData.SetSavePending();
            AutoSave.QuickSave();
        }
        private static void LineInput(bool persistent, UnityEditor.SceneView sceneView, bool skipPreview)
        {
            var lineData = persistent ? _selectedPersistentLineData : _lineData;
            if (lineData == null) return;
            if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
            {
                if (persistent)
                {
                    if (skipPreview)
                    {
                        PreviewPersistentLine(lineData);
                        LineStrokePreview(sceneView, lineData, persistent: true, forceUpdate: true, _firstNewObjIdx);
                    }
                    DeleteDisabledObjects();
                    _persistentItemWasEdited = true;
                    ApplySelectedPersistentLine(true);
                    DeleteDisabledObjects();
                    ToolProperties.RepainWindow();
                }
                else
                {
                    CreateLine();
                    ResetLineState(false);
                }
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete
                && !Event.current.control && !Event.current.alt && !Event.current.shift)
            {
                if (persistent && lineData.pointsCount <= 2)
                {
                    LineManager.instance.DeletePersistentItem(lineData.id, deleteObjects: true);
                    UnityEditor.SceneView.RepaintAll();
                }
                else
                {
                    lineData.RemoveSelectedPoints();
                    if (persistent)
                    {
                        lineData.UpdatePath(forceUpdate: true, updateOnSurfacePoints: true);
                        PreviewPersistentLine(lineData);
                        LineStrokePreview(sceneView, lineData, persistent: true, forceUpdate: true, _firstNewObjIdx);
                        DeleteDisabledObjects();
                        ApplySelectedPersistentLine(true);
                        _initialPersistentLineData = null;
                        _selectedPersistentLineData = null;
                    }
                    if (lineData.pointsCount >= 2) updateStroke = true;
                }
            }
            else if (Event.current.type == EventType.MouseDown && Event.current.button == 1
                && Event.current.control && !Event.current.alt && !Event.current.shift
                && LineManager.editModeType == LineManager.EditModeType.NODES)
            {
                if (MouseDot(out Vector3 point, out Vector3 normal, lineData.settings.mode, sceneView.in2DMode,
                lineData.settings.paintOnPalettePrefabs, lineData.settings.paintOnMeshesWithoutCollider, false,
                ignoreSceneColliders: lineData.settings.ignoreSceneColliders))
                {
                    point = SnapToBounds(point);
                    point = _snapToVertex ? LinePointSnapping(point)
                        : SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                        lineData.settings.paintOnPalettePrefabs, lineData.settings.paintOnMeshesWithoutCollider,
                        lineData.settings.ignoreSceneColliders, paintOnTheGrid: false, Vector3.down);
                    lineData.AddPoint(point, false);
                    if (persistent)
                    {
                        PreviewPersistentLine(_selectedPersistentLineData);
                        LineStrokePreview(sceneView, lineData, persistent: true, forceUpdate: true, _firstNewObjIdx);
                    }
                    else updateStroke = true;
                }
            }
            else if (PWBSettings.shortcuts.lineSelectAllPoints.Check()
                && LineManager.editModeType == LineManager.EditModeType.NODES)
                lineData.SelectAll();
            else if (PWBSettings.shortcuts.lineDeselectAllPoints.Check()) lineData.ClearSelection();
            else if (PWBSettings.shortcuts.lineToggleCurve.Check())
            {
                lineData.ToggleSegmentType();
                updateStroke = true;
            }
            else if (PWBSettings.shortcuts.lineToggleClosed.Check())
            {
                lineData.ToggleClosed();
                updateStroke = true;
            }
            else if (PWBSettings.shortcuts.lineEditGap.Check())
            {
                var deltaSign = Mathf.Sign(PWBSettings.shortcuts.lineEditGap.combination.delta);
                lineData.settings.gapSize += lineData.lenght * deltaSign * 0.001f;
                ToolProperties.RepainWindow();
            }
            if (!persistent) return;
            if (PWBSettings.shortcuts.editModeSelectParent.Check() && lineData != null)
            {
                var parent = lineData.GetParent();
                if (parent != null) UnityEditor.Selection.activeGameObject = parent;
            }
            else if (PWBSettings.shortcuts.editModeDeleteItemButNotItsChildren.Check())
                LineManager.instance.DeletePersistentItem(lineData.id, false);
            else if (PWBSettings.shortcuts.editModeDeleteItemAndItsChildren.Check())
                LineManager.instance.DeletePersistentItem(lineData.id, true);
            else if (PWBSettings.shortcuts.editModeDuplicate.Check()) DuplicateItem(lineData.id);
            else if (PWBSettings.shortcuts.lineEditModeTypeToggle.Check())
                LineManager.ToggleEditModeType();
        }
        #endregion

        #region EDIT MODE
        private static System.Collections.Generic.HashSet<GameObject> _disabledObjects
            = new System.Collections.Generic.HashSet<GameObject>();
        private static bool _editingPersistentLine = false;
        private static LineData _initialPersistentLineData = null;
        private static LineData _selectedPersistentLineData = null;

        private static System.Collections.Generic.Dictionary<long, IPersistentData.Visibility> _prevDataVisibility
            = new System.Collections.Generic.Dictionary<long, IPersistentData.Visibility>();

        private static void UpdateDataPrevVisibility(IPersistentData data)
        {
            if (data.visibility == IPersistentData.Visibility.HIDE_ALL)
                UnityEditor.SceneVisibilityManager.instance.Hide(data.objects, true);
            else UnityEditor.SceneVisibilityManager.instance.Show(data.objects, true);
            if (_prevDataVisibility.ContainsKey(data.id)) _prevDataVisibility[data.id] = data.visibility;
            else _prevDataVisibility.Add(data.id, data.visibility);
        }

        public static void SelectLine(LineData data)
        {
            ApplySelectedPersistentLine(true);
            _editingPersistentLine = true;
            data.ClearSelection();
            data.selectedPointIdx = 0;
            data.showHandles = true;
            _selectedPersistentLineData = data;
            if (_initialPersistentLineData == null) _initialPersistentLineData = data.Clone();
            LineManager.instance.CopyToolSettings(data.settings);
        }
        private static void LineToolEditMode(UnityEditor.SceneView sceneView)
        {
            var persistentLines = LineManager.instance.GetPersistentItems();
            var selectedLineId = _initialPersistentLineData == null ? -1 : _initialPersistentLineData.id;
            bool clickOnAnyPoint = false;
            bool someLinesWereEdited = false;
            var delta = Vector3.zero;
            var editedData = _selectedPersistentLineData;
            DrawSelectionRectangle();
            foreach (var lineData in persistentLines)
            {
                if(lineData.pointsCount <= 2)
                {
                    void DeleteItem()
                    {
                        LineManager.instance.DeletePersistentItem(lineData.id, deleteObjects: true, registerUndo: false);
                        PWBItemsWindow.RepainWindow();
                    }
                    if (lineData.pointsCount <= 1)
                    {
                        DeleteItem();
                        continue;
                    }
                    var points = lineData.points;
                    if (points[0] == points[1] && points[0] == Vector3.zero)
                    {
                        DeleteItem();
                        continue;
                    }
                }
                if (!_prevDataVisibility.ContainsKey(lineData.id) || lineData.visibility != _prevDataVisibility[lineData.id])
                {
                    if (lineData.visibility == IPersistentData.Visibility.HIDE_ALL)
                        UnityEditor.SceneVisibilityManager.instance.Hide(lineData.objects, true);
                    else UnityEditor.SceneVisibilityManager.instance.Show(lineData.objects, true);
                    UpdateDataPrevVisibility(lineData);
                }
                if (lineData.visibility != IPersistentData.Visibility.SHOW_ALL) continue;
                DrawLine(lineData, drawSurfacePath: lineData.selectionCount > 0);

                if (DrawLineControlPoints(lineData, isPersistent: true, ToolManager.editMode,
                    out bool clickOnPoint, out bool multiSelection, out bool addToselection,
                    out bool removedFromSelection, out bool wasEdited, out Vector3 localDelta))
                {
                    if (clickOnPoint)
                    {
                        clickOnAnyPoint = true;
                        _editingPersistentLine = true;
                        if (selectedLineId != lineData.id)
                        {
                            ApplySelectedPersistentLine(false);
                            if (selectedLineId == -1) _createProfileName = LineManager.instance.selectedProfileName;
                            else if (!addToselection && !removedFromSelection)
                            {
                                var selectedLines
                                    = persistentLines.Where(i => i != lineData && i.selectionCount > 0).ToArray();
                                foreach (var selected in selectedLines)
                                {
                                    PWBCore.SetActiveTempColliders(selected.objects, true);
                                    selected.showHandles = false;
                                    selected.ClearSelection();
                                }
                            }
                            LineManager.instance.CopyToolSettings(lineData.settings);
                            ToolProperties.RepainWindow();
                            PWBCore.SetActiveTempColliders(lineData.objects, false);
                        }
                        _selectedPersistentLineData = lineData;
                        if (_initialPersistentLineData == null) _initialPersistentLineData = lineData.Clone();
                        else if (_initialPersistentLineData.id != lineData.id) _initialPersistentLineData = lineData.Clone();
                        if (!removedFromSelection) foreach (var l in persistentLines) l.showHandles = (l == lineData);
                    }
                    if (addToselection) lineData.showHandles = true;
                    if (wasEdited)
                    {
                        _editingPersistentLine = true;
                        someLinesWereEdited = true;
                        delta = localDelta;
                        editedData = lineData;
                        _persistentItemWasEdited = true;
                    }
                }
            }

            var repaintItemsWindow = false;
            foreach (var lineData in persistentLines)
            {
                var isSelected = lineData.selectionCount > 0;
                if (lineData.isSelected != isSelected) repaintItemsWindow = true;
                lineData.isSelected = lineData.selectionCount > 0;
            }
            if (repaintItemsWindow) PWBItemsWindow.RepainWindow();

            var linesEdited = persistentLines.Where(i => i.selectionCount > 0).ToArray();

            if (someLinesWereEdited)
            {
                if (linesEdited.Length > 0) _disabledObjects.Clear();
                if (linesEdited.Length > 1)
                {
                    _paintStroke.Clear();
                    foreach (var lineData in linesEdited)
                    {
                        if (lineData != editedData) lineData.AddDeltaToSelection(delta);
                        lineData.UpdatePath(forceUpdate: false, updateOnSurfacePoints: true);
                        PreviewPersistentLine(lineData);
                        LineStrokePreview(sceneView, lineData, persistent: true, forceUpdate: true, _firstNewObjIdx);
                    }
                    PWBCore.SetSavePending();
                    return;
                }
            }
            if (linesEdited.Length > 1) PreviewPersistent(sceneView.camera);

            if (!ToolManager.editMode) return;

            if (LineManager.editModeType == LineManager.EditModeType.NODES) SelectionRectangleInput(clickOnAnyPoint);

            bool skipPreview = _selectedPersistentLineData != null
                && _selectedPersistentLineData.objectCount > PWBCore.staticData.maxPreviewCountInEditMode;
            if (!skipPreview)
            {
                if ((!someLinesWereEdited && linesEdited.Length <= 1)
                    && _editingPersistentLine && _selectedPersistentLineData != null)
                {
                    var forceStrokeUpdate = updateStroke;
                    if (updateStroke)
                    {
                        _selectedPersistentLineData.UpdatePath(forceUpdate: false, updateOnSurfacePoints: true);
                        PreviewPersistentLine(_selectedPersistentLineData);
                        updateStroke = false;
                        PWBCore.SetSavePending();
                    }
                    if (_brushstroke != null
                        && !BrushstrokeManager.BrushstrokeEqual(BrushstrokeManager.brushstroke, _brushstroke))
                        _paintStroke.Clear();

                    LineStrokePreview(sceneView, _selectedPersistentLineData,
                        persistent: true, forceStrokeUpdate, _firstNewObjIdx);
                }
            }
            LineInput(true, sceneView, skipPreview);
        }

        private static int _firstNewObjIdx = 0;

        public static void PreviewSelectedPersistentLines()
        {
            if (ToolManager.tool != ToolManager.PaintTool.LINE) return;
            var persistentLines = LineManager.instance.GetPersistentItems();
            foreach (var lineData in persistentLines)
            {
                if (!lineData.isSelected) continue;
                PreviewPersistentLine(lineData);
            }
        }
        public static void PreviewPersistentLine(LineData lineData)
        {
            BrushstrokeObject[] objPos = null;
            var objList = lineData.objectList;
            Vector3[] strokePos = null;
            var toolSettings = lineData.settings;
            BrushstrokeManager.UpdatePersistentLineBrushstroke(lineData.pathPoints,
                toolSettings, objList, out objPos, out strokePos, out _firstNewObjIdx);
            _disabledObjects.UnionWith(lineData.objects);
            float pathLength = 0;
            var prevSegmentDir = Vector3.zero;

            BrushSettings brushSettings = LineManager.instance.applyBrushToExisting ?
                PaletteManager.selectedBrush : PaletteManager.GetBrushById(lineData.initialBrushId);
            if (brushSettings == null && PaletteManager.selectedBrush != null)
            {
                brushSettings = PaletteManager.selectedBrush;
                lineData.SetInitialBrushId(brushSettings.id);
            }
            if (toolSettings.overwriteBrushProperties) brushSettings = toolSettings.brushSettings;
            if (brushSettings == null) brushSettings = new BrushSettings();
            var objSet = lineData.objectSet;
            float maxSurfaceHeight = 0f;
            for (int i = 0; i < objPos.Length; ++i)
            {
                var objIdx = objPos[i].objIdx;
                var obj = objList[objIdx];
                if (obj == null)
                {
                    lineData.RemovePose(objIdx);
                    continue;
                }
                obj.SetActive(true);
                var objScale = objPos[i].objScale;
                if (i > 0) pathLength += (objPos[i].objPosition - objPos[i - 1].objPosition).magnitude;

                var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (prefab == null) prefab = obj;
                var bounds = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation,
                    ignoreDissabled: true, BoundsUtils.ObjectProperty.BOUNDING_BOX, recursive: true, useDictionary: false);

                var size = Vector3.Scale(bounds.size, objScale);

                var height = size.x + size.y + size.z + maxSurfaceHeight + pathLength;
                Vector3 segmentDir = Vector3.zero;
                var objOnLineSize = AxesUtils.GetAxisValue(size, toolSettings.axisOrientedAlongTheLine);
               
                segmentDir = objPos[i].brushstrokeDirection;

                var perpendicularToTheSurface = toolSettings.perpendicularToTheSurface
                    || (brushSettings.rotateToTheSurface && !brushSettings.alwaysOrientUp);
                if (toolSettings.objectsOrientedAlongTheLine && !perpendicularToTheSurface)
                {
                    var projectionAxis = ((AxesUtils.SignedAxis)(toolSettings.projectionDirection)).axis;
                    segmentDir -= AxesUtils.GetVector(AxesUtils.GetAxisValue(segmentDir, projectionAxis), projectionAxis);
                }
                var normal = -toolSettings.projectionDirection;
                var otherAxes = AxesUtils.GetOtherAxes((AxesUtils.SignedAxis)(-toolSettings.projectionDirection));
                var tangetAxis = otherAxes[toolSettings.objectsOrientedAlongTheLine ? 0 : 1];
                Vector3 itemTangent = (AxesUtils.SignedAxis)(tangetAxis);
                var itemRotation = Quaternion.LookRotation(itemTangent, normal);
                var lookAt = Quaternion.LookRotation((Vector3)(AxesUtils.SignedAxis)
                    (toolSettings.axisOrientedAlongTheLine), Vector3.up);
                if (segmentDir != Vector3.zero) itemRotation = Quaternion.LookRotation(segmentDir, normal) * lookAt;
                var itemPosition = objPos[i].objPosition;
                var ray = new Ray(itemPosition + normal * height, -normal);
                if (toolSettings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (PWBToolRaycast(ray, out RaycastHit itemHit, out GameObject collider, maxDistance: float.MaxValue,
                        layerMask: -1, toolSettings.paintOnPalettePrefabs, toolSettings.paintOnMeshesWithoutCollider,
                        tags: null, terrainLayers: null, exceptions: objSet, sameOriginAsRay: false, origin: itemPosition,
                        createTempColliders: toolSettings.paintOnMeshesWithoutCollider,
                        ignoreSceneColliders: toolSettings.ignoreSceneColliders))
                    {
                        itemPosition = itemHit.point;
                        if (perpendicularToTheSurface) normal = itemHit.normal;
                        var surfObj = PWBCore.GetGameObjectFromTempCollider(collider);
                        var surfSize = BoundsUtils.GetBounds(surfObj.transform).size;
                        var h = surfSize.x + surfSize.y + surfSize.z;
                        maxSurfaceHeight = Mathf.Max(h, maxSurfaceHeight);
                    }
                    else if (toolSettings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE) continue;
                }

                if (perpendicularToTheSurface && segmentDir != Vector3.zero)
                {
                    if (toolSettings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                    {
                        var bitangent = Vector3.Cross(segmentDir, normal);
                        var lineNormal = Vector3.Cross(bitangent, segmentDir);
                        itemRotation = Quaternion.LookRotation(segmentDir, lineNormal) * lookAt;
                    }
                    else
                    {
                        var plane = new Plane(normal, itemPosition);
                        var tangent = plane.ClosestPointOnPlane(segmentDir + itemPosition) - itemPosition;
                        itemRotation = Quaternion.LookRotation(tangent, normal) * lookAt;
                    }
                }
                else if (!perpendicularToTheSurface && segmentDir != Vector3.zero)

                    if (!toolSettings.perpendicularToTheSurface
                            && brushSettings.rotateToTheSurface && brushSettings.alwaysOrientUp)
                    {
                        var fw = itemRotation * Vector3.forward;
                        const float minMag = 1e-6f;
                        fw.y = 0;
                        if (Mathf.Abs(fw.x) < minMag && Mathf.Abs(fw.z) < minMag) fw = Quaternion.Euler(0, 90, 0) * normal;
                        itemRotation = Quaternion.LookRotation(fw, Vector3.up);
                    }

                var pivotToCenter = prefab.transform.InverseTransformDirection(bounds.center - prefab.transform.position);
                pivotToCenter = itemRotation * Vector3.Scale(pivotToCenter, objScale);

                itemPosition += normal * (size.y / 2) - pivotToCenter;
                if (LineManager.instance.applyBrushToExisting)
                {
                    if (brushSettings.embedInSurface
                    && toolSettings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                    {
                        var bottomMagnitude = BoundsUtils.GetBottomMagnitude(obj.transform);
                        if (brushSettings.embedAtPivotHeight)
                            itemPosition += itemRotation * (normal * bottomMagnitude);
                        else
                        {
                            var TRS = Matrix4x4.TRS(itemPosition, itemRotation, objScale);
                            var bottomVertices = BoundsUtils.GetBottomVertices(obj.transform);
                            var bottomDistanceToSurfce = GetBottomDistanceToSurface(bottomVertices, TRS,
                                Mathf.Abs(bottomMagnitude), toolSettings.paintOnPalettePrefabs,
                                toolSettings.paintOnMeshesWithoutCollider, toolSettings.ignoreSceneColliders,
                                out Transform surfaceTransform, exceptions: objSet);
                            itemPosition += itemRotation * (normal * -bottomDistanceToSurfce);
                        }
                    }

                    itemPosition += normal * objPos[i].surfaceDistance;
                    itemPosition += itemRotation * brushSettings.localPositionOffset;

                    var additionalAngle = brushSettings.GetAdditionalAngle();
                    if (additionalAngle != Vector3.zero) itemRotation *= Quaternion.Euler(additionalAngle);
                    var flipX = brushSettings.GetFlipX();
                    var flipY = brushSettings.GetFlipY();
                    if (flipX || flipY)
                    {
                        var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();
                        foreach (var spriteRenderer in spriteRenderers)
                        {
                            UnityEditor.Undo.RecordObject(spriteRenderer, LineData.COMMAND_NAME);
                            spriteRenderer.flipX = flipX;
                            spriteRenderer.flipY = flipY;
                        }
                    }
                }
                UnityEditor.Undo.RecordObject(obj.transform, LineData.COMMAND_NAME);
                obj.transform.SetPositionAndRotation(itemPosition, itemRotation);
                obj.transform.localScale = objScale;
                _disabledObjects.Remove(obj);
                lineData.lastTangentPos = objPos[i].objPosition;
            }
            foreach (var obj in _disabledObjects) if (obj != null) obj.SetActive(false);
        }

        private static void ResetSelectedPersistentLine()
        {
            _editingPersistentLine = false;
            if (_initialPersistentLineData == null) return;
            var selectedLine = LineManager.instance.GetItem(_initialPersistentLineData.id);
            if (selectedLine == null) return;
            selectedLine.ResetPoses(_initialPersistentLineData);
            selectedLine.ClearSelection();
        }

        private static void ApplySelectedPersistentLine(bool deselectPoint)
        {
            if (!_persistentItemWasEdited) return;
            _persistentItemWasEdited = false;
            if (!ApplySelectedPersistentObject(deselectPoint, ref _editingPersistentLine, ref _initialPersistentLineData,
                ref _selectedPersistentLineData, LineManager.instance)) return;
            if (_initialPersistentLineData == null) return;
            var selected = LineManager.instance.GetItem(_initialPersistentLineData.id);
            _initialPersistentLineData = selected.Clone();
        }

        private static void DeselectPersistentLines()
        {
            var persistentLines = LineManager.instance.GetPersistentItems();
            foreach (var l in persistentLines) l.ClearSelection();
        }

        #endregion
    }
    #endregion
}
