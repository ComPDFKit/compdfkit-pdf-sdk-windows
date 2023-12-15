using System.Reflection;
using System.Resources;

namespace Compdfkit_Tools.Helper
{
    public abstract class LanguageHelper
    {
		public static ResourceManager BotaManager= new ResourceManager("Compdfkit_Tools.Strings.Bota", Assembly.GetExecutingAssembly());
        public static ResourceManager CommonManager= new ResourceManager("Compdfkit_Tools.Strings.Common", Assembly.GetExecutingAssembly());
        public static ResourceManager PropertyPanelManager= new ResourceManager("Compdfkit_Tools.Strings.PropertyPanel", Assembly.GetExecutingAssembly());
        public static ResourceManager ToolBarManager= new ResourceManager("Compdfkit_Tools.Strings.ToolBar", Assembly.GetExecutingAssembly());
        public static ResourceManager SigManager= new ResourceManager("Compdfkit_Tools.Strings.Signature", Assembly.GetExecutingAssembly());
        public static ResourceManager DocInfoManager= new ResourceManager("Compdfkit_Tools.Strings.DocInfo", Assembly.GetExecutingAssembly());
        public static ResourceManager SecurityManager= new ResourceManager("Compdfkit_Tools.Strings.Security", Assembly.GetExecutingAssembly());
        public static ResourceManager DocEditorManager= new ResourceManager("Compdfkit_Tools.Strings.DocEditor", Assembly.GetExecutingAssembly());
    }
}