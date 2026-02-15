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
    #region ITEM
    public struct BrushstrokeObject : System.IEquatable<BrushstrokeObject>
    {
        public readonly int objIdx;
        public readonly Vector3 objPosition;
        public readonly Quaternion objRotation;
        public readonly Vector3 additionalAngle;
        public readonly Vector3 objScale;
        public readonly bool flipX;
        public readonly bool flipY;
        public readonly float surfaceDistance;
        public readonly Vector3 brushstrokeDirection;

        public BrushstrokeObject(int objIdx, Vector3 objPosition, Quaternion objRotation, Vector3 additionalAngle,
            Vector3 objScale, bool flipX, bool flipY, float surfaceDistance, Vector3 brushstrokeDirection)
        {
            this.objIdx = objIdx;
            this.objPosition = objPosition;
            this.objRotation = objRotation;
            this.additionalAngle = additionalAngle;
            this.objScale = objScale;
            this.flipX = flipX;
            this.flipY = flipY;
            this.surfaceDistance = surfaceDistance;
            this.brushstrokeDirection = brushstrokeDirection;
        }

        public BrushstrokeObject Clone()
        {
            var clone = new BrushstrokeObject(objIdx, objPosition, objRotation, additionalAngle,
                objScale, flipX, flipY, surfaceDistance, brushstrokeDirection);
            return clone;
        }

        public bool Equals(BrushstrokeObject other)
        {
            return objPosition == other.objPosition && objRotation == other.objRotation
                && additionalAngle == other.additionalAngle && objScale == other.objScale
                && flipX == other.flipX && flipY == other.flipY && surfaceDistance == other.surfaceDistance
                && brushstrokeDirection == other.brushstrokeDirection;
        }
        public static bool operator ==(BrushstrokeObject lhs, BrushstrokeObject rhs) => lhs.Equals(rhs);
        public static bool operator !=(BrushstrokeObject lhs, BrushstrokeObject rhs) => !lhs.Equals(rhs);

        public override bool Equals(object obj) => obj is BrushstrokeObject other && Equals(other);

        public override int GetHashCode()
        {
            int hashCode = 861157388;
            hashCode = hashCode * -1521134295 + objIdx.GetHashCode();
            hashCode = hashCode * -1521134295 + objPosition.GetHashCode();
            hashCode = hashCode * -1521134295 + objRotation.GetHashCode();
            hashCode = hashCode * -1521134295 + additionalAngle.GetHashCode();
            hashCode = hashCode * -1521134295 + objScale.GetHashCode();
            hashCode = hashCode * -1521134295 + flipX.GetHashCode();
            hashCode = hashCode * -1521134295 + flipY.GetHashCode();
            hashCode = hashCode * -1521134295 + surfaceDistance.GetHashCode();
            hashCode = hashCode * -1521134295 + brushstrokeDirection.GetHashCode();
            return hashCode;
        }
    }

    public struct BrushstrokeItem : System.IEquatable<BrushstrokeItem>
    {
        public readonly MultibrushItemSettings settings;
        public readonly Vector3 tangentPosition;
        public readonly Vector3 additionalAngle;
        public readonly Vector3 scaleMultiplier;
        public Vector3 nextTangentPosition;
        public readonly bool flipX;
        public readonly bool flipY;
        public readonly float surfaceDistance;
        public readonly int index;
        public readonly int tokenIndex;

        public BrushstrokeItem(int index, int tokenIndex, MultibrushItemSettings settings, Vector3 tangentPosition,
            Vector3 additionalAngle, Vector3 scaleMultiplier, bool flipX, bool flipY, float surfaceDistance)
        {
            this.settings = settings;
            this.tangentPosition = tangentPosition;
            this.additionalAngle = additionalAngle;
            this.scaleMultiplier = scaleMultiplier;
            nextTangentPosition = tangentPosition;
            this.flipX = flipX;
            this.flipY = flipY;
            this.surfaceDistance = surfaceDistance;
            this.index = index;
            this.tokenIndex = tokenIndex;
        }

        public BrushstrokeItem Clone()
        {
            var clone = new BrushstrokeItem(index, tokenIndex, settings, tangentPosition, additionalAngle,
                scaleMultiplier, flipX, flipY, surfaceDistance);
            clone.nextTangentPosition = nextTangentPosition;
            return clone;
        }

        public bool Equals(BrushstrokeItem other)
        {
            return settings == other.settings && tangentPosition == other.tangentPosition
                && additionalAngle == other.additionalAngle && scaleMultiplier == other.scaleMultiplier
                && nextTangentPosition == other.nextTangentPosition;
        }
        public static bool operator ==(BrushstrokeItem lhs, BrushstrokeItem rhs) => lhs.Equals(rhs);
        public static bool operator !=(BrushstrokeItem lhs, BrushstrokeItem rhs) => !lhs.Equals(rhs);

        public override bool Equals(object obj) => obj is BrushstrokeItem other && Equals(other);

        public override int GetHashCode()
        {
            int hashCode = 861157388;
            hashCode = hashCode * -1521134295
                + System.Collections.Generic.EqualityComparer<MultibrushItemSettings>.Default.GetHashCode(settings);
            hashCode = hashCode * -1521134295 + tangentPosition.GetHashCode();
            hashCode = hashCode * -1521134295 + additionalAngle.GetHashCode();
            hashCode = hashCode * -1521134295 + scaleMultiplier.GetHashCode();
            hashCode = hashCode * -1521134295 + nextTangentPosition.GetHashCode();
            hashCode = hashCode * -1521134295 + flipX.GetHashCode();
            hashCode = hashCode * -1521134295 + flipY.GetHashCode();
            hashCode = hashCode * -1521134295 + surfaceDistance.GetHashCode();
            return hashCode;
        }
    }
    #endregion
    public static class BrushstrokeManager
    {
        #region COMMON
        private static System.Collections.Generic.List<BrushstrokeItem> _brushstroke
            = new System.Collections.Generic.List<BrushstrokeItem>();
        public static BrushstrokeItem[] brushstroke => _brushstroke.ToArray();
        public static int itemCount => _brushstroke.Count;

        public static void ClearBrushstroke() => _brushstroke.Clear();

        public static BrushstrokeItem[] brushstrokeClone
        {
            get
            {
                var clone = new BrushstrokeItem[_brushstroke.Count];
                for (int i = 0; i < clone.Length; ++i) clone[i] = _brushstroke[i].Clone();
                return clone;
            }
        }

        public static bool BrushstrokeEqual(BrushstrokeItem[] lhs, BrushstrokeItem[] rhs)
        {
            if (lhs.Length != rhs.Length) return false;
            for (int i = 0; i < lhs.Length; ++i)
                if (lhs[i] != rhs[i]) return false;
            return true;
        }
        private static void AddBrushstrokeItem(int index, int tokenIndex, Vector3 tangentPosition,
            Vector3 angle, Vector3 scale, IPaintToolSettings paintToolSettings)
        {
            if (index < 0 || index >= PaletteManager.selectedBrush.itemCount) return;

            var multiBrushSettings = PaletteManager.selectedBrush;
            BrushSettings brushSettings = multiBrushSettings.items[index];
            if (paintToolSettings != null && paintToolSettings.overwriteBrushProperties)
                brushSettings = paintToolSettings.brushSettings;

            var additonalAngle = (Quaternion.Euler(angle) * Quaternion.Euler(brushSettings.GetAdditionalAngle())).eulerAngles;
            var flipX = brushSettings.GetFlipX();
            var flipY = brushSettings.GetFlipY();
            var surfaceDistance = brushSettings.GetSurfaceDistance();
            var strokeItem = new BrushstrokeItem(index, tokenIndex,
                PaletteManager.selectedBrush.items[index], tangentPosition, additonalAngle,
                scale, flipX, flipY, surfaceDistance);
            if (_brushstroke.Count > 0)
            {
                var last = _brushstroke.Last();
                last.nextTangentPosition = tangentPosition;
                _brushstroke[_brushstroke.Count - 1] = last;
            }
            _brushstroke.Add(strokeItem);
        }

        private static Vector3 ScaleMultiplier(int itemIdx, IPaintToolSettings settings)
        {
            if (settings.overwriteBrushProperties) return settings.brushSettings.GetScaleMultiplier();
            if (PaletteManager.selectedBrush != null)
            {
                var nextItem = PaletteManager.selectedBrush.items[itemIdx];
                return nextItem.GetScaleMultiplier();
            }
            return Vector3.one;
        }
        public static void UpdateBrushstroke(bool brushChange = false)
        {
            if (ToolManager.tool == ToolManager.PaintTool.SELECTION) return;
            if (ToolManager.tool == ToolManager.PaintTool.LINE
                || ToolManager.tool == ToolManager.PaintTool.SHAPE
                || ToolManager.tool == ToolManager.PaintTool.TILING)
            {
                PWBIO.UpdateStroke();
                return;
            }
            if (!brushChange && ToolManager.tool == ToolManager.PaintTool.PIN && PinManager.settings.repeat) return;
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush == null) return;
            if (ToolManager.tool == ToolManager.PaintTool.BRUSH) UpdateBrushBaseStroke(BrushManager.settings);
            else if (ToolManager.tool == ToolManager.PaintTool.GRAVITY) UpdateBrushBaseStroke(GravityToolManager.settings);
            else if (ToolManager.tool == ToolManager.PaintTool.PIN) UpdateSingleBrushstroke(PinManager.settings);
            else if (ToolManager.tool == ToolManager.PaintTool.REPLACER) _brushstroke.Clear();
            else if (ToolManager.tool == ToolManager.PaintTool.FLOOR) UpdateFloorBrushstroke(setNextIdx: false);
            else if (ToolManager.tool == ToolManager.PaintTool.WALL)
                UpdateWallBrushstroke(WallManager.wallLenghtAxis, cellsCount: 1, setNextIdx: false, deleteMode: false);
        }
        #endregion

        #region LINE
        public static float _minLineSpacing = float.MaxValue;
        public static float GetLineSpacing(int itemIdx, LineSettings settings, Vector3 scaleMult)
        {
            float spacing = 0;
            if (itemIdx >= 0) spacing = settings.spacing;

            if (settings.spacingType == LineSettings.SpacingType.BOUNDS && itemIdx >= 0)
            {
                var item = PaletteManager.selectedBrush.items[itemIdx];
                if (item.prefab == null) return spacing;
                var bounds = BoundsUtils.GetBoundsRecursive(item.prefab.transform);
                var size = Vector3.Scale(bounds.size, scaleMult);
                var axis = settings.axisOrientedAlongTheLine;
                var sceneView = UnityEditor.SceneView.currentDrawingSceneView ?? UnityEditor.SceneView.lastActiveSceneView;
                if (item.isAsset2D && sceneView != null && sceneView.in2DMode
                    && axis == AxesUtils.Axis.Z) axis = AxesUtils.Axis.Y;
                spacing = AxesUtils.GetAxisValue(size, axis);
                if (spacing <= 0.0001) spacing = 0.5f;
            }
            spacing += settings.gapSize;
            _minLineSpacing = Mathf.Min(spacing, _minLineSpacing);
            return spacing;
        }
        private static void UpdateLineBrushstroke(Vector3[] points, LineSettings settings)
        {
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush == null) return;

            float lineLength = 0f;
            var lengthFromFirstPoint = new float[points.Length];
            var segmentLength = new float[points.Length];
            lengthFromFirstPoint[0] = 0f;
            for (int i = 1; i < points.Length; ++i)
            {
                segmentLength[i - 1] = (points[i] - points[i - 1]).magnitude;
                lineLength += segmentLength[i - 1];
                lengthFromFirstPoint[i] = lineLength;
            }

            float length = 0f;
            int segment = 0;
            if (PaletteManager.selectedBrush.patternMachine != null)
                PaletteManager.selectedBrush.patternMachine.Reset();
            var prefabSpacingDictionary = new System.Collections.Generic.Dictionary<(int, Vector3), float>();

            var brush = PaletteManager.selectedBrush;

            float Spacing(int itemIdx, Vector3 scale)
            {
                float spacing = 0;
                var item = brush.items[itemIdx];

                if (settings.spacingType == LineSettings.SpacingType.BOUNDS)
                {
                    var key = (itemIdx, scale);
                    if (item.randomScaleMultiplier) spacing = GetLineSpacing(itemIdx, settings, scale);
                    else if (prefabSpacingDictionary.ContainsKey(key)) spacing = prefabSpacingDictionary[key];
                    else
                    {
                        spacing = GetLineSpacing(itemIdx, settings, scale);
                        prefabSpacingDictionary.Add(key, spacing);
                    }
                }
                else spacing = GetLineSpacing(itemIdx, settings, scale);
                return spacing;
            }

            float endLenght = 0f;
            int[] endIndexes = null;
            if (brush.frequencyMode == MultibrushSettings.FrequencyMode.PATTERN)
            {
                endIndexes = brush.patternMachine.GetEndIndexes();
                foreach (var i in endIndexes)
                {
                    var idx = i - 1;
                    var item = brush.items[idx];
                    var scale = ScaleMultiplier(idx, LineManager.settings);
                    endLenght += Spacing(idx, scale);
                }
            }
            int currentEndIdx = 0;
            bool useEndIndexes = false;
            do
            {
                var nextIdx = brush.nextItemIndex;
                if (nextIdx < 0) break;
                if (useEndIndexes)
                {
                    if (currentEndIdx >= endIndexes.Length) break;
                    nextIdx = endIndexes[currentEndIdx] - 1;
                    ++currentEndIdx;
                    if (currentEndIdx == 1 && endIndexes.Length > 1)
                    {
                        while (endLenght > lineLength - length)
                        {
                            nextIdx = endIndexes[currentEndIdx] - 1;
                            var s = ScaleMultiplier(nextIdx, LineManager.settings);
                            endLenght -= Spacing(nextIdx, s);
                            ++currentEndIdx;
                            if (currentEndIdx >= endIndexes.Length) break;
                        }
                        if (currentEndIdx >= endIndexes.Length) break;
                    }
                }
                while (lengthFromFirstPoint[segment + 1] < length)
                {
                    ++segment;
                    if (segment >= points.Length - 1) break;
                }
                if (segment >= points.Length - 1) break;
                var segmentDirection = (points[segment + 1] - points[segment]).normalized;
                var distance = length - lengthFromFirstPoint[segment];
                var position = points[segment] + segmentDirection * distance;
                var scale = ScaleMultiplier(nextIdx, LineManager.settings);
                float spacing = Spacing(nextIdx, scale);

                var delta = Mathf.Max(spacing, _minLineSpacing);
                if (delta <= 0) break;
                spacing = Mathf.Max(spacing, _minLineSpacing);
                if (!useEndIndexes && brush.frequencyMode == MultibrushSettings.FrequencyMode.PATTERN
                    && endLenght > 0 && length + spacing > lineLength - endLenght && currentEndIdx == 0)
                {
                    useEndIndexes = true;
                    continue;
                }

                length += spacing;

                if (length > lineLength) break;
                AddBrushstrokeItem(nextIdx, PaletteManager.selectedBrush.GetPatternTokenIndex(), position,
                    angle: Vector3.zero, scale, settings);
            } while (length < lineLength);
        }
        public static void UpdateLineBrushstroke(Vector3[] pathPoints)
            => UpdateLineBrushstroke(pathPoints, LineManager.settings);
        private static float GetLineSpacing(Transform transform, LineSettings settings, Vector3 scale, bool useDictionary)
        {
            float spacing = settings.spacing;
            if (settings.spacingType == LineSettings.SpacingType.BOUNDS && transform != null)
            {
                var bounds = BoundsUtils.GetBoundsRecursive(transform, transform.rotation, ignoreDissabled: false,
                     BoundsUtils.ObjectProperty.BOUNDING_BOX, recursive: true, useDictionary);
                var size = Vector3.Scale(bounds.size, scale);
                var axis = settings.axisOrientedAlongTheLine;
                var sceneView = UnityEditor.SceneView.currentDrawingSceneView ?? UnityEditor.SceneView.lastActiveSceneView;
                if (Utils2D.Is2DAsset(transform.gameObject) && sceneView != null
                    && sceneView.in2DMode && axis == AxesUtils.Axis.Z)
                    axis = AxesUtils.Axis.Y;
                spacing = AxesUtils.GetAxisValue(size, axis);
            }
            spacing += settings.gapSize;
            _minLineSpacing = Mathf.Min(spacing, _minLineSpacing);
            return spacing;
        }

        public static void UpdatePersistentLineBrushstroke(Vector3[] pathPoints,
            LineSettings toolSettings, System.Collections.Generic.List<GameObject> lineObjects,
            out BrushstrokeObject[] objPositions, out Vector3[] strokePositions, out int firstNewObjectIdx)
        {
            _brushstroke.Clear();
            firstNewObjectIdx = 0;
            var objPositionsList = new System.Collections.Generic.List<BrushstrokeObject>();
            var strokePositionsList = new System.Collections.Generic.List<Vector3>();
            float lineLength = 0f;
            var lengthFromFirstPoint = new float[pathPoints.Length];
            var segmentLength = new float[pathPoints.Length];
            lengthFromFirstPoint[0] = 0f;
            for (int i = 1; i < pathPoints.Length; ++i)
            {
                segmentLength[i - 1] = (pathPoints[i] - pathPoints[i - 1]).magnitude;
                lineLength += segmentLength[i - 1];
                lengthFromFirstPoint[i] = lineLength;
            }

            float length = 0f;
            int segment = 0;
            if (PaletteManager.selectedBrush != null)
                if (PaletteManager.selectedBrush.patternMachine != null)
                    PaletteManager.selectedBrush.patternMachine.Reset();
            int objIdx = 0;
            var prefabSpacingDictionary = new System.Collections.Generic.Dictionary<(int, Vector3), float>();

            var brush = PaletteManager.selectedBrush;
            float endLenght = 0f;
            int BeginningObjectCount = lineObjects.Count;
            int endingObjectCount = 0;

            var brushSettings = toolSettings.overwriteBrushProperties ? toolSettings.brushSettings
               : PaletteManager.selectedBrush;
            Vector3 BrushScaleMultiplier() => (LineManager.instance.applyBrushToExisting
                && PaletteManager.selectedBrush != null) ? brushSettings.GetScaleMultiplier() : Vector3.one;
            if (brush != null && brush.frequencyMode == MultibrushSettings.FrequencyMode.PATTERN)
            {
                var endIndexes = brush.patternMachine.GetEndIndexes();
                endingObjectCount = Mathf.Min(endIndexes.Length, lineObjects.Count);
                BeginningObjectCount = lineObjects.Count - endingObjectCount;
                for (int i = 0; i < endingObjectCount; ++i)
                {
                    var obj = lineObjects[lineObjects.Count - 1 - i];
                    var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                    endLenght += GetLineSpacing(prefab.transform, toolSettings, BrushScaleMultiplier(), useDictionary: true);
                }
                endLenght = Mathf.Min(endLenght, lineLength);
            }
            var itemCount = 0;
            float newItemSpacing(int itemIdx, Vector3 scale)
            {
                if (PaletteManager.selectedBrush == null)
                {
                    if (Mathf.Approximately(_minLineSpacing, float.MaxValue)) return 0f;
                    return _minLineSpacing;
                }
                var item = PaletteManager.selectedBrush.items[itemIdx];
                var key = (itemIdx, scale);
                if (toolSettings.spacingType == LineSettings.SpacingType.BOUNDS && itemIdx >= 0)
                {
                    if (item.randomScaleMultiplier) return GetLineSpacing(itemIdx, toolSettings, scale);
                    else if (prefabSpacingDictionary.ContainsKey(key)) return prefabSpacingDictionary[key];
                    else
                    {
                        var spacing = GetLineSpacing(itemIdx, toolSettings, scale);
                        prefabSpacingDictionary.Add(key, spacing);
                        return spacing;
                    }
                }
                else return GetLineSpacing(itemIdx, toolSettings, scale);
            }

            var prevAtTheEnd = false;
            bool firstNewObjectAdded = false;
            do
            {
                var nextIdx = PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.nextItemIndex : -1;

                while (lengthFromFirstPoint[segment + 1] < length)
                {
                    ++segment;
                    if (segment >= pathPoints.Length - 1) break;
                }
                if (segment >= pathPoints.Length - 1) break;

                var segmentDirection = (pathPoints[segment + 1] - pathPoints[segment]).normalized;
                var distance = length - lengthFromFirstPoint[segment];


                var itemScaleMultiplier = ScaleMultiplier(nextIdx, LineManager.settings);
                var itemSpacing = newItemSpacing(nextIdx, itemScaleMultiplier);
                var isAtTheEnd = lineLength - length - itemSpacing <= endLenght;
                if (objIdx < lineObjects.Count && isAtTheEnd && !prevAtTheEnd)
                    objIdx = lineObjects.Count - endingObjectCount;
                prevAtTheEnd = isAtTheEnd;
                var addExistingObject = objIdx < lineObjects.Count && (itemCount < BeginningObjectCount || isAtTheEnd);
                if (addExistingObject && lineObjects[objIdx] == null) addExistingObject = false;
                float spacing = 0;

                var objScale = Vector3.one;
                if (addExistingObject)
                {
                    var obj = lineObjects[objIdx];
                    if (LineManager.instance.applyBrushToExisting)
                    {
                        var brushScaleMultiplier = BrushScaleMultiplier();
                        var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                        spacing = GetLineSpacing(prefab.transform, toolSettings, brushScaleMultiplier, useDictionary: true);
                        objScale = Vector3.Scale(prefab.transform.localScale, brushScaleMultiplier);
                    }
                    else
                    {
                        spacing = GetLineSpacing(obj.transform, toolSettings, Vector3.one, useDictionary: true);
                        objScale = obj.transform.localScale;
                    }
                }
                else if (PaletteManager.selectedBrush != null) spacing = itemSpacing;
                if (spacing == 0) break;
                spacing = Mathf.Max(spacing, _minLineSpacing);
                int nearestPathointIdx;
                var position = pathPoints[segment] + segmentDirection * distance;
                float distanceFromNearestPoint;
                var intersection = LineData.NearestPathPoint(segment, position, spacing, pathPoints, out nearestPathointIdx,
                    out distanceFromNearestPoint);

                if (nearestPathointIdx > segment)
                    spacing = (pathPoints[nearestPathointIdx] - position).magnitude
                        + (intersection - pathPoints[nearestPathointIdx]).magnitude;
                length = Mathf.Max(length + spacing, lengthFromFirstPoint[nearestPathointIdx] + distanceFromNearestPoint);
                if (lineLength < length) break;
                if (addExistingObject)
                {
                    var brushAdditionalAngle = Vector3.zero;
                    bool brushFlipX = false;
                    bool brushFlipY = false;
                    var brushSurfaceDistance = 0f;
                    if (LineManager.instance.applyBrushToExisting)
                    {
                        if (PaletteManager.selectedBrush != null)
                        {
                            brushAdditionalAngle = brushSettings.GetAdditionalAngle();
                            brushFlipX = brushSettings.GetFlipX();
                            brushFlipY = brushSettings.GetFlipY();
                            brushSurfaceDistance = brushSettings.GetSurfaceDistance();
                        }
                    }
                    var startToEnd = intersection - position;
                    var centerPosition = startToEnd / 2 + position;

                    var brushstrokeDirection = toolSettings.objectsOrientedAlongTheLine ? startToEnd.normalized : Vector3.left;
                    objPositionsList.Add(new BrushstrokeObject(objIdx, centerPosition, objRotation: Quaternion.identity,
                        brushAdditionalAngle, objScale, brushFlipX, brushFlipY, brushSurfaceDistance, brushstrokeDirection));
                    ++objIdx;
                    if (isAtTheEnd && objIdx >= lineObjects.Count) break;
                }
                else if (PaletteManager.selectedBrush == null) break;
                else
                {
                    AddBrushstrokeItem(nextIdx, PaletteManager.selectedBrush == null ? 0
                        : PaletteManager.selectedBrush.GetPatternTokenIndex(), position,
                        angle: Vector3.zero, itemScaleMultiplier, LineManager.settings);
                    strokePositionsList.Add(position);
                    if (!firstNewObjectAdded)
                    {
                        firstNewObjectAdded = true;
                        firstNewObjectIdx = itemCount;
                    }
                }
                ++itemCount;

            } while (lineLength > length);
            objPositions = objPositionsList.ToArray();
            strokePositions = strokePositionsList.ToArray();
        }
        #endregion

        #region SHAPE

        private static Vector3 GetAngleToShapeCenter(ShapeSettings settings, Vector3 position,
            Vector3 center, Vector3 itemDir, Quaternion lookAt, Vector3 defaultValue)
        {
            if (settings.projectionDirectionType == ShapeSettings.ShapeProjectionDirection.FROM_CENTER
                || settings.projectionDirectionType == ShapeSettings.ShapeProjectionDirection.TO_CENTER)
            {
                var upwards = settings.projectionDirectionType == ShapeSettings.ShapeProjectionDirection.FROM_CENTER
                    ? position - center : center - position;
                if (settings.shapeType == ShapeSettings.ShapeType.POLYGON) upwards -= Vector3.Project(upwards, itemDir);
                var segmentRotationToCenter = Quaternion.LookRotation(itemDir, upwards) * lookAt;
                return segmentRotationToCenter.eulerAngles;
            }
            return defaultValue;
        }

        public static void UpdateShapeBrushstroke()
        {
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush == null) return;
            var shapeData = ShapeData.instance;
            if (shapeData.state < ToolManager.ToolState.EDIT) return;
            var settings = ShapeManager.settings;
            var points = new System.Collections.Generic.List<Vector3>();
            var firstVertexIdx = shapeData.firstVertexIdxAfterIntersection;
            var lastVertexIdx = shapeData.lastVertexIdxBeforeIntersection;
            int sidesCount = settings.shapeType == ShapeSettings.ShapeType.POLYGON ? settings.sidesCount
                : shapeData.circleSideCount;
            int GetNextVertexIdx(int currentIdx) => currentIdx == sidesCount ? 1 : currentIdx + 1;
            int GetPrevVertexIdx(int currentIdx) => currentIdx == 1 ? sidesCount : currentIdx - 1;
            var firstPrev = GetPrevVertexIdx(firstVertexIdx);
            points.Add(shapeData.GetArcIntersection(0));
            if (lastVertexIdx != firstPrev || (lastVertexIdx == firstPrev && shapeData.arcAngle > 120))
            {
                var vertexIdx = firstVertexIdx;
                var nextVertexIdx = firstVertexIdx;
                do
                {
                    vertexIdx = nextVertexIdx;
                    points.Add(shapeData.GetPoint(vertexIdx));
                    nextVertexIdx = GetNextVertexIdx(nextVertexIdx);
                } while (vertexIdx != lastVertexIdx);
            }
            var lastPoint = shapeData.GetArcIntersection(1);
            if (points.Last() != lastPoint) points.Add(lastPoint);


            var prefabSpacingDictionary = new System.Collections.Generic.Dictionary<(int, Vector3), float>();
            void AddItemsToLine(Vector3 start, Vector3 end, ref int nextIdx)
            {
                if (nextIdx < 0) nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                var startToEnd = end - start;
                var lineLength = startToEnd.magnitude;
                float itemsSize = 0f;
                var items = new System.Collections.Generic.List<(int idx, float size, Vector3 scaleMult)>();

                do
                {
                    var nextItem = PaletteManager.selectedBrush.items[nextIdx];
                    var scaleMult = ScaleMultiplier(nextIdx, ShapeManager.settings);
                    float itemSize;
                    var key = (nextIdx, scaleMult);
                    if (nextItem.randomScaleMultiplier) itemSize = GetLineSpacing(nextIdx, settings, scaleMult);
                    else if (prefabSpacingDictionary.ContainsKey(key)) itemSize = prefabSpacingDictionary[key];
                    else
                    {
                        itemSize = GetLineSpacing(nextIdx, settings, scaleMult);
                        prefabSpacingDictionary.Add(key, itemSize);
                    }
                    itemSize = Mathf.Max(itemSize, _minLineSpacing);
                    if (itemsSize + itemSize > lineLength) break;
                    itemsSize += itemSize;
                    items.Add((nextIdx, itemSize, scaleMult));
                    nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                } while (itemsSize <= lineLength);
                var spacing = (lineLength - itemsSize) / (items.Count + 1);
                var distance = spacing;
                var direction = startToEnd.normalized;

                Vector3 itemDir = (settings.objectsOrientedAlongTheLine && direction != Vector3.zero)
                    ? direction : Vector3.forward;
                var planeUp = shapeData.planeRotation * Vector3.up;
                var lookAt = Quaternion.LookRotation((AxesUtils.SignedAxis)(settings.axisOrientedAlongTheLine), Vector3.up);
                var segmentRotation = Quaternion.LookRotation(itemDir, -settings.projectionDirection) * lookAt;
                var angle = segmentRotation.eulerAngles;
                foreach (var item in items)
                {
                    var brushItem = PaletteManager.selectedBrush.items[item.idx];
                    if (brushItem.prefab == null) continue;
                    var position = start + direction * (distance + item.size / 2);
                    var itemAngle = angle;
                    itemAngle = GetAngleToShapeCenter(settings, position, shapeData.center, itemDir, lookAt, itemAngle);
                    AddBrushstrokeItem(item.idx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                        position, itemAngle, item.scaleMult, settings);
                    distance += item.size + spacing;
                }
            }
            int nexItemItemIdx = -1;

            if (ShapeManager.settings.shapeType == ShapeSettings.ShapeType.CIRCLE)
            {
                const float TAU = 2 * Mathf.PI;
                var perimeter = TAU * shapeData.radius;
                var items = new System.Collections.Generic.List<(int idx, float size, Vector3 scaleMult)>();
                var minspacing = perimeter / 1024f;
                float itemsSize = 0f;

                var firstLocalArcIntersection = Quaternion.Inverse(shapeData.planeRotation)
                    * (shapeData.GetArcIntersection(0) - shapeData.center);
                var firstLocalAngle = Mathf.Atan2(firstLocalArcIntersection.z, firstLocalArcIntersection.x);
                if (firstLocalAngle < 0) firstLocalAngle += TAU;
                var secondLocalArcIntersection = Quaternion.Inverse(shapeData.planeRotation)
                   * (shapeData.GetArcIntersection(1) - shapeData.center);
                var secondLocalAngle = Mathf.Atan2(secondLocalArcIntersection.z, secondLocalArcIntersection.x);
                if (secondLocalAngle < 0) secondLocalAngle += TAU;
                if (secondLocalAngle <= firstLocalAngle) secondLocalAngle += TAU;
                var arcDelta = secondLocalAngle - firstLocalAngle;
                var arcPerimeter = arcDelta / TAU * perimeter;
                if (PaletteManager.selectedBrush.patternMachine != null &&
                    PaletteManager.selectedBrush.restartPatternForEachStroke)
                    PaletteManager.selectedBrush.patternMachine.Reset();
                do
                {
                    float itemSize = 0;
                    var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                    var nextItem = PaletteManager.selectedBrush.items[nextIdx];
                    var scaleMult = ScaleMultiplier(nextIdx, ShapeManager.settings);
                    var key = (nextIdx, scaleMult);
                    if (nextItem.randomScaleMultiplier) itemSize = GetLineSpacing(nextIdx, settings, scaleMult);
                    else if (prefabSpacingDictionary.ContainsKey(key)) itemSize = prefabSpacingDictionary[key];
                    else
                    {
                        itemSize = GetLineSpacing(nextIdx, settings, scaleMult);
                        prefabSpacingDictionary.Add(key, itemSize);
                    }
                    itemSize = Mathf.Max(itemSize, minspacing);
                    if (itemsSize + itemSize > arcPerimeter) break;
                    itemsSize += itemSize;
                    items.Add((nextIdx, itemSize, scaleMult));
                } while (itemsSize < arcPerimeter);

                var spacing = (arcPerimeter - itemsSize) / (items.Count);

                if (items.Count == 0) return;
                var distance = firstLocalAngle / TAU * perimeter + items[0].size / 2;

                for (int i = 0; i < items.Count; ++i)
                {
                    var item = items[i];
                    var arcAngle = distance / perimeter * TAU;
                    var LocalRadiusVector = new Vector3(Mathf.Cos(arcAngle), 0f, Mathf.Sin(arcAngle))
                        * ShapeData.instance.radius;
                    var radiusVector = ShapeData.instance.planeRotation * LocalRadiusVector;
                    var position = radiusVector + ShapeData.instance.center;
                    var itemDir = settings.objectsOrientedAlongTheLine
                        ? Vector3.Cross(ShapeData.instance.planeRotation * Vector3.up, radiusVector) : Vector3.forward;
                    var lookAt = Quaternion.LookRotation((AxesUtils.SignedAxis)(settings.axisOrientedAlongTheLine),
                        Vector3.up);
                    var segmentRotation = Quaternion.LookRotation(itemDir, -settings.projectionDirection) * lookAt;
                    var angle = segmentRotation.eulerAngles;
                    angle = GetAngleToShapeCenter(settings, position, shapeData.center, itemDir, lookAt, angle);
                    AddBrushstrokeItem(item.idx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                        position, angle, item.scaleMult, settings);
                    var next_Item = items[(i + 1) % items.Count];
                    distance += item.size / 2 + next_Item.size / 2 + spacing;
                }
            }
            else
            {
                if (PaletteManager.selectedBrush.patternMachine != null &&
                    PaletteManager.selectedBrush.restartPatternForEachStroke)
                    PaletteManager.selectedBrush.patternMachine.Reset();
                for (int i = 0; i < points.Count - 1; ++i)
                {
                    var start = points[i];
                    var end = points[i + 1];
                    AddItemsToLine(start, end, ref nexItemItemIdx);
                }
            }
        }

        public static void UpdatePersistentShapeBrushstroke(ShapeData data,
            System.Collections.Generic.List<GameObject> shapeObjects,
            out BrushstrokeObject[] objPoses)
        {
            _brushstroke.Clear();
            var objPosesList = new System.Collections.Generic.List<BrushstrokeObject>();
            var settings = data.settings;
            var prefabSpacingDictionary = new System.Collections.Generic.Dictionary<(int, Vector3), float>();
            int nextItemIdx = -1;
            objPoses = objPosesList.ToArray();

            var toolSettings = ShapeManager.settings;

            var brushSettings = toolSettings.overwriteBrushProperties ? toolSettings.brushSettings
               : PaletteManager.selectedBrush;
            Vector3 BrushScaleMultiplier() => (ShapeManager.instance.applyBrushToExisting
                  && PaletteManager.selectedBrush != null) ? brushSettings.GetScaleMultiplier() : Vector3.one;

            if (settings.shapeType == ShapeSettings.ShapeType.CIRCLE)
            {
                const float TAU = 2 * Mathf.PI;
                var perimeter = TAU * data.radius;
                var items = new System.Collections.Generic.List<(int idx, float size, bool objExist, Vector3 objScale)>();
                var minspacing = perimeter / 1024f;
                float itemsSize = 0f;

                var firstLocalArcIntersection = Quaternion.Inverse(data.planeRotation)
                    * (data.GetArcIntersection(0) - data.center);
                var firstLocalAngle = Mathf.Atan2(firstLocalArcIntersection.z, firstLocalArcIntersection.x);
                if (firstLocalAngle < 0) firstLocalAngle += TAU;
                var secondLocalArcIntersection = Quaternion.Inverse(data.planeRotation)
                   * (data.GetArcIntersection(1) - data.center);
                var secondLocalAngle = Mathf.Atan2(secondLocalArcIntersection.z, secondLocalArcIntersection.x);
                if (secondLocalAngle < 0) secondLocalAngle += TAU;
                if (secondLocalAngle <= firstLocalAngle) secondLocalAngle += TAU;
                var arcDelta = secondLocalAngle - firstLocalAngle;
                var arcPerimeter = arcDelta / TAU * perimeter;

                var objIdx = 0;
                int GetNextIdx() => PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.nextItemIndex : -1;
                if (nextItemIdx < 0) nextItemIdx = GetNextIdx();

                do
                {
                    float itemSize = 0;
                    var objectExist = objIdx < shapeObjects.Count && shapeObjects[objIdx] != null;
                    var objScale = Vector3.one;
                    var brushScaleMultiplier = BrushScaleMultiplier();
                    if (objectExist)
                    {
                        var obj = shapeObjects[objIdx];
                        if (ShapeManager.instance.applyBrushToExisting)
                        {
                            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                            itemSize = GetLineSpacing(prefab.transform, toolSettings,
                                brushScaleMultiplier, useDictionary: true);
                            objScale = Vector3.Scale(prefab.transform.localScale, brushScaleMultiplier);
                        }
                        else
                        {
                            itemSize = GetLineSpacing(shapeObjects[objIdx].transform, settings,
                            Vector3.one, useDictionary: false);
                            objScale = obj.transform.localScale;
                        }
                    }
                    else if (PaletteManager.selectedBrush != null)
                    {
                        var nextItem = PaletteManager.selectedBrush.items[nextItemIdx];
                        var prefab = nextItem.prefab;
                        itemSize = GetLineSpacing(prefab.transform, toolSettings,
                                brushScaleMultiplier, useDictionary: true);
                        objScale = Vector3.Scale(prefab.transform.localScale, brushScaleMultiplier);
                    }
                    else break;
                    itemSize = Mathf.Max(itemSize, minspacing);
                    if (itemsSize + itemSize > arcPerimeter) break;
                    itemsSize += itemSize;
                    items.Add((objectExist ? objIdx : nextItemIdx, itemSize, objectExist, objScale));
                    nextItemIdx = GetNextIdx();
                    if (objectExist) ++objIdx;
                } while (itemsSize < arcPerimeter);
                var spacing = (arcPerimeter - itemsSize) / (items.Count);

                if (items.Count == 0)
                {
                    return;
                }
                var distance = firstLocalAngle / TAU * perimeter + items[0].size / 2;
                int itemCount = 0;
                for (int i = 0; i < items.Count; ++i)
                {
                    var item = items[i];
                    GameObject obj = null;
                    if (item.objExist) obj = shapeObjects[item.idx];
                    else if (PaletteManager.selectedBrush != null) obj = PaletteManager.selectedBrush.items[item.idx].prefab;
                    if (obj == null) continue;

                    var arcAngle = distance / perimeter * TAU;
                    var LocalRadiusVector = new Vector3(Mathf.Cos(arcAngle), 0f, Mathf.Sin(arcAngle))
                        * data.radius;
                    var radiusVector = data.planeRotation * LocalRadiusVector;
                    var position = radiusVector + data.center;
                    var itemDir = settings.objectsOrientedAlongTheLine
                        ? Vector3.Cross(data.planeRotation * Vector3.up, radiusVector) : Vector3.forward;
                    var lookAt = Quaternion.LookRotation((AxesUtils.SignedAxis)(settings.axisOrientedAlongTheLine),
                        Vector3.up);
                    var segmentRotation = Quaternion.LookRotation(itemDir, -settings.projectionDirection) * lookAt;
                    var angle = segmentRotation.eulerAngles;
                    angle = GetAngleToShapeCenter(settings, position, data.center, itemDir, lookAt, angle);
                    if (item.objExist)
                    {
                        var brushAdditionalAngle = Vector3.zero;
                        bool brushFlipX = false;
                        bool brushFlipY = false;
                        var brushSurfaceDistance = 0f;
                        if (ShapeManager.instance.applyBrushToExisting)
                        {
                            if (PaletteManager.selectedBrush != null)
                            {
                                brushAdditionalAngle = brushSettings.GetAdditionalAngle();
                                brushFlipX = brushSettings.GetFlipX();
                                brushFlipY = brushSettings.GetFlipY();
                                brushSurfaceDistance = brushSettings.GetSurfaceDistance();
                            }
                        }
                        objPosesList.Add(new BrushstrokeObject(item.idx, position, Quaternion.Euler(angle),
                            brushAdditionalAngle, item.objScale, brushFlipX, brushFlipY, brushSurfaceDistance,
                            brushstrokeDirection: Vector3.zero));
                    }
                    else AddBrushstrokeItem(item.idx, PaletteManager.selectedBrush.GetPatternTokenIndex(), position, angle,
                        item.objScale, ShapeManager.settings);
                    var next_Item = items[(i + 1) % items.Count];
                    distance += item.size / 2 + next_Item.size / 2 + spacing;
                    ++itemCount;
                }
            }
            else
            {
                var points = new System.Collections.Generic.List<Vector3>();
                var firstVertexIdx = data.firstVertexIdxAfterIntersection;
                var lastVertexIdx = data.lastVertexIdxBeforeIntersection;
                int sidesCount = settings.shapeType == ShapeSettings.ShapeType.POLYGON ? settings.sidesCount
                    : data.circleSideCount;
                int GetNextVertexIdx(int currentIdx) => currentIdx == sidesCount ? 1 : currentIdx + 1;
                int GetPrevVertexIdx(int currentIdx) => currentIdx == 1 ? sidesCount : currentIdx - 1;
                var firstPrev = GetPrevVertexIdx(firstVertexIdx);
                points.Add(data.GetArcIntersection(0));
                if (lastVertexIdx != firstPrev || (lastVertexIdx == firstPrev && data.arcAngle > 120))
                {

                    var vertexIdx = firstVertexIdx;
                    var nextVertexIdx = firstVertexIdx;

                    do
                    {
                        vertexIdx = nextVertexIdx;
                        if (vertexIdx >= data.pointsCount || points.Count >= data.pointsCount)
                        {
                            ShapeData.instance.Update(true);
                            return;
                        }
                        points.Add(data.GetPoint(vertexIdx));
                        nextVertexIdx = GetNextVertexIdx(nextVertexIdx);
                    } while (vertexIdx != lastVertexIdx);
                }
                var lastPoint = data.GetArcIntersection(1);
                if (points.Last() != lastPoint) points.Add(lastPoint);
                int firstObjInSegmentIdx = 0;

                void AddItemsToLine(Vector3 start, Vector3 end)
                {
                    int GetNextIdx() => PaletteManager.selectedBrush != null
                        ? PaletteManager.selectedBrush.nextItemIndex : -1;
                    if (nextItemIdx < 0) nextItemIdx = GetNextIdx();
                    var startToEnd = end - start;
                    var lineLength = startToEnd.magnitude;

                    float itemsSize = 0f;
                    var items = new System.Collections.Generic.List<(int idx, float size, bool objExist, Vector3 objScale)>();
                    var minspacing = (lineLength * points.Count) / 1024f;
                    int objSegmentIdx = 0;
                    var objIdx = firstObjInSegmentIdx + objSegmentIdx;
                    do
                    {
                        float itemSize = 0;
                        var objectExist = objIdx < shapeObjects.Count;
                        var objScale = Vector3.one;
                        var brushScaleMultiplier = BrushScaleMultiplier();
                        if (objectExist)
                        {
                            var obj = shapeObjects[objIdx];
                            if(obj == null)
                            {
                                ++objIdx;
                                continue;
                            }
                            if (ShapeManager.instance.applyBrushToExisting)
                            {
                                var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                                itemSize = GetLineSpacing(prefab.transform, toolSettings,
                                    brushScaleMultiplier, useDictionary: true);
                                objScale = Vector3.Scale(prefab.transform.localScale, brushScaleMultiplier);
                            }
                            else
                            {
                                itemSize = GetLineSpacing(obj.transform, settings,
                                Vector3.one, useDictionary: false);
                                objScale = obj.transform.localScale;
                            }
                        }
                        else if (PaletteManager.selectedBrush != null)
                        {
                            var nextItem = PaletteManager.selectedBrush.items[nextItemIdx];
                            var prefab = nextItem.prefab;
                            itemSize = GetLineSpacing(prefab.transform, toolSettings,
                                    brushScaleMultiplier, useDictionary: true);
                            objScale = Vector3.Scale(prefab.transform.localScale, brushScaleMultiplier);
                        }
                        else break;
                        itemSize = Mathf.Max(itemSize, minspacing);
                        if (itemsSize + itemSize > lineLength) break;
                        itemsSize += itemSize;
                        items.Add((objectExist ? objIdx : nextItemIdx, itemSize, objectExist, objScale));
                        nextItemIdx = GetNextIdx();
                        if (objectExist) ++objIdx;
                    } while (itemsSize < lineLength);


                    var spacing = (lineLength - itemsSize) / (items.Count + 1);
                    var distance = spacing;
                    var direction = startToEnd.normalized;
                    Vector3 itemDir = (settings.objectsOrientedAlongTheLine && direction != Vector3.zero)
                        ? direction : Vector3.forward;
                    var lookAt = Quaternion.LookRotation(
                        (AxesUtils.SignedAxis)(settings.axisOrientedAlongTheLine), Vector3.up);
                    var segmentRotation = Quaternion.LookRotation(itemDir, -settings.projectionDirection) * lookAt;
                    var angle = segmentRotation.eulerAngles;
                    
                    foreach (var item in items)
                    {
                        GameObject obj = null;
                        if (item.objExist) obj = shapeObjects[item.idx];
                        else if (PaletteManager.selectedBrush != null)
                            obj = PaletteManager.selectedBrush.items[item.idx].prefab;
                        if (obj == null) continue;
                        var position = start + direction * (distance + item.size / 2);
                        var itemAngle = angle;
                        itemAngle = GetAngleToShapeCenter(settings, position, data.center, itemDir, lookAt, itemAngle);
                        if (item.objExist)
                        {
                            var brushAdditionalAngle = Vector3.zero;
                            bool brushFlipX = false;
                            bool brushFlipY = false;
                            var brushSurfaceDistance = 0f;
                            if (ShapeManager.instance.applyBrushToExisting)
                            {
                                if (PaletteManager.selectedBrush != null)
                                {
                                    brushAdditionalAngle = brushSettings.GetAdditionalAngle();
                                    brushFlipX = brushSettings.GetFlipX();
                                    brushFlipY = brushSettings.GetFlipY();
                                    brushSurfaceDistance = brushSettings.GetSurfaceDistance();
                                }
                            }
                            objPosesList.Add(new BrushstrokeObject(item.idx, position, Quaternion.Euler(itemAngle),
                                brushAdditionalAngle, item.objScale, brushFlipX, brushFlipY, brushSurfaceDistance,
                            brushstrokeDirection: Vector3.zero));
                        }
                        else AddBrushstrokeItem(item.idx,
                            PaletteManager.selectedBrush == null ? 0 : PaletteManager.selectedBrush.GetPatternTokenIndex(),
                            position, itemAngle, item.objScale, settings);
                        distance += item.size + spacing;
                        ++firstObjInSegmentIdx;
                    }
                }

                for (int i = 0; i < points.Count - 1; ++i)
                {
                    var start = points[i];
                    var end = points[i + 1];
                    AddItemsToLine(start, end);
                }
            }
            objPoses = objPosesList.ToArray();
        }
        #endregion

        #region TILING
        public static void UpdateTilingBrushstroke(Vector3[] cellCenters)
        {
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush == null) return;
            for (int i = 0; i < cellCenters.Length; ++i)
            {
                var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                AddBrushstrokeItem(nextIdx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                    cellCenters[i], angle: Vector3.zero, scale: Vector3.one,
                    TilingManager.settings);
            }
            ToolProperties.RepainWindow();
        }

        public static void UpdatePersistentTilingBrushstroke(Vector3[] cellCenters, TilingSettings settings,
            System.Collections.Generic.List<GameObject> tilingObjects,
            out Vector3[] objPositions, out Vector3[] strokePositions)
        {
            _brushstroke.Clear();
            var objPositionsList = new System.Collections.Generic.List<Vector3>();
            var strokePositionsList = new System.Collections.Generic.List<Vector3>();

            for (int i = 0; i < cellCenters.Length; ++i)
            {
                var objectExist = i < tilingObjects.Count;
                var position = cellCenters[i];
                if (objectExist) objPositionsList.Add(position);
                else
                {
                    if (PaletteManager.selectedBrush == null) break;
                    var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                    AddBrushstrokeItem(nextIdx, PaletteManager.selectedBrush == null ? 0
                        : PaletteManager.selectedBrush.GetPatternTokenIndex(), position,
                        angle: Vector3.zero, scale: Vector3.one,
                        settings);
                    strokePositionsList.Add(position);
                }
            }
            objPositions = objPositionsList.ToArray();
            strokePositions = strokePositionsList.ToArray();
        }
        #endregion

        #region PIN
        private static int _currentPinIdx = 0;
        public static void SetNextPinBrushstroke(int delta)
        {
            _currentPinIdx = _currentPinIdx + delta;
            var mod = _currentPinIdx % PaletteManager.selectedBrush.itemCount;
            _currentPinIdx = mod < 0 ? PaletteManager.selectedBrush.itemCount + mod : mod;
            _brushstroke.Clear();
            AddBrushstrokeItem(_currentPinIdx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                tangentPosition: Vector3.zero, angle: Vector3.zero,
                ScaleMultiplier(_currentPinIdx, PinManager.settings), PinManager.settings);
        }
        #endregion

        #region BRUSH
        private static bool TangentPositionIsOverlapedByBrushstrokeItems(float x, float y, float minSpacing)
        {
            var minSpacingSqr = minSpacing * minSpacing;
            for (int i = 0; i < _brushstroke.Count; ++i )
            {
                var item = _brushstroke[i];
                if (item.tangentPosition == Vector3.zero) continue;
                var dx = item.tangentPosition.x - x;
                var dy = item.tangentPosition.y - y;
                var distanceSqr = dx * dx + dy * dy;
                if (distanceSqr < minSpacingSqr) return true;
            }
            return false;
        }
        private static void UpdateBrushBaseStroke(BrushToolBase ToolSettings)
        {
            if (ToolSettings.spacingType == BrushToolBase.SpacingType.AUTO)
            {
                var maxSize = 0.1f;
                foreach (var item in PaletteManager.selectedBrush.items)
                {
                    if (item.prefab == null) continue;
                    var itemSize = BoundsUtils.GetBoundsRecursive(item.prefab.transform).size;
                    itemSize = Vector3.Scale(itemSize,
                        item.randomScaleMultiplier ? item.maxScaleMultiplier : item.scaleMultiplier);
                    maxSize = Mathf.Max(itemSize.x, itemSize.z, maxSize);
                }
                ToolSettings.minSpacing = maxSize;
                ToolProperties.RepainWindow();
            }

            if (ToolSettings.brushShape == BrushToolSettings.BrushShape.POINT)
            {
                var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                if (nextIdx == -1) return;
                if (PaletteManager.selectedBrush.frequencyMode == PluginMaster.MultibrushSettings.FrequencyMode.PATTERN
                    && nextIdx == -2) return;
                _brushstroke.Clear();

                AddBrushstrokeItem(nextIdx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                    tangentPosition: Vector3.zero, angle: Vector3.zero,
                    scale: ScaleMultiplier(nextIdx, ToolSettings), ToolSettings);
                _currentPinIdx = Mathf.Clamp(nextIdx, 0, PaletteManager.selectedBrush.itemCount - 1);
            }
            else
            {
                var radius = ToolSettings.radius;
                var radiusSqr = radius * radius;

                var minSpacing = ToolSettings.minSpacing * 100f / ToolSettings.density;

                var delta = minSpacing;
                var maxRandomOffset = delta * ToolSettings.randomness;

                int halfSize = (int)Mathf.Ceil(radius / delta) + 1;
                const int MAX_SIZE = 32;
                if (halfSize > MAX_SIZE)
                {
                    halfSize = MAX_SIZE;
                    delta = radius / MAX_SIZE;
                    minSpacing = delta;
                    maxRandomOffset = delta * ToolSettings.randomness;
                }
                int size = halfSize * 2;
                float col0x = -delta * halfSize;
                float row0y = -delta * halfSize;


                var availableCells = new System.Collections.Generic.HashSet<(int row, int col)>();
                for (int row = 0; row < size; ++row)
                {
                    for (int col = 0; col < size; ++col)
                    {
                        var x = col0x + col * delta;
                        var y = row0y + row * delta;
                        if (ToolSettings.brushShape == BrushToolBase.BrushShape.CIRCLE)
                        {
                            var distanceSqr = x * x + y * y;
                            if (distanceSqr >= radiusSqr) continue;
                        }
                        availableCells.Add((row, col));
                    }
                }

                while (availableCells.Count > 0)
                {
                    var randomIdx = Random.Range(0, availableCells.Count);
                    var cell = availableCells.ElementAt(randomIdx);
                    var col = cell.col;
                    var row = cell.row;
                    var x = col0x + col * delta;
                    var y = row0y + row * delta;

                    if (ToolSettings.randomizePositions)
                    {
                        x += Random.Range(-maxRandomOffset, maxRandomOffset);
                        y += Random.Range(-maxRandomOffset, maxRandomOffset);
                        col = Mathf.Clamp(Mathf.RoundToInt((x - col0x) / delta), 0, size - 1);
                        row = Mathf.Clamp(Mathf.RoundToInt((y - row0y) / delta), 0, size - 1);
                        cell = (row, col);
                        if (availableCells.Contains(cell)) availableCells.Remove(cell);
                        else continue;
                    }
                    else availableCells.Remove(cell);
                    if (ToolSettings is BrushToolSettings)
                    {
                        var btoolSettings = ToolSettings as BrushToolSettings;
                        if (btoolSettings.avoidOverlapping != BrushToolSettings.AvoidOverlappingType.DISABLED
                            && TangentPositionIsOverlapedByBrushstrokeItems(x, y, minSpacing))
                            continue;
                    }
                    if (ToolSettings.brushShape == BrushToolBase.BrushShape.CIRCLE)
                    {
                        var distanceSqr = x * x + y * y;
                        if (distanceSqr >= radiusSqr) continue;
                    }
                    else if (ToolSettings.brushShape == BrushToolBase.BrushShape.SQUARE)
                    {
                        if (Mathf.Abs(x) > radius || Mathf.Abs(y) > radius) continue;
                    }
                    var nextItemIdx = PaletteManager.selectedBrush.nextItemIndex;
                    var position = new Vector3(x, y, 0f);
                    if ((PaletteManager.selectedBrush.frequencyMode
                        == MultibrushSettings.FrequencyMode.RANDOM && nextItemIdx == -1)
                        || (PaletteManager.selectedBrush.frequencyMode
                        == MultibrushSettings.FrequencyMode.PATTERN && nextItemIdx == -2)) continue;
                    var item = PaletteManager.selectedBrush.items[nextItemIdx];

                    AddBrushstrokeItem(nextItemIdx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                        tangentPosition: position, angle: Vector3.zero,
                        ScaleMultiplier(nextItemIdx, ToolSettings), ToolSettings);
                }
            }
        }

        public static void UpdateSingleBrushstroke(IPaintToolSettings settings)
        {
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush == null) return;
            var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
            if (nextIdx == -1) return;
            if (PaletteManager.selectedBrush.frequencyMode == PluginMaster.MultibrushSettings.FrequencyMode.PATTERN
                && nextIdx == -2)
            {
                if (PaletteManager.selectedBrush.patternMachine != null) PaletteManager.selectedBrush.patternMachine.Reset();
                else return;
            }


            AddBrushstrokeItem(nextIdx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                tangentPosition: Vector3.zero, angle: Vector3.zero,
                scale: ScaleMultiplier(nextIdx, settings), settings);

            const int maxTries = 10;
            int tryCount = 0;
            while (_brushstroke.Count == 0 && ++tryCount < maxTries)
            {
                nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                if (nextIdx >= 0)
                {
                    AddBrushstrokeItem(nextIdx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                        tangentPosition: Vector3.zero, angle: Vector3.zero,
                        scale: ScaleMultiplier(nextIdx, settings), settings);
                    break;
                }
            }
            _currentPinIdx = Mathf.Clamp(nextIdx, 0, PaletteManager.selectedBrush.itemCount - 1);
        }
        #endregion

        #region REPLACER
        private static System.Collections.Generic.Dictionary<Transform, BrushstrokeItem> _replacerDictionary
            = new System.Collections.Generic.Dictionary<Transform, BrushstrokeItem>();
        private static System.Collections.Generic.Dictionary<BrushstrokeItem, Transform> _replacerDictionary2
           = new System.Collections.Generic.Dictionary<BrushstrokeItem, Transform>();
        public static void UpdateReplacerBrushstroke(bool clearDictionary,
            System.Collections.Generic.IEnumerable<GameObject> targets)
        {
            _brushstroke.Clear();
            if (clearDictionary) ClearReplacerDictionary();
            var toolSettings = ReplacerManager.settings;
            bool GetStrokeItem(Transform target, int itemIdx, out BrushstrokeItem item)
            {
                item = new BrushstrokeItem();
                if (itemIdx == -1) return false;
                if (PaletteManager.selectedBrush.frequencyMode == PluginMaster.MultibrushSettings.FrequencyMode.PATTERN
                    && itemIdx == -2)
                {
                    if (PaletteManager.selectedBrush.patternMachine != null)
                        PaletteManager.selectedBrush.patternMachine.Reset();
                    else return false;
                }
                var multiBrushSettings = PaletteManager.selectedBrush;
                BrushSettings brushSettings = multiBrushSettings.items[itemIdx];
                if (toolSettings.overwriteBrushProperties) brushSettings = toolSettings.brushSettings;
                var flipX = brushSettings.GetFlipX();
                var flipY = brushSettings.GetFlipY();

                var multiBrushItemSettings = PaletteManager.selectedBrush.items[itemIdx];
                var prefab = multiBrushItemSettings.prefab;
                if (prefab == null) return false;
                var itemRotation = target.rotation;
                var targetBounds = BoundsUtils.GetBoundsRecursive(target, target.rotation);
                var strokeRotation = Quaternion.identity;
                var scaleMult = Vector3.one;

                if (toolSettings.overwriteBrushProperties)
                {
                    var toolBrushSettings = toolSettings.brushSettings;
                    var additonalAngle = toolBrushSettings.addRandomRotation
                        ? toolBrushSettings.randomEulerOffset.randomVector : toolBrushSettings.eulerOffset;
                    strokeRotation *= Quaternion.Euler(additonalAngle);
                    scaleMult = toolBrushSettings.randomScaleMultiplier
                        ? toolBrushSettings.randomScaleMultiplierRange.randomVector : toolBrushSettings.scaleMultiplier;
                }
                var inverseStrokeRotation = Quaternion.Inverse(strokeRotation);
                itemRotation *= strokeRotation;
                var itemBounds = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation * strokeRotation);

                if (toolSettings.keepTargetSize)
                {
                    var targetSize = targetBounds.size;
                    var itemSize = itemBounds.size;

                    if (toolSettings.maintainProportions)
                    {
                        var targetMagnitude = Mathf.Max(targetSize.x, targetSize.y, targetSize.z);
                        var itemMagnitude = Mathf.Max(itemSize.x, itemSize.y, itemSize.z);
                        scaleMult = inverseStrokeRotation * (Vector3.one * (targetMagnitude / itemMagnitude));
                    }
                    else scaleMult = inverseStrokeRotation
                            * new Vector3(targetSize.x / itemSize.x, targetSize.y / itemSize.y, targetSize.z / itemSize.z);
                    scaleMult = new Vector3(Mathf.Abs(scaleMult.x), Mathf.Abs(scaleMult.y), Mathf.Abs(scaleMult.z));
                }
                var itemScale = Vector3.Scale(prefab.transform.localScale, scaleMult);
                var itemPosition = targetBounds.center;

                Transform replaceSurface = null;
                if (toolSettings.positionMode == ReplacerSettings.PositionMode.ON_SURFACE)
                {
                    var TRS = Matrix4x4.TRS(itemPosition, itemRotation, itemScale);
                    var bottomDistanceToSurfce = PWBIO.GetBottomDistanceToSurface(multiBrushItemSettings.bottomVertices,
                        TRS, Mathf.Abs(multiBrushItemSettings.bottomMagnitude), paintOnPalettePrefabs: true,
                        castOnMeshesWithoutCollider: true, ignoreSceneColliders: true, out replaceSurface,
                        new System.Collections.Generic.HashSet<GameObject> { target.gameObject });
                    itemPosition += itemRotation * new Vector3(0f, -bottomDistanceToSurfce, 0f);
                }
                else
                {
                    if (toolSettings.positionMode == ReplacerSettings.PositionMode.PIVOT)
                        itemPosition = target.position;
                    itemPosition -= itemRotation * Vector3.Scale(itemBounds.center - prefab.transform.position, scaleMult);
                }

                item = new BrushstrokeItem(itemIdx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                    multiBrushItemSettings, itemPosition, itemRotation.eulerAngles,
                    scaleMult, flipX, flipY, surfaceDistance: 0);
                return true;
            }
            void AddItem(Transform target)
            {
                var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                if (GetStrokeItem(target, nextIdx, out BrushstrokeItem strokeItem))
                {
                    _brushstroke.Add(strokeItem);
                    _replacerDictionary.Add(target, strokeItem);
                    _replacerDictionary2.Add(strokeItem, target);
                }
            }
            foreach (var sceneObj in targets)
            {
                var target = sceneObj.transform;
                BrushstrokeItem item;
                if (_replacerDictionary.ContainsKey(target))
                {
                    item = _replacerDictionary[target];
                    if (GetStrokeItem(target, item.index, out BrushstrokeItem strokeItem))
                    {
                        if (item == strokeItem) _brushstroke.Add(item);
                        else
                        {
                            _replacerDictionary.Remove(target);
                            _replacerDictionary2.Remove(item);
                            AddItem(target);
                        }
                    }
                }
                else AddItem(target);
            }

        }
        public static void ClearReplacerDictionary()
        {
            _replacerDictionary.Clear();
            _replacerDictionary2.Clear();
        }
        public static Transform GetReplacerTargetFromStrokeItem(BrushstrokeItem item) => _replacerDictionary2[item];
        #endregion

        #region MODULAR TOOLS
        private static void UpdateFirstModularBrushstroke(ModularToolBase settings, bool setNextIdx)
        {
            if (PaletteManager.selectedBrush == null) return;
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush.restartPatternForEachStroke)
                PaletteManager.selectedBrush.ResetCurrentItemIndex();
            else if (setNextIdx) PaletteManager.selectedBrush.SetNextItemIndex();
            var nextIdx = PaletteManager.selectedBrush.currentItemIndex;
            if (nextIdx == -1) return;
            if (PaletteManager.selectedBrush.frequencyMode == MultibrushSettings.FrequencyMode.PATTERN
                && nextIdx == -2)
            {
                if (PaletteManager.selectedBrush.patternMachine != null) PaletteManager.selectedBrush.patternMachine.Reset();
                else return;
            }

            var forwardAxis = settings.forwardAxis;
            if (settings is FloorSettings)
            {
                var floorSettings = (FloorSettings)settings;
                var quarterTurns = FloorManager.quarterTurns;
                if (floorSettings.swapXZ) ++quarterTurns;
                forwardAxis = Quaternion.AngleAxis(-90 * quarterTurns, settings.upwardAxis) * forwardAxis;
            }
            else if (settings is WallSettings && WallManager.halfTurn)
                forwardAxis = Quaternion.AngleAxis(180, settings.upwardAxis) * forwardAxis;
            var angle = AxesUtils.SignedAxis.GetEulerAnglesFromAxes(forwardAxis, settings.upwardAxis);
            angle = (Quaternion.Euler(angle) * SnapManager.settings.rotation).eulerAngles;
            var scale = ScaleMultiplier(nextIdx, settings);
            scale.x = Mathf.Abs(scale.x);
            scale.y = Mathf.Abs(scale.y);
            scale.z = Mathf.Abs(scale.z);
            AddBrushstrokeItem(nextIdx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                tangentPosition: Vector3.zero, angle, scale, settings);

            const int maxTries = 10;
            int tryCount = 0;
            while (_brushstroke.Count == 0 && ++tryCount < maxTries)
            {
                nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                if (nextIdx >= 0)
                {
                    AddBrushstrokeItem(nextIdx, PaletteManager.selectedBrush.GetPatternTokenIndex(),
                        tangentPosition: Vector3.zero, angle, scale, settings);
                    break;
                }
            }
        }

        #region FLOOR
        private static int _cellsCountX = 0;
        private static int _cellsCountZ = 0;
        public static int cellsCountX => _cellsCountX;
        public static int cellsCountZ => _cellsCountZ;
        public static void ResetCellCount()
        {
            _cellsCountX = 1;
            _cellsCountZ = 1;
        }
        public static void UpdateFloorBrushstroke(bool setNextIdx, bool deleteBox = false)
        {
            ResetCellCount();
            if (FloorManager.state == FloorManager.ToolState.FIRST_CORNER)
            {
                UpdateFirstModularBrushstroke(FloorManager.settings, setNextIdx);
                return;
            }
            var toolSettings = FloorManager.settings;
            var diagonal = FloorManager.secondCorner - FloorManager.firstCorner;
            var localDiagonal = Quaternion.Inverse(SnapManager.settings.rotation) * diagonal;

            _cellsCountX = Mathf.RoundToInt(Mathf.Abs(localDiagonal.x / toolSettings.moduleSize.x)) + 1;
            var signX = localDiagonal.x >= 0 ? 1 : -1;
            var dirX = SnapManager.settings.rotation * (Vector3.right * signX);
            var deltaX = dirX * toolSettings.moduleSize.x;

            _cellsCountZ = Mathf.RoundToInt(Mathf.Abs(localDiagonal.z / toolSettings.moduleSize.z)) + 1;
            var signZ = localDiagonal.z >= 0 ? 1 : -1;
            var dirZ = SnapManager.settings.rotation * (Vector3.forward * signZ);
            var deltaZ = dirZ * toolSettings.moduleSize.z;

            var localRotation = Quaternion.FromToRotation(Vector3.up, toolSettings.upwardAxis);
            var rotation = SnapManager.settings.rotation * localRotation;
            var angle = rotation.eulerAngles;

            var prevBrushstroke = _brushstroke.ToArray();
            _brushstroke.Clear();

            if (PaletteManager.selectedBrush.restartPatternForEachStroke)
                PaletteManager.selectedBrush.ResetCurrentItemIndex();
            var floorItemsCount = 0;
            for (int xIdx = 0; xIdx < _cellsCountX; ++xIdx)
            {
                var tangent = deltaX * xIdx;
                for (int zIdx = 0; zIdx < _cellsCountZ; ++zIdx)
                {
                    var bitangent = deltaZ * zIdx;
                    var cellCenter = FloorManager.firstCorner + tangent + bitangent;
                    var idx = PaletteManager.selectedBrush.currentItemIndex;
                    BrushSettings brush = PaletteManager.selectedBrush.GetItemAt(idx);
                    if (toolSettings.overwriteBrushProperties) brush = toolSettings.brushSettings;
                    if (toolSettings.subtractBrushOffset)
                    {
                        var r = SnapManager.settings.rotation;
                        if (FloorManager.quarterTurns > 0)
                            r *= Quaternion.AngleAxis(FloorManager.quarterTurns * 90, toolSettings.upwardAxis);
                        cellCenter += r * brush.localPositionOffset;
                    }
                    else cellCenter += rotation * brush.localPositionOffset;

                    if (deleteBox)
                    {
                        var additionalAngle = (Quaternion.Euler(angle)
                            * Quaternion.Euler(PaletteManager.selectedBrush.eulerOffset)).eulerAngles;
                        var strokeItem = new BrushstrokeItem(index: 0, tokenIndex: 0, brush as MultibrushItemSettings,
                            cellCenter, additionalAngle, scaleMultiplier: FloorManager.settings.moduleSize,
                            flipX: false, flipY: false, surfaceDistance: 0);
                        _brushstroke.Add(strokeItem);
                    }
                    else
                    {
                        var tokenIdx = PaletteManager.selectedBrush.GetPatternTokenIndex();
                        if (!PaletteManager.selectedBrush.restartPatternForEachStroke
                            && prevBrushstroke.Length > floorItemsCount)
                        {
                            idx = prevBrushstroke[floorItemsCount].index;
                            tokenIdx = prevBrushstroke[floorItemsCount].tokenIndex;
                            PaletteManager.selectedBrush.SetPatternTokenIndex(tokenIdx);
                        }
                        var scale = localRotation * ScaleMultiplier(idx, toolSettings);
                        scale.x = Mathf.Abs(scale.x);
                        scale.y = Mathf.Abs(scale.y);
                        scale.z = Mathf.Abs(scale.z);
                        AddBrushstrokeItem(idx, tokenIdx, cellCenter, angle, scale, toolSettings);
                        if (setNextIdx) PaletteManager.selectedBrush.SetNextItemIndex();
                    }
                    ++floorItemsCount;
                }
            }
        }
        #endregion
        #region WALL
        public static void UpdateWallBrushstroke(AxesUtils.Axis segmentAxis, int cellsCount,
            bool setNextIdx, bool deleteMode)
        {
            if (WallManager.state == WallManager.ToolState.FIRST_WALL_PREVIEW)
            {
                UpdateFirstModularBrushstroke(WallManager.settings, setNextIdx);
                return;
            }
            var toolSettings = WallManager.settings;
            var diagonal = WallManager.endPointSnapped - WallManager.startPointSnapped;
            var localDiagonal = Quaternion.Inverse(SnapManager.settings.rotation) * diagonal;
            Vector3 delta;
            if (segmentAxis == AxesUtils.Axis.X)
            {
                var sign = localDiagonal.x >= 0 ? 1 : -1;
                var dir = SnapManager.settings.rotation * (Vector3.right * sign);
                delta = dir * SnapManager.settings.step.x;
            }
            else
            {
                var sign = localDiagonal.z >= 0 ? 1 : -1;
                var dir = SnapManager.settings.rotation * (Vector3.forward * sign);
                delta = dir * SnapManager.settings.step.z;
            }

            var firstPoint = WallManager.endPointSnapped - (delta * (cellsCount - 1));
            var localRotation = Quaternion.Euler(AxesUtils.SignedAxis.GetEulerAnglesFromAxes(toolSettings.forwardAxis,
                toolSettings.upwardAxis));
            var rotation = SnapManager.settings.rotation * localRotation;
            var angle = rotation.eulerAngles;


            var prevBrushstroke = _brushstroke.ToArray();
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush.restartPatternForEachStroke)
                PaletteManager.selectedBrush.ResetCurrentItemIndex();
            var wallItemsCount = 0;
            void AddItem(Vector3 position, AxesUtils.Axis segmentAxis)
            {
                int nextIdx = 0;
                var brushItem = PaletteManager.selectedBrush.GetItemAt(PaletteManager.selectedBrush.currentItemIndex);
                if (!deleteMode)
                {
                    nextIdx = PaletteManager.selectedBrush.currentItemIndex;
                }
                if (deleteMode)
                {
                    var additionalAngle = (Quaternion.Euler(angle)
                            * Quaternion.Euler(PaletteManager.selectedBrush.eulerOffset)).eulerAngles;
                    var strokeItem = new BrushstrokeItem(index: 0, tokenIndex: 0, brushItem,
                        position, additionalAngle, scaleMultiplier: WallManager.settings.moduleSize,
                        flipX: false, flipY: false, surfaceDistance: 0);
                    _brushstroke.Add(strokeItem);
                }
                else
                {
                    var tokenIdx = PaletteManager.selectedBrush.GetPatternTokenIndex();
                    if (!PaletteManager.selectedBrush.restartPatternForEachStroke && prevBrushstroke.Length > wallItemsCount)
                    {
                        nextIdx = prevBrushstroke[wallItemsCount].index;
                        tokenIdx = prevBrushstroke[wallItemsCount].tokenIndex;
                        PaletteManager.selectedBrush.SetPatternTokenIndex(tokenIdx);
                    }
                    var scale = localRotation * ScaleMultiplier(nextIdx, toolSettings);
                    scale.x = Mathf.Abs(scale.x);
                    scale.y = Mathf.Abs(scale.y);
                    scale.z = Mathf.Abs(scale.z);
                    AddBrushstrokeItem(nextIdx, tokenIdx, position, angle, scale, toolSettings);
                    if (setNextIdx) PaletteManager.selectedBrush.SetNextItemIndex();
                }
                ++wallItemsCount;
            }
            for (int idx = 0; idx < cellsCount; ++idx)
            {
                var tangent = delta * idx;
                var position = firstPoint + tangent;
                AddItem(position, segmentAxis);
            }
        }
        #endregion
        #endregion
    }
}
