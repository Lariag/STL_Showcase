using STL_Showcase.Logic.Files;
using System;
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
        private bool _IsVisible;
        public bool IsVisible { get { return _IsVisible; } set { _IsVisible = value; NotifyPropertyChanged(nameof(IsVisible)); } }
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
            SuscribeDataPropertyEvent();
        }

        /// <summary>
        /// Cuts all the single-child root, returning the first child that has more than 1 child of its own. 
        /// If the first node has 0 childs, returns itself.
        /// Doesn't set the new root node parent to null.
        /// </summary>
        public ObservableCollection<ModelTreeItem> Trim()
        {
            if (this.ChildItems.Count == 1)
                return ChildItems[0].Trim();
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

        public IEnumerable<string> GetTextsToRoot()
        {
            if (this.ParentItem == null)
                return new[] { Text };
            return this.ParentItem.GetTextsToRoot().Append(this.Text);
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
        public void SetIsExpandedAll(bool expanded)
        {
            foreach (var child in ChildItems)
                child.SetIsExpandedAll(expanded);
            this.IsExpanded = expanded;
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
