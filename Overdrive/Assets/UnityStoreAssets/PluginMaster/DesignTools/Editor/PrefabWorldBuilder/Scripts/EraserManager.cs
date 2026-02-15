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
    #region DATA & SETTIGNS
    [System.Serializable]
    public class EraserSettings : CircleToolBase, ISelectionBrushTool, IModifierTool
    {
        [SerializeField] private ModifierToolSettings _modifierTool = new ModifierToolSettings();
        public EraserSettings() => _modifierTool.OnDataChanged += DataChanged;
        public ModifierToolSettings.Command command { get => _modifierTool.command; set => _modifierTool.command = value; }

        public bool modifyAllButSelected
        {
            get => _modifierTool.modifyAllButSelected;
            set => _modifierTool.modifyAllButSelected = value;
        }

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
            var otherEraserSettings = other as EraserSettings;
            if (otherEraserSettings == null) return;
            base.Copy(other);
            _modifierTool.Copy(otherEraserSettings);
        }
    }

    [System.Serializable]
    public class EraserManager : ToolManagerBase<EraserSettings> { }
    #endregion
    #region PWBIO
    public static partial class PWBIO
    {
        private static float _lastHitDistance = 20f;

        
        private static System.Collections.Generic.HashSet<GameObject> _toErase
            = new System.Collections.Generic.HashSet<GameObject>();


        private static void EraserMouseEvents()
        {
            if (Event.current.button == 0 && !Event.current.alt
                && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag))
            {
                Erase();
                Event.current.Use();
            }
            if (Event.current.button == 1)
            {
                if (Event.current.type == EventType.MouseDown && (Event.current.control || Event.current.shift))
                {
                    _pinned = true;
                    _pinMouse = Event.current.mousePosition;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp) _pinned = false;
            }
        }

        private static void Erase()
        {
            void EraseObject(GameObject obj)
            {
                if (obj == null) return;
                if (EraserManager.settings.outermostPrefabFilter)
                {
                    var root = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(obj);
                    if (root != null) obj = root;
                }
                else
                {
                    var parent = obj.transform.parent.gameObject;
                    if (parent != null)
                    {
                        GameObject outermost = null;
                        do
                        {
                            outermost = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                            if (outermost == null) break;
                            if (outermost == obj) break;
                            UnityEditor.PrefabUtility.UnpackPrefabInstance(outermost,
                                UnityEditor.PrefabUnpackMode.OutermostRoot, UnityEditor.InteractionMode.UserAction);
                        } while (outermost != parent);
                    }
                }
                PWBCore.DestroyTempCollider(obj.GetInstanceID());
                UnityEditor.Undo.DestroyObjectImmediate(obj);
            }
            foreach (var obj in _toErase) EraseObject(obj);
            _toErase.Clear();
        }

        private static void EraserDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            EraserMouseEvents();
            var mousePos = Event.current.mousePosition;
            if (_pinned) mousePos = _pinMouse;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePos);

            var center = mouseRay.GetPoint(_lastHitDistance);
            if (PWBToolRaycast(mouseRay, out RaycastHit mouseHit, out GameObject collider,
                float.MaxValue, -1, paintOnPalettePrefabs:true, castOnMeshesWithoutCollider:true, ignoreSceneColliders: true,
                createTempColliders: true))
            {
                _lastHitDistance = mouseHit.distance;
                center = mouseHit.point;
            }
            DrawCircleTool(center, sceneView.camera, new Color(1f, 0.0f, 0, 1f), EraserManager.settings.radius);
            GetCircleToolTargets(mouseRay, sceneView.camera, EraserManager.settings,EraserManager.settings.radius, _toErase);
            DrawObjectsToErase(sceneView.camera);
        }
        private static void DrawObjectsToErase(Camera camera)
        {
            if (Event.current.type != EventType.Repaint) return;
            foreach (var obj in _toErase)
            {
                var filters = obj.GetComponentsInChildren<MeshFilter>();
                foreach (var filter in filters)
                {
                    var mesh = filter.sharedMesh;
                    if (mesh == null) continue;
                    for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; ++subMeshIdx)
                        Graphics.DrawMesh(mesh, filter.transform.localToWorldMatrix,
                            transparentRedMaterial, 0, camera, subMeshIdx);
                }
            }
        }
    }
    #endregion
}
