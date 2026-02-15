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
    public class PrefabPalette : UnityEditor.EditorWindow, ISerializationCallbackReceiver
    {
        #region COMMON
        private GUISkin _skin = null;

        [SerializeField] private PaletteManager _paletteManager = null;

        private static PrefabPalette _instance = null;
        public static PrefabPalette instance => _instance;
        [UnityEditor.MenuItem("Tools/Plugin Master/Prefab World Builder/Palette...", false, 1110)]
        public static void ShowWindow() => _instance = GetWindow<PrefabPalette>("Palette");
        private static bool _repaint = false;
        public static void RepaintWindow()
        {
            if (_instance != null) _instance.Repaint();
            _repaint = true;
        }

        public static void OnChangeRepaint()
        {
            if (_instance != null)
            {
                _instance.OnPaletteChange();
                RepaintWindow();
            }
        }
        public static void CloseWindow()
        {
            if (_instance != null) _instance.Close();
        }

        private void OnEnable()
        {
            _instance = this;
            _paletteManager = PaletteManager.instance;
            _skin = Resources.Load<GUISkin>("PWBSkin");
            if (_skin == null) return;
            _toggleStyle = _skin.GetStyle("PaletteToggle");
            _loadingIcon = Resources.Load<Texture2D>("Sprites/Loading");
            _toggleStyle.margin = new RectOffset(4, 4, 4, 4);
            _dropdownIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/DropdownArrow"));
            _labelIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/Label"), "Filter by label");
            _selectionFilterIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/SelectionFilter"),
                "Filter by selection");
            _folderFilterIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/FolderFilter"), "Filter by folder");
            _newBrushIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/New"), "New Brush");
            _deleteBrushIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/Delete"), "Delete Brush");
            _pickerIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/Picker"), "Brush Picker");
            _clearFilterIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/Clear"));
            _settingsIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/Settings"));
            _pinTabIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/PinTab"), "Toggle pin status");
            _pinnedTabIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/PinnedTab"), "Toggle pin status");
            _pinTabLightIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/LightTheme/PinTab"), "Toggle pin status");
            _pinnedTabLightIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/LightTheme/PinnedTab"),
                "Toggle pin status");
            _cursorStyle = _skin.GetStyle("Cursor");
            autoRepaintOnSceneChange = true;
            if (PaletteManager.allPalettesCount == 0)
            {
                PWBCore.Initialize();
                PaletteManager.instance.LoadPaletteFiles(true);
                PaletteManager.InitializeSelectedPalette();
            }
            UpdateLabelFilter();
            UpdateFilteredList(false);
            PaletteManager.ClearSelection(false);
            UnityEditor.Undo.undoRedoPerformed += OnPaletteChange;
        }

        private void OnDisable() => UnityEditor.Undo.undoRedoPerformed -= OnPaletteChange;

        private void OnDestroy() => ToolManager.OnPaletteClosed();
        public static void ClearUndo()
        {
            if (_instance == null) return;
            UnityEditor.Undo.ClearUndo(_instance);
        }


        private void OnGUI()
        {
            if (UnityEditor.Lightmapping.isRunning) return;
            if (_skin == null)
            {
                Close();
                return;
            }
            if (PaletteManager.loadPaletteFilesPending)
            {
                PaletteManager.instance.LoadPaletteFiles(true);
                Reload(false);
                UpdateTabBar();
            }
            if (PWBCore.refreshDatabase) PWBCore.AssetDatabaseRefresh();
            if (_contextBrushAdded)
            {
                RegisterUndo("Add Brush");
                PaletteManager.selectedPalette.AddBrush(_newContextBrush);
                _newContextBrush = null;
                PaletteManager.selectedBrushIdx = PaletteManager.selectedPalette.brushes.Length - 1;
                _contextBrushAdded = false;
                OnPaletteChange();
                return;
            }
            try
            {
                TabBar();
                SearchBar();
                Palette();
            }
            catch
            {
                RepaintWindow();
            }
            var eventType = Event.current.rawType;
            if (eventType == EventType.MouseMove || eventType == EventType.MouseUp)
            {
                _moveBrush.to = -1;
                draggingBrush = false;
                _showCursor = false;
            }
            else if (PWBSettings.shortcuts.paletteDeleteBrush.Check()) OnDelete();
            if (PWBSettings.shortcuts.paletteReplaceSceneSelection.Check()) PWBIO.ReplaceSelected();
        }

        private void Update()
        {
            if (mouseOverWindow != this)
            {
                _moveBrush.to = -1;
                _showCursor = false;
            }
            else if (draggingBrush) _showCursor = true;
            if (_repaint)
            {
                _repaint = false;
                Repaint();
            }
            if (_frameSelectedBrush && _newSelectedPositionSet) DoFrameSelectedBrush();
            if (PaletteManager.savePending) PaletteManager.SaveIfPending();
        }
        private void RegisterUndo(string name)
        {
            if (PWBCore.staticData.undoPalette) UnityEditor.Undo.RegisterCompleteObjectUndo(this, name);
        }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            _repaint = true;
            PaletteManager.ClearSelection(false);
        }

        public void UpdateAllThumbnails() => PaletteManager.UpdateAllThumbnails();

        #endregion

        #region PALETTE
        private Vector2 _scrollPosition;
        private Rect _scrollViewRect;
        private Vector2 _prevSize;
        private int _columnCount = 1;
        private GUIStyle _toggleStyle = null;
        private const int MIN_ICON_SIZE = 24;
        private const int MAX_ICON_SIZE = 256;
        public const int DEFAULT_ICON_SIZE = 64;
        private int _prevIconSize = DEFAULT_ICON_SIZE;

        private GUIContent _dropdownIcon = null;
        private bool _draggingBrush = false;
        private bool _showCursor = false;
        private Rect _cursorRect;
        private GUIStyle _cursorStyle = null;
        private (int from, int to, bool perform) _moveBrush = (0, 0, false);

        private bool draggingBrush
        {
            get => _draggingBrush;
            set
            {
                _draggingBrush = value;
                wantsMouseMove = value;
                wantsMouseEnterLeaveWindow = value;
            }
        }

        private void Palette()
        {
            UpdateColumnCount();

            _prevIconSize = PaletteManager.iconSize;

            if (_moveBrush.perform)
            {
                RegisterUndo("Change Brush Order");
                var selection = PaletteManager.idxSelection;
                PaletteManager.selectedPalette.Swap(_moveBrush.from, _moveBrush.to, ref selection);
                PaletteManager.idxSelection = selection;
                if (selection.Length == 1) PaletteManager.selectedBrushIdx = selection[0];
                _moveBrush.perform = false;
                UpdateFilteredList(false);
            }
            BrushInputData toggleData = null;

            using (var scrollView = new UnityEditor.EditorGUILayout.ScrollViewScope(_scrollPosition, false, false,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, _skin.box))
            {
                _scrollPosition = scrollView.scrollPosition;
                Brushes(ref toggleData);
                if (_showCursor) GUI.Box(_cursorRect, string.Empty, _cursorStyle);
            }
            _scrollViewRect = GUILayoutUtility.GetLastRect();
            if (PaletteManager.selectedPalette.brushCount == 0) DropBox();

            Bottom();

            BrushMouseEventHandler(toggleData);
            PaletteContext();
            DropPrefab();
        }

        private void UpdateColumnCount()
        {
            if (PaletteManager.allPalettesCount == 0) return;
            var paletteData = PaletteManager.selectedPalette;
            var brushes = paletteData.brushes;
            if (_scrollViewRect.width > MIN_ICON_SIZE)
            {
                if (_prevSize != position.size || _prevIconSize != PaletteManager.iconSize || _repaint)
                {
                    var iconW = (float)((PaletteManager.iconSize + 4) * brushes.Length + 6) / brushes.Length;
                    _columnCount = Mathf.Max((int)(_scrollViewRect.width / iconW), 1);
                    var rowCount = Mathf.CeilToInt((float)brushes.Length / _columnCount);
                    var h = rowCount * (PaletteManager.iconSize + 4) + 42;

                    if (h > _scrollViewRect.height)
                    {
                        iconW = (float)((PaletteManager.iconSize + 4) * brushes.Length + 17) / brushes.Length;
                        _columnCount = Mathf.Max((int)(_scrollViewRect.width / iconW), 1);
                    }
                }
                _prevSize = position.size;
            }
        }

        public void OnPaletteChange()
        {
            UpdateLabelFilter();
            UpdateFilteredList(false);
            _repaint = true;
            UpdateColumnCount();
            Repaint();
        }
        #endregion

        #region BOTTOM
        private GUIContent _newBrushIcon = null;
        private GUIContent _deleteBrushIcon = null;
        private GUIContent _pickerIcon = null;
        private GUIContent _settingsIcon = null;
        private void Bottom()
        {
            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar, GUILayout.Height(18)))
            {
                if (PaletteManager.selectedPalette.brushCount > 0)
                {
                    var sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
                    sliderStyle.margin.top = 0;
                    PaletteManager.iconSize = (int)GUILayout.HorizontalSlider(
                        (float)PaletteManager.iconSize,
                        (float)MIN_ICON_SIZE,
                        (float)MAX_ICON_SIZE,
                        sliderStyle,
                        GUI.skin.horizontalSliderThumb,
                        GUILayout.MaxWidth(128));
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(_newBrushIcon, UnityEditor.EditorStyles.toolbarButton)) PaletteContextMenu();
                using (new UnityEditor.EditorGUI.DisabledGroupScope(PaletteManager.selectionCount == 0))
                {
                    if (GUILayout.Button(_deleteBrushIcon, UnityEditor.EditorStyles.toolbarButton)) OnDelete();
                }
                PaletteManager.pickingBrushes = GUILayout.Toggle(PaletteManager.pickingBrushes,
                    _pickerIcon, UnityEditor.EditorStyles.toolbarButton);
                if (GUILayout.Button(_settingsIcon, UnityEditor.EditorStyles.toolbarButton)) SettingsContextMenu();
            }
            var rect = GUILayoutUtility.GetLastRect();
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.DragUpdated
                    || Event.current.type == EventType.MouseDrag || Event.current.type == EventType.DragPerform)
                    Event.current.Use();
            }
        }

        private void OnDelete()
        {
            RegisterUndo("Delete Brush");
            DeleteBrushSelection();
            PaletteManager.ClearSelection();
            OnPaletteChange();
        }

        public void Reload(bool clearSelection)
        {
            if (clearSelection) PaletteManager.ClearSelection(true);
            _updateTabSize = true;
            OnPaletteChange();
        }

        private void SettingsContextMenu()
        {
            var menu = new UnityEditor.GenericMenu();
            menu.AddItem(new GUIContent(PaletteManager.viewList ? "Grid View" : "List View"), false,
                () => PaletteManager.viewList = !PaletteManager.viewList);
            if (!PaletteManager.viewList)
                menu.AddItem(new GUIContent("Show Brush Name"), PaletteManager.showBrushName,
                () => PaletteManager.showBrushName = !PaletteManager.showBrushName);
            if (PaletteManager.selectedPalette.brushCount > 1)
            {
                menu.AddItem(new GUIContent("Ascending Sort"), false,
                    () => { PaletteManager.selectedPalette.AscendingSort(); });
                menu.AddItem(new GUIContent("Descending Sort"), false,
                    () => { PaletteManager.selectedPalette.DescendingSort(); });
            }
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Rename palette..."), false, ShowRenamePaletteWindow,
                       new RenameData(PaletteManager.selectedPalette, PaletteManager.selectedPalette.name,
                       position.position + Event.current.mousePosition));
            menu.AddItem(new GUIContent("Delete palette"), false, ShowDeleteConfirmation,
                PaletteManager.selectedPalette);
            menu.AddItem(new GUIContent("Cleanup palette"), false, () =>
            {
                PaletteManager.Cleanup();
                OnPaletteChange();
                UpdateTabBar();
                Repaint();
            });
            menu.AddItem(new GUIContent("Load palette files"), false, () =>
            {
                PaletteManager.instance.LoadPaletteFiles(true);
                Reload(!ThumbnailUtils.savingImage);
            });
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Update all thumbnails"), false, UpdateAllThumbnails);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Brush creation settings..."), false,
                BrushCreationSettingsWindow.ShowWindow);
            menu.ShowAsContext();
        }
        #endregion

        #region BRUSHES
        private Vector3 _selectedBrushPosition = Vector3.zero;
        private bool _frameSelectedBrush = false;
        private bool _newSelectedPositionSet = false;
        private Texture2D _loadingIcon = null;
        public void FrameSelectedBrush()
        {
            _frameSelectedBrush = true;
            _newSelectedPositionSet = false;
        }

        private void DoFrameSelectedBrush()
        {
            _frameSelectedBrush = false;
            if (_scrollPosition.y > _selectedBrushPosition.y
                || _scrollPosition.y + _scrollViewRect.height < _selectedBrushPosition.y)
                _scrollPosition.y = _selectedBrushPosition.y - 4;
            RepaintWindow();
        }

        private static bool _edittingThumbnail = false;
        private static int _edittingThumbnailIdx = -1;
        private void Brushes(ref BrushInputData toggleData)
        {
            if (Event.current.control && Event.current.keyCode == KeyCode.A && _filteredBrushList.Count > 0)
            {
                PaletteManager.ClearSelection();
                foreach (var brush in _filteredBrushList) PaletteManager.AddToSelection(brush.index);
                PaletteManager.selectedBrushIdx = _filteredBrushList[0].index;
                Repaint();
            }
            if (PaletteManager.selectedPalette.brushCount == 0) return;
            if (filteredBrushListCount == 0) return;

            var filteredBrushes = filteredBrushList.ToArray();
            int filterBrushIdx = 0;

            var nameStyle = GUIStyle.none;
            nameStyle.margin = new RectOffset(2, 2, 0, 1);
            nameStyle.clipping = TextClipping.Clip;
            nameStyle.fontSize = 8;
            nameStyle.normal.textColor = Color.white;

            MultibrushSettings brushSettings = null;
            int brushIdx = -1;
            Texture2D icon = null;

            void GetBrushSettings(ref GUIStyle style)
            {
                brushSettings = filteredBrushes[filterBrushIdx].brush;
                brushIdx = filteredBrushes[filterBrushIdx].index;
                if (PaletteManager.SelectionContains(brushIdx))
                    style.normal = _toggleStyle.onNormal;
                icon = brushSettings.thumbnail;
                if (icon == null) icon = _loadingIcon;
            }

            void GetInputData(ref BrushInputData inputData)
            {
                var rect = GUILayoutUtility.GetLastRect();
                void GetPaletteInputData(ref BrushInputData data)
                {
                    data = new BrushInputData(brushIdx, rect, Event.current.type,
                    Event.current.control, Event.current.shift, Event.current.mousePosition.x);
                }
                if (rect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDrag && Event.current.button == 1
                        && Event.current.delta != Vector2.zero)
                    {
                        if (!_edittingThumbnail) _edittingThumbnailIdx = brushIdx;
                        if (!Event.current.control && !Event.current.shift)
                        {
                            var brush = PaletteManager.selectedPalette.GetBrush(_edittingThumbnailIdx);
                            if (brush.thumbnailSettings.useCustomImage || PWBCore.staticData.useAssetPreview)
                            {
                                GetPaletteInputData(ref inputData);
                                return;
                            }
                            var rot = Quaternion.Euler(brush.thumbnailSettings.targetEuler);
                            brush.thumbnailSettings.targetEuler = (Quaternion.AngleAxis(Event.current.delta.y, Vector3.left)
                                 * Quaternion.AngleAxis(Event.current.delta.x, Vector3.down) * rot).eulerAngles;
                            brush.UpdateThumbnail(true, false);
                            Event.current.Use();
                            _edittingThumbnail = true;
                        }
                        else if (Event.current.control && !Event.current.shift)
                        {
                            var brush = PaletteManager.selectedPalette.GetBrush(_edittingThumbnailIdx);
                            if (brush.thumbnailSettings.useCustomImage || PWBCore.staticData.useAssetPreview)
                            {
                                GetPaletteInputData(ref inputData);
                                return;
                            }
                            var delta = Event.current.delta / PaletteManager.iconSize;
                            delta.y = -delta.y;
                            brush.thumbnailSettings.targetOffset = Vector2.Min(Vector2.one,
                                Vector2.Max(brush.thumbnailSettings.targetOffset + delta, -Vector2.one));
                            brush.UpdateThumbnail(true, false);
                            Event.current.Use();
                            _edittingThumbnail = true;
                        }
                    }
                    else if (Event.current.type == EventType.ContextClick && _edittingThumbnail)
                    {
                        var brush = PaletteManager.selectedPalette.GetBrush(brushIdx);
                        if (brush.thumbnailSettings.useCustomImage || PWBCore.staticData.useAssetPreview)
                        {
                            GetPaletteInputData(ref inputData);
                            return;
                        }
                        brush.UpdateThumbnail(true, true);
                        Event.current.Use();
                        _edittingThumbnail = false;
                    }
                    else if (Event.current.isScrollWheel && Event.current.control && !Event.current.shift)
                    {
                        var brush = PaletteManager.selectedPalette.GetBrush(brushIdx);
                        if (brush.thumbnailSettings.useCustomImage || PWBCore.staticData.useAssetPreview)
                        {
                            GetPaletteInputData(ref inputData);
                            return;
                        }
                        var scrollSign = Mathf.Sign(Event.current.delta.y);
                        brush.thumbnailSettings.zoom += scrollSign * 0.1f;
                        brush.UpdateThumbnail(true, false);
                        Event.current.Use();
                    }
                    else GetPaletteInputData(ref inputData);
                }
                if (Event.current.type != EventType.Layout && PaletteManager.selectedBrushIdx == brushIdx)
                {
                    _selectedBrushPosition = rect.position;
                    _newSelectedPositionSet = true;
                }
            }

            void GridViewRow(ref BrushInputData inputData)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int col = 0; col < _columnCount && filterBrushIdx < filteredBrushes.Length; ++col)
                    {
                        var style = new GUIStyle(_toggleStyle);
                        GetBrushSettings(ref style);
                        using (new GUILayout.VerticalScope(style))
                        {
                            if (PaletteManager.showBrushName)
                                GUILayout.Box(new GUIContent(brushSettings.name, brushSettings.name),
                                    nameStyle, GUILayout.Width(PaletteManager.iconSize));
                            GUILayout.Box(new GUIContent(icon, brushSettings.name), GUIStyle.none,
                                GUILayout.Width(PaletteManager.iconSize),
                            GUILayout.Height(PaletteManager.iconSize));
                        }
                        GetInputData(ref inputData);
                        ++filterBrushIdx;
                    }
                    GUILayout.FlexibleSpace();
                }
            }

            void ListView(ref BrushInputData inputData)
            {
                var style = new GUIStyle(_toggleStyle);
                style.padding = new RectOffset(0, 0, 0, 0);
                GetBrushSettings(ref style);
                using (new GUILayout.HorizontalScope(style))
                {
                    GUILayout.Box(new GUIContent(icon, brushSettings.name), GUIStyle.none,
                        GUILayout.Width(PaletteManager.iconSize),
                        GUILayout.Height(PaletteManager.iconSize));
                    GUILayout.Space(4);
                    using (new GUILayout.VerticalScope())
                    {
                        var span = (PaletteManager.iconSize - 16) / 2;
                        GUILayout.Space(span);
                        GUILayout.Box(new GUIContent(brushSettings.name, brushSettings.name), nameStyle);
                        GUILayout.Space(span);
                    }
                }
                GetInputData(ref inputData);
                ++filterBrushIdx;
            }
            nameStyle.fontSize = PaletteManager.viewList ? 12 : 8;
            nameStyle.fontSize = Mathf.Max(Mathf.RoundToInt(nameStyle.fontSize
                * ((float)PaletteManager.iconSize / (float)PrefabPalette.DEFAULT_ICON_SIZE)), nameStyle.fontSize);

            while (filterBrushIdx < filteredBrushes.Length)
            {
                if (PaletteManager.viewList) ListView(ref toggleData);
                else GridViewRow(ref toggleData);
            }
        }
        public void DeselectAllButThis(int index)
        {
            if (PaletteManager.selectedBrushIdx == index && PaletteManager.selectionCount == 1) return;
            PaletteManager.ClearSelection();
            if (index < 0) return;
            PaletteManager.AddToSelection(index);
            PaletteManager.selectedBrushIdx = index;
        }

        private void DeleteBrushSelection()
        {
            var descendingSelection = PaletteManager.idxSelection;
            System.Array.Sort<int>(descendingSelection, new System.Comparison<int>((i1, i2) => i2.CompareTo(i1)));
            foreach (var i in descendingSelection) PaletteManager.selectedPalette.RemoveBrushAt(i);
        }
        private void DeleteBrush(object idx)
        {
            RegisterUndo("Delete Brush");
            if (PaletteManager.SelectionContains((int)idx)) DeleteBrushSelection();
            else PaletteManager.selectedPalette.RemoveBrushAt((int)idx);
            PaletteManager.ClearSelection();
            OnPaletteChange();
        }

        private void CopyBrushSettings(object idx)
            => PaletteManager.clipboardSetting = PaletteManager.selectedPalette.brushes[(int)idx].CloneMainSettings();

        private void PasteBrushSettings(object idx)
        {
            RegisterUndo("Paste Brush Settings");
            PaletteManager.selectedPalette.brushes[(int)idx].Copy(PaletteManager.clipboardSetting);
            if (BrushProperties.instance != null) BrushProperties.instance.Repaint();
            PaletteManager.selectedPalette.Save();
        }

        private void DuplicateBrush(object idx)
        {
            RegisterUndo("Duplicate Brush");
            if (PaletteManager.SelectionContains((int)idx))
            {
                var descendingSelection = PaletteManager.idxSelection;
                System.Array.Sort<int>(descendingSelection, new System.Comparison<int>((i1, i2) => i2.CompareTo(i1)));
                for (int i = 0; i < descendingSelection.Length; ++i)
                {
                    PaletteManager.selectedPalette.DuplicateBrush(descendingSelection[i], out MultibrushSettings duplicate);
                    descendingSelection[i] += descendingSelection.Length - 1 - i;
                }
                PaletteManager.idxSelection = descendingSelection;
            }
            else PaletteManager.selectedPalette.DuplicateBrush((int)idx, out MultibrushSettings duplicate);
            OnPaletteChange();
        }

        private void MergeBrushes()
        {
            RegisterUndo("Merge Brushes");
            var selection = new System.Collections.Generic.List<int>(PaletteManager.idxSelection);
            selection.Sort();
            var resultIdx = selection[0];
            var lastIdx = selection.Last() + 1;
            PaletteManager.selectedPalette.DuplicateBrushAt(resultIdx, lastIdx, out MultibrushSettings duplicate);
            if (duplicate == null)
            {
                PaletteManager.selectedPalette.Cleanup();
                return;
            }
            resultIdx = lastIdx;
            var firstItem = duplicate.GetItemAt(0);
            if (!firstItem.overwriteSettings) firstItem.Copy(duplicate);
            firstItem.overwriteSettings = true;
            duplicate.name += "_merged";

            selection.RemoveAt(0);
            bool cleanupPalette = false;
            for (int i = 0; i < selection.Count; ++i)
            {
                var idx = selection[i];
                var other = PaletteManager.selectedPalette.GetBrush(idx);
                if (other == null)
                {
                    cleanupPalette = true;
                    continue;
                }
                var otherItems = other.items;
                foreach (var item in otherItems)
                {
                    if (item == null)
                    {
                        cleanupPalette = true;
                        continue;
                    }
                    var clone = new MultibrushItemSettings(item.prefab, duplicate);
                    if (item.overwriteSettings) clone.Copy(item);
                    else clone.Copy(other);
                    clone.overwriteSettings = true;
                    duplicate.AddItem(clone);
                }
            }
            if (cleanupPalette) PaletteManager.selectedPalette.Cleanup();
            duplicate.Reset();
            PaletteManager.ClearSelection();
            PaletteManager.AddToSelection(resultIdx);
            PaletteManager.selectedBrushIdx = resultIdx;
            OnPaletteChange();
        }

        private void OnMergeBrushesContext()
        {
            RegisterUndo("Merge Brushes");
            var selection = new System.Collections.Generic.List<int>(PaletteManager.idxSelection);
            selection.Sort();
            var resultIdx = selection[0];
            selection.RemoveAt(0);
            selection.Reverse();
            var result = PaletteManager.selectedPalette.GetBrush(resultIdx);
            if (result == null)
            {
                PaletteManager.selectedPalette.Cleanup();
                return;
            }
            bool cleanupPalette = false;
            for (int i = 0; i < selection.Count; ++i)
            {
                var idx = selection[i];
                var other = PaletteManager.selectedPalette.GetBrush(idx);
                if (other == null)
                {
                    cleanupPalette = true;
                    continue;
                }
                var otherItems = other.items;
                foreach (var item in otherItems)
                {
                    if (item == null)
                    {
                        cleanupPalette = true;
                        continue;
                    }
                    var clone = item.Clone() as MultibrushItemSettings;
                    clone.parentSettings = result;
                    result.AddItem(clone);
                }
                PaletteManager.selectedPalette.RemoveBrushAt(idx);
            }
            if (cleanupPalette) PaletteManager.selectedPalette.Cleanup();
            PaletteManager.ClearSelection();
            PaletteManager.AddToSelection(resultIdx);
            PaletteManager.selectedBrushIdx = resultIdx;
            OnPaletteChange();
        }


        private void SelectPrefabs(object idx)
        {
            var prefabs = new System.Collections.Generic.List<GameObject>();
            if (PaletteManager.SelectionContains((int)idx))
            {
                foreach (int selectedIdx in PaletteManager.idxSelection)
                {
                    var brush = PaletteManager.selectedPalette.GetBrush(selectedIdx);
                    foreach (var item in brush.items)
                    {
                        if (item.prefab != null) prefabs.Add(item.prefab);
                    }
                }
            }
            else
            {
                var brush = PaletteManager.selectedPalette.GetBrush((int)idx);
                foreach (var item in brush.items)
                {
                    if (item.prefab != null) prefabs.Add(item.prefab);
                }
            }
            UnityEditor.Selection.objects = prefabs.ToArray();
        }

        private void OpenPrefab(object idx)
            => UnityEditor.AssetDatabase.OpenAsset(PaletteManager.selectedPalette.GetBrush((int)idx).items[0].prefab);

        private void SelectReferences(object idx)
        {
            var items = PaletteManager.selectedPalette.GetBrush((int)idx).items;
            var itemsprefabIds = new System.Collections.Generic.List<int>();
            foreach (var item in items)
            {
                if (item.prefab != null) itemsprefabIds.Add(item.prefab.GetInstanceID());
            }
            var selection = new System.Collections.Generic.List<GameObject>();
#if UNITY_2022_2_OR_NEWER
            var objects = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None);
#else
            var objects = GameObject.FindObjectsOfType<Transform>();
#endif
            foreach (var obj in objects)
            {
                var source = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (source == null) continue;
                var sourceIdx = source.gameObject.GetInstanceID();
                if (itemsprefabIds.Contains(sourceIdx)) selection.Add(obj.gameObject);
            }
            UnityEditor.Selection.objects = selection.ToArray();
        }

        private void UpdateThumbnail(object idx) => PaletteManager.UpdateSelectedThumbnails();

        private void EditThumbnail(object idx)
        {
            var brushIdx = (int)idx;
            var brush = PaletteManager.selectedPalette.GetBrush(brushIdx);
            ThumbnailEditorWindow.ShowWindow(brush, brushIdx);
        }

        private void CopyThumbnailSettings(object idx)
        {
            var brush = PaletteManager.selectedPalette.brushes[(int)idx];
            PaletteManager.clipboardThumbnailSettings = brush.thumbnailSettings.Clone();
            PaletteManager.clipboardOverwriteThumbnailSettings = PaletteManager.Trit.SAME;
        }
        private void PasteThumbnailSettings(object idx)
        {
            if (PaletteManager.clipboardThumbnailSettings == null) return;
            RegisterUndo("Paste Thumbnail Settings");
            void Paste(MultibrushSettings brush)
            {
                brush.thumbnailSettings.Copy(PaletteManager.clipboardThumbnailSettings);
                ThumbnailUtils.UpdateThumbnail(brushSettings: brush, updateItemThumbnails: true, savePng: true);
            }
            if (PaletteManager.SelectionContains((int)idx))
            {
                foreach (var i in PaletteManager.idxSelection) Paste(PaletteManager.selectedPalette.brushes[i]);
            }
            else Paste(PaletteManager.selectedPalette.brushes[(int)idx]);
            PaletteManager.selectedPalette.Save();
        }

        private void BrushContext(int idx)
        {
            void ShowBrushProperties(object idx)
            {
                PaletteManager.ClearSelection();
                PaletteManager.AddToSelection((int)idx);
                PaletteManager.selectedBrushIdx = (int)idx;
                BrushProperties.ShowWindow();
            }
            var menu = new UnityEditor.GenericMenu();
            menu.AddItem(new GUIContent("Brush Properties..."), false, ShowBrushProperties, idx);
            menu.AddSeparator(string.Empty);
            var brush = PaletteManager.selectedPalette.GetBrush(idx);
            menu.AddItem(new GUIContent("Select Prefab" + (PaletteManager.selectionCount > 1
                || brush.itemCount > 1 ? "s" : "")), false, SelectPrefabs, idx);
            if (brush.itemCount == 1) menu.AddItem(new GUIContent("Open Prefab"), false, OpenPrefab, idx);
            menu.AddItem(new GUIContent("Select References In Scene"), false, SelectReferences, idx);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Update Thumbnail"), false, UpdateThumbnail, idx);
            if (!PWBCore.staticData.useAssetPreview)
            {
                menu.AddItem(new GUIContent("Edit Thumbnail..."), false, EditThumbnail, idx);
                menu.AddItem(new GUIContent("Copy Thumbnail Settings"), false, CopyThumbnailSettings, idx);
                if (PaletteManager.clipboardThumbnailSettings != null)
                    menu.AddItem(new GUIContent("Paste Thumbnail Settings"), false, PasteThumbnailSettings, idx);
            }
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Delete"), false, DeleteBrush, idx);
            menu.AddItem(new GUIContent("Duplicate"), false, DuplicateBrush, idx);
            if (PaletteManager.selectionCount > 1) menu.AddItem(new GUIContent("Merge"), false, OnMergeBrushesContext);
            if (PaletteManager.selectionCount == 1)
                menu.AddItem(new GUIContent("Copy Brush Settings"), false, CopyBrushSettings, idx);
            if (PaletteManager.clipboardSetting != null)
                menu.AddItem(new GUIContent("Paste Brush Settings"), false, PasteBrushSettings, idx);
            menu.AddSeparator(string.Empty);
            PaletteContextAddMenuItems(menu);
            menu.ShowAsContext();
        }

        private void BrushMouseEventHandler(BrushInputData data)
        {
            void DeselectAllButCurrent()
            {
                PaletteManager.ClearSelection();
                PaletteManager.selectedBrushIdx = data.index;
                PaletteManager.AddToSelection(data.index);
            }
            if (data == null) return;
            if (data.eventType == EventType.MouseMove) Event.current.Use();
            if (data.eventType == EventType.MouseDown && Event.current.button == 0)
            {
                void DeselectAll() => PaletteManager.ClearSelection();
                void ToggleCurrent()
                {
                    if (PaletteManager.SelectionContains(data.index)) PaletteManager.RemoveFromSelection(data.index);
                    else PaletteManager.AddToSelection(data.index);
                    PaletteManager.selectedBrushIdx = PaletteManager.selectionCount == 1
                        ? PaletteManager.idxSelection[0] : -1;
                }
                if (data.shift)
                {
                    var selectedIdx = PaletteManager.selectedBrushIdx;
                    var sign = (int)Mathf.Sign(data.index - selectedIdx);
                    if (sign != 0)
                    {
                        PaletteManager.ClearSelection();
                        for (int i = selectedIdx; i != data.index; i += sign)
                        {
                            if (FilteredListContains(i)) PaletteManager.AddToSelection(i);
                        }
                        PaletteManager.AddToSelection(data.index);
                        PaletteManager.selectedBrushIdx = selectedIdx;
                    }
                    else DeselectAllButCurrent();
                }
                else
                {
                    if (data.control && PaletteManager.selectionCount < 2)
                    {
                        if (PaletteManager.selectedBrushIdx == data.index) DeselectAll();
                        else ToggleCurrent();
                    }
                    else if (data.control && PaletteManager.selectionCount > 1) ToggleCurrent();
                    else if (!data.control && PaletteManager.selectionCount < 2)
                    {
                        if (PaletteManager.selectedBrushIdx == data.index) DeselectAll();
                        else DeselectAllButCurrent();
                    }
                    else if (!data.control && PaletteManager.selectionCount > 1) DeselectAllButCurrent();
                }
                Event.current.Use();
                Repaint();
            }
            else if (data.eventType == EventType.ContextClick)
            {
                BrushContext(data.index);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseDrag && Event.current.button == 0
               && Event.current.delta != Vector2.zero)
            {
                if (!PaletteManager.SelectionContains(data.index)) DeselectAllButCurrent();
                UnityEditor.DragAndDrop.PrepareStartDrag();
                if (Event.current.control)
                {
                    UnityEditor.DragAndDrop.StartDrag("Dragging brush");
                    UnityEditor.DragAndDrop.objectReferences = new Object[]
                        { PaletteManager.selectedBrush.GetItemAt(0).prefab };
                    UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Move;
                }
                else
                {
                    PWBIO.sceneDragReceiver.brushId = data.index;
                    SceneDragAndDrop.StartDrag(PWBIO.sceneDragReceiver, "Dragging brush");
                    UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
                }
                draggingBrush = true;
                _moveBrush.from = data.index;
                _moveBrush.perform = false;
                _moveBrush.to = -1;
            }
            else if (data.eventType == EventType.DragUpdated && Event.current.button == 0)
            {
                if (Event.current.control) UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Move;
                else
                {
                    UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
                    var size = new Vector2(4, PaletteManager.iconSize);
                    var min = data.rect.min;
                    bool toTheRight = data.mouseX - data.rect.center.x > 0;
                    min.x = toTheRight ? data.rect.max.x : min.x - size.x;
                    _cursorRect = new Rect(min, size);
                    _showCursor = true;
                    _moveBrush.to = data.index;
                    if (toTheRight) ++_moveBrush.to;
                }
            }
            else if (data.eventType == EventType.DragPerform && Event.current.button == 0 && !Event.current.control)
            {
                _moveBrush.to = data.index;
                bool toTheRight = data.mouseX - data.rect.center.x > 0;
                if (toTheRight) ++_moveBrush.to;
                if (draggingBrush)
                {
                    _moveBrush.perform = _moveBrush.from != _moveBrush.to;
                    draggingBrush = false;
                }
                _showCursor = false;
            }
            else if (data.eventType == EventType.DragExited && Event.current.button == 0 && !Event.current.control)
            {
                _showCursor = false;
                draggingBrush = false;
                _moveBrush.to = -1;
            }
        }
        #endregion

        #region PALETTE CONTEXT
        private int _currentPickerId = -1;
        private bool _contextBrushAdded = false;
        private MultibrushSettings _newContextBrush = null;

        private void PaletteContext()
        {
            if (_scrollViewRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.ContextClick)
                {
                    PaletteContextMenu();
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    PaletteManager.ClearSelection();
                    Repaint();
                }
            }

            if (Event.current.commandName == "ObjectSelectorClosed"
                && UnityEditor.EditorGUIUtility.GetObjectPickerControlID() == _currentPickerId)
            {
                var obj = UnityEditor.EditorGUIUtility.GetObjectPickerObject();
                if (obj != null)
                {
                    var prefabType = UnityEditor.PrefabUtility.GetPrefabAssetType(obj);
                    if (prefabType == UnityEditor.PrefabAssetType.Regular
                        || prefabType == UnityEditor.PrefabAssetType.Variant)
                    {
                        _contextBrushAdded = true;
                        var gameObj = obj as GameObject;
                        AddLabels(gameObj);
                        _newContextBrush = new MultibrushSettings(gameObj, PaletteManager.selectedPalette);
                    }
                }
                _currentPickerId = -1;
            }
        }

        private void PaletteContextAddMenuItems(UnityEditor.GenericMenu menu)
        {
            menu.AddItem(new GUIContent("New Brush From Prefab..."), false, CreateBrushFromPrefab);
            menu.AddItem(new GUIContent("New MultiBrush From Folder..."), false, CreateBrushFromFolder);
            menu.AddItem(new GUIContent("New Brush From Each Prefab In Folder..."), false,
                CreateBrushFromEachPrefabInFolder);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("New MultiBrush From Selection"), false, CreateBrushFromSelection);
            menu.AddItem(new GUIContent("New Brush From Each Prefab Selected"), false,
                CreateBushFromEachPrefabSelected);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Update all thumbnails"), false, UpdateAllThumbnails);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Brush Creation And Drop Settings..."), false,
                BrushCreationSettingsWindow.ShowWindow);
            if (PaletteManager.selectedBrushIdx > 0 || PaletteManager.movingBrushes)
            {
                menu.AddSeparator(string.Empty);
                if (PaletteManager.selectedBrushIdx > 0)
                    menu.AddItem(new GUIContent("Copy Selected brushes"), false, PaletteManager.SelectBrushesToMove);
                if (PaletteManager.movingBrushes)
                {
                    menu.AddItem(new GUIContent("Paste brushes and keep originals"),
                        false, PasteBrushesToSelectedPalette);
                    menu.AddItem(new GUIContent("Paste brushes and delete originals"),
                        false, MoveBrushesToSelectedPalette);
                }
            }
        }

        private void PasteBrushesToSelectedPalette()
        {
            PaletteManager.PasteBrushesToSelectedPalette();
            OnPaletteChange();
        }
        private void MoveBrushesToSelectedPalette()
        {
            PaletteManager.MoveBrushesToSelectedPalette();
            OnPaletteChange();
        }
        private void PaletteContextMenu()
        {
            var menu = new UnityEditor.GenericMenu();
            PaletteContextAddMenuItems(menu);
            menu.ShowAsContext();
        }

        private void CreateBrushFromPrefab()
        {
            _currentPickerId = GUIUtility.GetControlID(FocusType.Passive) + 100;
            UnityEditor.EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "t:Prefab", _currentPickerId);
        }

        private void CreateBrushFromFolder()
        {
            var items = PluginMaster.DropUtils.GetFolderItems();
            if (items == null) return;
            RegisterUndo("Add Brush");
            var brush = new MultibrushSettings(items[0].obj, PaletteManager.selectedPalette);
            AddLabels(items[0].obj);
            PaletteManager.selectedPalette.AddBrush(brush);
            DeselectAllButThis(PaletteManager.selectedPalette.brushes.Length - 1);
            for (int i = 1; i < items.Length; ++i)
            {
                var item = new MultibrushItemSettings(items[i].obj, brush);
                AddLabels(items[i].obj);
                brush.AddItem(item);
            }
            OnPaletteChange();
        }

        private void CreateBrushFromEachPrefabInFolder()
        {
            var items = PluginMaster.DropUtils.GetFolderItems();
            if (items == null) return;
            foreach (var item in items)
            {
                if (item.obj == null) continue;
                RegisterUndo("Add Brush");
                AddLabels(item.obj);
                var brush = new MultibrushSettings(item.obj, PaletteManager.selectedPalette);
                PaletteManager.selectedPalette.AddBrush(brush);
            }
            DeselectAllButThis(PaletteManager.selectedPalette.brushes.Length - 1);
            OnPaletteChange();
        }

        private string GetPrefabFolder(GameObject obj)
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
            var folders = path.Split(new char[] { '\\', '/' });
            var subFolder = folders[folders.Length - 2];
            return subFolder;
        }

        public void CreateBrushFromSelection()
        {
            if (PaletteManager.selectionCount > 1)
            {
                MergeBrushes();
                return;
            }

            var selectionPrefabs = SelectionManager.GetSelectionPrefabs();
            CreateBrushFromSelection(selectionPrefabs);
        }

        public void CreateBrushFromSelection(GameObject[] selectionPrefabs)
        {
            if (selectionPrefabs.Length == 0) return;

            RegisterUndo("Add Brush");
            AddLabels(selectionPrefabs[0]);
            var brush = new MultibrushSettings(selectionPrefabs[0], PaletteManager.selectedPalette);
            PaletteManager.selectedPalette.AddBrush(brush);
            DeselectAllButThis(PaletteManager.selectedPalette.brushes.Length - 1);
            for (int i = 1; i < selectionPrefabs.Length; ++i)
            {
                AddLabels(selectionPrefabs[i]);
                brush.AddItem(new MultibrushItemSettings(selectionPrefabs[i], brush));
            }
            OnPaletteChange();
        }

        public void CreateBrushFromSelection(GameObject selectedPrefab)
            => CreateBrushFromSelection(new GameObject[] { selectedPrefab });

        public void CreateBushFromEachPrefabSelected()
        {
            var selectionPrefabs = SelectionManager.GetSelectionPrefabs();
            if (selectionPrefabs.Length == 0) return;
            foreach (var obj in selectionPrefabs)
            {
                if (obj == null) continue;
                RegisterUndo("Add Brush");
                var brush = new MultibrushSettings(obj, PaletteManager.selectedPalette);
                AddLabels(obj);
                PaletteManager.selectedPalette.AddBrush(brush);
            }
            DeselectAllButThis(PaletteManager.selectedPalette.brushes.Length - 1);
            OnPaletteChange();
        }
        #endregion

        #region DROPBOX
        private void DropBox()
        {
            GUIStyle dragAndDropBoxStyle = new GUIStyle();
            dragAndDropBoxStyle.alignment = TextAnchor.MiddleCenter;
            dragAndDropBoxStyle.fontStyle = FontStyle.Italic;
            dragAndDropBoxStyle.fontSize = 12;
            dragAndDropBoxStyle.normal.textColor = Color.white;
            dragAndDropBoxStyle.wordWrap = true;
            GUI.Box(_scrollViewRect, "Drag and Drop Prefabs Or Folders Here", dragAndDropBoxStyle);
        }

        private void AddLabels(GameObject obj)
        {
            if (!PaletteManager.selectedPalette.brushCreationSettings.addLabelsToDroppedPrefabs) return;
            var labels = new System.Collections.Generic.HashSet<string>(UnityEditor.AssetDatabase.GetLabels(obj));
            int labelCount = labels.Count;
            if (PaletteManager.selectedPalette.brushCreationSettings.addLabelsToDroppedPrefabs)
                labels.UnionWith(PaletteManager.selectedPalette.brushCreationSettings.labels);
            if (labelCount != labels.Count) UnityEditor.AssetDatabase.SetLabels(obj, labels.ToArray());
        }

        private void DropPrefab()
        {
            if (_scrollViewRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    bool paletteChanged = false;
                    var items = DropUtils.GetDroppedPrefabs();
                    if (items.Length > 0) PaletteManager.ClearSelection();
                    var i = 0;
                    foreach (var item in items)
                    {
                        AddLabels(item.obj);
                        var brush = new MultibrushSettings(item.obj, PaletteManager.selectedPalette);
                        RegisterUndo("Add Brush");
                        if (_moveBrush.to < 0)
                        {
                            PaletteManager.selectedPalette.AddBrush(brush);
                            PaletteManager.selectedBrushIdx = PaletteManager.selectedPalette.brushes.Length - 1;
                        }
                        else
                        {
                            var idx = _moveBrush.to + i++;
                            PaletteManager.selectedPalette.InsertBrushAt(brush, idx);
                            PaletteManager.selectedBrushIdx = _moveBrush.to;
                        }
                        paletteChanged = true;
                    }
                    if (paletteChanged) OnPaletteChange();
                    if (draggingBrush && _moveBrush.to >= 0)
                    {
                        _moveBrush.perform = _moveBrush.from != _moveBrush.to;
                        draggingBrush = false;
                    }
                    _showCursor = false;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.DragExited) _showCursor = false;
            }
        }
        #endregion

        #region TAB BAR
        private GUIContent _pinTabIcon = null;
        private GUIContent _pinnedTabIcon = null;
        private GUIContent _pinTabLightIcon = null;
        private GUIContent _pinnedTabLightIcon = null;
        private GUIContent pinTabIcon
        {
            get
            {
                if (_pinTabIcon.image == null)
                    _pinTabIcon.image = Resources.Load<Texture2D>("Sprites/PinTab");
                if (_pinTabLightIcon.image == null)
                    _pinTabLightIcon.image = Resources.Load<Texture2D>("Sprites/LightTheme/PinTab");
                return UnityEditor.EditorGUIUtility.isProSkin ? _pinTabIcon : _pinTabLightIcon;
            }
        }
        private GUIContent pinnedTabIcon
        {
            get
            {
                if (_pinnedTabIcon.image == null)
                    _pinnedTabIcon.image = Resources.Load<Texture2D>("Sprites/PinnedTab");
                if (_pinnedTabLightIcon.image == null)
                    _pinnedTabLightIcon.image = Resources.Load<Texture2D>("Sprites/LightTheme/PinnedTab");
                return UnityEditor.EditorGUIUtility.isProSkin ? _pinnedTabIcon : _pinnedTabLightIcon;
            }
        }
        #region RENAME
        private class RenamePaletteWindow : UnityEditor.EditorWindow
        {
            private string _name = string.Empty;
            private System.Action<RenameData> _onDone;
            private bool _focusSet = false;
            private int _delayFrames = 0;
            RenameData data;

            public static void ShowWindow(RenameData data, System.Action<RenameData> onDone)
            {
                var window = GetWindow<RenamePaletteWindow>(true, "Rename Palette");
                window.data = data;
                window._name = data.newName;
                window._onDone = onDone;
                window.position = new Rect(data.mousePosition.x + 50, data.mousePosition.y + 50, 0, 0);
                window.minSize = window.maxSize = new Vector2(160, 45);
                window._focusSet = false;
                window._delayFrames = 0;
            }

            private void OnGUI()
            {
                _delayFrames++;

                UnityEditor.EditorGUIUtility.labelWidth = 50;
                UnityEditor.EditorGUIUtility.fieldWidth = 70;
                GUI.SetNextControlName("NameField");
                _name = UnityEditor.EditorGUILayout.TextField(_name);

                if (!_focusSet && _delayFrames > 2 && Event.current.type == EventType.Repaint)
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        if (this != null)
                        {
                            UnityEditor.EditorGUI.FocusTextInControl("NameField");
                            Repaint();
                        }
                    };
                    _focusSet = true;
                }

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                {
                    if (!string.IsNullOrWhiteSpace(_name))
                    {
                        data.newName = _name;
                        _onDone(data);
                        Close();
                    }
                    Event.current.Use();
                }

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                {
                    Close();
                    Event.current.Use();
                }

                using (new UnityEditor.EditorGUI.DisabledGroupScope(string.IsNullOrWhiteSpace(_name)))
                {
                    if (GUILayout.Button("Apply"))
                    {
                        data.newName = _name;
                        _onDone(data);
                        Close();
                    }
                }
            }
        }

        private struct RenameData
        {
            public readonly PaletteData palette;
            public readonly Vector2 mousePosition;
            public string newName;

            public RenameData(PaletteData palette, string newName, Vector2 mousePosition)
                => (this.palette, this.newName, this.mousePosition) = (palette, newName, mousePosition);
        }
        private void ShowRenamePaletteWindow(object obj)
        {
            if (!(obj is RenameData)) return;
            var data = (RenameData)obj;
            RenamePaletteWindow.ShowWindow(data, RenamePalette);
        }
        private void RenamePalette(RenameData data)
        {
            RegisterUndo("Rename Palette");
            data.palette.name = data.newName;
            Repaint();
        }
        #endregion

        private void ShowDeleteConfirmation(object obj)
        {
            var palette = (PaletteData)obj;
            if (UnityEditor.EditorUtility.DisplayDialog("Delete Palette: " + palette.name,
                "Are you sure you want to delete this palette?\n" + palette.name, "Delete", "Cancel"))
            {
                RegisterUndo("Remove Palette");
                var isSelected = PaletteManager.IsPaletteSelected(palette);
                PaletteManager.RemovePalette(palette);
                if (PaletteManager.allPalettesCount == 0) CreatePalette();
                else if (isSelected) PaletteManager.SelectPreviousPalette();
                PaletteManager.selectedBrushIdx = -1;

                _updateTabSize = true;
                UpdateFilteredList(false);
                Repaint();
            }
        }

        #region TAB BUTTONS
        private bool _updateTabSize = false;

        public static void UpdateTabBar()
        {
            if (instance == null) return;
            instance._updateTabSize = true;

        }
        public void SelectPalette(PaletteData palette)
        {
            if (palette == null) return;
            PaletteManager.selectedPalette = palette;
            PaletteManager.selectedBrushIdx = -1;
            PaletteManager.ClearSelection();
            _updateTabSize = true;
            OnPaletteChange();
        }

        private void SelectPalette(object obj)
        {
            SelectPalette((PaletteData)obj);
            if (PaletteManager.showTabsInMultipleRows) return;
            PaletteManager.ShowSelectedFirst();
        }

        private void CreatePalette()
        {
            var palette = new PaletteData("Palette" + (PaletteManager.nonPinnedCount + 1),
                System.DateTime.Now.ToBinary());
            PaletteManager.AddPalette(palette, save: true);
            SelectPalette(palette);
            UpdateTabBar();
        }
        private void DuplicatePalette(object obj)
        {
            var palette = (PaletteData)obj;
            PaletteManager.DuplicatePalette(palette);
            UpdateTabBar();
            RepaintWindow();
        }

        private void ToggleMultipleRows()
            => PaletteManager.showTabsInMultipleRows = !PaletteManager.showTabsInMultipleRows;
        private System.Collections.Generic.Dictionary<long, (PaletteData palette, Rect rect)> _tabRects
            = new System.Collections.Generic.Dictionary<long, (PaletteData, Rect)>();
        private System.Collections.Generic.Dictionary<long, float> _tabSize
            = new System.Collections.Generic.Dictionary<long, float>();
        private void TabBar()
        {
            HandleTabBarContextClick();
            if (Event.current.type == EventType.Repaint) _tabRects.Clear();
            DrawPinnedTabsIfAvailable();
            DrawNonPinnedTabsIfAvailable();
            RecalculateTabSizesIfNeeded();
        }

        private void HandleTabBarContextClick()
        {
            if (Event.current.type != EventType.MouseDown || Event.current.button != 1 || _updateTabSize) return;

            foreach (var tabRect in _tabRects.Values)
            {
                if (!tabRect.rect.Contains(Event.current.mousePosition)) continue;

                var palette = tabRect.palette;
                var name = palette.name;
                var menu = new UnityEditor.GenericMenu();
                menu.AddItem(new GUIContent("Rename"), false, ShowRenamePaletteWindow,
                    new RenameData(palette, name, position.position + Event.current.mousePosition));
                menu.AddItem(new GUIContent("Delete"), false, ShowDeleteConfirmation, palette);
                menu.AddItem(new GUIContent("Duplicate"), false, DuplicatePalette, palette);
                menu.ShowAsContext();
                Event.current.Use();
                break;
            }
        }

        private void DrawPinnedTabsIfAvailable()
        {
            if (_updateTabSize || PaletteManager.pinnedCount <= 0) return;

            if (!TryGetRowItemCounts(PaletteManager.pinnedPalettes, out var rowItemCount)) return;
            if (rowItemCount.Count == 0) return;

            int fromIdx = 0;
            int toIdx = 0;

            foreach (var itemCount in rowItemCount)
            {
                toIdx = fromIdx + itemCount - 1;
                using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar))
                {
                    if (fromIdx == 0) DrawDropDownButton();
                    DrawTabs(fromIdx, toIdx, pinned: true);
                }
                fromIdx = toIdx + 1;
                if (fromIdx >= PaletteManager.pinnedCount) break;
            }
        }

        private void DrawNonPinnedTabsIfAvailable()
        {
            if (_updateTabSize || PaletteManager.nonPinnedCount <= 0) return;
            if (!TryGetRowItemCounts(PaletteManager.nonPinnedPalettes, out var rowItemCount)) return;
            if (rowItemCount.Count == 0) return;

            int fromIdx = 0;
            int toIdx = 0;

            if (PaletteManager.showTabsInMultipleRows)
            {
                foreach (var itemCount in rowItemCount)
                {
                    toIdx = fromIdx + itemCount - 1;
                    using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar))
                    {
                        if (fromIdx == 0 && PaletteManager.pinnedCount == 0) DrawDropDownButton();
                        DrawTabs(fromIdx, toIdx, pinned: false);
                    }
                    fromIdx = toIdx + 1;
                    if (fromIdx >= PaletteManager.nonPinnedCount) break;
                }
            }
            else
            {
                using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar))
                {
                    if (PaletteManager.pinnedCount == 0) DrawDropDownButton();
                    DrawTabs(fromIdx, to: rowItemCount[0] - 1, pinned: false);
                }
            }

            if(PaletteManager.pinnedCount == 0)
            {
                if(!PaletteManager.nonPinnedPalettes.Exists(p => p.id == PaletteManager.selectedPalette.id))
                    SelectPalette(PaletteManager.nonPinnedPalettes[0]);
            }
            else if (!PaletteManager.nonPinnedPalettes.Exists(p => p.id == PaletteManager.selectedPalette.id))
            {
                if (!PaletteManager.pinnedPalettes.Exists(p => p.id == PaletteManager.selectedPalette.id))
                    SelectPalette(PaletteManager.pinnedPalettes[0]);
            }
        }

        private bool TryGetRowItemCounts(System.Collections.Generic.IList<PaletteData> palettes,
            out System.Collections.Generic.List<int> rowItemCount)
        {
            rowItemCount = new System.Collections.Generic.List<int>();
            float tabsWidth = 0f;
            int tabItemCount = 0;
            for (int i = 0; i < palettes.Count; ++i)
            {
                var id = palettes[i].id;
                if (!_tabSize.ContainsKey(id))
                {
                    _updateTabSize = true;
                    rowItemCount.Clear();
                    return false;
                }

                var w = _tabSize[id];
                tabsWidth += w;

                if (tabsWidth > position.width)
                {
                    rowItemCount.Add(Mathf.Max(tabItemCount, 1));
                    tabsWidth = tabItemCount > 0 ? w : 0;
                    if (tabItemCount == 0) continue;
                    tabItemCount = 0;
                }
                ++tabItemCount;
            }
            if (tabItemCount > 0) rowItemCount.Add(tabItemCount);
            return !_updateTabSize;
        }

        private void DrawTabs(int from, int to, bool pinned)
        {
            var palettes = pinned ? PaletteManager.pinnedPalettes : PaletteManager.nonPinnedPalettes;
            for (int i = from; i <= to; ++i)
            {
                var palette = palettes[i];
                var isSelected = PaletteManager.IsPaletteSelected(palette);
                var name = palette.name;
                if (GUILayout.Toggle(isSelected, name, UnityEditor.EditorStyles.toolbarButton)
                    && Event.current.button == 0 && !isSelected)
                {
                    SelectPalette(palette);
                }
                var toggleRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint)
                    _tabRects[palette.id] = (palette, toggleRect);
                if (GUILayout.Button(pinned ? pinnedTabIcon : pinTabIcon, UnityEditor.EditorStyles.toolbarButton))
                {
                    PaletteManager.TogglePinnedPalette(palette);
                    UpdateTabBar();
                    RepaintWindow();
                }
            }
            GUILayout.FlexibleSpace();
        }

        private void DrawDropDownButton()
        {
            if (!GUILayout.Button(_dropdownIcon, UnityEditor.EditorStyles.toolbarButton)) return;

            var menu = new UnityEditor.GenericMenu();
            menu.AddItem(new GUIContent("New palette"), false, CreatePalette);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Show tabs in multiple rows"),
                PaletteManager.showTabsInMultipleRows, ToggleMultipleRows);
            menu.AddSeparator(string.Empty);

            var allPalettes = PaletteManager.allPalettes;
            var sortedPalettes = allPalettes.OrderBy(p => p.name).ToList();
            var repeatedNameCount = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var palette in sortedPalettes)
            {
                var name = palette.name;
                var isRepeated = repeatedNameCount.ContainsKey(name);
                var displayName = isRepeated ? name + "(" + repeatedNameCount[name] + ")" : name;
                var isSelected = PaletteManager.IsPaletteSelected(palette);
                menu.AddItem(new GUIContent(displayName), isSelected, SelectPalette, palette);
                if (isRepeated) repeatedNameCount[name] += 1;
                else repeatedNameCount.Add(name, 1);
            }
            menu.ShowAsContext();
        }

        private void RecalculateTabSizesIfNeeded()
        {
            if (!_updateTabSize || Event.current.type != EventType.Repaint) return;

            var allPalettes = PaletteManager.allPalettes;
            _tabSize.Clear();
            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar))
            {
                for (int i = 0; i < allPalettes.Count; ++i)
                {
                    var palette = allPalettes[i];
                    var name = palette.name;
                    var content = UnityEditor.EditorGUIUtility.TrTempContent(name);
                    var size = UnityEditor.EditorStyles.toolbarButton.CalcSize(content).x + 18f; // 18 for pin icon
                    var id = palette.id;
                    if (_tabSize.ContainsKey(id)) _tabSize[id] = size;
                    else _tabSize.Add(id, size);
                }
            }
            _updateTabSize = false;
            Repaint();
        }
        #endregion
        #endregion

        #region SEARCH BAR
        private string _filterText = string.Empty;
        private GUIContent _labelIcon = null;
        private GUIContent _selectionFilterIcon = null;
        private GUIContent _folderFilterIcon = null;
        private GUIContent _clearFilterIcon = null;

        private struct FilteredBrush
        {
            public readonly MultibrushSettings brush;
            public readonly int index;
            public FilteredBrush(MultibrushSettings brush, int index) => (this.brush, this.index) = (brush, index);
        }
        private System.Collections.Generic.List<FilteredBrush> _filteredBrushList
            = new System.Collections.Generic.List<FilteredBrush>();
        private System.Collections.Generic.List<FilteredBrush> filteredBrushList
        {
            get
            {
                if (_filteredBrushList == null) _filteredBrushList = new System.Collections.Generic.List<FilteredBrush>();
                return _filteredBrushList;
            }
        }
        public bool FilteredBrushListContains(int index) => _filteredBrushList.Exists(brush => brush.index == index);
        private System.Collections.Generic.Dictionary<string, bool> _labelFilter
            = new System.Collections.Generic.Dictionary<string, bool>();
        public System.Collections.Generic.Dictionary<string, bool> labelFilter
        {
            get
            {
                if (_labelFilter == null) _labelFilter = new System.Collections.Generic.Dictionary<string, bool>();
                return _labelFilter;
            }
            set => _labelFilter = value;
        }

        private bool _updateLabelFilter = true;
        public int filteredBrushListCount => filteredBrushList.Count;

        public string filterText
        {
            get
            {
                if (_filterText == null) _filterText = string.Empty;
                return _filterText;
            }
            set => _filterText = value;
        }

        private System.Collections.Generic.Dictionary<long, string[]> _hiddenFolders
            = new System.Collections.Generic.Dictionary<long, string[]>();

        private string[] hiddenFolders
        {
            get
            {
                if (_hiddenFolders.Count == 0 || !_hiddenFolders.ContainsKey(PaletteManager.selectedPalette.id))
                    return new string[] { };
                return _hiddenFolders[PaletteManager.selectedPalette.id];
            }
        }
        public static string[] GetHiddenFolders()
        {
            if (instance == null) return new string[] { };
            return instance.hiddenFolders;
        }

        public static void SetHiddenFolders(string[] value)
        {
            if (instance == null) return;
            if (instance._hiddenFolders.ContainsKey(PaletteManager.selectedPalette.id))
                instance._hiddenFolders[PaletteManager.selectedPalette.id] = value;
            else instance._hiddenFolders.Add(PaletteManager.selectedPalette.id, value);
            instance.UpdateFilteredList(false);
            RepaintWindow();
        }
        private void ClearLabelFilter()
        {
            foreach (var key in labelFilter.Keys.ToArray()) labelFilter[key] = false;
        }

        private void SearchBar()
        {
            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();

                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
#if UNITY_2019_1_OR_NEWER
                    var searchFieldStyle = UnityEditor.EditorStyles.toolbarSearchField;
#else
                    var searchFieldStyle = EditorStyles.toolbarTextField;
#endif
                    GUILayout.Space(2);
                    filterText = UnityEditor.EditorGUILayout.TextField(filterText, searchFieldStyle).Trim();
                    if (check.changed) UpdateFilteredList(true);
                }
                if (filterText != string.Empty)
                {
                    if (GUILayout.Button(_clearFilterIcon, UnityEditor.EditorStyles.toolbarButton))
                    {
                        filterText = string.Empty;
                        ClearLabelFilter();
                        UpdateFilteredList(true);
                        GUI.FocusControl(null);
                    }
                }

                if (GUILayout.Button(_labelIcon, UnityEditor.EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    UpdateLabelFilter();
                    var menu = new UnityEditor.GenericMenu();
                    if (labelFilter.Count == 0)
                        menu.AddItem(new GUIContent("No labels Found"), false, null);
                    else
                        foreach (var labelItem in labelFilter.OrderBy(item => item.Key))
                            menu.AddItem(new GUIContent(labelItem.Key), labelItem.Value,
                                SelectLabelFilter, labelItem.Key);
                    menu.ShowAsContext();
                }

                if (GUILayout.Button(_selectionFilterIcon, UnityEditor.EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    FilterBySelection();
                }
                if (GUILayout.Button(_folderFilterIcon, UnityEditor.EditorStyles.toolbarButton))
                {
                    FilterByFolderWindow.ShowWindow();
                }
            }
            if (_updateLabelFilter)
            {
                _updateLabelFilter = false;
                UpdateLabelFilter();
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                GUI.FocusControl(null);
                Repaint();
            }
        }

        private bool FilteredListContains(int index)
        {
            foreach (var filtered in filteredBrushList)
            {
                if (filtered.index == index) return true;
            }
            return false;
        }

        private void UpdateFilteredList(bool textCanged)
        {
            filteredBrushList.Clear();
            var selectedPalette = PaletteManager.selectedPalette;
            if (selectedPalette == null) return;

            void RemoveFromSelection(int index)
            {
                PaletteManager.RemoveFromSelection(index);
                if (PaletteManager.selectedBrushIdx == index) PaletteManager.selectedBrushIdx = -1;
                if (PaletteManager.selectionCount == 1)
                    PaletteManager.selectedBrushIdx = PaletteManager.idxSelection[0];
            }

            //filter by label
            var filterTextArray = filterText.Split(',');
            var filterTextSet = new System.Collections.Generic.List<string>();
            ClearLabelFilter();
            bool filterByLabel = false;
            for (int i = 0; i < filterTextArray.Length; ++i)
            {
                var filterText = filterTextArray[i].Trim();
                if (filterText.Length >= 2 && filterText.Substring(0, 2) == "l:")
                {
                    filterText = filterText.Substring(2);
                    if (labelFilter.ContainsKey(filterText))
                    {
                        labelFilter[filterText] = true;
                        filterByLabel = true;
                    }
                    else return;
                    continue;
                }
                filterTextSet.Add(filterText);
            }

            var tempFilteredBrushList = new System.Collections.Generic.HashSet<FilteredBrush>();
            var brushes = PaletteManager.selectedPalette.brushes;
            if (!filterByLabel)
                for (int i = 0; i < brushes.Length; ++i)
                {
                    if (brushes[i].containMissingPrefabs) continue;
                    tempFilteredBrushList.Add(new FilteredBrush(brushes[i], i));
                }
            else
            {
                for (int i = 0; i < brushes.Length; ++i)
                {
                    var brush = brushes[i];
                    if (brush.containMissingPrefabs) continue;
                    bool itemContainsFilter = false;
                    foreach (var item in brush.items)
                    {
                        if (item.prefab == null) continue;
                        var labels = UnityEditor.AssetDatabase.GetLabels(item.prefab);
                        foreach (var label in labels)
                        {
                            if (labelFilter[label])
                            {
                                itemContainsFilter = true;
                                break;
                            }
                        }
                        if (itemContainsFilter) break;
                    }
                    if (itemContainsFilter) tempFilteredBrushList.Add(new FilteredBrush(brush, i));
                    else RemoveFromSelection(i);
                }
            }
            if (tempFilteredBrushList.Count == 0) return;
            //filter by name
            var listIsEmpty = filterTextSet.Count == 0;
            if (!listIsEmpty)
            {
                listIsEmpty = true;
                foreach (var filter in filterTextSet)
                {
                    if (filter != string.Empty)
                    {
                        listIsEmpty = false;
                        break;
                    }
                }
            }

            if (!listIsEmpty)
            {
                foreach (var filteredItem in tempFilteredBrushList.ToArray())
                {
                    for (int i = 0; i < filterTextSet.Count; ++i)
                    {
                        var filterText = filterTextSet[i].Trim();
                        bool wholeWordOnly = false;
                        if (filterText == string.Empty) continue;
                        if (filterText.Length >= 2 && filterText.Substring(0, 2) == "w:")
                        {
                            wholeWordOnly = true;
                            filterText = filterText.Substring(2);
                        }
                        if (filterText == string.Empty) continue;
                        filterText = filterText.ToLower();
                        var brush = filteredItem.brush;
                        if ((!wholeWordOnly && brush.name.ToLower().Contains(filterText))
                            || (wholeWordOnly && brush.name.ToLower() == filterText))
                            tempFilteredBrushList.Add(filteredItem);
                        else
                        {
                            if (tempFilteredBrushList.Contains(filteredItem)) tempFilteredBrushList.Remove(filteredItem);
                            RemoveFromSelection(filteredItem.index);
                        }
                    }
                }
            }
            if (tempFilteredBrushList.Count == 0) return;
            // Filter by folder
            foreach (var filteredItem in tempFilteredBrushList.ToArray())
            {
                var brushItems = filteredItem.brush.items;
                foreach (var brushItem in brushItems)
                {
                    if (filteredBrushList.Contains(filteredItem)) continue;
                    if (hiddenFolders.Any(filter => brushItem.prefabPath.StartsWith(filter)))
                        RemoveFromSelection(filteredItem.index);
                    else filteredBrushList.Add(filteredItem);
                }
            }
        }

        private void UpdateLabelFilter()
        {
            var selectedPalette = PaletteManager.selectedPalette;
            if (selectedPalette == null) return;
            foreach (var brush in selectedPalette.brushes)
            {
                foreach (var item in brush.items)
                {
                    if (item.prefab == null) continue;
                    var labels = UnityEditor.AssetDatabase.GetLabels(item.prefab);
                    foreach (var label in labels)
                    {
                        if (labelFilter.ContainsKey(label)) continue;
                        labelFilter.Add(label, false);
                    }
                }
            }
        }

        private void SelectLabelFilter(object key)
        {
            labelFilter[(string)key] = !labelFilter[(string)key];
            foreach (var pair in labelFilter)
            {
                if (!pair.Value) continue;
                var labelFilter = "l:" + pair.Key;
                if (filterText.Contains(labelFilter)) continue;
                if (filterText.Length > 0) filterText += ", ";
                filterText += labelFilter;
            }
            var filterTextArray = filterText.Split(',');
            filterText = string.Empty;
            for (int i = 0; i < filterTextArray.Length; ++i)
            {
                var filter = filterTextArray[i].Trim();
                if (filter.Length >= 2 && filter.Substring(0, 2) == "l:")
                {
                    var label = filter.Substring(2);
                    if (!labelFilter.ContainsKey(label)) continue;
                    if (!labelFilter[label]) continue;
                    if (filterText.Contains(filter)) continue;
                }
                if (filter == string.Empty) continue;
                filterText += filter + ", ";
            }
            if (filterText != string.Empty) filterText = filterText.Substring(0, filterText.Length - 2);
            UpdateFilteredList(false);
            Repaint();
        }

        public int FilterBySelection()
        {
            var selection = SelectionManager.GetSelectionPrefabs();
            filterText = string.Empty;
            for (int i = 0; i < selection.Length; ++i)
            {
                filterText += "w:" + selection[i].name;
                if (i < selection.Length - 1) filterText += ", ";
            }
            UpdateFilteredList(false);
            return filteredBrushListCount;
        }

        public void SelectFirstBrush()
        {
            if (filteredBrushListCount == 0) return;
            DeselectAllButThis(filteredBrushList[0].index);
        }
        #endregion
    }
}
