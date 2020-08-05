using STL_Showcase.Shared.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Presentation.UI.Clases
{
    public class ModelItemListData : INotifyPropertyChanged
    {

        public ModelItemListData()
        {
            var userSettings = DefaultFactory.GetDefaultUserSettings();
            _FileOnlyFoldersFilter = userSettings.GetSettingBool(Shared.Enums.UserSettingEnum.EnableTreeOnlyFolders);
            _FileCollectionMode = userSettings.GetSettingBool(Shared.Enums.UserSettingEnum.EnableTreeCollections);
        }

        private ObservableCollection<ModelTreeItem> _ModelTreeRoot = new ObservableCollection<ModelTreeItem>();
        public ObservableCollection<ModelTreeItem> ModelTreeRoot { get { return _ModelTreeRoot; } set { _ModelTreeRoot = value; NotifyPropertyChanged(nameof(ModelTreeRoot)); } }
        private ModelTreeItem _SelectedTreeItem;
        public ModelTreeItem SelectedTreeItem { get { return _SelectedTreeItem; } set { _SelectedTreeItem = value; NotifyPropertyChanged(nameof(SelectedTreeItem)); } }
        private ObservableCollection<ModelListItem> _ModelList = new ObservableCollection<ModelListItem>();
        private Dictionary<string, ModelListItem> _ModelListDictionary;
        private List<string> _LoadedDirectories = new List<string>();


        public ObservableCollection<ModelListItem> ModelList {
            get { return _ModelList; }
            set
            {
                _ModelList = value;
                _ModelListDictionary = value.ToDictionary(m => m.FileData.FileFullPath, a => a);
                NotifyPropertyChanged(nameof(ModelList));
            }
        }
        public ModelListItem GetFromInternalDictionary(string fullFilePath)
        {
            if (_ModelListDictionary.TryGetValue(fullFilePath, out ModelListItem m))
                return m;
            return null;
        }
        public ObservableCollection<ModelListItem> ModelListFiltered {
            get
            {
                return new ObservableCollection<ModelListItem>(ModelList.Where(m => m.FileData != null &&
                  (string.IsNullOrWhiteSpace(ModelListTextFilter) || m.FileData.FileName.IndexOf(ModelListTextFilter, 0, StringComparison.InvariantCultureIgnoreCase) >= 0) &&
                  (string.IsNullOrWhiteSpace(ModelListDirectoryFilter) || m.FileData.FileFullPath.Contains(ModelListDirectoryFilter)) &&
                    ((FileTypeFilterSTL && m.FileData.FileName.EndsWith(".stl", StringComparison.InvariantCultureIgnoreCase) ||
                    (FileTypeFilterOBJ && m.FileData.FileName.EndsWith(".obj", StringComparison.InvariantCultureIgnoreCase)) ||
                    (FileTypeFilter3MF && m.FileData.FileName.EndsWith(".3mf", StringComparison.InvariantCultureIgnoreCase)))
                  )));
            }
        }
        private ModelListItem _SelectedListItem;
        public ModelListItem SelectedListItem {
            get { return _SelectedListItem; }
            set
            {
                if (_SelectedListItem != null)
                    _SelectedListItem.IsSelected = false;
                _SelectedListItem = value;
                if (_SelectedListItem != null)
                    _SelectedListItem.IsSelected = true;
                NotifyPropertyChanged(nameof(SelectedListItem));
            }
        }

        private string _ModelListTextFilter;
        public string ModelListTextFilter {
            get { return _ModelListTextFilter ?? ""; }
            set { _ModelListTextFilter = value?.Trim(); NotifyPropertyChanged(nameof(ModelListTextFilter)); }
        }
        private string _ModelListDirectoryFilter;

        public string ModelListDirectoryFilter {
            get { return _ModelListDirectoryFilter ?? string.Empty; }
            set { _ModelListDirectoryFilter = value?.Trim(); NotifyPropertyChanged(nameof(ModelListDirectoryFilter)); NotifyPropertyChanged(nameof(ModelListDirectoryFilterShortened)); }
        }

        public string ModelListDirectoryFilterShortened {
            get
            {
                if (string.IsNullOrEmpty(_ModelListDirectoryFilter))
                    return string.Empty;
                return _ModelListDirectoryFilter.Length > 40 ? $"...{_ModelListDirectoryFilter.Substring(_ModelListDirectoryFilter.Length - 40)}" : _ModelListDirectoryFilter;
            }
        }

        private bool _FileTypeFilterSTL;
        public bool FileTypeFilterSTL { get { return _FileTypeFilterSTL; } set { _FileTypeFilterSTL = value; NotifyPropertyChanged(nameof(FileTypeFilterSTL)); } }
        private bool _FileTypeFilterOBJ;
        public bool FileTypeFilterOBJ { get { return _FileTypeFilterOBJ; } set { _FileTypeFilterOBJ = value; NotifyPropertyChanged(nameof(FileTypeFilterOBJ)); } }
        private bool _FileTypeFilter3MF;
        public bool FileTypeFilter3MF { get { return _FileTypeFilter3MF; } set { _FileTypeFilter3MF = value; NotifyPropertyChanged(nameof(FileTypeFilter3MF)); } }
        private bool _FileOnlyFoldersFilter;
        public bool FileOnlyFoldersFilter { get { return _FileOnlyFoldersFilter; } set { _FileOnlyFoldersFilter = value; NotifyPropertyChanged(nameof(FileOnlyFoldersFilter)); } }
        private bool _FileCollectionMode;
        public bool FileCollectionMode {
            get { return _FileCollectionMode; }
            set
            {
                foreach (var treeRoot in this._ModelTreeRoot)
                {
                    if (value) treeRoot.GenerateCollectionsRecursive();
                    else treeRoot.RemoveCollectionsRecursive();
                    treeRoot.OrderChildsByDataAndText(true);
                }

                _FileCollectionMode = value; NotifyPropertyChanged(nameof(FileCollectionMode)); NotifyPropertyChanged(nameof(ModelTreeRoot));
            }
        }

        private bool _ModelListDirectionOrder = true;
        private string _ModelListTextOrder = "Directory";
        public string ModelListTextOrder {
            get { return _ModelListTextOrder ?? ""; }
            set
            {
                if (_ModelListTextOrder == value) _ModelListDirectionOrder = !_ModelListDirectionOrder;
                else _ModelListDirectionOrder = true;
                _ModelListTextOrder = value;

                if (_ModelListDirectionOrder)
                    this._ModelList = new ObservableCollection<ModelListItem>(_ModelList.OrderByDescending(m => m.FileData == null ? "" : m.FileData.OrderFunc(_ModelListTextOrder)));
                else
                    this._ModelList = new ObservableCollection<ModelListItem>(_ModelList.OrderBy(m => m.FileData == null ? "" : m.FileData.OrderFunc(_ModelListTextOrder)));

                NotifyPropertyChanged(nameof(ModelListTextOrder));
            }
        }

        private int _zoomSizeBase = 68; // Initial list item.
        private int _zoomSizeStep = 30; // Increments of size over the original size.

        private double _CurrentZoomLevel = 1;
        public double CurrentZoomLevel {
            get { return _CurrentZoomLevel; }
            set { _CurrentZoomLevel = value; ZoomLevelChanged?.Invoke(this, null); NotifySizePropertyChanged(); }
        }
        public double MaximumZoomLevel { get; set; }

        public int ModelListItemContentSize {
            get { return this._zoomSizeBase + (int)(this._zoomSizeStep * _CurrentZoomLevel); }
        }
        public int ModelListItemMargin { get; } = 2;
        public int ModelListItemBorderSize { get { return ModelListItemContentSize + 10; } }
        public int ModelListItemShadowSize { get { return ModelListItemContentSize + 8; } }

        public string ModelListItemColor { get; set; } = "White";

        public void ApplyFilterToTree()
        {
            foreach (var node in this.ModelTreeRoot)
                node.IsVisible = ApplyFilterToTreeRecursive(node) || !node.HasData; // Keep root directories visible.
        }
        private bool ApplyFilterToTreeRecursive(ModelTreeItem node)
        {
            bool visible = false;
            foreach (var child in node.ChildItems)
            {
                if (child.HasData && !this.FileOnlyFoldersFilter)
                {
                    child.IsVisible = (this.FileTypeFilterSTL && child.Text.EndsWith(".stl", StringComparison.InvariantCultureIgnoreCase) ||
                        this.FileTypeFilterOBJ && child.Text.EndsWith(".obj", StringComparison.InvariantCultureIgnoreCase) ||
                        this.FileTypeFilter3MF && child.Text.EndsWith(".3mf", StringComparison.InvariantCultureIgnoreCase));

                    visible = visible || child.IsVisible;
                }
                else
                {
                    visible = ApplyFilterToTreeRecursive(child) || visible || this.FileOnlyFoldersFilter;
                }
            }
            node.IsVisible = visible;
            return visible;
        }
        public void Reset()
        {
            this.SelectedTreeItem = null;
            this.SelectedListItem = null;
            this.FileTypeFilterSTL = true;
            this.FileTypeFilterOBJ = true;
            this.FileTypeFilter3MF = true;
            this.FileOnlyFoldersFilter = false;
            this._LoadedDirectories = new List<string>();
            this.ModelList = new ObservableCollection<ModelListItem>();
            this.ModelTreeRoot = new ObservableCollection<ModelTreeItem>();
            this._ModelListTextOrder = "Directory";
            this._ModelListTextFilter = "";
            this._ModelListDirectoryFilter = "";
        }

        #region Property Change

        public void NotifySizePropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModelListItemContentSize)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModelListItemBorderSize)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModelListItemShadowSize)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentZoomLevel)));
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ChangeZoomLevel(int zoomChange)
        {
            if (CurrentZoomLevel + zoomChange >= 0 && CurrentZoomLevel + zoomChange <= MaximumZoomLevel)
                this.CurrentZoomLevel += zoomChange;
        }

        public bool IsDirectoryLoaded(string path, bool checkSubdirectories)
        {
            return (checkSubdirectories && _LoadedDirectories.Any(p => path.Contains(p))) || _LoadedDirectories.Contains(path);
        }
        public void AddDirectoryLoaded(string path)
        {
            if (!IsDirectoryLoaded(path, true))
                _LoadedDirectories.Add(path);
        }
        public void AddDirectoriesLoaded(IEnumerable<string> paths)
        {
            foreach (string path in paths)
                AddDirectoryLoaded(path);
        }
        public void RemoveDirectoryLoaded(string path)
        {
            _LoadedDirectories.Remove(path);
        }
        public IEnumerable<string> GetDirectoriesLoaded()
        {
            return _LoadedDirectories.ToArray();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ZoomLevelChanged;

        #endregion
    }
}
