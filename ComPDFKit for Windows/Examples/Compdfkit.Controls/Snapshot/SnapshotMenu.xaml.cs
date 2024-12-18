using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.Import;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool;
using ComPDFKitViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComPDFKit.Controls.Snapshot
{
    public partial class SnapshotMenu : UserControl
    {
        public SnapshotEditToolArgs SnapToolArgs { get; set; }
        public PDFViewControl PdfViewer { get; set; }
        public event EventHandler<KeyValuePair<string,object>> SnapToolEvent;

        public SnapshotMenu()
        {
            InitializeComponent();
            SetLangText();
        }

        private void SetLangText()
        {
            ToolTipService.SetToolTip(ReSnapshotBtn, LanguageHelper.CompressManager.GetString("ContentSelection_CancelDo"));
            ToolTipService.SetToolTip(SnapshotPrintBtn, LanguageHelper.CompressManager.GetString("Main_Print"));
            ToolTipService.SetToolTip(SnapshotSaveBtn, LanguageHelper.CompressManager.GetString("ContentSelection_Output"));
            ToolTipService.SetToolTip(SnapshotCloseBtn, LanguageHelper.CompressManager.GetString("ContentSelection_Exit"));
            ToolTipService.SetToolTip(SnapshotCopyBtn, LanguageHelper.CompressManager.GetString("Main_Copy"));
            ToolTipService.SetToolTip(SnapshotCorpBtn, LanguageHelper.CompressManager.GetString("Main_Crop"));
            ToolTipService.SetToolTip(SnapshotCorpBtn2, LanguageHelper.CompressManager.GetString("Main_Crop"));
        }

        private void Button_ReSnapshot(object sender, RoutedEventArgs e)
        {
            if(SnapToolArgs!=null)
            {
                SnapToolArgs.ReSnapshot();
            }
        }

        private void Button_SnapshotClose(object sender, RoutedEventArgs e)
        {
            if(SnapToolEvent!=null)
            {
                KeyValuePair<string, object> param = new KeyValuePair<string, object>("CloseSnap",null);
                SnapToolEvent.Invoke(this, param);
            }
        }

        private void Button_SnapshotCopy(object sender, RoutedEventArgs e)
        {
            if (SnapToolArgs != null)
            {
                SnapToolArgs.SaveSnapshotToClipboard();
                if (SnapToolEvent != null)
                {
                    KeyValuePair<string, object> param = new KeyValuePair<string, object>("CloseSnap", null);
                    SnapToolEvent.Invoke(this, param);
                }
            }
        }

        private int CropPageUI { get; set; }
       
        private void Button_SnapshotCorp(object sender, RoutedEventArgs e)
        {
            if (MessageBoxEx.Show(LanguageHelper.CompressManager.GetString("Corp_Customize_PRM") + "                                                           ", LanguageHelper.CompressManager.GetString("Corp_Customize"), System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
            {
                if (SnapToolArgs != null && PdfViewer != null)
                {
                    List<int> cropPageList = new List<int>();
                    Rect rect = SnapToolArgs.GetSnapshotPDFRect(out int pageIndexex);
                    cropPageList.Add(pageIndexex);
                    PdfViewer?.CropPage(CPDFDisplayBox.CropBox, rect, cropPageList);
                    PdfViewer.FocusPDFViewTool.GetCPDFViewer().GoToPage(pageIndexex);
                    KeyValuePair<string, object> param = new KeyValuePair<string, object>("Save", null);
                    SnapToolEvent.Invoke(this, param);

                    if (SnapToolEvent != null)
                    {
                        param = new KeyValuePair<string, object>("CloseSnap", null);
                        SnapToolEvent.Invoke(this, param);
                    }
                }
            }
        }

        private void Button_SnapshotSave(object sender, RoutedEventArgs e)
        {
            if (SnapToolArgs != null && PdfViewer != null && PdfViewer.PDFToolManager != null)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "jpg|*.jpg|png|*.png";
                dlg.FileName = PdfViewer.GetCPDFViewer().GetDocument().FileName;
                if(dlg.FileName==null ||dlg.FileName.Trim().Length==0)
                {
                    dlg.FileName = "Blank" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                if (dlg.ShowDialog() == true)
                {
                    string fileName = dlg.FileName;
                    WriteableBitmap saveBitmap= SnapToolArgs.GetSnapshotImage();
                    if(saveBitmap!=null)
                    {
                        if (dlg.SafeFileName.ToLower().EndsWith(".jpg"))
                        {
                            Stream saveStream = dlg.OpenFile();
                            JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
                            BitmapFrame frame = BitmapFrame.Create(saveBitmap);
                            jpgEncoder.Frames.Add(frame);
                            jpgEncoder.Save(saveStream);
                            saveStream.Dispose();
                        }
                        else if (dlg.SafeFileName.ToLower().EndsWith(".png"))
                        {
                            Stream saveStream = dlg.OpenFile();
                            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                            BitmapFrame frame = BitmapFrame.Create(saveBitmap);
                            pngEncoder.Frames.Add(frame);
                            pngEncoder.Save(saveStream);
                            saveStream.Dispose();
                        }

                        CommonHelper.ExplorerFile(dlg.FileName);
                    }

                    if (SnapToolEvent != null)
                    {
                        KeyValuePair<string, object> param = new KeyValuePair<string, object>("CloseSnap", null);
                        SnapToolEvent.Invoke(this, param);
                    }
                }
            }
        }

        private void Button_PrintSnapshot(object sender, RoutedEventArgs e)
        {
            if (SnapToolArgs != null)
            {
                try
                {
                    WriteableBitmap saveBitmap = SnapToolArgs.GetSnapshotImage();
                    if (saveBitmap != null)
                    {
                        PrintDialog printDlg = new PrintDialog();
                        if (printDlg.ShowDialog() == true)
                        {
                            DrawingVisual visualItem = new DrawingVisual();
                            DrawingContext drawContext = visualItem.RenderOpen();
                            drawContext.DrawImage(saveBitmap, new Rect(0, 0, saveBitmap.Width, saveBitmap.Height));
                            drawContext.Close();
                            printDlg.PrintVisual(visualItem, "Snapshot");
                        }
                    }

                    if (SnapToolEvent != null)
                    {
                        KeyValuePair<string, object> param = new KeyValuePair<string, object>("CloseSnap", null);
                        SnapToolEvent.Invoke(this, param);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }

    public class SnapshotToolArgs
    {
        public Color LineColor;
        public Color BgColor;
        public double LineWidth;
        public double Transparency;
        public DashStyle LineDash;
        public bool HasBgColor;

        public SnapshotToolArgs()
        {
            LineDash = new DashStyle();
            HasBgColor = true;
            LineColor = Color.FromRgb(49, 106, 197);
            BgColor = Color.FromRgb(153, 193, 218);
            LineWidth = 1;
            Transparency = 0.75;
        }
    }

    public class SnapshotEditToolArgs : SnapshotToolArgs
    {
        public Color HighLightColor;
        public Color ControlPointColor;
        public UserControl ToolPanel;
        public PDFViewControl Viewer { get; set; }

        public SnapshotEditToolArgs() : base()
        {
            HighLightColor = Colors.Transparent;
            ControlPointColor = Color.FromRgb(71, 126, 222);
        }

        public void ReSnapshot()
        {
            Viewer.FocusPDFViewTool.CleanPageSelectedRect();
        }

        public void SaveSnapshotToClipboard()
        {
            WriteableBitmap snapImage = GetSnapshotImage();
            if (snapImage != null)
            {
                try
                {
                    Clipboard.SetImage(snapImage);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public WriteableBitmap GetSnapshotImage()
        {
            if (Viewer != null && Viewer.SnapshotData != null)
            {
                CPDFDocument pdfDoc = Viewer.GetCPDFViewer().GetDocument();
                PageSelectedData snapData = Viewer.SnapshotData;
                CPDFViewer pdfViewer = Viewer.GetCPDFViewer();
                if (snapData.SelectRect.IsEmpty == false)
                {
                    try
                    {
                        CPDFPage pdfPage = pdfDoc.PageAtIndex(snapData.PageIndex);
                        double zoom = pdfViewer.GetZoom();
                        Rect snapRect = snapData.SelectRect;
                        DrawMode drawMode = pdfViewer.GetDrawModes();
                        uint bgColor = 0xFFFFFFFF;
                        int flag = 1;
                        switch (drawMode)
                        {
                            case DrawMode.Soft:
                                bgColor = 0xFFFFEFB2;
                                break;
                            case DrawMode.Green:
                                bgColor = 0xFFCBE9CE;
                                break;
                            case DrawMode.Dark:
                                bgColor = 0xFF000000;
                                flag = flag | 0x08 | 0x10000;
                                break;
                            case DrawMode.Normal:
                                bgColor = 0xFFFFFFFF;
                                break;
                            case DrawMode.Custom:
                                bgColor = pdfViewer.GetPDFBackground();
                                break;
                            default:
                                break;
                        }
                        byte[] imageData = new byte[(int)snapRect.Width * (int)snapRect.Height * 4];
                        pdfPage.RenderPageBitmapWithMatrix((float)(zoom * 96D / 72D), new CRect(
                            (float)snapRect.Left,
                            (float)snapRect.Bottom,
                            (float)snapRect.Right,
                            (float)snapRect.Top),
                            bgColor,
                            imageData,
                            flag,
                            true);
                        WriteableBitmap snapBitmap = new WriteableBitmap((int)snapRect.Width, (int)snapRect.Height, 96, 96, PixelFormats.Bgra32, null);
                        snapBitmap.WritePixels(new Int32Rect(0, 0, (int)snapRect.Width, (int)snapRect.Height), imageData, snapBitmap.BackBufferStride, 0);
                        return snapBitmap;
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return null;
        }

        public Rect GetSnapshotPDFRect(out int pageIndex)
        {
            pageIndex = -1;
            if (Viewer != null && Viewer.SnapshotData != null)
            {
                PageSelectedData snapData = Viewer.SnapshotData;

                pageIndex = snapData.PageIndex;
                return snapData.RawRect;
            }

            return Rect.Empty;
        }
    }
}
