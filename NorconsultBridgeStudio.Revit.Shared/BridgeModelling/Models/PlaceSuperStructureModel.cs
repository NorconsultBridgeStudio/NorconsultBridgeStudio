using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using NorconsultBridgeStudio.Revit.BridgeModelling.ViewModels;
using NorconsultBridgeStudio.Revit.Core;
using NorconsultBridgeStudio.Revit.Core.Exceptions;
using NorconsultBridgeStudio.Revit.Core.Extensions;
using NorconsultBridgeStudio.Revit.Core.Utils;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Models
{
    static class PlaceSuperStructureModel
    {
        public static void Execute()
        {
            var superstructures = GetSuperStructureFamilies();
            var vm = new SingleComboboxViewModel("NorconsultBridgeStudio - Place Superstructure", "Superstructures", superstructures);
            if (vm.View.ShowDialog() != true)
                return;

            PlaceInDocument(App.CurrentDocument, vm.Element.Id);
        }

        private static List<RevitElementModel> GetSuperStructureFamilies()
        {
            var families = new FilteredElementCollector(App.CurrentDocument)
                                        .OfClass(typeof(Family))
                                        .ToElements()
                                        .Cast<Family>()
                                        .Where(family => family.FamilyCategoryId == new ElementId(BuiltInCategory.OST_GenericModel))
                                        .Where(family => App.CurrentDocument.GetElement(family.GetFamilySymbolIds().FirstOrDefault() ?? ElementId.InvalidElementId)?.LookupParameter(Constants.WatermarkParameter)?.AsString() == Constants.WatermarkValue)
                                        .ToList();

            if (!families.Any())
                throw new NorconsultBridgeStudioError("No superstructure families found in the .rvt-document. Please create or load a superstructurefamily first.");

            List<RevitElementModel> elements = new List<RevitElementModel>();
            foreach (Family family in families)
            {
                var element = new RevitElementModel(family.GetFamilySymbolIds().First(), family.Name);
                elements.Add(element);
            }

            return elements;
        }

        public static ElementId PlaceInDocument(Document doc, ElementId symbolId)
        {
            ElementId familyInstanceId;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Place Superstructure");
                familyInstanceId = PlaceInstance(doc, symbolId);
                trans.Commit();
            }
            return familyInstanceId;
        }
        private static ElementId PlaceInstance(Document doc, ElementId symbolId)
        {
            FamilySymbol symbol = doc.GetElement(symbolId) as FamilySymbol;

            if (!symbol.IsActive)
                symbol.Activate();

            XYZ familyOrigin = GetRelativeFamilyOrigin(symbol);
            double rotationAngle = doc.GetProjectRotation().Value;

            FamilyInstance instance = doc.Create.NewFamilyInstance(familyOrigin, symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            XYZ pivotPointInPlane = new XYZ();
            XYZ pivotPointInPlanePlusZ = new XYZ(0, 0, 1000);
            Line axisProjectBasePoint = Line.CreateBound(pivotPointInPlane, pivotPointInPlanePlusZ);
            ElementTransformUtils.RotateElement(doc, instance.Id, axisProjectBasePoint, rotationAngle);

            return instance.Id;
        }


        public static XYZ GetRelativeFamilyOrigin(FamilySymbol familySymbol)
        {
            double xOrigin = familySymbol.LookupParameter(Constants.FamilyBasePointX).AsDouble();
            double yOrigin = familySymbol.LookupParameter(Constants.FamilyBasePointY).AsDouble();
            double zOrigin = familySymbol.LookupParameter(Constants.FamilyBasePointZ).AsDouble();

            XYZ origin = new XYZ(xOrigin, yOrigin, zOrigin);
            return origin;
        }
    }
}