using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NorconsultBridgeStudio.Revit.BridgeModelling.Utils;
using NorconsultBridgeStudio.Revit.Core;
using NorconsultBridgeStudio.Revit.Core.Exceptions;
using NorconsultBridgeStudio.Revit.Core.Extensions;
using NorconsultBridgeStudio.Revit.Core.Utils;
using Microsoft.Win32;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Models
{
    class AdaptiveCrossSectionModel
    {
        private readonly List<Reference> _references;
        private readonly Document _rvtDocument;
        public AdaptiveCrossSectionModel(List<Reference> references) 
        { 
            _references = references;
            _rvtDocument = App.CurrentDocument;
        }
        public void Execute()
        {
            try
            {
                var curves = GetCurves();
                var path = GetCrossSectionPath();
                var newDocument = CreateFamilyDocument(path, curves);
                newDocument.Close();

                //OpenAndActivateSketchPlaneSectionView(path);

                App.CurrentDocument = _rvtDocument;
            }
            catch (Exception ex)
            {
                App.CurrentDocument = _rvtDocument;
                throw ex;
            }
        }
        private List<Curve> GetCurves()
        {
            List<Curve> curves = new List<Curve>();

            Transform translation = GetTranslation();
            Transform rotation = GetRotation();

            foreach (var curveElementReference in _references)
            {
                DetailCurve detailCurve = App.CurrentDocument.GetElement(curveElementReference) as DetailCurve;
                curves.Add(detailCurve.GeometryCurve.CreateTransformed(translation).CreateTransformed(rotation));
            }
            return curves;
        }
        private string GetCrossSectionPath()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (!string.IsNullOrEmpty(Paths.LastDirectory))
                saveFileDialog.InitialDirectory = Paths.LastDirectory;

            saveFileDialog.Filter = "NorconsultBridgeStudio cross section family file (_*.rfa)|*.rfa|Revit family file (*.rfa)|*.rfa|All files (*.*)|*.*";

            if (!saveFileDialog.ShowDialog() == true)
                throw new NorconsultBridgeStudioError("Operation cancelled by user, file was not saved");

            string path = saveFileDialog.FileName;
            Paths.LastDirectory = Path.GetDirectoryName(path);
            Paths.LastCrossSection = path;


            return path;
        }
        private Document CreateFamilyDocument(string path, List<Curve> curves)
        {
            var newDocument = App.UIApplication.Application.NewFamilyDocument(Paths.GetAdaptivePath());

            using (Transaction transaction = new Transaction(newDocument))
            {
                transaction.Start("Draw Lines");

                Parameter hostRebarParameter = App.CurrentDocument.OwnerFamily.get_Parameter(BuiltInParameter.FAMILY_CAN_HOST_REBAR);
                hostRebarParameter.Set(0);

                Parameter sharedParameter = App.CurrentDocument.OwnerFamily.get_Parameter(BuiltInParameter.FAMILY_SHARED);
                sharedParameter.Set(0);

                SketchPlane sketchPlane = App.CurrentDocument.GetSketchPlane("Center (Front/Back)");

                foreach (var curve in curves)
                {
                    newDocument.FamilyCreate.NewModelCurve(curve, sketchPlane);
                }
                transaction.Commit();
            }
            SaveAsOptions saveAsOptions = new SaveAsOptions()
            {
                OverwriteExistingFile = true,
                MaximumBackups = 1
            };

            App.CurrentDocument.SaveAs(path, saveAsOptions);

            return newDocument;
        }
        private Transform GetTranslation()
        {
            ViewSection view = App.CurrentDocument.ActiveView as ViewSection;
            XYZ origin = ExtensibleStorageUtils.RestoreOrigin(view);
            Transform translation = Transform.CreateTranslation(origin.Negate());
            return translation;
        }
        private Transform GetRotation()
        {
            ViewSection view = App.CurrentDocument.ActiveView as ViewSection;
            XYZ directionFromUser = view.ViewDirection.Negate();
            double angle = XYZ.BasisY.AngleOnPlaneTo(directionFromUser, XYZ.BasisZ);
            Transform rotation = Transform.CreateRotation(XYZ.BasisZ, -angle);
            return rotation;
        }
        //private void SwitchView()
        //{
        //    ViewSection newActiveView = new FilteredElementCollector(App.CurrentDocument)
        //                                    .OfClass(typeof(ViewSection))
        //                                    .ToElements()
        //                                    .Cast<ViewSection>()
        //                                    .First(v => v.Name.Contains("Front"));

        //    App.UIApplication.ActiveUIDocument.ActiveView = newActiveView;

        //    HashSet<ElementId> openUIViewIds = App.UIApplication.ActiveUIDocument.GetOpenUIViews()
        //                                  .Select(view => view.ViewId)
        //                                  .ToHashSet();

        //    HashSet<ElementId> thisDocViewIds = new FilteredElementCollector(App.CurrentDocument)
        //                                           .OfClass(typeof(View))
        //                                           .ToElementIds()
        //                                           .ToHashSet();

        //    thisDocViewIds.UnionWith(openUIViewIds);
        //    thisDocViewIds.Remove(newActiveView.Id);

        //    List<UIView> openUIViews = App.UIApplication.ActiveUIDocument.GetOpenUIViews()
        //                                    .Where(view => thisDocViewIds.Contains(view.ViewId))
        //                                    .ToList();

        //    foreach (UIView uiView in openUIViews)
        //    {
        //        uiView.Close();
        //    }
        //}
        //private void OpenAndActivateSketchPlaneSectionView(string path)
        //{
        //    App.UIApplication.OpenAndActivateDocument(path);
        //    SwitchView();
        //}
    }
}