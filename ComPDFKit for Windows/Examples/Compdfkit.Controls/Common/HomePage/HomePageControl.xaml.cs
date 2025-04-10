using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ComPDFKit.Controls.Compress;
using ComPDFKit.Controls.Helper;
using Path = System.Windows.Shapes.Path;

namespace ComPDFKit.Controls.PDFControl
{
    /// <summary>
    /// Interaction logic for HomePageControl.xaml
    /// </summary>
    public partial class HomePageControl : UserControl
    {
        private const string AnnotationFileName = @"TestFile\ComPDFKit_Annotations_Sample_File.pdf";
        private const string FormFileName = @"TestFile\ComPDFKit_Forms_Sample_File.pdf";
        private const string SignatureFileName = @"TestFile\ComPDFKit_Signatures_Sample_File.pdf";
        private const string MeasurementFileName = @"TestFile\ComPDFKit_Measurement_Sample_File.pdf";
        private const string SampleFileName = @"TestFile\ComPDFKit_Sample_File_Windows.pdf";
        private const string WatermarkFileName = @"TestFile\ComPDFKit_Watermark_Sample_File.pdf";
        private const string PasswordFileName = @"TestFile\Password_compdfkit_Security_Sample_File.pdf";

        private readonly Canvas viewerCanvas = new Canvas();
        private readonly Canvas annotationsCanvas = new Canvas();
        private readonly Canvas formsCanvas = new Canvas();
        private readonly Canvas signatureCanvas = new Canvas();
        private readonly Canvas documentEditorCanvas = new Canvas();
        private readonly Canvas contentEditorCanvas = new Canvas();
        private readonly Canvas securityCanvas = new Canvas();
        private readonly Canvas redactionCanvas = new Canvas();
        private readonly Canvas watermarkCanvas = new Canvas();
        private readonly Canvas compareDocumentsCanvas = new Canvas();
        private readonly Canvas conversionCanvas = new Canvas();
        private readonly Canvas compressCanvas = new Canvas();
        private readonly Canvas measurementCanvas = new Canvas();

        private List<CustomItem> customItems;

        /// <summary>
        /// Open file event. 
        /// param: Whether to create a file
        /// </summary>
        public event EventHandler<OpenFileEventArgs> OpenFileEvent;

        public HomePageControl()
        {
            InitializeComponent();
            RecentFilesControl.OpenFileEvent += (sender, args) =>
            {
                OpenFileEvent?.Invoke(this, args);
            };

            InitFeatureList();
        }

        private void InitFeatureList()
        {
            CreatFeatureIcon();

            CreateCustomItems();

            ImportFeatures();

            UseTheScrollViewerScrolling(FeaturesListControl);
        }

        private void ImportFeatures()
        {
            if (FeaturesListControl != null)
            {
                foreach (CustomItem item in customItems)
                {
                    FeaturesListControl.Items.Add(item);
                }
            }
            FeaturesListControl.SelectionChanged -= FeaturesListControl_SelectionChanged;
            FeaturesListControl.SelectionChanged += FeaturesListControl_SelectionChanged;
        }

        private void FeaturesListControl_SelectionChanged(object sender, CustomItem e)
        {
            var parentWindow = Window.GetWindow(this);

            switch (e.Feature)
            {
                case "Viewer":
                    {
                        OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.OpenFileDirectly, SampleFileName,  e.Feature));
                        break;
                    }
                case "Annotations":
                    {
                        OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.OpenFileDirectly, AnnotationFileName,  e.Feature));
                        break;
                    }
                case "Forms":
                    {
                        OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.OpenFileDirectly, FormFileName, e.Feature));
                        break;
                    }
                case "Signatures":
                    {
                        OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.OpenFileDirectly, SignatureFileName, e.Feature));
                        break;
                    }
                case "Document Editor":
                    {
                        OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.OpenFileDirectly, SampleFileName , e.Feature));
                        break;
                    }
                case "Content Editor":
                    {
                        OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.OpenFileDirectly, SampleFileName, e.Feature));
                        break;
                    }
                case "Watermark":
                    {
                        WatermarkOperationTypeDialog watermarkOperationTypeDialog = new WatermarkOperationTypeDialog()
                        {
                            Owner = parentWindow
                        };
                        watermarkOperationTypeDialog.ShowDialog();
                        break;
                    }
                case "Security":
                    {
                        SecurityOperationTypeDialog securityOperationTypeDialog = new SecurityOperationTypeDialog()
                        {
                            Owner = parentWindow
                        };
                        securityOperationTypeDialog.ShowDialog();
                        break;
                    }
                case "Redaction":
                    {
                        System.Diagnostics.Process.Start("https://www.compdf.com/pdf-sdk/security");
                        break;
                    }
                case "Compare Documents":
                    {
                        OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.OpenFileDirectly, SampleFileName, e.Feature));
                        break;
                    }
                case "Conversion":
                    {
                        System.Diagnostics.Process.Start("https://www.compdf.com/conversion");
                        break;
                    }
                case "Compress":
                    {
                        CompressDialog compressDialog = new CompressDialog()
                        {
                            Owner = parentWindow
                        };
                        compressDialog.ShowDialog();
                        break;
                    }
                case "Measurement":
                    {
                        OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.OpenFileDirectly, MeasurementFileName, e.Feature));
                        break;
                    }

                default: 
                    break;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UseTheScrollViewerScrolling(FrameworkElement fElement)
        {
            fElement.PreviewMouseWheel += (sender, e) =>
              {
                  var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                  eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                  eventArg.Source = sender;
                  fElement.RaiseEvent(eventArg);
              };
        }

        private void CreateCustomItems()
        {
            customItems = new List<CustomItem>()
            {
                new CustomItem{ IconCanvas = compressCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_Compress"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_Compress"), Feature = "Compress"},
                new CustomItem{ IconCanvas = viewerCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_Viewer"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_Viewer"), Feature = "Viewer"},
                new CustomItem{ IconCanvas = annotationsCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_Annotations"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_Annotations"), Feature = "Annotations"},
                new CustomItem{ IconCanvas = formsCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_Forms"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_Forms"), Feature = "Forms"},
                new CustomItem{ IconCanvas = signatureCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_Signatures"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_Signatures"), Feature = "Signatures"},
                new CustomItem{ IconCanvas = documentEditorCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_DocEditor"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_DocEditor"), Feature = "Document Editor"},
                new CustomItem{ IconCanvas = contentEditorCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_ContentEditor"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_ContentEditor"), Feature = "Content Editor"},
                new CustomItem{ IconCanvas = watermarkCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_Watermark"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_Watermark"), Feature = "Watermark"},
                new CustomItem{ IconCanvas = securityCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_Security"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_Security"), Feature = "Security"},
                new CustomItem{ IconCanvas = redactionCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_Redaction"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_Redaction"), Feature = "Redaction"},
                new CustomItem{ IconCanvas = compareDocumentsCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_DocCompare"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_DocCompare"), Feature = "Compare Documents"},
                new CustomItem{ IconCanvas = conversionCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_Conversion"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_Conversion"), Feature = "Conversion"},
                new CustomItem{ IconCanvas = measurementCanvas,TitleText = LanguageHelper.CommonManager.GetString("Func_Measurement"), DescriptionText= LanguageHelper.CommonManager.GetString("FuncDetail_Measurement"), Feature = "Measurement"},
            };
        }

        private void CreatFeatureIcon()
        {
            //Viewer
            {
                Path path1 = new Path
                {
                    Data = Geometry.Parse("M13.2071 1.5H2.5V18.5H17.5V5.79289L13.2071 1.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80477EDE")),
                    Clip = new RectangleGeometry(new Rect(0, 0, 20, 20))
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M4.5 3.5H12.5L15.5 6.5V16.5H4.5L4.5 3.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477EDE")),
                };

                Path path3 = new Path
                {
                    Data = Geometry.Parse("M9.95833 12.75C11.6763 12.75 13.1387 11.2833 13.6862 10.6551C13.8279 10.4925 13.8279 10.2575 13.6862 10.0949C13.1387 9.46673 11.6763 8 9.95833 8C8.24035 8 6.778 9.46673 6.23047 10.0949C6.08878 10.2575 6.08878 10.4925 6.23047 10.6551C6.778 11.2833 8.24035 12.75 9.95833 12.75Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff"))
                };

                Path path4 = new Path
                {
                    Data = Geometry.Parse("M9.95833 11.3639C10.5049 11.3639 10.9479 10.9209 10.9479 10.3743C10.9479 9.82782 10.5049 9.38477 9.95833 9.38477C9.41181 9.38477 8.96875 9.82782 8.96875 10.3743C8.96875 10.9209 9.41181 11.3639 9.95833 11.3639Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477EDE"))
                };

                viewerCanvas.Children.Add(path1);
                viewerCanvas.Children.Add(path2);
                viewerCanvas.Children.Add(path3);
                viewerCanvas.Children.Add(path4);
            }

            // Annotations
            {
                Path path1 = new Path
                {
                    Data = Geometry.Parse("M11.2071 0.5H0.5V17.5H15.5V4.79289L11.2071 0.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#806675FE")),
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M2.5 2.5H10.5L13.5 5.5V15.5H2.5L2.5 2.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6675FE")),
                };

                Path path3 = new Path
                {
                    Data = Geometry.Parse("M8.81755 12.6459C8.71612 12.559 8.56646 12.559 8.46503 12.6459L8.32746 12.7639C8.02318 13.0247 7.57419 13.0247 7.26991 12.7639L7.13234 12.6459C7.03091 12.559 6.88125 12.559 6.77982 12.6459L6.59593 12.8036C6.32336 13.0372 5.92977 13.0643 5.62778 12.8701L5.12441 12.5465C4.99859 12.4656 4.96216 12.2981 5.04305 12.1722C5.12393 12.0464 5.2915 12.01 5.41733 12.0909L5.9207 12.4145C6.02136 12.4792 6.15256 12.4702 6.24341 12.3923L6.4273 12.2347C6.73158 11.9739 7.18058 11.9739 7.48486 12.2347L7.62243 12.3526C7.72385 12.4395 7.87352 12.4395 7.97494 12.3526L8.11251 12.2347C8.41679 11.9739 8.86579 11.9739 9.17006 12.2347L9.35396 12.3923C9.44481 12.4702 9.57601 12.4792 9.67667 12.4145L10.18 12.0909C10.3059 12.01 10.4734 12.0464 10.5543 12.1722C10.6352 12.2981 10.5988 12.4656 10.473 12.5465L9.96959 12.8701C9.6676 13.0643 9.27401 13.0372 9.00144 12.8036L8.81755 12.6459Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff"))
                };

                Path path4 = new Path
                {
                    Data = Geometry.Parse("M10.5066 6V7.15097H8.50593V11.0556H7.09957V7.15097H5.08984V6H10.5066Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff"))
                };

                annotationsCanvas.Children.Add(path1);
                annotationsCanvas.Children.Add(path2);
                annotationsCanvas.Children.Add(path3);
                annotationsCanvas.Children.Add(path4);
            }

            // Forms
            {
                Path path1 = new Path
                {
                    Data = Geometry.Parse("M6.12502 4.25H1.90628V6.76817H6.12502V4.25ZM0.500028 3.55233V1.19767C0.500028 0.81236 0.812387 0.5 1.1977 0.5H14.8024C15.1877 0.5 15.5 0.81236 15.5 1.19767V3.55233V14.8023C15.5 15.1876 15.1877 15.5 14.8024 15.5H1.1977C0.812388 15.5 0.500028 15.1876 0.500028 14.8023V8.17442H0.5V6.76817H0.500028V3.55233ZM7.53127 4.25H14.0938V6.76817H7.53127V4.25ZM1.90628 8.17442H6.12502V14.0938H1.90628V8.17442ZM7.53127 14.0938H14.0938V8.17442H7.53127V14.0938ZM13.2437 9.73906C13.3232 9.81854 13.3232 9.94741 13.2437 10.0269L10.7112 12.5594C10.4552 12.8154 10.0403 12.8154 9.78432 12.5594L8.37532 11.1504C8.29584 11.071 8.29584 10.9421 8.37532 10.8626L8.74955 10.4884C8.82903 10.4089 8.9579 10.4089 9.03738 10.4884L10.2478 11.6988L12.5817 9.36484C12.6612 9.28535 12.79 9.28535 12.8695 9.36484L13.2437 9.73906Z"),
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff")),
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M6.12502 4.25H1.90628V6.76817H6.12502V4.25ZM0.500028 3.55233V1.19767C0.500028 0.81236 0.812387 0.5 1.1977 0.5H14.8024C15.1877 0.5 15.5 0.81236 15.5 1.19767V3.55233V14.8023C15.5 15.1876 15.1877 15.5 14.8024 15.5H1.1977C0.812388 15.5 0.500028 15.1876 0.500028 14.8023V8.17442H0.5V6.76817H0.500028V3.55233ZM7.53127 4.25H14.0938V6.76817H7.53127V4.25ZM1.90628 8.17442H6.12502V14.0938H1.90628V8.17442ZM7.53127 14.0938H14.0938V8.17442H7.53127V14.0938ZM13.2437 9.73906C13.3232 9.81854 13.3232 9.94741 13.2437 10.0269L10.7112 12.5594C10.4552 12.8154 10.0403 12.8154 9.78432 12.5594L8.37532 11.1504C8.29584 11.071 8.29584 10.9421 8.37532 10.8626L8.74955 10.4884C8.82903 10.4089 8.9579 10.4089 9.03738 10.4884L10.2478 11.6988L12.5817 9.36484C12.6612 9.28535 12.79 9.28535 12.8695 9.36484L13.2437 9.73906Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477EDE")),
                };
                formsCanvas.Children.Add(path1);
                formsCanvas.Children.Add(path2);
            }

            // Signature
            {
                Path path1 = new Path
                {
                    Data = Geometry.Parse("M11.4771 3.16735C11.6151 2.96809 11.8998 2.94242 12.0712 3.11378L15.2582 6.30084C15.4296 6.4722 15.4039 6.75692 15.2047 6.89487L12.0633 9.06966C12.0332 9.09048 12.0114 9.12124 12.0018 9.15653L11.2248 12.0057C11.2059 12.0747 11.143 12.123 11.0717 12.1277C10.7929 12.1458 10.0064 12.2356 8.44611 12.6725C7.06518 13.0592 5.39632 13.8125 4.47439 14.2527C4.61684 14.5428 4.56744 14.9032 4.32618 15.1445C4.0228 15.4479 3.53092 15.4479 3.22754 15.1445C2.92415 14.8411 2.92415 14.3492 3.22754 14.0458C3.46379 13.8096 3.81435 13.7573 4.10119 13.889C4.57233 12.8495 5.42026 10.9231 5.69949 9.92588C6.02875 8.74998 6.19116 7.62277 6.23578 7.28285C6.24404 7.21992 6.28895 7.16832 6.35019 7.15162L9.21546 6.37018C9.25075 6.36056 9.28151 6.33879 9.30233 6.30872L11.4771 3.16735ZM14.7596 14.6595C14.7015 14.6633 14.6428 14.6651 14.5837 14.6651C14.5253 14.6651 14.4673 14.6633 14.4098 14.6597L13.209 16.7396C13.148 16.8452 12.9948 16.8427 12.9374 16.7352L12.4679 15.8567C12.44 15.8045 12.3849 15.7726 12.3258 15.7746L11.3303 15.8073C11.2084 15.8113 11.1297 15.6799 11.1906 15.5743L12.372 13.5281C12.0527 13.0824 11.8647 12.5362 11.8647 11.9461C11.8647 10.4445 13.082 9.22712 14.5837 9.22712C16.0854 9.22712 17.3027 10.4445 17.3027 11.9461C17.3027 12.5355 17.1152 13.0811 16.7966 13.5264L17.9789 15.5743C18.0399 15.6799 17.9611 15.8113 17.8393 15.8073L16.8437 15.7746C16.7846 15.7726 16.7295 15.8045 16.7016 15.8567L16.2322 16.7352C16.1747 16.8427 16.0215 16.8452 15.9606 16.7396L14.7596 14.6595Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6675FE")),
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M8.66951 8.63805C8.69551 8.59472 8.75532 8.58735 8.79106 8.62309L9.74899 9.58102C9.78473 9.61676 9.77737 9.67657 9.73403 9.70257L7.33919 11.1395C7.26984 11.1811 7.191 11.1022 7.23261 11.0329L8.66951 8.63805Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff")),
                };

                signatureCanvas.Children.Add(path1);
                signatureCanvas.Children.Add(path2);
            }

            // Document Editor
            {
                Path path1 = new Path
                {
                    Data = Geometry.Parse("M8.6 17.5H15.5V9.6H8.6V17.5ZM15.5 8.4V0.5H8.6V8.4H15.5ZM7.4 17.5V9.6H0.5V17.5H7.4ZM0.5 8.4H7.4V0.5H0.5V8.4Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477EDE")),
                };
                documentEditorCanvas.Children.Add(path1);
            }

            // Content Editor
            {
                Path path1 = new Path
                {
                    Data = Geometry.Parse("M4.13281 15.8681V4.13281H15.8681V15.8681H4.13281Z"),
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F08E26")),
                    StrokeThickness = 1.5
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M6.42546 6.0293C6.33775 6.0293 6.26664 6.1004 6.26664 6.18812V6.7528C6.26664 6.84051 6.33775 6.91162 6.42546 6.91162H8.83596C8.92368 6.91162 8.99478 6.84051 8.99478 6.7528V6.18812C8.99478 6.1004 8.92368 6.0293 8.83596 6.0293H6.42546ZM10.1456 6.02938C10.0579 6.02938 9.98679 6.10049 9.98679 6.1882V8.51753C9.98679 8.60524 10.0579 8.67635 10.1456 8.67635H13.5482C13.6359 8.67635 13.707 8.60524 13.707 8.51753V6.1882C13.707 6.10049 13.6359 6.02938 13.5482 6.02938H10.1456ZM6.2666 7.95299C6.2666 7.86527 6.33771 7.79416 6.42543 7.79416H8.83592C8.92364 7.79416 8.99475 7.86527 8.99475 7.95299V8.51766C8.99475 8.60538 8.92364 8.67649 8.83592 8.67649H6.42543C6.33771 8.67649 6.2666 8.60538 6.2666 8.51766V7.95299ZM6.42543 9.55876C6.33771 9.55876 6.2666 9.62987 6.2666 9.71759V10.2823C6.2666 10.37 6.33771 10.4411 6.42542 10.4411H13.5482C13.6359 10.4411 13.707 10.37 13.707 10.2823V9.71759C13.707 9.62987 13.6359 9.55876 13.5482 9.55876H6.42543ZM6.2666 11.4822C6.2666 11.3945 6.33771 11.3234 6.42543 11.3234H13.5482C13.6359 11.3234 13.707 11.3945 13.707 11.4822V12.0469C13.707 12.1346 13.6359 12.2057 13.5482 12.2057H6.42542C6.33771 12.2057 6.2666 12.1346 6.2666 12.0469V11.4822ZM6.42543 13.0881C6.33771 13.0881 6.2666 13.1593 6.2666 13.247V13.8116C6.2666 13.8994 6.33771 13.9705 6.42542 13.9705H11.3161C11.4038 13.9705 11.4749 13.8994 11.4749 13.8116V13.247C11.4749 13.1593 11.4038 13.0881 11.3161 13.0881H6.42543Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F08E26")),
                };


                for (int i = 0; i < 4; i++)
                {
                    Rectangle rectangle = new Rectangle
                    {
                        Width = 2.64706,
                        Height = 2.64706,
                        RadiusX = 0.441176,
                        RadiusY = 0.441176,
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F08E26")),
                        Opacity = 0.5
                    };

                    switch (i)
                    {
                        case 0:
                            Canvas.SetLeft(rectangle, 2.5);
                            Canvas.SetTop(rectangle, 2.5);
                            break;
                        case 1:
                            Canvas.SetLeft(rectangle, 14.8525);
                            Canvas.SetTop(rectangle, 2.5);
                            break;
                        case 2:
                            Canvas.SetLeft(rectangle, 2.5);
                            Canvas.SetTop(rectangle, 14.8535);
                            break;
                        case 3:
                            Canvas.SetLeft(rectangle, 14.8525);
                            Canvas.SetTop(rectangle, 14.8535);
                            break;
                    }
                    contentEditorCanvas.Children.Add(rectangle);
                }
                contentEditorCanvas.Children.Add(path1);
                contentEditorCanvas.Children.Add(path2);
            }

            // Watermark
            {
                Path path1 = new Path
                {
                    Data = Geometry.Parse("M13.2071 1.5H2.5V18.5H17.5V5.79289L13.2071 1.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6675FE")),
                    Opacity = 0.5,
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M4.5 3.5H12.5L15.5 6.5V16.5H4.5L4.5 3.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6675FE")),
                };

                Path path3 = new Path
                {
                    Data = Geometry.Parse("M11.168 12.1729C10.8242 12.7637 10.2119 13.0967 9.43848 13.0967C8.32666 13.0967 7.50488 12.4199 7.50488 11.3618V11.3511C7.50488 10.3091 8.28906 9.70215 9.68018 9.61621L11.2593 9.5249V9.00928C11.2593 8.37012 10.8564 8.01562 10.0669 8.01562C9.42236 8.01562 8.99805 8.25195 8.8584 8.66553L8.85303 8.68701H7.73047L7.73584 8.64941C7.87549 7.69336 8.79395 7.0542 10.1206 7.0542C11.5869 7.0542 12.4141 7.78467 12.4141 9.00928V13H11.2593V12.1729H11.168ZM8.66504 11.3135C8.66504 11.8506 9.12158 12.1675 9.75 12.1675C10.6201 12.1675 11.2593 11.5981 11.2593 10.8462V10.3467L9.83594 10.438C9.03027 10.4863 8.66504 10.7764 8.66504 11.3027V11.3135Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff"))
                };
                watermarkCanvas.Children.Add(path1);
                watermarkCanvas.Children.Add(path2);
                watermarkCanvas.Children.Add(path3);
            }

            // Security
            {
                Path path1 = new Path
                {
                    Data = Geometry.Parse("M10.3071 0.225716L9.99547 0L9.68692 0.230128C8.51956 1.1008 7.30957 1.81198 6.05658 2.36432C4.80991 2.91388 3.61227 3.26584 2.46352 3.42184L2 3.48479V9.32316C2 12.3349 3.41409 15.1613 5.79677 16.9119L10 20L14.2032 16.9119C16.5859 15.1613 18 12.3349 18 9.32316V3.49464L17.5473 3.42343C16.3019 3.22754 15.101 2.8746 13.9434 2.36432C12.7803 1.85161 11.5679 1.139 10.3071 0.225716Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477EDE")),
                    Opacity = 0.5,
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M10.2303 2.66929L9.9966 2.5L9.76519 2.6726C8.88967 3.3256 7.98218 3.85898 7.04243 4.27324C6.10743 4.68541 5.20921 4.94938 4.34764 5.06638L4 5.11359V9.49237C4 11.7512 5.06057 13.871 6.84758 15.1839L10 17.5L13.1524 15.1839C14.9394 13.871 16 11.7512 16 9.49237V5.12098L15.6605 5.06757C14.7265 4.92065 13.8257 4.65595 12.9576 4.27324C12.0852 3.88871 11.1759 3.35425 10.2303 2.66929Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477EDE")),
                };

                securityCanvas.Children.Add(path1);
                securityCanvas.Children.Add(path2);

            }

            //Redaction
            {
                Path path1 = new Path
                {
                    Data = Geometry.Parse("M13.2071 1.5H2.5V18.5H17.5V5.79289L13.2071 1.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6675FE")),
                    Opacity = 0.5,
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M4.5 3.5H12.5L15.5 6.5V16.5H4.5L4.5 3.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6675FE")),
                };

                Rectangle rectangle = new Rectangle
                {
                    Width = 10,
                    Height = 3,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff")),
                };
                Canvas.SetLeft(rectangle, 5);
                Canvas.SetTop(rectangle, 8);
                redactionCanvas.Children.Add(path1);
                redactionCanvas.Children.Add(path2);
                redactionCanvas.Children.Add(rectangle);
            }

            // Compare Documents
            {

                Path path1 = new Path
                {
                    Data = Geometry.Parse("M0 0.5H7L8.5 2.10714L10 3.71429V15.5H0V0.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477EDE")),
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M4.05861 11L2.28516 5.36324H3.92189L4.94924 9.47655H5.02346L6.0469 5.36324H7.68363L5.91018 11H4.05861Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff")),
                };

                Path path3 = new Path
                {
                    Data = Geometry.Parse("M10 0.5H17L18.5 2.10714L20 3.71429V15.5H10V0.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477EDE")),
                };

                Path path4 = new Path
                {
                    Data = Geometry.Parse("M14.8291 11.1523C13.4229 11.1523 12.54 10.5117 12.4814 9.46483L12.4775 9.39452H13.8447L13.8486 9.42968C13.8877 9.77734 14.2744 10.0273 14.8526 10.0273C15.4112 10.0273 15.7783 9.77343 15.7783 9.41405V9.41015C15.7783 9.09374 15.5283 8.9453 14.8682 8.81639L14.2861 8.70311C13.0947 8.47264 12.5791 7.91014 12.5791 7.03122V7.02732C12.583 5.92184 13.5205 5.2109 14.8174 5.2109C16.2315 5.2109 17.001 5.92575 17.0869 6.92185L17.0909 6.96872H15.7627L15.7549 6.92185C15.6963 6.57028 15.3447 6.33591 14.8252 6.33591C14.3096 6.33591 14.0049 6.56638 14.0049 6.90232V6.90622C14.0049 7.22654 14.2627 7.40232 14.8721 7.52342L15.4541 7.6367C16.6533 7.87107 17.2002 8.35155 17.2002 9.26171V9.26561C17.2002 10.418 16.3135 11.1523 14.8291 11.1523Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff")),
                };

                compareDocumentsCanvas.Children.Add(path1);
                compareDocumentsCanvas.Children.Add(path2);
                compareDocumentsCanvas.Children.Add(path3);
                compareDocumentsCanvas.Children.Add(path4);
            }

            // Conversion
            {

                Path path1 = new Path
                {
                    Data = Geometry.Parse("M17.0999 12.5541V1.40002H3.50348V2.60002H15.8999L15.8999 12.5482L14.4238 11.0753L13.5762 11.9248L16.5069 14.8486L19.4248 11.9238L18.5752 11.0763L17.0999 12.5541ZM4.1028 17.4001L4.10331 7.45219L5.57574 8.92429L6.42427 8.07576L3.50001 5.1515L0.575745 8.07576L1.42427 8.92429L2.90338 7.44552V18.6001H16.4998V17.4001H4.1028Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477EDE")),
                    Opacity = 0.5,
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M11.6667 5H6V15H14V6.75L11.6667 5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477EDE")),
                };

                conversionCanvas.Children.Add(path1);
                conversionCanvas.Children.Add(path2);
            }

            // Compress
            {

                Path path1 = new Path
                {
                    Data = Geometry.Parse("M12.5 1.5H2.5V18.5H17.5V6.5H12.5V1.5Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F08E26")),
                };

                Path path2 = new Path
                {
                    Data = Geometry.Parse("M13.5 1.20703L17.793 5.50003H13.5V1.20703Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F08E26")),
                };

                Path path3 = new Path
                {
                    Data = Geometry.Parse("M6.90456 5.5H8.30912V6.4364H6.90456V5.5ZM5.5 6.43652H6.90456V7.37292H5.5V6.43652ZM8.3091 7.37264V8.30902H6.90454V7.37262L8.3091 7.37264ZM6.90456 8.30912V9.2455H5.5V8.3091L6.90456 8.30912ZM8.3091 9.24569V10.1821H6.90454V9.24567L8.3091 9.24569ZM6.90456 10.1817V11.1181H5.5V10.1817H6.90456ZM6.90456 12.0546H5.5V15.8001H8.30912V11.1183H6.90456V12.0546ZM5.9682 14.8637V13.9273H7.84094V14.8637H5.9682Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff")),
                };

                compressCanvas.Children.Add(path1);
                compressCanvas.Children.Add(path2);
                compressCanvas.Children.Add(path3);
            }

            // Measurement
            {

                Path path1 = new Path
                {
                    Data = Geometry.Parse("M15.1176 11.6606V15.1176H11.6606L15.1176 11.6606ZM15.1176 9.53924L9.53927 15.1176H5.88235L15.1176 5.88232V9.53924ZM13.9963 4.88232L4.88235 13.9963V10.8959L10.8781 4.90008C10.8838 4.89446 10.8889 4.88852 10.8935 4.88232H13.9963ZM8.77457 4.88232H4.88235V8.77454L8.77457 4.88232ZM3.38235 3.8235C3.38235 3.57985 3.57988 3.38232 3.82353 3.38232H16.1765C16.4201 3.38232 16.6176 3.57985 16.6176 3.8235V16.1764C16.6176 16.4201 16.4201 16.6176 16.1765 16.6176H3.82353C3.57988 16.6176 3.38235 16.4201 3.38235 16.1764V3.8235Z"),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00CC92")),
                };

                for (int i = 0; i < 4; i++)
                {
                    Rectangle rectangle = new Rectangle
                    {
                        Width = 2.64706,
                        Height = 2.64706,
                        RadiusX = 0.441176,
                        RadiusY = 0.441176,
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00CC92")),
                        Opacity = 0.5
                    };

                    switch (i)
                    {
                        case 0:
                            Canvas.SetLeft(rectangle, 2.5);
                            Canvas.SetTop(rectangle, 2.5);
                            break;
                        case 1:
                            Canvas.SetLeft(rectangle, 14.8525);
                            Canvas.SetTop(rectangle, 2.5);
                            break;
                        case 2:
                            Canvas.SetLeft(rectangle, 2.5);
                            Canvas.SetTop(rectangle, 14.8535);
                            break;
                        case 3:
                            Canvas.SetLeft(rectangle, 14.8525);
                            Canvas.SetTop(rectangle, 14.8535);
                            break;
                    }
                    measurementCanvas.Children.Add(rectangle);
                }
                measurementCanvas.Children.Add(path1);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void CreateDocument_Click(object sender, RoutedEventArgs e)
        {
            OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.CreateNewFile));
        }

        private void OpenDocument_Click(object sender, RoutedEventArgs e)
        {
            OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.OpenFileDialog));
        }
    }

    public class OpenFileEventArgs : EventArgs
    {
        public string FilePath { get; set; } 

        public FileOperationType OperationType { get; set; }

        public string FeatureName { get; set; } = string.Empty;

        public OpenFileEventArgs(FileOperationType operationType, string filePath = "", string featureName = "")
        {
            OperationType = operationType;
            FilePath = filePath;
            FeatureName = featureName;
        }
    }

    public enum FileOperationType
    {
        CreateNewFile,
        OpenFileDialog,
        OpenFileDirectly
    }

}
