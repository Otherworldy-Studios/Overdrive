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
    #region BRUSH SETTINGS

    [System.Serializable]
    public class BrushSettings : ISerializationCallbackReceiver
    {
        [SerializeField] private long _id = -1;
        [SerializeField] private float _surfaceDistance = 0f;
        [SerializeField] private bool _randomSurfaceDistance = false;
        [SerializeField] private RandomUtils.Range _randomSurfaceDistanceRange = new RandomUtils.Range(-0.005f, 0.005f);
        [SerializeField] protected bool _embedInSurface = false;
        [SerializeField] protected bool _embedAtPivotHeight = true;
        [SerializeField] protected Vector3 _localPositionOffset = Vector3.zero;
        [SerializeField] protected bool _rotateToTheSurface = true;
        [SerializeField] private Vector3 _eulerOffset = Vector3.zero;
        [SerializeField] private bool _addRandomRotation = false;
        [SerializeField] private float _rotationFactor = 90;
        [SerializeField] private bool _rotateInMultiples = false;
        [SerializeField]
        private RandomUtils.Range3 _randomEulerOffset = new RandomUtils.Range3(Vector3.zero, Vector3.zero);
        [SerializeField] private bool _alwaysOrientUp = false;
        [SerializeField] private bool _separateScaleAxes = false;
        [SerializeField] private Vector3 _scaleMultiplier = Vector3.one;
        [SerializeField] private bool _randomScaleMultiplier = false;
        [SerializeField]
        private RandomUtils.Range3 _randomScaleMultiplierRange = new RandomUtils.Range3(Vector3.one, Vector3.one);

        public enum FlipAction { NONE, FLIP, RANDOM }
        [SerializeField] private FlipAction _flipX = FlipAction.NONE;
        [SerializeField] private FlipAction _flipY = FlipAction.NONE;

        [SerializeField] private ThumbnailSettings _thumbnailSettings = new ThumbnailSettings();
        [field: System.NonSerialized] private Texture2D _thumbnail = null;
        public System.Action OnDataChangedAction;
        public static System.Action OnBrushSettingsChanged;
        protected virtual void OnDataChanged()
        {
            if (OnDataChangedAction != null) OnDataChangedAction();
            if (OnBrushSettingsChanged != null) OnBrushSettingsChanged();
        }
        public long id => _id;
        public virtual float surfaceDistance
        {
            get => _surfaceDistance;
            set
            {
                if (_surfaceDistance == value) return;
                _surfaceDistance = value;
                OnDataChanged();
            }
        }

        public virtual bool randomSurfaceDistance
        {
            get => _randomSurfaceDistance;
            set
            {
                if (_randomSurfaceDistance == value) return;
                _randomSurfaceDistance = value;
                OnDataChanged();
            }
        }

        public virtual RandomUtils.Range randomSurfaceDistanceRange
        {
            get => _randomSurfaceDistanceRange;
            set
            {
                if (_randomSurfaceDistanceRange == value) return;
                _randomSurfaceDistanceRange = value;
                OnDataChanged();
            }
        }
        public virtual bool embedInSurface
        {
            get => _embedInSurface;
            set
            {
                if (_embedInSurface == value) return;
                _embedInSurface = value;
                OnDataChanged();
            }
        }
        public virtual bool embedAtPivotHeight
        {
            get => _embedAtPivotHeight;
            set
            {
                if (_embedAtPivotHeight == value) return;
                _embedAtPivotHeight = value;
                OnDataChanged();
            }
        }
        public virtual void UpdateBottomVertices() { }

        public virtual Vector3 localPositionOffset
        {
            get => _localPositionOffset;
            set
            {
                if (_localPositionOffset == value) return;
                _localPositionOffset = value;
                OnDataChanged();
            }
        }

        public void SetLocalPositionOffset(float value, AxesUtils.Axis axis)
        {
#if UNITY_2021_1_OR_NEWER
            value = System.MathF.Round(value, digits: 5);
#else
            value = (float)System.Math.Round(value, digits: 5);
#endif
            var currentValue = AxesUtils.GetAxisValue(_localPositionOffset, axis);
            if (currentValue == value) return;
            AxesUtils.SetAxisValue(ref _localPositionOffset, axis, value);
            OnDataChanged();
        }
        public virtual bool rotateToTheSurface
        {
            get => _rotateToTheSurface;
            set
            {
                if (_rotateToTheSurface == value) return;
                _rotateToTheSurface = value;
                OnDataChanged();
            }
        }
        public virtual Vector3 eulerOffset
        {
            get => _eulerOffset;
            set
            {
                if (_eulerOffset == value) return;
                _eulerOffset = value;
                _randomEulerOffset.v1 = _randomEulerOffset.v2 = Vector3.zero;
                OnDataChanged();
            }
        }
        public virtual bool addRandomRotation
        {
            get => _addRandomRotation;
            set
            {
                if (_addRandomRotation == value) return;
                _addRandomRotation = value;
                OnDataChanged();
            }
        }
        public virtual float rotationFactor
        {
            get => _rotationFactor;
            set
            {
                value = Mathf.Max(value, 0f);
                if (_rotationFactor == value) return;
                _rotationFactor = value;
                OnDataChanged();
            }
        }
        public virtual bool rotateInMultiples
        {
            get => _rotateInMultiples;
            set
            {
                if (_rotateInMultiples == value) return;
                _rotateInMultiples = value;
                OnDataChanged();
            }
        }
        public virtual RandomUtils.Range3 randomEulerOffset
        {
            get => _randomEulerOffset;
            set
            {
                if (_randomEulerOffset == value) return;
                _randomEulerOffset = value;
                _eulerOffset = Vector3.zero;
                OnDataChanged();
            }
        }
        public virtual bool alwaysOrientUp
        {
            get => rotateToTheSurface ? _alwaysOrientUp : false;
            set
            {
                if (_alwaysOrientUp == value) return;
                _alwaysOrientUp = value;
                OnDataChanged();
            }
        }
        public virtual bool separateScaleAxes
        {
            get => _separateScaleAxes;
            set
            {
                if (_separateScaleAxes == value) return;
                _separateScaleAxes = value;
                OnDataChanged();
            }
        }
        public virtual Vector3 scaleMultiplier
        {
            get => _scaleMultiplier;
            set
            {
                if (Mathf.Approximately(value.x, 0) && Mathf.Approximately(value.y, 0) && Mathf.Approximately(value.z, 0))
                    return;
                if (_scaleMultiplier == value) return;
                _scaleMultiplier = value;
                _randomScaleMultiplierRange.v1 = _randomScaleMultiplierRange.v2 = Vector3.one;
                OnDataChanged();
            }
        }
        public virtual RandomUtils.Range3 randomScaleMultiplierRange
        {
            get => _randomScaleMultiplierRange;
            set
            {
                if (_randomScaleMultiplierRange == value) return;
                _randomScaleMultiplierRange = value;
                _scaleMultiplier = Vector3.one;
                OnDataChanged();
            }
        }
        public virtual bool randomScaleMultiplier
        {
            get => _randomScaleMultiplier;
            set
            {
                if (_randomScaleMultiplier == value) return;
                _randomScaleMultiplier = value;
                _randomScaleMultiplierRange.v1 = _randomScaleMultiplierRange.v2 = _scaleMultiplier = Vector3.one;
                OnDataChanged();
            }
        }

        public virtual bool isAsset2D { get; set; }
        public virtual FlipAction flipX
        {
            get => _flipX;
            set
            {
                if (_flipX == value) return;
                _flipX = value;
                OnDataChanged();
            }
        }
        public virtual FlipAction flipY
        {
            get => _flipY;
            set
            {
                if (_flipY == value) return;
                _flipY = value;
                OnDataChanged();
            }
        }


        public virtual Vector3 maxScaleMultiplier
            => randomScaleMultiplier ? randomScaleMultiplierRange.max : scaleMultiplier;
        public virtual Vector3 minScaleMultiplier
            => randomScaleMultiplier ? randomScaleMultiplierRange.min : scaleMultiplier;

        public Vector3 GetAdditionalAngle()
        {
            if (addRandomRotation)
            {
                var randomAngle = randomEulerOffset.randomVector;
                if (rotateInMultiples)
                {
                    randomAngle = new Vector3(
                        Mathf.Round(randomAngle.x / rotationFactor) * rotationFactor,
                        Mathf.Round(randomAngle.y / rotationFactor) * rotationFactor,
                        Mathf.Round(randomAngle.z / rotationFactor) * rotationFactor);
                }
                return randomAngle;
            }
            return eulerOffset;
        }

        public Vector3 GetScaleMultiplier()
        {
            var scale = randomScaleMultiplier ? randomScaleMultiplierRange.randomVector : scaleMultiplier;
            if (!separateScaleAxes) scale.z = scale.y = scale.x;
            return scale;
        }

        public bool GetFlipX() => flipX == FlipAction.NONE ? false : flipX == FlipAction.FLIP ? true : Random.value > 0.5;
        public bool GetFlipY() => flipY == FlipAction.NONE ? false : flipY == FlipAction.FLIP ? true : Random.value > 0.5;
        public float GetSurfaceDistance() => randomSurfaceDistance ? randomSurfaceDistanceRange.randomValue : surfaceDistance;

        public virtual string thumbnailPath { get; }
        public virtual ThumbnailSettings thumbnailSettings
        {
            get => _thumbnailSettings;
            set => _thumbnailSettings.Copy(value);
        }

        public Texture2D thumbnail
        {
            get
            {
                if (_thumbnail == null) LoadThumbnailFromFile();
                if (_thumbnail == null) UpdateThumbnail(updateItemThumbnails: true, savePng: true);
                return _thumbnail;
            }
        }

        public void LoadThumbnailFromFile()
        {
            var filePath = thumbnailPath;
            if (filePath != null)
            {
                if (System.IO.File.Exists(filePath))
                {
                    var fileData = System.IO.File.ReadAllBytes(filePath);
                    _thumbnail = new Texture2D(ThumbnailUtils.SIZE, ThumbnailUtils.SIZE);
                    _thumbnail.LoadImage(fileData);
                }
                else
                {
                    _thumbnailSettings.useCustomImage = false;
                    UpdateThumbnail(updateItemThumbnails: true, savePng: true);
                }
            }
        }

        public Texture2D thumbnailTexture
        {
            get
            {
                if (_thumbnail == null) _thumbnail = new Texture2D(ThumbnailUtils.SIZE, ThumbnailUtils.SIZE);
                return _thumbnail;
            }
        }

        public void SetCustomThumbnailTexture(Texture2D customThumbnailTexture, bool savePng)
        {
            _thumbnail = customThumbnailTexture;
            if (savePng) ThumbnailUtils.SavePngResource(_thumbnail, thumbnailPath);
        }


        public void UpdateThumbnail(bool updateItemThumbnails, bool savePng)
            => ThumbnailUtils.UpdateThumbnail(brushItem: this, updateItemThumbnails, savePng);

        public virtual BrushSettings Clone()
        {
            var clone = new BrushSettings();
            clone.Copy(this);
            if (_thumbnail != null)
            {
                var thumbnailClone = new Texture2D(_thumbnail.width, _thumbnail.height, _thumbnail.format, false);
                thumbnailClone.SetPixels(_thumbnail.GetPixels());
                thumbnailClone.Apply();
                clone._thumbnail = thumbnailClone;
            }
            return clone;
        }

        public virtual void Copy(BrushSettings other)
        {
            _surfaceDistance = other._surfaceDistance;
            _randomSurfaceDistance = other._randomSurfaceDistance;
            _randomSurfaceDistanceRange = other._randomSurfaceDistanceRange;
            _embedInSurface = other._embedInSurface;
            _embedAtPivotHeight = other._embedAtPivotHeight;
            _localPositionOffset = other._localPositionOffset;
            _rotateToTheSurface = other._rotateToTheSurface;
            _addRandomRotation = other._addRandomRotation;
            _eulerOffset = other._eulerOffset;
            _randomEulerOffset = new RandomUtils.Range3(other._randomEulerOffset);
            _randomScaleMultiplier = other._randomScaleMultiplier;
            _alwaysOrientUp = other._alwaysOrientUp;
            _separateScaleAxes = other._separateScaleAxes;
            _scaleMultiplier = other._scaleMultiplier;
            _randomScaleMultiplierRange = new RandomUtils.Range3(other._randomScaleMultiplierRange);
            _thumbnailSettings.Copy(other._thumbnailSettings);
            _rotationFactor = other._rotationFactor;
            _rotateInMultiples = other._rotateInMultiples;
            _flipX = other._flipX;
            _flipY = other._flipY;
        }
        public void Reset()
        {
            _surfaceDistance = 0f;
            _randomSurfaceDistance = false;
            _randomSurfaceDistanceRange = new RandomUtils.Range(-0.005f, 0.005f);
            _embedInSurface = false;
            _embedAtPivotHeight = true;
            _localPositionOffset = Vector3.zero;
            _rotateToTheSurface = true;
            _addRandomRotation = false;
            _eulerOffset = Vector3.zero;
            _randomEulerOffset = new RandomUtils.Range3(Vector3.zero, Vector3.zero);
            _randomScaleMultiplier = false;
            _alwaysOrientUp = false;
            _separateScaleAxes = false;
            _scaleMultiplier = Vector3.one;
            _randomScaleMultiplierRange = new RandomUtils.Range3(Vector3.one, Vector3.one);
            _thumbnailSettings = new ThumbnailSettings();
            _rotationFactor = 90;
            _rotateInMultiples = false;
            _flipX = FlipAction.NONE;
            _flipY = FlipAction.NONE;
        }

        private static long _prevId = 0;
        protected void SetId()
        {
            _id = System.DateTime.Now.Ticks;
            if (_id <= _prevId) _id = _prevId + 1;
            _prevId = _id;
        }
        public BrushSettings() { }
        public BrushSettings(BrushSettings other) => Copy(other);

        public virtual void OnBeforeSerialize() { }

        public virtual void OnAfterDeserialize() { }

        public override int GetHashCode()
        {
            int hashCode = 917907199;
            hashCode = hashCode * -1521134295 + _id.GetHashCode();
            hashCode = hashCode * -1521134295 + _surfaceDistance.GetHashCode();
            hashCode = hashCode * -1521134295 + _randomSurfaceDistance.GetHashCode();
            hashCode = hashCode * -1521134295 + _randomSurfaceDistanceRange.GetHashCode();
            hashCode = hashCode * -1521134295 + _embedInSurface.GetHashCode();
            hashCode = hashCode * -1521134295 + _embedAtPivotHeight.GetHashCode();
            hashCode = hashCode * -1521134295 + _localPositionOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + _rotateToTheSurface.GetHashCode();
            hashCode = hashCode * -1521134295 + _eulerOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + _addRandomRotation.GetHashCode();
            hashCode = hashCode * -1521134295 + _rotationFactor.GetHashCode();
            hashCode = hashCode * -1521134295 + _rotateInMultiples.GetHashCode();
            hashCode = hashCode * -1521134295 + _randomEulerOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + _alwaysOrientUp.GetHashCode();
            hashCode = hashCode * -1521134295 + _separateScaleAxes.GetHashCode();
            hashCode = hashCode * -1521134295 + _scaleMultiplier.GetHashCode();
            hashCode = hashCode * -1521134295 + _randomScaleMultiplier.GetHashCode();
            hashCode = hashCode * -1521134295 + _randomScaleMultiplier.GetHashCode();
            hashCode = hashCode * -1521134295 + _flipX.GetHashCode();
            hashCode = hashCode * -1521134295 + _flipY.GetHashCode();
            hashCode = hashCode * -1521134295 + _thumbnailSettings.GetHashCode();
            return hashCode;
        }
    }

    public static class SelectionUtils
    {
        public static void Swap<T>(int fromIdx, int toIdx, ref int[] selection, System.Collections.Generic.List<T> list)
        {
            if (fromIdx == toIdx) return;
            var newOrder = new System.Collections.Generic.List<T>();
            var newSelection = selection.ToArray();
            for (int idx = 0; idx <= list.Count; ++idx)
            {
                if (idx == toIdx)
                {
                    System.Array.Sort(selection);
                    int newSelectionIdx = 0;
                    foreach (var selectionIdx in selection)
                    {
                        newOrder.Add(list[selectionIdx]);
                        newSelection[newSelectionIdx++] = newOrder.Count - 1;
                    }
                    if (idx < list.Count && !selection.Contains(idx)) newOrder.Add(list[idx]);
                }
                else if (selection.Contains(idx)) continue;
                else if (idx < list.Count) newOrder.Add(list[idx]);
            }
            selection = newSelection;
            list.Clear();
            list.AddRange(newOrder);
            PWBCore.staticData.Save();
        }
    }

    [System.Serializable]
    public class ThumbnailSettings
    {
        [SerializeField] private Color _backgroudColor = Color.gray;
        [SerializeField] private Vector2 _lightEuler = new Vector2(130, -165);
        [SerializeField] private Color _lightColor = Color.white;
        [SerializeField] private float _lightIntensity = 1;
        [SerializeField] private float _zoom = 1;
        [SerializeField] private Vector3 _targetEuler = new Vector3(0, 125, 0);
        [SerializeField] private Vector2 _targetOffset = Vector2.zero;
        [SerializeField] private bool _useCustomImage = false;

        public Color backgroudColor { get => _backgroudColor; set => _backgroudColor = value; }
        public Vector2 lightEuler { get => _lightEuler; set => _lightEuler = value; }
        public Color lightColor { get => _lightColor; set => _lightColor = value; }
        public float lightIntensity { get => _lightIntensity; set => _lightIntensity = value; }
        public float zoom { get => _zoom; set => _zoom = value; }
        public Vector3 targetEuler { get => _targetEuler; set => _targetEuler = value; }
        public Vector2 targetOffset { get => _targetOffset; set => _targetOffset = value; }
        public bool useCustomImage { get => _useCustomImage; set => _useCustomImage = value; }
        public ThumbnailSettings() { }

        private static ThumbnailSettings _defaultTAsset2DThumbnailSettings = null;
        public static ThumbnailSettings GetDefaultTAsset2DThumbnailSettings()
        {
            if (_defaultTAsset2DThumbnailSettings == null)
            {
                _defaultTAsset2DThumbnailSettings = new ThumbnailSettings();
                _defaultTAsset2DThumbnailSettings.targetEuler = new Vector3(17.5f, 0f, 0f);
                _defaultTAsset2DThumbnailSettings.zoom = 1.47f;
                _defaultTAsset2DThumbnailSettings.targetOffset = new Vector2(0f, -0.06f);
            }
            return _defaultTAsset2DThumbnailSettings;
        }
        public ThumbnailSettings(Color backgroudColor, Vector3 lightEuler, Color lightColor, float lightIntensity,
            float zoom, Vector3 targetEuler, Vector2 targetOffset, bool useCustomImage)
        {
            _backgroudColor = backgroudColor;
            _lightEuler = lightEuler;
            _lightColor = lightColor;
            _lightIntensity = lightIntensity;
            _zoom = zoom;
            _targetEuler = targetEuler;
            _targetOffset = targetOffset;
            _useCustomImage = useCustomImage;
        }

        public ThumbnailSettings(ThumbnailSettings other) => Copy(other);
        public void Copy(ThumbnailSettings other)
        {
            _backgroudColor = other._backgroudColor;
            _lightEuler = other._lightEuler;
            _lightColor = other._lightColor;
            _lightIntensity = other._lightIntensity;
            _zoom = other._zoom;
            _targetEuler = other._targetEuler;
            _targetOffset = other._targetOffset;
            _useCustomImage = other._useCustomImage;
        }

        public ThumbnailSettings Clone()
        {
            var clone = new ThumbnailSettings();
            clone.Copy(this);
            return clone;
        }

        public override int GetHashCode()
        {
            int hashCode = 917907199;
            hashCode = hashCode * -1521134295 + _backgroudColor.GetHashCode();
            hashCode = hashCode * -1521134295 + _lightEuler.GetHashCode();
            hashCode = hashCode * -1521134295 + _lightColor.GetHashCode();
            hashCode = hashCode * -1521134295 + _lightIntensity.GetHashCode();
            hashCode = hashCode * -1521134295 + _zoom.GetHashCode();
            hashCode = hashCode * -1521134295 + _targetEuler.GetHashCode();
            hashCode = hashCode * -1521134295 + _targetOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + _useCustomImage.GetHashCode();
            return hashCode;
        }
    }

    [System.Serializable]
    public class MultibrushItemSettings : BrushSettings
    {
        [SerializeField] private bool _overwriteSettings = false;
        [SerializeField] private string _guid = string.Empty;
        [SerializeField] private string _prefabPath = string.Empty;
        [SerializeField] private float _frequency = 1;
        [SerializeField] private long _parentId = -1;
        [SerializeField] private bool _overwriteThumbnailSettings = false;
        [SerializeField] private bool _includeInThumbnail = true;
        [SerializeField] private bool _isAsset2D = false;

        //[SerializeField] private Vector3 _boundingBoxCenterOffset = Vector3.zero;
        //[SerializeField] private Vector3 _boundingBoxScale = Vector3.one;

        private Vector3[] _bottomVertices = null;
        private float _bottomMagnitude = 0;
        private float _height = 1f;
        private Vector3 _size = Vector3.zero;
        private GameObject _prefab = null;

        private System.Collections.Generic.Dictionary<Vector3, (Vector3[] vertices, float magnitude)>
            _furthestVerticesInDirection = new System.Collections.Generic.Dictionary<Vector3,
                (Vector3[] vertices, float magnitude)>();
        private System.Collections.Generic.List<Vector3> _directionList = new System.Collections.Generic.List<Vector3>();

        [System.NonSerialized] private MultibrushSettings _parentSettings = null;
        public MultibrushSettings parentSettings
        {
            get
            {
                if (_parentSettings == null) _parentSettings = PaletteManager.GetBrushById(_parentId);
                return _parentSettings;
            }
            set
            {
                if (value == null)
                {
                    _parentId = -1;
                    _parentSettings = null;
                    return;
                }
                _parentSettings = value;
                _parentId = value.id;
            }
        }
        protected override void OnDataChanged()
        {
            base.OnDataChanged();
            SavePalette();
        }
        private void SavePalette()
        {
            if (parentSettings == null) return;
            parentSettings.SavePalette();
        }
        public MultibrushItemSettings(GameObject prefab, MultibrushSettings parentSettings) : base()
        {
            SetId();
            _prefab = prefab;
            _parentId = parentSettings.id;
            _parentSettings = parentSettings;
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_prefab, out _guid, out long localId);
            if (_prefab == null) return;
            _prefabPath = UnityEditor.AssetDatabase.GetAssetPath(_prefab);
            _bottomVertices = BoundsUtils.GetBottomVertices(prefab.transform);
            _height = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation).size.y;
            _size = BoundsUtils.GetBoundsRecursive(prefab.transform).size;
            _bottomMagnitude = BoundsUtils.GetBottomMagnitude(prefab.transform);
            UpdateAssetType();
            if (isAsset2D)
            {
                thumbnailSettings.Copy(ThumbnailSettings.GetDefaultTAsset2DThumbnailSettings());
                _rotateToTheSurface = false;
            }
            UpdateThumbnail(updateItemThumbnails: false, savePng: true);
        }

        public void InitializeParentSettings(MultibrushSettings parentSettings)
        {
            _parentId = parentSettings.id;
            _parentSettings = parentSettings;
            this.parentSettings.UpdateTotalFrequency();
        }

        public override string thumbnailPath
        {
            get
            {
                if (parentSettings == null) return null;
                var parentPath = parentSettings.thumbnailPath;
                if (parentPath == null) return null;
                var path = parentPath.Insert(parentPath.Length - 4, "_" + id.ToString("X"));
                return path;
            }
        }

        public bool overwriteSettings
        {
            get => _overwriteSettings;
            set
            {
                if (_overwriteSettings == value) return;
                _overwriteSettings = value;
                SavePalette();
            }
        }

        public float frequency
        {
            get => _frequency;
            set
            {
                value = Mathf.Max(value, 0);
                if (_frequency == value) return;
                _frequency = value;
                if (parentSettings != null) parentSettings.UpdateTotalFrequency();
                OnDataChanged();
            }
        }
        public GameObject prefab
        {
            get
            {
                if (_prefab == null)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(_guid);
                    if (!string.IsNullOrEmpty(path))
                    {
                        _prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (_prefab != null)
                        {
                            _prefabPath = path;
                            return _prefab;
                        }
                    }
                    if (_prefab == null && !string.IsNullOrEmpty(_prefabPath))
                    {
                        _prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
                        if (_prefab != null)
                        {
                            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_prefab, out _guid, out long localId);
                            return _prefab;
                        }
                    }
                    Debug.LogWarning($"Missing prefab at: {_prefabPath}. " +
                        "It is recommended to run 'Cleanup Palette' from the palette settings menu.");
                    return null;
                }
                else
                {
                    var assetPath = UnityEditor.AssetDatabase.GetAssetPath(_prefab);
                    if (string.IsNullOrEmpty(assetPath)) _prefab = null;
                    else
                    {
                        _prefabPath = assetPath;
                        return _prefab;
                    }
                }
                return _prefab;
            }
        }
        public string guid => _guid;
        public string prefabPath => _prefabPath;
        public override float surfaceDistance
            => _overwriteSettings || parentSettings == null ? base.surfaceDistance : parentSettings.surfaceDistance;

        public override bool randomSurfaceDistance
            => _overwriteSettings || parentSettings == null
            ? base.randomSurfaceDistance : parentSettings.randomSurfaceDistance;

        public override RandomUtils.Range randomSurfaceDistanceRange
            => _overwriteSettings || parentSettings == null
            ? base.randomSurfaceDistanceRange : parentSettings.randomSurfaceDistanceRange;

        public override bool embedInSurface
        {
            get => _overwriteSettings || parentSettings == null ? base.embedInSurface
                : parentSettings.embedInSurface;
            set
            {
                if (_embedInSurface == value) return;
                _embedInSurface = value;
                if (_embedInSurface) UpdateBottomVertices();
            }
        }

        public override bool embedAtPivotHeight
        {
            get => _overwriteSettings || parentSettings == null ? base.embedAtPivotHeight : parentSettings.embedAtPivotHeight;
            set
            {
                if (_embedAtPivotHeight == value) return;
                _embedAtPivotHeight = value;
            }
        }

        public override Vector3 localPositionOffset
            => _overwriteSettings || parentSettings == null ? base.localPositionOffset : parentSettings.localPositionOffset;
        public override bool rotateToTheSurface
            => _overwriteSettings || parentSettings == null ? base.rotateToTheSurface : parentSettings.rotateToTheSurface;
        public override Vector3 eulerOffset
            => _overwriteSettings || parentSettings == null ? base.eulerOffset : parentSettings.eulerOffset;
        public override bool addRandomRotation
            => _overwriteSettings || parentSettings == null ? base.addRandomRotation : parentSettings.addRandomRotation;
        public override RandomUtils.Range3 randomEulerOffset
            => _overwriteSettings || parentSettings == null ? base.randomEulerOffset : parentSettings.randomEulerOffset;
        public override float rotationFactor
            => _overwriteSettings || parentSettings == null ? base.rotationFactor : parentSettings.rotationFactor;
        public override bool rotateInMultiples
            => _overwriteSettings || parentSettings == null ? base.rotateInMultiples : parentSettings.rotateInMultiples;
        public override bool alwaysOrientUp
            => _overwriteSettings || parentSettings == null ? base.alwaysOrientUp : parentSettings.alwaysOrientUp;
        public override bool separateScaleAxes
            => _overwriteSettings || parentSettings == null ? base.separateScaleAxes : parentSettings.separateScaleAxes;
        public override Vector3 scaleMultiplier
            => _overwriteSettings || parentSettings == null ? base.scaleMultiplier : parentSettings.scaleMultiplier;
        public override RandomUtils.Range3 randomScaleMultiplierRange
            => _overwriteSettings || parentSettings == null ? base.randomScaleMultiplierRange
            : parentSettings.randomScaleMultiplierRange;
        public override bool randomScaleMultiplier
            => _overwriteSettings || parentSettings == null ? base.randomScaleMultiplier
            : parentSettings.randomScaleMultiplier;
        public override FlipAction flipX
            => _overwriteSettings || parentSettings == null ? base.flipX : parentSettings.flipX;
        public override FlipAction flipY
            => _overwriteSettings || parentSettings == null ? base.flipY : parentSettings.flipY;
        public override Vector3 maxScaleMultiplier
            => _overwriteSettings || parentSettings == null ? base.maxScaleMultiplier
            : randomScaleMultiplier ? randomScaleMultiplierRange.max : scaleMultiplier;
        public override Vector3 minScaleMultiplier
            => _overwriteSettings || parentSettings == null ? base.minScaleMultiplier
            : randomScaleMultiplier ? randomScaleMultiplierRange.min : scaleMultiplier;
        public virtual bool overwriteThumbnailSettings
        {
            get => _overwriteThumbnailSettings;
            set
            {
                if (_overwriteThumbnailSettings == value) return;
                _overwriteThumbnailSettings = value;
            }
        }
        public override ThumbnailSettings thumbnailSettings
        {
            get => _overwriteThumbnailSettings || parentSettings == null
                ? base.thumbnailSettings : parentSettings.thumbnailSettings;
            set => base.thumbnailSettings = value;
        }
        public bool includeInThumbnail
        {
            get => _includeInThumbnail;
            set
            {
                if (_includeInThumbnail == value) return;
                _includeInThumbnail = value;
            }
        }

        public override bool isAsset2D { get => _isAsset2D; set => _isAsset2D = value; }

        public void UpdateAssetType() => _isAsset2D = Utils2D.Is2DAsset(prefab);
        public override void Copy(BrushSettings other)
        {
            if (other is MultibrushItemSettings)
            {
                var otherItemSettings = other as MultibrushItemSettings;
                _overwriteSettings = otherItemSettings._overwriteSettings;
                _frequency = otherItemSettings._frequency;
                _overwriteThumbnailSettings = otherItemSettings._overwriteThumbnailSettings;
                _includeInThumbnail = otherItemSettings._includeInThumbnail;
                _isAsset2D = otherItemSettings._isAsset2D;
            }
            base.Copy(other);
        }

        public MultibrushItemSettings() : base() { }

        public MultibrushItemSettings(MultibrushItemSettings other) : base() => Copy(other);

        public override BrushSettings Clone()
        {
            var clone = new MultibrushItemSettings();
            clone._prefab = _prefab;
            clone._guid = _guid;
            if (parentSettings != null)
            {
                clone._parentId = parentSettings.id;
                clone._parentSettings = parentSettings;
            }
            clone._bottomVertices = bottomVertices == null ? null : bottomVertices.ToArray();
            clone._bottomMagnitude = bottomMagnitude;
            clone._height = height;
            clone.Copy(this);
            clone.SetId();
            if (thumbnail != null)
            {
                var texture = new Texture2D(thumbnail.width, thumbnail.height, thumbnail.format, false);
                texture.SetPixels(thumbnail.GetPixels());
                texture.Apply();
                clone.SetCustomThumbnailTexture(texture, savePng: false);
            }
            clone._furthestVerticesInDirection = new System.Collections.Generic.Dictionary<Vector3,
                (Vector3[] vertices, float magnitude)>(_furthestVerticesInDirection);
            clone._directionList = new System.Collections.Generic.List<Vector3>(_directionList);
            return clone;
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            _prefabPath = UnityEditor.AssetDatabase.GetAssetPath(_prefab);
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            _prefab = null;
        }

        public Vector3[] bottomVertices
        {
            get
            {
                if (_bottomVertices == null) UpdateBottomVertices();
                return _bottomVertices;
            }
        }

        public float bottomMagnitude
        {
            get
            {
                if (prefab == null) return 0f;
                if (_bottomMagnitude == 0) _bottomMagnitude = BoundsUtils.GetBottomMagnitude(prefab.transform);
                return _bottomMagnitude;
            }
        }

        public float height => _height;
        public Vector3 size
        {
            get
            {
                if (prefab == null) return Vector3.zero;
                if (_size == Vector3.zero) _size = BoundsUtils.GetBoundsRecursive(prefab.transform).size;
                return _size;
            }
        }
        public override void UpdateBottomVertices()
        {
            base.UpdateBottomVertices();
            if (prefab == null) return;
            _bottomVertices = BoundsUtils.GetBottomVertices(prefab.transform);
            _height = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation).size.y;
            _size = BoundsUtils.GetBoundsRecursive(prefab.transform).size;
            _bottomMagnitude = BoundsUtils.GetBottomMagnitude(prefab.transform);
        }

        public Vector3[] GetFurthestVerticesInDirection(Vector3 direction, out float magnitude)
        {
            var vertices = BoundsUtils.GetFurthestVertices(prefab.transform, direction, out magnitude);
            return vertices;
        }

        public override int GetHashCode()
        {
            var hashCode = base.GetHashCode();
            hashCode = hashCode * -1521134295 + _overwriteSettings.GetHashCode();
            hashCode = hashCode * -1521134295 + _guid.GetHashCode();
            hashCode = hashCode * -1521134295 + _prefabPath.GetHashCode();
            hashCode = hashCode * -1521134295 + _frequency.GetHashCode();
            hashCode = hashCode * -1521134295 + _parentId.GetHashCode();
            hashCode = hashCode * -1521134295 + _embedAtPivotHeight.GetHashCode();
            hashCode = hashCode * -1521134295 + _overwriteThumbnailSettings.GetHashCode();
            hashCode = hashCode * -1521134295 + _includeInThumbnail.GetHashCode();
            hashCode = hashCode * -1521134295 + _isAsset2D.GetHashCode();
            return hashCode;
        }
    }

    [System.Serializable]
    public class MultibrushSettings : BrushSettings
    {
        public enum FrequencyMode { RANDOM, PATTERN }
        [SerializeField] private string _name = null;
        [SerializeField]
        private System.Collections.Generic.List<MultibrushItemSettings> _items
            = new System.Collections.Generic.List<MultibrushItemSettings>();
        [SerializeField] private FrequencyMode _frequencyMode = FrequencyMode.RANDOM;
        [SerializeField] private string _pattern = "1...";
        [SerializeField] private bool _restartPatternForEachStroke = true;

        [field: System.NonSerialized] private float _totalFrequency = -1;
        [field: System.NonSerialized] private PatternMachine _patternMachine = null;
        [field: System.NonSerialized] private PaletteData _palette = null;


        public string name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnDataChanged();
            }
        }

        public FrequencyMode frequencyMode
        {
            get => _frequencyMode;
            set
            {
                if (_frequencyMode == value) return;
                _frequencyMode = value;
                OnDataChanged();
            }
        }

        public string pattern
        {
            get => _pattern;
            set
            {
                if (_pattern == value) return;
                _pattern = value;
                OnDataChanged();
            }
        }

        public PatternMachine patternMachine
        {
            get
            {
                if (_patternMachine == null)
                {
                    if (PatternMachine.Validate(_pattern, _items.Count,
                        out PatternMachine.Token[] tokens, out PatternMachine.Token[] endTokens)
                        == PatternMachine.ValidationResult.VALID) _patternMachine = new PatternMachine(tokens, endTokens);
                }
                return _patternMachine;
            }
            set
            {
                _patternMachine = value;
                OnDataChanged();
            }
        }

        public bool restartPatternForEachStroke
        {
            get => _restartPatternForEachStroke;
            set
            {
                if (_restartPatternForEachStroke == value) return;
                _restartPatternForEachStroke = value;
                OnDataChanged();
            }
        }

        public PaletteData palette
        {
            get
            {
                if (_palette == null) _palette = PaletteManager.GetPalette(this);
                return _palette;
            }
            set => _palette = value;
        }

        public override bool isAsset2D
        {
            get => _items.Exists(i => i.isAsset2D);
            set
            {
                foreach (var item in _items) item.isAsset2D = value;
            }
        }

        protected override void OnDataChanged()
        {
            base.OnDataChanged();
            SavePalette();
        }
        public void SavePalette()
        {
            if (palette != null) palette.Save();
        }
        public MultibrushSettings(GameObject prefab, PaletteData palette) : base()
        {
            SetId();
            this.palette = palette;
            _items.Add(new MultibrushItemSettings(prefab, this));
            _name = prefab.name;
            Copy(palette.brushCreationSettings.defaultBrushSettings);
            thumbnailSettings.Copy(palette.brushCreationSettings.defaultThumbnailSettings);
            if (isAsset2D)
            {
                thumbnailSettings.Copy(ThumbnailSettings.GetDefaultTAsset2DThumbnailSettings());
                _rotateToTheSurface = false;
            }
            UpdateThumbnail(updateItemThumbnails: false, savePng: true);

        }

        public override string thumbnailPath
            => palette == null ? null : palette.thumbnailsFolderPath + "/" + id.ToString("X") + ".png";

        public void AddItem(MultibrushItemSettings item)
        {
            _items.Add(item);
            OnItemCountChange();
        }

        private void RemoveFromPalette()
        {
            if (palette != null) palette.RemoveBrush(this);
        }

        public void RemoveItemAt(int index)
        {
            _items.RemoveAt(index);
            OnItemCountChange();
            if (_items.Count == 0) RemoveFromPalette();
        }

        public void RemoveItem(MultibrushItemSettings item)
        {
            if (!_items.Contains(item)) return;
            _items.Remove(item);
            OnItemCountChange();
            if (_items.Count == 0) RemoveFromPalette();
        }

        public MultibrushItemSettings GetItemAt(int index)
        {
            if (index >= _items.Count) return null;
            return _items[index];
        }

        public bool ItemExist(long itemId) => _items.Exists(i => i.id == itemId);
        public MultibrushItemSettings GetItemById(long itemId)
        {
            var items = _items.Where(i => i.id == itemId).ToArray();
            if (items.Length == 0) return null;
            return items[0];
        }

        public void InsertItemAt(MultibrushItemSettings item, int index)
        {
            _items.Insert(index, item);
            OnItemCountChange();
        }

        private void OnItemCountChange()
        {
            UpdateTotalFrequency();
            UpdatePatternMachine();
            PWBCore.staticData.SaveAndUpdateVersion();
            BrushstrokeManager.UpdateBrushstroke();
            SavePalette();
            UpdateThumbnail(updateItemThumbnails: false, savePng: true);
            if (_palette != null) _palette.ClearObjectQuery();
        }

        public void Swap(int fromIdx, int toIdx, ref int[] selection)
            => SelectionUtils.Swap<MultibrushItemSettings>(fromIdx, toIdx, ref selection, _items);

        public MultibrushItemSettings[] items => _items.ToArray();

        public int itemCount => _items.Count;

        public int notNullItemCount => _items.Where(i => i.prefab != null).Count();
        public bool containMissingPrefabs
        {
            get
            {
                foreach (var item in _items)
                    if (item.prefab == null) return true;
                return false;
            }
        }

        public bool allPrefabMissing
        {
            get
            {
                foreach (var item in _items)
                    if (item.prefab != null) return false;
                return true;
            }
        }
        public void UpdateTotalFrequency()
        {
            _totalFrequency = 0;
            foreach (var item in _items) _totalFrequency += item.frequency;
        }

        public float totalFrequency
        {
            get
            {
                if (_totalFrequency == -1) UpdateTotalFrequency();
                return _totalFrequency;
            }
        }
        private int GetNextItemIndex()
        {
            if (frequencyMode == FrequencyMode.RANDOM)
            {
                if (_items.Count == 1) return 0;
                var rand = UnityEngine.Random.Range(0f, totalFrequency);
                float sum = 0;
                for (int i = 0; i < _items.Count; ++i)
                {
                    sum += _items[i].frequency;
                    if (rand <= sum) return i;
                }
                return -1;
            }
            if (_patternMachine == null)
            {
                if (PatternMachine.Validate(_pattern, _items.Count,
                    out PatternMachine.Token[] tokens, out PatternMachine.Token[] endTokens)
                    == PatternMachine.ValidationResult.VALID) _patternMachine = new PatternMachine(tokens, endTokens);
            }
            var result = _patternMachine == null ? -2 : _patternMachine.nextIndex - 1;
            return result;
        }
        private int _currentItemIndex = 0;
        public int nextItemIndex
        {
            get
            {
                _currentItemIndex = GetNextItemIndex();
                return _currentItemIndex;
            }
        }
        public int currentItemIndex
        {
            get
            {
                return _currentItemIndex;
            }
        }
        public void ResetCurrentItemIndex()
        {
            if (frequencyMode == FrequencyMode.RANDOM) return;
            if (_patternMachine == null)
            {
                if (PatternMachine.Validate(_pattern, _items.Count,
                    out PatternMachine.Token[] tokens, out PatternMachine.Token[] endTokens)
                    == PatternMachine.ValidationResult.VALID) _patternMachine = new PatternMachine(tokens, endTokens);
                return;
            }
            _patternMachine.Reset();
            _currentItemIndex = _patternMachine.nextIndex - 1;
        }
        public void SetNextItemIndex()
        {
            _currentItemIndex = GetNextItemIndex();
        }
        public int GetPatternTokenIndex()
        {
            if (frequencyMode == FrequencyMode.RANDOM | _patternMachine == null) return 0;
            return _patternMachine.tokenIndex;
        }
        public void SetPatternTokenIndex(int value)
        {
            if (frequencyMode == FrequencyMode.RANDOM | _patternMachine == null) return;
            _patternMachine.SetTokenIndex(value);
        }
        private void UpdatePatternMachine()
        {
            if (PatternMachine.Validate(_pattern, _items.Count,
                out PatternMachine.Token[] tokens, out PatternMachine.Token[] endTokens)
                != PatternMachine.ValidationResult.VALID)
                _patternMachine = null;
        }

        public override void Copy(BrushSettings other)
        {

            if (other is MultibrushSettings)
            {
                var otherMulti = other as MultibrushSettings;
                _items.Clear();
                foreach (var item in otherMulti._items)
                {
                    var clone = item.Clone() as MultibrushItemSettings;
                    clone.parentSettings = this;
                    _items.Add(clone);
                }
                _name = otherMulti._name;
                _frequencyMode = otherMulti._frequencyMode;
                _pattern = otherMulti._pattern;
                _restartPatternForEachStroke = otherMulti._restartPatternForEachStroke;
                _totalFrequency = otherMulti._totalFrequency;
            }
            base.Copy(other);
        }

        private MultibrushSettings() : base() { }
        public override BrushSettings Clone()
        {
            var clone = new MultibrushSettings();
            clone.Copy(this);
            clone.SetId();
            clone.palette = _palette;
            if (thumbnail != null)
            {
                var texture = new Texture2D(thumbnail.width, thumbnail.height, thumbnail.format, false);
                texture.SetPixels(thumbnail.GetPixels());
                texture.Apply();
                clone.SetCustomThumbnailTexture(texture, savePng: false);
            }
            return clone;
        }

        public MultibrushSettings CloneAndChangePalette(PaletteData palette)
        {
            var clone = new MultibrushSettings();
            clone.Copy(this);
            clone.SetId();
            clone.palette = palette;
            return clone;
        }
        public BrushSettings CloneMainSettings()
        {
            var clone = new BrushSettings();
            clone.Copy(this);
            return clone;
        }

        public void Duplicate(int index)
        {
            var original = _items[index];
            var clone = original.Clone();
            if (clone.thumbnailSettings.useCustomImage)
            {
                clone.thumbnailSettings.useCustomImage = false;
                clone.UpdateThumbnail(updateItemThumbnails: true, savePng: true);
                clone.thumbnailSettings.useCustomImage = true;
                var thumbnailClone = new Texture2D(original.thumbnail.width, original.thumbnail.height);
                thumbnailClone.SetPixels(original.thumbnail.GetPixels());
                thumbnailClone.Apply();
                clone.SetCustomThumbnailTexture(thumbnailClone, savePng: true);
            }
            else clone.UpdateThumbnail(updateItemThumbnails: true, savePng: true);
            _items.Insert(index, clone as MultibrushItemSettings);
            OnItemCountChange();
        }

        public override void UpdateBottomVertices()
        {
            foreach (var item in _items) item.UpdateBottomVertices();
        }

        public override bool embedInSurface
        {
            get => _embedInSurface;
            set
            {
                if (_embedInSurface == value) return;
                _embedInSurface = value;
                if (_embedInSurface) UpdateBottomVertices();
            }
        }
#if UNITY_2021_1_OR_NEWER
        public System.Collections.Generic.HashSet<GameObject> prefabs
            => _items.Select(i => i.prefab).Where(p => p != null).ToHashSet();
#else
        public System.Collections.Generic.HashSet<GameObject> prefabs
            => new System.Collections.Generic.HashSet<GameObject>(_items.Select(i => i.prefab).Where(p => p != null));
#endif
        public bool ContainsPrefab(int prefabId)
            => _items.Exists(item => item.prefab != null && item.prefab.GetInstanceID() == prefabId);

        public bool ContainsPrefabPath(string path) => _items.Exists(item => item.prefabPath == path);
        public bool ContainsSceneObject(GameObject obj)
        {
            if (obj == null) return false;
            var outermostPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            if (outermostPrefab == null) return false;
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(outermostPrefab);
            if (prefab == null) return false;
            return ContainsPrefab(prefab.GetInstanceID());
        }

        public Vector3 minBrushSize
        {
            get
            {
                var min = Vector3.one * float.MaxValue;
                foreach (var item in _items)
                    min = Vector3.Min(min, item.size);
                return min;
            }
        }

        public float minBrushMagnitude
        {
            get
            {
                var min = minBrushSize;
                return Mathf.Min(min.x, min.y, min.z);
            }
        }

        public Vector3 maxBrushSize
        {
            get
            {
                var max = Vector3.one * float.MinValue;
                foreach (var item in _items)
                    max = Vector3.Max(max, item.size);
                max = Vector3.Scale(max, maxScaleMultiplier);
                return max;
            }
        }

        public float maxBrushMagnitude
        {
            get
            {
                var max = maxBrushSize;
                return Mathf.Min(max.x, max.y, max.z);
            }
        }

        public void UpdateAssetTypes()
        {
            foreach (var item in _items) item.UpdateAssetType();
        }

        public void Cleanup()
        {
            foreach (var item in items) if (item.prefab == null) RemoveItem(item);
        }

        public override int GetHashCode()
        {
            int hashCode = 917907199;
            hashCode = hashCode * -1521134295 + _name.GetHashCode();
            hashCode = hashCode * -1521134295 + _frequencyMode.GetHashCode();
            hashCode = hashCode * -1521134295 + _pattern.GetHashCode();
            hashCode = hashCode * -1521134295 + _restartPatternForEachStroke.GetHashCode();
            foreach (var item in _items) hashCode = hashCode * -1521134295 + item.GetHashCode();
            return hashCode;

        }
    }

    [System.Serializable]
    public class BrushCreationSettings
    {
        [SerializeField] private bool _includeSubfolders = true;
        [SerializeField] private bool _addLabelsToDroppedPrefabs = false;
        [SerializeField] private string _labelsCSV = null;
        private string[] _labels = null;
        [SerializeField] private BrushSettings _defaultBrushSettings = new BrushSettings();
        [SerializeField] private ThumbnailSettings _defaultThumbnailSettings = new ThumbnailSettings();

        public bool includeSubfolders
        {
            get => _includeSubfolders;
            set
            {
                if (_includeSubfolders == value) return;
                _includeSubfolders = value;
            }
        }

        public bool addLabelsToDroppedPrefabs
        {
            get => _addLabelsToDroppedPrefabs;
            set
            {
                if (_addLabelsToDroppedPrefabs == value) return;
                _addLabelsToDroppedPrefabs = value;
            }
        }

        private void SplitCSV() => _labels = _labelsCSV.Replace(", ", ",").Split(',');

        public string[] labels
        {
            get
            {
                if (_labels == null || (_labels.Length == 0 && _labelsCSV != null && _labelsCSV != string.Empty))
                    SplitCSV();
                return _labels;
            }
        }

        public string labelsCSV
        {
            get => _labelsCSV;
            set
            {
                if (_labelsCSV == value) return;
                if (value == string.Empty)
                {
                    _labelsCSV = string.Empty;
                    _labels = new string[0];
                    return;
                }
                var trimmed = System.Text.RegularExpressions.Regex.Replace(value.Trim(), "[( *, +)]+", ", ");
                if (trimmed.Last() == ' ') trimmed = trimmed.Substring(0, trimmed.Length - 2);
                if (trimmed.First() == ',') trimmed = trimmed.Substring(1);
                if (_labelsCSV == trimmed) return;
                _labelsCSV = trimmed;
                SplitCSV();
            }
        }

        public BrushSettings defaultBrushSettings => _defaultBrushSettings;
        public void FactoryResetDefaultBrushSettings() => _defaultBrushSettings = new BrushSettings();

        public ThumbnailSettings defaultThumbnailSettings => _defaultThumbnailSettings;
        public void FactoryResetDefaultThumbnailSettings() => _defaultThumbnailSettings = new ThumbnailSettings();

        public BrushCreationSettings Clone()
        {
            var clone = new BrushCreationSettings();
            clone.Copy(this);
            return clone;
        }

        public void Copy(BrushCreationSettings other)
        {
            _includeSubfolders = other._includeSubfolders;
            _addLabelsToDroppedPrefabs = other._addLabelsToDroppedPrefabs;
            _labelsCSV = other._labelsCSV;
            if (other._labels != null)
            {
                _labels = new string[other._labels.Length];
                System.Array.Copy(other._labels, _labels, other._labels.Length);
            }
            _defaultBrushSettings.Copy(other._defaultBrushSettings);
            _defaultThumbnailSettings.Copy(other._defaultThumbnailSettings);
        }

        public override int GetHashCode()
        {
            int hashCode = 917907199;
            hashCode = hashCode * -1521134295 + _includeSubfolders.GetHashCode();
            hashCode = hashCode * -1521134295 + _addLabelsToDroppedPrefabs.GetHashCode();
            hashCode = hashCode * -1521134295 + _defaultBrushSettings.GetHashCode();
            hashCode = hashCode * -1521134295 + _defaultThumbnailSettings.GetHashCode();
            return hashCode;
        }
    }
    public class BrushInputData
    {
        public readonly int index;
        public readonly Rect rect;
        public readonly EventType eventType;
        public readonly bool control;
        public readonly bool shift;
        public readonly float mouseX;
        public BrushInputData(int index, Rect rect, EventType eventType, bool control, bool shift, float mouseX)
        {
            this.index = index;
            this.rect = rect;
            this.eventType = eventType;
            this.control = !shift && control;
            this.shift = shift;
            this.mouseX = mouseX;
        }
    }

    #endregion

    #region PALETTE DATA & MANAGER
    [System.Serializable]
    public class PaletteData : ISerializationCallbackReceiver
    {
        [SerializeField] private string _version = PWBData.VERSION;
        [SerializeField] private string _name = string.Empty;
        [SerializeField] private long _id = -1;
        [SerializeField] private string _hexId = string.Empty;
        [SerializeField]
        private System.Collections.Generic.List<MultibrushSettings> _brushes
            = new System.Collections.Generic.List<MultibrushSettings>();
        [SerializeField] BrushCreationSettings _brushCreationSettings = new BrushCreationSettings();
        [SerializeField] private int _hashCode = 0;
        [SerializeField] private bool _isPinned = false;
        private string _filePath = null;
        private bool _saving = false;
        public PaletteData(string name, long id) => (_name, _id) = (name, id);

        public string name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                Save();
            }
        }
        public long id => _id;
        public string hexId => _id.ToString("X");
        public MultibrushSettings[] brushes => _brushes.Where(b => !b.allPrefabMissing).ToArray();

        public int brushCount => _brushes.Count;

        public BrushCreationSettings brushCreationSettings => _brushCreationSettings;

        public string filePath
        {
            get
            {
                void SetFilePath() => _filePath = PWBData.palettesDirectory + "/"
                    + GetFileNameFromData(this, includeExtension: true);
                if (_filePath == null) SetFilePath();
                else if (!System.IO.File.Exists(_filePath)) SetFilePath();
                return _filePath;
            }
            set => _filePath = value;
        }
        public string thumbnailsFolderPath
        {
            get
            {
                var path = PWBData.palettesDirectory + (PWBCore.staticData.createThumbnailsFolder ? "/Thumbnails/" : "/")
                    + GetFileNameFromData(this, includeExtension: false);
                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                return path;
            }
        }

        public string version
        {
            get
            {
                if (_version == null || _version == string.Empty) _version = PWBData.VERSION;
                return _version;
            }
            set => _version = value;
        }
        public bool saving => _saving;
        public bool isPinned
        {
            get => _isPinned;
            set
            {
                if (_isPinned == value) return;
                _isPinned = value;
                Save();
            }
        }
        public void StopSaving() => _saving = false;

        public static string GetFileNameFromData(PaletteData data, bool includeExtension)
            => "PWB_" + data._id.ToString("X") + (includeExtension ? ".txt" : "");

        public MultibrushSettings GetBrush(int idx)
        {
            if (idx < 0 || idx >= _brushes.Count) return null;
            if (_brushes[idx].allPrefabMissing) return null;
            return _brushes[idx];
        }

        public void UpdateAllThumbnails()
        {
            foreach (var brush in _brushes) brush.UpdateThumbnail(updateItemThumbnails: true, savePng: true);
        }

        public void AddBrush(MultibrushSettings brush)
        {
            _brushes.Add(brush);
            brush.palette = this;
            PWBCore.staticData.SaveAndUpdateVersion();
            Save();
            ClearObjectQuery();
        }

        public void RemoveBrushAt(int idx)
        {
            _brushes.RemoveAt(idx);
            PWBCore.staticData.SaveAndUpdateVersion();
            BrushstrokeManager.UpdateBrushstroke();
            Save();
            ClearObjectQuery();
        }

        public void RemoveBrush(MultibrushSettings brush)
        {
            _brushes.Remove(brush);
            PWBCore.staticData.SaveAndUpdateVersion();
            BrushstrokeManager.UpdateBrushstroke();
            PrefabPalette.OnChangeRepaint();
            Save();
            ClearObjectQuery();
        }

        public void InsertBrushAt(MultibrushSettings brush, int idx)
        {
            _brushes.Insert(idx, brush);
            brush.palette = this;
            PWBCore.staticData.SaveAndUpdateVersion();
            Save();
            ClearObjectQuery();
        }

        public void Swap(int fromIdx, int toIdx, ref int[] selection)
            => SelectionUtils.Swap(fromIdx, toIdx, ref selection, _brushes);

        public void AscendingSort()
        {
            _brushes.Sort(delegate (MultibrushSettings x, MultibrushSettings y) { return x.name.CompareTo(y.name); });
            PaletteManager.ClearSelection();
            PWBCore.staticData.SaveAndUpdateVersion();
            PrefabPalette.OnChangeRepaint();
        }

        public void DescendingSort()
        {
            _brushes.Sort(delegate (MultibrushSettings x, MultibrushSettings y) { return y.name.CompareTo(x.name); });
            PaletteManager.ClearSelection();
            PWBCore.staticData.SaveAndUpdateVersion();
            PrefabPalette.OnChangeRepaint();
        }

        public void DuplicateBrush(int index, out MultibrushSettings duplicate)
            => DuplicateBrushAt(index, index, out duplicate);

        public void DuplicateBrushAt(int indexToDuplicate, int at, out MultibrushSettings duplicate)
        {
            var original = _brushes[indexToDuplicate];
            duplicate = original.Clone() as MultibrushSettings;

            if (duplicate.thumbnailSettings.useCustomImage)
            {
                duplicate.thumbnailSettings.useCustomImage = false;
                duplicate.UpdateThumbnail(updateItemThumbnails: true, savePng: true);
                duplicate.thumbnailSettings.useCustomImage = true;
                var thumbnailClone = new Texture2D(original.thumbnail.width, original.thumbnail.height);
                thumbnailClone.SetPixels(original.thumbnail.GetPixels());
                thumbnailClone.Apply();
                duplicate.SetCustomThumbnailTexture(thumbnailClone, savePng: true);
            }
            else duplicate.UpdateThumbnail(updateItemThumbnails: true, savePng: true);
            _brushes.Insert(at, duplicate);
            PWBCore.staticData.SaveAndUpdateVersion();
            Save();
        }

        private System.Collections.Generic.Dictionary<int, bool> _objectQuery
            = new System.Collections.Generic.Dictionary<int, bool>();

        private System.Collections.Generic.Dictionary<int, bool> objectQuery
        {
            get
            {
                if (_objectQuery == null) _objectQuery = new System.Collections.Generic.Dictionary<int, bool>();
                return _objectQuery;
            }
        }
        public void ClearObjectQuery() => objectQuery.Clear();

        public bool ContainsSceneObject(GameObject obj)
        {
            if (obj == null) return false;
            int id = obj.GetInstanceID();
            if (objectQuery.TryGetValue(id, out bool cached)) return cached;
            bool found = false;
            var root = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            if (root != null)
            {
                var source = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(root);
                if (source != null) found = _brushes.Exists(brush => brush.ContainsPrefab(source.GetInstanceID()));
            }
            objectQuery[id] = found;
            return found;
        }

        public System.Collections.Generic.HashSet<GameObject> prefabs
        {
            get
            {
                var prefabSet = new System.Collections.Generic.HashSet<GameObject>();
                foreach (var brush in _brushes) prefabSet.UnionWith(brush.prefabs);
                return prefabSet;
            }
        }
        public bool ContainsPrefabPath(string path) => _brushes.Exists(brush => brush.ContainsPrefabPath(path));
        public int FindBrushIdx(GameObject obj)
        {
            if (obj == null) return -1;
            var outermostPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            if (outermostPrefab == null) return -1;
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(outermostPrefab);
            if (prefab == null) return -1;
            var idx = _brushes.FindIndex(brush => brush.ContainsPrefab(prefab.GetInstanceID()) && brush.itemCount == 1);
            if (idx == -1) idx = _brushes.FindIndex(brush => brush.ContainsPrefab(prefab.GetInstanceID()));
            return idx;
        }

        public bool ContainsBrush(MultibrushSettings brush)
            => _brushes.Contains(brush) || _brushes.Exists(b => b.id == brush.id);

        public static System.Action OnPaletteSaved;
        public string Save()
        {
            _saving = true;
            var jsonString = JsonUtility.ToJson(this, true);
            var fileExist = System.IO.File.Exists(filePath);
            System.IO.File.WriteAllText(filePath, jsonString);
            if (!fileExist) PWBCore.AssetDatabaseRefresh();
            if (OnPaletteSaved != null) OnPaletteSaved();
            return filePath;
        }

        public void Copy(PaletteData other)
        {
            _brushes.Clear();
            var otherBrushes = other.brushes.ToArray();
            var cloneBrushes = otherBrushes.Select(b => b.CloneAndChangePalette(this)).ToArray();
            _brushes.AddRange(cloneBrushes);
            _name = other.name;
            _brushCreationSettings.Copy(other._brushCreationSettings);
        }
        public void ReloadFromFile()
        {
            var fileText = System.IO.File.ReadAllText(_filePath);
            if (string.IsNullOrEmpty(fileText)) return;
            var paletteData = JsonUtility.FromJson<PaletteData>(fileText);
            if (paletteData == null) return;
            Copy(paletteData);
            ClearObjectQuery();
        }

        public void Cleanup()
        {
            foreach (var brush in _brushes.ToArray()) brush.Cleanup();
            Save();
            ClearObjectQuery();
        }

        public int hashCode => _hashCode;
        public override int GetHashCode()
        {
            if (_name == null) return 0;
            int hashCode = 917907199;
            hashCode = hashCode * -1521134295 + version.GetHashCode();
            hashCode = hashCode * -1521134295 + _name.GetHashCode();
            hashCode = hashCode * -1521134295 + _id.GetHashCode();
            foreach (var brush in _brushes) hashCode = hashCode * -1521134295 + brush.GetHashCode();
            hashCode = hashCode * -1521134295 + _brushCreationSettings.GetHashCode();
            return hashCode;
        }

        public void ResetHashCode()
        {
            _hashCode = GetHashCode();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            try
            {
                _hashCode = GetHashCode();
                _hexId = _id.ToString("X");
            }
            catch
            {
                _hashCode = 0;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
        }
    }

    [System.Serializable]
    public struct BasicPaletteData
    {
        [SerializeField] private int _hashCode;
        [SerializeField] private string _name;
        [SerializeField] private long _id;
        public int hashCode => _hashCode;
        public string name => _name;
        public long id => _id;
    }

    [System.Serializable]
    public class PaletteManager : ISerializationCallbackReceiver
    {
        private System.Collections.Generic.List<PaletteData> _nonPinnedPaletteDataList
            = new System.Collections.Generic.List<PaletteData>();
        private System.Collections.Generic.List<PaletteData> _pinnedPaletteDataList
            = new System.Collections.Generic.List<PaletteData>();
        public static System.Collections.Generic.List<PaletteData> nonPinnedPalettes => instance._nonPinnedPaletteDataList;
        public static System.Collections.Generic.List<PaletteData> pinnedPalettes => instance._pinnedPaletteDataList;
        public static System.Collections.Generic.List<PaletteData> allPalettes
        {
            get
            {
                var result = new System.Collections.Generic.List<PaletteData>(instance._pinnedPaletteDataList);
                result.AddRange(instance._nonPinnedPaletteDataList);
                return result;
            }
        }
        private PaletteData _selectedPalette = null;
        [SerializeField] private long _selectedPaletteId = -1;
        public static bool addingPalettes { get; set; }

        [SerializeField] private int _selectedBrushIdx = -1;
        [SerializeField] private bool _showBrushName = false;
        [SerializeField] private bool _viewList = false;
        [SerializeField] private bool _showTabsInMultipleRows = false;
        private System.Collections.Generic.HashSet<int> _idxSelection = new System.Collections.Generic.HashSet<int>();

        public static System.Action OnBrushSelectionChanged;
        public static System.Action OnSelectionChanged;
        public static System.Action OnPaletteChanged;
        private bool _pickingBrushes = false;

        private PaletteData _movingBrushesFrom = null;
        public static bool movingBrushes => instance._movingBrushesFrom != null;
        private System.Collections.Generic.List<MultibrushSettings> _brushesToMove
            = new System.Collections.Generic.List<MultibrushSettings>();


        [SerializeField] private int _iconSize = PrefabPalette.DEFAULT_ICON_SIZE;

        private PaletteData CreateEmptyPalette()
        {
            var p = new PaletteData("Palette", System.DateTime.Now.ToBinary());
            AddPalette(p, save: true);
            _selectedPalette = p;
            return p;
        }
        private System.Collections.Generic.List<PaletteData> nonPinnedPaletteDataList
        {
            get
            {
                if (_nonPinnedPaletteDataList.Count == 0 && _pinnedPaletteDataList.Count == 0)
                {
                    CreateEmptyPalette();
                    _selectedBrushIdx = -1;
                }
                return _nonPinnedPaletteDataList;
            }
        }

        private static PaletteManager _instance = null;

        private PaletteManager() { }
        public static PaletteManager instance
        {
            get
            {
                if (_instance == null) _instance = new PaletteManager();
                return _instance;
            }
        }
        private static bool _loadPaletteFilesPending = false;
        public static void SetLoadPaletteFilesPending()
        {
            _loadPaletteFilesPending = true;
        }
        public static bool loadPaletteFilesPending => _loadPaletteFilesPending;
        static System.DateTime _lastLoadTime = System.DateTime.MinValue;
        public void LoadPaletteFiles(bool deleteUnusedThumbnails)
        {
            
            const double throttleSeconds = 3.0;
            var now = System.DateTime.Now;
            if ((now - _lastLoadTime).TotalSeconds < throttleSeconds)
                return;
            _lastLoadTime = now;
            _loadPaletteFilesPending = false;
            var txtPaths = System.IO.Directory.GetFiles(PWBData.palettesDirectory, "*.txt");
            if (txtPaths.Length == 0)
            {
                if (_nonPinnedPaletteDataList.Count == 0) CreateEmptyPalette();
                _nonPinnedPaletteDataList[0].filePath = _nonPinnedPaletteDataList[0].Save();
            }
            var allPalettesList = allPalettes;

            System.Collections.Generic.List<PaletteData> toRemove = new System.Collections.Generic.List<PaletteData>();
            System.Collections.Generic.HashSet<long> loadedIds = new System.Collections.Generic.HashSet<long>();
            foreach (var path in txtPaths)
            {
                var fileText = System.IO.File.ReadAllText(path);
                if (string.IsNullOrEmpty(fileText)) continue;
                try
                {
                    var basicPaletteDataFromFile = JsonUtility.FromJson<BasicPaletteData>(fileText);
                    if (allPalettesList.Count > 0)
                    {
                        var sameHash = allPalettesList.Where(
                            p => p.GetHashCode() == basicPaletteDataFromFile.hashCode);
                        if (sameHash.Count() > 0)
                        {
                            toRemove.AddRange(sameHash); 
                            continue;
                        }
                        var sameId = allPalettesList.Where(p => p.id == basicPaletteDataFromFile.id);
                        if (sameId.Count() > 0)
                        {
                            var currentData = sameId.First();
                            currentData.Save();
                            toRemove.AddRange(sameId);
                            continue;
                        }
                    }
                    var paletteData = JsonUtility.FromJson<PaletteData>(fileText);
                    if (paletteData == null) continue;
                    var loadedId = paletteData.id;
                    if (loadedIds.Contains(loadedId))
                    {
                        Debug.LogWarning($"PWB found a duplicated palette id in file: {path}, " +
                            $"name: {paletteData.name}, id: {PaletteData.GetFileNameFromData(paletteData, false)}");
                        DeletePaletteFile(path, paletteData.thumbnailsFolderPath);
                        continue;
                    }
                    loadedIds.Add(loadedId);
                    paletteData.filePath = path;
                    AddPalette(paletteData, save: false);
                }
                catch
                {
                    Debug.LogWarning($"PWB found a corrupted palette file at: {path}");
                }
                for (int i = 0; i < toRemove.Count; i++)  RemovePalette(toRemove[i]);
            }
            if (deleteUnusedThumbnails) ThumbnailUtils.DeleteUnusedThumbnails();
        }

        public static void Clear()
        {
            ClearPaletteList();
            instance.CreateEmptyPalette();
            instance._selectedBrushIdx = -1;
            instance._idxSelection.Clear();
            instance._pickingBrushes = false;
        }

        public static bool showBrushName
        {
            get => instance._showBrushName;
            set
            {
                if (instance._showBrushName == value) return;
                instance._showBrushName = value;
                PWBCore.staticData.SaveAndUpdateVersion();
            }
        }

        public static bool viewList
        {
            get => instance._viewList;
            set
            {
                if (instance._viewList == value) return;
                instance._viewList = value;
                PWBCore.staticData.SaveAndUpdateVersion();
            }
        }
        public static bool showTabsInMultipleRows
        {
            get => instance._showTabsInMultipleRows;
            set
            {
                if (instance._showTabsInMultipleRows == value) return;
                instance._showTabsInMultipleRows = value;
                PWBCore.staticData.SaveAndUpdateVersion();
            }
        }
        public static void ClearPaletteList() => instance._nonPinnedPaletteDataList.Clear();
        public static void AddPalette(PaletteData palette, bool save)
        {
            addingPalettes = true;
            if (allPalettes.Exists(p => p.id == palette.id)) return;
            if (palette.isPinned) instance._pinnedPaletteDataList.Add(palette);
            else instance._nonPinnedPaletteDataList.Add(palette);
            if (save)
            {
                palette.filePath = PWBData.palettesDirectory + "/"
                    + PaletteData.GetFileNameFromData(palette, includeExtension: true);
                palette.Save();
            }
        }


        public static void DuplicatePalette(PaletteData original)
        {
            if (original == null) return;
            var cloneId = System.DateTime.Now.ToBinary();
            var cloneName = original.name + " Copy";
            var clone = new PaletteData(cloneName, cloneId);
            clone.Copy(original);
            clone.name = cloneName;
            AddPalette(clone, save: true);
            clone.UpdateAllThumbnails();
        }
        public static void RemovePalette(PaletteData palette)
        {
            if (palette == null) return;
            if (!instance._nonPinnedPaletteDataList.Contains(palette) && !instance._pinnedPaletteDataList.Contains(palette))
                return;
            var filePath = palette.filePath;
            var thumbnailFolderPath = palette.thumbnailsFolderPath;
            if (!instance._nonPinnedPaletteDataList.Remove(palette))
                instance._pinnedPaletteDataList.Remove(palette);
            DeletePaletteFile(filePath, thumbnailFolderPath);
            if (instance._selectedPalette == palette) instance._selectedPalette = null;

            if (allPalettesCount == 0) instance.CreateEmptyPalette();
            else
            {
                var isSelected = instance._selectedPalette == palette;
                if (isSelected) PaletteManager.SelectPreviousPalette();
            }
            PaletteManager.selectedBrushIdx = -1;
            PWBCore.AssetDatabaseRefresh();
        }

        private static void DeletePaletteFile(string filePath, string thumbnailFolderPath)
        {
            var metapath = filePath + ".meta";
            if (System.IO.File.Exists(metapath)) System.IO.File.Delete(metapath);
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            metapath = thumbnailFolderPath + ".meta";
            if (System.IO.File.Exists(metapath)) System.IO.File.Delete(metapath);
            if (System.IO.Directory.Exists(thumbnailFolderPath))
                System.IO.Directory.Delete(thumbnailFolderPath, true);
        }

        public static void SelectNextPalette()
        {
            if (PrefabPalette.instance == null) return;
            if (allPalettesCount <= 1) return;
            instance._idxSelection.Clear();

            var paletteList = new System.Collections.Generic.List<PaletteData>(instance._pinnedPaletteDataList);
            if (PWBCore.staticData.selectTheNextPaletteInAlphabeticalOrder)
                paletteList.AddRange(instance.nonPinnedPaletteDataList.OrderBy(item => item.name).ToList());
            else paletteList.AddRange(instance.nonPinnedPaletteDataList);

            var nextPaletteIdx = paletteList.IndexOf(selectedPalette) + 1;
            if (nextPaletteIdx >= paletteList.Count) nextPaletteIdx = 0;
            var NextPalette = paletteList[nextPaletteIdx];

            PrefabPalette.instance.SelectPalette(NextPalette);
            if (!showTabsInMultipleRows) ShowSelectedFirst();
            selectedBrushIdx = 0;
            AddToSelection(selectedBrushIdx);
            PrefabPalette.instance.FrameSelectedBrush();
            PrefabPalette.RepaintWindow();
        }

        public static void SelectPreviousPalette()
        {
            if (PrefabPalette.instance == null) return;
            if (allPalettesCount == 0) return;
            if (allPalettesCount == 1 || selectedPalette == null)
            {
                var palette = allPalettes[0];
                PrefabPalette.instance.SelectPalette(palette);
                selectedBrushIdx = -1;
                return;
            }
            instance._idxSelection.Clear();

            var paletteList = new System.Collections.Generic.List<PaletteData>(instance._pinnedPaletteDataList);
            if (PWBCore.staticData.selectTheNextPaletteInAlphabeticalOrder)
                paletteList.AddRange(instance.nonPinnedPaletteDataList.OrderBy(item => item.name).ToList());
            else paletteList.AddRange(instance.nonPinnedPaletteDataList);
            
            var nextPaletteIdx = paletteList.IndexOf(selectedPalette) - 1;
            if (nextPaletteIdx < 0) nextPaletteIdx = paletteList.Count - 1;
            var NextPalette = paletteList[nextPaletteIdx];

            PrefabPalette.instance.SelectPalette(NextPalette);
            if (!showTabsInMultipleRows) ShowSelectedFirst();
            selectedBrushIdx = 0;
            AddToSelection(selectedBrushIdx);
            PrefabPalette.instance.FrameSelectedBrush();
            PrefabPalette.RepaintWindow();
        }

        public static string[] paletteNames
        {
            get
            {
                var nonPinnedPalettesNames = new string[instance.nonPinnedPaletteDataList.Count];
                for (int i = 0; i < instance.nonPinnedPaletteDataList.Count; i++)
                    nonPinnedPalettesNames[i] = instance.nonPinnedPaletteDataList[i].name;
                return nonPinnedPalettesNames;
            }
        }
        public static long[] paletteIds
        {
            get
            {
                var nonPinnedPalettesIds = new long[instance.nonPinnedPaletteDataList.Count];
                for (int i = 0; i < instance.nonPinnedPaletteDataList.Count; i++)
                    nonPinnedPalettesIds[i] = instance.nonPinnedPaletteDataList[i].id;
                return nonPinnedPalettesIds;
            }
        }
        public static void TogglePinnedPalette(PaletteData palette)
        {
            if (palette == null) return;

            if (palette.isPinned)
            {
                palette.isPinned = false;
                if (!instance.nonPinnedPaletteDataList.Contains(palette)) instance.nonPinnedPaletteDataList.Add(palette);
                instance._pinnedPaletteDataList.RemoveAll(p => p.id == palette.id);
            }
            else
            {
                palette.isPinned = true;
                instance.nonPinnedPaletteDataList.RemoveAll(p => p.id == palette.id);
                if (!instance._pinnedPaletteDataList.Contains(palette)) instance._pinnedPaletteDataList.Add(palette);
            }
        }

        public static long[] pinnedPaletteIds
        {
            get
            {
                var pinnedPalettesIds = new long[instance._pinnedPaletteDataList.Count];
                for (int i = 0; i < instance._pinnedPaletteDataList.Count; i++)
                    pinnedPalettesIds[i] = instance._pinnedPaletteDataList[i].id;
                return pinnedPalettesIds;
            }
        }
        public static string[] pinnedPaletteNames
        {
            get
            {
                var pinnedPalettesNames = new string[instance._pinnedPaletteDataList.Count];
                for (int i = 0; i < instance._pinnedPaletteDataList.Count; i++)
                    pinnedPalettesNames[i] = instance._pinnedPaletteDataList[i].name;
                return pinnedPalettesNames;
            }
        }
        public static int pinnedCount => instance._pinnedPaletteDataList.Count;

        public static int selectedBrushIdx
        {
            get => instance._selectedBrushIdx;
            set
            {
                if (instance._selectedBrushIdx == value) return;
                instance._selectedBrushIdx = value;
                if (selectedBrush != null)
                {
                    selectedBrush.UpdateBottomVertices();
                    selectedBrush.UpdateAssetTypes();
                }
                else instance._selectedBrushIdx = -1;
                if (PWBCore.staticData.openBrushPropertiesWhenABrushIsSelected && instance._selectedBrushIdx >= 0)
                    BrushProperties.ShowWindow();
                if (ToolManager.tool == ToolManager.PaintTool.PIN)
                {
                    BrushstrokeManager.UpdateBrushstroke(true);
                    PWBIO.ResetPinValues();
                }
                if (OnBrushSelectionChanged != null) OnBrushSelectionChanged();
                BrushstrokeManager.UpdateBrushstroke(true);
            }
        }

        public static bool pickingBrushes
        {
            get => instance._pickingBrushes;
            set
            {
                if (instance._pickingBrushes == value) return;
                instance._pickingBrushes = value;
                if (instance._pickingBrushes)
                {
                    PWBIO.repaint = true;
                    UnityEditor.SceneView.RepaintAll();
                    if (UnityEditor.SceneView.sceneViews.Count > 0)
                        ((UnityEditor.SceneView)UnityEditor.SceneView.sceneViews[0]).Focus();
                }
                PrefabPalette.RepaintWindow();
            }
        }

        public static int iconSize
        {
            get => instance._iconSize;
            set
            {
                if (instance._iconSize == value) return;
                instance._iconSize = value;
                PWBCore.staticData.SaveAndUpdateVersion();
            }
        }

        public static void SelectBrush(int idx)
        {
            if (PrefabPalette.instance == null) return;
            if (selectedPalette.brushCount == 0) return;
            if (!PrefabPalette.instance.FilteredBrushListContains(idx)) return;
            instance._idxSelection.Clear();
            selectedBrushIdx = idx;
            if (selectedBrush != null)
            {
                selectedBrush.UpdateBottomVertices();
                selectedBrush.UpdateAssetTypes();
            }
            AddToSelection(selectedBrushIdx);
            PrefabPalette.instance.FrameSelectedBrush();
            PrefabPalette.RepaintWindow();
        }

        public static void SelectNextBrush()
        {
            if (PrefabPalette.instance == null) return;
            if (selectedPalette.brushCount <= 1) return;
            instance._idxSelection.Clear();
            int selectedIdx = instance._selectedBrushIdx;
            int count = 0;
            do
            {
                selectedIdx = (selectedIdx + 1) % selectedPalette.brushCount;
                if (++count > selectedPalette.brushCount) return;
            }
            while (!PrefabPalette.instance.FilteredBrushListContains(selectedIdx));
            selectedBrushIdx = selectedIdx;
            if (selectedBrush != null)
            {
                selectedBrush.UpdateBottomVertices();
                selectedBrush.UpdateAssetTypes();
            }
            AddToSelection(selectedBrushIdx);
            PrefabPalette.instance.FrameSelectedBrush();
        }
        public static void SelectPreviousBrush()
        {
            if (PrefabPalette.instance == null) return;
            if (selectedPalette.brushCount <= 1) return;
            instance._idxSelection.Clear();
            int selectedIdx = instance._selectedBrushIdx;
            int count = 0;
            do
            {
                selectedIdx = (selectedIdx == 0 ? selectedPalette.brushCount : selectedIdx) - 1;
                if (++count > selectedPalette.brushCount) return;
            }
            while (!PrefabPalette.instance.FilteredBrushListContains(selectedIdx));
            selectedBrushIdx = selectedIdx;
            if (selectedBrush != null)
            {
                selectedBrush.UpdateBottomVertices();
                selectedBrush.UpdateAssetTypes();
            }
            AddToSelection(selectedBrushIdx);
            PrefabPalette.instance.FrameSelectedBrush();
        }

        public static MultibrushSettings selectedBrush
            => instance._selectedBrushIdx < 0 ? null : selectedPalette.GetBrush(instance._selectedBrushIdx);


        public static PaletteData InitializeSelectedPalette()
        {
            if (instance._selectedPalette != null) return instance._selectedPalette;
            bool createEmptyPalette = true;
            PaletteData SelectFirstPalette()
            {
                createEmptyPalette = false;
                if (instance._selectedPaletteId != -1)
                {
                    instance._selectedPalette = GetPalette(instance._selectedPaletteId);
                    if (instance._selectedPalette == null)
                    {
                        instance._selectedPaletteId = -1;
                        return null;
                    }
                    if (PrefabPalette.instance) PrefabPalette.instance.OnPaletteChange();
                    return instance._selectedPalette;
                }
                else if (!PWBCore._loadedFromFile)
                {
                    return null;
                }
                if (instance._pinnedPaletteDataList.Count > 0)
                {
                    instance._selectedPalette = instance._pinnedPaletteDataList.First();
                    return instance._selectedPalette;
                }
                if (instance._nonPinnedPaletteDataList.Count > 0)
                {
                    instance._selectedPalette = instance._nonPinnedPaletteDataList.First();
                    return instance._selectedPalette;
                }
                createEmptyPalette = true;
                return null;
            }
            var selectedPalette = SelectFirstPalette();
            if (selectedPalette != null) return selectedPalette;
            instance._pinnedPaletteDataList.Clear();
            instance._nonPinnedPaletteDataList.Clear();
            instance.LoadPaletteFiles(true);
            selectedPalette = SelectFirstPalette();
            if (selectedPalette != null) return selectedPalette;
            if (createEmptyPalette)
                instance.CreateEmptyPalette();
            return instance._selectedPalette;
        }

        public static PaletteData selectedPalette
        {
            get =>  instance._selectedPalette;
            set
            {
                if (value == null) return;
                if (value == instance._selectedPalette) return;
                bool isInAList = false;
                if(instance._pinnedPaletteDataList.Contains(value)) isInAList = true;
                if(!isInAList && instance._nonPinnedPaletteDataList.Contains(value)) isInAList = true;
                if (!isInAList) return;
                instance._selectedPalette = value;
                instance._selectedPaletteId = value.id;
                OnPaletteChanged();
                PWBCore.SetSavePending();
            }
        }

        public static bool IsPaletteSelected(PaletteData palette)
            => instance._selectedPalette != null && palette != null && instance._selectedPalette.id == palette.id;
        public static void ShowSelectedFirst()
        {
            var selected = selectedPalette;
            instance.nonPinnedPaletteDataList.RemoveAll(p => p.id == selected.id);
            instance.nonPinnedPaletteDataList.Insert(0, selected);
        }
        public static int nonPinnedCount => instance._nonPinnedPaletteDataList.Count;
        public static int allPalettesCount => instance._nonPinnedPaletteDataList.Count + instance._pinnedPaletteDataList.Count;
        public static MultibrushSettings GetBrushById(long id)
        {
            foreach (var palette in allPalettes)
                foreach (var brush in palette.brushes)
                    if (brush.id == id) return brush;
            return null;
        }

        public static MultibrushSettings GetBrushByThumbnail(string thumbnailPath)
        {
            foreach (var palette in allPalettes)
                foreach (var brush in palette.brushes)
                    if (brush.thumbnailPath == thumbnailPath) return brush;
            return null;
        }

        public static MultibrushSettings GetBrushByItemId(long id)
        {
            foreach (var palette in allPalettes)
                foreach (var brush in palette.brushes)
                    foreach (var item in brush.items)
                        if (item.id == id) return brush;
            return null;
        }

        public static bool BrushExist(long id) => allPalettes.Exists(b => b.id == id);
        public static int GetBrushIdx(long id)
        {
            var palette = selectedPalette;
            var brushes = palette.brushes;
            for (int i = 0; i < brushes.Length; ++i)
                if (brushes[i].id == id) return i;
            return -1;
        }

        public static int[] idxSelection
        {
            get => instance._idxSelection.ToArray();
            set
            {
                instance._idxSelection = new System.Collections.Generic.HashSet<int>(value);
                if (OnSelectionChanged != null) OnSelectionChanged();
            }
        }
        public static int selectionCount
        {
            get
            {
                if (instance._idxSelection.Count == 0 && instance._selectedBrushIdx > 0 && selectedBrush != null)
                {
                    instance._idxSelection.Add(instance._selectedBrushIdx);
                    if (OnSelectionChanged != null) OnSelectionChanged();
                }
                return instance._idxSelection.Count;
            }
        }
        public static void AddToSelection(int index)
        {
            instance._idxSelection.Add(index);
            if (OnSelectionChanged != null) OnSelectionChanged();
        }
        public static bool SelectionContains(int index) => instance._idxSelection.Contains(index);
        public static void RemoveFromSelection(int index)
        {
            instance._idxSelection.Remove(index);
            if (OnSelectionChanged != null) OnSelectionChanged();
        }
        public static void ClearSelection(bool updateBrushProperties = true)
        {
            selectedBrushIdx = -1;
            instance._idxSelection.Clear();
            if (!updateBrushProperties) return;
            if (OnSelectionChanged != null) OnSelectionChanged();
            BrushProperties.RepaintWindow();
        }
        public static void SelectBrushesToMove()
        {
            instance._movingBrushesFrom = selectedPalette;
            instance._brushesToMove.Clear();
            foreach (var idx in instance._idxSelection) instance._brushesToMove.Add(selectedPalette.GetBrush(idx));
        }
        public static void MoveBrushesToAnotherPalette(PaletteData destinationPalette, bool removeFromSource)
        {
            if (instance._movingBrushesFrom == null) return;
            if (instance._movingBrushesFrom != destinationPalette)
            {
                var sourcePalette = instance._movingBrushesFrom;
                foreach (var brush in instance._brushesToMove)
                {
                    destinationPalette.AddBrush(brush);
                    if (removeFromSource) sourcePalette.RemoveBrush(brush);
                }
            }
            instance._brushesToMove.Clear();
            instance._movingBrushesFrom = null;
        }
        public static void MoveBrushesToSelectedPalette() => MoveBrushesToAnotherPalette(selectedPalette, true);
        public static void PasteBrushesToSelectedPalette() => MoveBrushesToAnotherPalette(selectedPalette, false);
        public static void UpdateSelectedThumbnails()
        {
            foreach (var idx in instance._idxSelection)
                selectedPalette.GetBrush(idx).UpdateThumbnail(updateItemThumbnails: true, savePng: true);
        }

        public static void UpdateAllThumbnails()
        {
            var palettes = allPalettes.ToArray();
            foreach (var palette in palettes) palette.UpdateAllThumbnails();
        }
        public static PaletteData GetPalette(MultibrushSettings brush)
        {
            foreach (var palette in allPalettes)
                if (palette.ContainsBrush(brush)) return palette;
            return null;
        }

        public static PaletteData GetPalette(long id)
        {
            foreach (var palette in allPalettes)
                if (palette.id == id) return palette;
            return null;
        }

        public static string[] GetPaletteThumbnailFolderPaths()
        {
            var paths = new string[allPalettes.Count];
            for (int i = 0; i < paths.Length; ++i) paths[i] = allPalettes[i].thumbnailsFolderPath;
            return paths;
        }
        public static void Cleanup()
        {
            foreach (var palette in allPalettes.ToArray()) palette.Cleanup();
        }
        public void OnBeforeSerialize()
        {
            _selectedPaletteId = _selectedPalette == null ? -1 : _selectedPalette.id;
        }

        public void OnAfterDeserialize()
        {
        }

        private static bool _savePending = false;
        public static bool savePending => _savePending;
        public static void SetSavePending()
        {
            _savePending = true;
        }

        private static void SavePalettes()
        {
            foreach (var palette in allPalettes) palette.Save();
        }
        public static void SaveIfPending()
        {
            if (_savePending) SavePalettes();
            _savePending = false;
        }

        #region CLIPBOARD
        private static BrushSettings _clipboardSettings = null;
        private static ThumbnailSettings _clipboardThumbnailSettings = null;
        public enum Trit { FALSE, TRUE, SAME }
        private static Trit _clipboardOverwriteThumbnailSettings = Trit.FALSE;
        public static BrushSettings clipboardSetting { get => _clipboardSettings; set => _clipboardSettings = value; }
        public static ThumbnailSettings clipboardThumbnailSettings
        { get => _clipboardThumbnailSettings; set => _clipboardThumbnailSettings = value; }
        public static Trit clipboardOverwriteThumbnailSettings
        { get => _clipboardOverwriteThumbnailSettings; set => _clipboardOverwriteThumbnailSettings = value; }

        #endregion
    }

    public class PaletteAssetPostprocessor : UnityEditor.AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool repaintPalette = false;
            var selectedPalette = PaletteManager.selectedPalette;
            foreach (var path in importedAssets)
            {
                if (selectedPalette != null && selectedPalette.ContainsPrefabPath(path))
                {
                    repaintPalette = true;
                    break;
                }
            }
            foreach (var path in deletedAssets)
            {
                if (selectedPalette != null && selectedPalette.ContainsPrefabPath(path))
                {
                    PaletteManager.Cleanup();
                    PaletteManager.ClearSelection();
                    repaintPalette = true;
                    break;
                }
            }
            if (repaintPalette) PrefabPalette.OnChangeRepaint();
        }
    }
    #endregion
}
