using ComPDFKit.PDFDocument;
using ComPDFKit.PDFWatermark;
using Compdfkit_Tools.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace Compdfkit_Tools.PDFControl
{
    /// <summary>
    /// Interaction logic for WatermarkListDialog.xaml
    /// </summary>
    public partial class WatermarkListDialog : Window
    {
        private WatermarkData watermarkData;
        public WatermarkListDialog()
        {
            InitializeComponent();
            Title = LanguageHelper.SecurityManager.GetString("Title_AddWatermark");
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            WatermarkDialog watermarkDialog = new WatermarkDialog
            {
                Owner = this
            };

            watermarkDialog.WindowClosed += WatermarkDialog_WindowClosed;
            if (watermarkDialog.InitWithFileInfo(FileGridListWithPageRangeControl.FileForDisplay))
            {
                watermarkDialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("No file selected.");
            }
        }

        private void WatermarkDialog_WindowClosed(object sender, WatermarkData watermarkData)
        {
            if (watermarkData != null)
            {
                this.watermarkData = watermarkData;
                var dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                else
                {
                    var path = dialog.SelectedPath;
                    foreach (var fileInfo in FileGridListWithPageRangeControl.FileInfoDataList)
                    {
                        if (watermarkData.Type == C_Watermark_Type.WATERMARK_TYPE_TEXT)
                        {
                            UpdateByTextWatermarkData(fileInfo.Document, fileInfo.PageRangeList);
                        }
                        else if (watermarkData.Type == C_Watermark_Type.WATERMARK_TYPE_IMG)
                        {
                            UpdateByImageWatermarkData(fileInfo.Document, fileInfo.PageRangeList);
                        }
                        else
                        {
                            return;
                        }
                        string savePath = Path.Combine(path, Path.GetFileNameWithoutExtension(fileInfo.Document.FileName) + "_" 
                            + LanguageHelper.SecurityManager.GetString("FileName_Watermark")) + ".pdf";
                        fileInfo.Document.WriteToFilePath(savePath);
                    }
                    System.Diagnostics.Process.Start("explorer.exe", "/select," + path + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileGridListWithPageRangeControl.FileInfoDataList[0].Document.FileName) + "_Watermark.pdf");
                }
            }
            Close();
        }

        private void UpdateByImageWatermarkData(CPDFDocument document, List<int> pageRangeList)
        {
            CPDFWatermark watermark = document.InitWatermark(watermarkData.Type);
            Bitmap bitmap = new Bitmap(watermarkData.ImagePath);
            bitmap = CommonHelper.ConvertTo32bppArgb(bitmap);
            byte[] byteArray = CommonHelper.ConvertBitmapToByteArray(bitmap);
            watermark.SetImage(byteArray, bitmap.Width, bitmap.Height);
            watermark.SetScale((float)(watermarkData.ImageScale / 100.0));
            UpdateByCommonWatermarkData(watermark, pageRangeList);
        }

        private void UpdateByTextWatermarkData(CPDFDocument document, List<int> pageRangeList)
        {
            CPDFWatermark watermark = document.InitWatermark(watermarkData.Type);
            watermark.SetText(watermarkData.Text);
            watermark.SetFontName(watermarkData.FontName);
            watermark.SetTextRGBColor(watermarkData.Color);
            watermark.SetFontSize(watermarkData.FontSize);
            watermark.SetScale(1);
            UpdateByCommonWatermarkData(watermark, pageRangeList);
        }

        private void UpdateByCommonWatermarkData(CPDFWatermark watermark, List<int> pageRangeList)
        {
            for (int i = 0; i < pageRangeList.Count; i++)
            {
                pageRangeList[i]--;
            }
            watermark.SetPages(CommonHelper.GetPageParmFromList(pageRangeList));
            watermark.SetRotation((float)(watermarkData.Rotation * Math.PI / 180.0));
            watermark.SetOpacity(watermarkData.Opacity);

            watermark.SetVertalign((C_Watermark_Vertalign)(watermarkData.Align / 3));
            watermark.SetHorizalign((C_Watermark_Horizalign)(watermarkData.Align % 3));
 
            watermark.SetVertOffset(watermarkData.VertOffset);
            watermark.SetHorizOffset(watermarkData.HorizOffset);
            watermark.SetFront(watermarkData.IsFront);
            watermark.SetFullScreen(watermarkData.IsFullScreen);
            watermark.SetVerticalSpacing(watermarkData.VerticalSpacing);
            watermark.SetHorizontalSpacing(watermarkData.HorizontalSpacing);
            watermark.CreateWatermark();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var fileInfo in FileGridListWithPageRangeControl.FileInfoDataList)
            {
                fileInfo.Document.Release();
            }
            base.OnClosed(e);
        }
    }
}
