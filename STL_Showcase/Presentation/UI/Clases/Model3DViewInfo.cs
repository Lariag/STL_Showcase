using STL_Showcase.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Presentation.UI.Clases
{
    public class Model3DViewInfo : INotifyPropertyChanged
    {
        public ObservableCollection<RenderAspectItem> RenderAspectList { get; set; }
        private RenderAspectEnum _SelectedRenderAspect { get; set; }
        public RenderAspectEnum SelectedRenderAspect { get { return _SelectedRenderAspect; } set { _SelectedRenderAspect = value; NotifyPropertyChanged(nameof(SelectedRenderAspect)); } }
        public string ModelName { get; private set; }
        public string ModelTris { get; private set; }
        public string ModelVerts { get; private set; }
        public string ModelSizeKB { get; private set; }

        private int MaxNameLength = 20;

        public Model3DViewInfo()
        {
            RenderAspectList = new ObservableCollection<RenderAspectItem>(((IEnumerable<int>)Enum.GetValues(typeof(RenderAspectEnum))).Select(en => new RenderAspectItem(en)));
        }
        public void SetData(string name, int tris, int verts, int sizeKB)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelName = "No model selected";
                ModelTris = "- Triangles";
                ModelVerts = "- Vertices";
                ModelSizeKB = "- KB";
            }
            else
            {
                ModelName = name.Length > MaxNameLength ? name.Remove(MaxNameLength - 3, name.Length - MaxNameLength).Insert(MaxNameLength - 3, "...") : name;
                ModelTris = $"{tris} Triangles";
                ModelVerts = $"{verts} Vertices";
                ModelSizeKB = $"{sizeKB} KB";
            }

            NotifyPropertyChanged(nameof(ModelName));
            NotifyPropertyChanged(nameof(ModelTris));
            NotifyPropertyChanged(nameof(ModelVerts));
            NotifyPropertyChanged(nameof(ModelSizeKB));
        }
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public class RenderAspectItem
        {
            public RenderAspectItem(int index)
            {
                this.Index = index;
            }
            public int Index { get; set; }
        }
    }
}
