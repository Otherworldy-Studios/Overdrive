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
    #region DATA & SETTIGNS
    [System.Serializable]
    public class CircleSelectSettings : CircleToolBase, ISelectionBrushTool
    {
        [SerializeField] private ModifierToolSettings _modifierTool = new ModifierToolSettings();
        public CircleSelectSettings() => _modifierTool.OnDataChanged += DataChanged;
        public ModifierToolSettings.Command command { get => _modifierTool.command; set => _modifierTool.command = value; }

        public bool onlyTheClosest
        {
            get => _modifierTool.onlyTheClosest;
            set => _modifierTool.onlyTheClosest = value;
        }

        public bool outermostPrefabFilter
        {
            get => _modifierTool.outermostPrefabFilter;
            set => _modifierTool.outermostPrefabFilter = value;
        }
        public override void Copy(IToolSettings other)
        {
            var otherCircleSelectSettings = other as CircleSelectSettings;
            if (otherCircleSelectSettings == null) return;
            base.Copy(other);
            _modifierTool.Copy(otherCircleSelectSettings);
        }
    }

    [System.Serializable]
    public class CircleSelectManager : ToolManagerBase<CircleSelectSettings> { }
    #endregion
    #region PWBIO
    public static partial class PWBIO
    {
        private static Material _transparentBlueMaterial = null;
        public static Material transparentBlueMaterial
        {
            get
            {
                if (_transparentBlueMaterial == null)
                    _transparentBlueMaterial = new Material(Shader.Find("PluginMaster/TransparentBlue"));
                return _transparentBlueMaterial;
            }
        }

        private static System.Collections.Generic.HashSet<GameObject> _toSelect
            = new System.Collections.Generic.HashSet<GameObject>();
        private static void CircleSelectDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            CircleSelectMouseEvents();
            var mousePos = Event.current.mousePosition;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePos);

            var center = mouseRay.GetPoint(_lastHitDistance);
            if (PWBToolRaycast(mouseRay, out RaycastHit mouseHit, out GameObject collider,
                float.MaxValue, -1, paintOnPalettePrefabs: true, castOnMeshesWithoutCollider: true,
                ignoreSceneColliders: true, createTempColliders: true))
            {
                _lastHitDistance = mouseHit.distance;
                center = mouseHit.point;
            }
            DrawCircleTool(center, sceneView.camera, new Color(0.455f, 0.596f, 0.8f, 1f), CircleSelectManager.settings.radius);
            GetCircleToolTargets(mouseRay, sceneView.camera, CircleSelectManager.settings,
                CircleSelectManager.settings.radius, _toSelect);
            DrawObjectsToSelect(sceneView.camera);
        }

        private static void DrawObjectsToSelect(Camera camera)
        {
            if (Event.current.type != EventType.Repaint) return;
            foreach (var obj in _toSelect)
            {
                var filters = obj.GetComponentsInChildren<MeshFilter>();
                foreach (var filter in filters)
                {
                    var mesh = filter.sharedMesh;
                    if (mesh == null) continue;
                    for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; ++subMeshIdx)
                        Graphics.DrawMesh(mesh, filter.transform.localToWorldMatrix,
                            transparentBlueMaterial, 0, camera, subMeshIdx);
                }
            }
        }

        private static void CircleSelectMouseEvents()
        {
            if (Event.current.button == 0 && !Event.current.alt
                && (Event.current.type == EventType.MouseDown
                || (Event.current.type == EventType.MouseDrag && !Event.current.control)))
            {
#if UNITY_2021_1_OR_NEWER
                using (UnityEngine.Pool.HashSetPool<Object>
                    .Get(out System.Collections.Generic.HashSet<Object> selectedObjectsSet))
#else
                var selectedObjectsSet = new System.Collections.Generic.HashSet<Object>();
#endif
                {
                    if (Event.current.shift || Event.current.control)
                    {
                        selectedObjectsSet.UnionWith(UnityEditor.Selection.objects);
                        if (Event.current.control)
                        {
                            selectedObjectsSet.ExceptWith(_toSelect);
                            var selectedGameObjects
                                = UnityEditor.Selection.GetFiltered<GameObject>(UnityEditor.SelectionMode.Unfiltered);
                            _toSelect.ExceptWith(selectedGameObjects);
                        }
                    }
                    selectedObjectsSet.UnionWith(_toSelect);
                    UnityEditor.Selection.objects = selectedObjectsSet.ToArray();
                    Event.current.Use();
                }
            }
        }
    }
#endregion
}
