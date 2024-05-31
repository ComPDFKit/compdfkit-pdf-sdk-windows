using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ComPDFKit.Tool
{
    public static class CustomCommands
    {
        internal enum CustomCommandId
        {
            PasteMatchStyle
        }

        public class CustomCommand: RoutedUICommand
        {
            internal CustomCommandId Id;

            public CustomCommand(string name, string text) : base(text, name, typeof(CustomCommands))
            {
                
            }
        }
        
        private static List<CustomCommand> Commands { get; set; }=new List<CustomCommand>();
        /// <summary>
        /// Pastes text matching style.
        /// </summary>
        public static RoutedUICommand PasteWithoutStyle => EnsureCommand(CustomCommandId.PasteMatchStyle);

        private static RoutedUICommand EnsureCommand(CustomCommandId idCommand)
        {
            CustomCommand command = null;
            if (Commands!=null && Commands.Count>0)
            {
                command = Commands.AsEnumerable().FirstOrDefault(x => x.Id == idCommand);
            }

            if(command == null)
            {
                command= new CustomCommand("PasteWithoutStyle", "Paste Without Style");
                command.Id = idCommand;
                Commands?.Add(command);
            }
            return command;
        }
    }
}