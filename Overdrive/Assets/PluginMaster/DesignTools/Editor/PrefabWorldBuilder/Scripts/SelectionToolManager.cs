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

namespace PluginMaster
{
    #region DATA & SETTINGS
    [System.Serializable]
    public class SelectionToolSettings : SelectionToolBase, IToolSettings, ISerializationCallbackReceiver
    {
        [SerializeField] private bool _move = true;
        [SerializeField] private bool _rotate = false;
        [SerializeField] private bool _scale = false;
        [SerializeField] private Space _handleSpace = Space.Self;
        [SerializeField] private Space _boxSpace = Space.Self;
        [SerializeField] private bool _paletteFilter = false;
        [SerializeField] private bool _brushFilter = false;
        [SerializeField] private LayerMask _layerFilter = -1;
        [SerializeField] private System.Collections.Generic.List<string> _tagFilter = null;

        public Space handleSpace
        {
            get => _handleSpace;
            set
            {
                if (_handleSpace == value) return;
                _handleSpace = value;
                if (_handleSpace == Space.World) _scale = false;
                DataChanged();
            }
        }

        public bool move
        {
            get => _move;
            set
            {
                if (_move == value) return;
                _move = value;
                DataChanged();
            }
        }
        public bool rotate
        {
            get => _rotate;
            set
            {
                if (_rotate == value) return;
                _rotate = value;
                DataChanged();
            }
        }
        public bool scale
        {
            get => _scale;
            set
            {
                if (_scale == value) return;
                _scale = value;
                if (_scale) _handleSpace = Space.Self;
                DataChanged();
            }
        }

        public Space boxSpace
        {
            get => _boxSpace;
            set
            {
                if (_boxSpace == value) return;
                _boxSpace = value;
                DataChanged();
            }
        }

        public bool paletteFilter
        {
            get => _paletteFilter;
            set
            {
                if (_paletteFilter == value) return;
                _paletteFilter = value;
                DataChanged();
            }
        }

        public bool brushFilter
        {
            get => _brushFilter;
            set
            {
                if (_brushFilter == value) return;
                _brushFilter = value;
                DataChanged();
            }
        }
        public LayerMask layerFilter
        {
            get => _layerFilter;
            set
            {
                if (_layerFilter == value) return;
                _layerFilter = value;
                DataChanged();
            }
        }
        public System.Collections.Generic.List<string> tagFilter
        {
            get
            {
                if (_tagFilter == null) UpdateTagFilter();
                return _tagFilter;
            }
            set
            {
                if (_tagFilter == value) return;
                _tagFilter = value;
                DataChanged();
            }
        }
        private void UpdateTagFilter()
        {
            if (_tagFilter != null) return;
            _tagFilter = new System.Collections.Generic.List<string>(UnityEditorInternal.InternalEditorUtility.tags);
        }
        public void OnBeforeSerialize() => UpdateTagFilter();
        public void OnAfterDeserialize() => UpdateTagFilter();

        public override void Copy(IToolSettings other)
        {
            var otherSelectionToolSettings = other as SelectionToolSettings;
            if (otherSelectionToolSettings == null) return;
            base.Copy(other);
            _move = otherSelectionToolSettings._move;
            _rotate = otherSelectionToolSettings._rotate;
            _scale = otherSelectionToolSettings._scale;
            _handleSpace = otherSelectionToolSettings._handleSpace;
            _boxSpace = otherSelectionToolSettings._boxSpace;
            _paletteFilter = otherSelectionToolSettings._paletteFilter;
            _brushFilter = otherSelectionToolSettings._brushFilter;
            _layerFilter = otherSelectionToolSettings._layerFilter;
            _tagFilter = otherSelectionToolSettings._tagFilter == null ? null
                : new System.Collections.Generic.List<string>(otherSelectionToolSettings._tagFilter);
        }
    }

    [System.Serializable]
    public class SelectionToolManager : ToolManagerBase<SelectionToolSettings> { }
    #endregion

    #region PWBIO
    public static partial class PWBIO
    {
        private static int _selectedBoxPointIdx = -1;
        private static Quaternion _selectionRotation = Quaternion.identity;
        private static Vector3 _selectionScale = Vector3.one;
        private static Vector3 _snappedPoint;
        private static bool _snappedPointIsVisible = false;
        private static bool _snappedPointIsSelected = false;
        private static (Vector3 position, GameObject[] selection) _selectionMoveFrom;
        private static bool _selectionMoving = false;
        private static bool _editingSelectionHandlePosition = false;
        private static Vector3 _tempSelectionHandle = Vector3.zero;
        private static bool _selectionChanged = false;
        private static Bounds _selectionBounds;
        private static bool _setSelectionOriginPosition = false;

        public static void InitializeSelectionTool()
        {
            SetSelectionOriginPosition();
            SelectionManager.UpdateSelection();
            ResetUnityCurrentTool();
            UpdateOctree();
        }

        private static void SelectionDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                _snappedPointIsSelected = false;
                _selectionMoving = false;
                if (_selectedBoxPointIdx >= 0 && _selectedBoxPointIdx != 10) _selectedBoxPointIdx = 10;
                else
                {
                    ResetUnityCurrentTool();
                    ToolManager.DeselectTool();
                    return;
                }
            }
            if (UnityEditor.Tools.current != UnityEditor.Tool.View && UnityEditor.Tools.current != UnityEditor.Tool.None)
                UnityEditor.Tools.current = UnityEditor.Tool.None;
            if (SelectionManager.topLevelSelection.Length == 0) return;

            var points = SelectionPoints(sceneView.camera);

            if (_setSelectionOriginPosition && SnapManager.settings.snappingEnabled && !SnapManager.settings.lockedGrid)
            {
                _setSelectionOriginPosition = false;
                SnapManager.settings.SetOriginHeight(points[_selectedBoxPointIdx], SnapManager.settings.gridAxis);
            }

            SelectionInput(points, sceneView.in2DMode);
            if (_selectionMoving)
            {
                UnityEditor.Handles.CircleHandleCap(0, _selectionMoveFrom.position, sceneView.camera.transform.rotation,
                    UnityEditor.HandleUtility.GetHandleSize(_selectionMoveFrom.position) * 0.06f, EventType.Repaint);
                if (_selectedBoxPointIdx >= 0)
                    UnityEditor.Handles.DrawLine(_selectionMoveFrom.position, points[_selectedBoxPointIdx]);
            }

            bool mouseDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;
            bool clickOnPoint = false;

            bool SelectPoint(Vector3 point, int i)
            {
                if (_editingSelectionHandlePosition) return false;
                if (clickOnPoint) return false;
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                var distFromMouse = UnityEditor.HandleUtility.DistanceToRectangle(point, Quaternion.identity, 0f);
                UnityEditor.HandleUtility.AddControl(controlId, distFromMouse);
                if (UnityEditor.HandleUtility.nearestControl != controlId) return false;
                DrawDotHandleCap(point, 1f, 1.2f);
                if (!mouseDown) return false;
                _selectedBoxPointIdx = i;
                clickOnPoint = true;
                Event.current.Use();
                return true;
            }

            for (int i = 0; i < points.Count; ++i)
            {
                if (SelectPoint(points[i], i)) _snappedPointIsSelected = false;
                if (clickOnPoint) break;
            }

            if (_snappedPointIsVisible || _snappedPointIsSelected)
            {
                points.Add(_snappedPoint);
                if (SelectPoint(_snappedPoint, points.Count - 1)) _snappedPointIsSelected = true;
            }
            if (_selectionChanged)
            {
                _tempSelectionHandle = Vector3.zero;
                _selectionChanged = false;
                ApplySelectionFilters();
            }

            if (_editingSelectionHandlePosition)
            {
                _selectedBoxPointIdx = 11;
                UnityEditor.Handles.CircleHandleCap(0, points[11], sceneView.camera.transform.rotation,
                    UnityEditor.HandleUtility.GetHandleSize(points[11]) * 0.06f, EventType.Repaint);
            }
            if (_selectedBoxPointIdx >= 0)
            {
                var rotation = GetSelectionRotation();
                if (_editingSelectionHandlePosition)
                {
                    var delta = points[_selectedBoxPointIdx];
                    points[_selectedBoxPointIdx] = UnityEditor.Handles.PositionHandle(points[_selectedBoxPointIdx], rotation);
                    delta = points[_selectedBoxPointIdx] - delta;
                    _tempSelectionHandle += delta;
                }
                else
                {
                    bool handleWasUsed = MoveSelectionToHandlePosition(rotation, points, sceneView);
                    handleWasUsed |= MoveSelectionToMousePosition(points, sceneView);
                    if (RotateSelection(rotation, points, sceneView))
                    {
                        handleWasUsed = true;
                    }
                    handleWasUsed |= ScaleSelection(rotation, points);
                    if (handleWasUsed)
                    {
                        if (SelectionToolManager.settings.embedInSurface) EmbedSelectionInSurface(_selectionRotation);
                        PWBCore.UpdateTempCollidersTransforms(SelectionManager.topLevelSelection);
                    }
                }
            }
            else _editingSelectionHandlePosition = false;
        }

        private static void SelectionInput(System.Collections.Generic.List<Vector3> points, bool in2DMode)
        {
            if (UnityEditor.Tools.current == UnityEditor.Tool.Move) return;
            var keyCode = Event.current.keyCode;
            if (PWBSettings.shortcuts.selectionTogglePositionHandle.Check())
            {
                SelectionToolManager.settings.move = !SelectionToolManager.settings.move;
                PWBToolbar.RepaintWindow();
            }
            else if (PWBSettings.shortcuts.selectionToggleRotationHandle.Check())
            {
                SelectionToolManager.settings.rotate = !SelectionToolManager.settings.rotate;
                PWBToolbar.RepaintWindow();
            }
            else if (PWBSettings.shortcuts.selectionToggleScaleHandle.Check())
            {
                SelectionToolManager.settings.scale = !SelectionToolManager.settings.scale;
                PWBToolbar.RepaintWindow();
            }
            else if (Event.current.type == EventType.KeyDown
                && (PWBSettings.shortcuts.selectionEditCustomHandle.Check()
                || (_editingSelectionHandlePosition && (Event.current.keyCode == KeyCode.Escape
                || Event.current.keyCode == KeyCode.Return))))
            {
                _editingSelectionHandlePosition = !_editingSelectionHandlePosition;
            }
            else if (_snappedToVertex && _selectedBoxPointIdx < 0)
            {
                _snappedPointIsVisible = false;
                var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (SnapToVertex(mouseRay, out RaycastHit snappedHit, in2DMode, SelectionManager.topLevelSelection))
                {
                    _snappedPoint = snappedHit.point;
                    _snappedPointIsVisible = true;
                }
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return
                && _selectedBoxPointIdx >= 0)
            {
                _editingSelectionHandlePosition = false;
                if (_selectionMoving)
                {
                    var delta = points[_selectedBoxPointIdx] - _selectionMoveFrom.position;
                    foreach (var obj in _selectionMoveFrom.selection)
                    {
                        if (obj == null) continue;
                        UnityEditor.Undo.RecordObject(obj.transform, "Move Selection");
                        obj.transform.position += delta;
                    }
                    _selectionMoving = false;
                    SelectionManager.UpdateSelection();
                    _selectedBoxPointIdx = -1;
                }
                else
                {
                    _selectionMoveFrom = (points[_selectedBoxPointIdx], SelectionManager.topLevelSelection);
                    _selectionMoving = true;
                }
            }
            else if (PWBSettings.shortcuts.selectionRotate90XCCW.Check())
                RotateSelection90Deg(Vector3.left, points);
            else if (PWBSettings.shortcuts.selectionRotate90XCW.Check())
                RotateSelection90Deg(Vector3.right, points);
            else if (PWBSettings.shortcuts.selectionRotate90YCCW.Check())
                RotateSelection90Deg(Vector3.down, points);
            else if (PWBSettings.shortcuts.selectionRotate90YCW.Check())
                RotateSelection90Deg(Vector3.up, points);
            else if (PWBSettings.shortcuts.selectionRotate90ZCCW.Check())
                RotateSelection90Deg(Vector3.back, points);
            else if (PWBSettings.shortcuts.selectionRotate90ZCW.Check())
                RotateSelection90Deg(Vector3.forward, points);
            else if (PWBSettings.shortcuts.selectionToggleSpace.Check())
            {
                SelectionToolManager.settings.handleSpace = SelectionToolManager.settings.handleSpace == Space.Self
                    ? Space.World : Space.Self;
                if (SelectionToolManager.settings.handleSpace == Space.World) ResetSelectionRotation();
                UnityEditor.SceneView.RepaintAll();
                ToolProperties.RepainWindow();
                Event.current.Use();
            }
        }

        public static void ApplySelectionFilters()
        {
            var selection = SelectionManager.topLevelSelection;
            if (selection == null)
            {
                SelectionManager.UpdateSelection();
                selection = SelectionManager.topLevelSelection;
            }

            var filtered = new System.Collections.Generic.List<GameObject>(selection.Length);

            for (int i = 0; i < selection.Length; ++i)
            {
                var obj = selection[i];
                if (obj == null) continue;

                // Palette filter
                if (SelectionToolManager.settings.paletteFilter)
                {
                    if (PaletteManager.selectedPalette == null) continue;
                    if (!PaletteManager.selectedPalette.ContainsSceneObject(obj)) continue;
                }

                // Brush filter
                if (SelectionToolManager.settings.brushFilter)
                {
                    if (PaletteManager.selectedBrush == null) continue;
                    if (!PaletteManager.selectedBrush.ContainsSceneObject(obj)) continue;
                }

                // Layer filter
                if (SelectionToolManager.settings.layerFilter != -1)
                {
                    var layerMask = SelectionToolManager.settings.layerFilter;
                    if ((layerMask & (1 << obj.layer)) == 0) continue;
                }

                // Tag filter
                if (SelectionToolManager.settings.tagFilter != null && SelectionToolManager.settings.tagFilter.Count > 0)
                {
                    bool tagFound = false;
                    for (int t = 0; t < SelectionToolManager.settings.tagFilter.Count; ++t)
                    {
                        if (obj.tag == SelectionToolManager.settings.tagFilter[t])
                        {
                            tagFound = true;
                            break;
                        }
                    }
                    if (!tagFound) continue;
                }
                else continue;
                filtered.Add(obj);
            }

            UnityEditor.Selection.objects = filtered.ToArray();
        }

        public static void InitializeSelectionToolOnBrushChanged()
        {
            ApplySelectionFilters();
            _selectionSurfaceMagnitude = 0f;
            _selectionSurfaceMagnitudeDic.Clear();
        }

        private static System.Collections.Generic.Dictionary<int, Quaternion> _initialRotations
            = new System.Collections.Generic.Dictionary<int, Quaternion>();
        private static System.Collections.Generic.Dictionary<int, float> _selectionSurfaceMagnitudeDic
            = new System.Collections.Generic.Dictionary<int, float>();
        private static float _selectionSurfaceMagnitude;
        private static bool _editingSelectionRotation = false;
        private static void EmbedSelectionInSurface(Quaternion rotation)
        {
            PWBCore.SetActiveTempColliders(SelectionManager.topLevelSelection, false);
            var placeOnSurfaceData = new PlaceOnSurfaceUtils.PlaceOnSurfaceData();
            placeOnSurfaceData.projectionDirectionSpace = Space.World;
            placeOnSurfaceData.rotateToSurface = false;
            var objHeight = new float[SelectionManager.topLevelSelection.Length];
            for (int i = 0; i < SelectionManager.topLevelSelection.Length; ++i)
            {
                var obj = SelectionManager.topLevelSelection[i];
                objHeight[i] = BoundsUtils.GetMagnitude(obj.transform);
                obj.SetActive(false);
            }
            var maxSurfMag = 0f;
            for (int i = 0; i < SelectionManager.topLevelSelection.Length; ++i)
            {
                var obj = SelectionManager.topLevelSelection[i];
                obj.transform.rotation = Quaternion.Euler(0, obj.transform.rotation.eulerAngles.y, 0);
                var bottomVertices = BoundsUtils.GetBottomVertices(obj.transform);
                var TRS = obj.transform.localToWorldMatrix;
                maxSurfMag = Mathf.Max(maxSurfMag, _selectionSurfaceMagnitude);
                var magnitude = BoundsUtils.GetMagnitude(obj.transform) + maxSurfMag;
                Transform surfaceTransform;
                var exceptions = new System.Collections.Generic.HashSet<GameObject>() { obj };
                var surfceDistance = SelectionToolManager.settings.embedAtPivotHeight
                    ? GetPivotDistanceToSurfaceSigned(obj.transform.position, magnitude, paintOnPalettePrefabs: true,
                    castOnMeshesWithoutCollider: true, ignoreSceneColliders: true, out surfaceTransform,
                    exceptions)
                    : GetBottomDistanceToSurface(bottomVertices, TRS, magnitude,
                     paintOnPalettePrefabs: true, castOnMeshesWithoutCollider: true, ignoreSceneColliders: true,
                     out surfaceTransform, exceptions);

                if (surfaceTransform != null)
                {
                    var surfId = surfaceTransform.GetInstanceID();
                    if (_selectionSurfaceMagnitudeDic.ContainsKey(surfId))
                        _selectionSurfaceMagnitude = _selectionSurfaceMagnitudeDic[surfId];
                    else
                    {
                        var m = BoundsUtils.GetMagnitude(surfaceTransform);
                        _selectionSurfaceMagnitude = m;
                        _selectionSurfaceMagnitudeDic.Add(surfId, m);
                    }
                }

                surfceDistance -= SelectionToolManager.settings.surfaceDistance;
                if (surfceDistance != 0f)
                {
                    var euler = obj.transform.rotation.eulerAngles;
                    var delta = obj.transform.rotation * new Vector3(0f, -surfceDistance, 0f);
                    obj.transform.position += obj.transform.rotation * new Vector3(0f, -surfceDistance, 0f);
                }
                if (SelectionToolManager.settings.rotateToTheSurface)
                {
                    var down = obj.transform.rotation * Vector3.down;
                    var ray = new Ray(obj.transform.position - down * objHeight[i], down);
                    if (PWBToolRaycast(ray, out RaycastHit hitInfo, out GameObject collider, float.MaxValue, layerMask: -1,
                        paintOnPalettePrefabs: true, castOnMeshesWithoutCollider: true, ignoreSceneColliders: true,
                        exceptions: new System.Collections.Generic.HashSet<GameObject>() { obj }, createTempColliders: true))
                    {
                        var initialRotation = obj.transform.rotation;
                        if (!_editingSelectionRotation)
                        {
                            initialRotation = rotation;
                            if (_initialRotations.ContainsKey(i)) initialRotation = _initialRotations[i];
                        }
                        obj.transform.rotation = GetRotationFromNormal(hitInfo.normal, initialRotation);

                    }
                }
            }
            foreach (var obj in SelectionManager.topLevelSelection) obj.SetActive(true);
            _selectionBounds = BoundsUtils.GetSelectionBounds(SelectionManager.topLevelSelection, rotation);
            PWBCore.SetActiveTempColliders(SelectionManager.topLevelSelection, true);
        }

        public static void EmbedSelectionInSurface()
            => EmbedSelectionInSurface(_selectionRotation);

        private static void RotateSelection90Deg(Vector3 axis, System.Collections.Generic.List<Vector3> points)
        {
            var rotation = _selectionRotation;
            foreach (var obj in SelectionManager.topLevelSelection)
            {
                if (obj == null)
                {
                    SelectionManager.UpdateSelection();
                    return;
                }
                UnityEditor.Undo.RecordObject(obj.transform, "Rotate Selection");
                obj.transform.RotateAround(points[_selectedBoxPointIdx < 0 ? 10 : _selectedBoxPointIdx],
                    rotation * axis, 90);
            }
            _selectionRotation = rotation * Quaternion.AngleAxis(90, axis);
            var localCenter = _selectionBounds.center - points[_selectedBoxPointIdx];
            _selectionBounds.center = (Quaternion.AngleAxis(90, axis) * localCenter) + points[_selectedBoxPointIdx];
            if (SelectionToolManager.settings.embedInSurface) EmbedSelectionInSurface();
            PWBCore.UpdateTempCollidersTransforms(SelectionManager.topLevelSelection);
        }

        private static bool MoveSelection(
            System.Collections.Generic.List<Vector3> points, UnityEditor.SceneView sceneView, Vector3 position)
        {
            void SetSetectedPoint(Vector3 value) => points[_selectedBoxPointIdx] = value;
            var prevPosition = points[_selectedBoxPointIdx];
            if (_snapToVertex)
            {
                if (SnapToVertex(UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition),
                    out RaycastHit closestVertexInfo, sceneView.in2DMode, null))
                    SetSetectedPoint(closestVertexInfo.point);
            }
            else
            {
                var snappedPoint = SnapToBounds(position);
                snappedPoint = SnapAndUpdateGridOrigin(snappedPoint,
                    SnapManager.settings.snappingEnabled, paintOnPalettePrefabs: true, paintOnMeshesWithoutCollider: true,
                    ignoresceneColliders: true, paintOnTheGrid: true, Vector3.down);
                SetSetectedPoint(snappedPoint);
            }

            if (prevPosition == points[_selectedBoxPointIdx]) return false;
            if (_snappedPointIsSelected) _snappedPoint = points[_selectedBoxPointIdx];

            var delta = points[_selectedBoxPointIdx] - prevPosition;
            foreach (var obj in SelectionManager.topLevelSelection)
            {
                if (obj == null)
                {
                    SelectionManager.UpdateSelection();
                    return false;
                }
                UnityEditor.Undo.RecordObject(obj.transform, "Move Selection");
                obj.transform.position += delta;
            }
            _selectionBounds.center += delta;
            return true;
        }
        private static int _previousHotControl;
        private static Vector3 _selectionHandlePosition = Vector3.zero;
        private static bool _movingSelectionHandle = false;
        private static Quaternion _selectionHnadleRotation = Quaternion.identity;

        private static bool MoveSelectionToHandlePosition(Quaternion rotation,
            System.Collections.Generic.List<Vector3> points, UnityEditor.SceneView sceneView)
        {
            if (!SelectionToolManager.settings.move) return false;

            if (SelectionToolManager.settings.handleSpace == Space.World) rotation = Quaternion.identity;
            else if (SelectionManager.topLevelSelection.Length == 1)
                rotation = SelectionManager.topLevelSelection[0].transform.rotation;

            int hotControlBefore = GUIUtility.hotControl;

            var handlePosition = _movingSelectionHandle ? _selectionHandlePosition : points[_selectedBoxPointIdx];
            var r = _movingSelectionHandle ? _selectionHnadleRotation : rotation;
            handlePosition = UnityEditor.Handles.PositionHandle(handlePosition, r);
            if (_editingSelectionRotation) return false;
            _selectionHandlePosition = handlePosition;
            int hotControlAfter = GUIUtility.hotControl;
            if (_previousHotControl != hotControlAfter)
            {
                if (hotControlBefore == 0 && hotControlAfter != 0)
                {
                    _initialRotations.Clear();
                    for (int i = 0; i < SelectionManager.topLevelSelection.Length; ++i)
                        _initialRotations.Add(i, SelectionManager.topLevelSelection[i].transform.rotation);
                    _movingSelectionHandle = true;
                    _selectionHandlePosition = points[_selectedBoxPointIdx];
                    _selectionHnadleRotation = rotation;
                }
                else if (hotControlBefore != 0 && hotControlAfter == 0)
                {
                    _movingSelectionHandle = false;
                }
            }
            _previousHotControl = hotControlAfter;

            return MoveSelection(points, sceneView, handlePosition);
        }

        private static bool _moveSelectionToMousePositionEnabled = false;
        private static bool MoveSelectionToMousePosition(
            System.Collections.Generic.List<Vector3> points, UnityEditor.SceneView sceneView)
        {
            var shortcutCheck = PWBSettings.shortcuts.selectionMoveToMousePosition
                .holdKeysAndMouseCombination.CheckIsHoldingKeys();
            if (shortcutCheck && !_moveSelectionToMousePositionEnabled)
            {
                _initialRotations.Clear();
                for (int i = 0; i < SelectionManager.topLevelSelection.Length; ++i)
                    _initialRotations.Add(i, SelectionManager.topLevelSelection[i].transform.rotation);
            }
            else if (!shortcutCheck && _moveSelectionToMousePositionEnabled) _initialRotations.Clear();

            _moveSelectionToMousePositionEnabled = shortcutCheck;
            if (!PWBSettings.shortcuts.selectionMoveToMousePosition.Check()) return false;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var nearbyObjects = boundsOctree.GetColliding(mouseRay, float.MaxValue);


            if (!MeshUtils.Raycast(mouseRay, out RaycastHit hitInfo, out GameObject collider, nearbyObjects, float.MaxValue))
                return false;
            return MoveSelection(points, sceneView, hitInfo.point);
        }

        private static bool RotateSelection(Quaternion rotation,
            System.Collections.Generic.List<Vector3> points, UnityEditor.SceneView sceneView)
        {
            if (!SelectionToolManager.settings.rotate) return false;
            if (SelectionToolManager.settings.handleSpace == Space.Self && SelectionManager.topLevelSelection.Length == 1)
            {
                rotation = SelectionManager.topLevelSelection[0].transform.rotation;
            }
            int hotControlBefore = GUIUtility.hotControl;

            var prevRotation = rotation;
            var handlePosition = _movingSelectionHandle ? _selectionHandlePosition : points[_selectedBoxPointIdx];
            var newRotation = UnityEditor.Handles.RotationHandle(prevRotation, handlePosition);
            int hotControlAfter = GUIUtility.hotControl;
            if (_previousHotControl != hotControlAfter)
            {
                if (hotControlBefore == 0 && hotControlAfter != 0)
                {
                    _initialRotations.Clear();
                    for (int i = 0; i < SelectionManager.topLevelSelection.Length; ++i)
                        _initialRotations.Add(i, SelectionManager.topLevelSelection[i].transform.rotation);
                    _movingSelectionHandle = true;
                    _selectionHandlePosition = points[_selectedBoxPointIdx];
                    _editingSelectionRotation = true;
                }
                else if (hotControlBefore != 0 && hotControlAfter == 0)
                {
                    _movingSelectionHandle = false;
                    _editingSelectionRotation = false;
                }
            }

            _previousHotControl = hotControlAfter;
            if (!_movingSelectionHandle) return false;

            _selectionRotation = newRotation;
            var angle = Quaternion.Angle(prevRotation, newRotation);
            var axis = Vector3.Cross(prevRotation * Vector3.forward, newRotation * Vector3.forward);
            if (axis == Vector3.zero) axis = Vector3.Cross(prevRotation * Vector3.up, newRotation * Vector3.up);
            axis.Normalize();
            foreach (var obj in SelectionManager.topLevelSelection)
            {
                if (obj == null)
                {
                    SelectionManager.UpdateSelection();
                    return false;
                }
                UnityEditor.Undo.RecordObject(obj.transform, "Rotate Selection");
                obj.transform.RotateAround(points[_selectedBoxPointIdx], axis, angle);
            }
            var localCenter = _selectionBounds.center - points[_selectedBoxPointIdx];
            _selectionBounds.center = (Quaternion.AngleAxis(angle, axis) * localCenter)
                + points[_selectedBoxPointIdx];
            if (SelectionToolManager.settings.rotateToTheSurface)
            {
                MoveSelection(points, sceneView, handlePosition);
            }
            return true;
        }

        private static bool ScaleSelection(Quaternion rotation, System.Collections.Generic.List<Vector3> points)
        {
            if (!SelectionToolManager.settings.scale) return false;
            var prevScale = _selectionScale;
            var newScale = UnityEditor.Handles.ScaleHandle(prevScale, points[_selectedBoxPointIdx],
                rotation, UnityEditor.HandleUtility.GetHandleSize(points[_selectedBoxPointIdx]) * 1.4f);
            if (prevScale == newScale) return false;
            _selectionScale = newScale;
            var scaleFactor = new Vector3(
                prevScale.x == 0 ? newScale.x : newScale.x / prevScale.x,
                prevScale.y == 0 ? newScale.y : newScale.y / prevScale.y,
                prevScale.z == 0 ? newScale.z : newScale.z / prevScale.z);
            var pivot = new GameObject();
            pivot.hideFlags = HideFlags.HideAndDontSave;
            pivot.transform.position = points[_selectedBoxPointIdx];
            pivot.transform.rotation = rotation;
            foreach (var obj in SelectionManager.topLevelSelection)
            {
                if (obj == null)
                {
                    SelectionManager.UpdateSelection();
                    break;
                }
                UnityEditor.Undo.RecordObject(obj.transform, "Scale Selection");
                pivot.transform.localScale = Vector3.one;
                var localPosition = pivot.transform.InverseTransformPoint(obj.transform.position);
                pivot.transform.localScale = scaleFactor;
                obj.transform.position = pivot.transform.TransformPoint(localPosition);
                obj.transform.localScale = Vector3.Scale(obj.transform.localScale, scaleFactor);
            }
            GameObject.DestroyImmediate(pivot);
            var pivotToCenter = _selectionBounds.center - points[_selectedBoxPointIdx];
            _selectionBounds.center = points[_selectedBoxPointIdx] + Vector3.Scale(pivotToCenter, scaleFactor);
            _selectionBounds.size = Vector3.Scale(_selectionBounds.size, scaleFactor);
            return true;
        }
        public static void ResetSelectionRotation()
        {
            _selectionRotation = Quaternion.identity;
            UpdateSelection();
        }

        private static Quaternion GetSelectionRotation()
        {
            var rotation = _selectionRotation;
            if (SelectionManager.topLevelSelection.Length == 1)
            {
                if (SelectionManager.topLevelSelection[0] == null) SelectionManager.UpdateSelection();
                else if (SelectionToolManager.settings.boxSpace == Space.Self)
                    rotation = SelectionManager.topLevelSelection[0].transform.rotation;
            }
            else if (SelectionToolManager.settings.handleSpace == Space.Self)
            {
                var count = 0;
                var avgForward = Vector3.forward;
                var avgUp = Vector3.up;
                if (SelectionManager.topLevelSelection.Length > 0)
                {
                    avgForward = Vector3.zero;
                    avgUp = Vector3.zero;
                }
                foreach (var obj in SelectionManager.topLevelSelection)
                {
                    if (obj == null) continue;
                    ++count;
                    avgForward += obj.transform.rotation * Vector3.forward;
                    avgUp += obj.transform.rotation * Vector3.up;
                }
                avgForward /= count;
                avgUp /= count;
                rotation = Quaternion.LookRotation(avgForward, avgUp);
            }
            return rotation;
        }

        private static System.Collections.Generic.List<Vector3> SelectionPoints(Camera camera)
        {
            var rotation = GetSelectionRotation();
            var bounds = _selectionBounds;
            var halfSizeRotated = rotation * bounds.size / 2;
            var min = bounds.center - halfSizeRotated;
            var max = bounds.center + halfSizeRotated;
            var points = new System.Collections.Generic.List<Vector3>
            {
                min,
                min + rotation * new Vector3(bounds.size.x, 0f, 0f),
                min + rotation * new Vector3(bounds.size.x, 0f, bounds.size.z),
                min + rotation * new Vector3(0f, 0f, bounds.size.z),
                min + rotation * new Vector3(0f, bounds.size.y, 0f),
                min + rotation * new Vector3(bounds.size.x, bounds.size.y, 0f),
                max,
                min + rotation * new Vector3(0f, bounds.size.y, bounds.size.z),
                min + rotation * new Vector3(bounds.size.x, 0f, bounds.size.z) / 2,
                max - rotation * new Vector3(bounds.size.x, 0f, bounds.size.z) / 2,
            };

            var visibleIdx = GetVisiblePoints(points.ToArray(), camera);

            points.Add(bounds.center);
            points.Add(bounds.center + _selectionRotation * _tempSelectionHandle);

            void DrawLine(Vector3[] line, float alpha = 1f)
            {
                UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.5f);
                UnityEditor.Handles.DrawAAPolyLine(10, line);
                UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.3f * alpha);
                UnityEditor.Handles.DrawAAPolyLine(4, line);
            }

            var visibleLines = new System.Collections.Generic.List<Vector3[]>();
            float ocludedAlpha = 0.5f;
            for (int i = 0; i < 8; ++i)
            {
                var visibleLine = visibleIdx.Contains(i) && visibleIdx.Contains(i + 4);
                if (i < 4)
                {
                    var vLine = new Vector3[] { points[i],
                        points[i] + rotation * new Vector3(0f, bounds.size.y, 0f) };
                    if (visibleLine) visibleLines.Add(vLine);
                    else DrawLine(vLine, ocludedAlpha);
                    points.Add(vLine[0] + (vLine[1] - vLine[0]) / 2);
                }
                int nextI = ((i + 1) % 4) + 4 * (i / 4);
                visibleLine = visibleIdx.Contains(i) && visibleIdx.Contains(nextI);
                var hLine = new Vector3[] { points[i], points[nextI] };
                if (visibleLine) visibleLines.Add(hLine);
                else DrawLine(hLine, ocludedAlpha);
                var midpoint = hLine[0] + (hLine[1] - hLine[0]) / 2;
                points.Add(midpoint);
                if (i < 4) points.Add(midpoint + rotation * new Vector3(0f, bounds.size.y / 2, 0f));
            }
            foreach (var line in visibleLines) DrawLine(line);
            for (int i = 0; i < 8; ++i)
            {
                var alpha = visibleIdx.Contains(i) ? 1f : 0.3f;
                DrawDotHandleCap(points[i], alpha);
            }
            DrawDotHandleCap(points[11], 1);
            if (SelectionManager.topLevelSelection.Length == 1)
            {
                var pivotPosition = SelectionManager.topLevelSelection[0].transform.position;
                points.Add(pivotPosition);
                DrawDotHandleCap(pivotPosition, isPivot: true);
            }
            return points;
        }

        public static void SetSelectionOriginPosition() => _setSelectionOriginPosition = true;
        #region VISIBLE POINTS
        private static System.Collections.Generic.HashSet<int> GetVisiblePoints(Vector3[] points, Camera camera)
        {
            var resultSet = new System.Collections.Generic.HashSet<int>(GrahamScan(points));
            if (resultSet.Count == 6)
            {
                var ocluded = new System.Collections.Generic.List<int>();
                for (int i = 0; i < points.Length; ++i)
                {
                    if (resultSet.Contains(i)) continue;
                    ocluded.Add(i);
                }
                if ((ocluded[0] / 4 == ocluded[1] / 4) || (ocluded[1] == ocluded[0] + 4))
                    return resultSet;
                var nearestIdx = camera.transform.InverseTransformPoint(points[ocluded[0]]).z
                    < camera.transform.InverseTransformPoint(points[ocluded[1]]).z ? ocluded[0] : ocluded[1];
                resultSet.Add(nearestIdx);
            }
            return resultSet;
        }
        private static int[] GrahamScan(Vector3[] points)
        {
            var screenPoints = new System.Collections.Generic.List<BoxPoint>();
            for (int i = 0; i < points.Length; ++i)
                screenPoints.Add(new BoxPoint(i, UnityEditor.HandleUtility.WorldToGUIPoint(points[i])));
            var p0 = screenPoints[0];
            foreach (var value in screenPoints)
            {
                if (p0.point.y > value.point.y) p0 = value;
            }
            var order = new System.Collections.Generic.List<BoxPoint>();
            foreach (var point in screenPoints)
            {
                if (p0 != point) order.Add(point);
            }
            order = MergeSort(p0, order);
            var result = new System.Collections.Generic.List<BoxPoint>();
            result.Add(p0);
            result.Add(order[0]);
            result.Add(order[1]);
            order.RemoveAt(0);
            order.RemoveAt(0);
            foreach (var value in order) KeepLeft(result, value);
            var resultIdx = new int[result.Count];
            for (int i = 0; i < result.Count; ++i) resultIdx[i] = result[i];
            return resultIdx;
        }

        private class BoxPoint
        {
            public int idx = -1;
            public Vector2 point = Vector2.zero;
            public BoxPoint(int idx, Vector2 point) => (this.idx, this.point) = (idx, point);
            public override int GetHashCode()
            {
                int hashCode = 386348313;
                hashCode = hashCode * -1521134295 + idx.GetHashCode();
                hashCode = hashCode * -1521134295 + point.GetHashCode();
                return hashCode;
            }
            public bool Equals(BoxPoint other) => GetHashCode() == other.GetHashCode();
            public override bool Equals(object obj) => Equals(obj as BoxPoint);
            public static bool operator ==(BoxPoint l, BoxPoint r) => l.Equals(r);
            public static bool operator !=(BoxPoint l, BoxPoint r) => !l.Equals(r);
            public static implicit operator Vector2(BoxPoint value) => value.point;
            public static implicit operator int(BoxPoint value) => value.idx;
        }

        private static System.Collections.Generic.List<BoxPoint> MergeSort(BoxPoint p0,
            System.Collections.Generic.List<BoxPoint> pointList)
        {
            if (pointList.Count == 1) return pointList;
            var sortedList = new System.Collections.Generic.List<BoxPoint>();
            int middle = pointList.Count / 2;
            var leftArray = pointList.GetRange(0, middle);
            var rightArray = pointList.GetRange(middle, pointList.Count - middle);
            leftArray = MergeSort(p0, leftArray);
            rightArray = MergeSort(p0, rightArray);
            int leftptr = 0;
            int rightptr = 0;
            for (int i = 0; i < leftArray.Count + rightArray.Count; i++)
            {
                if (leftptr == leftArray.Count)
                {
                    sortedList.Add(rightArray[rightptr]);
                    rightptr++;
                }
                else if (rightptr == rightArray.Count)
                {
                    sortedList.Add(leftArray[leftptr]);
                    leftptr++;
                }
                else if (GetAngle(p0, leftArray[leftptr]) < GetAngle(p0, rightArray[rightptr]))
                {
                    sortedList.Add(leftArray[leftptr]);
                    leftptr++;
                }
                else
                {
                    sortedList.Add(rightArray[rightptr]);
                    rightptr++;
                }
            }
            return sortedList;
        }

        private static double GetAngle(Vector2 p1, Vector2 p2)
        {
            float xDiff = p2.x - p1.x;
            float yDiff = p2.y - p1.y;
            return Mathf.Atan2(yDiff, xDiff) * 180f / Mathf.PI;
        }

        private static void KeepLeft(System.Collections.Generic.List<BoxPoint> hull, BoxPoint point)
        {
            int turn(Vector2 p, Vector2 q, Vector2 r)
                => ((q.x - p.x) * (r.y - p.y) - (r.x - p.x) * (q.y - p.y)).CompareTo(0);
            while (hull.Count > 1 && turn(hull[hull.Count - 2], hull[hull.Count - 1], point) != 1)
                hull.RemoveAt(hull.Count - 1);
            if (hull.Count == 0 || hull[hull.Count - 1] != point) hull.Add(point);
        }
        #endregion
    }
    #endregion
}
