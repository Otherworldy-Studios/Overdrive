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
#if UNITY_2022_2_OR_NEWER
using UnityEngine;
namespace PluginMaster
{
    [UnityEditor.Overlays.Overlay(typeof(UnityEditor.SceneView), displayName: "PWB/Shortcuts",
    defaultDisplay: false, defaultDockZone = UnityEditor.Overlays.DockZone.Floating)]

    public class PWBShortcutPanel : UnityEditor.Overlays.Overlay
    {
        private System.Collections.Generic.List<(string Command, string Shortcut)> _shortcutTable
            = new System.Collections.Generic.List<(string Command, string Shortcut)>();
        UnityEngine.UIElements.MultiColumnListView _multiColumnListView = null;

        public PWBShortcutPanel()
        {
            PWBSettings.OnShrotcutProfileChanged += UpdatePanel;
        }

        public override UnityEngine.UIElements.VisualElement CreatePanelContent()
        {
            _multiColumnListView = new UnityEngine.UIElements.MultiColumnListView
            {
                showBoundCollectionSize = false,
                virtualizationMethod = UnityEngine.UIElements.CollectionVirtualizationMethod.DynamicHeight,
                selectionType = UnityEngine.UIElements.SelectionType.None,
            };

            _multiColumnListView.columns.Add(new UnityEngine.UIElements.Column
            {
                title = "Shortcut",
                stretchable = true,
                minWidth = 220,
                makeCell = () => new UnityEngine.UIElements.Label(),
                bindCell = (UnityEngine.UIElements.VisualElement element, int index) =>
                    (element as UnityEngine.UIElements.Label).text = _shortcutTable[index].Shortcut
            });
            _multiColumnListView.columns.Add(new UnityEngine.UIElements.Column
            {
                title = "Command",
                stretchable = true,
                minWidth = 270,
                makeCell = () => new UnityEngine.UIElements.Label(),
                bindCell = (UnityEngine.UIElements.VisualElement element, int index) =>
                    (element as UnityEngine.UIElements.Label).text = _shortcutTable[index].Command
            });
            _multiColumnListView.itemsSource = _shortcutTable;
            UnityEditor.SceneView.duringSceneGui += DuringSceneGUI;
            ToolManager.OnToolChange += OnToolChange;
            return _multiColumnListView;
        }

        private void UpdatePanel()
        {
            _shortcutTable.Clear();
            if (ToolManager.tool == ToolManager.PaintTool.NONE)
            {
                _shortcutTable.AddRange(PWBShortcuts.GetAllShortcuts(PWBShortcut.Group.GLOBAL, PWBShortcut.Group.GRID));
                _shortcutTable.AddRange(PWBShortcuts.GetAllShortcuts(PWBShortcut.Group.GRID, PWBShortcut.Group.NONE));
            }
            else _shortcutTable.AddRange(PWBShortcuts.GetAllShortcuts(GetGroup(), PWBShortcut.Group.NONE));
            if(_multiColumnListView == null) CreatePanelContent();
            _multiColumnListView.Rebuild();
        }

        private void OnToolChange(ToolManager.PaintTool prevTool) => UpdatePanel();

        private static PWBShortcut.Group GetGroup()
        {
            switch (ToolManager.tool)
            {
                case ToolManager.PaintTool.PIN: return PWBShortcut.Group.PIN;
                case ToolManager.PaintTool.BRUSH: return PWBShortcut.Group.BRUSH;
                case ToolManager.PaintTool.GRAVITY: return PWBShortcut.Group.GRAVITY;
                case ToolManager.PaintTool.LINE: return PWBShortcut.Group.LINE;
                case ToolManager.PaintTool.SHAPE: return PWBShortcut.Group.SHAPE;
                case ToolManager.PaintTool.TILING: return PWBShortcut.Group.TILING;
                case ToolManager.PaintTool.REPLACER: return PWBShortcut.Group.REPLACER;
                case ToolManager.PaintTool.ERASER: return PWBShortcut.Group.ERASER;
                case ToolManager.PaintTool.SELECTION: return PWBShortcut.Group.SELECTION;
                case ToolManager.PaintTool.CIRCLE_SELECT: return PWBShortcut.Group.CIRCLE_SELECT;
                default: return PWBShortcut.Group.NONE;
            }
        }

        private void DuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (_shortcutTable.Count == 0) OnToolChange(ToolManager.PaintTool.NONE);
        }
    }
}
#endif