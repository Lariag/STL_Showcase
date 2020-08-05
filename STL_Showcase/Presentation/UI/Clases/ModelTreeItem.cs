using STL_Showcase.Logic.Files;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace STL_Showcase.Presentation.UI.Clases
{


    public class ModelTreeItem : INotifyPropertyChanged
    {

        private static ConcurrentDictionary<string, bool> _DictionaryIsExpanded = new ConcurrentDictionary<string, bool>();
        public ModelTreeItem ParentItem { get; set; }
        public ObservableCollection<ModelTreeItem> ChildItems { get; set; } = new ObservableCollection<ModelTreeItem>();
        public string Text { get; set; }
        public string TextPlusParents { get { return string.Join(System.IO.Path.DirectorySeparatorChar + "", GetTextsToRoot()); } }
        public int TotalLeafs { get { return ChildItems.Sum(c => c.HasData ? 1 : c.TotalLeafs); } }
        public int TotalSubNodes { get { return ChildItems.Sum(c => !c.HasData ? 1 : c.TotalSubNodes); } }
        public BitmapSource Image {
            get
            {
                return Data != null && _ImageFunc != null ? _ImageFunc(Data) : null;
            }
        }
        private bool _IsExpanded;
        public bool IsExpanded {
            get { return _IsExpanded; }
            set
            {
                string key = $"{this.Level}_{this.Text}";
                _DictionaryIsExpanded.AddOrUpdate(key, (asd) => value, (das, dsa) => value);

                _IsExpanded = value;
                if (_IsExpanded)
                {
                    var parent = this.ParentItem;
                    while (parent != null && !parent.IsExpanded)
                    {
                        parent.IsExpanded = _IsExpanded;
                        parent = parent.ParentItem;
                    }
                }
                NotifyPropertyChanged(nameof(IsExpanded));
            }
        }
        public bool HasExpandedCache { get { return _DictionaryIsExpanded.TryGetValue($"{this.Level}_{this.Text}", out _IsExpanded); } }
        private bool _IsVisible;
        public bool IsVisible { get { return _IsVisible; } set { _IsVisible = value; NotifyPropertyChanged(nameof(IsVisible)); } }
        private bool _IsVirtual;
        public bool IsVirtual { get { return _IsVirtual; } set { _IsVirtual = value; NotifyPropertyChanged(nameof(IsVirtual)); } }
        public int Level { get; set; }
        private object _Data;
        public object Data { get { return _Data; } set { _Data = value; SuscribeDataPropertyEvent(); } }
        public bool HasData { get { return Data != null; } }

        private Func<object, BitmapSource> _ImageFunc;

        public ModelTreeItem(int level, string text, object data = default(object), Func<object, BitmapSource> imageFunc = null)
        {
            this.Level = level;
            this.Text = text;
            this.Data = data;
            this._ImageFunc = imageFunc;
            this._IsVisible = true;
            _DictionaryIsExpanded.TryGetValue($"{level}_{text}", out _IsExpanded);
            SuscribeDataPropertyEvent();
        }

        public IEnumerable<ModelTreeItem> GetAllChildItems()
        {
            foreach (var child in ChildItems)
            {
                yield return child;
                foreach (var childOfChild in child.GetAllChildItems())
                    yield return childOfChild;
            }
        }

        /// <summary>
        /// Cuts all the single-child root, returning the first child that has more than 1 child of its own. 
        /// If the first node has 0 childs, returns itself.
        /// Doesn't set the new root node parent to null.
        /// </summary>
        public ObservableCollection<ModelTreeItem> Trim()
        {
            if (this.ChildItems.Count == 1)
            {
                return ChildItems[0].Trim();
            }
            else if (!ChildItems.Any(ci => ci.HasData))
            {
                return new ObservableCollection<ModelTreeItem>(ChildItems.SelectMany(ci => ci.Trim()));
            }
            return new ObservableCollection<ModelTreeItem>(new ModelTreeItem[] { this });

        }

        /// <summary>
        /// Builds the three, recursively, parsing a string of full file paths. 
        /// Only assings data for the leaf nodes.
        /// </summary>
        /// <param name="tuples">Item1: Full file path, separated by the directory separator. Item2: Data item.</param>
        public void BuildTreeRecursive(IEnumerable<Tuple<string[], object>> tuples)
        {
            string lastPath = "";
            foreach (var tuple in tuples)
            {
                if (tuple.Item1.Length <= this.Level) continue;
                if (tuple.Item1[this.Level] == lastPath) continue;

                if (this.Text == tuple.Item1[this.Level - 1])
                {
                    var newNode = new ModelTreeItem(this.Level + 1, lastPath = tuple.Item1[this.Level], imageFunc: this._ImageFunc);
                    newNode.ParentItem = this;
                    this.ChildItems.Add(newNode);
                    if (tuple.Item1.Length == this.Level + 1)
                        newNode.Data = tuple.Item2;
                    else
                        newNode.BuildTreeRecursive(tuples);
                }
            }
            this.ChildItems = new ObservableCollection<ModelTreeItem>(this.ChildItems.OrderBy(c => c.HasData));
        }

        public IEnumerable<string> GetTextsToRoot(bool includeVirtual = false)
        {
            if (this.IsVirtual && !includeVirtual)
                return this.ParentItem?.GetTextsToRoot(includeVirtual) ?? new string[0];
            else
                return this.ParentItem?.GetTextsToRoot(includeVirtual).Append(this.Text) ?? new[] { Text };
        }
        //public int GetTextsToRoot()
        //{
        //    foreach (var child in ChildItems)
        //        if (child.HasData)
        //            return 1;
        //        else
        //            return child.GetTextsToRoot()
        //    return this.ParentItem.GetTextsToRoot().Append(this.Text);
        //}


        public ModelTreeItem VirtualizeChildrenByTextPrefix(string virtualNodeText, string textPrefix)
        {

            var selectedChildren = ChildItems.Where(c => c.Text.StartsWith(textPrefix)).ToArray();
            ModelTreeItem newNode = new ModelTreeItem(this.Level + 1, virtualNodeText, imageFunc: this._ImageFunc) { IsVirtual = true, ParentItem = this };
            if (!selectedChildren.Any())
                return newNode;

            //int oldIndexInChildItems = this.ChildItems.IndexOf(selectedChildren.First());

            foreach (var selectedChild in selectedChildren)
            {
                ChildItems.Remove(selectedChild);
                selectedChild.ParentItem = newNode;
                selectedChild.IncreaseLevelRecursive(1);
                newNode.ChildItems.Add(selectedChild);
            }

            //this.ChildItems.Insert(oldIndexInChildItems, newNode);
            this.ChildItems.Add(newNode);
            return newNode;
        }

        public void IncreaseLevelRecursive(int increment)
        {
            this.Level += increment;
            foreach (var child in ChildItems) // TODO: Except for the new virtualized nodes.
                child.IncreaseLevelRecursive(increment);
        }

        private static char[] _CollectionSeparators = { '_', '-' };
        public void GenerateCollectionsRecursive()
        {
            List<ModelTreeItem> newNodes = new List<ModelTreeItem>();

            {
                // Identify collections by separator characters.
                var collectionGroups = ChildItems.GroupBy(c =>
                {
                    int indexOfSeparator = c.Text.IndexOfAny(_CollectionSeparators);
                    if (indexOfSeparator > 0)
                        return c.Text.Substring(0, indexOfSeparator);
                    return string.Empty;
                });

                foreach (var group in collectionGroups)
                {
                    if (group.Count() > 1 && group.Key != string.Empty)
                    {

                        // Locate the full collection name within the items text.
                        int originalSeparatorIndex = group.Key.Length;
                        int refinedSeparatorIndex = originalSeparatorIndex;

                        while (true)
                        {
                            if (group.First().Text.Length < refinedSeparatorIndex)
                                break;
                            int newSeparatorIndex = group.First().Text.IndexOfAny(_CollectionSeparators, refinedSeparatorIndex + 1);
                            if (newSeparatorIndex > refinedSeparatorIndex)
                            {
                                string newPrefix = group.First().Text.Substring(0, newSeparatorIndex);
                                if (group.Any(t => t.Text.Length < newSeparatorIndex || t.Text.Substring(0, newSeparatorIndex) != newPrefix))
                                    break;
                                refinedSeparatorIndex = newSeparatorIndex;
                            }
                            else
                                break;
                        }
                        string newCollectionKey = originalSeparatorIndex != refinedSeparatorIndex ? group.First().Text.Substring(0, refinedSeparatorIndex) : group.Key;

                        // Virtualize collection, except if the name matches with this node.
                        if (this.Text != newCollectionKey)
                            newNodes.Add(this.VirtualizeChildrenByTextPrefix(newCollectionKey, newCollectionKey));
                    }
                }
            }

            foreach (var child in ChildItems.Where(c => !newNodes.Contains(c)))
                child.GenerateCollectionsRecursive();
        }

        public void RemoveCollectionsRecursive()
        {
            if (this.IsVirtual)
            {
                this.ParentItem?.ChildItems.Remove(this);
                foreach (var child in ChildItems)
                    this.ParentItem?.ChildItems.Add(child);
            }

            var childs = ChildItems.ToArray();
            foreach (var child in childs)
                child.RemoveCollectionsRecursive();
        }

        public void SetIsExpandedAll(bool expanded)
        {
            foreach (var child in ChildItems)
                child.SetIsExpandedAll(expanded);
            this.IsExpanded = expanded;
        }

        public void OrderChildsByDataAndText(bool recursive)
        {
            var orderedChildItems = this.ChildItems.OrderBy(c => c.Text).OrderBy(c => c.Data != null).ToArray();
            ChildItems.Clear();
            foreach (var childItem in orderedChildItems)
                this.ChildItems.Add(childItem);

            if (recursive)
                foreach (var child in ChildItems)
                    child.OrderChildsByDataAndText(recursive);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SuscribeDataPropertyEvent()
        {
            if (Data != null && Data is INotifyPropertyChanged)
            {
                INotifyPropertyChanged n = (INotifyPropertyChanged)Data;
                n.PropertyChanged += DataPropertyChanger;
            }
        }
        private void DataPropertyChanger(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.IndexOf(nameof(Image), StringComparison.InvariantCultureIgnoreCase) >= 0)
                this.NotifyPropertyChanged(nameof(Image));
        }
#if DEBUG
        public void DebugListTree()
        {
            Console.WriteLine("".PadLeft(this.Level, ' ') + " " + this.Text + " " + (this.Data == null ? "DATA" : "null"));
            foreach (var child in ChildItems)
            {
                child.DebugListTree();
            }
        }
#endif
    }
}
