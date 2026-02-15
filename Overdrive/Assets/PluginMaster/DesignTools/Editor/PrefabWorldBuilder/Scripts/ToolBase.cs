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
    #region MANAGER
    public static class ToolProfile
    {
        public const string DEFAULT = "Default";
    }

    public interface IToolManager
    {
        string selectedProfileName { get; set; }
        string[] profileNames { get; }
        void SaveProfile();
        void SaveProfileAs(string name);
        void DeleteProfile();
        void Revert();
        void FactoryReset();
    }

    [System.Serializable]
    public class ToolManagerBase<TOOL_SETTINGS> : IToolManager, ISerializationCallbackReceiver
        where TOOL_SETTINGS : IToolSettings, new()
    {
        protected static ToolManagerBase<TOOL_SETTINGS> _instance = null;
        private static System.Collections.Generic.Dictionary<string, TOOL_SETTINGS> _staticProfiles
            = new System.Collections.Generic.Dictionary<string, TOOL_SETTINGS>
        { { ToolProfile.DEFAULT, new TOOL_SETTINGS() } };
        [SerializeField] private string[] _profileKeys = { ToolProfile.DEFAULT };
        [SerializeField] private TOOL_SETTINGS[] _profileValues = { new TOOL_SETTINGS() };
        private static string _staticSelectedProfileName = ToolProfile.DEFAULT;
        [SerializeField] private string _selectedProfileName = _staticSelectedProfileName;
        private static TOOL_SETTINGS _staticUnsavedProfile = new TOOL_SETTINGS();
        [SerializeField] private TOOL_SETTINGS _unsavedProfile = _staticUnsavedProfile;

        protected ToolManagerBase() { }

        public static ToolManagerBase<TOOL_SETTINGS> instance
        {
            get
            {
                if (_instance == null) _instance = new ToolManagerBase<TOOL_SETTINGS>();
                return _instance;
            }
        }
        public static TOOL_SETTINGS settings => _staticUnsavedProfile;

        private void UpdateUnsaved()
        {
            var tool = ToolManager.GetToolFromSettings(settings);
            if (ToolManager.tool == ToolManager.PaintTool.NONE || tool != ToolManager.tool) return;
            if (!_staticProfiles.ContainsKey(_staticSelectedProfileName))
                _staticSelectedProfileName = ToolProfile.DEFAULT;
            _staticUnsavedProfile.Copy(_staticProfiles[_staticSelectedProfileName]);
        }

        public string selectedProfileName
        {
            get => _staticSelectedProfileName;
            set
            {
                if (_staticSelectedProfileName == value) return;
                _staticSelectedProfileName = value;
                _selectedProfileName = value;
                UpdateUnsaved();
                _staticUnsavedProfile.DataChanged();
            }
        }

        public string[] profileNames => _staticProfiles.Keys.ToArray();
        public void SaveProfile()
        {
            _staticProfiles[_staticSelectedProfileName].Copy(_staticUnsavedProfile);
            PWBCore.staticData.SaveAndUpdateVersion();
        }
        public void SaveProfileAs(string name)
        {
            if (!_staticProfiles.ContainsKey(name))
            {
                var newProfile = new TOOL_SETTINGS();
                newProfile.Copy(_unsavedProfile);
                _staticProfiles.Add(name, newProfile);
            }
            else _staticProfiles[name].Copy(_staticUnsavedProfile);
            _staticSelectedProfileName = name;
            UpdateUnsaved();
            _staticUnsavedProfile.DataChanged();
            PWBCore.staticData.SaveAndUpdateVersion();
        }
        public void DeleteProfile()
        {
            if (_staticSelectedProfileName == ToolProfile.DEFAULT) return;
            _staticProfiles.Remove(_staticSelectedProfileName);
            _staticSelectedProfileName = ToolProfile.DEFAULT;
            _staticUnsavedProfile.Copy(_staticProfiles[ToolProfile.DEFAULT]);
            _staticUnsavedProfile.DataChanged();
            PWBCore.staticData.SaveAndUpdateVersion();
        }
        public void Revert()
        {
            UpdateUnsaved();
            _staticUnsavedProfile.DataChanged();
            PWBCore.staticData.SaveAndUpdateVersion();
        }

        public void FactoryReset()
        {
            if(_staticUnsavedProfile == null) _staticUnsavedProfile = new TOOL_SETTINGS();
            else
            {
                var newSettings = new TOOL_SETTINGS();
                _staticUnsavedProfile.Copy(newSettings);
            }
            _staticUnsavedProfile.DataChanged();
            PWBCore.staticData.SaveAndUpdateVersion();
        }

        public void CopyToolSettings(TOOL_SETTINGS value) => _staticUnsavedProfile.Copy(value);
        public virtual void OnBeforeSerialize()
        {
            _selectedProfileName = _staticSelectedProfileName;
            _profileKeys = _staticProfiles.Keys.ToArray();
            _profileValues = _staticProfiles.Values.ToArray();
        }

        public virtual void OnAfterDeserialize()
        {
            _staticSelectedProfileName = _selectedProfileName;
            if (_profileKeys.Length > 1)
            {
                _staticProfiles.Clear();
                for (int i = 0; i < _profileKeys.Length; ++i) _staticProfiles.Add(_profileKeys[i], _profileValues[i]);
            }
        }
    }

    public interface IPersistentToolManager
    {
        bool applyBrushToExisting { get; set; }
        IPersistentData[] GetItems();
        void ToggleItemsVisibility();
        void DeletePersistentItem(long itemId, bool deleteObjects, bool registerUndo = true);
        void DeselectAllItems();
        IPersistentData Duplicate(long itemId);
        string GetToolName();
    }

    [System.Serializable]
    public class PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>
        : ToolManagerBase<TOOL_SETTINGS>, IPersistentToolManager
        where TOOL_NAME : IToolName, new()
        where TOOL_SETTINGS : IToolSettings, new()
        where CONTROL_POINT : ControlPoint, new()
        where TOOL_DATA : PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>, new()
        where SCENE_DATA : SceneData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA>, new()
    {
        private static System.Collections.Generic.List<SCENE_DATA> _staticSceneItems = null;
        [SerializeField] private System.Collections.Generic.List<SCENE_DATA> _sceneItems = _staticSceneItems;

        private static bool _staticShowPreexistingElements = true;
        [SerializeField] private bool _showPreexistingElements = _staticShowPreexistingElements;

        private static bool _staticApplyBrushToExisting = false;
        [SerializeField] private bool _applyBrushToExisting = _staticApplyBrushToExisting;

        private static IPersistentData.Visibility _itemsVisibility = IPersistentData.Visibility.SHOW_ALL;
        protected PersistentToolManagerBase() { }
        public new static PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA> instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PersistentToolManagerBase
                        <TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>();
                return _instance as PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>;
            }
        }

        public void AddPersistentItem(string sceneGUID, TOOL_DATA data)
        {
            if (_staticSceneItems == null)
                _staticSceneItems = new System.Collections.Generic.List<SCENE_DATA>();
            var sceneItem = _staticSceneItems.Find(i => i.sceneGUID == sceneGUID);
            if (sceneItem == null)
            {
                sceneItem = new SCENE_DATA();
                sceneItem.sceneGUID = sceneGUID;
                _staticSceneItems.Add(sceneItem);
            }
            if (sceneItem.items != null)
            {
                var item = sceneItem.items.Find(i => i.id == data.id);
                if (item != null) return;
            }
            sceneItem.AddItem(data);
            PWBCore.staticData.SaveAndUpdateVersion();
        }

        public string GetToolName() => (new TOOL_NAME()).value;
        public TOOL_DATA[] GetPersistentItems()
        {
            var items = new System.Collections.Generic.List<TOOL_DATA>();

            if (_staticSceneItems == null) return items.ToArray();

            if (PWBIO.isInPrefabMode)
            {
                var guid = UnityEditor.AssetDatabase.AssetPathToGUID(PWBIO.prefabStage.assetPath);
                var data = _staticSceneItems.Find(item => item.sceneGUID == guid);
                if (data == null) _staticSceneItems.Remove(data);
                else items.AddRange(data.items);
                return items.ToArray();
            }

            var openedSceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (int i = 0; i < openedSceneCount; ++i)
            {
                string sceneGUID = UnityEditor.AssetDatabase.AssetPathToGUID
                    (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).path);
                var data = _staticSceneItems.Find(item => item.sceneGUID == sceneGUID);
                if (data == null)
                {
                    _staticSceneItems.Remove(data);
                    continue;
                }
                items.AddRange(data.items);
            }
            return items.ToArray();
        }

        public IPersistentData[] GetItems() => GetPersistentItems();

        public void DeselectAllItems()
        {
            var items = GetItems();
            foreach (var item in items)
            {
                item.isSelected = false;
                item.ClearSelection();
            }
        }

        public IPersistentData Duplicate(long itemId)
        {
            var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            var sceneGUID = UnityEditor.AssetDatabase.AssetPathToGUID(scenePath);
            var sceneItem = _staticSceneItems.Find(i => i.sceneGUID == sceneGUID);

            var source = GetItem(itemId);
            var clone = new TOOL_DATA();
            clone.Duplicate(source);

            sceneItem.AddItem(clone);
            PWBCore.staticData.SaveAndUpdateVersion();
            return clone;
        }
        public void ToggleItemsVisibility()
        {
            switch (_itemsVisibility)
            {
                case IPersistentData.Visibility.SHOW_ALL: _itemsVisibility = IPersistentData.Visibility.SHOW_OBJECTS; break;
                case IPersistentData.Visibility.SHOW_OBJECTS: _itemsVisibility = IPersistentData.Visibility.HIDE_ALL; break;
                case IPersistentData.Visibility.HIDE_ALL: _itemsVisibility = IPersistentData.Visibility.SHOW_ALL; break;
            }
            var items = GetItems();
            foreach (var item in items) item.visibility = _itemsVisibility;
        }

        public bool ReplaceObject(GameObject target, GameObject obj)
        {
            var items = GetPersistentItems();
            foreach (var item in items)
                if (item.ReplaceObject(target, obj)) return true;
            return false;
        }

        public void RemovePersistentItem(long itemId)
        {
            foreach (var item in _staticSceneItems) item.RemoveItemData(itemId);
            PWBCore.staticData.SaveAndUpdateVersion();
        }
        public void DeletePersistentItem(long itemId, bool deleteObjects, bool registerUndo = true)
        {
            if (registerUndo) ToolProperties.RegisterUndo("Delete Item");
            var parents = new System.Collections.Generic.HashSet<GameObject>();
            foreach (var item in _staticSceneItems)
            {
                var itemParents = item.GetParents(itemId);
                foreach (var parent in itemParents)
                    if (!parents.Contains(parent)) parents.Add(parent);
                item.DeleteItemData(itemId, deleteObjects);
            }

            foreach (var parent in parents)
            {
                var components = parent.GetComponentsInChildren<Component>();
                if (components.Length == 1)
                {
                    if (registerUndo) UnityEditor.Undo.DestroyObjectImmediate(parent);
                    else Object.DestroyImmediate(parent);
                }
            }
            PWBCore.staticData.SaveAndUpdateVersion();
        }
        public TOOL_DATA GetItem(long itemId)
        {
            var items = GetPersistentItems();
            foreach (var item in items)
                if (item.id == itemId) return item;
            return null;
        }

        public TOOL_DATA GetItem(string hexItemId)
        {
            var splittedId = hexItemId.Split('_');
            if (splittedId.Length != 2) return null;
            var provider = new System.Globalization.CultureInfo("en-US");
            if (long.TryParse(splittedId[1], System.Globalization.NumberStyles.AllowHexSpecifier, provider, out long itemId))
                return GetItem(itemId);
            return null;
        }

        public bool showPreexistingElements
        {
            get => _staticShowPreexistingElements;
            set
            {
                if (_staticShowPreexistingElements == value) return;
                _staticShowPreexistingElements = value;
                _showPreexistingElements = value;
                PWBCore.staticData.SaveAndUpdateVersion();
            }
        }

        public bool applyBrushToExisting
        {
            get => _staticApplyBrushToExisting;
            set
            {
                if (_staticApplyBrushToExisting == value) return;
                _staticApplyBrushToExisting = value;
                _applyBrushToExisting = value;
                PWBCore.staticData.SaveAndUpdateVersion();
            }
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            _sceneItems = _staticSceneItems;
            _showPreexistingElements = _staticShowPreexistingElements;
            _applyBrushToExisting = _staticApplyBrushToExisting;
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            _staticSceneItems = _sceneItems;
            _staticShowPreexistingElements = _showPreexistingElements;
            _staticApplyBrushToExisting = _applyBrushToExisting;
        }
    }
    #endregion
    #region SETTINGS
    public interface IToolSettings
    {
        void DataChanged();
        void Copy(IToolSettings other);
    }

    public static class TilesUtils
    {
        public enum SizeType
        {
            SMALLEST_OBJECT,
            BIGGEST_OBJECT,
            CUSTOM
        }

        public static Vector3 GetCellSize(SizeType cellSizeType, MultibrushSettings multibrush,
            Vector3 DefaultValue, bool subtractBrushOffset)
        {
            if (cellSizeType == SizeType.CUSTOM) return DefaultValue;

            void SubtractBrushOffset(ref Vector3 size, MultibrushItemSettings brush)
            {
                var dv = size - brush.localPositionOffset;
#if UNITY_2021_1_OR_NEWER
                dv.x = System.MathF.Round(dv.x, digits: 5);
                dv.y = System.MathF.Round(dv.y, digits: 5);
                dv.z = System.MathF.Round(dv.z, digits: 5);
#else
                dv.x = (float)System.Math.Round(dv.x, digits: 5);
                dv.y = (float)System.Math.Round(dv.y, digits: 5);
                dv.z = (float)System.Math.Round(dv.z, digits: 5);
#endif
                if (dv.x <= 0) dv.x = size.x;
                if (dv.y <= 0) dv.y = size.y;
                if (dv.z <= 0) dv.z = size.z;
                size = dv;
            }

            var cellSize = Vector3.one * (cellSizeType == SizeType.SMALLEST_OBJECT
                   ? float.MaxValue : float.MinValue);
            foreach (var item in multibrush.items)
            {
                var prefab = item.prefab;
                if (prefab == null) continue;
                var scaleMultiplier = cellSizeType == SizeType.SMALLEST_OBJECT
                    ? item.minScaleMultiplier : item.maxScaleMultiplier;
                var bounds = BoundsUtils.GetBoundsRecursive(prefab.transform,
                    prefab.transform.rotation * Quaternion.Euler(multibrush.eulerOffset), ignoreDissabled: true,
                    BoundsUtils.ObjectProperty.BOUNDING_BOX, recursive: true, useDictionary: false);
                var localSize = bounds.size;
                if (subtractBrushOffset) SubtractBrushOffset(ref localSize, item);
                var itemSize = Vector3.Scale(localSize, scaleMultiplier);
                cellSize = cellSizeType == SizeType.SMALLEST_OBJECT
                    ? Vector3.Min(cellSize, itemSize) : Vector3.Max(cellSize, itemSize);
            }
            return cellSize;
        }

        public static Vector3 GetCellSize(SizeType cellSizeType, BrushSettings brush,
            AxesUtils.SignedAxis upwardAxis, AxesUtils.SignedAxis forwardAxis,
            Vector3 defaultValue, bool tangentSpace, int quarterTurns, bool subtractBrushOffset)
        {
            if (brush == null) return defaultValue;
            void SubtractBrushOffset(ref Vector3 size)
            {
                var dv = size - brush.localPositionOffset;
#if UNITY_2021_1_OR_NEWER
                dv.x = System.MathF.Round(dv.x, digits: 5);
                dv.y = System.MathF.Round(dv.y, digits: 5);
                dv.z = System.MathF.Round(dv.z, digits: 5);
#else
                dv.x = (float)System.Math.Round(dv.x, digits: 5);
                dv.y = (float)System.Math.Round(dv.y, digits: 5);
                dv.z = (float)System.Math.Round(dv.z, digits: 5);
#endif
                if (dv.x <= 0) dv.x = size.x;
                if (dv.y <= 0) dv.y = size.y;
                if (dv.z <= 0) dv.z = size.z;
                size = dv;
            }

            if (cellSizeType == SizeType.CUSTOM)
            {
                if (subtractBrushOffset) SubtractBrushOffset(ref defaultValue);
                return defaultValue;
            }

            var cellSize = Vector3.one * (cellSizeType == SizeType.SMALLEST_OBJECT
                    ? float.MaxValue : float.MinValue);
            if (ToolManager.tool == ToolManager.PaintTool.TILING && ToolManager.editMode
                && PWBIO.selectedPersistentTilingData != null)
            {
                var prefabs = new System.Collections.Generic.HashSet<GameObject>();
                var objSet = PWBIO.selectedPersistentTilingData.objectSet;
                var scaleMultiplier = cellSizeType == SizeType.SMALLEST_OBJECT
                        ? brush.minScaleMultiplier : brush.maxScaleMultiplier;

                foreach (var obj in objSet)
                {
                    if (obj == null) continue;
                    var objSize = BoundsUtils.GetBoundsRecursive(obj.transform,
                        obj.transform.rotation * Quaternion.Euler(brush.eulerOffset)).size;
                    cellSize = cellSizeType == SizeType.SMALLEST_OBJECT
                        ? Vector3.Min(cellSize, objSize) : Vector3.Max(cellSize, objSize);
                    var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                    if (prefab == null) continue;
                    if (prefabs.Contains(prefab)) continue;
                    prefabs.Add(prefab);
                    var prefabSize = Vector3.Scale(BoundsUtils.GetBoundsRecursive(prefab.transform,
                       prefab.transform.rotation * Quaternion.Euler(brush.eulerOffset)).size, scaleMultiplier);
                    cellSize = cellSizeType == SizeType.SMALLEST_OBJECT
                        ? Vector3.Min(cellSize, prefabSize) : Vector3.Max(cellSize, prefabSize);
                }
            }
            else if (brush is MultibrushSettings)
            {
                var multibrush = brush as MultibrushSettings;
                cellSize = GetCellSize(cellSizeType, multibrush, defaultValue, subtractBrushOffset);
            }

            if (cellSize == Vector3.one * float.MaxValue || cellSize == Vector3.one * float.MinValue) return defaultValue;
            var rotation = Quaternion.Euler(AxesUtils.SignedAxis.GetEulerAnglesFromAxes(forwardAxis, upwardAxis));

            if (tangentSpace)
            {
                if (upwardAxis.axis == AxesUtils.Axis.Y) cellSize.y = cellSize.z;
                else if (upwardAxis.axis == AxesUtils.Axis.X)
                {
                    cellSize.x = cellSize.y;
                    cellSize.y = cellSize.z;
                }
            }
            else
            {
                cellSize = rotation * cellSize;
                if (quarterTurns > 0) cellSize = Quaternion.AngleAxis(quarterTurns * 90, upwardAxis) * cellSize;
                cellSize.x = Mathf.Abs(cellSize.x);
                cellSize.y = Mathf.Abs(cellSize.y);
                cellSize.z = Mathf.Abs(cellSize.z);
            }
            if (Mathf.Approximately(cellSize.x, 0)) cellSize.x = 0.5f;
            if (Mathf.Approximately(cellSize.y, 0)) cellSize.y = 0.5f;
            if (Mathf.Approximately(cellSize.z, 0)) cellSize.z = 0.5f;
            return cellSize;
        }
    }

    #region CIRCLE TOOLS
    [System.Serializable]
    public class CircleToolBase : IToolSettings
    {
        [SerializeField] private float _radius = 1f;

        public float radius
        {
            get => _radius;
            set
            {
                value = Mathf.Max(value, 0.05f);
                if (_radius == value) return;
                _radius = value;
                DataChanged();
            }
        }

        public virtual void Copy(IToolSettings other)
        {
            var otherCircleToolBase = other as CircleToolBase;
            if (otherCircleToolBase == null) return;
            _radius = otherCircleToolBase._radius;
        }


        public virtual void DataChanged() => PWBCore.SetSavePending();


    }
    [System.Serializable]
    public class BrushToolBase : CircleToolBase, IPaintToolSettings, IToolParentingSettings
    {
        [SerializeField] private PaintToolSettings _paintTool = new PaintToolSettings();
        public enum BrushShape { POINT, CIRCLE, SQUARE }
        [SerializeField] protected BrushShape _brushShape = BrushShape.CIRCLE;
        [SerializeField] private int _density = 50;
        [SerializeField] private bool _orientAlongBrushstroke = false;
        [SerializeField] private Vector3 _additionalOrientationAngle = Vector3.zero;
        public enum SpacingType { AUTO, CUSTOM }
        [SerializeField] private SpacingType _spacingType = SpacingType.AUTO;
        [SerializeField] protected float _minSpacing = 1f;
        [SerializeField] private bool _randomizePositions = true;
        [SerializeField] private float _randomness = 1f;
        public BrushToolBase() : base() => _paintTool.OnDataChanged += DataChanged;

        public BrushShape brushShape
        {
            get => _brushShape;
            set
            {
                if (_brushShape == value) return;
                _brushShape = value;
                DataChanged();
            }
        }
        public int density
        {
            get => _density;
            set
            {
                value = Mathf.Clamp(value, 0, 100);
                if (_density == value) return;
                _density = value;
                DataChanged();
            }
        }
        public bool orientAlongBrushstroke
        {
            get => _orientAlongBrushstroke;
            set
            {
                if (_orientAlongBrushstroke == value) return;
                _orientAlongBrushstroke = value;
                DataChanged();
            }
        }
        public Vector3 additionalOrientationAngle
        {
            get => _additionalOrientationAngle;
            set
            {
                if (_additionalOrientationAngle == value) return;
                _additionalOrientationAngle = value;
                DataChanged();
            }
        }
        public SpacingType spacingType
        {
            get => _spacingType;
            set
            {
                if (_spacingType == value) return;
                _spacingType = value;
                DataChanged();
            }
        }
        public float minSpacing
        {
            get => _minSpacing;
            set
            {
                if (_minSpacing == value) return;
                _minSpacing = value;
                DataChanged();
            }
        }
        public bool randomizePositions
        {
            get => _randomizePositions;
            set
            {
                if (_randomizePositions == value) return;
                _randomizePositions = value;
                DataChanged();
            }
        }

        public float randomness
        {
            get => _randomness;
            set
            {
                value = Mathf.Clamp01(value);
                if (_randomness == value) return;
                _randomness = value;
                DataChanged();
            }
        }

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
            set => _paintTool.overwriteBrushProperties = value;
        }

        public BrushSettings brushSettings => _paintTool.brushSettings;

        public bool overwriteParentingSettings
        {
            get => _paintTool.overwriteParentingSettings;
            set => _paintTool.overwriteParentingSettings = value;
        }

        public IToolParentingSettings GetParentingSettings() => _paintTool as ToolParentingSettings;

        public override void DataChanged()
        {
            base.DataChanged();
            BrushstrokeManager.UpdateBrushstroke();
        }

        public override void Copy(IToolSettings other)
        {
            var otherBrushToolBase = other as BrushToolBase;
            if (otherBrushToolBase == null) return;
            base.Copy(other);
            _paintTool.Copy(otherBrushToolBase._paintTool);
            _brushShape = otherBrushToolBase._brushShape;
            _density = otherBrushToolBase.density;
            _orientAlongBrushstroke = otherBrushToolBase._orientAlongBrushstroke;
            _additionalOrientationAngle = otherBrushToolBase._additionalOrientationAngle;
            _spacingType = otherBrushToolBase._spacingType;
            _minSpacing = otherBrushToolBase._minSpacing;
            _randomizePositions = otherBrushToolBase._randomizePositions;
        }

    }
    #endregion
    #region PAINT TOOLS

    public interface IToolParentingSettings
    {
        bool autoCreateParent { get; set; }
        bool setSurfaceAsParent { get; set; }
        bool setLastSelectedAsParent { get; set; }

        bool createSubparentPerPalette { get; set; }
        bool createSubparentPerTool { get; set; }
        bool createSubparentPerBrush { get; set; }
        bool createSubparentPerPrefab { get; set; }
        Transform parent { get; set; }
    }

    public interface IPaintToolSettings
    {
        bool autoCreateParent { get; set; }
        bool setSurfaceAsParent { get; set; }
        bool setLastSelectedAsParent { get; set; }

        bool createSubparentPerPalette { get; set; }
        bool createSubparentPerTool { get; set; }
        bool createSubparentPerBrush { get; set; }
        bool createSubparentPerPrefab { get; set; }
        Transform parent { get; set; }
        bool overwriteParentingSettings { get; set; }

        bool overwritePrefabLayer { get; set; }
        int layer { get; set; }
        bool overwriteBrushProperties { get; set; }
        BrushSettings brushSettings { get; }
        IToolParentingSettings GetParentingSettings();
    }

    [System.Serializable]
    public class ToolParentingSettings : IToolParentingSettings, ISerializationCallbackReceiver, IToolSettings
    {
        private Transform _parent = null;
        [SerializeField] private string _parentGlobalId = null;
        [SerializeField] private bool _autoCreateParent = true;
        [SerializeField] private bool _setSurfaceAsParent = false;
        [SerializeField] private bool _setLastSelectedAsParent = false;
        [SerializeField] private bool _createSubparentPerPalette = true;
        [SerializeField] private bool _createSubparentPerTool = true;
        [SerializeField] private bool _createSubparentPerBrush = false;
        [SerializeField] private bool _createSubparentPerPrefab = false;

        public System.Action OnDataChanged;

        public ToolParentingSettings()
        {
            OnDataChanged += DataChanged;
        }

        public Transform parent
        {
            get
            {
                if (_parent == null && _parentGlobalId != null)
                {
                    var obj = ObjectId.FindObject<GameObject>(_parentGlobalId, ignorePrefabMode: true);
                    if (obj == null) _parentGlobalId = null;
                    else _parent = obj.transform;
                }
                return _parent;
            }
            set
            {
                if (_parent == value) return;
                _parent = value;
                _parentGlobalId = _parent == null ? null
                    : UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(_parent.gameObject).ToString();
                OnDataChanged();
            }
        }
        public bool autoCreateParent
        {
            get => _autoCreateParent;
            set
            {
                if (_autoCreateParent == value) return;
                _autoCreateParent = value;
                OnDataChanged();
            }
        }

        public bool setSurfaceAsParent
        {
            get => _setSurfaceAsParent;
            set
            {
                if (_setSurfaceAsParent == value) return;
                _setSurfaceAsParent = value;
                OnDataChanged();
            }
        }

        public bool setLastSelectedAsParent
        {
            get => _setLastSelectedAsParent;
            set
            {
                if (_setLastSelectedAsParent == value) return;
                _setLastSelectedAsParent = value;
                OnDataChanged();
            }
        }

        public bool createSubparentPerPalette
        {
            get => _createSubparentPerPalette;
            set
            {
                if (_createSubparentPerPalette == value) return;
                _createSubparentPerPalette = value;
                OnDataChanged();
            }
        }
        public bool createSubparentPerTool
        {
            get => _createSubparentPerTool;
            set
            {
                if (_createSubparentPerTool == value) return;
                _createSubparentPerTool = value;
                OnDataChanged();
            }
        }
        public bool createSubparentPerBrush
        {
            get => _createSubparentPerBrush;
            set
            {
                if (_createSubparentPerBrush == value) return;
                _createSubparentPerBrush = value;
                OnDataChanged();
            }
        }

        public bool createSubparentPerPrefab
        {
            get => _createSubparentPerPrefab;
            set
            {
                if (_createSubparentPerPrefab == value) return;
                _createSubparentPerPrefab = value;
                OnDataChanged();
            }
        }

        public virtual void DataChanged() => PWBCore.SetSavePending();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() => _parent = null;

        public virtual void Copy(IToolSettings other)
        {
            var otherPaintToolSettings = other as ToolParentingSettings;
            if (otherPaintToolSettings == null) return;

            _autoCreateParent = otherPaintToolSettings._autoCreateParent;
            _setSurfaceAsParent = otherPaintToolSettings._setSurfaceAsParent;
            _createSubparentPerPalette = otherPaintToolSettings._createSubparentPerPalette;
            _createSubparentPerTool = otherPaintToolSettings._createSubparentPerTool;
            _createSubparentPerBrush = otherPaintToolSettings._createSubparentPerBrush;
            _createSubparentPerPrefab = otherPaintToolSettings._createSubparentPerPrefab;
        }
    }

    [System.Serializable]
    public class PaintToolSettings : ToolParentingSettings, IPaintToolSettings
    {
        [SerializeField] private bool _overwritePrefabLayer = false;
        [SerializeField] private int _layer = 0;
        [SerializeField] private bool _overwriteBrushProperties = false;
        [SerializeField] private BrushSettings _brushSettings = new BrushSettings();
        [SerializeField] private bool _overwriteParentingSettings = false;

        public PaintToolSettings()
        {
            _brushSettings.OnDataChangedAction += DataChanged;
        }

        public bool overwritePrefabLayer
        {
            get => _overwritePrefabLayer;
            set
            {
                if (_overwritePrefabLayer == value) return;
                _overwritePrefabLayer = value;
                OnDataChanged();
            }
        }

        public int layer
        {
            get => _layer;
            set
            {
                if (_layer == value) return;
                _layer = value;
                OnDataChanged();
            }
        }

        public bool overwriteBrushProperties
        {
            get => _overwriteBrushProperties;
            set
            {
                if (_overwriteBrushProperties == value) return;
                _overwriteBrushProperties = value;
                OnDataChanged();
            }
        }

        public BrushSettings brushSettings => _brushSettings;

        public bool overwriteParentingSettings
        {
            get => _overwriteParentingSettings;
            set
            {
                if (_overwriteParentingSettings == value) return;
                _overwriteParentingSettings = value;
                OnDataChanged();
            }
        }

        public override void Copy(IToolSettings other)
        {
            var otherPaintToolSettings = other as PaintToolSettings;
            if (otherPaintToolSettings == null) return;
            base.Copy(otherPaintToolSettings);
            _overwritePrefabLayer = otherPaintToolSettings._overwritePrefabLayer;
            _layer = otherPaintToolSettings._layer;
            _overwriteBrushProperties = otherPaintToolSettings._overwriteBrushProperties;
            _brushSettings.Copy(otherPaintToolSettings._brushSettings);
            _overwriteParentingSettings = otherPaintToolSettings._overwriteParentingSettings;
        }

        public IToolParentingSettings GetParentingSettings() => this as ToolParentingSettings;
    }
    #endregion
    #region PAINT ON SURFACE TOOLS
    public interface IPaintOnSurfaceToolSettings
    {
        bool paintOnMeshesWithoutCollider { get; set; }
        bool ignoreSceneColliders { get; set; }
        bool paintOnSelectedOnly { get; set; }
        bool paintOnPalettePrefabs { get; set; }
    }

    public abstract class PaintOnSurfaceToolSettingsBase : IPaintOnSurfaceToolSettings
    {
        public abstract bool paintOnMeshesWithoutCollider { get; set; }
        public abstract bool ignoreSceneColliders { get; set; }
        public abstract bool paintOnSelectedOnly { get; set; }
        public abstract bool paintOnPalettePrefabs { get; set; }
        public enum PaintMode
        {
            AUTO,
            ON_SURFACE,
            ON_SHAPE
        }
    }

    [System.Serializable]
    public class PaintOnSurfaceToolSettings : PaintOnSurfaceToolSettingsBase,
        ISerializationCallbackReceiver, IToolSettings
    {
        [SerializeField] private bool _paintOnMeshesWithoutCollider = false;
        [SerializeField] private bool _ignoreScenetColliders = false;
        [SerializeField] private bool _paintOnSelectedOnly = false;
        [SerializeField] private bool _paintOnPalettePrefabs = false;
        [SerializeField] private PaintMode _mode = PaintMode.AUTO;
        [SerializeField] private bool _paralellToTheSurface = true;
        public System.Action OnDataChanged;

        public PaintOnSurfaceToolSettings() => OnDataChanged += DataChanged;
        public PaintMode mode
        {
            get => _mode;
            set
            {
                if (_mode == value) return;
                _mode = value;
                OnDataChanged();
            }
        }
        public bool perpendicularToTheSurface
        {
            get => _paralellToTheSurface;
            set
            {
                if (_paralellToTheSurface == value) return;
                _paralellToTheSurface = value;
                OnDataChanged();
            }
        }

        public override bool paintOnMeshesWithoutCollider
        {
            get => _paintOnMeshesWithoutCollider;
            set
            {
                if (_paintOnMeshesWithoutCollider == value) return;
                _paintOnMeshesWithoutCollider = value;
                OnDataChanged();
            }
        }

        public override bool ignoreSceneColliders
        {
            get => _ignoreScenetColliders;
            set
            {
                if(_ignoreScenetColliders == value) return;
                _ignoreScenetColliders = value;
                OnDataChanged();
            }
        }
        public override bool paintOnSelectedOnly
        {
            get => _paintOnSelectedOnly;
            set
            {
                if (_paintOnSelectedOnly == value) return;
                _paintOnSelectedOnly = value;
                OnDataChanged();
            }
        }

        public override bool paintOnPalettePrefabs
        {
            get => _paintOnPalettePrefabs;
            set
            {
                if (_paintOnPalettePrefabs == value) return;
                _paintOnPalettePrefabs = value;
                OnDataChanged();
            }
        }

        public virtual void Copy(IToolSettings other)
        {
            var otherPaintOnSurfaceToolSettings = other as PaintOnSurfaceToolSettings;
            if (otherPaintOnSurfaceToolSettings == null) return;
            _paintOnMeshesWithoutCollider = otherPaintOnSurfaceToolSettings._paintOnMeshesWithoutCollider;
            _paintOnSelectedOnly = otherPaintOnSurfaceToolSettings._paintOnSelectedOnly;
            _paintOnPalettePrefabs = otherPaintOnSurfaceToolSettings._paintOnPalettePrefabs;
            _mode = otherPaintOnSurfaceToolSettings._mode;
            _paralellToTheSurface = otherPaintOnSurfaceToolSettings._paralellToTheSurface;
        }
        public virtual void DataChanged()
        {
            PWBCore.SetSavePending();
        }
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() { }
    }
    #endregion
    #region SELECTION TOOLS
    [System.Serializable]
    public class SelectionToolBaseBasic : IToolSettings
    {
        [SerializeField] private bool _embedInSurface = false;
        [SerializeField] private bool _embedAtPivotHeight = false;
        [SerializeField] private float _surfaceDistance = 0f;
        [SerializeField] private bool _createTempColliders = true;

        public bool embedInSurface
        {
            get => _embedInSurface;
            set
            {
                if (_embedInSurface == value) return;
                _embedInSurface = value;
                DataChanged();
            }
        }

        public bool embedAtPivotHeight
        {
            get => _embedAtPivotHeight;
            set
            {
                if (_embedAtPivotHeight == value) return;
                _embedAtPivotHeight = value;
                DataChanged();
            }
        }

        public float surfaceDistance
        {
            get => _surfaceDistance;
            set
            {
                if (_surfaceDistance == value) return;
                _surfaceDistance = value;
                DataChanged();
            }
        }

        public bool createTempColliders
        {
            get => _createTempColliders;
            set
            {
                if (_createTempColliders == value) return;
                _createTempColliders = value;
                DataChanged();
            }
        }
        public virtual void Copy(IToolSettings other)
        {
            var otherSelectionTool = other as SelectionToolBaseBasic;
            if (otherSelectionTool == null) return;
            _embedInSurface = otherSelectionTool._embedInSurface;
            _embedAtPivotHeight = otherSelectionTool._embedAtPivotHeight;
            _surfaceDistance = otherSelectionTool._surfaceDistance;
            _createTempColliders = otherSelectionTool._createTempColliders;
        }

        public virtual void DataChanged() => PWBCore.SetSavePending();
    }

    [System.Serializable]
    public class SelectionToolBase : SelectionToolBaseBasic
    {
        [SerializeField] private bool _rotateToTheSurface = false;

        public bool rotateToTheSurface
        {
            get => _rotateToTheSurface;
            set
            {
                if (_rotateToTheSurface == value) return;
                _rotateToTheSurface = value;
                DataChanged();
            }
        }
        public override void Copy(IToolSettings other)
        {
            var otherSelectionTool = other as SelectionToolBase;
            if (otherSelectionTool == null) return;
            base.Copy(other);
            _rotateToTheSurface = otherSelectionTool._rotateToTheSurface;
        }
    }

    public interface ISelectionBrushTool
    {
        ModifierToolSettings.Command command { get; set; }
        bool onlyTheClosest { get; set; }
        bool outermostPrefabFilter { get; set; }
    }
    public interface IModifierTool
    {
        bool modifyAllButSelected { get; set; }
    }

    [System.Serializable]
    public class SelectionBrushToolSettings : ISelectionBrushTool, IToolSettings
    {
        public enum Command
        {
            SELECT_ALL,
            SELECT_PALETTE_PREFABS,
            SELECT_BRUSH_PREFABS
        }

        [SerializeField] private Command _command = Command.SELECT_ALL;
        [SerializeField] private bool _onlyTheClosest = false;
        [SerializeField] private bool _outermostPrefabFilter = true;
        public System.Action OnDataChanged;

        public SelectionBrushToolSettings() => OnDataChanged += DataChanged;
        public Command command
        {
            get => _command;
            set
            {
                if (_command == value) return;
                _command = value;
                DataChanged();
            }
        }

        public bool onlyTheClosest
        {
            get => _onlyTheClosest;
            set
            {
                if (_onlyTheClosest == value) return;
                _onlyTheClosest = value;
                DataChanged();
            }
        }

        public bool outermostPrefabFilter
        {
            get => _outermostPrefabFilter;
            set
            {
                if (_outermostPrefabFilter == value) return;
                _outermostPrefabFilter = value;
                DataChanged();
            }
        }
        public void DataChanged() => PWBCore.SetSavePending();

        public virtual void Copy(IToolSettings other)
        {
            var otherModifier = other as ISelectionBrushTool;
            if (otherModifier == null) return;
            _command = otherModifier.command;
            _onlyTheClosest = otherModifier.onlyTheClosest;
            _outermostPrefabFilter = otherModifier.outermostPrefabFilter;
        }

    }

    [System.Serializable]
    public class ModifierToolSettings : SelectionBrushToolSettings, IModifierTool
    {

        [SerializeField] private bool _allButSelected = false;

        public bool modifyAllButSelected
        {
            get => _allButSelected;
            set
            {
                if (_allButSelected == value) return;
                _allButSelected = value;
                DataChanged();
            }
        }

        public override void Copy(IToolSettings other)
        {
            var otherModifier = other as IModifierTool;
            if (otherModifier == null) return;
            base.Copy(other);
            _allButSelected = otherModifier.modifyAllButSelected;
        }
    }
    #endregion
    #region MODULAR TOOLS
    public class ModularToolBase : IToolSettings, IPaintToolSettings
    {
        [SerializeField] private TilesUtils.SizeType _moduleSizeType = TilesUtils.SizeType.BIGGEST_OBJECT;
        [SerializeField] protected Vector3 _moduleSize = Vector3.one;
        [SerializeField] protected bool _subtractBrushOffset = false;
        [SerializeField] private Vector3 _spacing = Vector3.zero;
        [SerializeField] protected AxesUtils.SignedAxis _upwardAxis = AxesUtils.SignedAxis.UP;
        [SerializeField] private AxesUtils.SignedAxis _forwardAxis = AxesUtils.SignedAxis.FORWARD;
        public System.Action OnDataChanged;

        public ModularToolBase() : base()
        {
            _paintTool.OnDataChanged += DataChanged;
            OnDataChanged += DataChanged;
        }
        public virtual TilesUtils.SizeType moduleSizeType
        {
            get => _moduleSizeType;
            set
            {
                if (_moduleSizeType == value) return;
                _moduleSizeType = value;
                UpdateCellSize();
                OnDataChanged();
            }
        }
        public virtual Vector3 moduleSize
        {
            get => _moduleSize;
            set
            {
                if (_moduleSize == value) return;
                _moduleSize = value;
                OnDataChanged();
            }
        }
        public bool subtractBrushOffset
        {
            get => _subtractBrushOffset;
            set
            {
                if (_subtractBrushOffset == value) return;
                _subtractBrushOffset = value;
                UpdateCellSize();
                OnDataChanged();
            }
        }
        public Vector3 spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                OnDataChanged();
            }
        }
        public AxesUtils.SignedAxis upwardAxis
        {
            get => _upwardAxis;
            set
            {
                if (_upwardAxis == value) return;
                if (_forwardAxis.axis == value) _forwardAxis = _upwardAxis;
                _upwardAxis = value;
                UpdateCellSize();
                OnDataChanged();
            }
        }
        public AxesUtils.SignedAxis forwardAxis
        {
            get => _forwardAxis;
            set
            {
                if (_forwardAxis == value) return;
                if (_upwardAxis.axis == value) _upwardAxis = _forwardAxis;
                _forwardAxis = value;
                UpdateCellSize();
                OnDataChanged();
            }
        }
        public void SetUpwardAxis(AxesUtils.SignedAxis value) => _upwardAxis = value;
        public void SetForwardAxis(AxesUtils.SignedAxis value) => _forwardAxis = value;
        public virtual Vector3 GetCellSize(BrushSettings brush)
        {
            if (moduleSizeType == TilesUtils.SizeType.CUSTOM) return _moduleSize;
            if (brush == null) return Vector3.one;
            int quarterTurns = 0;
            if (this is FloorSettings) quarterTurns = FloorManager.quarterTurns;
            return TilesUtils.GetCellSize(moduleSizeType, brush, upwardAxis, forwardAxis,
                moduleSize, tangentSpace: false, quarterTurns, FloorManager.settings.subtractBrushOffset);
        }
        public virtual void UpdateCellSize()
        {
            if (moduleSizeType == TilesUtils.SizeType.CUSTOM && !FloorManager.settings.subtractBrushOffset) return;

            BrushSettings brush = PaletteManager.selectedBrush;
            if (overwriteBrushProperties) brush = brushSettings;
            if (brush == null) return;

            int quarterTurns = 0;
            if (this is FloorSettings) quarterTurns = FloorManager.quarterTurns;
            else if (this is WallSettings) quarterTurns = WallManager.halfTurn ? 2 : 0;
            _moduleSize = TilesUtils.GetCellSize(moduleSizeType, brush, upwardAxis, forwardAxis,
                moduleSize, tangentSpace: false, quarterTurns, FloorManager.settings.subtractBrushOffset);
            ToolProperties.RepainWindow();
            UnityEditor.SceneView.RepaintAll();
        }

        public void SetCellSize(Vector3 value)
        {
            _moduleSize = value;
            ToolProperties.RepainWindow();
            UnityEditor.SceneView.RepaintAll();
        }
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
            set => _paintTool.overwriteBrushProperties = value;
        }
        public BrushSettings brushSettings => _paintTool.brushSettings;
        public bool overwriteParentingSettings
        {
            get => _paintTool.overwriteParentingSettings;
            set => _paintTool.overwriteParentingSettings = value;
        }
        public IToolParentingSettings GetParentingSettings() => _paintTool as ToolParentingSettings;
        #endregion
        public void DataChanged()
        {
            PWBCore.SetSavePending();
        }
        public virtual void Copy(IToolSettings other)
        {
            var otherModularToolSettings = other as ModularToolBase;
            if (otherModularToolSettings == null) return;
            _paintTool.Copy(otherModularToolSettings._paintTool);
            _moduleSizeType = otherModularToolSettings._moduleSizeType;
            _moduleSize = otherModularToolSettings._moduleSize;
            _spacing = otherModularToolSettings._spacing;
            _upwardAxis = otherModularToolSettings._upwardAxis;
        }
    }
    #endregion
    #endregion
    #region DATA
    [System.Serializable]
    public class ControlPoint
    {
        public Vector3 position = Vector3.zero;
        public ControlPoint() { }

        public ControlPoint(Vector3 position) => this.position = position;
        public ControlPoint(ControlPoint other) => position = other.position;

        public virtual void Copy(ControlPoint other)
        {
            position = other.position;
        }
        public static implicit operator ControlPoint(Vector3 position) => new ControlPoint(position);
        public static implicit operator Vector3(ControlPoint point) => point.position;
        public static Vector3[] PointArrayToVectorArray(ControlPoint[] array)
            => array.Select(point => point.position).ToArray();
        public static ControlPoint[] VectorArrayToPointArray(Vector3[] array)
            => array.Select(position => new ControlPoint(position)).ToArray();
    }

    [System.Serializable]
    public struct ObjectId : System.IEquatable<ObjectId>
    {
        [SerializeField] private string _globalObjId;
        public string globalObjId { get => _globalObjId; set => _globalObjId = value; }

        public ObjectId(GameObject gameObject)
        {
            _globalObjId = null;
            if (gameObject == null) return;
            if(PWBIO.isInPrefabMode)
            {
                _globalObjId = string.Empty;
                Transform current = gameObject.transform;

                var outermostPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
                if (outermostPrefab == null) return;
                var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(outermostPrefab);
                if (prefab == null) return;

                while (current != null && current != PWBIO.prefabStage.prefabContentsRoot.transform)
                {
                    var index = current.GetSiblingIndex().ToString();
                    _globalObjId = string.IsNullOrEmpty(_globalObjId) ? index : index + "," + _globalObjId;
                    current = current.parent;
                }
                UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string prefabGuid, out long localId);
                _globalObjId = prefabGuid + "," + _globalObjId;
                _globalObjId = UnityEditor.AssetDatabase.AssetPathToGUID(PWBIO.prefabStage.assetPath) + "," + _globalObjId;
                return;
            }
            _globalObjId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
        }

        public ObjectId(string globalObjId)
        {
            _globalObjId = globalObjId;
        }
        public override int GetHashCode()
        {
            int hashCode = 917907199;
            hashCode = hashCode * -1521134295
                + System.Collections.Generic.EqualityComparer<string>.Default.GetHashCode(_globalObjId);
            return hashCode;
        }
        public bool Equals(ObjectId other) => _globalObjId == other._globalObjId;
        public override bool Equals(object obj) => obj is ObjectId other && this.Equals(other);
        public static bool operator ==(ObjectId lhs, ObjectId rhs) => lhs.Equals(rhs);
        public static bool operator !=(ObjectId lhs, ObjectId rhs) => !lhs.Equals(rhs);

        public void Copy(ObjectId other)
        {
            _globalObjId = other._globalObjId;
        }

        public static T FindObject<T>(string globObjId, bool ignorePrefabMode) where T : Object
        {
            if (PWBIO.isInPrefabMode && !ignorePrefabMode)
            {
                var indices = globObjId.Split(',');
                var rootId = indices[0];
                var rootPath = UnityEditor.AssetDatabase.GUIDToAssetPath(rootId);
                if (rootPath != PWBIO.prefabStage.assetPath) return null;
                var prefabId = indices[1];
                var current = PWBIO.prefabStage.prefabContentsRoot.transform;
                for(int i = 2; i < indices.Length; ++i)
                {
                    var indexStr = indices[i];
                    if (!int.TryParse(indexStr, out int childIndex)) return null;
                    if (childIndex < 0 || childIndex >= current.childCount) return null;
                    current = current.GetChild(childIndex);
                }
                var outermostPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(current);
                if (outermostPrefab == null) return null;
                var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(outermostPrefab);
                if (prefab == null) return null;
                UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab,
                    out string currentPrefabGuid, out long localId);
                if(currentPrefabGuid != prefabId) return null;
                return current.gameObject as T;
            }
            
            T obj = null;
            if (UnityEditor.GlobalObjectId.TryParse(globObjId, out UnityEditor.GlobalObjectId id))
                obj = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as T;
            return obj;
        }

        public static GameObject FindObject(ObjectId objId)
            => FindObject<GameObject>(objId.globalObjId, ignorePrefabMode: false);
    }

    [System.Serializable]
    public struct ObjectPose
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] private Quaternion _localRotation;
        [SerializeField] private Vector3 _localScale;

        public ObjectPose(Vector3 position, Quaternion localRotation, Vector3 localScale)
        {
            _position = position;
            _localRotation = localRotation;
            _localScale = localScale;
        }

        public ObjectPose(GameObject obj)
        {
            _position = obj.transform.position;
            _localRotation = obj.transform.localRotation;
            _localScale = obj.transform.localScale;
        }
        public Vector3 position { get => _position; set => _position = value; }
        public Quaternion localRotation { get => _localRotation; set => _localRotation = value; }
        public Vector3 localScale { get => _localScale; set => _localScale = value; }
        public void Copy(ObjectPose other)
        {
            _position = other._position;
            _localRotation = other._localRotation;
            _localScale = other._localScale;
        }
    }

    #region PERSISTENT DATA
    public interface IToolName { string value { get; } }

    public interface IPersistentData
    {
        long id { get; }
        string name { get; set; }
        void Rename(string newName, bool renameParentObject);
        public enum Visibility
        {
            SHOW_ALL,
            SHOW_OBJECTS,
            HIDE_ALL,
        }
        Visibility visibility { get; set; }
        void ToggleVisibility();
        GameObject[] objects { get; }
        bool isSelected { get; set; }
        void ToggleSelection();
        void ClearSelection();
        void SelectAll();
        bool AllPointsAreSelected();
        Bounds GetBounds(float sizeMultiplier);
        GameObject GetParent();
        bool ControlPointIsSelected(int idx);
        string toolName { get; }
    }
    [System.Serializable]
    public class PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> : IPersistentData, ISerializationCallbackReceiver
        where TOOL_NAME : IToolName, new()
        where TOOL_SETTINGS : IToolSettings, new()
        where CONTROL_POINT : ControlPoint, new()
    {
        #region ID & NAME
        private static long _nextId = System.DateTime.Now.Ticks;
        [SerializeField] protected long _id = _nextId;
        public static long nextId => _nextId;
        public static string HexId(long value) => (new TOOL_NAME()).value + "_" + value.ToString("X");
        public static string nextHexId => HexId(_nextId);
        public static void SetNextId() => _nextId = System.DateTime.Now.Ticks;
        public long id => _id;
        public string hexId => HexId(id);

        [SerializeField] private string _name = string.Empty;
        public string name
        {
            get
            {
                if (string.IsNullOrEmpty(_name)) _name = hexId;
                return _name;
            }
            set
            {
                if (_name == value) return;
                _name = value;
                PWBCore.SetSavePending();
            }
        }

        public void Rename(string newName, bool renameParentObject)
        {
            var oldName = name;
            if (oldName == newName) return;
            name = newName;
            if (!renameParentObject) return;
            var parent = GetParent().transform;
            if (parent == null) return;
            do
            {
                if (parent.name == oldName)
                {
                    parent.name = newName;
                    return;
                }
                parent = parent.transform.parent;
            }
            while (parent != null);
        }
        #endregion
        #region VISIBILITY
        [SerializeField] private IPersistentData.Visibility _visibility = IPersistentData.Visibility.SHOW_ALL;
        public IPersistentData.Visibility visibility
        {
            get => _visibility;
            set
            {
                if (_visibility == value) return;
                _visibility = value;
                PWBCore.SetSavePending();
            }
        }
        public void ToggleVisibility()
        {
            switch (visibility)
            {
                case IPersistentData.Visibility.SHOW_ALL: _visibility = IPersistentData.Visibility.SHOW_OBJECTS; break;
                case IPersistentData.Visibility.SHOW_OBJECTS: _visibility = IPersistentData.Visibility.HIDE_ALL; break;
                case IPersistentData.Visibility.HIDE_ALL: _visibility = IPersistentData.Visibility.SHOW_ALL; break;
            }
            PWBCore.SetSavePending();
        }
        #endregion

        #region SELECTION
        public bool isSelected { get; set; }
        public virtual void ToggleSelection() => isSelected = !isSelected;
        #endregion

        #region OBJECT POSES
        [SerializeField]
        private System.Collections.Generic.List<ObjectPose> _poses = new System.Collections.Generic.List<ObjectPose>();
        [SerializeField]
        private System.Collections.Generic.List<ObjectId> _objectIds = new System.Collections.Generic.List<ObjectId>();
        private System.Collections.Generic.List<GameObject> _objects = new System.Collections.Generic.List<GameObject>();


        private void FindObjects()
        {
            if (_objectIds.Count == _objects.Count) return;
            var ids = _objectIds.ToArray();
            _objectIds.Clear();
            _poses.Clear();
            _objects.Clear();
            void AddPose(GameObject obj)
            {
                if (obj == null) return;
                _objectIds.Add(new ObjectId(obj));
                _objects.Add(obj);
                _poses.Add(new ObjectPose(obj));
            }

            var first = ObjectId.FindObject(ids[0]);
            if (first != null)
            {
                var parent = first.transform.parent;
                if (parent != null && parent.childCount == ids.Length)
                {
                    foreach (Transform child in parent) AddPose(child.gameObject);
                    return;
                }
            }
            for (int i = 0; i < ids.Length; ++i)
            {
                var obj = ObjectId.FindObject(ids[i]);
                if (obj == null) continue;
                _objectIds.Add(ids[i]);
                _objects.Add(obj);
                _poses.Add(new ObjectPose(obj));
            }
        }

        public void AddPose(ObjectId objId, ObjectPose pose, bool updateObjectArray = true)
        {
            var obj = ObjectId.FindObject(objId);
            if (obj == null) return;
            _poses.Add(pose);
            _objectIds.Add(objId);
            if (!updateObjectArray) return;
            if (_objectIds.Count + 1 != _objects.Count) FindObjects();
            _objects.Add(obj);
        }
        public void InitializePoses((ObjectId, ObjectPose)[] items)
        {
            _poses.Clear();
            _objectIds.Clear();
            _objects.Clear();
            foreach (var item in items) AddPose(item.Item1, item.Item2);
        }
        public void InsertPose(int index, ObjectPose pose, ObjectId objId, bool updateObjectArray = true)
        {
            var obj = ObjectId.FindObject(objId);
            if (obj == null) return;
            _poses.Insert(index, pose);
            _objectIds.Insert(index, objId);
            if (!updateObjectArray) return;
            if (_objectIds.Count + 1 != _objects.Count) FindObjects();
            _objects.Insert(index, obj);
        }

        public void RemovePose(int index)
        {
            _poses.RemoveAt(index);
            _objectIds.RemoveAt(index);
            if (_objectIds.Count - 1 == _objects.Count) _objects.RemoveAt(index);
        }

        public void RemoveAllPoses()
        {
            _poses.Clear();
            _objectIds.Clear();
            _objects.Clear();
        }

        public void UpdatePoses()
        {
            if (_objectIds.Count != _objects.Count)
            {
                FindObjects();
                return;
            }
            var objCount = _objectIds.Count;
            var ids = _objectIds.ToArray();
            var poses = _poses.ToArray();
            var objs = _objects.ToArray();
            _objectIds.Clear();
            _poses.Clear();
            _objects.Clear();
            for (int i = 0; i < objCount; ++i)
            {
                var obj = objs[i];
                if (obj == null) continue;
                AddPose(new ObjectId(obj), new ObjectPose(obj));
            }
        }

        public void AddObjects((GameObject, int)[] objects)
        {
            for (int i = 0; i < objects.Length; ++i)
            {
                var idx = objects[i].Item2;
                var obj = objects[i].Item1;
                if (idx > -1 && idx < objectCount)
                    InsertPose(idx, new ObjectPose(obj.transform.position, obj.transform.localRotation,
                        obj.transform.localScale), new ObjectId(obj));
                else AddPose(new ObjectId(obj),
                    new ObjectPose(obj.transform.position, obj.transform.localRotation, obj.transform.localScale));
            }
        }

        public bool ReplaceObject(GameObject target, GameObject obj)
        {
            int targetIdx = -1;
            var targetId = new ObjectId(target);
            for (int i = 0; i < _objectIds.Count; ++i)
            {
                var objId = _objectIds[i];
                if (targetId == objId)
                {
                    targetIdx = i;
                    break;
                }
            }
            if (targetIdx == -1) return false;
            InsertPose(targetIdx, new ObjectPose(obj.transform.position, obj.transform.localRotation,
                obj.transform.localScale), new ObjectId(obj), updateObjectArray: true);
            RemovePose(targetIdx + 1);
            return true;
        }

        public int objectCount => _objectIds.Count;

        public bool isEmpty()
        {
            if (_objectIds.Count == 0) return true;
            var allObjectsAreNull = _objects.Count > 0 && !_objects.Exists(i => i != null);
            return allObjectsAreNull;
        }

        public GameObject[] objects
        {
            get
            {
                if (_objectIds.Count != _objects.Count) FindObjects();
                return _objects.ToArray();
            }
        }

        public System.Collections.Generic.HashSet<GameObject> objectSet
        {
            get
            {
                if (_objectIds.Count != _objects.Count) FindObjects();
                return new System.Collections.Generic.HashSet<GameObject>(_objects);
            }
        }

        public System.Collections.Generic.List<GameObject> objectList
        {
            get
            {
                if (_objectIds.Count != _objects.Count) FindObjects();
                return _objects.ToList();
            }
        }

        public void DestroyGameObjects()
        {
            foreach (var obj in objectList)
                if (obj != null)
                    UnityEditor.Undo.DestroyObjectImmediate(obj);
        }

        public virtual void ResetPoses(PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> initialData)
        {
            var initialPoses = initialData._poses;
            if (_objectIds.Count != _objects.Count) FindObjects();
            for (int i = 0; i < objectCount; ++i)
            {
                var obj = _objects[i];
                if (obj == null) obj = ObjectId.FindObject(_objectIds[i]);
                if (obj == null) continue;
                var pose = _poses[i];
                UnityEditor.Undo.RecordObject(obj.transform, RESET_COMMAND_NAME);
                obj.transform.position = pose.position;
                obj.transform.localRotation = pose.localRotation;
                obj.transform.localScale = pose.localScale;
                obj.SetActive(true);
            }
            Copy(initialData);
        }

        public GameObject GetParent()
        {
            var parents = new System.Collections.Generic.List<GameObject>();
            if (_objectIds.Count != _objects.Count) FindObjects();
            var objList = objectList;
            void GetParentList()
            {
                parents.Clear();
                foreach (var obj in objList)
                {
                    if (obj == null || obj.transform.parent == null)
                    {
                        parents.Clear();
                        return;
                    }
                    else
                    {
                        if (parents.Contains(obj.transform.parent.gameObject)) continue;
                        parents.Add(obj.transform.parent.gameObject);
                    }
                }
            }
            do
            {
                GetParentList();
                objList = parents.ToList();
            }
            while (parents.Count > 1);
            if (parents.Count == 0) return null;
            return parents[0];
        }

        #endregion
        #region CONTROL POINTS
        [SerializeField]
        protected System.Collections.Generic.List<CONTROL_POINT> _controlPoints
            = new System.Collections.Generic.List<CONTROL_POINT>();
        private int _selectedPointIdx = -1;
        protected System.Collections.Generic.List<int> _selection = new System.Collections.Generic.List<int>();
        protected Vector3[] _pointPositions = null;
        private static string _commandName = null;
        public const string RESET_COMMAND_NAME = "Reset persistent pose";
        public static string COMMAND_NAME
        {
            get
            {
                if (_commandName == null) _commandName = "Edit " + (new TOOL_NAME()).value;
                return _commandName;
            }
        }
        public Vector3[] points => _pointPositions;
        public int pointsCount => _pointPositions.Length;
        public Vector3 GetPoint(int idx)
        {
            if (idx < 0) idx += _pointPositions.Length;
            return _pointPositions[idx];
        }
        public Vector3 selectedPoint => _pointPositions[_selectedPointIdx];
        public bool ControlPointIsSelected(int idx) => _selection.Contains(idx);
        public CONTROL_POINT[] controlPoints => _controlPoints.ToArray();
        public int selectionCount => _selection.Count;
        public virtual bool SetPoint(int idx, Vector3 value, bool registerUndo, bool selectAll, bool moveSelection = true)
        {
            if (_pointPositions.Length <= 1) Initialize();
            if (idx < 0 || idx >= _pointPositions.Length) return false;
            if (_pointPositions[idx] == value) return false;
            if (registerUndo) ToolProperties.RegisterUndo(COMMAND_NAME);
            var delta = value - _pointPositions[idx];
            _pointPositions[idx] = _controlPoints[idx].position = value;
            var selection = _selection.ToArray();
            if (!moveSelection) return true;
            if (selectAll)
            {
                selection = new int[_controlPoints.Count];
                for (int i = 0; i < selection.Length; ++i) selection[i] = i;
            }
            foreach (var selectedIdx in selection)
            {
                if (selectedIdx == idx) continue;
                _controlPoints[selectedIdx].position += delta;
                _pointPositions[selectedIdx] = _controlPoints[selectedIdx].position;
            }
            return true;
        }

        public void AddDeltaToSelection(Vector3 delta)
        {
            foreach (var selectedIdx in _selection)
            {
                _controlPoints[selectedIdx].position += delta;
                _pointPositions[selectedIdx] = _controlPoints[selectedIdx].position;
            }
        }

        public void AddValue(int idx, Vector3 value)
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            _controlPoints[idx].position += value;
            _pointPositions[idx] = _controlPoints[idx].position;
        }

        protected virtual void UpdatePoints(bool deserializing = false)
            => _pointPositions = ControlPoint.PointArrayToVectorArray(_controlPoints.ToArray());

        public void RemoveSelectedPoints()
        {
            if (_selectedPointIdx == -1)
            {
                _selection.Clear();
                return;
            }
            RemovePoints(_selection.ToArray());
        }

        public void RemovePoint(int idx)
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            if (_controlPoints.Count <= 2)
            {
                Initialize();
                return;
            }
            _controlPoints.RemoveAt(idx);
            if (_selectedPointIdx == idx) _selectedPointIdx = -1;
            RemoveFromSelection(idx);
            UpdatePoints();
        }
        public void RemovePoints(int[] indexes)
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            var toRemove = new System.Collections.Generic.List<int>(indexes);
            toRemove.Sort();
            if (toRemove.Count >= _pointPositions.Length - 1)
            {
                Initialize();
                return;
            }
            for (int i = toRemove.Count - 1; i >= 0; --i) _controlPoints.RemoveAt(toRemove[i]);
            _selectedPointIdx = -1;
            _selection.Clear();
            UpdatePoints();
        }

        public void InsertPoint(int idx, CONTROL_POINT point)
        {
            if (idx < 0) return;
            idx = Mathf.Max(idx, 1);
            ToolProperties.RegisterUndo(COMMAND_NAME);
            _controlPoints.Insert(idx, point);
            UpdatePoints();
        }

        protected void AddPoint(CONTROL_POINT point, bool registerUndo = true)
        {
            if (registerUndo) ToolProperties.RegisterUndo(COMMAND_NAME);
            _controlPoints.Add(point);
            UpdatePoints();
        }
        protected void AddPointRange(System.Collections.Generic.IEnumerable<CONTROL_POINT> collection)
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            _controlPoints.AddRange(collection);
            UpdatePoints();
        }
        protected void PointsRemoveRange(int index, int count)
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            _controlPoints.RemoveRange(index, count);
            UpdatePoints();
        }
        protected CONTROL_POINT[] PointsGetRange(int index, int count) => _controlPoints.GetRange(index, count).ToArray();
        public int selectedPointIdx
        {
            get
            {
                if (_selectedPointIdx >= _pointPositions.Length) ClearSelection();
                return _selectedPointIdx;
            }
            set
            {
                if (_selectedPointIdx == value) return;
                _selectedPointIdx = value;
            }
        }
        public void AddToSelection(int idx)
        {
            if (!_selection.Contains(idx)) _selection.Add(idx);
        }
        public void SelectAll()
        {
            _selection.Clear();
            for (int i = 0; i < pointsCount; ++i) _selection.Add(i);
            if (_selectedPointIdx < 0) _selectedPointIdx = 0;
        }

        public bool AllPointsAreSelected() => _selection.Count == pointsCount;
        public void RemoveFromSelection(int idx)
        {
            if (_selection.Contains(idx)) _selection.Remove(idx);
        }
        public void ClearSelection()
        {
            _selectedPointIdx = -1;
            _selection.Clear();
            isSelected = false;
        }
        public void Reset() => Initialize();

        public Bounds GetBounds(float sizeMultiplier)
        {
            var max = BoundsUtils.MIN_VECTOR3;
            var min = BoundsUtils.MAX_VECTOR3;
            foreach (var point in _controlPoints)
            {
                max = Vector3.Max(max, point);
                min = Vector3.Min(min, point);
            }
            var size = (max - min);
            var center = size / 2 + min;
            size *= sizeMultiplier;
            return new Bounds(center, size);
        }
        #endregion
        #region SETTINGS
        [SerializeField] protected TOOL_SETTINGS _settings = new TOOL_SETTINGS();
        public TOOL_SETTINGS settings { get => _settings; set => _settings = value; }
        #endregion
        #region STATE
        [SerializeField] private ToolManager.ToolState _state = ToolManager.ToolState.NONE;
        public virtual ToolManager.ToolState state
        {
            get => _state;
            set
            {
                if (_state == value) return;
                ToolProperties.RegisterUndo(COMMAND_NAME);
                _state = value;
            }
        }
        #endregion
        #region COMMON
        public string toolName => (new TOOL_NAME()).value;
        protected virtual void Initialize()
        {
            _selectedPointIdx = -1;
            _selection.Clear();
            _state = ToolManager.ToolState.NONE;
            _controlPoints.Clear();
            UpdatePoints();
        }

        [SerializeField] protected long _initialBrushId = -1;

        public PersistentData() => Initialize();
        public PersistentData((GameObject, int)[] objects, long initialBrushId,
            PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> data)
        {
            Copy(data);
            _name = data.name;
            _settings = new TOOL_SETTINGS();
            _settings.Copy(data._settings);
            _id = nextId;
            SetNextId();
            _initialBrushId = initialBrushId;
            _selectedPointIdx = -1;
            _selection.Clear();
            _state = ToolManager.ToolState.PERSISTENT;
            if (objects == null || objects.Length == 0) return;
            _poses = new System.Collections.Generic.List<ObjectPose>();
            _objectIds = new System.Collections.Generic.List<ObjectId>();
            _objects = new System.Collections.Generic.List<GameObject>();
            AddObjects(objects);
        }

        public long initialBrushId => _initialBrushId;
        public void SetInitialBrushId(long value) => _initialBrushId = value;

        protected void Clone(PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> clone)
        {
            if (clone == null) clone = new PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>();
            clone._id = id;
            clone._controlPoints.Clear();
            foreach (var point in _controlPoints)
            {
                var pointClone = new CONTROL_POINT();
                pointClone.Copy(point);
                clone._controlPoints.Add(pointClone);
            }
            clone._pointPositions = _pointPositions == null ? null : _pointPositions.ToArray();
            clone._poses = _poses.ToList();
            clone._objectIds = _objectIds.ToList();
            clone._objects = _objects.ToList();
            clone._initialBrushId = _initialBrushId;
            clone.settings.Copy(_settings);
            clone._selectedPointIdx = -1;
            clone._selection.Clear();
        }
        public virtual void Copy(PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> other)
        {
            _controlPoints.Clear();
            foreach (var point in other._controlPoints)
            {
                var pointClone = new CONTROL_POINT();
                pointClone.Copy(point);
                _controlPoints.Add(pointClone);
            }
            _selectedPointIdx = other._selectedPointIdx;
            _selection = other._selection.ToList();
            _pointPositions = other._pointPositions == null ? null : other._pointPositions.ToArray();

            _settings = other._settings;
            _poses = _poses.ToList();
            _objectIds = _objectIds.ToList();
            _objects = _objects.ToList();
            _initialBrushId = other._initialBrushId;
        }

        public virtual void Duplicate(PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> other)
        {
            _controlPoints.Clear();
            foreach (var point in other._controlPoints)
            {
                var pointClone = new CONTROL_POINT();
                pointClone.Copy(point);
                _controlPoints.Add(pointClone);
            }
            _selectedPointIdx = other._selectedPointIdx;
            _selection = other._selection.ToList();
            _pointPositions = other._pointPositions == null ? null : other._pointPositions.ToArray();

            _settings = other._settings;
            _initialBrushId = other._initialBrushId;

            foreach (var obj in other._objects)
            {
                GameObject clone = null;
                var prefabName = obj.name;
                var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (prefab == null)
                {
                    clone = GameObject.Instantiate(obj);
                    prefabName = obj.name;
                }
                else
                {
                    clone = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                    prefabName = prefab.name;
                }
                clone.transform.position = obj.transform.position;
                clone.transform.rotation = obj.transform.rotation;
                clone.transform.localScale = obj.transform.lossyScale;
                clone.name = prefabName;

                Transform surface = obj.transform.parent;
                while (surface != null)
                {
                    var compCount = surface.gameObject.GetComponents<Component>().Length;
                    if (compCount == 1) surface = surface.parent;
                    else break;
                }

                var settings = other.settings as IPaintToolSettings;
                var parent = PWBIO.GetParent(settings, prefabName,
                    create: true, surface, hexId);

                var commandName = "Duplicate item";
                UnityEditor.Undo.RegisterCreatedObjectUndo(obj, commandName);
                UnityEditor.Undo.SetTransformParent(clone.transform, parent, commandName);


                AddPose(new ObjectId(clone), new ObjectPose(clone));
                PWBIO.AddObjectToOctree(clone);
            }
        }

        private bool _deserializing = false;
        protected bool deserializing { get => _deserializing; set => _deserializing = value; }
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            deserializing = true;
            deserializing = false;
            UpdatePoints(deserializing: true);
            PWBIO.repaint = true;
        }
        #endregion
    }

    [System.Serializable]
    public class SceneData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA>
        where TOOL_NAME : IToolName, new()
        where TOOL_SETTINGS : IToolSettings, new()
        where CONTROL_POINT : ControlPoint, new()
        where TOOL_DATA : PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>, new()
    {
        [SerializeField] private string _sceneGUID = null;
        [SerializeField] private System.Collections.Generic.List<TOOL_DATA> _items = null;

        public string sceneGUID { get => _sceneGUID; set => _sceneGUID = value; }
        public System.Collections.Generic.List<TOOL_DATA> items => _items;

        public SceneData() { }
        public SceneData(string sceneGUID) => _sceneGUID = sceneGUID;

        public void AddItem(TOOL_DATA data)
        {
            if (_items == null) _items = new System.Collections.Generic.List<TOOL_DATA>();
            _items.Add(data);
        }

        public void RemoveItemData(long itemId) => _items.RemoveAll(i => i.id == itemId);

        public void DeleteItemData(long itemId, bool deleteObjects)
        {
            var item = GetItem(itemId);
            if (item == null) return;
            if (deleteObjects) item.DestroyGameObjects();
            RemoveItemData(itemId);
        }
        public TOOL_DATA GetItem(long itemId) => _items.Find(i => i.id == itemId);

        public GameObject[] GetParents(long itemId)
        {
            var parents = new System.Collections.Generic.HashSet<GameObject>();
            var item = GetItem(itemId);
            if (item == null) return parents.ToArray();
            var objs = item.objects;
            foreach (var obj in objs)
            {
                if (obj == null) continue;
                if (obj.transform.parent == null) continue;
                var parent = obj.transform.parent.gameObject;
                if (parents.Contains(parent)) continue;
                parents.Add(parent);
                do
                {
                    if (parent.transform.parent == null) parent = null;
                    else
                    {
                        parent = parent.transform.parent.gameObject;
                        if (!parents.Contains(parent)) parents.Add(parent);
                    }
                }
                while (parent != null);
            }
            return parents.ToArray();
        }
    }
    #endregion
    #endregion
}
