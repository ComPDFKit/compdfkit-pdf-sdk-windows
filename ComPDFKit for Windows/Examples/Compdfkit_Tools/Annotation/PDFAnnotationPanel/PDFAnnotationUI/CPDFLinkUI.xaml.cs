using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Compdfkit_Tools.Annotation.PDFAnnotationUI
{
    public partial class CPDFLinkUI : UserControl, INotifyPropertyChanged
    {
        bool OpenPDF = false;
        int totalPage = 0;
        int LinkPage = 0;

        private LinkAnnotArgs LinkAnnot;
        private AnnotAttribEvent annotAttribEvent;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
                if (annotAttribEvent != null)
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

        public void SetPresentAnnotAttrib(AnnotAttribEvent AttribEvent, int PageCount)
        {
            annotAttribEvent = AttribEvent;
            UrlText.Text = "";
            PageText.Text = "";
            EmailText.Text = "";
            SaveBtn.IsEnabled = true;
            totalPage = PageCount;
            PagePromptText = Helper.LanguageHelper.PropertyPanelManager.GetString("Holder_Jump") + totalPage;
            if (AttribEvent.Attribs.ContainsKey(AnnotAttrib.LinkDestIndx))
            {
                int pageNum = (int)AttribEvent.Attribs[AnnotAttrib.LinkDestIndx] + 1;
                if (pageNum > 0 && pageNum <= totalPage)
                {
                    PageText.Text = pageNum.ToString();
                    SelectedIndex = 1;
                }
            }
            if (AttribEvent.Attribs.ContainsKey(AnnotAttrib.LinkUri))
            {
                string linkUrl = (string)AttribEvent.Attribs[AnnotAttrib.LinkUri];
                if (!string.IsNullOrEmpty(linkUrl))
                {

                    if (linkUrl.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                    {
                        EmailText.Text = linkUrl.ToLower().TrimStart("mailto:".ToCharArray());
                        SelectedIndex = 2;
                    }
                    else
                    {
                        UrlText.Text = linkUrl;
                        SelectedIndex = 0;
                    }
                }
            }
        }

        public void InitLinkAnnotArgs(LinkAnnotArgs linkAnnotArgs, int PageCount)
        {
            LinkAnnot = linkAnnotArgs;
            InputEnable = true;
            LinkAnnot.LinkDrawFinished += LinkAnnot_LinkDrawFinished;
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
            if (LinkAnnot != null)
            {
                switch (SelectedIndex)
                {
                    case 0:
                        LinkAnnot.LinkType = LINK_TYPE.URI;
                        string urlPath = UrlText.Text.Trim().ToLower();
                        if (urlPath.StartsWith("http://") || urlPath.StartsWith("https://"))
                        {
                            LinkAnnot.URI = urlPath;
                        }
                        else
                        {
                            LinkAnnot.URI = "http://" + urlPath;
                        }
                        break;
                    case 1:
                        LinkAnnot.LinkType = LINK_TYPE.GOTO;
                        LinkAnnot.DestIndex = LinkPage - 1;
                        break;
                    case 2:
                        LinkAnnot.LinkType = LINK_TYPE.URI;
                        LinkAnnot.URI = "mailto:" + EmailText.Text.Trim();
                        break;
                    default:
                        break;
                }

                LinkAnnot.InvokeLinkSaveCalled(null, null);
            }
            else
            {
                switch (SelectedIndex)
                {
                    case 0:
                        annotAttribEvent.UpdateAttrib(AnnotAttrib.LinkType, LINK_TYPE.URI);
                        string urlPath = UrlText.Text.Trim().ToLower();
                        if (urlPath.StartsWith("http://") || urlPath.StartsWith("https://"))
                        {
                            annotAttribEvent.UpdateAttrib(AnnotAttrib.LinkUri, urlPath);
                        }
                        else
                        {
                            annotAttribEvent.UpdateAttrib(AnnotAttrib.LinkUri, "http://" + UrlText.Text.Trim().ToLower());
                        }
                        break;
                    case 1:
                        annotAttribEvent.UpdateAttrib(AnnotAttrib.LinkType, LINK_TYPE.GOTO);
                        annotAttribEvent.UpdateAttrib(AnnotAttrib.LinkDestIndx, LinkPage - 1);
                        break;
                    case 2:
                        annotAttribEvent.UpdateAttrib(AnnotAttrib.LinkType, LINK_TYPE.URI);
                        annotAttribEvent.UpdateAttrib(AnnotAttrib.LinkUri, "mailto:" + EmailText.Text.Trim());
                        break;
                    default:
                        break;
                }
                annotAttribEvent.UpdateAnnot();
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
            if (LinkAnnot != null)
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
            if (LinkAnnot != null)
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
            if (LinkAnnot != null)
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
            bool BtnIsEnabled = false;

            if (LinkAnnot != null)
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
        }
        #endregion
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Binding Indexbinding = new Binding();
            Indexbinding.Source = this;
            Indexbinding.Path = new System.Windows.PropertyPath("SelectedIndex");
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
