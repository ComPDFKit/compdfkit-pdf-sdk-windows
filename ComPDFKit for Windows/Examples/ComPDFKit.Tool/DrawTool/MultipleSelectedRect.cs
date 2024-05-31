using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ComPDFKit.Tool.DrawTool
{
    public partial class MultipleSelectedRect: DrawingVisual
    {
        protected DrawingContext drawDC { get; set; }

        /// <summary>
        /// 数据改变中事件
        /// </summary>
        public event EventHandler<SelectedAnnotData> MultipleDataChanging;

        /// <summary>
        /// 数据改变完成事件
        /// </summary>
        public event EventHandler<SelectedAnnotData> MultipleDataChanged;

        protected bool isHover = false;

        protected bool isSelected = false;

        protected SelectedType selectedType = SelectedType.None;

        public void SetIsHover(bool hover)
        {
            isHover = hover;
        }

        public void SetIsSelected(bool selected)
        {
            isSelected = selected;
        }
    }
}
