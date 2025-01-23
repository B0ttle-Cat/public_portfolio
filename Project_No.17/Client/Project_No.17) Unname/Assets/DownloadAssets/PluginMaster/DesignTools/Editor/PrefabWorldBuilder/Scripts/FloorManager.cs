/*
Copyright(c) 2021 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2021.

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
    public class FloorSettings : ICloneableToolSettings, IPaintToolSettings
    {
        [SerializeField] private TilesUtils.CellSizeType _cellSizeType = TilesUtils.CellSizeType.SMALLEST_OBJECT;
        [SerializeField] private Vector3 _cellSize = Vector3.one;
        [SerializeField] private Vector2 _spacing = Vector2.zero;
        [SerializeField] private AxesUtils.SignedAxis _axisAlignedWithNormal = AxesUtils.SignedAxis.UP;

        public System.Action OnDataChanged;

        public FloorSettings() : base()
        {
            _paintTool.OnDataChanged += DataChanged;
            OnDataChanged += DataChanged;
        }
        public TilesUtils.CellSizeType cellSizeType
        {
            get => _cellSizeType;
            set
            {
                if (_cellSizeType == value) return;
                _cellSizeType = value;
                UpdateCellSize();
                OnDataChanged();
            }
        }
        public Vector3 cellSize
        {
            get => _cellSize;
            set
            {
                if (_cellSize == value) return;
                _cellSize = value;
                OnDataChanged();
            }
        }
        public Vector2 spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                OnDataChanged();
            }
        }
        public AxesUtils.SignedAxis axisAlignedWithNormal
        {
            get => _axisAlignedWithNormal;
            set
            {
                if (_axisAlignedWithNormal == value) return;
                _axisAlignedWithNormal = value;
                UpdateCellSize();
                OnDataChanged();
            }
        }

        public void UpdateCellSize()
        {
            if (_cellSizeType != TilesUtils.CellSizeType.CUSTOM)
            {
                var toolSettings = FloorManager.settings;
                BrushSettings brush = PaletteManager.selectedBrush;
                if (toolSettings.overwriteBrushProperties) brush = toolSettings.brushSettings;
                if (brush == null) return;
                _cellSize = TilesUtils.GetCellSize(_cellSizeType, brush, _axisAlignedWithNormal,
                    _cellSize, tangentSpace: false);
                ToolProperties.RepainWindow();
                UnityEditor.SceneView.RepaintAll();
            }
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

        #endregion

        public void Clone(ICloneableToolSettings clone)
        {
            if (clone == null && !(clone is FloorSettings)) clone = new FloorSettings();
            var FloorToolClone = clone as FloorSettings;
            FloorToolClone.Copy(this);
        }

        public void DataChanged()
        {
            PWBCore.SetSavePending();
        }
        public void Copy(IToolSettings other)
        {
            var otherFloorSettings = other as FloorSettings;
            if (otherFloorSettings == null) return;
            _paintTool.Copy(otherFloorSettings._paintTool);
            _cellSizeType = otherFloorSettings._cellSizeType;
            _cellSize = otherFloorSettings._cellSize;
            _spacing = otherFloorSettings._spacing;
            _axisAlignedWithNormal = otherFloorSettings._axisAlignedWithNormal;
        }
    }

    [System.Serializable]
    public class FloorManager : ToolManagerBase<FloorSettings>
    {
        public enum ToolState
        {
            FIRST_CORNER,
            SECOND_CORNER
        }
        public static ToolState state { get; set; } = ToolState.FIRST_CORNER;
        public static Vector3 firstCorner { get; set; } = Vector3.zero;
        public static Vector3 secondCorner { get; set; } = Vector3.zero;
    }
    #endregion
    #region PWBIO
    public static partial class PWBIO
    {
        #region HANDLERS
        private static void FloorInitializeOnLoad()
        {
            FloorManager.settings.OnDataChanged += OnFloorSettingsChanged;
        }

        private static void SetSnapStepToFloorCellSize()
        {
            SnapManager.settings.step = FloorManager.settings.cellSize;
            SnapManager.settings.DataChanged(repaint);
        }

        private static void OnFloorSettingsChanged()
        {
            repaint = true;
            BrushstrokeManager.UpdateFloorBrushstroke();
            SetSnapStepToFloorCellSize();
        }
        #endregion
        public static void UpdateFloorSettingsOnBrushChanged(bool repaint)
        {
            FloorManager.settings.UpdateCellSize();
            SetSnapStepToFloorCellSize();
            FloorManager.state = FloorManager.ToolState.FIRST_CORNER;
        }
        public static void OnFloorEnabled()
        {
            SnapManager.settings.radialGridEnabled = false;
            SnapManager.settings.gridOnY = true;
            SnapManager.settings.visibleGrid = true;
            SnapManager.settings.lockedGrid = true;
            SnapManager.settings.snappingOnX = true;
            SnapManager.settings.snappingOnZ = true;
            SnapManager.settings.snappingEnabled = true;
            UpdateFloorSettingsOnBrushChanged(repaint: false);
            SnapManager.settings.DataChanged(repaint: true);
            FloorManager.state = FloorManager.ToolState.FIRST_CORNER;
        }
        private static Vector3 _floorSecondCorner = Vector3.zero;
        private static bool _floorDeleteMode = false;
        private static void FloorToolDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            var mousePos2D = Event.current.mousePosition;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePos2D);
            var mousePos3D = Vector3.zero;
            if (GridRaycast(mouseRay, out RaycastHit gridHit)) mousePos3D = SnapFloorTilePosition(gridHit.point);
            else return;
            if (Event.current.control && Event.current.type == EventType.KeyDown) _floorDeleteMode = true;
            else if (_floorDeleteMode && !Event.current.control && Event.current.type == EventType.KeyUp)
            {
                _floorDeleteMode = false;
                FloorManager.state = FloorManager.ToolState.FIRST_CORNER;
                return;
            }
            if (PaletteManager.selectedBrush == null) return;
            if (Event.current.button == 0)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    FloorManager.state = FloorManager.ToolState.SECOND_CORNER;
                    FloorManager.secondCorner = FloorManager.firstCorner = mousePos3D;
                    BrushstrokeManager.UpdateFloorBrushstroke(_floorDeleteMode);
                }
                if (FloorManager.state == FloorManager.ToolState.SECOND_CORNER)
                {
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        FloorManager.secondCorner = mousePos3D;
                        if (_floorSecondCorner != FloorManager.secondCorner)
                            BrushstrokeManager.UpdateFloorBrushstroke(_floorDeleteMode);
                    }
                    if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseMove)
                    {
                        FloorManager.secondCorner = mousePos3D;
                        if (_floorDeleteMode)
                        {
                            BrushstrokeManager.UpdateFloorBrushstroke(_floorDeleteMode);
                            DeleteFloor();
                        }
                        else
                        {
                            _paintStroke.Clear();
                            BrushstrokeManager.UpdateFloorBrushstroke();
                            PreviewFloorRectangle(sceneView.camera);
                            CreateFloor();
                        }
                        FloorManager.state = FloorManager.ToolState.FIRST_CORNER;
                        BrushstrokeManager.UpdateFloorBrushstroke();
                    }
                }
                _floorSecondCorner = FloorManager.secondCorner;
            }
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
            {
                FloorManager.state = FloorManager.ToolState.FIRST_CORNER;
                BrushstrokeManager.UpdateFloorBrushstroke();
            }
            switch (FloorManager.state)
            {
                case FloorManager.ToolState.FIRST_CORNER:
                    if (_floorDeleteMode) PreviewFloorDeleteSingleTile(sceneView.camera, mousePos3D);
                    else PreviewFloorSingleTile(sceneView.camera, mousePos3D);
                    break;
                case FloorManager.ToolState.SECOND_CORNER:
                    if (_floorDeleteMode)
                        PreviewFloorDeleteRectangle(sceneView.camera, mousePos3D);
                    else PreviewFloorRectangle(sceneView.camera);
                    break;
            }
        }
        private static Vector3 GetCenterToPivot(GameObject prefab, Vector3 scaleMult, Quaternion rotation)
        {
            var itemBounds = BoundsUtils.GetBoundsRecursive(prefab.transform, Quaternion.identity);
            var centerToPivot = prefab.transform.position - itemBounds.center;
            centerToPivot = Vector3.Scale(centerToPivot, scaleMult);
            centerToPivot = rotation * centerToPivot;
            return centerToPivot;
        }
        private static void PreviewFloorSingleTile(Camera camera, Vector3 mousePos3D)
        {
            if (BrushstrokeManager.brushstroke.Length == 0) return;
            var strokeItem = BrushstrokeManager.brushstroke[0].Clone();
            if (strokeItem.settings == null)
            {
                BrushstrokeManager.UpdateFloorBrushstroke();
                return;
            }
            var prefab = strokeItem.settings.prefab;
            if (prefab == null) return;
            BrushSettings brushSettings = strokeItem.settings;
            if (FloorManager.settings.overwriteBrushProperties) brushSettings = FloorManager.settings.brushSettings;
            var itemRotation = Quaternion.Euler(strokeItem.additionalAngle);
            var scaleMult = strokeItem.scaleMultiplier;

            var cellCenter = mousePos3D;
            var centerToPivot = GetCenterToPivot(prefab, scaleMult, itemRotation);
            var itemPosition = cellCenter + centerToPivot;
            var translateMatrix = Matrix4x4.Translate(Quaternion.Inverse(itemRotation) * -prefab.transform.position);
            var rootToWorld = Matrix4x4.TRS(itemPosition, itemRotation, scaleMult) * translateMatrix;
            var layer = FloorManager.settings.overwritePrefabLayer ? FloorManager.settings.layer : prefab.layer;

            PreviewBrushItem(prefab, rootToWorld, layer, camera,
                redMaterial: false, reverseTriangles: false, flipX: false, flipY: false);
        }

        private static void PreviewFloorRectangle(Camera camera)
        {
            BrushstrokeItem[] brushstroke = null;
            if (PreviewIfBrushtrokestaysTheSame(out brushstroke, camera, forceUpdate: _paintStroke.Count == 0)) return;
            if (BrushstrokeManager.brushstroke.Length == 0) return;
            _paintStroke.Clear();
            var toolSettings = FloorManager.settings;
            for (int i = 0; i < brushstroke.Length; ++i)
            {
                var strokeItem = brushstroke[i];
                if (strokeItem.settings == null) return;
                var prefab = strokeItem.settings.prefab;
                if (prefab == null) return;
                BrushSettings brushSettings = strokeItem.settings;
                if (toolSettings.overwriteBrushProperties) brushSettings = toolSettings.brushSettings;
                var scaleMult = strokeItem.scaleMultiplier;
                var itemRotation = Quaternion.Euler(strokeItem.additionalAngle);

                var cellCenter = strokeItem.tangentPosition;
                var centerToPivot = GetCenterToPivot(prefab, scaleMult, itemRotation);
                var itemPosition = cellCenter + centerToPivot;
                
                var nearbyObjects = new System.Collections.Generic.List<GameObject>();
                boundsOctree.GetColliding(nearbyObjects, new Bounds(cellCenter, FloorManager.settings.cellSize * 0.99f));

                if (nearbyObjects.Count > 0)
                {
                    bool checkNextItem = false;
                    foreach (var obj in nearbyObjects)
                    {
                        if (!obj.activeInHierarchy) continue;
                        if (PaletteManager.selectedBrush.ContainsSceneObject(obj))
                        {
                            checkNextItem = true;
                            break;
                        }
                    }
                    if (checkNextItem) continue;
                }

                var translateMatrix = Matrix4x4.Translate(Quaternion.Inverse(itemRotation) * -prefab.transform.position);
                var rootToWorld = Matrix4x4.TRS(itemPosition, itemRotation, scaleMult) * translateMatrix;
                var layer = toolSettings.overwritePrefabLayer ? toolSettings.layer : prefab.layer;
                PreviewBrushItem(prefab, rootToWorld, layer, camera,
                    redMaterial: false, reverseTriangles: false, flipX: false, flipY: false);
                _previewData.Add(new PreviewData(prefab, rootToWorld, layer, flipX: false, flipY: false));
                var itemScale = Vector3.Scale(prefab.transform.localScale, scaleMult);
                Transform parentTransform = toolSettings.parent;
                var paintItem = new PaintStrokeItem(prefab, itemPosition, itemRotation,
                    itemScale, layer, parentTransform, surface: null, flipX: false, flipY: false);
                _paintStroke.Add(paintItem);
            }
        }
        private static Mesh _cubeMesh = null;
        private static Mesh cubeMesh
        {
            get
            {
                if (_cubeMesh == null) _cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                return _cubeMesh;
            }
        }
        private static void PreviewFloorDeleteSingleTile(Camera camera, Vector3 mousePos3D)
        {
            if (BrushstrokeManager.brushstroke.Length == 0) return;
            var TRS = Matrix4x4.TRS(mousePos3D, SnapManager.settings.rotation, FloorManager.settings.cellSize);
            Graphics.DrawMesh(cubeMesh, TRS, transparentRedMaterial2, 0, camera);
        }

        private static System.Collections.Generic.HashSet<Vector3> _floorDeleteStroke
            = new System.Collections.Generic.HashSet<Vector3>();
        private static void PreviewFloorDeleteRectangle(Camera camera, Vector3 mousePos3D)
        {
            if (BrushstrokeManager.brushstroke.Length == 0) return;
            var brushstroke = BrushstrokeManager.brushstroke;
            var toolSettings = FloorManager.settings;
            _floorDeleteStroke.Clear();
            for (int i = 0; i < brushstroke.Length; ++i)
            {
                var strokeItem = brushstroke[i];
                var itemPosition = strokeItem.tangentPosition;
                var rootToWorld = Matrix4x4.TRS(itemPosition, SnapManager.settings.rotation, FloorManager.settings.cellSize);
                Graphics.DrawMesh(cubeMesh, rootToWorld, transparentRedMaterial2, layer: 0, camera);
                _floorDeleteStroke.Add(itemPosition);
            }
        }
        private static void CreateFloor()
        {
            Paint(FloorManager.settings);
        }
        private static void DeleteFloor()
        {
            if (_floorDeleteStroke.Count == 0) return;
            var toolSettings = FloorManager.settings;
            var octreeDistance = Mathf.Min(toolSettings.cellSize.x, toolSettings.cellSize.z) / 2;
            var toBeDeleted = new System.Collections.Generic.HashSet<GameObject>();
            foreach (var cellCenter in _floorDeleteStroke)
            {
                var nearbyObjects = new System.Collections.Generic.List<GameObject>();
                boundsOctree.GetColliding(nearbyObjects, new Bounds(cellCenter, FloorManager.settings.cellSize * 0.99f));
                if (nearbyObjects.Count == 0) continue;

                foreach (var obj in nearbyObjects)
                {
                    boundsOctree.Remove(obj);
                    if (obj == null) continue;
                    if (!obj.activeInHierarchy) continue;
                    if (PaletteManager.selectedBrush.ContainsSceneObject(obj)) toBeDeleted.Add(obj);
                }
            }
            void EraseObject(GameObject obj)
            {
                if (obj == null) return;
                var root = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(obj);
                if (root != null) obj = root;
                PWBCore.DestroyTempCollider(obj.GetInstanceID());
                UnityEditor.Undo.DestroyObjectImmediate(obj);
            }
            foreach (var obj in toBeDeleted) EraseObject(obj);
        }
    }
    #endregion
}
