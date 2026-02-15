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
    [UnityEditor.InitializeOnLoad]
    public class RenderPipelineDefine
    {
        private const string _sesionStateKey = "PWB_LastPipeline";
        static RenderPipelineDefine()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
                var pipeline = GetCurrentRenderPipeline();
                var lastPipeLine = UnityEditor.SessionState.GetString(_sesionStateKey, string.Empty);
                if (pipeline == lastPipeLine) return;
                SetRenderPipelineDefineSymbol(pipeline);
            };
        }
        private static string GetCurrentRenderPipeline()
        {
            var currentRenderPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (currentRenderPipeline != null)
            {
                if (currentRenderPipeline.GetType().ToString().Contains("HighDefinition")) return "HDRP";
                if (currentRenderPipeline.GetType().ToString().Contains("Universal")) return "URP";
            }
            return "BIRP";
        }
        private static void SetRenderPipelineDefineSymbol(string pipeline)
        {
            string define = $"PWB_{pipeline}";
            var target = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(target);
#if UNITY_2022_2_OR_NEWER
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            var definesSCSV = UnityEditor.PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            var definesSCSV = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif
            var defines = definesSCSV.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var d in defines) if (d.Trim() == define) return;
            definesSCSV = string.IsNullOrEmpty(definesSCSV) ? define : definesSCSV + ";" + define;
#if UNITY_2022_2_OR_NEWER
            UnityEditor.PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, definesSCSV);
#else
            UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, definesSCSV);
#endif
            UnityEditor.SessionState.SetString(_sesionStateKey, pipeline);
        }

        public static void SetRenderPipelineDefineSymbol()
        {
            var pipeline = GetCurrentRenderPipeline();
            SetRenderPipelineDefineSymbol(pipeline);
        }
    }

    public class ThumbnailUtils
    {
        private static LayerMask layerMask => 1 << PWBCore.staticData.thumbnailLayer;
        private const int MULTIBRUSH_SIZE = 256;
        public const int SIZE = 256;
        private const int MIN_SIZE = 24;
        private static Texture2D _emptyTexture = null;
        private static bool _savingImage = false;
        public static bool savingImage => _savingImage;
        private class ThumbnailEditor
        {
            public ThumbnailSettings settings = null;
            public GameObject root = null;
            public Camera camera = null;
            public RenderTexture renderTexture = null;
            public Light light = null;
            public Transform pivot = null;
            public GameObject target = null;
#if PWB_HDRP
            public UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData HDCamData = null;
            public UnityEngine.Rendering.Volume volume = null;
            public BoxCollider volumeCollider = null;
#endif
        }

        public static void RenderTextureToTexture2D(RenderTexture renderTexture, Texture2D texture)
        {
            var prevActive = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, SIZE, SIZE), 0, 0);
            texture.Apply();
            RenderTexture.active = prevActive;
        }

        private static Texture2D emptyTexture
        {
            get
            {
                if (_emptyTexture == null) _emptyTexture = Resources.Load<Texture2D>("Sprites/Empty");
                return _emptyTexture;
            }
        }
        public static void SavePngResource(Texture2D texture, string thumbnailPath)
        {
            if (texture == null || string.IsNullOrEmpty(thumbnailPath)) return;
            _savingImage = true;
            byte[] buffer = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(thumbnailPath, buffer);
            UnityEditor.AssetDatabase.Refresh();
            _savingImage = false;
        }

        public static Texture2D ScaleImage(string imagePath)
        {
            if (!System.IO.File.Exists(imagePath)) return null;
            var rawData = System.IO.File.ReadAllBytes(imagePath);
            Texture2D source = new Texture2D(2, 2);
            ImageConversion.LoadImage(source, rawData);
            RenderTexture renderTexture = RenderTexture.GetTemporary(SIZE, SIZE);
            Graphics.Blit(source, renderTexture);
            Texture2D scaledTexture = new Texture2D(SIZE, SIZE);
            RenderTextureToTexture2D(renderTexture, scaledTexture);
            return scaledTexture;
        }

        public static void CopyTexture(Texture2D from, out Texture2D to)
        {
            if (from == null)
            {
                to = null;
                return;
            }
            to = new Texture2D(from.width, from.height);
            to.SetPixels(from.GetPixels());
            to.Apply();
        }

        private static Material _bgMaterial = null;
        private static Cubemap _defaultCubemap = null;
        private static ThumbnailEditor _thumbnailEditor = null;
        public static void UpdateThumbnail(ThumbnailSettings settings,
            Texture2D thumbnailTexture, GameObject prefab, string thumbnailPath, bool savePng)
        {
            var magnitude = BoundsUtils.GetMagnitude(prefab.transform);
            if (_thumbnailEditor == null) _thumbnailEditor = new ThumbnailEditor();
            if (_thumbnailEditor.root != null) _thumbnailEditor.root.SetActive(true);
            _thumbnailEditor.settings = new ThumbnailSettings(settings);

            if (magnitude == 0)
            {
                if (_emptyTexture == null) _emptyTexture = Resources.Load<Texture2D>("Sprites/Empty");
                var pixels = _emptyTexture.GetPixels32();
                for (int i = 0; i < pixels.Length; ++i)
                {
                    if (pixels[i].a == 0) pixels[i] = _thumbnailEditor.settings.backgroudColor;
                }
                thumbnailTexture.SetPixels32(pixels);
                thumbnailTexture.Apply();
                return;
            }
#if UNITY_2022_2_OR_NEWER
            var foundLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
            var foundLights = Object.FindObjectsOfType<Light>();
#endif
            var sceneLights = new System.Collections.Generic.Dictionary<Light, int>(foundLights.Length);
            for (int i = 0; i < foundLights.Length; ++i)
                sceneLights.Add(foundLights[i], foundLights[i].cullingMask);

            const string rootName = "PWBThumbnailEditor";

            do
            {
                var obj = GameObject.Find(rootName);
                if (obj == null) break;
                else GameObject.DestroyImmediate(obj);
            } while (true);
            if (_thumbnailEditor.root == null) _thumbnailEditor.root = new GameObject();
            _thumbnailEditor.root.name = rootName;
            if (_thumbnailEditor.camera == null)
            {
                var camObj = new GameObject("PWBThumbnailEditorCam");
                _thumbnailEditor.camera = camObj.AddComponent<Camera>();
            }
            _thumbnailEditor.camera.transform.SetParent(_thumbnailEditor.root.transform);
            _thumbnailEditor.camera.transform.localPosition = new Vector3(0f, 1.2f, -4f);
            _thumbnailEditor.camera.transform.localRotation = Quaternion.Euler(17.5f, 0f, 0f);
            _thumbnailEditor.camera.fieldOfView = 20f;
            _thumbnailEditor.camera.clearFlags = CameraClearFlags.SolidColor;
            _thumbnailEditor.camera.backgroundColor = _thumbnailEditor.settings.backgroudColor;
            _thumbnailEditor.camera.cullingMask = layerMask;
            _thumbnailEditor.renderTexture = new RenderTexture(SIZE, SIZE, 24);
            _thumbnailEditor.camera.targetTexture = _thumbnailEditor.renderTexture;

            var originalAmbientMode = RenderSettings.ambientMode;
            var originalAmbientLight = RenderSettings.ambientLight;
            var originalAmbientEquatorColor = RenderSettings.ambientEquatorColor;
            var originalAmbientGroundColor = RenderSettings.ambientGroundColor;
            var originalAmbientSkyColor = RenderSettings.ambientSkyColor;
            var originalAmbientIntensity = RenderSettings.ambientIntensity;
            var originalAmbientProbe = RenderSettings.ambientProbe;
            var originalReflectionMode = RenderSettings.defaultReflectionMode;
            var originalSkybox = RenderSettings.skybox;
            var originalFog = RenderSettings.fog;
            var originalFogColor = RenderSettings.fogColor;
            var originalFogStartDistance = RenderSettings.fogStartDistance;
            var originalFogEndDistance = RenderSettings.fogEndDistance;
            var originalFogDensity = RenderSettings.fogDensity;
            var originalFogMode = RenderSettings.fogMode;
            var originalHaloStrength = RenderSettings.haloStrength;
            var originalFlareFadeSpeed = RenderSettings.flareFadeSpeed;
            var originalFlareStrength = RenderSettings.flareStrength;
            var originalReflectionIntensity = RenderSettings.reflectionIntensity;
            var originalReflectionBounces = RenderSettings.reflectionBounces;
            var originalDefaultReflectionResolution = RenderSettings.defaultReflectionResolution;
            var originalSubtractiveShadowColor = RenderSettings.subtractiveShadowColor;
            var originalSun = RenderSettings.sun;
#if UNITY_2022_2_OR_NEWER
            var originalReflectionTexture = RenderSettings.customReflectionTexture;
#else
            var originalReflectionTexture = RenderSettings.customReflection;
#endif
            float intensityMultiplier = 0.7f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = _thumbnailEditor.settings.lightColor;
            RenderSettings.ambientEquatorColor = _thumbnailEditor.settings.backgroudColor;
            RenderSettings.ambientGroundColor = _thumbnailEditor.settings.backgroudColor;
            RenderSettings.ambientSkyColor = _thumbnailEditor.settings.backgroudColor;
            RenderSettings.ambientIntensity = _thumbnailEditor.settings.lightIntensity * intensityMultiplier;
            RenderSettings.ambientProbe = new UnityEngine.Rendering.SphericalHarmonicsL2();
            RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
            RenderSettings.skybox = null;
            RenderSettings.fog = false;
            RenderSettings.fogColor = Color.clear;
            RenderSettings.fogStartDistance = 0f;
            RenderSettings.fogEndDistance = 1f;
            RenderSettings.fogDensity = 0f;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.haloStrength = 0f;
            RenderSettings.flareFadeSpeed = 1f;
            RenderSettings.flareStrength = 0f;
            RenderSettings.reflectionIntensity = _thumbnailEditor.settings.lightIntensity * intensityMultiplier;
            RenderSettings.reflectionBounces = 1;
            RenderSettings.defaultReflectionResolution = 128;
            RenderSettings.subtractiveShadowColor = Color.black;
            RenderSettings.sun = null;
            if (_defaultCubemap == null)
            {
                _defaultCubemap = new Cubemap(1, TextureFormat.RGB24, false);
                Color[] colors = { Color.white };
                for (int face = 0; face < 6; face++)
                {
                    _defaultCubemap.SetPixels(colors, (CubemapFace)face);
                }
                _defaultCubemap.Apply();
            }
#if UNITY_2022_2_OR_NEWER
            RenderSettings.customReflectionTexture = _defaultCubemap;
#else
            RenderSettings.customReflection = _defaultCubemap;
#endif

            if (_thumbnailEditor.light == null)
            {
                var lightObj = new GameObject("PWBThumbnailEditorLight");
                _thumbnailEditor.light = lightObj.AddComponent<Light>();
            }
            _thumbnailEditor.light.type = LightType.Directional;
            _thumbnailEditor.light.transform.SetParent(_thumbnailEditor.root.transform);
            _thumbnailEditor.light.transform.localRotation = Quaternion.Euler(_thumbnailEditor.settings.lightEuler);
            _thumbnailEditor.light.color = _thumbnailEditor.settings.lightColor;
            _thumbnailEditor.light.intensity = _thumbnailEditor.settings.lightIntensity;
            _thumbnailEditor.light.cullingMask = layerMask;
            if (_thumbnailEditor.pivot == null)
            {
                var pivotObj = new GameObject("PWBThumbnailEditorPivot");
                _thumbnailEditor.pivot = pivotObj.transform;
            }
            _thumbnailEditor.pivot.gameObject.layer = PWBCore.staticData.thumbnailLayer;
            _thumbnailEditor.pivot.transform.SetParent(_thumbnailEditor.root.transform);
            _thumbnailEditor.pivot.localPosition = _thumbnailEditor.settings.targetOffset;
            _thumbnailEditor.pivot.transform.localRotation = Quaternion.identity;
            _thumbnailEditor.pivot.transform.localScale = Vector3.one;

            Transform InstantiateBones(Transform source, Transform parent)
            {
                var obj = new GameObject();
                obj.name = source.name;
                obj.transform.SetParent(parent);
                obj.transform.position = source.position;
                obj.transform.rotation = source.rotation;
                obj.transform.localScale = source.localScale;
                foreach (Transform child in source) InstantiateBones(child, obj.transform);
                return obj.transform;
            }

            bool Requires(System.Type obj, System.Type requirement)
            {
                if (!System.Attribute.IsDefined(obj, typeof(RequireComponent)))
                    return false;

                var attrs = System.Attribute.GetCustomAttributes(obj, typeof(RequireComponent));
                for (int i = 0; i < attrs.Length; ++i)
                {
                    var rc = attrs[i] as RequireComponent;
                    if (rc != null && rc.m_Type0 != null && rc.m_Type0.IsAssignableFrom(requirement))
                        return true;
                }
                return false;
            }

            bool CanDestroy(GameObject go, System.Type t)
            {
                var comps = go.GetComponents<Component>();
                for (int i = 0; i < comps.Length; ++i)
                    if (Requires(comps[i].GetType(), t))
                        return false;
                return true;
            }

            void CopyComponents(GameObject source, GameObject destination)
            {
                var srcComps = source.GetComponentsInChildren<Component>();
                foreach (var srcComp in srcComps)
                {
                    if (srcComp is MonoBehaviour) continue;
                    var destComp = srcComp is Transform ? destination.transform : destination.AddComponent(srcComp.GetType());
                    UnityEditor.EditorUtility.CopySerialized(srcComp, destComp);
                }
                foreach (Transform srcChild in source.transform)
                {
                    var destChild = new GameObject();
                    destChild.transform.SetParent(destination.transform);
                    CopyComponents(srcChild.gameObject, destChild);
                }
            }

            GameObject InstantiateAndRemoveMonoBehaviours()
            {
                var obj = Object.Instantiate(prefab);
                var toBeDestroyed = new System.Collections.Generic.List<Component>(obj.GetComponentsInChildren<Component>());

                while (toBeDestroyed.Count > 0)
                {
                    var components = toBeDestroyed.ToArray();
                    int compCount = components.Length;
                    toBeDestroyed.Clear();
                    foreach (var comp in components)
                    {
                        if (comp is MonoBehaviour)
                        {
                            var monoBehaviour = comp as MonoBehaviour;
                            monoBehaviour.enabled = false;
                            monoBehaviour.runInEditMode = false;
                            if (CanDestroy(comp.gameObject, comp.GetType())) Object.DestroyImmediate(comp);
                            else toBeDestroyed.Add(comp);
                        }
                    }
                    if (compCount == toBeDestroyed.Count) break;
                }
                if (toBeDestroyed.Count > 0)
                {
                    var noMonoBehaviourObj = new GameObject();
                    CopyComponents(noMonoBehaviourObj, obj);
                    Object.DestroyImmediate(obj);
                    obj = noMonoBehaviourObj;
                }
                return obj;
            }

            _thumbnailEditor.target = InstantiateAndRemoveMonoBehaviours();

            var monoBehaviours = _thumbnailEditor.target.GetComponentsInChildren<MonoBehaviour>();
            foreach (var monoBehaviour in monoBehaviours)
                if (monoBehaviour != null) monoBehaviour.enabled = false;

            magnitude = BoundsUtils.GetMagnitude(_thumbnailEditor.target.transform);
            var targetScale = magnitude > 0 ? 1f / magnitude : 1f;
            var targetBounds = BoundsUtils.GetBoundsRecursive(_thumbnailEditor.target.transform);
            var localPosition = (_thumbnailEditor.target.transform.localPosition - targetBounds.center) * targetScale;
            _thumbnailEditor.target.transform.SetParent(_thumbnailEditor.pivot);
            _thumbnailEditor.target.transform.localPosition = localPosition;
            _thumbnailEditor.target.transform.localRotation = Quaternion.identity;
            _thumbnailEditor.target.transform.localScale = prefab.transform.localScale * targetScale;
            _thumbnailEditor.pivot.localScale = Vector3.one * _thumbnailEditor.settings.zoom;
            _thumbnailEditor.pivot.localRotation = Quaternion.Euler(_thumbnailEditor.settings.targetEuler);

            var bgObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bgObject.name = "PWBThumbnailEditorBg";
            if (_bgMaterial == null)
            {
#if PWB_HDRP
                _bgMaterial = new Material(Shader.Find("HDRP/Unlit"));
#else
                _bgMaterial = new Material(Shader.Find("Unlit/Color"));
#endif
            }
            _bgMaterial.color = _thumbnailEditor.settings.backgroudColor;

            var bgRenderer = bgObject.GetComponent<MeshRenderer>();
            bgRenderer.sharedMaterial = _bgMaterial;
            bgObject.transform.SetParent(_thumbnailEditor.root.transform);
            bgObject.transform.localPosition = new Vector3(0, -3, 10);
            bgObject.transform.localScale = new Vector3(30, 30, 0.1f);


#if PWB_HDRP || PWB_URP
#if UNITY_2022_2_OR_NEWER
            var foundVolumes = Object.FindObjectsByType<UnityEngine.Rendering.Volume>(FindObjectsSortMode.None);
#else
            var foundVolumes = Object.FindObjectsOfType<UnityEngine.Rendering.Volume>();    
#endif
            var sceneVolumes
                = new System.Collections.Generic.Dictionary<UnityEngine.Rendering.Volume, bool>(foundVolumes.Length);
            for (int i = 0; i < foundVolumes.Length; ++i)
            {
                sceneVolumes.Add(foundVolumes[i], foundVolumes[i].isActiveAndEnabled);
                foundVolumes[i].gameObject.SetActive(false);
            }

            var meshRenderersArray = _thumbnailEditor.target.GetComponentsInChildren<MeshRenderer>();
            var meshRenderers = new System.Collections.Generic.Dictionary<MeshRenderer,
                UnityEngine.Rendering.LightProbeUsage>(meshRenderersArray.Length);
            for (int i = 0; i < meshRenderersArray.Length; ++i)
            {
                meshRenderers.Add(meshRenderersArray[i], meshRenderersArray[i].lightProbeUsage);
                meshRenderersArray[i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            }

            var skinnedMeshRenderArray = _thumbnailEditor.target.GetComponentsInChildren<SkinnedMeshRenderer>();
            var skinnedMeshRenderers = new System.Collections.Generic.Dictionary<SkinnedMeshRenderer,
                UnityEngine.Rendering.LightProbeUsage>(skinnedMeshRenderArray.Length);
            for (int i = 0; i < skinnedMeshRenderArray.Length; ++i)
            {
                skinnedMeshRenderers.Add(skinnedMeshRenderArray[i], skinnedMeshRenderArray[i].lightProbeUsage);
                skinnedMeshRenderArray[i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            }
#endif

#if PWB_HDRP
            if (_thumbnailEditor.HDCamData == null)
            {
                _thumbnailEditor.HDCamData = _thumbnailEditor.camera.gameObject
                    .AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
            }
            _thumbnailEditor.HDCamData.volumeLayerMask = layerMask | 1;
            _thumbnailEditor.HDCamData.probeLayerMask = 0;
            _thumbnailEditor.HDCamData.clearColorMode
                = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.ClearColorMode.Color;
            _thumbnailEditor.HDCamData.backgroundColorHDR = _thumbnailEditor.settings.backgroudColor;
            _thumbnailEditor.HDCamData.antialiasing
                = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
            if (_thumbnailEditor.volume == null)
            {
                var volumeObj = new GameObject("PWBThumbnailEditorVolume");
                volumeObj.transform.SetParent(_thumbnailEditor.root.transform);
                _thumbnailEditor.volume = volumeObj.AddComponent<UnityEngine.Rendering.Volume>();
                _thumbnailEditor.volumeCollider = _thumbnailEditor.volume.gameObject.AddComponent<BoxCollider>();
            }
            _thumbnailEditor.volume.isGlobal = false;
            _thumbnailEditor.volume.priority = 1;
            _thumbnailEditor.volume.profile = Resources.Load<UnityEngine.Rendering.VolumeProfile>("ThumbnailVolume");
            UnityEngine.Rendering.HighDefinition.Exposure exposure = null;
            if (!_thumbnailEditor.volume.profile.Has<UnityEngine.Rendering.HighDefinition.Exposure>())
                exposure = _thumbnailEditor.volume.profile.Add<UnityEngine.Rendering.HighDefinition.Exposure>(true);
            else _thumbnailEditor.volume.profile.TryGet<UnityEngine.Rendering.HighDefinition.Exposure>(out exposure);
            if (exposure != null)
            {
                exposure.mode.value = UnityEngine.Rendering.HighDefinition.ExposureMode.AutomaticHistogram;
                exposure.meteringMode.value = UnityEngine.Rendering.HighDefinition.MeteringMode.CenterWeighted;
                exposure.limitMin.Override(13f);
                exposure.limitMax.Override(15f);
                exposure.compensation.Override(_thumbnailEditor.light.intensity);
            }

            _thumbnailEditor.volumeCollider.size = new Vector3(50, 50, 50);
#endif

            _thumbnailEditor.root.transform.position = new Vector3(10000, 10000, 10000);

            var children = _thumbnailEditor.root.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                child.gameObject.layer = PWBCore.staticData.thumbnailLayer;
                child.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }

            foreach (var light in sceneLights.Keys) light.cullingMask = light.cullingMask & ~layerMask;

            for (int i = 0; i < 9; ++i) _thumbnailEditor.camera.Render();

            foreach (var light in sceneLights.Keys) light.cullingMask = sceneLights[light];
#if PWB_HDRP  || PWB_URP
            foreach (var vol in sceneVolumes) vol.Key.gameObject.SetActive(vol.Value);
            foreach (var meshRenderer in meshRenderers) meshRenderer.Key.lightProbeUsage = meshRenderer.Value;
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                skinnedMeshRenderer.Key.lightProbeUsage = skinnedMeshRenderer.Value;
#endif

            RenderTextureToTexture2D(_thumbnailEditor.camera.targetTexture, thumbnailTexture);

            RenderSettings.ambientMode = originalAmbientMode;
            RenderSettings.ambientLight = originalAmbientLight;
            RenderSettings.ambientEquatorColor = originalAmbientEquatorColor;
            RenderSettings.ambientGroundColor = originalAmbientGroundColor;
            RenderSettings.ambientSkyColor = originalAmbientSkyColor;
            RenderSettings.ambientIntensity = originalAmbientIntensity;
            RenderSettings.ambientProbe = originalAmbientProbe;
            RenderSettings.defaultReflectionMode = originalReflectionMode;
            RenderSettings.skybox = originalSkybox;
            RenderSettings.fog = originalFog;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogStartDistance = originalFogStartDistance;
            RenderSettings.fogEndDistance = originalFogEndDistance;
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.fogMode = originalFogMode;
            RenderSettings.haloStrength = originalHaloStrength;
            RenderSettings.flareFadeSpeed = originalFlareFadeSpeed;
            RenderSettings.flareStrength = originalFlareStrength;
            RenderSettings.reflectionIntensity = originalReflectionIntensity;
            RenderSettings.reflectionBounces = originalReflectionBounces;
            RenderSettings.defaultReflectionResolution = originalDefaultReflectionResolution;
            RenderSettings.subtractiveShadowColor = originalSubtractiveShadowColor;
            RenderSettings.sun = originalSun;

#if UNITY_2022_2_OR_NEWER
            RenderSettings.customReflectionTexture = originalReflectionTexture;
#else
            RenderSettings.customReflection = originalReflectionTexture;
#endif
            if (_thumbnailEditor.camera != null) _thumbnailEditor.camera.targetTexture = null;
            if (_thumbnailEditor.renderTexture != null) Object.DestroyImmediate(_thumbnailEditor.renderTexture);
            Object.DestroyImmediate(_thumbnailEditor.target);
            _thumbnailEditor.root.SetActive(false);
            if (savePng) SavePngResource(thumbnailTexture, thumbnailPath);
        }


        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterPlayModeStateChangedCallback()
            => UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state != UnityEditor.PlayModeStateChange.ExitingEditMode) return;
            if (_thumbnailEditor == null) return;

            if (_thumbnailEditor.camera != null)
            {
                _thumbnailEditor.camera.targetTexture = null;
                Object.DestroyImmediate(_thumbnailEditor.camera.gameObject);
            }

            if (_thumbnailEditor.renderTexture != null)
                Object.DestroyImmediate(_thumbnailEditor.renderTexture);

            if (_thumbnailEditor.light != null)
                Object.DestroyImmediate(_thumbnailEditor.light.gameObject);

            if (_thumbnailEditor.root != null)
                Object.DestroyImmediate(_thumbnailEditor.root);

            if (_bgMaterial != null)
            {
                Object.DestroyImmediate(_bgMaterial);
                _bgMaterial = null;
            }

            if (_defaultCubemap != null)
            {
                Object.DestroyImmediate(_defaultCubemap);
                _defaultCubemap = null;
            }
        }

        public static void UpdateThumbnail(ThumbnailSettings settings,
            Texture2D thumbnailTexture, Texture2D[] subThumbnails, string thumbnailPath, bool savePng)
        {
            if (subThumbnails.Length == 0)
            {
                thumbnailTexture.SetPixels(new Color[SIZE * SIZE]);
                thumbnailTexture.Apply();
                return;
            }

            var sqrt = Mathf.Sqrt(subThumbnails.Length);
            var sideCellsCount = Mathf.FloorToInt(sqrt);
            if (Mathf.CeilToInt(sqrt) != sideCellsCount) ++sideCellsCount;
            var spacing = (SIZE * sideCellsCount) / MIN_SIZE;
            var bigSize = SIZE * sideCellsCount + spacing * (sideCellsCount - 1);
            var texture = new Texture2D(bigSize, bigSize);
            var pixelCount = bigSize * bigSize;
            var pixels = new Color32[pixelCount];
            texture.SetPixels32(pixels);
            int subIdx = 0;
            bool finished = false;
            for (int i = sideCellsCount - 1; i >= 0 && !finished; --i)
            {
                for (int j = 0; j < sideCellsCount && !finished; ++j)
                {
                    var x = j * (SIZE + spacing);
                    var y = i * (SIZE + spacing);
                    if (subThumbnails[subIdx] == null) continue;
                    var subPixels = subThumbnails[subIdx].GetPixels32();
                    texture.SetPixels32(x, y, SIZE, SIZE, subPixels);
                    ++subIdx;
                    if (subIdx == subThumbnails.Length) finished = true;
                }
            }
            texture.filterMode = FilterMode.Trilinear;
            texture.Apply();
            var renderTexture = new RenderTexture(MULTIBRUSH_SIZE, MULTIBRUSH_SIZE, 24);
            var prevActive = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Graphics.Blit(texture, renderTexture);
            if (thumbnailTexture.width != MULTIBRUSH_SIZE || thumbnailTexture.height != MULTIBRUSH_SIZE)
                thumbnailTexture.Reinitialize(MULTIBRUSH_SIZE, MULTIBRUSH_SIZE);
            thumbnailTexture.ReadPixels(new Rect(0, 0, MULTIBRUSH_SIZE, MULTIBRUSH_SIZE), 0, 0);
            thumbnailTexture.Apply();
            RenderTexture.active = prevActive;
            Object.DestroyImmediate(texture);
            if (savePng) SavePngResource(thumbnailTexture, thumbnailPath);
        }

        public static void UpdateThumbnail(MultibrushItemSettings brushItem, bool savePng, bool updateParent)
        {
            if (brushItem.thumbnailSettings.useCustomImage)
            {
                brushItem.LoadThumbnailFromFile();
                return;
            }
            if (brushItem.prefab == null) return;

            if (PWBCore.staticData.useAssetPreview)
            {
                var preview = UnityEditor.AssetPreview.GetAssetPreview(brushItem.prefab);
                if (preview != null)
                {
                    CopyTexture(preview, out Texture2D thumbnailTexture);
                    var rt = RenderTexture.GetTemporary(SIZE, SIZE);
                    Graphics.Blit(thumbnailTexture, rt);
                    var resizedTexture = new Texture2D(SIZE, SIZE, thumbnailTexture.format, false);
                    RenderTexture.active = rt;
                    resizedTexture.ReadPixels(new Rect(0, 0, SIZE, SIZE), 0, 0);
                    resizedTexture.Apply();
                    RenderTexture.active = null;
                    RenderTexture.ReleaseTemporary(rt);
                    brushItem.SetCustomThumbnailTexture(resizedTexture, savePng);
                    if (updateParent)
                        UpdateThumbnail(brushItem.parentSettings, updateItemThumbnails: false, savePng);
                }
                return;
            }
            UpdateThumbnail(brushItem.thumbnailSettings, brushItem.thumbnailTexture,
                brushItem.prefab, brushItem.thumbnailPath, savePng);
            if (updateParent)
                UpdateThumbnail(brushItem.parentSettings, updateItemThumbnails: false, savePng);
        }

        public static void UpdateThumbnail(MultibrushSettings brushSettings, bool updateItemThumbnails, bool savePng)
        {
            if (brushSettings.thumbnailSettings.useCustomImage) return;
            var brushItems = brushSettings.items;
            var subThumbnails = new System.Collections.Generic.List<Texture2D>();
            foreach (var item in brushItems)
            {
                if (updateItemThumbnails) UpdateThumbnail(item, savePng, updateParent: false);
                if (item.includeInThumbnail) subThumbnails.Add(item.thumbnail);
            }
            UpdateThumbnail(brushSettings.thumbnailSettings, brushSettings.thumbnailTexture,
                subThumbnails.ToArray(), brushSettings.thumbnailPath, savePng);
        }

        public static void UpdateThumbnail(BrushSettings brushItem, bool updateItemThumbnails, bool savePng)
        {
            if (brushItem is MultibrushItemSettings)
                UpdateThumbnail(brushItem as MultibrushItemSettings, savePng, updateParent: true);
            else if (brushItem is MultibrushSettings)
                UpdateThumbnail(brushItem as MultibrushSettings, updateItemThumbnails, savePng);
        }

        public static void DeleteUnusedThumbnails()
        {
            var palettes = PaletteManager.allPalettes;
            bool CheckThumbnailPath(string thumbnailPath)
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(thumbnailPath);
                var ids = fileName.Split('_');
                if (ids.Length > 2) return false;
                long itemId = -1;
                long brushId = -1;
                var provider = new System.Globalization.CultureInfo("en-US");
                if (!long.TryParse(ids[0], System.Globalization.NumberStyles.HexNumber, provider, out brushId)) return false;
                var brush = PaletteManager.GetBrushById(brushId);
                if (brush == null) return false;
                if (ids.Length == 1) return true;
                if (!long.TryParse(ids[1], System.Globalization.NumberStyles.HexNumber, provider, out itemId)) return false;
                return brush.ItemExist(itemId);
            }

            var folderPaths = PaletteManager.GetPaletteThumbnailFolderPaths();
            foreach (var folderPath in folderPaths)
            {
                var thumbnailPaths = System.IO.Directory.GetFiles(folderPath, "*.png");
                foreach (var thumbnailPath in thumbnailPaths)
                {
                    if (!CheckThumbnailPath(thumbnailPath))
                    {
                        System.IO.File.Delete(thumbnailPath);
                        var metapath = thumbnailPath + ".meta";
                        if (System.IO.File.Exists(metapath)) System.IO.File.Delete(metapath);
                        PWBCore.refreshDatabase = true;
                    }
                }
            }
        }
    }
}
