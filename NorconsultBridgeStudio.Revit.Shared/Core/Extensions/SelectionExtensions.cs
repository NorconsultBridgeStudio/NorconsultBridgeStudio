using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;

namespace NorconsultBridgeStudio.Revit.Core.Extensions
{
    public static class SelectionExtensions
    {
        /// <summary>
        /// Returns any preselected elements for a given selection filter
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="selectionFilter">The selection filter to apply to the preselected elements</param>
        /// <returns></returns>
        public static List<Reference> GetPreselection(this Selection selection, ISelectionFilter selectionFilter)
        {
            var references = new List<Reference>();

            var ids = selection.GetElementIds();
            foreach ( var id in ids )
            {
                Element element = App.CurrentDocument.GetElement(id);
                if (selectionFilter.AllowElement(element))
                    references.Add(new Reference(element));
            }

            return references;
        }
    }
}