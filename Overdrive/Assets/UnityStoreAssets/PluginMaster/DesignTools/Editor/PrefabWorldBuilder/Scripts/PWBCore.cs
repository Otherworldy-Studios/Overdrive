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
    #region CORE
    public static class PWBCore
    {
        public static bool refreshDatabase { get; set; }
        public static void AssetDatabaseRefresh()
        {
            if (!ApplicationEventHandler.importingPackage)
            {
                if (!DataReimportHandler.importingAssets && !ApplicationEventHandler.sceneOpening)
                    UnityEditor.AssetDatabase.Refresh();
            }
            else ApplicationEventHandler.RefreshOnImportingCancelled();
            refreshDatabase = false;
        }

        public static string iconPath => UnityEditor.EditorGUIUtility.isProSkin ? "Sprites/" : "Sprites/LightTheme/";

        #region DATA
        private static PWBData _staticData = null;
        public static bool staticDataWasInitialized => _staticData != null;
        public static PWBData staticData
        {
            get
            {
                if (_staticData != null) return _staticData;
                _staticData = new PWBData();
                return _staticData;
            }
        }

        public static PWBData GetLoadedStaticData()
        {
            if (_staticData != null) return _staticData;
            LoadFromFile();
            return staticData;
        }

        public static void Initialize()
        {
            if (!_loadedFromFile) LoadFromFile();
        }

        public static bool _loadedFromFile = false;

        public static long _loadTimeSpan = 0;
        public static System.Action OnLoadedFromFile;
        public static void LoadFromFile()
        {
            var now = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
            if (now - _loadTimeSpan < 1000) return;
            _loadTimeSpan = now;
            _loadedFromFile = true;
            var text = PWBData.ReadDataText();
            void CreateFile()
            {
                _staticData = new PWBData();
                _staticData.SaveAndUpdateVersion();
            }
            if (text == null) CreateFile();
            else
            {
                _staticData = null;
                try
                {
                    _staticData = JsonUtility.FromJson<PWBData>(text);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
                if (_staticData == null)
                {
                    CreateFile();
                    return;
                }
                foreach (var palette in PaletteManager.allPalettes)
                    foreach (var brush in palette.brushes)
                        foreach (var item in brush.items) item.InitializeParentSettings(brush);
            }
            if (OnLoadedFromFile != null) OnLoadedFromFile();
        }

        public static void SetSavePending()
        {
            AutoSave.QuickSave();
            staticData.SetSavePending();
        }

        public static string GetRelativePath(string fullPath)
        {
            var fullUri = new System.Uri(fullPath);
            var dataUri = new System.Uri(Application.dataPath);
            return System.Uri.UnescapeDataString(dataUri.MakeRelativeUri(fullUri).ToString());
        }
        public static string GetFullPath(string retalivePath)
             => Application.dataPath.Substring(0, Application.dataPath.Length - 6) + retalivePath;
        
        private static readonly char[] kInvalidPathCharsArray = System.IO.Path.GetInvalidPathChars();
        public static bool IsFullPath(string path)
            => !string.IsNullOrWhiteSpace(path)
            && path.IndexOfAny(kInvalidPathCharsArray) == -1
            && System.IO.Path.IsPathRooted(path)
            && !System.IO.Path.GetPathRoot(path).Equals(System.IO.Path.DirectorySeparatorChar.ToString(),
                System.StringComparison.Ordinal);


        #endregion
        #region TEMP COLLIDERS
        public const string PARENT_COLLIDER_NAME = "PluginMasterPrefabPaintTempMeshColliders";
        private static GameObject _parentCollider = null;
        private static GameObject parentCollider 
        {
            get
            {
                if(PWBIO.isInPrefabMode)
                {
                    var parentCollider = PWBIO.prefabStage.prefabContentsRoot.transform.Find(PARENT_COLLIDER_NAME);
                    if (parentCollider == null)
                    {
                        _parentCollider = new GameObject(PWBCore.PARENT_COLLIDER_NAME);
                        _parentCollider.transform.SetParent(PWBIO.prefabStage.prefabContentsRoot.transform);
                    }
                    else _parentCollider = parentCollider.gameObject;
                    _parentCollider.hideFlags = HideFlags.HideAndDontSave;
                }
                else if (_parentCollider == null)
                {
                    _parentCollider = new GameObject(PARENT_COLLIDER_NAME);
                    _parentColliderId = _parentCollider.GetInstanceID();
                    _parentCollider.hideFlags = HideFlags.HideAndDontSave;
                }
                return _parentCollider;
            }
        }
        private static int _parentColliderId = -1;
        public static int parentColliderId => _parentColliderId;

        private static System.Collections.Generic.Dictionary<int, GameObject> _tempCollidersIds
            = new System.Collections.Generic.Dictionary<int, GameObject>();
        private static System.Collections.Generic.Dictionary<int, GameObject> _tempCollidersTargets
            = new System.Collections.Generic.Dictionary<int, GameObject>();
        private static System.Collections.Generic.HashSet<int> _tempCollidersRootTargets
            = new System.Collections.Generic.HashSet<int>();
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<int>>
            _tempCollidersTargetParentsIds
            = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<int>>();
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<int>>
            _tempCollidersTargetChildrenIds
            = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<int>>();
        public static bool CollidersContains(GameObject[] selection, string colliderName)
        {
            int objId;
            if (!int.TryParse(colliderName, out objId)) return false;
            foreach (var obj in selection)
                if (obj.GetInstanceID() == objId)
                    return true;
            return false;
        }

        public static bool IsTempCollider(int instanceId) => _tempCollidersIds.ContainsKey(instanceId);

        public static GameObject GetGameObjectFromTempColliderId(int instanceId)
        {
            if (!_tempCollidersIds.ContainsKey(instanceId)) return null;
            else if (_tempCollidersIds[instanceId] == null)
            {
                _tempCollidersIds.Remove(instanceId);
                var tempCol = UnityEditor.EditorUtility.InstanceIDToObject(instanceId);
                if (tempCol != null) Object.DestroyImmediate(tempCol);
                return null;
            }
            return _tempCollidersIds[instanceId];
        }

        public static GameObject GetGameObjectFromTempCollider(GameObject source)
        {
            if (source == null) return null;
            if (IsTempCollider(source.GetInstanceID())) return GetGameObjectFromTempColliderId(source.GetInstanceID());
            return source;
        }

        public static bool updatingTempColliders { get; set; }
        public static void UpdateTempColliders(bool force = false)
        {
            updatingTempColliders = true;
            DestroyTempColliders();
            if (!force && ToolManager.tool == ToolManager.PaintTool.GRAVITY 
                && !GravityToolManager.settings.createTempColliders) return;
            bool createWithinFrustrum = ToolManager.tool == ToolManager.PaintTool.GRAVITY
                && GravityToolManager.settings.tempCollidersAction 
                == GravityToolSettings.TempCollidersAction.CREATE_WITHIN_FRUSTRUM;
            if (createWithinFrustrum)
            {
                if (UnityEditor.SceneView.lastActiveSceneView != null)
                {
                    CreateTempCollidersWithinFrustum(UnityEditor.SceneView.lastActiveSceneView.camera);
                    return;
                }
            }
            var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; ++i)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene == null || !scene.IsValid() || !scene.isLoaded) continue;
                GameObject[] rootObjs;
                if (PWBIO.isInPrefabMode) rootObjs = new GameObject[] { PWBIO.prefabStage.prefabContentsRoot };
                else rootObjs = scene.GetRootGameObjects();
                foreach (var rootObj in rootObjs)
                {
                    if (!rootObj.activeInHierarchy) continue;
                    AddTempCollider(rootObj);
                }
            }
        }


        public static void UpdateTempCollidersIfHierarchyChanged()
        {
            if (!ApplicationEventHandler.hierarchyChangedWhileUsingTools) return;
            UpdateTempColliders();
            ApplicationEventHandler.hierarchyChangedWhileUsingTools = false;
        }

        public static void AddTempCollider(GameObject obj, Pose pose)
        {
            var currentPose = new Pose(obj.transform.position, obj.transform.rotation);
            obj.transform.SetPositionAndRotation(pose.position, pose.rotation);
            AddTempCollider(obj);
            obj.transform.SetPositionAndRotation(currentPose.position, currentPose.rotation);
        }

        private static void AddParentsIds(GameObject target)
        {
            var parents = target.GetComponentsInParent<Transform>();
            foreach (var parent in parents)
            {
                if (!_tempCollidersTargetParentsIds.ContainsKey(target.GetInstanceID()))
                    _tempCollidersTargetParentsIds.Add(target.GetInstanceID(), new System.Collections.Generic.HashSet<int>());
                _tempCollidersTargetParentsIds[target.GetInstanceID()].Add(parent.gameObject.GetInstanceID());
                if (!_tempCollidersTargetChildrenIds.ContainsKey(parent.gameObject.GetInstanceID()))
                    _tempCollidersTargetChildrenIds.Add(parent.gameObject.GetInstanceID(),
                        new System.Collections.Generic.HashSet<int>());
                _tempCollidersTargetChildrenIds[parent.gameObject.GetInstanceID()].Add(target.GetInstanceID());
            }
        }

        private static GameObject CreateTempCollider(GameObject target, Mesh mesh)
        {
            if (target == null || mesh == null) return null;
            var differentVertices = new System.Collections.Generic.HashSet<Vector3>();
            foreach (var vertex in mesh.vertices)
            {
                if (!differentVertices.Contains(vertex)) differentVertices.Add(vertex);
                if (differentVertices.Count >= 3) break;
            }
            if (differentVertices.Count < 3) return null;
            if (_tempCollidersTargets.ContainsKey(target.GetInstanceID()))
            {
                if (_tempCollidersTargets[target.GetInstanceID()] != null)
                    return _tempCollidersTargets[target.GetInstanceID()];
                else _tempCollidersTargets.Remove(target.GetInstanceID());
            }
            var name = target.GetInstanceID().ToString();
            var tempObj = new GameObject(name);
            tempObj.hideFlags = HideFlags.HideAndDontSave;
            _tempCollidersIds.Add(tempObj.GetInstanceID(), target);
            tempObj.transform.SetParent(parentCollider.transform);
            tempObj.transform.position = target.transform.position;
            tempObj.transform.rotation = target.transform.rotation;
            tempObj.transform.localScale = target.transform.lossyScale;
            _tempCollidersTargets.Add(target.GetInstanceID(), tempObj);
            AddParentsIds(target);
            MeshUtils.AddCollider(mesh, tempObj);
            return tempObj;
        }
        public static void AddTempCollider(GameObject obj)
        {
            if (!_tempCollidersRootTargets.Add(obj.GetInstanceID())) return;
            var meshFilters = obj.GetComponentsInChildren<MeshFilter>().ToArray();
            foreach (var meshFilter in meshFilters)
            {
                if(meshFilter.gameObject.activeInHierarchy)
                    CreateTempCollider(meshFilter.gameObject, meshFilter.sharedMesh);
            }

            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>().ToArray();
            foreach (var renderer in skinnedMeshRenderers)
            {
                if (renderer.gameObject.activeInHierarchy)
                    CreateTempCollider(renderer.gameObject, renderer.sharedMesh);
            }

            var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();
            foreach (var spriteRenderer in spriteRenderers)
            {
                var target = spriteRenderer.gameObject;
                if (!target.activeInHierarchy) continue;
                if (spriteRenderer.sprite == null) continue;
                if (_tempCollidersTargets.ContainsKey(target.GetInstanceID()))
                {
                    if (_tempCollidersTargets[target.GetInstanceID()] != null) return;
                    else _tempCollidersTargets.Remove(target.GetInstanceID());
                }
                var name = spriteRenderer.gameObject.GetInstanceID().ToString();
                var tempObj = new GameObject(name);
                tempObj.hideFlags = HideFlags.HideAndDontSave;
                _tempCollidersIds.Add(tempObj.GetInstanceID(), spriteRenderer.gameObject);
                tempObj.transform.SetParent(parentCollider.transform);
                tempObj.transform.position = spriteRenderer.transform.position;
                tempObj.transform.rotation = spriteRenderer.transform.rotation;
                tempObj.transform.localScale = spriteRenderer.transform.lossyScale;
                _tempCollidersTargets.Add(target.GetInstanceID(), tempObj);
                AddParentsIds(target);
                var boxCollider = tempObj.AddComponent<BoxCollider>();
                boxCollider.size = (Vector3)(spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit)
                    + new Vector3(0f, 0f, 0.01f);
                var collider = spriteRenderer.GetComponent<Collider2D>();
                if (collider != null && !collider.isTrigger) continue;
                tempObj = new GameObject(name);
                tempObj.hideFlags = HideFlags.HideAndDontSave;
                _tempCollidersIds.Add(tempObj.GetInstanceID(), spriteRenderer.gameObject);
                tempObj.transform.SetParent(parentCollider.transform);
                tempObj.transform.position = spriteRenderer.transform.position;
                tempObj.transform.rotation = spriteRenderer.transform.rotation;
                tempObj.transform.localScale = spriteRenderer.transform.lossyScale;
                var boxCollider2D = tempObj.AddComponent<BoxCollider2D>();
                boxCollider2D.size = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit;
            }
        }


        public static void CreateTempCollidersWithinFrustum(Camera cam)
        {
            var objects = PWBIO.boundsOctree.GetWithinFrustum(cam);
            updatingTempColliders = true;
            foreach (var obj in objects) AddTempCollider(obj);
        }


        public static void DestroyTempCollider(int objId)
        {
            if (_tempCollidersRootTargets.Contains(objId)) _tempCollidersRootTargets.Remove(objId);
            if (!_tempCollidersTargets.ContainsKey(objId)) return;
            var temCollider = _tempCollidersTargets[objId];
            if (temCollider == null)
            {
                _tempCollidersTargets.Remove(objId);
                return;
            }
            var tempId = temCollider.GetInstanceID();
            _tempCollidersIds.Remove(tempId);
            _tempCollidersTargets.Remove(objId);
            _tempCollidersTargetParentsIds.Remove(objId);
            Object.DestroyImmediate(temCollider);
        }
        public static void DestroyTempColliders()
        {
            _tempCollidersIds.Clear();
            _tempCollidersTargets.Clear();
            _tempCollidersRootTargets.Clear();
            _tempCollidersTargetParentsIds.Clear();
            _tempCollidersTargetChildrenIds.Clear();
            var parentObj = GameObject.Find(PWBCore.PARENT_COLLIDER_NAME);
            if (parentObj != null) Object.DestroyImmediate(parentObj);
            _parentColliderId = -1;
        }


        public static void UpdateTempCollidersTransforms(GameObject[] objects)
        {
            foreach (var obj in objects)
            {
                var parentId = obj.GetInstanceID();
                bool isParent = false;
                _tempCollidersRootTargets.Add(parentId);
                foreach (var childId in _tempCollidersTargetParentsIds.Keys)
                {
                    var parentsIds = _tempCollidersTargetParentsIds[childId];
                    if (parentsIds.Contains(parentId))
                    {
                        isParent = true;
                        break;
                    }
                }
                if (!isParent) continue;
                foreach (var id in _tempCollidersTargetChildrenIds[parentId].ToArray())
                {
                    if (!_tempCollidersTargets.ContainsKey(id))
                    {
                        _tempCollidersTargetChildrenIds[parentId].Remove(id);
                        continue;
                    }
                    var tempCollider = _tempCollidersTargets[id];
                    if (tempCollider == null)
                    {
                        _tempCollidersTargetChildrenIds[parentId].Remove(id);
                        _tempCollidersTargets.Remove(id);
                        continue;
                    }
                    var childObj = (GameObject)UnityEditor.EditorUtility.InstanceIDToObject(id);
                    if (childObj == null) continue;
                    tempCollider.transform.position = childObj.transform.position;
                    tempCollider.transform.rotation = childObj.transform.rotation;
                    tempCollider.transform.localScale = childObj.transform.lossyScale;
                }
            }
        }

        public static void SetActiveTempColliders(GameObject[] objects, bool value)
        {
            foreach (var obj in objects)
            {
                if (obj == null) continue;
                if (!obj.activeInHierarchy) continue;
                var parentId = obj.GetInstanceID();
                bool isParent = false;
                foreach (var childId in _tempCollidersTargetParentsIds.Keys)
                {
                    var parentsIds = _tempCollidersTargetParentsIds[childId];
                    if (parentsIds.Contains(parentId))
                    {
                        isParent = true;
                        break;
                    }
                }
                if (!isParent) continue;
                foreach (var id in _tempCollidersTargetChildrenIds[parentId].ToArray())
                {
                    if (!_tempCollidersTargets.ContainsKey(id))
                    {
                        _tempCollidersTargetChildrenIds[parentId].Remove(id);
                        continue;
                    }
                    var tempCollider = _tempCollidersTargets[id];
                    if (tempCollider == null)
                    {
                        _tempCollidersTargetChildrenIds[parentId].Remove(id);
                        _tempCollidersTargets.Remove(id);
                        continue;
                    }
                    var childObj = (GameObject)UnityEditor.EditorUtility.InstanceIDToObject(id);
                    if (childObj == null) continue;
                    tempCollider.SetActive(value);
                    tempCollider.transform.position = childObj.transform.position;
                    tempCollider.transform.rotation = childObj.transform.rotation;
                    tempCollider.transform.localScale = childObj.transform.lossyScale;
                }
            }
        }

        public static GameObject[] GetTempColliders(GameObject obj)
        {
            var parentId = obj.GetInstanceID();
            bool isParent = false;
            foreach (var childId in _tempCollidersTargetParentsIds.Keys)
            {
                var parentsIds = _tempCollidersTargetParentsIds[childId];
                if (parentsIds.Contains(parentId))
                {
                    isParent = true;
                    break;
                }
            }
            if (!isParent) return null;
            var tempColliders = new System.Collections.Generic.List<GameObject>();
            foreach (var id in _tempCollidersTargetChildrenIds[parentId].ToArray())
            {
                if (!_tempCollidersTargets.ContainsKey(id))
                {
                    _tempCollidersTargetChildrenIds[parentId].Remove(id);
                    continue;
                }
                var tempCollider = _tempCollidersTargets[id];
                if (tempCollider == null)
                {
                    _tempCollidersTargetChildrenIds[parentId].Remove(id);
                    _tempCollidersTargets.Remove(id);
                    continue;
                }
                tempColliders.Add(tempCollider);
            }
            return tempColliders.ToArray();
        }
        #endregion

        #region DOCUMENTATION
        private static UnityEngine.Object _documentationPdf = null;
        [UnityEditor.MenuItem("Tools/Plugin Master/Prefab World Builder/Documentation...", false, 1300)]
        public static void OpenDocFile()
        {
            if (_documentationPdf == null)
                _documentationPdf = UnityEditor.AssetDatabase.LoadMainAssetAtPath(PWBCore.staticData.documentationPath);
            if (_documentationPdf == null) Debug.LogWarning("Missing Documentation File");
            else UnityEditor.AssetDatabase.OpenAsset(_documentationPdf);
        }
        #endregion
    }

    [System.Serializable]
    public class PWBData
    {
        public const string DATA_DIR = "Data";
        public const string FILE_NAME = "PWBData";
        public const string FULL_FILE_NAME = FILE_NAME + ".txt";
        public const string RELATIVE_TOOL_DIR = "PluginMaster/DesignTools/Editor/PrefabWorldBuilder";
        public const string RELATIVE_RESOURCES_DIR = RELATIVE_TOOL_DIR + "/Resources";
        public const string RELATIVE_DATA_DIR = RELATIVE_RESOURCES_DIR + "/" + DATA_DIR;
        public const string PALETTES_DIR = "Palettes";
        public const string VERSION = "4.9";
        [SerializeField] private string _version = VERSION;
        [SerializeField] private string _rootDirectory = null;
        [SerializeField] private int _autoSavePeriodMinutes = 1;
        [SerializeField] private bool _undoPalette = true;
        [SerializeField] private int _controlPointSize = 2;
        [SerializeField] private Color _selectedContolPointColor = Color.cyan;
        [SerializeField] private bool _selectedControlPointBlink = false;
        [SerializeField] private bool _closeAllWindowsWhenClosingTheToolbar = false;
        [SerializeField] private bool _openToolPropertiesWhenAToolIsSelected = false;
        [SerializeField] private bool _selectTheNextPaletteInAlphabeticalOrder = true;
        [SerializeField] private int _thumbnailLayer = 7;
        [SerializeField] private bool _openBrushPropertiesWhenABrushIsSelected = false;
        [SerializeField] private int _maxPreviewCountInEditMode = 200;
        [SerializeField] private bool _createThumbnailsFolder = false;
        [SerializeField] private bool _renameItemParent = false;
        [SerializeField] private bool _showInfoText = true;
        [SerializeField] private bool _useAssetPreview = false;
        [SerializeField]
        private SerializableDictionary<string, int> _prefabCountDict
            = new SerializableDictionary<string, int>();
        [SerializeField] private bool _addEnumerationToName = true;
        public enum UnsavedChangesAction { ASK, SAVE, DISCARD }
        [SerializeField] private UnsavedChangesAction _unsavedChangesAction = UnsavedChangesAction.ASK;

        [SerializeField] private PaletteManager _paletteManager = PaletteManager.instance;

        [SerializeField] private FloorManager _floorManager = FloorManager.instance as FloorManager;
        [SerializeField] private WallManager _wallManager = WallManager.instance as WallManager;

        [SerializeField] private PinManager _pinManager = PinManager.instance as PinManager;
        [SerializeField] private BrushManager _brushManager = BrushManager.instance as BrushManager;
        [SerializeField] private GravityToolManager _gravityToolManager = GravityToolManager.instance as GravityToolManager;
        [SerializeField] private LineManager _lineManager = LineManager.instance as LineManager;
        [SerializeField] private ShapeManager _shapeManager = ShapeManager.instance as ShapeManager;
        [SerializeField] private TilingManager _tilingManager = TilingManager.instance as TilingManager;
        [SerializeField] private ReplacerManager _replacerManager = ReplacerManager.instance as ReplacerManager;
        [SerializeField] private EraserManager _eraserManager = EraserManager.instance as EraserManager;

        [SerializeField]
        private SelectionToolManager _selectionToolManager = SelectionToolManager.instance as SelectionToolManager;
        [SerializeField]
        private CircleSelectManager _circleSelectToolManager = CircleSelectManager.instance as CircleSelectManager;
        [SerializeField] private ExtrudeManager _extrudeSettings = ExtrudeManager.instance as ExtrudeManager;
        [SerializeField] private MirrorManager _mirrorManager = MirrorManager.instance as MirrorManager;

        [SerializeField] private SnapManager _snapManager = new SnapManager();

        [SerializeField] private ToolParentingSettings _globalParentingSettings = new ToolParentingSettings();
         
        public ToolParentingSettings globalParentingSettings => _globalParentingSettings;
        private bool _savePending = false;
        private bool _saving = false;

        public static string palettesDirectory
        {
            get
            {
                var dir = PWBSettings.fullDataDir + "/" + PALETTES_DIR;
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string dataPath => PWBSettings.fullDataDir + "/" + FULL_FILE_NAME;

        public string version => _version;
        public int autoSavePeriodMinutes
        {
            get => _autoSavePeriodMinutes;
            set
            {
                value = Mathf.Clamp(value, 1, 10);
                if (_autoSavePeriodMinutes == value) return;
                _autoSavePeriodMinutes = value;
                SaveAndUpdateVersion();
            }
        }

        public bool undoPalette
        {
            get => _undoPalette;
            set
            {
                if (_undoPalette == value) return;
                _undoPalette = value;
                SaveAndUpdateVersion();
            }
        }

        public bool ranameItemParent
        {
            get => _renameItemParent;
            set
            {
                if (_renameItemParent == value) return;
                _renameItemParent = value;
                SaveAndUpdateVersion();
            }
        }

        public int controPointSize
        {
            get => _controlPointSize;
            set
            {
                if (_controlPointSize == value) return;
                _controlPointSize = value;
                SaveAndUpdateVersion();
            }
        }

        public Color selectedContolPointColor
        {
            get => _selectedContolPointColor;
            set
            {
                if (_selectedContolPointColor == value) return;
                _selectedContolPointColor = value;
                SaveAndUpdateVersion();
            }
        }
        public bool selectedControlPointBlink
        {
            get => _selectedControlPointBlink;
            set
            {
                if (_selectedControlPointBlink == value) return;
                _selectedControlPointBlink = value;
                SaveAndUpdateVersion();
            }
        }
        public bool closeAllWindowsWhenClosingTheToolbar
        {
            get => _closeAllWindowsWhenClosingTheToolbar;
            set
            {
                if (_closeAllWindowsWhenClosingTheToolbar == value) return;
                _closeAllWindowsWhenClosingTheToolbar = value;
                SaveAndUpdateVersion();
            }
        }
        public bool openToolPropertiesWhenAToolIsSelected
        {
            get => _openToolPropertiesWhenAToolIsSelected;
            set
            {
                if (_openToolPropertiesWhenAToolIsSelected == value) return;
                _openToolPropertiesWhenAToolIsSelected = value;
                SaveAndUpdateVersion();
            }
        }
        public bool selectTheNextPaletteInAlphabeticalOrder
        {
            get => _selectTheNextPaletteInAlphabeticalOrder;
            set
            {
                if (_selectTheNextPaletteInAlphabeticalOrder == value) return;
                _selectTheNextPaletteInAlphabeticalOrder = value;
                SaveAndUpdateVersion();
            }
        }

        public int thumbnailLayer
        {
            get => _thumbnailLayer;
            set
            {
                value = Mathf.Clamp(value, 0, 31);
                if (_thumbnailLayer == value) return;
                _thumbnailLayer = value;
                SaveAndUpdateVersion();
            }
        }

        public bool createThumbnailsFolder
        {
            get => _createThumbnailsFolder;
            set
            {
                if (_createThumbnailsFolder == value) return;
                _createThumbnailsFolder = value;
                var thumbnailsFolderName = "Thumbnails";
                if (palettesDirectory.Contains(Application.dataPath))
                {
                    var palettesFolder = palettesDirectory.Substring(Application.dataPath.Length - 6);
                    var thumbnailsFolderPath = palettesFolder + "/" + thumbnailsFolderName;
                    if (_createThumbnailsFolder)
                    {
                        if (!UnityEditor.AssetDatabase.IsValidFolder(thumbnailsFolderPath))
                            UnityEditor.AssetDatabase.CreateFolder(palettesFolder, thumbnailsFolderName);
                        var palettesFolders = UnityEditor.AssetDatabase.GetSubFolders(palettesFolder);
                        foreach (var paletteFolder in palettesFolders)
                        {
                            var folderName = System.IO.Path.GetFileName(paletteFolder);
                            if (folderName == thumbnailsFolderName) continue;
                            var destinationPath = thumbnailsFolderPath + "/" + folderName;
                            UnityEditor.AssetDatabase.MoveAsset(paletteFolder, destinationPath);
                        }
                    }
                    else if (UnityEditor.AssetDatabase.IsValidFolder(thumbnailsFolderPath))
                    {
                        var thumbnailsFolders = UnityEditor.AssetDatabase.GetSubFolders(thumbnailsFolderPath);
                        foreach (var thumbnailFolder in thumbnailsFolders)
                        {
                            var folderName = System.IO.Path.GetFileName(thumbnailFolder);
                            var destinationPath = palettesFolder + "/" + folderName;
                            UnityEditor.AssetDatabase.MoveAsset(thumbnailFolder, destinationPath);
                        }
                        UnityEditor.AssetDatabase.DeleteAsset(thumbnailsFolderPath);
                    }
                }
                else
                {
                    var thumbnailsFolderPath = palettesDirectory + "/" + thumbnailsFolderName;
                    if (_createThumbnailsFolder)
                    {
                        if (!System.IO.Directory.Exists(thumbnailsFolderPath))
                            System.IO.Directory.CreateDirectory(thumbnailsFolderPath);
                        var palettesFolders = System.IO.Directory.GetDirectories(palettesDirectory);
                        foreach (var paletteFolder in palettesFolders)
                        {
                            var folderName = System.IO.Path.GetFileName(paletteFolder);
                            if (folderName == thumbnailsFolderName) continue;
                            var destinationPath = thumbnailsFolderPath + "/" + folderName;
                            System.IO.Directory.Move(paletteFolder, destinationPath);
                        }
                    }
                    else if (System.IO.Directory.Exists(thumbnailsFolderPath))
                    {
                        var thumbnailsFolders = System.IO.Directory.GetDirectories(thumbnailsFolderPath);
                        foreach (var thumbnailFolder in thumbnailsFolders)
                        {
                            var folderName = System.IO.Path.GetFileName(thumbnailFolder);
                            var destinationPath = palettesDirectory + "/" + folderName;
                            System.IO.Directory.Move(thumbnailFolder, destinationPath);
                        }
                        System.IO.Directory.Delete(thumbnailsFolderPath);
                    }
                }
                SaveAndUpdateVersion();
            }
        }
        public bool openBrushPropertiesWhenABrushIsSelected
        {
            get => _openBrushPropertiesWhenABrushIsSelected;
            set
            {
                if (_openBrushPropertiesWhenABrushIsSelected == value) return;
                _openBrushPropertiesWhenABrushIsSelected = value;
                SaveAndUpdateVersion();
            }
        }
        public int maxPreviewCountInEditMode
        {
            get => _maxPreviewCountInEditMode;
            set
            {
                value = Mathf.Max(value, 0);
                if (_maxPreviewCountInEditMode == value) return;
                _maxPreviewCountInEditMode = value;
                SaveAndUpdateVersion();
            }
        }

        public UnsavedChangesAction unsavedChangesAction
        {
            get => _unsavedChangesAction;
            set
            {
                if (_unsavedChangesAction == value) return;
                _unsavedChangesAction = value;
                SaveAndUpdateVersion();
            }
        }

        public bool showInfoText
        {
            get => _showInfoText;
            set
            {
                if (_showInfoText == value) return;
                _showInfoText = value;
                SaveAndUpdateVersion();
            }
        }
        public bool useAssetPreview
        {
            get => _useAssetPreview;
            set
            {
                if (_useAssetPreview == value) return;
                _useAssetPreview = value;
                PaletteManager.UpdateAllThumbnails();
                SaveAndUpdateVersion();
            }
        }
        public void ToggleInfoText() => showInfoText = !showInfoText;
        public void SetSavePending() { _savePending = true; }
        public bool saving => _saving;
        public bool VersionUpdate()
        {
            string currentText = null;
            PWBDataVersion dataVersion = null;

            bool ReadTextAndGetVersion()
            {
                currentText = ReadDataText();
                if (currentText == null) return false;
                try
                {
                    dataVersion = JsonUtility.FromJson<PWBDataVersion>(currentText);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
                if (dataVersion == null)
                {
                    DeleteFile();
                    return false;
                }
                return true;
            }

            if (!ReadTextAndGetVersion()) return false;
            var updated = false;
            if (dataVersion.IsOlderThan("2.9"))
            {
                var v2_8_data = JsonUtility.FromJson<V2_8_PWBData>(currentText);
                if (v2_8_data._paletteManager._paletteData.Length > 0) PaletteManager.ClearPaletteList();
                foreach (var paletteData in v2_8_data._paletteManager._paletteData)
                {
                    paletteData.version = VERSION;
                    PaletteManager.AddPalette(paletteData, save: true);
                }
                var textAssets = Resources.LoadAll<TextAsset>(FILE_NAME);
                for (int i = 0; i < textAssets.Length; ++i)
                {
                    var assetPath = UnityEditor.AssetDatabase.GetAssetPath(textAssets[i]);
                    UnityEditor.AssetDatabase.DeleteAsset(assetPath);
                }
                PWBCore.staticData.Save(updateVersion: false);
                PrefabPalette.RepaintWindow();
                updated = true;
            }
            if (updated) if (!ReadTextAndGetVersion()) return false;
            if (dataVersion.IsOlderThan("4.1"))
            {
                var v4_0_data = JsonUtility.FromJson<V4_0_PWBData>(currentText);
                var v4_0_lineSceneItems = v4_0_data._lineManager._sceneItems;
                foreach (var v4_0_sceneItem in v4_0_lineSceneItems)
                {
                    var v4_0_items = v4_0_sceneItem._items;
                    foreach (var v4_0_item in v4_0_items)
                    {
                        var data = LineManager.instance.GetItem(v4_0_item._id);
                        if (data == null) continue;
                        var v4_0_poses = v4_0_item._objectPoses;
                        data.RemoveAllPoses();
                        foreach (var v4_0_pose in v4_0_poses)
                        {
                            var pose = new ObjectPose(v4_0_pose._position, v4_0_pose._localRotation, v4_0_pose._localScale);
                            data.AddPose(v4_0_pose._id, pose);
                        }
                    }
                }
                var v4_0_shapeSceneItems = v4_0_data._shapeManager._sceneItems;
                foreach (var v4_0_sceneItem in v4_0_shapeSceneItems)
                {
                    var v4_0_items = v4_0_sceneItem._items;
                    foreach (var v4_0_item in v4_0_items)
                    {
                        var data = ShapeManager.instance.GetItem(v4_0_item._id);
                        if (data == null) continue;
                        var v4_0_poses = v4_0_item._objectPoses;
                        data.RemoveAllPoses();
                        foreach (var v4_0_pose in v4_0_poses)
                        {
                            var pose = new ObjectPose(v4_0_pose._position, v4_0_pose._localRotation, v4_0_pose._localScale);
                            data.AddPose(v4_0_pose._id, pose);
                        }
                    }
                }
                var v4_0_tilingSceneItems = v4_0_data._tilingManager._sceneItems;
                foreach (var v4_0_sceneItem in v4_0_tilingSceneItems)
                {
                    var v4_0_items = v4_0_sceneItem._items;
                    foreach (var v4_0_item in v4_0_items)
                    {
                        var data = TilingManager.instance.GetItem(v4_0_item._id);
                        if (data == null) continue;
                        var v4_0_poses = v4_0_item._objectPoses;
                        data.RemoveAllPoses();
                        foreach (var v4_0_pose in v4_0_poses)
                        {
                            var pose = new ObjectPose(v4_0_pose._position, v4_0_pose._localRotation, v4_0_pose._localScale);
                            data.AddPose(v4_0_pose._id, pose);
                        }
                    }
                }
                PWBCore.staticData.Save(updateVersion: false);
                updated = true;
            }
            if (dataVersion.IsOlderThan(VERSION)) updated = true;
            if (updated)
            {
                PWBCore.AssetDatabaseRefresh();
                PaletteManager.instance.LoadPaletteFiles(deleteUnusedThumbnails: true);
            }

            return updated;
        }

        public void UpdateRootDirectory()
        {
            var directories = System.IO.Directory.GetDirectories(Application.dataPath, "PrefabWorldBuilder",
               System.IO.SearchOption.AllDirectories).Where(d => d.Replace("\\", "/").Contains(RELATIVE_TOOL_DIR)).ToArray();
            if (directories.Length == 0)
            {
                _rootDirectory = Application.dataPath + "/" + RELATIVE_TOOL_DIR;
                _rootDirectory = _rootDirectory.Replace("\\", "/");
                System.IO.Directory.CreateDirectory(_rootDirectory);
            }
            else _rootDirectory = directories[0];
            _rootDirectory = PWBCore.GetRelativePath(_rootDirectory);
        }

        private string rootDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_rootDirectory)) UpdateRootDirectory();
                else
                {
                    var fullPath = PWBCore.GetFullPath(_rootDirectory);
                    if (!System.IO.Directory.Exists(fullPath)) UpdateRootDirectory();
                }
                return _rootDirectory;
            }
        }
        public void Save() => Save(false);
        public void SaveAndUpdateVersion() => Save(true);
        public void Save(bool updateVersion)
        {
            _saving = true;
            if (updateVersion) VersionUpdate();
            _version = VERSION;
            var jsonString = JsonUtility.ToJson(this, true);
            var fileExist = System.IO.File.Exists(dataPath);
            if (!System.IO.Directory.Exists(PWBSettings.fullDataDir))
                System.IO.Directory.CreateDirectory(PWBSettings.fullDataDir);
            System.IO.File.WriteAllText(dataPath, jsonString);
            if (!fileExist) PWBCore.AssetDatabaseRefresh();
            _savePending = false;
            _saving = false;
        }

        public static string ReadDataText()
        {
            var fullFilePath = dataPath;
            if (!System.IO.File.Exists(fullFilePath)) PWBCore.staticData.Save(false);
            return System.IO.File.ReadAllText(fullFilePath);
        }

        public static void DeleteFile()
        {
            var fullFilePath = dataPath;
            if (System.IO.File.Exists(fullFilePath)) System.IO.File.Delete(fullFilePath);
            var metaPath = fullFilePath += ".meta";
            if (System.IO.File.Exists(metaPath)) System.IO.File.Delete(metaPath);
        }
        public void SaveIfPending() { if (_savePending) SaveAndUpdateVersion(); }

        public string documentationPath => rootDirectory + "/Documentation/Prefab World Builder Documentation.pdf";

        public string GetPrefabCount(string prefabGuid)
        {
            int count = 1;
            if (_prefabCountDict.ContainsKey(prefabGuid))
            {
                count = _prefabCountDict[prefabGuid] + 1;
                _prefabCountDict[prefabGuid] = count;
            }
            else _prefabCountDict.Add(prefabGuid, 1);
            PWBCore.SetSavePending();
            return count.ToString();
        }

        public bool addEnumerationToName
        {
            get => _addEnumerationToName;
            set
            {
                if (_addEnumerationToName == value) return;
                _addEnumerationToName = value;
                SaveAndUpdateVersion();
            }
        }
    }
    #endregion

    #region SHORTCUTS
    #region COMBINATION CLASSES
    [System.Serializable]
    public class PWBShortcutCombination : System.IEquatable<PWBShortcutCombination>
    {
        [SerializeField] protected EventModifiers _modifiers = EventModifiers.None;
        public virtual EventModifiers modifiers => _modifiers;
        public bool control => (modifiers & EventModifiers.Control) != 0;
        public bool alt => (modifiers & EventModifiers.Alt) != 0;
        public bool shift => (modifiers & EventModifiers.Shift) != 0;
        public static EventModifiers FilterModifiers(EventModifiers modifiers)
            => modifiers & (EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        public PWBShortcutCombination(EventModifiers modifiers) => _modifiers = FilterModifiers(modifiers);

        public virtual bool Check(PWBShortcut.Group group = PWBShortcut.Group.NONE, bool ignoreMousePressed = true)
        {
            if (Event.current == null) return false;
            var currentModifiers = FilterModifiers(Event.current.modifiers);
            return currentModifiers == modifiers;
        }

        public bool Equals(PWBShortcutCombination other)
        {
            if (other == null) return false;
            return modifiers == other.modifiers;
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (!(other is PWBShortcutCombination otherCombination)) return false;
            return Equals(otherCombination);
        }
        public override int GetHashCode()
        {
            int hashCode = 822824530;
            hashCode = hashCode * -1521134295 + modifiers.GetHashCode();
            return hashCode;
        }
        public static bool operator ==(PWBShortcutCombination lhs, PWBShortcutCombination rhs)
        {
            if ((object)lhs == null && (object)rhs == null) return true;
            if ((object)lhs == null || (object)rhs == null) return false;
            return lhs.Equals(rhs);
        }
        public static bool operator !=(PWBShortcutCombination lhs, PWBShortcutCombination rhs) => !(lhs == rhs);
        public override string ToString()
        {
            var result = string.Empty;
            if (control) result = "Ctrl";
            if (alt) result += (result == string.Empty ? "Alt" : "+Alt");
            if (shift) result += (result == string.Empty ? "Shift" : "+Shift");
            if (result != string.Empty) result += "+";
            return result;
        }
    }
    [System.Serializable]
    public class PWBKeyCombination : PWBShortcutCombination, System.IEquatable<PWBKeyCombination>
    {
        [SerializeField] private KeyCode _keyCode = KeyCode.None;
        public virtual KeyCode keyCode => _keyCode;

        public void Set(KeyCode keyCode, EventModifiers modifiers = EventModifiers.None)
        {
            _keyCode = keyCode;
            _modifiers = FilterModifiers(modifiers);
        }
        public PWBKeyCombination(KeyCode keyCode, EventModifiers modifiers = EventModifiers.None) : base(modifiers)
            => _keyCode = keyCode;

        public PWBKeyCombination() : base(EventModifiers.None) { }
        public bool Equals(PWBKeyCombination other)
        {
            if (other == null) return false;
            return ToString() == other.ToString();
        }
        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (!(other is PWBKeyCombination otherCombination)) return false;
            return ToString() == otherCombination.ToString();
        }
        public override int GetHashCode()
        {
            int hashCode = 822824530;
            hashCode = hashCode * -1521134295 + _modifiers.GetHashCode();
            hashCode = hashCode * -1521134295 + keyCode.GetHashCode();
            return hashCode;
        }
        public static bool operator ==(PWBKeyCombination lhs, PWBKeyCombination rhs)
        {
            if ((object)lhs == null && (object)rhs == null) return true;
            if ((object)lhs == null || (object)rhs == null) return false;
            return lhs.ToString() == rhs.ToString();
        }
        public static bool operator !=(PWBKeyCombination lhs, PWBKeyCombination rhs) => !(lhs == rhs);

        public override string ToString()
        {
            var result = string.Empty;
            if (keyCode == KeyCode.None) return "Disabled";
            result += base.ToString() + keyCode.ToString().Replace("Alpha", "");
            return result;
        }

        public override bool Check(PWBShortcut.Group group = PWBShortcut.Group.NONE, bool ignoreMousePressed = false)
        {
            if (!ignoreMousePressed && PWBIO.mousePressed) return false;
            if (keyCode == KeyCode.None) return false;
            if (Event.current.type != EventType.KeyDown || Event.current.keyCode != keyCode) return false;
            return base.Check(group, ignoreMousePressed);
        }
    }

    [System.Serializable]
    public class PWBKeyCombinationUSM : PWBKeyCombination
    {
        private string _shortcutId = null;

        public PWBKeyCombinationUSM(string shortcutId)
            : base(KeyCode.None, EventModifiers.None) => _shortcutId = shortcutId;

        public override KeyCode keyCode
        {
            get
            {
                var keyCombinationSequence = UnityEditor.ShortcutManagement.ShortcutManager.instance
                .GetShortcutBinding(_shortcutId).keyCombinationSequence;
                if (keyCombinationSequence.Count() == 0) return KeyCode.None;
                return keyCombinationSequence.First().keyCode;
            }
        }
        public override EventModifiers modifiers
        {
            get
            {
                var keyCombinationSequence = UnityEditor.ShortcutManagement.ShortcutManager.instance
                    .GetShortcutBinding(_shortcutId).keyCombinationSequence;

                if (keyCombinationSequence.Count() == 0) return EventModifiers.None;

                var mods = keyCombinationSequence.First().modifiers;
                var result = EventModifiers.None;
                if ((mods & UnityEditor.ShortcutManagement.ShortcutModifiers.Action)
                    == UnityEditor.ShortcutManagement.ShortcutModifiers.Action) result |= EventModifiers.Control;
                if ((mods & UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)
                    == UnityEditor.ShortcutManagement.ShortcutModifiers.Alt) result |= EventModifiers.Alt;
                if ((mods & UnityEditor.ShortcutManagement.ShortcutModifiers.Shift)
                    == UnityEditor.ShortcutManagement.ShortcutModifiers.Shift) result |= EventModifiers.Shift;
                return result;
            }
        }
        public void Rebind(KeyCode keyCode, EventModifiers modifiers)
        {
            var mods = UnityEditor.ShortcutManagement.ShortcutModifiers.None;
            if ((modifiers & EventModifiers.Control) == EventModifiers.Control)
                mods |= UnityEditor.ShortcutManagement.ShortcutModifiers.Action;
            if ((modifiers & EventModifiers.Alt) == EventModifiers.Alt)
                mods |= UnityEditor.ShortcutManagement.ShortcutModifiers.Alt;
            if ((modifiers & EventModifiers.Shift) == EventModifiers.Shift)
                mods |= UnityEditor.ShortcutManagement.ShortcutModifiers.Shift;
            UnityEditor.ShortcutManagement.ShortcutManager.instance.RebindShortcut(_shortcutId,
                new UnityEditor.ShortcutManagement.ShortcutBinding(
                    new UnityEditor.ShortcutManagement.KeyCombination(keyCode, mods)));
        }

        public void Reset()
        {
            UnityEditor.ShortcutManagement.ShortcutManager.instance.ClearShortcutOverride(_shortcutId);
        }
    }

    [System.Serializable]
    public class PWBHoldKeysAndMouseCombination : PWBKeyCombination
    {
        private bool _holdingKeys = false;
        public bool holdingKeys => _holdingKeys;
        private bool _holdingChanged = false;
        public bool holdingChanged => _holdingChanged;
        public enum MouseEvent { MOVE, CLICK }
        private MouseEvent _mouseEvent = MouseEvent.CLICK;
        public MouseEvent mouseEvent => _mouseEvent;
        public PWBHoldKeysAndMouseCombination(KeyCode keyCode, EventModifiers modifiers, MouseEvent mouseEvent)
            : base(keyCode, modifiers) => _mouseEvent = mouseEvent;
        public PWBHoldKeysAndMouseCombination() : base() { }
        public override bool Check(PWBShortcut.Group group = PWBShortcut.Group.NONE, bool ignoreMousePressed = true)
        {
            CheckIsHoldingKeys(group, ignoreMousePressed);
            return _holdingKeys && Event.current.button == 0
                && Event.current.type == (_mouseEvent == MouseEvent.CLICK ? EventType.MouseDown : EventType.MouseMove);
        }
        public bool CheckIsHoldingKeys(PWBShortcut.Group group = PWBShortcut.Group.NONE, bool ignoreMousePressed = true)
        {
            _holdingChanged = false;
            if (Event.current.keyCode == keyCode)
            {
                var prevHolding = _holdingKeys;
                if (Event.current.type == EventType.KeyDown && base.Check(group, ignoreMousePressed)) _holdingKeys = true;
                else if (Event.current.type == EventType.KeyUp) _holdingKeys = false;
                _holdingChanged = prevHolding != _holdingKeys;
            }
            return _holdingKeys;
        }
        public override string ToString()
        {
            var result = base.ToString();
            if (keyCode != KeyCode.None)
                result = "Hold " + result + " + " + (_mouseEvent == MouseEvent.CLICK ? "Click" : "Mouse move");
            return result;
        }
    }

    [System.Serializable]
    public class PWBMouseCombination : PWBShortcutCombination, System.IEquatable<PWBMouseCombination>
    {
        public enum MouseEvents
        {
            NONE,
            SCROLL_WHEEL,
            DRAG_R_H,
            DRAG_R_V,
            DRAG_M_H,
            DRAG_M_V
        }

        [SerializeField] private MouseEvents _mouseEvent = MouseEvents.NONE;

        public MouseEvents mouseEvent => _mouseEvent;
        public void Set(EventModifiers modifiers, MouseEvents mouseEvent)
        {
            _modifiers = FilterModifiers(modifiers);
            _mouseEvent = mouseEvent;
        }

        public PWBMouseCombination(EventModifiers modifiers, MouseEvents mouseEvent) : base(modifiers)
        => _mouseEvent = mouseEvent;
        public bool Equals(PWBMouseCombination other)
        {
            if (other == null) return false;
            return _mouseEvent == other._mouseEvent && _modifiers == other._modifiers;
        }
        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (!(other is PWBMouseCombination otherCombination)) return false;
            return Equals(otherCombination);
        }

        public override int GetHashCode()
        {
            int hashCode = 1068782991;
            hashCode = hashCode * -1521134295 + _modifiers.GetHashCode();
            hashCode = hashCode * -1521134295 + _mouseEvent.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(PWBMouseCombination lhs, PWBMouseCombination rhs)
        {
            if ((object)lhs == null && (object)rhs == null) return true;
            if ((object)lhs == null || (object)rhs == null) return false;
            return lhs.Equals(rhs);
        }
        public static bool operator !=(PWBMouseCombination lhs, PWBMouseCombination rhs) => !(lhs == rhs);

        public bool isRDrag => mouseEvent == MouseEvents.DRAG_R_H || mouseEvent == MouseEvents.DRAG_R_V;
        public bool isMDrag => mouseEvent == MouseEvents.DRAG_M_H || mouseEvent == MouseEvents.DRAG_M_V;
        public bool isMouseDragEvent => mouseEvent == MouseEvents.DRAG_R_H || mouseEvent == MouseEvents.DRAG_R_V
                || mouseEvent == MouseEvents.DRAG_M_H || mouseEvent == MouseEvents.DRAG_M_V;
        public bool isHorizontalDragEvent => mouseEvent == MouseEvents.DRAG_R_H || mouseEvent == MouseEvents.DRAG_M_H;
        private float _delta = 0;
        public float delta => _delta;

        public override bool Check(PWBShortcut.Group group = PWBShortcut.Group.NONE, bool ignoreMousePressed = true)
        {
            if (mouseEvent == MouseEvents.NONE) return false;
            if (FilterModifiers(Event.current.modifiers) == EventModifiers.None) return false;
            if (!base.Check(group, ignoreMousePressed)) return false;
            if (isMouseDragEvent)
            {
                _delta = isHorizontalDragEvent ? Event.current.delta.x : -Event.current.delta.y;
                if (Event.current.type != EventType.MouseDrag) return false;
                if (isRDrag && Event.current.button != 1)
                    return false;
                if (isMDrag && Event.current.button != 2) return false;

                var xIsGreaterThanY = Mathf.Abs(Event.current.delta.x) > Mathf.Abs(Event.current.delta.y);
                if (isHorizontalDragEvent && !xIsGreaterThanY)
                {
                    var other = new PWBMouseCombination(base.modifiers,
                        mouseEvent == MouseEvents.DRAG_R_H ? MouseEvents.DRAG_R_V : MouseEvents.DRAG_M_V);
                    if (!PWBSettings.shortcuts.CombinationExist(other, group)) Event.current.Use();
                    return false;
                }
                if (!isHorizontalDragEvent && xIsGreaterThanY)
                {
                    var other = new PWBMouseCombination(base.modifiers,
                        mouseEvent == MouseEvents.DRAG_R_V ? MouseEvents.DRAG_R_H : MouseEvents.DRAG_M_H);
                    if (!PWBSettings.shortcuts.CombinationExist(other, group)) Event.current.Use();
                    return false;
                }
            }
            if (mouseEvent == MouseEvents.SCROLL_WHEEL)
            {
                if (!Event.current.isScrollWheel) return false;
                _delta = (Mathf.Abs(Event.current.delta.x) > Mathf.Abs(Event.current.delta.y))
                    ? Event.current.delta.x : Event.current.delta.y;
            }
            Event.current.Use();
            return true;
        }

        public override string ToString()
        {
            var result = string.Empty;
            if (mouseEvent == MouseEvents.NONE) return "Disabled";
            var mouseEventString = string.Empty;
            switch (mouseEvent)
            {
                case MouseEvents.SCROLL_WHEEL:
                    mouseEventString = "Scroll wheel";
                    break;
                case MouseEvents.DRAG_R_H:
                    mouseEventString = "Mouse R Btn H drag";
                    break;
                case MouseEvents.DRAG_R_V:
                    mouseEventString = "Mouse R Btn V drag";
                    break;
                case MouseEvents.DRAG_M_H:
                    mouseEventString = "Mouse Mid Btn H drag";
                    break;
                case MouseEvents.DRAG_M_V:
                    mouseEventString = "Mouse Mid Btn V drag";
                    break;
            }
            result += base.ToString() + mouseEventString;
            return result;
        }
    }
    #endregion

    #region SHORTCUT CLASSES
    [System.Serializable]
    public class PWBShortcut
    {
        public enum Group
        {
            NONE = 0,
            GLOBAL = 1,
            GRID = 2,
            PIN = 4,
            BRUSH = 8,
            GRAVITY = 16,
            LINE = 32,
            SHAPE = 64,
            TILING = 128,
            ERASER = 256,
            REPLACER = 512,
            SELECTION = 1024,
            PALETTE = 2048,
            CIRCLE_SELECT = 4096,
            EXTRUDE = 8192,
            MIRROR = 16384,
            FLOOR = 32768,
            WALL = 65536
        }
        [SerializeField] private string _name = null;
        [SerializeField] private Group _group = Group.NONE;
        [SerializeField] private bool _conflicted = false;

        public PWBShortcut(string name, Group group)
        {
            _name = name;
            _group = group;
        }

        public string name => _name;

        public Group group => _group;
        protected void SetGroup(Group value) => _group = value;

        public bool conflicted { get => _conflicted; set => _conflicted = value; }
        public virtual void Copy(PWBShortcut other)
        {
            if (other == null) return;
            _name = other._name;
            _group = other._group;
        }
    }

    [System.Serializable]
    public class PWBKeyShortcut : PWBShortcut
    {
        [SerializeField]
        protected PWBKeyCombination _keyCombination = null;

        public PWBKeyShortcut(string name, Group group, KeyCode keyCode, EventModifiers modifiers = EventModifiers.None)
            : base(name, group) => combination.Set(keyCode, modifiers);
        public PWBKeyShortcut(string name, Group group, PWBKeyCombination keyCombination) : base(name, group)
            => _keyCombination = keyCombination;

        public virtual PWBKeyCombination combination
        {
            get
            {
                if (_keyCombination == null) _keyCombination = new PWBKeyCombination();
                return _keyCombination;
            }
        }
        public bool Check()
        {
            if (PWBIO.gridShorcutEnabled && group != Group.GRID) return false;
            return combination.Check(group);
        }

        public override void Copy(PWBShortcut other)
        {
            base.Copy(other);
            var otherKeyShortcut = other as PWBKeyShortcut;
            if (otherKeyShortcut == null) return;
            if (otherKeyShortcut._keyCombination == null) return;
            combination.Set(otherKeyShortcut._keyCombination.keyCode, otherKeyShortcut._keyCombination.modifiers);
        }
    }

    [System.Serializable]
    public class PWBHoldKeysAndClickShortcut : PWBKeyShortcut
    {
        public PWBHoldKeysAndClickShortcut(string name, Group group, KeyCode keyCode,
            EventModifiers modifiers = EventModifiers.None) : base(name, group, keyCode, modifiers) { }
        public override PWBKeyCombination combination
        {
            get
            {
                if (_keyCombination == null)
                    _keyCombination = new PWBHoldKeysAndMouseCombination(KeyCode.None, EventModifiers.None,
                        PWBHoldKeysAndMouseCombination.MouseEvent.CLICK);
                return _keyCombination;
            }
        }

        public PWBHoldKeysAndMouseCombination holdKeysAndClickCombination => _keyCombination as PWBHoldKeysAndMouseCombination;
    }

    [System.Serializable]
    public class PWBHoldKeysAndMouseMoveShortcut : PWBKeyShortcut
    {
        public PWBHoldKeysAndMouseMoveShortcut(string name, Group group, KeyCode keyCode,
            EventModifiers modifiers = EventModifiers.None) : base(name, group, keyCode, modifiers) { }
        public override PWBKeyCombination combination
        {
            get
            {
                if (_keyCombination == null) _keyCombination = new PWBHoldKeysAndMouseCombination(KeyCode.None,
                    EventModifiers.None, PWBHoldKeysAndMouseCombination.MouseEvent.MOVE);
                return _keyCombination;
            }
        }

        public PWBHoldKeysAndMouseCombination holdKeysAndMouseCombination => _keyCombination as PWBHoldKeysAndMouseCombination;
    }

    [System.Serializable]
    public class PWBTwoStepKeyShortcut : PWBKeyShortcut
    {
        [SerializeField] private bool _firstStepEnabled = true;

        public PWBTwoStepKeyShortcut(string name, Group group,
            KeyCode keyCode, EventModifiers modifiers = EventModifiers.None, bool firstStepEnabled = true)
            : base(name, group, keyCode, modifiers) { }

        public bool firstStepEnabled
        {
            get => _firstStepEnabled;
            set
            {
                if (_firstStepEnabled == value) return;
                _firstStepEnabled = value;
                if (_firstStepEnabled) SetGroup(PWBShortcut.Group.GRID);
                else SetGroup(PWBShortcut.Group.GLOBAL | PWBShortcut.Group.GRID);
            }
        }
        public override void Copy(PWBShortcut other)
        {
            base.Copy(other);
            var other2StepKeyShortcut = other as PWBTwoStepKeyShortcut;
            if (other2StepKeyShortcut == null) return;
            _firstStepEnabled = other2StepKeyShortcut._firstStepEnabled;
        }
    }
    [System.Serializable]
    public class PWBMouseShortcut : PWBShortcut
    {
        [SerializeField]
        private PWBMouseCombination _combination
            = new PWBMouseCombination(EventModifiers.None, PWBMouseCombination.MouseEvents.NONE);

        public PWBMouseShortcut(string name, Group group,
            EventModifiers modifiers, PWBMouseCombination.MouseEvents mouseEvent)
            : base(name, group) => _combination.Set(modifiers, mouseEvent);
        public PWBMouseCombination combination => _combination;
        public bool Check() => combination.Check(group);
        public override void Copy(PWBShortcut other)
        {
            base.Copy(other);
            var otherMouseShortcut = other as PWBMouseShortcut;
            if (otherMouseShortcut == null) return;
            _combination.Set(otherMouseShortcut._combination.modifiers, otherMouseShortcut._combination.mouseEvent);
        }
    }
    #endregion

    [System.Serializable]
    public class PWBShortcuts
    {
        #region PROFILE
        [SerializeField] private string _profileName = string.Empty;
        public string profileName { get => _profileName; set => _profileName = value; }
        public PWBShortcuts(string name) => _profileName = name;
        public void Copy(PWBShortcuts other)
        {
            //keyShortcuts
            /*/// GIZMOS ///*/
            _gizmosToggleInfotext.Copy(other._gizmosToggleInfotext);
            /*/// GRID ///*/
            _gridEnableShortcuts.Copy(other._gridEnableShortcuts);
            _gridToggle.Copy(other._gridToggle);
            _gridToggleSnapping.Copy(other._gridToggleSnapping);
            _gridToggleLock.Copy(other._gridToggleLock);
            _gridSetOriginPosition.Copy(other._gridSetOriginPosition);
            _gridSetOriginRotation.Copy(other._gridSetOriginRotation);
            _gridSetSize.Copy(other._gridSetSize);
            _gridFrameOrigin.Copy(other._gridFrameOrigin);
            _gridTogglePositionHandle.Copy(other._gridTogglePositionHandle);
            _gridToggleRotationHandle.Copy(other._gridToggleRotationHandle);
            _gridToggleSpacingHandle.Copy(other._gridToggleSpacingHandle);
            _gridMoveOriginUp.Copy(other._gridMoveOriginUp);
            _gridMoveOriginDown.Copy(other._gridMoveOriginDown);
            _gridNextOrigin.Copy(other._gridNextOrigin);
            /*/// SNAP ///*/
            _snapToggleBoundsSnapping.Copy(other._snapToggleBoundsSnapping);
            /*/// PIN ///*/
            _pinMoveHandlesUp.Copy(other._pinMoveHandlesUp);
            _pinMoveHandlesDown.Copy(other._pinMoveHandlesDown);
            _pinSelectNextHandle.Copy(other._pinSelectNextHandle);
            _pinSelectPivotHandle.Copy(other._pinSelectPivotHandle);
            _pinToggleRepeatItem.Copy(other._pinToggleRepeatItem);
            _pinResetScale = other._pinResetScale;

            _pinRotate90YCW.Copy(other._pinRotate90YCW);
            _pinRotate90YCCW.Copy(other._pinRotate90YCCW);
            _pinRotateAStepYCW.Copy(other._pinRotateAStepYCW);
            _pinRotateAStepYCCW.Copy(other._pinRotateAStepYCCW);

            _pinRotate90XCW.Copy(other._pinRotate90XCW);
            _pinRotate90XCCW.Copy(other._pinRotate90XCCW);
            _pinRotateAStepXCW.Copy(other._pinRotateAStepXCW);
            _pinRotateAStepXCCW.Copy(other._pinRotateAStepXCCW);

            _pinRotate90ZCW.Copy(other._pinRotate90ZCW);
            _pinRotate90ZCCW.Copy(other._pinRotate90ZCCW);
            _pinRotateAStepZCW.Copy(other._pinRotateAStepZCW);
            _pinRotateAStepZCCW.Copy(other._pinRotateAStepZCCW);

            _pinResetRotation.Copy(other._pinResetRotation);
            _pinSnapRotationToGrid.Copy(other._pinSnapRotationToGrid);

            _pinAdd1UnitToSurfDist.Copy(other._pinAdd1UnitToSurfDist);
            _pinSubtract1UnitFromSurfDist.Copy(other._pinSubtract1UnitFromSurfDist);
            _pinAdd01UnitToSurfDist.Copy(other._pinAdd01UnitToSurfDist);
            _pinSubtract01UnitFromSurfDist.Copy(other._pinSubtract01UnitFromSurfDist);

            _pinResetSurfDist.Copy(other._pinResetSurfDist);

            _pinSelectPreviousItem.Copy(other._pinSelectPreviousItem);
            _pinSelectNextItem.Copy(other._pinSelectNextItem);

            _pinFlipX.Copy(other._pinFlipX);
            /*/// BRUSH & GRAVITY ///*/
            _brushUpdatebrushstroke.Copy(other._brushUpdatebrushstroke);
            _brushResetRotation.Copy(other._brushResetRotation);
            /*/// GRAVITY ///*/
            _gravityAdd1UnitToSurfDist.Copy(other._gravityAdd1UnitToSurfDist);
            _gravitySubtract1UnitFromSurfDist.Copy(other._gravitySubtract1UnitFromSurfDist);
            _gravityAdd01UnitToSurfDist.Copy(other._gravityAdd01UnitToSurfDist);
            _gravitySubtract01UnitFromSurfDist.Copy(other._gravitySubtract01UnitFromSurfDist);
            /*/// EDIT MODE ///*/
            _editModeDeleteItemAndItsChildren.Copy(other._editModeDeleteItemAndItsChildren);
            _editModeDeleteItemButNotItsChildren.Copy(other._editModeDeleteItemButNotItsChildren);
            _editModeSelectParent.Copy(other._editModeSelectParent);
            _editModeToggle.Copy(other._editModeToggle);
            _editModeDuplicate.Copy(other._editModeDuplicate);
            /*/// LINE ///*/
            _lineSelectAllPoints.Copy(other._lineSelectAllPoints);
            _lineDeselectAllPoints.Copy(other._lineDeselectAllPoints);
            _lineToggleCurve.Copy(other._lineToggleCurve);
            _lineToggleClosed.Copy(other._lineToggleClosed);
            _lineEditModeTypeToggle.Copy(other._lineEditModeTypeToggle);
            /*/// TILING & SELECTION ///*/
            _selectionRotate90XCW.Copy(other._selectionRotate90XCW);
            _selectionRotate90XCCW.Copy(other._selectionRotate90XCCW);
            _selectionRotate90YCW.Copy(other._selectionRotate90YCW);
            _selectionRotate90YCCW.Copy(other._selectionRotate90YCCW);
            _selectionRotate90ZCW.Copy(other._selectionRotate90ZCW);
            _selectionRotate90ZCCW.Copy(other._selectionRotate90ZCCW);
            /*/// SELECTION ///*/
            _selectionTogglePositionHandle.Copy(other._selectionTogglePositionHandle);
            _selectionToggleRotationHandle.Copy(other._selectionToggleRotationHandle);
            _selectionToggleScaleHandle.Copy(other._selectionToggleScaleHandle);
            _selectionEditCustomHandle.Copy(other._selectionEditCustomHandle);
            _selectionToggleSpace.Copy(other._selectionToggleSpace);
            /*/// PALETTE ///*/
            _paletteDeleteBrush.Copy(other._paletteDeleteBrush);
            _palettePreviousBrush.Copy(other._palettePreviousBrush);
            _paletteNextBrush.Copy(other._paletteNextBrush);
            _palettePreviousPalette.Copy(other._palettePreviousPalette);
            _paletteNextPalette.Copy(other._paletteNextPalette);
            _palettePickBrush.Copy(other._palettePickBrush);
            _paletteReplaceSceneSelection.Copy(other._paletteReplaceSceneSelection);
            /*/// TOOLBAR ///*/
            _toolbarPinToggle.Copy(other._toolbarPinToggle);
            _toolbarBrushToggle.Copy(other._toolbarBrushToggle);
            _toolbarGravityToggle.Copy(other._toolbarGravityToggle);
            _toolbarLineToggle.Copy(other._toolbarLineToggle);
            _toolbarShapeToggle.Copy(other._toolbarShapeToggle);
            _toolbarTilingToggle.Copy(other._toolbarTilingToggle);
            _toolbarReplacerToggle.Copy(other._toolbarReplacerToggle);
            _toolbarEraserToggle.Copy(other._toolbarEraserToggle);
            _toolbarSelectionToggle.Copy(other._toolbarSelectionToggle);
            _toolbarExtrudeToggle.Copy(other._toolbarExtrudeToggle);
            _toolbarMirrorToggle.Copy(other._toolbarMirrorToggle);
            /*/// FLOOR ///*/
            _floorRotate90YCW.Copy(other._floorRotate90YCW);
            /*/// WALL ///*/
            _wallHalfTurn.Copy(other._wallHalfTurn);

            //Mouse shortcuts
            /*/// PIN ///*/
            _pinScale.Copy(other._pinScale);
            _pinSelectNextItemScroll.Copy(other._pinSelectNextItemScroll);

            _pinRotateAroundY.Copy(other._pinRotateAroundY);
            _pinRotateAroundYSnaped.Copy(other._pinRotateAroundYSnaped);
            _pinRotateAroundX.Copy(other._pinRotateAroundX);
            _pinRotateAroundXSnaped.Copy(other._pinRotateAroundXSnaped);
            _pinRotateAroundZ.Copy(other._pinRotateAroundZ);
            _pinRotateAroundZSnaped.Copy(other._pinRotateAroundZSnaped);

            _pinSurfDist.Copy(other._pinSurfDist);
            /*/// RADIUS ///*/
            _brushRadius.Copy(other._brushRadius);
            /*/// BRUSH & GRAVITY ///*/
            _brushDensity.Copy(other._brushDensity);
            _brushRotate.Copy(other._brushRotate);
            /*/// BRUSH & GRAVITY ///*/
            _gravitySurfDist.Copy(other._gravitySurfDist);
            /*/// LINE & SHAPE ///*/
            _lineEditGap.Copy(other._lineEditGap);
            /*/// LINE ///*/

            /*/// TILING ///*/
            _tilingEditSpacing1.Copy(other._tilingEditSpacing1);
            _tilingEditSpacing2.Copy(other._tilingEditSpacing2);
            /*/// PALETTE ///*/
            _paletteNextBrushScroll.Copy(other._paletteNextBrushScroll);
            _paletteNextPaletteScroll.Copy(other._paletteNextPaletteScroll);
        }

        public static PWBShortcuts GetDefault(int i)
        {
            if (i == 0) return new PWBShortcuts("Default 1");
            else if (i == 1)
            {
                var d2 = new PWBShortcuts("Default 2");
                d2.pinMoveHandlesUp.combination.Set(KeyCode.PageUp);
                d2.pinMoveHandlesDown.combination.Set(KeyCode.PageDown);
                d2.pinSelectPivotHandle.combination.Set(KeyCode.Home);
                d2.pinSelectNextHandle.combination.Set(KeyCode.End);
                d2.pinResetScale.combination.Set(KeyCode.Home, EventModifiers.Control | EventModifiers.Shift);

                d2.pinRotate90YCW.combination.Set(KeyCode.LeftArrow, EventModifiers.Control);
                d2._pinRotate90YCCW.combination.Set(KeyCode.RightArrow, EventModifiers.Control);
                d2.pinRotateAStepYCW.combination.Set(KeyCode.LeftArrow,
                    EventModifiers.Control | EventModifiers.Shift);
                d2.pinRotateAStepYCCW.combination.Set(KeyCode.RightArrow,
                    EventModifiers.Control | EventModifiers.Shift);

                d2.pinRotate90XCW.combination.Set(KeyCode.LeftArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.pinRotate90XCCW.combination.Set(KeyCode.RightArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.pinRotateAStepXCW.combination.Set(KeyCode.LeftArrow,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
                d2.pinRotateAStepXCCW.combination.Set(KeyCode.RightArrow,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

                d2.pinResetRotation.combination.Set(KeyCode.Home, EventModifiers.Control);

                d2.pinAdd1UnitToSurfDist.combination.Set(KeyCode.UpArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.pinSubtract1UnitFromSurfDist.combination.Set(KeyCode.DownArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.pinAdd01UnitToSurfDist.combination.Set(KeyCode.UpArrow,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
                d2.pinSubtract01UnitFromSurfDist.combination.Set(KeyCode.DownArrow,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

                d2.lineToggleCurve.combination.Set(KeyCode.PageDown);
                d2.lineToggleClosed.combination.Set(KeyCode.End);

                d2.selectionRotate90XCW.combination.Set(KeyCode.PageUp,
                    EventModifiers.Control | EventModifiers.Shift);
                d2.selectionRotate90XCCW.combination.Set(KeyCode.PageDown,
                    EventModifiers.Control | EventModifiers.Shift);
                d2.selectionRotate90YCW.combination.Set(KeyCode.LeftArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.selectionRotate90YCCW.combination.Set(KeyCode.RightArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.selectionRotate90ZCW.combination.Set(KeyCode.UpArrow,
                   EventModifiers.Control | EventModifiers.Alt);
                d2.selectionRotate90ZCCW.combination.Set(KeyCode.DownArrow,
                    EventModifiers.Control | EventModifiers.Alt);

                d2.brushRadius.combination.Set(EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_R_H);
                return d2;
            }
            else if (i == 2)
            {
                var d0 = new PWBShortcuts("Default 0");
                //GRID
                d0.gridEnableShortcuts.combination.Set(KeyCode.None);
                d0.gridToggle.firstStepEnabled = false;
                d0.gridToggleSnaping.firstStepEnabled = false;
                d0.gridToggleLock.firstStepEnabled = false;
                d0.gridSetOriginPosition.firstStepEnabled = false;
                d0.gridSetOriginRotation.firstStepEnabled = false;
                d0.gridSetSize.firstStepEnabled = false;
                d0.gridFrameOrigin.firstStepEnabled = false;
                d0.gridTogglePositionHandle.firstStepEnabled = false;
                d0.gridToggleRotationHandle.firstStepEnabled = false;
                d0.gridToggleSpacingHandle.firstStepEnabled = false;
                d0.gridMoveOriginUp.firstStepEnabled = false;
                d0.gridMoveOriginDown.firstStepEnabled = false;
                d0.gridNextOrigin.firstStepEnabled = false;

                //PIN
                d0.pinMoveHandlesUp.combination.Set(KeyCode.J);
                d0.pinMoveHandlesDown.combination.Set(KeyCode.J, EventModifiers.Control | EventModifiers.Shift);
                d0.pinSelectPrevHandle.combination.Set(KeyCode.I);
                d0.pinSelectNextHandle.combination.Set(KeyCode.O);
                d0.pinSelectPivotHandle.combination.Set(KeyCode.L);

                d0.pinToggleRepeatItem.combination.Set(KeyCode.Alpha0);

                d0.pinResetScale.combination.Set(KeyCode.Period);

                d0.pinRotate90YCW.combination.Set(KeyCode.U);
                d0.pinRotate90YCCW.combination.Set(KeyCode.U, EventModifiers.Control | EventModifiers.Alt);
                d0.pinRotateAStepYCW.combination.Set(KeyCode.U, EventModifiers.Shift);
                d0.pinRotateAStepYCCW.combination.Set(KeyCode.U,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

                d0.pinRotate90XCW.combination.Set(KeyCode.Q);
                d0.pinRotate90XCCW.combination.Set(KeyCode.Q, EventModifiers.Control | EventModifiers.Alt);
                d0.pinRotateAStepXCW.combination.Set(KeyCode.Q, EventModifiers.Shift);
                d0.pinRotateAStepXCCW.combination.Set(KeyCode.Q,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

                d0.pinRotate90ZCW.combination.Set(KeyCode.B);
                d0.pinRotate90ZCCW.combination.Set(KeyCode.B, EventModifiers.Control | EventModifiers.Alt);
                d0.pinRotateAStepZCW.combination.Set(KeyCode.B, EventModifiers.Shift);
                d0.pinRotateAStepZCCW.combination.Set(KeyCode.B,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

                d0.pinResetRotation.combination.Set(KeyCode.Comma);

                d0.pinSnapRotationToGrid.combination.Set(KeyCode.G);

                d0.pinAdd1UnitToSurfDist.combination.Set(KeyCode.H);
                d0.pinSubtract1UnitFromSurfDist.combination.Set(KeyCode.N);
                d0.pinAdd01UnitToSurfDist.combination.Set(KeyCode.H, EventModifiers.Shift);
                d0.pinSubtract01UnitFromSurfDist.combination.Set(KeyCode.N, EventModifiers.Shift);

                d0.pinResetSurfDist.combination.Set(KeyCode.M);

                d0.pinSelectPreviousItem.combination.Set(KeyCode.A);
                d0.pinSelectNextItem.combination.Set(KeyCode.D);

                d0.pinFlipX.combination.Set(KeyCode.Minus);
                //BRUSH & GRAVITY
                d0.brushUpdatebrushstroke.combination.Set(KeyCode.U);
                d0.brushResetRotation.combination.Set(KeyCode.Comma);

                //GRAVITY
                d0.gravityAdd1UnitToSurfDist.combination.Set(KeyCode.H);
                d0.gravitySubtract1UnitFromSurfDist.combination.Set(KeyCode.N);
                d0.gravityAdd01UnitToSurfDist.combination.Set(KeyCode.H, EventModifiers.Shift);
                d0.gravitySubtract01UnitFromSurfDist.combination.Set(KeyCode.N, EventModifiers.Shift);

                //EDIT MODE
                d0.editModeSelectParent.combination.Set(KeyCode.T);
                d0.editModeToggle.combination.Set(KeyCode.Period);
                d0.editModeDuplicate.combination.Set(KeyCode.D);

                //LINE
                d0.lineSelectAllPoints.combination.Set(KeyCode.A);
                d0.lineDeselectAllPoints.combination.Set(KeyCode.S);
                d0.lineToggleCurve.combination.Set(KeyCode.U);
                d0.lineToggleClosed.combination.Set(KeyCode.O);
                d0.lineEditModeTypeToggle.combination.Set(KeyCode.Comma);

                //TILING & SELECTION
                d0.selectionRotate90YCW.combination.Set(KeyCode.S);
                d0.selectionRotate90YCCW.combination.Set(KeyCode.S, EventModifiers.Control | EventModifiers.Alt);
                d0.selectionRotate90XCW.combination.Set(KeyCode.C);
                d0.selectionRotate90XCCW.combination.Set(KeyCode.C, EventModifiers.Control | EventModifiers.Alt);
                d0.selectionRotate90ZCW.combination.Set(KeyCode.B);
                d0.selectionRotate90ZCCW.combination.Set(KeyCode.B, EventModifiers.Control | EventModifiers.Alt);

                //SELECTION
                d0.selectionToggleSpace.combination.Set(KeyCode.A);

                //FLOOR
                d0.floorRotate90YCW.combination.Set(KeyCode.S);

                //WALL
                d0.wallHalfTurn.combination.Set(KeyCode.S);

                return d0;
            }
            return null;
        }
        #endregion

        #region KEY COMBINATIONS
        #region GIZMOS
        [SerializeField]
        private PWBKeyShortcut _gizmosToggleInfotext = new PWBKeyShortcut("Toggle InfoText",
          PWBShortcut.Group.GLOBAL, KeyCode.I, EventModifiers.Control | EventModifiers.Alt);
        public PWBKeyShortcut gizmosToggleInfotext => _gizmosToggleInfotext;
        #endregion

        #region GRID
        [SerializeField]
        private PWBKeyShortcut _gridEnableShortcuts = new PWBKeyShortcut("First step to enable grid shortcuts",
           PWBShortcut.Group.GLOBAL | PWBShortcut.Group.GRID, KeyCode.G, EventModifiers.Control);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridToggle = new PWBTwoStepKeyShortcut("Toggle grid",
            PWBShortcut.Group.GRID, KeyCode.G, EventModifiers.Control);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridToggleSnapping = new PWBTwoStepKeyShortcut("Toggle snapping",
            PWBShortcut.Group.GRID, KeyCode.H, EventModifiers.Control);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridToggleLock = new PWBTwoStepKeyShortcut("Toggle grid lock",
            PWBShortcut.Group.GRID, KeyCode.L, EventModifiers.Control);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridSetOriginPosition
            = new PWBTwoStepKeyShortcut("Set the origin to the active gameobject position",
            PWBShortcut.Group.GRID, KeyCode.W, EventModifiers.Control);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridSetOriginRotation
            = new PWBTwoStepKeyShortcut("Set the grid rotation to the active gameobject rotation",
            PWBShortcut.Group.GRID, KeyCode.E, EventModifiers.Control);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridSetSize
            = new PWBTwoStepKeyShortcut("Set the snap value to the size of the active gameobject",
            PWBShortcut.Group.GRID, KeyCode.R, EventModifiers.Control);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridFrameOrigin = new PWBTwoStepKeyShortcut("Frame grid origin",
            PWBShortcut.Group.GRID, KeyCode.Q, EventModifiers.Control);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridTogglePositionHandle = new PWBTwoStepKeyShortcut("Toggle Postion Handle",
            PWBShortcut.Group.GRID, KeyCode.W, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridToggleRotationHandle = new PWBTwoStepKeyShortcut("Toggle Rotation Handle",
            PWBShortcut.Group.GRID, KeyCode.E, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridToggleSpacingHandle = new PWBTwoStepKeyShortcut("Toggle Spacing Handle",
            PWBShortcut.Group.GRID, KeyCode.R, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridMoveOriginUp = new PWBTwoStepKeyShortcut("Move the origin one step up",
            PWBShortcut.Group.GRID, KeyCode.J, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridMoveOriginDown = new PWBTwoStepKeyShortcut("Move the origin one step down",
           PWBShortcut.Group.GRID, KeyCode.M, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBTwoStepKeyShortcut _gridNextOrigin = new PWBTwoStepKeyShortcut("Set next origin",
           PWBShortcut.Group.GRID, KeyCode.Alpha9, EventModifiers.Control | EventModifiers.Alt);
        public PWBKeyShortcut gridEnableShortcuts => _gridEnableShortcuts;
        public PWBTwoStepKeyShortcut gridToggle => _gridToggle;
        public PWBTwoStepKeyShortcut gridToggleSnaping => _gridToggleSnapping;
        public PWBTwoStepKeyShortcut gridToggleLock => _gridToggleLock;
        public PWBTwoStepKeyShortcut gridSetOriginPosition => _gridSetOriginPosition;
        public PWBTwoStepKeyShortcut gridSetOriginRotation => _gridSetOriginRotation;
        public PWBTwoStepKeyShortcut gridSetSize => _gridSetSize;
        public PWBTwoStepKeyShortcut gridFrameOrigin => _gridFrameOrigin;
        public PWBTwoStepKeyShortcut gridTogglePositionHandle => _gridTogglePositionHandle;
        public PWBTwoStepKeyShortcut gridToggleRotationHandle => _gridToggleRotationHandle;
        public PWBTwoStepKeyShortcut gridToggleSpacingHandle => _gridToggleSpacingHandle;
        public PWBTwoStepKeyShortcut gridMoveOriginUp => _gridMoveOriginUp;
        public PWBTwoStepKeyShortcut gridMoveOriginDown => _gridMoveOriginDown;
        public PWBTwoStepKeyShortcut gridNextOrigin => _gridNextOrigin;

        #endregion
        #region SNAP
        [SerializeField]
        private PWBKeyShortcut _snapToggleBoundsSnapping = new PWBKeyShortcut("Toggle bounds snapping",
            PWBShortcut.Group.GLOBAL, KeyCode.K, EventModifiers.Control | EventModifiers.Shift);
        public PWBKeyShortcut snapToggleBoundsSnapping => _snapToggleBoundsSnapping;
        #endregion

        #region PIN
        [SerializeField]
        private PWBKeyShortcut _pinMoveHandlesUp = new PWBKeyShortcut("Move handles up",
           PWBShortcut.Group.PIN, KeyCode.U, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinMoveHandlesDown = new PWBKeyShortcut("Move handles down",
           PWBShortcut.Group.PIN, KeyCode.J, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinSelectNextHandle = new PWBKeyShortcut("Select the next handle as active",
           PWBShortcut.Group.PIN, KeyCode.Y, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinSelectPrevHandle = new PWBKeyShortcut("Select the previous handle as active",
           PWBShortcut.Group.PIN, KeyCode.H, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinSelectPivotHandle = new PWBKeyShortcut("Set the pivot as the active handle",
           PWBShortcut.Group.PIN, KeyCode.T, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinToggleRepeatItem = new PWBKeyShortcut("Toggle repeat item option",
           PWBShortcut.Group.PIN, KeyCode.T, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _pinResetScale = new PWBKeyShortcut("Reset scale",
          PWBShortcut.Group.PIN, KeyCode.Period, EventModifiers.Control | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinRotate90YCW = new PWBKeyShortcut("Rotate 90º around Y",
          PWBShortcut.Group.PIN, KeyCode.Q, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _pinRotate90YCCW = new PWBKeyShortcut("Rotate -90º around Y",
          PWBShortcut.Group.PIN, KeyCode.W, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepYCW = new PWBKeyShortcut("Rotate in steps around Y",
        PWBShortcut.Group.PIN, KeyCode.Q, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepYCCW
            = new PWBKeyShortcut("Rotate in negative steps around Y",
        PWBShortcut.Group.PIN, KeyCode.W, EventModifiers.Control | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinRotate90XCW = new PWBKeyShortcut("Rotate 90º around X",
          PWBShortcut.Group.PIN, KeyCode.K, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinRotate90XCCW = new PWBKeyShortcut("Rotate -90º around X",
          PWBShortcut.Group.PIN, KeyCode.L, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepXCW = new PWBKeyShortcut("Rotate in steps around X",
        PWBShortcut.Group.PIN, KeyCode.K, EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepXCCW
            = new PWBKeyShortcut("Rotate in negative steps around X",
        PWBShortcut.Group.PIN, KeyCode.L, EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinRotate90ZCW = new PWBKeyShortcut("Rotate 90º around Z",
          PWBShortcut.Group.PIN, KeyCode.Period, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinRotate90ZCCW = new PWBKeyShortcut("Rotate -90º around Z",
          PWBShortcut.Group.PIN, KeyCode.Comma, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepZCW = new PWBKeyShortcut("Rotate in steps around Z",
        PWBShortcut.Group.PIN, KeyCode.Period, EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepZCCW
            = new PWBKeyShortcut("Rotate in negative steps around Z",
        PWBShortcut.Group.PIN, KeyCode.Comma, EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinResetRotation = new PWBKeyShortcut("Reset rotation to zero",
         PWBShortcut.Group.PIN, KeyCode.M, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinSnapRotationToGrid = new PWBKeyShortcut("Snap rotation to grid",
         PWBShortcut.Group.PIN, KeyCode.G, EventModifiers.Shift);


        [SerializeField]
        private PWBKeyShortcut _pinAdd1UnitToSurfDist = new PWBKeyShortcut("Surface distance +1",
          PWBShortcut.Group.PIN, KeyCode.U, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinSubtract1UnitFromSurfDist
            = new PWBKeyShortcut("Surface distance -1",
          PWBShortcut.Group.PIN, KeyCode.J, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinAdd01UnitToSurfDist
            = new PWBKeyShortcut("Surface distance +0.1",
         PWBShortcut.Group.PIN, KeyCode.U, EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinSubtract01UnitFromSurfDist
            = new PWBKeyShortcut("Surface distance -0.1",
          PWBShortcut.Group.PIN, KeyCode.J,
          EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinResetSurfDist = new PWBKeyShortcut("Surface distance = 0.0",
         PWBShortcut.Group.PIN, KeyCode.B, EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinSelectPreviousItem = new PWBKeyShortcut("Select previous item in the multi-brush",
          PWBShortcut.Group.PIN, KeyCode.O, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinSelectNextItem = new PWBKeyShortcut("Select next item in the multi-brush",
          PWBShortcut.Group.PIN, KeyCode.N, EventModifiers.Control | EventModifiers.Alt);

        [SerializeField]
        private PWBKeyShortcut _pinFlipX = new PWBKeyShortcut("Flip sprite horizontally",
            PWBShortcut.Group.PIN, KeyCode.U, EventModifiers.Shift);

        public PWBKeyShortcut pinMoveHandlesUp => _pinMoveHandlesUp;
        public PWBKeyShortcut pinMoveHandlesDown => _pinMoveHandlesDown;
        public PWBKeyShortcut pinSelectNextHandle => _pinSelectNextHandle;
        public PWBKeyShortcut pinSelectPrevHandle => _pinSelectPrevHandle;
        public PWBKeyShortcut pinSelectPivotHandle => _pinSelectPivotHandle;
        public PWBKeyShortcut pinToggleRepeatItem => _pinToggleRepeatItem;
        public PWBKeyShortcut pinResetScale => _pinResetScale;

        public PWBKeyShortcut pinRotate90YCW => _pinRotate90YCW;
        public PWBKeyShortcut pinRotate90YCCW => _pinRotate90YCCW;
        public PWBKeyShortcut pinRotateAStepYCW => _pinRotateAStepYCW;
        public PWBKeyShortcut pinRotateAStepYCCW => _pinRotateAStepYCCW;

        public PWBKeyShortcut pinRotate90XCW => _pinRotate90XCW;
        public PWBKeyShortcut pinRotate90XCCW => _pinRotate90XCCW;
        public PWBKeyShortcut pinRotateAStepXCW => _pinRotateAStepXCW;
        public PWBKeyShortcut pinRotateAStepXCCW => _pinRotateAStepXCCW;

        public PWBKeyShortcut pinRotate90ZCW => _pinRotate90ZCW;
        public PWBKeyShortcut pinRotate90ZCCW => _pinRotate90ZCCW;
        public PWBKeyShortcut pinRotateAStepZCW => _pinRotateAStepZCW;
        public PWBKeyShortcut pinRotateAStepZCCW => _pinRotateAStepZCCW;

        public PWBKeyShortcut pinResetRotation => _pinResetRotation;
        public PWBKeyShortcut pinSnapRotationToGrid => _pinSnapRotationToGrid;

        public PWBKeyShortcut pinAdd1UnitToSurfDist => _pinAdd1UnitToSurfDist;
        public PWBKeyShortcut pinSubtract1UnitFromSurfDist => _pinSubtract1UnitFromSurfDist;
        public PWBKeyShortcut pinAdd01UnitToSurfDist => _pinAdd01UnitToSurfDist;
        public PWBKeyShortcut pinSubtract01UnitFromSurfDist => _pinSubtract01UnitFromSurfDist;

        public PWBKeyShortcut pinResetSurfDist => _pinResetSurfDist;

        public PWBKeyShortcut pinSelectPreviousItem => _pinSelectPreviousItem;
        public PWBKeyShortcut pinSelectNextItem => _pinSelectNextItem;

        public PWBKeyShortcut pinFlipX => _pinFlipX;


        #endregion

        #region BRUSH & GRAVITY
        [SerializeField]
        private PWBKeyShortcut _brushUpdatebrushstroke = new PWBKeyShortcut("Update brushstroke",
          PWBShortcut.Group.BRUSH | PWBShortcut.Group.GRAVITY, KeyCode.Period, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _brushResetRotation = new PWBKeyShortcut("Reset brush rotation",
          PWBShortcut.Group.BRUSH | PWBShortcut.Group.GRAVITY, KeyCode.M, EventModifiers.Control);
        public PWBKeyShortcut brushUpdatebrushstroke => _brushUpdatebrushstroke;
        public PWBKeyShortcut brushResetRotation => _brushResetRotation;
        #endregion

        #region GRAVITY
        [SerializeField]
        private PWBKeyShortcut _gravityAdd1UnitToSurfDist
            = new PWBKeyShortcut("Surface distance +1",
          PWBShortcut.Group.GRAVITY, KeyCode.U, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _gravitySubtract1UnitFromSurfDist
            = new PWBKeyShortcut("Surface distance -1",
          PWBShortcut.Group.GRAVITY, KeyCode.J, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _gravityAdd01UnitToSurfDist
            = new PWBKeyShortcut("Surface distance +0.1",
         PWBShortcut.Group.GRAVITY, KeyCode.U,
         EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _gravitySubtract01UnitFromSurfDist
            = new PWBKeyShortcut("Surface distance -0.1",
          PWBShortcut.Group.GRAVITY, KeyCode.J,
          EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

        public PWBKeyShortcut gravityAdd1UnitToSurfDist => _gravityAdd1UnitToSurfDist;
        public PWBKeyShortcut gravitySubtract1UnitFromSurfDist => _gravitySubtract1UnitFromSurfDist;
        public PWBKeyShortcut gravityAdd01UnitToSurfDist => _gravityAdd01UnitToSurfDist;
        public PWBKeyShortcut gravitySubtract01UnitFromSurfDist => _gravitySubtract01UnitFromSurfDist;
        #endregion

        #region EDIT MODE
        [SerializeField]
        private PWBKeyShortcut _editModeDeleteItemAndItsChildren
            = new PWBKeyShortcut("Delete selected item and its children",
           PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE | PWBShortcut.Group.TILING,
           KeyCode.Delete, EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _editModeDeleteItemButNotItsChildren
            = new PWBKeyShortcut("Delete selected item but not its children",
           PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE | PWBShortcut.Group.TILING,
           KeyCode.Delete, EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _editModeSelectParent = new PWBKeyShortcut("Select parent object",
           PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE | PWBShortcut.Group.TILING,
           KeyCode.T, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _editModeToggle = new PWBKeyShortcut("Toggle edit mode",
          PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE | PWBShortcut.Group.TILING,
          KeyCode.Period, EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _editModeDuplicate = new PWBKeyShortcut("Duplicate",
          PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE | PWBShortcut.Group.TILING,
          KeyCode.D, EventModifiers.Control | EventModifiers.Shift);
        public PWBKeyShortcut editModeDeleteItemAndItsChildren => _editModeDeleteItemAndItsChildren;
        public PWBKeyShortcut editModeDeleteItemButNotItsChildren => _editModeDeleteItemButNotItsChildren;
        public PWBKeyShortcut editModeSelectParent => _editModeSelectParent;
        public PWBKeyShortcut editModeToggle => _editModeToggle;
        public PWBKeyShortcut editModeDuplicate => _editModeDuplicate;
        #endregion

        #region LINE
        [SerializeField]
        private PWBKeyShortcut _lineSelectAllPoints = new PWBKeyShortcut("Select all points",
          PWBShortcut.Group.LINE, KeyCode.A, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _lineDeselectAllPoints = new PWBKeyShortcut("Deselect all points",
          PWBShortcut.Group.LINE, KeyCode.D, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _lineToggleCurve = new PWBKeyShortcut("Set previous segment as Curved or Straight",
          PWBShortcut.Group.LINE, KeyCode.Y, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _lineToggleClosed = new PWBKeyShortcut("Close or open the line",
          PWBShortcut.Group.LINE, KeyCode.O, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _lineEditModeTypeToggle = new PWBKeyShortcut("Toggle edit mode type",
         PWBShortcut.Group.LINE, KeyCode.Comma, EventModifiers.Alt | EventModifiers.Shift);

        public PWBKeyShortcut lineSelectAllPoints => _lineSelectAllPoints;
        public PWBKeyShortcut lineDeselectAllPoints => _lineDeselectAllPoints;
        public PWBKeyShortcut lineToggleCurve => _lineToggleCurve;
        public PWBKeyShortcut lineToggleClosed => _lineToggleClosed;
        public PWBKeyShortcut lineEditModeTypeToggle => _lineEditModeTypeToggle;
        #endregion

        #region TILING & SELECTION
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90XCW = new PWBKeyShortcut("Rotate 90º around X",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.U, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90XCCW = new PWBKeyShortcut("Rotate -90º around X",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.J, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90YCW = new PWBKeyShortcut("Rotate 90º around Y",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.K, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90YCCW = new PWBKeyShortcut("Rotate -90º around Y",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.L, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90ZCW = new PWBKeyShortcut("Rotate 90º around Z",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.U, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90ZCCW = new PWBKeyShortcut("Rotate -90º around Z",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.J, EventModifiers.Control | EventModifiers.Alt);
        public PWBKeyShortcut selectionRotate90XCW => _selectionRotate90XCW;
        public PWBKeyShortcut selectionRotate90XCCW => _selectionRotate90XCCW;
        public PWBKeyShortcut selectionRotate90YCW => _selectionRotate90YCW;
        public PWBKeyShortcut selectionRotate90YCCW => _selectionRotate90YCCW;
        public PWBKeyShortcut selectionRotate90ZCW => _selectionRotate90ZCW;
        public PWBKeyShortcut selectionRotate90ZCCW => _selectionRotate90ZCCW;
        #endregion

        #region SELECTION
        [SerializeField]
        private PWBKeyShortcut _selectionTogglePositionHandle = new PWBKeyShortcut("Toggle position handle",
          PWBShortcut.Group.SELECTION, KeyCode.W);
        [SerializeField]
        private PWBKeyShortcut _selectionToggleRotationHandle = new PWBKeyShortcut("Toggle rotation handle",
          PWBShortcut.Group.SELECTION, KeyCode.E);
        [SerializeField]
        private PWBKeyShortcut _selectionToggleScaleHandle = new PWBKeyShortcut("Toggle scale handle",
          PWBShortcut.Group.SELECTION, KeyCode.R);
        [SerializeField]
        private PWBKeyShortcut _selectionEditCustomHandle = new PWBKeyShortcut("Edit custom handle",
          PWBShortcut.Group.SELECTION, KeyCode.U);
        [SerializeField]
        private PWBKeyShortcut _selectionToggleSpace = new PWBKeyShortcut("Toggle Space Global/Local",
          PWBShortcut.Group.SELECTION, KeyCode.X, EventModifiers.Shift);
        [SerializeField]
        private PWBHoldKeysAndMouseMoveShortcut _selectionMoveToMousePosition
            = new PWBHoldKeysAndMouseMoveShortcut("Move to mouse position", PWBShortcut.Group.SELECTION,
          KeyCode.W, EventModifiers.Shift);
        public PWBKeyShortcut selectionTogglePositionHandle => _selectionTogglePositionHandle;
        public PWBKeyShortcut selectionToggleRotationHandle => _selectionToggleRotationHandle;
        public PWBKeyShortcut selectionToggleScaleHandle => _selectionToggleScaleHandle;
        public PWBKeyShortcut selectionEditCustomHandle => _selectionEditCustomHandle;
        public PWBKeyShortcut selectionToggleSpace => _selectionToggleSpace;
        public PWBHoldKeysAndMouseMoveShortcut selectionMoveToMousePosition => _selectionMoveToMousePosition;
        #endregion

        #region TOOLBAR
        private PWBKeyShortcut _toolbarFloorToggle = new PWBKeyShortcut("Toggle Floor Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_FLOOR_SHORTCUT_ID));
        private PWBKeyShortcut _toolbarWallToggle = new PWBKeyShortcut("Toggle Wall Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_WALL_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarPinToggle = new PWBKeyShortcut("Toggle Pin Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_PIN_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarBrushToggle = new PWBKeyShortcut("Toggle Brush Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_BRUSH_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarGravityToggle = new PWBKeyShortcut("Toggle Gravity Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_GRAVITY_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarLineToggle = new PWBKeyShortcut("Toggle Line Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_LINE_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarShapeToggle = new PWBKeyShortcut("Toggle Shape Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_SHAPE_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarTilingToggle = new PWBKeyShortcut("Toggle Tiling Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_TILING_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarReplacerToggle = new PWBKeyShortcut("Toggle Replacer Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_REPLACER_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarEraserToggle = new PWBKeyShortcut("Toggle Eraser Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_ERASER_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarSelectionToggle = new PWBKeyShortcut("Toggle Selection Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_SELECTION_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarCircleSelectToggle = new PWBKeyShortcut("Toggle Circle Selection Tool",
           PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_CIRCLE_SELECT_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarExtrudeToggle = new PWBKeyShortcut("Toggle Extrude Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_EXTRUDE_SHORTCUT_ID));

        private PWBKeyShortcut _toolbarMirrorToggle = new PWBKeyShortcut("Toggle Mirror Tool",
            PWBShortcut.Group.GLOBAL, new PWBKeyCombinationUSM(Shortcuts.PWB_TOGGLE_MIRROR_SHORTCUT_ID));
        public PWBKeyShortcut toolbarFloorToggle => _toolbarFloorToggle;
        public PWBKeyShortcut toolbarWallToggle => _toolbarWallToggle;
        public PWBKeyShortcut toolbarPinToggle => _toolbarPinToggle;
        public PWBKeyShortcut toolbarBrushToggle => _toolbarBrushToggle;
        public PWBKeyShortcut toolbarGravityToggle => _toolbarGravityToggle;
        public PWBKeyShortcut toolbarLineToggle => _toolbarLineToggle;
        public PWBKeyShortcut toolbarShapeToggle => _toolbarShapeToggle;
        public PWBKeyShortcut toolbarTilingToggle => _toolbarTilingToggle;
        public PWBKeyShortcut toolbarReplacerToggle => _toolbarReplacerToggle;
        public PWBKeyShortcut toolbarEraserToggle => _toolbarEraserToggle;
        public PWBKeyShortcut toolbarSelectionToggle => _toolbarSelectionToggle;
        public PWBKeyShortcut toolbarCircleSelectToggle => _toolbarCircleSelectToggle;
        public PWBKeyShortcut toolbarExtrudeToggle => _toolbarExtrudeToggle;
        public PWBKeyShortcut toolbarMirrorToggle => _toolbarMirrorToggle;
        #endregion

        #region PALETTE
        [SerializeField]
        private PWBKeyShortcut _paletteDeleteBrush = new PWBKeyShortcut("Delete selected brushes",
           PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
           KeyCode.Delete, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _palettePreviousBrush = new PWBKeyShortcut("Select previous brush",
          PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
          KeyCode.Z, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _paletteNextBrush = new PWBKeyShortcut("Select next brush",
          PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
          KeyCode.X, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _palettePreviousPalette = new PWBKeyShortcut("Select previous palette",
          PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
          KeyCode.Z, EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _paletteNextPalette = new PWBKeyShortcut("Select next palette",
          PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
          KeyCode.X, EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBHoldKeysAndClickShortcut _palettePickBrush = new PWBHoldKeysAndClickShortcut("Pick or add a brush",
          PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
          KeyCode.Alpha1, EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _paletteReplaceSceneSelection = new PWBKeyShortcut("Replace selected objects in scene",
          PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
          KeyCode.I, EventModifiers.Control | EventModifiers.Shift);

        public PWBKeyShortcut paletteDeleteBrush => _paletteDeleteBrush;
        public PWBKeyShortcut palettePreviousBrush => _palettePreviousBrush;
        public PWBKeyShortcut paletteNextBrush => _paletteNextBrush;
        public PWBKeyShortcut palettePreviousPalette => _palettePreviousPalette;
        public PWBKeyShortcut paletteNextPalette => _paletteNextPalette;
        public PWBHoldKeysAndClickShortcut palettePickBrush => _palettePickBrush;
        public PWBKeyShortcut paletteReplaceSceneSelection => _paletteReplaceSceneSelection;
        #endregion

        #region FLOOR
        [SerializeField]
        private PWBKeyShortcut _floorRotate90YCW = new PWBKeyShortcut("Rotate 90º around Y",
          PWBShortcut.Group.FLOOR, KeyCode.E, EventModifiers.Shift);
        public PWBKeyShortcut floorRotate90YCW => _floorRotate90YCW;
        #endregion

        #region WALL
        [SerializeField]
        private PWBKeyShortcut _wallHalfTurn = new PWBKeyShortcut("Rotate 180º around Y",
          PWBShortcut.Group.WALL, KeyCode.E, EventModifiers.Shift);
        public PWBKeyShortcut wallHalfTurn => _wallHalfTurn;
        #endregion

        #region CONFLICTS
        private PWBKeyShortcut[] _keyShortcuts = null;
        public PWBKeyShortcut[] keyShortcuts
        {
            get
            {
                if (_keyShortcuts == null)
                    _keyShortcuts = new PWBKeyShortcut[]
                    {
                        /*/// GIZMOS ///*/
                        _gizmosToggleInfotext,
                        /*/// GRID ///*/
                        _gridEnableShortcuts,
                        _gridToggle,
                        _gridToggleSnapping,
                        _gridToggleLock,
                        _gridSetOriginPosition,
                        _gridSetOriginRotation,
                        _gridSetSize,
                        _gridFrameOrigin,
                        _gridTogglePositionHandle,
                        _gridToggleRotationHandle,
                        _gridToggleSpacingHandle,
                        _gridMoveOriginUp,
                        _gridMoveOriginDown,
                        _gridNextOrigin,
                        /*/// SNAP ///*/
                        _snapToggleBoundsSnapping,
                        /*/// PIN ///*/
                        _pinMoveHandlesUp,
                        _pinMoveHandlesDown,
                        _pinSelectNextHandle,
                        _pinSelectPivotHandle,
                        _pinToggleRepeatItem,
                        _pinResetScale,

                        _pinRotate90YCW,
                        _pinRotate90YCCW,
                        _pinRotateAStepYCW,
                        _pinRotateAStepYCCW,

                        _pinRotate90XCW,
                        _pinRotate90XCCW,
                        _pinRotateAStepXCW,
                        _pinRotateAStepXCCW,

                        _pinRotate90ZCW,
                        _pinRotate90ZCCW,
                        _pinRotateAStepZCW,
                        _pinRotateAStepZCCW,

                        _pinResetRotation,
                        _pinSnapRotationToGrid,

                        _pinAdd1UnitToSurfDist,
                        _pinSubtract1UnitFromSurfDist,
                        _pinAdd01UnitToSurfDist,
                        _pinSubtract01UnitFromSurfDist,

                        _pinResetSurfDist,

                        _pinSelectPreviousItem,
                        _pinSelectNextItem,

                        _pinFlipX,
                        /*/// BRUSH & GRAVITY ///*/
                        _brushUpdatebrushstroke,
                        _brushResetRotation,
                        /*/// GRAVITY ///*/
                        _gravityAdd1UnitToSurfDist,
                        _gravitySubtract1UnitFromSurfDist,
                        _gravityAdd01UnitToSurfDist,
                        _gravitySubtract01UnitFromSurfDist,
                        /*/// EDIT MODE ///*/
                        _editModeDeleteItemAndItsChildren,
                        _editModeDeleteItemButNotItsChildren,
                        _editModeSelectParent,
                        _editModeToggle,
                        _editModeDuplicate,
                        /*/// LINE ///*/
                        _lineSelectAllPoints,
                        _lineDeselectAllPoints,
                        _lineToggleCurve,
                        _lineToggleClosed,
                        _lineEditModeTypeToggle,
                        /*/// TILING & SELECTION ///*/
                        _selectionRotate90XCW,
                        _selectionRotate90XCCW,
                        _selectionRotate90YCW,
                        _selectionRotate90YCCW,
                        _selectionRotate90ZCW,
                        _selectionRotate90ZCCW,
                        /*/// SELECTION ///*/
                        _selectionTogglePositionHandle,
                        _selectionToggleRotationHandle,
                        _selectionToggleScaleHandle,
                        _selectionEditCustomHandle,
                        _selectionToggleSpace,
                        _selectionMoveToMousePosition,
                        /*/// PALETTE ///*/
                        _paletteDeleteBrush,
                        _palettePreviousBrush,
                        _paletteNextBrush,
                        _palettePreviousPalette,
                        _paletteNextPalette,
                        _palettePickBrush,
                        _paletteReplaceSceneSelection,
                        /*/// TOOLBAR ///*/
                        _toolbarPinToggle,
                        _toolbarBrushToggle,
                        _toolbarGravityToggle,
                        _toolbarLineToggle,
                        _toolbarShapeToggle,
                        _toolbarTilingToggle,
                        _toolbarReplacerToggle,
                        _toolbarEraserToggle,
                        _toolbarSelectionToggle,
                        _toolbarExtrudeToggle,
                        _toolbarMirrorToggle,
                        /*/// FLOOR ///*/
                        _floorRotate90YCW,
                        /*/// WALL ///*/
                        _wallHalfTurn,
                    };
                return _keyShortcuts;
            }
        }
        private System.Collections.Generic.Dictionary<PWBKeyShortcut, PWBKeyShortcut> _keyConflicts
            = new System.Collections.Generic.Dictionary<PWBKeyShortcut, PWBKeyShortcut>();
        public void UpdateConficts()
        {
            _keyConflicts.Clear();
            foreach (var shortcut in keyShortcuts) shortcut.conflicted = false;
            for (int i = 0; i < keyShortcuts.Length; ++i)
            {
                var shortcut1 = keyShortcuts[i];
                if (shortcut1.conflicted) continue;
                if (shortcut1.combination.keyCode == KeyCode.None) continue;
                for (int j = i + 1; j < keyShortcuts.Length; ++j)
                {
                    var shortcut2 = keyShortcuts[j];
                    if (shortcut2.conflicted) continue;
                    if (shortcut2.combination.keyCode == KeyCode.None) continue;

                    if ((shortcut1.group & shortcut2.group) == 0 && (shortcut1.group & PWBShortcut.Group.GLOBAL) == 0
                       && (shortcut2.group & PWBShortcut.Group.GLOBAL) == 0) continue;
                    if (shortcut1 == gridEnableShortcuts && (shortcut2.group & PWBShortcut.Group.GRID) != 0
                        && (shortcut2.group & PWBShortcut.Group.GLOBAL) == 0)
                        continue;

                    if (shortcut1.combination == shortcut2.combination)
                    {
                        shortcut1.conflicted = true;
                        shortcut2.conflicted = true;
                        var combiString = shortcut1.combination.ToString();
                        if (!_keyConflicts.ContainsKey(shortcut1)) _keyConflicts.Add(shortcut1, shortcut2);
                        if (!_keyConflicts.ContainsKey(shortcut2)) _keyConflicts.Add(shortcut2, shortcut1);
                    }
                }
            }
        }
        public bool GetConflictShortcuts(PWBKeyShortcut shortcut1, out PWBKeyShortcut shortcut2)
        {
            shortcut2 = null;
            if (!_keyConflicts.ContainsKey(shortcut1)) UpdateConficts();
            if (!_keyConflicts.ContainsKey(shortcut1)) return false;
            shortcut2 = _keyConflicts[shortcut1];
            return true;
        }

        public bool CheckConflicts(PWBKeyCombination combi, PWBKeyShortcut target, out string conflicts)
        {
            conflicts = string.Empty;
            foreach (var shortcut in keyShortcuts)
            {
                if (target == shortcut) continue;
                if (target.combination.keyCode == KeyCode.None || shortcut.combination.keyCode == KeyCode.None) continue;
                if (combi == shortcut.combination && ((target.group & shortcut.group) != 0
                    || (shortcut.group & PWBShortcut.Group.GLOBAL) != 0 || (target.group & PWBShortcut.Group.GLOBAL) != 0))
                {
                    if (shortcut == gridEnableShortcuts && (target.group & PWBShortcut.Group.GRID) != 0) continue;
                    if (target == gridEnableShortcuts && (shortcut.group & PWBShortcut.Group.GRID) != 0) continue;
                    if (conflicts != string.Empty) conflicts += "\n";
                    conflicts += shortcut.name;
                }
            }
            return conflicts != string.Empty;
        }
        #endregion
        #endregion

        #region MOUSE COMBINATIONS

        #region PIN
        [SerializeField]
        private PWBMouseShortcut _pinScale = new PWBMouseShortcut("Edit Scale",
           PWBShortcut.Group.PIN, EventModifiers.Control, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);
        [SerializeField]
        private PWBMouseShortcut _pinSelectNextItemScroll
            = new PWBMouseShortcut("Select prev/next item in the multi-brush",
                PWBShortcut.Group.PIN, EventModifiers.Control | EventModifiers.Alt,
                PWBMouseCombination.MouseEvents.SCROLL_WHEEL);

        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundY = new PWBMouseShortcut("Rotate freely around Y",
           PWBShortcut.Group.PIN, EventModifiers.Control, PWBMouseCombination.MouseEvents.DRAG_R_H);
        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundYSnaped
            = new PWBMouseShortcut("Rotate freely around Y in steps",
                PWBShortcut.Group.PIN, EventModifiers.Control | EventModifiers.Alt, PWBMouseCombination.MouseEvents.DRAG_R_H);

        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundX = new PWBMouseShortcut("Rotate freely around X",
           PWBShortcut.Group.PIN, EventModifiers.Control, PWBMouseCombination.MouseEvents.DRAG_M_V);
        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundXSnaped
            = new PWBMouseShortcut("Rotate freely around X in steps",
                PWBShortcut.Group.PIN, EventModifiers.Control | EventModifiers.Alt, PWBMouseCombination.MouseEvents.DRAG_M_V);

        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundZ = new PWBMouseShortcut("Rotate freely around Z",
           PWBShortcut.Group.PIN, EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_M_V);
        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundZSnaped
            = new PWBMouseShortcut("Rotate freely around Z in steps", PWBShortcut.Group.PIN,
                EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_M_V);

        [SerializeField]
        private PWBMouseShortcut _pinSurfDist = new PWBMouseShortcut("Edit distance to the surface",
           PWBShortcut.Group.PIN,
           EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_R_V);

        public PWBMouseShortcut pinScale => _pinScale;
        public PWBMouseShortcut pinSelectNextItemScroll => _pinSelectNextItemScroll;

        public PWBMouseShortcut pinRotateAroundY => _pinRotateAroundY;
        public PWBMouseShortcut pinRotateAroundYSnaped => _pinRotateAroundYSnaped;
        public PWBMouseShortcut pinRotateAroundX => _pinRotateAroundX;
        public PWBMouseShortcut pinRotateAroundXSnaped => _pinRotateAroundXSnaped;
        public PWBMouseShortcut pinRotateAroundZ => _pinRotateAroundZ;
        public PWBMouseShortcut pinRotateAroundZSnaped => _pinRotateAroundZSnaped;

        public PWBMouseShortcut pinSurfDist => _pinSurfDist;
        #endregion

        #region RADIUS
        [SerializeField]
        private PWBMouseShortcut _brushRadius = new PWBMouseShortcut("Change radius",
           PWBShortcut.Group.BRUSH | PWBShortcut.Group.GRAVITY
            | PWBShortcut.Group.ERASER | PWBShortcut.Group.REPLACER | PWBShortcut.Group.CIRCLE_SELECT,
           EventModifiers.Control, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);
        public PWBMouseShortcut brushRadius => _brushRadius;
        #endregion

        #region BRUSH & GRAVITY
        [SerializeField]
        private PWBMouseShortcut _brushDensity = new PWBMouseShortcut("Edit density",
           PWBShortcut.Group.BRUSH | PWBShortcut.Group.GRAVITY,
           EventModifiers.Control | EventModifiers.Alt, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);
        [SerializeField]
        private PWBMouseShortcut _brushRotate = new PWBMouseShortcut("Rotate brush",
           PWBShortcut.Group.BRUSH | PWBShortcut.Group.GRAVITY,
           EventModifiers.Control, PWBMouseCombination.MouseEvents.DRAG_R_H);

        public PWBMouseShortcut brushDensity => _brushDensity;
        public PWBMouseShortcut brushRotate => _brushRotate;
        #endregion

        #region GRAVITY
        [SerializeField]
        private PWBMouseShortcut _gravitySurfDist
            = new PWBMouseShortcut("Edit distance to the surface", PWBShortcut.Group.GRAVITY,
           EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_R_V);
        public PWBMouseShortcut gravitySurfDist => _gravitySurfDist;
        #endregion

        #region LINE & SHAPE
        [SerializeField]
        private PWBMouseShortcut _lineEditGap
            = new PWBMouseShortcut("Edit gap size", PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE,
           EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_R_H);
        public PWBMouseShortcut lineEditGap => _lineEditGap;
        #endregion

        #region TILING
        [SerializeField]
        private PWBMouseShortcut _tilingEditSpacing1 = new PWBMouseShortcut("Edit spacing on axis 1", PWBShortcut.Group.TILING,
           EventModifiers.Control, PWBMouseCombination.MouseEvents.DRAG_R_H);
        [SerializeField]
        private PWBMouseShortcut _tilingEditSpacing2 = new PWBMouseShortcut("Edit spacing on axis 2", PWBShortcut.Group.TILING,
           EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_R_H);
        public PWBMouseShortcut tilingEditSpacing1 => _tilingEditSpacing1;
        public PWBMouseShortcut tilingEditSpacing2 => _tilingEditSpacing2;
        #endregion

        #region PALETTE
        [SerializeField]
        private PWBMouseShortcut _paletteNextBrushScroll = new PWBMouseShortcut("Select prev/next brush",
            PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
           EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);
        [SerializeField]
        private PWBMouseShortcut _paletteNextPaletteScroll = new PWBMouseShortcut("Select prev/next palette",
            PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
           EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);
        public PWBMouseShortcut paletteNextBrushScroll => _paletteNextBrushScroll;
        public PWBMouseShortcut paletteNextPaletteScroll => _paletteNextPaletteScroll;
        #endregion

        #region CONFLICTS
        private PWBMouseShortcut[] _mouseShortcuts = null;
        public PWBMouseShortcut[] mouseShortcuts
        {
            get
            {
                if (_mouseShortcuts == null)
                    _mouseShortcuts = new PWBMouseShortcut[]
                    {
                        /*/// PIN ///*/
                        _pinScale,
                        _pinSelectNextItemScroll,

                        _pinRotateAroundY,
                        _pinRotateAroundYSnaped,
                        _pinRotateAroundX,
                        _pinRotateAroundXSnaped,
                        _pinRotateAroundZ,
                        _pinRotateAroundZSnaped,

                        _pinSurfDist,
                        /*/// RADIUS ///*/
                        _brushRadius,
                        /*/// BRUSH & GRAVITY ///*/
                        _brushDensity,
                        _brushRotate,
                        /*/// BRUSH & GRAVITY ///*/
                        _gravitySurfDist,
                        /*/// LINE & SHAPE ///*/
                        _lineEditGap,
                        /*/// LINE ///*/
                        
                        /*/// TILING ///*/
                        _tilingEditSpacing1,
                        _tilingEditSpacing2,
                        /*/// PALETTE ///*/
                        _paletteNextBrushScroll,
                        _paletteNextPaletteScroll

                    };
                return _mouseShortcuts;
            }
        }

        public void UpdateMouseConficts()
        {
            foreach (var scrollShortcut in mouseShortcuts) scrollShortcut.conflicted = false;
            for (int i = 0; i < mouseShortcuts.Length; ++i)
            {
                var shortcut1 = mouseShortcuts[i];
                if (shortcut1.conflicted) continue;
                if (shortcut1.combination.modifiers == EventModifiers.None) continue;
                for (int j = i + 1; j < mouseShortcuts.Length; ++j)
                {
                    var shortcut2 = mouseShortcuts[j];
                    if (shortcut2.conflicted) continue;
                    if (shortcut2.combination.modifiers == EventModifiers.None) continue;
                    if ((shortcut1.group & shortcut2.group) == 0 && (shortcut1.group & PWBShortcut.Group.GLOBAL) == 0
                        && (shortcut1.group & PWBShortcut.Group.GLOBAL) == 0) continue;
                    if (shortcut1.combination == shortcut2.combination)
                    {
                        shortcut1.conflicted = true;
                        shortcut2.conflicted = true;
                    }
                }
            }
        }

        public bool CheckMouseConflicts(PWBMouseCombination combi, PWBMouseShortcut target, out string conflicts)
        {
            conflicts = string.Empty;
            foreach (var shortcut in mouseShortcuts)
            {
                if (target == shortcut) continue;
                if (target.combination.modifiers == EventModifiers.None
                    || shortcut.combination.modifiers == EventModifiers.None) continue;
                if (combi == shortcut.combination && ((target.group & shortcut.group) != 0
                    || (shortcut.group & PWBShortcut.Group.GLOBAL) != 0 || (target.group & PWBShortcut.Group.GLOBAL) != 0))
                {
                    if (conflicts != string.Empty) conflicts += "\n";
                    conflicts += shortcut.name;
                }
            }
            return conflicts != string.Empty;
        }

        public bool CombinationExist(PWBMouseCombination combi, PWBShortcut.Group group)
        {
            foreach (var shortcut in mouseShortcuts)
            {
                if (combi == shortcut.combination && ((group & shortcut.group) != 0
                    || (shortcut.group & PWBShortcut.Group.GLOBAL) != 0 || (group & PWBShortcut.Group.GLOBAL) != 0))
                    return true;
            }
            return false;
        }
        #endregion
        #endregion
        public static (string Command, string Shortcut)[] GetShortcuts(PWBShortcut.Group group, EventModifiers modifiers)
        {
            var result = new System.Collections.Generic.List<(string Command, string Shortcut)>();
            if (group == PWBShortcut.Group.NONE) return new (string Command, string Shortcut)[] { };
            var defaultProfile = PWBSettings.shortcuts;
            var keyShortcutsArray = new System.Collections.Generic.List<PWBKeyShortcut>(defaultProfile.keyShortcuts);
            foreach (var shortcut in keyShortcutsArray)
            {
                if ((shortcut.group & group) != group) continue;
                if (shortcut.combination.modifiers == modifiers) result.Add((shortcut.name, shortcut.combination.ToString()));
            }
            var mouseShortcutsArray = new System.Collections.Generic.List<PWBMouseShortcut>(defaultProfile.mouseShortcuts);
            foreach (var shortcut in mouseShortcutsArray)
            {
                if ((shortcut.group & group) != group) continue;
                if (shortcut.combination.modifiers == modifiers) result.Add((shortcut.name, shortcut.combination.ToString()));
            }
            return result.ToArray();
        }

        public static (string Command, string Shortcut)[] GetAllShortcuts(PWBShortcut.Group group, PWBShortcut.Group exclude)
        {
            var result = new System.Collections.Generic.List<(string Command, string Shortcut)>();
            if (group == PWBShortcut.Group.NONE) return new (string Command, string Shortcut)[] { };
            var defaultProfile = PWBSettings.shortcuts;
            var keyShortcutsArray = new System.Collections.Generic.List<PWBKeyShortcut>(defaultProfile.keyShortcuts);
            foreach (var shortcut in keyShortcutsArray)
            {
                if ((shortcut.group & group) != group) continue;
                if (exclude != PWBShortcut.Group.NONE && (shortcut.group & exclude) == exclude) continue;
                result.Add((shortcut.name, shortcut.combination.ToString()));
            }
            var mouseShortcutsArray = new System.Collections.Generic.List<PWBMouseShortcut>(defaultProfile.mouseShortcuts);
            foreach (var shortcut in mouseShortcutsArray)
            {
                if ((shortcut.group & group) != group) continue;
                if (exclude != PWBShortcut.Group.NONE && (shortcut.group & exclude) == exclude) continue;
                result.Add((shortcut.name, shortcut.combination.ToString()));
            }
            return result.ToArray();
        }
    }
    #endregion

    #region SETTINGS
    [System.Serializable]
    public class PWBSettings
    {
        #region COMMON
        private static string _settingsPath = null;
        private static PWBSettings _instance = null;
        private PWBSettings() { }

        private static PWBSettings instance
        {
            get
            {
                if (_instance == null) _instance = new PWBSettings();
                return _instance;
            }
        }
        private static string settingsPath
        {
            get
            {
                if (_settingsPath == null)
                    _settingsPath = System.IO.Directory.GetParent(Application.dataPath) + "/ProjectSettings/PWBSettings.txt";
                return _settingsPath;
            }
        }
        private void LoadFromFile()
        {
            if (!System.IO.File.Exists(settingsPath))
            {
                var files = System.IO.Directory.GetFiles(Application.dataPath,
                        PWBData.FULL_FILE_NAME, System.IO.SearchOption.AllDirectories);
                if (files.Length > 0) _dataDir = System.IO.Path.GetDirectoryName(files[0]);
                else
                {
                    _dataDir = Application.dataPath + "/" + PWBData.RELATIVE_DATA_DIR;
                    System.IO.Directory.CreateDirectory(_dataDir);
                    _dataDir = PWBCore.GetRelativePath(_dataDir);
                }
                Save();
            }
            else
            {
                var settings = JsonUtility.FromJson<PWBSettings>(System.IO.File.ReadAllText(settingsPath));
                _dataDir = settings._dataDir;
                if (PWBCore.IsFullPath(_dataDir)) _dataDir = PWBCore.GetRelativePath(_dataDir);

                foreach (var loadedProfile in settings._shortcutProfiles)
                {
                    var profiles = _shortcutProfiles.Where(p => p.profileName == loadedProfile.profileName);
                    if (profiles.Count() <= 0) continue;
                    var profile = profiles.First();
                    profile.Copy(loadedProfile);
                }
                _selectedProfileIdx = settings._selectedProfileIdx;
            }
        }

        private void Save()
        {
            var jsonString = JsonUtility.ToJson(this, true);
            System.IO.File.WriteAllText(settingsPath, jsonString);
        }
        #endregion

        #region DATA DIR
        [SerializeField] private string _dataDir = null;
        private static bool _movingDir = false;
        public static bool movingDir => _movingDir;
        private static void CheckDataDir()
        {
            if (instance._dataDir == null) instance.LoadFromFile();
            if (PWBCore.IsFullPath(instance._dataDir)) instance._dataDir = PWBCore.GetRelativePath(instance._dataDir);

        }

        public static string relativeDataDir
        {
            get
            {
                CheckDataDir();
                var currentDir = PWBCore.GetFullPath(instance._dataDir);
                if (!System.IO.Directory.Exists(currentDir))
                {
                    if (currentDir.Replace("\\", "/").Contains(PWBData.RELATIVE_DATA_DIR))
                    {
                        var directories = System.IO.Directory.GetDirectories(Application.dataPath, PWBData.DATA_DIR,
                            System.IO.SearchOption.AllDirectories)
                            .Where(d => d.Replace("\\", "/").Contains(PWBData.RELATIVE_DATA_DIR)).ToArray();
                        if (directories.Length > 0)
                        {
                            instance._dataDir = PWBCore.GetRelativePath(directories[0].Replace("\\", "/"));
                            instance.Save();
                            PaletteManager.instance.LoadPaletteFiles(false);
                            PrefabPalette.UpdateTabBar();
                        }
                    }
                }
                return instance._dataDir;
            }
        }

        public static void SetDataDir(string fullPath)
        {
            var newDirRelative = PWBCore.GetRelativePath(fullPath);
            if (instance._dataDir == newDirRelative) return;
            var currentFullDir = PWBCore.GetFullPath(instance._dataDir);
            void DeleteMeta(string path)
            {
                var metapath = path + ".meta";
                if (System.IO.File.Exists(metapath)) System.IO.File.Delete(metapath);
            }

            bool DeleteIfEmpty(string dirPath)
            {
                if (System.IO.Directory.GetFiles(dirPath).Length != 0) return false;
                System.IO.Directory.Delete(dirPath);
                DeleteMeta(dirPath);
                return true;
            }
            if (System.IO.Directory.Exists(currentFullDir))
            {
                _movingDir = true;
                var currentDataPath = currentFullDir + "/" + PWBData.FULL_FILE_NAME;
                if (System.IO.File.Exists(currentDataPath))
                {
                    var newDataPath = fullPath + "/" + PWBData.FULL_FILE_NAME;
                    if (System.IO.File.Exists(newDataPath)) System.IO.File.Delete(newDataPath);
                    DeleteMeta(currentDataPath);
                    System.IO.File.Move(currentDataPath, newDataPath);

                    var currentPalettesDir = currentFullDir + "/" + PWBData.PALETTES_DIR;
                    if (System.IO.Directory.Exists(currentPalettesDir))
                    {
                        var newPalettesDir = fullPath + "/" + PWBData.PALETTES_DIR;
                        if (!System.IO.Directory.Exists(newPalettesDir))
                            System.IO.Directory.CreateDirectory(newPalettesDir);
                        var palettesPaths = System.IO.Directory.GetFiles(currentPalettesDir, "*.txt");
                        foreach (var currentPalettePath in palettesPaths)
                        {
                            var fileName = System.IO.Path.GetFileName(currentPalettePath);
                            var newPalettePath = newPalettesDir + "/" + fileName;
                            if (System.IO.File.Exists(newPalettePath)) System.IO.File.Delete(newPalettePath);
                            DeleteMeta(currentPalettePath);

                            var paletteText = System.IO.File.ReadAllText(currentPalettePath);
                            var palette = JsonUtility.FromJson<PaletteData>(paletteText);
                            palette.filePath = newPalettePath;

                            System.IO.File.Move(currentPalettePath, newPalettePath);
                            System.IO.File.Delete(currentPalettePath);

                            var currentThumbnailsPath = currentPalettePath.Substring(0, currentPalettePath.Length - 4);
                            if (!System.IO.Directory.Exists(currentThumbnailsPath)) continue;
                            var thumbnailsDirName = fileName.Substring(0, fileName.Length - 4);
                            var newThumbnailPath = newPalettesDir + "/" + thumbnailsDirName;
                            if (System.IO.Directory.Exists(newThumbnailPath)) System.IO.Directory.Delete(newThumbnailPath);
                            DeleteMeta(currentThumbnailsPath);
                            System.IO.Directory.Move(currentThumbnailsPath, newThumbnailPath);
                        }
                    }
                    if (DeleteIfEmpty(currentPalettesDir)) DeleteIfEmpty(currentFullDir);
                    PWBCore.AssetDatabaseRefresh();
                }
                _movingDir = false;
            }
            instance._dataDir = PWBCore.GetRelativePath(fullPath);
            instance.Save();
            PaletteManager.instance.LoadPaletteFiles(true);
            PrefabPalette.UpdateTabBar();
        }
        public static string fullDataDir => PWBCore.GetFullPath(relativeDataDir);
        #endregion

        #region SHORTCUTS
        [SerializeField]
        private System.Collections.Generic.List<PWBShortcuts> _shortcutProfiles
           = new System.Collections.Generic.List<PWBShortcuts>()
           {
                PWBShortcuts.GetDefault(0),
                PWBShortcuts.GetDefault(1),
                PWBShortcuts.GetDefault(2)
           };
        [SerializeField] private int _selectedProfileIdx = 2;
        public static System.Action OnShrotcutProfileChanged;
        private PWBShortcuts selectedProfile
        {
            get
            {
                if (_selectedProfileIdx < 0 || _selectedProfileIdx > _shortcutProfiles.Count) _selectedProfileIdx = 0;
                return _shortcutProfiles[_selectedProfileIdx];
            }
        }

        public static PWBShortcuts shortcuts
        {
            get
            {
                CheckDataDir();
                return instance.selectedProfile;
            }
        }

        public static string[] shotcutProfileNames
        {
            get
            {
                CheckDataDir();
                return instance._shortcutProfiles.Select(p => p.profileName).ToArray();
            }
        }

        public static int selectedProfileIdx
        {
            get
            {
                CheckDataDir();
                return instance._selectedProfileIdx;
            }
            set
            {
                CheckDataDir();
                if (value == instance._selectedProfileIdx) return;
                instance._selectedProfileIdx = value;
                instance.Save();
                if (OnShrotcutProfileChanged != null) OnShrotcutProfileChanged();
            }
        }

        public static void UpdateShrotcutsConflictsAndSaveFile()
        {
            CheckDataDir();
            shortcuts.UpdateConficts();
            shortcuts.UpdateMouseConficts();
            instance.Save();
        }

        public static bool GetShortcutConflict(PWBKeyShortcut shortcut1, out PWBKeyShortcut shortcut2)
        {
            return shortcuts.GetConflictShortcuts(shortcut1, out shortcut2);
        }

        public static void ResetSelectedProfile()
        {
            CheckDataDir();
            instance._shortcutProfiles[selectedProfileIdx].Copy(PWBShortcuts.GetDefault(selectedProfileIdx));
        }

        public static void ResetShortcutToDefault(PWBKeyShortcut shortcut)
        {
            var defaultProfile = selectedProfileIdx == 1 ? PWBShortcuts.GetDefault(1) : PWBShortcuts.GetDefault(0);
            foreach (var ds in defaultProfile.keyShortcuts)
            {
                if (ds.group == shortcut.group && ds.name == shortcut.name)
                {
                    shortcut.combination.Set(ds.combination.keyCode, ds.combination.modifiers);
                    return;
                }
            }
        }

        public static void ResetShortcutToDefault(PWBMouseShortcut shortcut)
        {
            var defaultProfile = selectedProfileIdx == 1 ? PWBShortcuts.GetDefault(1) : PWBShortcuts.GetDefault(0);
            foreach (var ds in defaultProfile.mouseShortcuts)
            {
                if (ds.group == shortcut.group && ds.name == shortcut.name)
                {
                    shortcut.combination.Set(ds.combination.modifiers, ds.combination.mouseEvent);
                    return;
                }
            }
        }

        public static void ResetShortcutToDefault(PWBShortcut shortcut)
        {
            if (shortcut is PWBKeyShortcut) ResetShortcutToDefault(shortcut as PWBKeyShortcut);
            else if (shortcut is PWBMouseShortcut) ResetShortcutToDefault(shortcut as PWBMouseShortcut);
        }

        #endregion
    }
    #endregion

    #region HANDLERS
    [UnityEditor.InitializeOnLoad]
    public static class ApplicationEventHandler
    {
        private static bool _importingPackage = false;
        public static bool importingPackage => _importingPackage;
        private static bool _refreshOnImportingCancelled = false;
        public static bool RefreshOnImportingCancelled() => _refreshOnImportingCancelled = true;

        private static bool _sceneOpening = false;
        public static bool sceneOpening => _sceneOpening;

        private static bool _hierarchyChangedWhileUsingTools = false;
        public static bool hierarchyChangedWhileUsingTools
        { get => _hierarchyChangedWhileUsingTools; set => _hierarchyChangedWhileUsingTools = value; }

        static ApplicationEventHandler()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnStateChanged;
            UnityEditor.EditorApplication.hierarchyChanged += OnHierarchyChanged;
            UnityEditor.AssetDatabase.importPackageStarted += OnImportPackageStarted;
            UnityEditor.AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
            UnityEditor.AssetDatabase.importPackageCancelled += OnImportPackageCancelled;
            UnityEditor.AssetDatabase.importPackageFailed += OnImportPackageFailed;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpening += OnSceneOpening;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneOpened;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                PWBCore.Initialize();
                const string firstInitSessionStateKey = "PWBFirstInitDone";
                if (!UnityEditor.SessionState.GetBool(firstInitSessionStateKey, false))
                {
                    UnityEditor.SessionState.SetBool(firstInitSessionStateKey, true);
                }
            };
        }
        private static void OnSceneOpening(string path, UnityEditor.SceneManagement.OpenSceneMode mode)
            => _sceneOpening = true;

        private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene,
            UnityEditor.SceneManagement.OpenSceneMode mode)
            => _sceneOpening = false;

        private static void OnHierarchyChanged()
        {
            if (PWBCore.updatingTempColliders || PWBIO.painting)
            {
                if (PWBCore.updatingTempColliders) PWBCore.updatingTempColliders = false;
                if (PWBIO.painting) PWBIO.painting = false;
                return;
            }
            if (ToolManager.tool != ToolManager.PaintTool.NONE)
                hierarchyChangedWhileUsingTools = true;
            else
            {
                PWBIO.ClearPreviewDictionaries();
            }
        }

        private static void OnStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode
                || state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                PWBCore.staticData.SaveIfPending();
        }

        private static void OnImportPackageStarted(string packageName) => _importingPackage = true;
        private static void OnImportPackageCompleted(string packageName) => _importingPackage = false;
        private static void OnImportPackageCancelled(string packageName)
        {
            if (_refreshOnImportingCancelled)
            {
                UnityEditor.AssetDatabase.Refresh();
                _refreshOnImportingCancelled = false;
            }
            _importingPackage = false;
        }
        private static void OnImportPackageFailed(string packageName, string errorMessage) => _importingPackage = false;
    }

    public class DataReimportHandler : UnityEditor.AssetPostprocessor
    {
        private static bool _importingAssets = false;
        public static bool importingAssets => _importingAssets;
        void OnPreprocessAsset() => _importingAssets = true;
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            _importingAssets = false;
            if (PWBSettings.movingDir) return;
            if (PWBCore.staticData.saving) return;
            if (!PWBData.palettesDirectory.Contains(Application.dataPath)) return;
            if (PaletteManager.addingPalettes)
            {
                PaletteManager.addingPalettes = false;
                return;
            }
            var paths = new System.Collections.Generic.List<string>(importedAssets);
            paths.AddRange(deletedAssets);
            paths.AddRange(movedAssets);
            paths.AddRange(movedFromAssetPaths);

            var relativeDataPath = PWBSettings.relativeDataDir.Replace(Application.dataPath, string.Empty);
            if (paths.Exists(p => p.Contains(relativeDataPath) && System.IO.Path.GetExtension(p) == ".txt"))
            {
                if (PaletteManager.selectedPalette != null && PaletteManager.selectedPalette.saving)
                {
                    PaletteManager.selectedPalette.StopSaving();
                    return;
                }
            }
        }
    }


    #endregion

    #region AUTOSAVE
    [UnityEditor.InitializeOnLoad]
    public static class AutoSave
    {
        private static int _quickSaveCount = 3;

        static AutoSave()
        {
            PWBCore.staticData.UpdateRootDirectory();
            PeriodicSave();
            PeriodicQuickSave();
        }
        private async static void PeriodicSave()
        {
            if (PWBCore.staticDataWasInitialized)
            {
                await System.Threading.Tasks.Task.Delay(PWBCore.staticData.autoSavePeriodMinutes * 60000);
                PWBCore.staticData.SaveIfPending();
            }
            else await System.Threading.Tasks.Task.Delay(60000);
            PeriodicSave();
        }

        private async static void PeriodicQuickSave()
        {
            await System.Threading.Tasks.Task.Delay(300);
            ++_quickSaveCount;
            if (_quickSaveCount == 3 && PWBCore.staticDataWasInitialized)
                PWBCore.staticData.SaveIfPending();
            PeriodicQuickSave();
        }

        public static void QuickSave() => _quickSaveCount = 0;
    }
    #endregion

    #region VERSION
    [System.Serializable]
    public class PWBDataVersion
    {
        [SerializeField] public string _version;
        public bool IsOlderThan(string value) => IsOlderThan(value, _version);

        public static bool IsOlderThan(string value, string referenceValue)
        {
            var intArray = GetIntArray(referenceValue);
            var otherIntArray = GetIntArray(value);
            var minLength = Mathf.Min(intArray.Length, otherIntArray.Length);
            for (int i = 0; i < minLength; ++i)
            {
                if (intArray[i] < otherIntArray[i]) return true;
                else if (intArray[i] > otherIntArray[i]) return false;
            }
            return false;
        }
        private static int[] GetIntArray(string value)
        {
            var stringArray = value.Split('.');
            if (stringArray.Length == 0) return new int[] { 1, 0 };
            var intArray = new int[stringArray.Length];
            for (int i = 0; i < intArray.Length; ++i) intArray[i] = int.Parse(stringArray[i]);
            return intArray;
        }
    }
    #endregion

    #region DATA 2.8
    [System.Serializable]
    public class V2_8_PaletteManager
    {
        [SerializeField] public PaletteData[] _paletteData;
    }
    [System.Serializable]
    public class V2_8_PWBData
    {
        [SerializeField] public V2_8_PaletteManager _paletteManager;
    }
    #endregion

    #region DATA 4.0
    [System.Serializable]
    public struct V4_0_ObjectPose
    {
        [SerializeField] public ObjectId _id;
        [SerializeField] public Vector3 _position;
        [SerializeField] public Quaternion _localRotation;
        [SerializeField] public Vector3 _localScale;
    }
    [System.Serializable]
    public struct V4_0_ToolData
    {
        [SerializeField] public long _id;
        [SerializeField] public V4_0_ObjectPose[] _objectPoses;
    }
    [System.Serializable]
    public struct V4_0_SceneData
    {
        [SerializeField] public V4_0_ToolData[] _items;
    }
    [System.Serializable]
    public struct V4_0_ToolManager
    {
        [SerializeField] public V4_0_SceneData[] _sceneItems;
    }
    [System.Serializable]
    public struct V4_0_PWBData
    {
        [SerializeField] public PinManager pinManager;
        [SerializeField] public V4_0_ToolManager _lineManager;
        [SerializeField] public V4_0_ToolManager _shapeManager;
        [SerializeField] public V4_0_ToolManager _tilingManager;
    }
    #endregion
}