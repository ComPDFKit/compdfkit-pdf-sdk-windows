using ComPDFKit.Import;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.Viewer.Helper;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ComPDFKit.Tool.DrawTool;
using ComPDFKitViewer.Helper;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ComPDFKit.Tool.SettingParam;
using System.Windows.Media;

namespace ComPDFKit.Controls.Edit
{
    public partial class PDFPathEditControl : UserControl, INotifyPropertyChanged
    {
        #region property
        public CPDFViewerTool ToolView { get; set; }
        public List<PathEditParam> EditEvents { get; set; } = new List<PathEditParam>();

        private Visibility _onlySingleVisible = Visibility.Collapsed;
        public Visibility OnlySingleVisible
        {
            get => _onlySingleVisible;
            set => UpdateProper(ref _onlySingleVisible, value);
        }

        #endregion 

        public PDFPathEditControl()
        {
            DataContext = this;
            InitializeComponent();
            Loaded += PDPathEditControl_Loaded;
            Unloaded += PDFPathEditControl_Unloaded;
        }

        #region Load unload custom control

        private void PDPathEditControl_Loaded(object sender, RoutedEventArgs e)
        {
            RotateUI.RotationChanged -= RotateUI_RotationChanged;
            FlipUI.FlipChanged -= FlipUI_FlipChanged;
            RotateUI.RotationChanged += RotateUI_RotationChanged;
            FlipUI.FlipChanged += FlipUI_FlipChanged;
            ToolView.SelectedDataChanged -= ToolView_SelectedDataChanged;
            ToolView.SelectedDataChanged += ToolView_SelectedDataChanged;

            StrokeColorUI.ColorChanged -= StrokeColorUI_ColorChanged;
            StrokeColorUI.ColorChanged += StrokeColorUI_ColorChanged;

            FillColorUI.ColorChanged -= FillColorUI_ColorChanged;
            FillColorUI.ColorChanged += FillColorUI_ColorChanged;
        }

        private void StrokeColorUI_ColorChanged(object sender, EventArgs e)
        {
            SolidColorBrush newBrush = StrokeColorUI.Brush as SolidColorBrush;
            GetPathArea(out List<CPDFEditPathArea> pathAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (pathAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(pathAreas[0].GetFrame());
                bool result = pathAreas[0].SetStrokeColor(new byte[] { newBrush.Color.R, newBrush.Color.G, newBrush.Color.B});
                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, pathAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditPathArea pathArea in pathAreas)
                {
                    if (pathArea.SetStrokeColor(new byte[] { newBrush.Color.R, newBrush.Color.G, newBrush.Color.B }))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }

                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                    ToolView.GetCPDFViewer()?.UpdateRenderFrame();
                }
            }

            editPage.EndEdit();
            if (EditEvents.Count > 0 && newBrush != null)
            {
                byte[] Color = new byte[3];
                Color[0] = newBrush.Color.R;
                Color[1] = newBrush.Color.G;
                Color[2] = newBrush.Color.B;
                EditEvents.FirstOrDefault().StrokeColor = Color;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvents.FirstOrDefault());
            }
        }

        private void FillColorUI_ColorChanged(object sender, EventArgs e)
        {
            SolidColorBrush newBrush = FillColorUI.Brush as SolidColorBrush;
            GetPathArea(out List<CPDFEditPathArea> pathAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (pathAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(pathAreas[0].GetFrame());
                bool result = pathAreas[0].SetFillColor(new byte[] { newBrush.Color.R, newBrush.Color.G, newBrush.Color.B });
                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, pathAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditPathArea pathArea in pathAreas)
                {
                    if (pathArea.SetFillColor(new byte[] { newBrush.Color.R, newBrush.Color.G, newBrush.Color.B }))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }

                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                    ToolView.GetCPDFViewer()?.UpdateRenderFrame();
                }
            }

            editPage.EndEdit();
            if (EditEvents.Count > 0 && newBrush != null)
            {
                byte[] Color = new byte[3];
                Color[0] = newBrush.Color.R;
                Color[1] = newBrush.Color.G;
                Color[2] = newBrush.Color.B;
                EditEvents.FirstOrDefault().FillColor = Color;
                DefaultSettingParam defaultSettingParam = ToolView.GetDefaultSettingParam();
                defaultSettingParam.SetPDFEditParamm(EditEvents.FirstOrDefault());
            }
        }

        private void PDFPathEditControl_Unloaded(object sender, RoutedEventArgs e)
        {
            RotateUI.RotationChanged -= RotateUI_RotationChanged;
            FlipUI.FlipChanged -= FlipUI_FlipChanged;
        }

        #endregion

        #region Property changed

        private void PathCut()
        {
            GetPathArea(out List<CPDFEditPathArea> pathAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (pathAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            SelectedRect selectedRect = ToolView.GetSelectedRectForEditAreaObject(pathAreas[0]);
            if (selectedRect == null)
                return;

            Rect oldRect = DataConversionForWPF.CRectConversionForRect(pathAreas[0].GetFrame());
            double currentZoom = ToolView.GetCPDFViewer().CurrentRenderFrame.ZoomFactor;
            Rect rect = selectedRect.GetRect();
            Rect maxRect = selectedRect.GetMaxRect();
            Rect pdfRect = new Rect((rect.X - maxRect.X) / currentZoom, (rect.Y - maxRect.Y) / currentZoom, rect.Width / currentZoom, rect.Height / currentZoom);
            pdfRect = DpiHelper.StandardRectToPDFRect(pdfRect);
            CRect newCRect = new CRect((float)pdfRect.Left, (float)pdfRect.Bottom, (float)pdfRect.Right, (float)pdfRect.Top);
            if (pathAreas[0].CutWithRect(newCRect))
            {
                PDFEditHistory editHistory = new PDFEditHistory();
                editHistory.EditPage = editPage;
                editHistory.PageIndex = pdfPage.PageIndex;

                ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                ToolView.UpdateRender(oldRect, pathAreas[0]);
            }

            editPage.EndEdit();
        }

        private void ToolView_SelectedDataChanged(object sender, SelectedAnnotData e)
        {
            if (ToolView.GetIsCropMode())
            {
                PathCut();
            }
        }

        #endregion

        #region Init PDFViewer
        public void InitWithPDFViewer(CPDFViewerTool newPDFView)
        {
            ToolView = newPDFView;
        }
        #endregion

        #region public method

        //public void SetRotationText(float rotation)
        //{
        //    RotationTxb.Text = rotation.ToString(CultureInfo.CurrentCulture);
        //}

        #endregion

        #region Path Edit

        private void SetPathTransparency(double transparency)
        {
            PathOpacitySlider.Value = transparency / 255D;
            OpacityTextBox.Text = string.Format("{0}%", (int)(Math.Round(PathOpacitySlider.Value * 100)));
        }

        public void SetPDFPathEditData(List<PathEditParam> newEvents)
        {
            EditEvents = newEvents.Where(newEvent => newEvent.EditIndex >= 0 && newEvent.EditType == CPDFEditType.EditPath).ToList();
            if (EditEvents.Count == 0)
                return;

            PathEditParam defaultEvent = EditEvents.FirstOrDefault();

            SetPathTransparency(defaultEvent.Transparency);
            RotationTxb.Text = defaultEvent.Rotate.ToString();

            StrokeColorUI.SetCheckedForColor(Color.FromRgb(
            defaultEvent.StrokeColor[0],
            defaultEvent.StrokeColor[1],
            defaultEvent.StrokeColor[2]));

            FillColorUI.SetCheckedForColor(Color.FromRgb(
            defaultEvent.FillColor[0],
            defaultEvent.FillColor[1],
            defaultEvent.FillColor[2]));

            if (EditEvents.Count == 1)
            {
                OnlySingleVisible = Visibility.Visible;
            }
            else
            {
                OnlySingleVisible = Visibility.Collapsed;
            }
        }

        #endregion

        #region Property changed

        private void FlipUI_FlipChanged(object sender, bool e)
        {
            GetPathArea(out List<CPDFEditPathArea> pathAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (pathAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(pathAreas[0].GetFrame());
                bool result;
                if (e)
                {
                    result = pathAreas[0].VerticalMirror();
                }
                else
                {
                    result = pathAreas[0].HorizontalMirror();
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, pathAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditPathArea pathArea in pathAreas)
                {
                    bool result;
                    if (e)
                    {
                        result = pathArea.VerticalMirror();
                    }
                    else
                    {
                        result = pathArea.HorizontalMirror();
                    }

                    if (result)
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
        }

        private void RotateUI_RotationChanged(object sender, double e)
        {
            GetPathArea(out List<CPDFEditPathArea> pathAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (pathAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(pathAreas[0].GetFrame());
                if (pathAreas[0].Rotate((int)e))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, pathAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditPathArea pathArea in pathAreas)
                {
                    if (pathArea.Rotate((int)e))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            RotationTxb.Text = pathAreas.FirstOrDefault().GetRotation().ToString();
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                slider.Tag = "false";
            }
        }

        private void SliderOpacity_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                slider.Tag = "true";
            }

            GetPathArea(out List<CPDFEditPathArea> pathAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (pathAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(pathAreas[0].GetFrame());
                if (pathAreas[0].SetTransparency((byte)(PathOpacitySlider.Value * 255)))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, pathAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditPathArea pathArea in pathAreas)
                {
                    if (pathArea.SetTransparency((byte)(PathOpacitySlider.Value * 255)))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
        }

        private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (OpacityTextBox != null)
            {
                OpacityTextBox.Text = string.Format("{0}%", (int)(PathOpacitySlider.Value * 100));
            }

            if (slider != null && slider.Tag != null && slider.Tag.ToString() == "false")
            {
                return;
            }

            GetPathArea(out List<CPDFEditPathArea> pathAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (pathAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(pathAreas[0].GetFrame());
                if (pathAreas[0].SetTransparency((byte)(PathOpacitySlider.Value * 255)))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, pathAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditPathArea pathArea in pathAreas)
                {
                    if (pathArea.SetTransparency((byte)(PathOpacitySlider.Value * 255)))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
        }

        private void PathClipBtn_Click(object sender, RoutedEventArgs e)
        {
            ToolView.SetCropMode(!ToolView.GetIsCropMode());
        }

        private void OpacityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectItem = OpacityComboBox.SelectedItem as ComboBoxItem;
            if (selectItem != null && selectItem.Content != null)
            {
                if (double.TryParse(selectItem.Content.ToString().TrimEnd('%'), out double newOpacity))
                {
                    OpacityTextBox.Text = selectItem.Content.ToString();
                    PathOpacitySlider.Value = newOpacity / 100.0;
                }
            }
        }

        private void GetPathArea(out List<CPDFEditPathArea> pathAreas, out CPDFPage pdfPage, out CPDFEditPage editPage)
        {
            pathAreas = new List<CPDFEditPathArea>();
            editPage = null;
            pdfPage = null;
            if (ToolView == null || EditEvents.Count == 0)
            {
                return;
            }

            try
            {
                foreach (var EditEvent in EditEvents)
                {
                    CPDFViewer pdfViewer = ToolView.GetCPDFViewer();
                    CPDFDocument pdfDoc = pdfViewer.GetDocument();
                    pdfPage = pdfDoc.PageAtIndex(EditEvent.PageIndex);
                    editPage = pdfPage.GetEditPage();
                    List<CPDFEditArea> editAreas = editPage.GetEditAreaList();
                    if (editAreas != null && editAreas.Count > EditEvent.EditIndex)
                    {
                        pathAreas.Add(editAreas[EditEvent.EditIndex] as CPDFEditPathArea);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SetAbsRotation(double absRotation)
        {
            GetPathArea(out List<CPDFEditPathArea> pathAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (pathAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(pathAreas[0].GetFrame());
                int rotation = (int)absRotation - pathAreas[0].GetRotation();
                if (pathAreas[0].Rotate(rotation))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, pathAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditPathArea pathArea in pathAreas)
                {
                    int rotation = (int)absRotation - pathArea.GetRotation();
                    if (pathArea.Rotate(rotation))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
        }

        private void RotationTxb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(RotationTxb.Text, out double rotation))
            {
                return;
            }
            SetAbsRotation(rotation);
        }

        private void RotationTxb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RotationTxb_LostFocus(null, null);
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool UpdateProper<T>(ref T properValue,
                                           T newValue,
                                           [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return false;

            properValue = newValue;
            OnPropertyChanged(properName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
