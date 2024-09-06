using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Text;

namespace NorconsultBridgeStudio.Revit.Core.Utils
{
    public class NorconsultBridgeStudioErrorDialog : TaskDialog
    {
        public string FunctionName;

        public NorconsultBridgeStudioErrorDialog(Exception e, string functionName) : base("Bruvit Error")
        {
            MainInstruction = "Something went wrong";

            MainContent = e.Message;
            ExpandedContent = InterpretException(e);
        }
        public string InterpretException(Exception e)
        {
            string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split("\\".ToCharArray());

            string name = userName.LastOrDefault();

            string content = "Error registered in Bruvit\r\n" +
                $"\r\n" +
                $"Reported by: {name}\r\n" +
                $"\r\n" +
                $"Function: {FunctionName}\r\n" +
                $"\r\n" +
                $"{ParseException(e)}\r\n" +
                $"\r\n" +
                $"Document: {App.CurrentDocument.PathName}\r\n";
            
            return content;
        }

        private string ParseException(Exception e)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(
                $"----------\r\n" +
                $"Exception:\r\n" +
                $"\r\n" +
                $"Message:\r\n" +
                $"{e.Message}\r\n" +
                $"\r\n" +
                $"StackTrace:\r\n" +
                $"{e.StackTrace}\r\n" +
                $"\r\n" +
                $"Source:\r\n" +
                $"{e.Source}\r\n" +
                $"\r\n" +
                $"TargetSite:\r\n" +
                $"{e.TargetSite.Name}\r\n" +
                $"\r\n");

            if (e.InnerException != null)
            {
                sb.Append("InnerException:\r\n");
                sb.Append(ParseException(e.InnerException));
            }

            sb.Append($"----------\r\n");

            return sb.ToString();
        }
    }
}