using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using ComPDFKit.Controls.DigitalSignature.CPDFSignatureListControl;
using ComPDFKit.Controls.Helper;
using ComPDFKit.DigitalSign;
using System.Collections.Generic;
using System.Linq;

namespace ComPDFKit.Controls.PDFControl
{
    public enum BOTATools
    {
        Bookmark,
        Outline,
        Thumbnail,
        Annotation,
        Search,
        Signature,
    }

    public partial class CPDFBOTABarControl : UserControl
    {
        private PDFViewControl pdfViewer;

        private ToggleButton bookmarkButton;
        private ToggleButton outlineButton;
        private ToggleButton thumbnailButton;
        private ToggleButton annotButton;
        private ToggleButton searchButton;
        private CPDFSearchControl searchControl;
        private ToggleButton signatureButton;
        ToggleButton checkBtn;

        public bool ReplaceFunctionEnabled
        {
            get;
            set; 
        }

        private Dictionary<BOTATools,ToggleButton> botaToolBtnDic;
        public event EventHandler DeleteSignatureEvent;
        public event EventHandler<CPDFSignature> ViewCertificateEvent; 
        public event EventHandler<CPDFSignature> ViewSignatureEvent;
        public CPDFBOTABarControl()
        {
            InitializeComponent();
            InitBOTAButtons();
        }

        public void InitWithPDFViewer(PDFViewControl pdfViewer)
        {
            this.pdfViewer = pdfViewer;
            UIElement currentBotaTool = GetBotaTool();
            if (currentBotaTool is CPDFSearchControl)
            {
                ((CPDFSearchControl)currentBotaTool).InitWithPDFViewer(pdfViewer);
            }

            if (currentBotaTool is CPDFThumbnailControl)
            {
                ((CPDFThumbnailControl)currentBotaTool).InitWithPDFViewer(pdfViewer);
                ((CPDFThumbnailControl)currentBotaTool).ThumbLoaded = false;
                ((CPDFThumbnailControl)currentBotaTool).LoadThumb();
            }

            if (currentBotaTool is CPDFBookmarkControl)
            {
                ((CPDFBookmarkControl)currentBotaTool).InitWithPDFViewer(pdfViewer);
                ((CPDFBookmarkControl)currentBotaTool).LoadBookmark();
            }

            if (currentBotaTool is CPDFOutlineControl)
            {
                ((CPDFOutlineControl)currentBotaTool).InitWithPDFViewer(pdfViewer);
            }

            if (currentBotaTool is CPDFAnnotationListControl)
            {
                CPDFAnnotationListControl annotListControl= currentBotaTool as CPDFAnnotationListControl;

                annotListControl.InitWithPDFViewer(pdfViewer);
                annotListControl.LoadAnnotationList();
            }
        }

        public void AddBOTAContent(BOTATools botaTools)
        {
            if (!botaToolBtnDic.TryGetValue(botaTools, out var value)) return;
            BOTABarTitleGrid.Children.Add(value);
        }
        
        public void AddBOTAContent(BOTATools[] botaTools)
        {
            BOTABarTitleGrid.Children.Clear();
            botaTools = botaTools.Distinct().ToArray();
            foreach (BOTATools tool in botaTools)
            {
                AddBOTAContent(tool);
            }
        }

        public void RemoveBOTAContent(BOTATools botaTools)
        {
            if (!botaToolBtnDic.TryGetValue(botaTools, out var value)) return;
            BOTABarTitleGrid.Children.Remove(value);
            if(checkBtn == value)
            {
                botaToolBtnDic[botaTools].IsChecked = false;
                if (BOTABarTitleGrid.Children.Count > 0)
                {
                    if (BOTABarTitleGrid.Children[0] is ToggleButton buttonTool)
                    {
                        SelectBotaTool(botaToolBtnDic.FirstOrDefault(x => x.Value == buttonTool).Key);
                    }
                }
                else
                {
                    SetBotaTool(null);
                    ExpandTool(false);
                }
                
            }
        }

        public void RemoveBOTAContent(BOTATools[] botaTools)
        {
            foreach (BOTATools tool in botaTools)
            {
                RemoveBOTAContent(tool);
            }
        }

        private void InitBOTAButtons()
        {
            var brush = (Brush)FindResource("btn.bg.bota");

            thumbnailButton = new ToggleButton();
            thumbnailButton.Background = brush;
            var thumbnailGeometry = Geometry.Parse("M1.6001 0.850098H0.850098V1.6001V12.8001V13.5501H1.6001H4.0001V13.7001V15.2001H5.5001H13.7001H15.2001V13.7001V5.5001V4.0001H13.7001H13.5501V1.6001V0.850098H12.8001H1.6001ZM12.0501 4.0001V2.3501H2.3501V12.0501H4.0001V5.5001V4.0001H5.5001H12.0501ZM5.5001 5.5001H13.7001V13.7001H5.5001V5.5001Z");
            thumbnailButton.Height = 40;
            thumbnailButton.Width = 52;
            thumbnailButton.BorderThickness = new Thickness(0);
            thumbnailButton.Style = FindResource("ToggleButtonStyle") as Style;
            thumbnailButton.ToolTip = LanguageHelper.BotaManager.GetString("Tooltip_Thumb");
            thumbnailButton.Content = new Path
            {
                Width = 16,
                Height = 16,
                Data = thumbnailGeometry,
                Fill = new SolidColorBrush(Color.FromRgb(0x27, 0x3C, 0x62))
            };
            thumbnailButton.Click += ThumbnailButton_Click;
            
            
            outlineButton = new ToggleButton();
            outlineButton.Background = brush;
            Geometry outlineGeometry = Geometry.Parse("M3 0.5H0V3.5H3V0.5ZM3 5.5H0V8.5H3V5.5ZM0 10.5H3V13.5H0V10.5ZM14 2.75V1.25H5V2.75H14ZM14 6.25V7.75H5V6.25H14ZM14 12.75V11.25H5V12.75H14Z");
            outlineButton.Height = 40;
            outlineButton.Width = 52;
            outlineButton.BorderThickness= new Thickness(0);
            outlineButton.Style = FindResource("ToggleButtonStyle") as Style;
            outlineButton.ToolTip = LanguageHelper.BotaManager.GetString("Tooltip_Outlines");
            outlineButton.Content = new Path
            {
                Width = 16,
                Height = 16,
                Data = outlineGeometry,
                Fill = new SolidColorBrush(Color.FromRgb(0x27, 0x3C, 0x62))
            };
            outlineButton.Click += OutlineButton_Click; 
            
            
            bookmarkButton = new ToggleButton();
            bookmarkButton.Background = brush;
            Geometry bookmarkGeometry = Geometry.Parse("M5.6221 9.85217L0.75 12.6942V0.75H11.25V12.6942L6.3779 9.85217L6 9.63172L5.6221 9.85217Z");
            bookmarkButton.Height = 40;
            bookmarkButton.Width = 52;
            bookmarkButton.BorderThickness = new Thickness(0);
            bookmarkButton.Style = FindResource("ToggleButtonStyle") as Style;
            bookmarkButton.ToolTip = LanguageHelper.BotaManager.GetString("Tooltip_Bookmarks");
            bookmarkButton.Content = new Path
            {
                Width = 16,
                Height = 16,
                Data = bookmarkGeometry,
                StrokeThickness = 1.5,
                Stroke = new SolidColorBrush(Color.FromRgb(0x27, 0x3C, 0x62))
            };
            bookmarkButton.Click += BookmarkButton_Click;
            annotButton = new ToggleButton();
            annotButton.Background = brush;
            Geometry annoGeometry = Geometry.Parse("M16 0H1.6L0 14H14.4L16 0ZM8.34885 1.8143L12.0693 11.1154H13.2692V12.1154H10.1923V11.1154H10.9922L10.2384 9.23077H5.53048L4.77689 11.1154H5.57692V12.1154H2.5V11.1154H3.69995L7.42038 1.8143H8.34885ZM7.884 3.345L9.83837 8.23077H5.93035L7.884 3.345Z");
            annotButton.Height = 40;
            annotButton.Width = 52;
            annotButton.BorderThickness = new Thickness(0);
            annotButton.Style = FindResource("ToggleButtonStyle") as Style;
            annotButton.ToolTip = LanguageHelper.BotaManager.GetString("Tooltip_Annot");
            annotButton.Content = new Path
            {
                Width = 16,
                Height = 16,
                Data = annoGeometry,
                Fill = new SolidColorBrush(Color.FromRgb(0x27, 0x3C, 0x62))
            };
            annotButton.Click += AnnotButton_Click;
            
            searchButton = new ToggleButton();
            searchButton.Background = brush;
            Geometry searchGeometry = Geometry.Parse("M3.4284 10.1635C1.56851 8.30364 1.56851 5.28816 3.4284 3.42827C5.28829 1.56838 8.30377 1.56838 10.1637 3.42827C12.0235 5.28816 12.0235 8.30364 10.1637 10.1635C8.30377 12.0234 5.28829 12.0234 3.4284 10.1635ZM2.36774 2.36761C-0.0779349 4.81329 -0.0779349 8.77851 2.36774 11.2242C4.63397 13.4904 8.20494 13.6567 10.6626 11.723L12.8875 13.9479C13.1804 14.2408 13.6552 14.2408 13.9481 13.9479C14.241 13.655 14.241 13.1801 13.9481 12.8872L11.7233 10.6624C13.6568 8.20466 13.4905 4.63379 11.2243 2.36761C8.77864 -0.078065 4.81342 -0.078065 2.36774 2.36761Z");
            searchButton.Height = 40;
            searchButton.Width = 52;
            searchButton.BorderThickness = new Thickness(0);
            searchButton.Style = FindResource("ToggleButtonStyle") as Style;
            searchButton.ToolTip = LanguageHelper.BotaManager.GetString("Tooltip_Search");
            searchButton.Content = new Path
            {
                Width = 16,
                Height = 16,
                Data = searchGeometry,
                Fill = new SolidColorBrush(Color.FromRgb(0x27, 0x3C, 0x62))
            };
            searchButton.Click += SearchButton_Click;
            
            
            signatureButton = new ToggleButton();
            signatureButton.Background = brush;
            Geometry signatureGeometry = Geometry.Parse("M13.7028 3.47125L10.2315 0L8.48761 1.7439L11.9589 5.21515L13.7028 3.47125ZM2.71046 7.52105L7.90907 2.32244L11.3803 5.79369L6.18171 10.9923L1.55338 12.1494L2.71046 7.52105ZM0.589844 13.9283L1.10936 15.3354L1.46319 15.2055L2.48296 14.8397L3.43876 14.5094L4.32885 14.2146L4.88485 14.0376L5.6615 13.8014L6.14025 13.6633L6.58719 13.5406L7.00179 13.4333L7.38354 13.3413L7.89341 13.2318L8.19073 13.1778C8.23741 13.17 8.28264 13.1629 8.32642 13.1564L8.57153 13.125L8.78117 13.1086C9.00487 13.0982 9.1551 13.1183 9.22814 13.1684C9.30366 13.2203 9.33349 13.2525 9.32936 13.2642L9.1248 13.7393C9.05684 13.9069 9.03233 14.0023 9.0131 14.1599C8.96315 14.5694 9.11269 14.9528 9.45819 15.2228C9.89718 15.5659 10.6693 15.5717 11.916 15.2623L12.3493 15.1475C12.4246 15.1264 12.5013 15.1044 12.5796 15.0814L13.0678 14.9319L13.5942 14.7595L14.1598 14.5644L14.7657 14.3467L15.4133 14.1067L14.8859 12.7025L14.5448 12.8295L13.8979 13.064L13.2982 13.2723L12.7457 13.4545L12.2409 13.6104L11.7837 13.7402C11.6393 13.7791 11.5029 13.8136 11.3745 13.8437L11.231 13.8761C11.1153 13.9011 11.0093 13.9216 10.9143 13.9386L10.6516 13.9818L10.7258 13.7931C10.8803 13.3576 10.8912 13.0188 10.6834 12.5983C10.5561 12.3407 10.3519 12.1204 10.077 11.9317C9.60934 11.6108 8.77614 11.5669 7.51741 11.7931L7.1057 11.8735L6.66529 11.9718C6.58947 11.9897 6.51244 12.0083 6.43418 12.0276L5.94992 12.1527L5.16759 12.3736L4.31689 12.6343L3.39641 12.9347L2.40472 13.2745L1.34042 13.6537L0.589844 13.9283Z");
            signatureButton.Height = 40;
            signatureButton.Width = 52;
            signatureButton.BorderThickness = new Thickness(0);
            signatureButton.Style = FindResource("ToggleButtonStyle") as Style;
            signatureButton.ToolTip = LanguageHelper.BotaManager.GetString("Tooltip_Sig");
            signatureButton.Content = new Path
            {
                Width = 16,
                Height = 16,
                Data = signatureGeometry,
                Fill = new SolidColorBrush(Color.FromRgb(0x27, 0x3C, 0x62))
            };
            signatureButton.Click += SignatureButton_Click;
            
            botaToolBtnDic = new Dictionary<BOTATools, ToggleButton>
            {
                { BOTATools.Thumbnail, thumbnailButton },
                { BOTATools.Outline, outlineButton },
                { BOTATools.Bookmark, bookmarkButton },
                { BOTATools.Search, searchButton },
                { BOTATools.Annotation, annotButton },
                { BOTATools.Signature, signatureButton }
            };
        }

        private UIElement GetBotaTool()
        {
            return BotaToolContainer.Child;
        }

        private void SetBotaTool(UIElement newChild)
        {
            BotaToolContainer.Child = newChild;
        }

        private void ExpandTool(bool isExpand)
        {
            BotaToolContainer.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ClearToolState(UIElement ignoreTool)
        {
            foreach (UIElement child in BOTABarTitleGrid.Children)
            {
                if (child != ignoreTool && child is ToggleButton buttonTool)
                {
                    buttonTool.IsChecked = false;
                }
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
           SelectBotaTool(BOTATools.Search);
        }

        private void AnnotButton_Click(object sender, RoutedEventArgs e)
        {
            SelectBotaTool(BOTATools.Annotation);
        }

        private void ThumbnailButton_Click(object sender, RoutedEventArgs e)
        {
            SelectBotaTool(BOTATools.Thumbnail);
        }

        private void OutlineButton_Click(object sender, RoutedEventArgs e)
        {
            SelectBotaTool(BOTATools.Outline);
        }

        private void BookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectBotaTool(BOTATools.Bookmark);
        }
        
        
        private void SignatureButton_Click(object sender, RoutedEventArgs e)
        {
            SelectBotaTool(BOTATools.Signature);
        }

        public void SelectBotaTool(BOTATools tool)
        {
            UIElement botaTool = GetBotaTool();

            switch(tool)
            {
                case BOTATools.Thumbnail:
                    {
                        if(thumbnailButton != null)
                        {
                            thumbnailButton.IsChecked = true;
                            if (botaTool == null || !(botaTool is CPDFThumbnailControl))
                            {
                                CPDFThumbnailControl thumbnailControl = new CPDFThumbnailControl();

                                if (pdfViewer != null && pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument() != null)
                                {
                                    thumbnailControl.InitWithPDFViewer(pdfViewer);
                                    thumbnailControl.LoadThumb();
                                }
                                SetBotaTool(thumbnailControl);
                                checkBtn = thumbnailButton;
                            }
                        }
                    }
                    break;
                case BOTATools.Outline:
                    {
                        if(outlineButton != null)
                        {
                            outlineButton.IsChecked = true;
                            if (botaTool == null || !(botaTool is CPDFOutlineControl))
                            {
                                CPDFOutlineControl outlineControl = new CPDFOutlineControl();

                                if (pdfViewer != null && pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument() != null)
                                {
                                    outlineControl.InitWithPDFViewer(pdfViewer);
                                }
                                SetBotaTool(outlineControl);
                                checkBtn = outlineButton;
                            }
                        }
                    }
                    break;
                case BOTATools.Bookmark:
                    {
                        if(bookmarkButton!=null)
                        {
                            bookmarkButton.IsChecked = true;
                            if (botaTool == null || !(botaTool is CPDFBookmarkControl))
                            {
                                CPDFBookmarkControl pdfBookmarkControl = new CPDFBookmarkControl();
                                if (pdfViewer != null && pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument() != null)
                                {
                                    pdfBookmarkControl.InitWithPDFViewer(pdfViewer);
                                    pdfBookmarkControl.LoadBookmark(); 
                                }
                                SetBotaTool(pdfBookmarkControl);
                            }
                            checkBtn = bookmarkButton;
                        }
                    }
                    break;
                case BOTATools.Search:
                    {
                        if(searchButton!=null)
                        {
                            searchButton.IsChecked = true;
                            if (botaTool == null || !(botaTool is CPDFSearchControl))
                            {

                                if (searchControl == null)
                                {
                                    searchControl = new CPDFSearchControl();

                                    if (pdfViewer != null && pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument() != null)
                                    {
                                        searchControl.InitWithPDFViewer(pdfViewer);
                                    }
                                }
                                searchControl.ReplaceTog.Visibility = ReplaceFunctionEnabled ? Visibility.Visible : Visibility.Collapsed;
                                SetBotaTool(searchControl);
                                checkBtn = searchButton;
                            }
                        }
                    }
                    break;
                case BOTATools.Annotation:
                    {
                        if(annotButton!=null)
                        {
                            annotButton.IsChecked = true;
                            if (botaTool == null || !(botaTool is CPDFAnnotationListControl))
                            {
                                CPDFAnnotationListControl annotationListControl = new CPDFAnnotationListControl();

                                if (pdfViewer != null && pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument() != null)
                                {
                                    annotationListControl.InitWithPDFViewer(pdfViewer);
                                    annotationListControl.LoadAnnotationList();
                                }
                                SetBotaTool(annotationListControl);
                                checkBtn = annotButton;
                            }
                        }
                    }
                    break;
                case BOTATools.Signature:
                    {
                        if (signatureButton != null)
                        {
                            signatureButton.IsChecked = true;
                            if (botaTool == null || !(botaTool is CPDFSignatureListControl))
                            {
                                CPDFSignatureListControl signatureControl = new CPDFSignatureListControl();

                                if (pdfViewer != null && pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument() != null)
                                {
                                    signatureControl.InitWithPDFViewer(pdfViewer);
                                    signatureControl.LoadSignatureList();
                                }
                                SetBotaTool(signatureControl);
                                checkBtn = signatureButton;
                                signatureControl.DeleteSignatureEvent += (sender, args) =>
                                {
                                    DeleteSignatureEvent?.Invoke(this, null);
                                };
                                signatureControl.ViewCertificateEvent += (sender, args) =>
                                {
                                    ViewCertificateEvent?.Invoke(this, args);
                                };
                                signatureControl.ViewSignatureEvent += (sender, args) =>
                                {
                                    ViewSignatureEvent?.Invoke(this, args);
                                };
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            if(checkBtn!=null)
            {
                ExpandTool(checkBtn.IsChecked == true);
                ClearToolState(checkBtn);
            }
        }

        public void LoadAnnotationList()
        {
            UIElement currentBotaTool = GetBotaTool();
            if (currentBotaTool is CPDFAnnotationListControl control)
            {
                control.LoadAnnotationList();
            }
        }
        
        public void LoadSignatureList()
        {
            UIElement currentBotaTool = GetBotaTool();
            if (currentBotaTool is CPDFSignatureListControl control)
            {
                control.LoadSignatureList();
            }
        }

        private void CPDFBOTABarControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if(searchControl!=null && searchControl.ReplaceTog!=null)
            {
                searchControl.ReplaceTog.Visibility = ReplaceFunctionEnabled ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public void LoadThumbnail()
        {
            UIElement currentBotaTool = GetBotaTool();
            if (currentBotaTool is CPDFThumbnailControl)
            {
                ((CPDFThumbnailControl)currentBotaTool).LoadThumb();
            }
        }
    }
}
