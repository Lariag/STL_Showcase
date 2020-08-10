using STL_Showcase.Logic.Localization;
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
        public bool IsLoaded { get; private set; }

        private int MaxNameLength = 20;

        public Model3DViewInfo()
        {
            RenderAspectList = new ObservableCollection<RenderAspectItem>(((IEnumerable<int>)Enum.GetValues(typeof(RenderAspectEnum))).Select(en => new RenderAspectItem(en)));
            SetData(string.Empty, 0, 0, 0);
        }
        public void SetData(string name, int tris, int verts, int sizeKB)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelName = Loc.GetText("NoModelSelected");
                ModelTris = Loc.GetTextFormatted("NumberTriangles", "-");
                ModelVerts = Loc.GetTextFormatted("NumberVertices", "-");
                ModelSizeKB = Loc.GetTextFormatted("NumberKB", "-");
                IsLoaded = false;
            }
            else
            {
                ModelName = name;
                ModelTris = Loc.GetTextFormatted("NumberTriangles", tris);
                ModelVerts = Loc.GetTextFormatted("NumberVertices", verts);
                ModelSizeKB = Loc.GetTextFormatted("NumberKB", sizeKB);
                IsLoaded = true;
            }

            NotifyPropertyChanged(nameof(ModelName));
            NotifyPropertyChanged(nameof(ModelTris));
            NotifyPropertyChanged(nameof(ModelVerts));
            NotifyPropertyChanged(nameof(ModelSizeKB));
            NotifyPropertyChanged(nameof(IsLoaded));
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
