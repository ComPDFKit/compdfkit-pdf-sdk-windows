using System.Reflection;
using System.Resources;

namespace ComPDFKit.Controls.Helper
{
    public abstract class LanguageHelper
    {
		public static ResourceManager BotaManager= new ResourceManager("ComPDFKit.Controls.Strings.Bota", Assembly.GetExecutingAssembly());
        public static ResourceManager CommonManager= new ResourceManager("ComPDFKit.Controls.Strings.Common", Assembly.GetExecutingAssembly());
        public static ResourceManager PropertyPanelManager= new ResourceManager("ComPDFKit.Controls.Strings.PropertyPanel", Assembly.GetExecutingAssembly());
        public static ResourceManager ToolBarManager= new ResourceManager("ComPDFKit.Controls.Strings.ToolBar", Assembly.GetExecutingAssembly());
        public static ResourceManager SigManager= new ResourceManager("ComPDFKit.Controls.Strings.Signature", Assembly.GetExecutingAssembly());
        public static ResourceManager DocInfoManager= new ResourceManager("ComPDFKit.Controls.Strings.DocInfo", Assembly.GetExecutingAssembly());
        public static ResourceManager SecurityManager= new ResourceManager("ComPDFKit.Controls.Strings.Security", Assembly.GetExecutingAssembly());
        public static ResourceManager DocEditorManager= new ResourceManager("ComPDFKit.Controls.Strings.DocEditor", Assembly.GetExecutingAssembly());
        public static ResourceManager CompressManager = new ResourceManager("ComPDFKit.Controls.Strings.Compress", Assembly.GetExecutingAssembly());
    }
}