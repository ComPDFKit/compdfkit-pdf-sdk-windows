using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.PDFDocument;
using ComPDFKitViewer;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;
using ComPDFKit.Import;

namespace ComPDFKit.Controls.Annotation.PDFAnnotationUI
{
    public partial class CPDFLinkUI : UserControl, INotifyPropertyChanged
    {
        int totalPage = 0;
        int LinkPage = 0;

        private LinkParam linkParam;
        private CPDFLinkAnnotation linkAnnot;
        private PDFViewControl viewControl;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool _drawLink;
        public bool DrawLink
        {
            get
            {
                return _drawLink;
            }
            set
            {
                _drawLink = value;
                InputEnable = _drawLink;
            }
        }

        private string _pagePromptIndex;
        public string PagePromptText
        {
            get
            {
                return _pagePromptIndex;
            }
            set
            {
                _pagePromptIndex = value;
                OnPropertyChanged();
            }
        }

        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                CheckingItem(_selectedIndex);
                OnPropertyChanged();
            }
        }

        public bool InputEnable
        {
            get
            {
                if (linkAnnot != null)
                {
                    return true;
                }
                return DrawLink;
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public CPDFLinkUI()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetPresentAnnotAttrib(LinkParam param, CPDFLinkAnnotation annot, PDFViewControl view, int PageCount)
        {
            linkAnnot = annot;
            linkParam = param;
            viewControl = view;
            UrlText.Text = "";
            PageText.Text = "";
            EmailText.Text = "";
            SaveBtn.IsEnabled = true;
            totalPage = PageCount;
            PagePromptText = Helper.LanguageHelper.PropertyPanelManager.GetString("Holder_Jump") + totalPage;

            if (param.Action == C_ACTION_TYPE.ACTION_TYPE_GOTO)
            {
                PageText.Text = (param.DestinationPageIndex + 1).ToString();
                SelectedIndex = 1;
            }

            if (param.Action == C_ACTION_TYPE.ACTION_TYPE_URI)
            {
                if (param.Uri.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                {
                    EmailText.Text = param.Uri.ToLower().TrimStart("mailto:".ToCharArray());
                    SelectedIndex = 2;
                }
                else
                {
                    UrlText.Text = param.Uri;
                    SelectedIndex = 0;
                }
            }
        }

        public void InitLinkAnnotArgs(LinkParam param, int PageCount)
        {
            linkParam = param;
            InputEnable = true;
            //linkParam.LinkDrawFinished += LinkAnnot_LinkDrawFinished;
            totalPage = PageCount;
            PagePromptText = Helper.LanguageHelper.PropertyPanelManager.GetString("Holder_Jump") + totalPage;
        }

        private void LinkAnnot_LinkDrawFinished(object sender, bool e)
        {
            DrawLink = e;
            UrlText.Text = "";
            PageText.Text = "";
            EmailText.Text = "";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (linkParam != null)
            {
                switch (SelectedIndex)
                {
                    case 0:
                        linkParam.Action = C_ACTION_TYPE.ACTION_TYPE_URI;
                        string urlPath = UrlText.Text.Trim().ToLower();
                        if (urlPath.StartsWith("http://") || urlPath.StartsWith("https://"))
                        {
                            linkParam.Uri = urlPath;
                        }
                        else
                        {
                            linkParam.Uri = "http://" + urlPath;
                        }
                        break;
                    case 1:
                        linkParam.Action = C_ACTION_TYPE.ACTION_TYPE_GOTO;

                        linkParam.DestinationPageIndex = LinkPage - 1;
                        CSize pageSize = new CSize();
                        if (viewControl != null)
                        {
                            if (LinkPage - 1 > viewControl.GetCPDFViewer().GetDocument().PageCount || LinkPage - 1 < 0)
                            {
                                return;
                            }
                            pageSize = viewControl.GetCPDFViewer().GetDocument().PageAtIndex(LinkPage - 1).PageSize;
                        }

                        linkParam.DestinationPosition = new CPoint(0, pageSize.height);
                        break;
                    case 2:
                        linkParam.Action = C_ACTION_TYPE.ACTION_TYPE_URI;
                        linkParam.Uri = "mailto:" + EmailText.Text.Trim();
                        break;
                    default:
                        break;
                }

                if (viewControl != null)
                {
                    LinkAnnotHistory history = new LinkAnnotHistory();
                    history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, linkAnnot.Page.PageIndex, linkAnnot);
                    if (ParamConverter.SetParamForPDFAnnot(viewControl.GetCPDFViewer().GetDocument(), linkAnnot, linkParam))
                    {
                        viewControl.UpdateAnnotFrame();
                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, linkAnnot.Page.PageIndex, linkAnnot);
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                    };
                }
            }
            else
            {
                if (linkAnnot != null && linkAnnot.IsValid())
                {
                    switch (SelectedIndex)
                    {
                        case 0:
                            {
                                CPDFUriAction uriAction = new CPDFUriAction();
                                string urlPath = UrlText.Text.Trim().ToLower();
                                if (urlPath.StartsWith("http://") || urlPath.StartsWith("https://"))
                                {
                                    uriAction.SetUri(urlPath);
                                }
                                else
                                {
                                    uriAction.SetUri("http://" + UrlText.Text.Trim().ToLower());
                                }
                                linkAnnot.SetLinkAction(uriAction);
                            }

                            break;
                        case 1:
                            {
                                CPDFGoToAction gotoAction = new CPDFGoToAction();
                                CPDFDestination destination = new CPDFDestination();
                                destination.PageIndex = LinkPage - 1;
                                CPDFViewer pdfViewer = viewControl.PDFViewTool.GetCPDFViewer();
                                gotoAction.SetDestination(pdfViewer.GetDocument(), destination);
                                linkAnnot.SetLinkAction(gotoAction);
                            }
                            break;
                        case 2:
                            {
                                CPDFUriAction uriAction = new CPDFUriAction();
                                string urlPath = "mailto:" + EmailText.Text.Trim();
                                uriAction.SetUri(urlPath);
                                linkAnnot.SetLinkAction(uriAction);
                            }
                            break;
                        default:
                            break;
                    }
                }

            }
            DrawLink = false;
            SaveBtn.IsEnabled = false;
        }

        #region Data Verification
        private bool CheckPageNumVaild(out int pageNum, string text)
        {
            pageNum = -1;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            if (text.Trim() != string.Empty)
            {
                if (!int.TryParse(text.Trim(), out pageNum))
                {
                    return false;
                }
            }
            if (pageNum < 1 || pageNum > totalPage)
            {
                return false;
            }

            return true;
        }

        private bool CheckPageWebVaild(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            string checkUrl = text.ToLower().TrimStart("http://".ToCharArray()).TrimStart("https://".ToCharArray());
            if (!Regex.IsMatch(checkUrl, "([a-zA-Z0-9/\\-%\\?#&=]+[./\\-%\\?#&=]?)+"))
            {
                return false;
            }
            string matchText = Regex.Match(checkUrl, "([a-zA-Z0-9/\\-%\\?#&=]+[./\\-%\\?#&=]?)+").Value;
            if (matchText.Length != checkUrl.Length)
            {
                return false;
            }
            return true;
        }

        private bool CheckPageMailVaild(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            if (!Regex.IsMatch(text, "^[A-Za-z0-9\u4e00-\u9fa5_\\-\\.]+@[a-zA-Z0-9_-]+(\\.[a-zA-Z0-9_-]+)+$"))
            {
                return false;
            }
            return true;
        }

        private void UrlText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (linkParam != null)
            {
                if (CheckPageWebVaild((sender as TextBox).Text) && DrawLink)
                {
                    SaveBtn.IsEnabled = true;
                }
                else
                {
                    SaveBtn.IsEnabled = false;
                }
            }
            else
            {
                if (CheckPageWebVaild((sender as TextBox).Text))
                {
                    SaveBtn.IsEnabled = true;
                }
                else
                {
                    SaveBtn.IsEnabled = false;
                }
            }
        }

        private void PageText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (linkParam != null)
            {
                if (CheckPageNumVaild(out LinkPage, (sender as TextBox).Text) && DrawLink)
                {
                    SaveBtn.IsEnabled = true;
                }
                else
                {
                    SaveBtn.IsEnabled = false;
                }
            }
            else
            {
                if (CheckPageNumVaild(out LinkPage, (sender as TextBox).Text))
                {
                    SaveBtn.IsEnabled = true;
                }
                else
                {
                    SaveBtn.IsEnabled = false;
                }
            }
        }

        private void EmailText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (linkParam != null)
            {
                if (CheckPageMailVaild((sender as TextBox).Text) && DrawLink)
                {
                    SaveBtn.IsEnabled = true;
                }
                else
                {
                    SaveBtn.IsEnabled = false;
                }
            }
            else
            {
                if (CheckPageMailVaild((sender as TextBox).Text))
                {
                    SaveBtn.IsEnabled = true;
                }
                else
                {
                    SaveBtn.IsEnabled = false;
                }
            }
        }

        private void CheckingItem(int ItemIndex)
        {
            DrawLink = true;
            bool BtnIsEnabled = false;

            if (linkParam != null)
            {
                switch (ItemIndex)
                {
                    case 0:
                        BtnIsEnabled = CheckPageWebVaild(UrlText.Text) && DrawLink;
                        break;
                    case 1:
                        BtnIsEnabled = CheckPageNumVaild(out LinkPage, PageText.Text) && DrawLink;
                        break;
                    case 2:
                        BtnIsEnabled = CheckPageMailVaild(EmailText.Text) && DrawLink;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (ItemIndex)
                {
                    case 0:
                        BtnIsEnabled = CheckPageWebVaild(UrlText.Text);
                        break;
                    case 1:
                        BtnIsEnabled = CheckPageNumVaild(out LinkPage, PageText.Text);
                        break;
                    case 2:
                        BtnIsEnabled = CheckPageMailVaild(EmailText.Text);
                        break;
                    default:
                        break;
                }
            }
            SaveBtn.IsEnabled = BtnIsEnabled;
            SaveBtn.IsEnabled = true;
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Binding Indexbinding = new Binding();
            Indexbinding.Source = this;
            Indexbinding.Path = new PropertyPath("SelectedIndex");
            Indexbinding.Mode = BindingMode.TwoWay;
            HeadTabControl.SetBinding(TabControl.SelectedIndexProperty, Indexbinding);
        }

        private void PART_BtnClear_Click(object sender, RoutedEventArgs e)
        {
            switch (SelectedIndex)
            {
                case 0:
                    UrlText.Text = "";
                    break;
                case 1:
                    PageText.Text = "";
                    break;
                case 2:
                    EmailText.Text = "";
                    break;
                default:
                    break;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            UrlText.Text = string.Empty;
            PageText.Text = string.Empty;
            EmailText.Text = string.Empty;
        }
    }
}
