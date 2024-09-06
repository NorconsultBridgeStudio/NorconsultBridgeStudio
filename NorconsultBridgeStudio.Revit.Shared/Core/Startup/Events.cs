using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace NorconsultBridgeStudio.Revit.Core.Startup
{
    static class Events
    {
        internal static bool StateIsRegistered_Application_DocumentOpened = false;
        internal static bool StateIsRegistered_Application_DocumentCreated = false;
        internal static bool StateIsRegistered_Application_ViewActivated = false;
        public static void AddEvents(ControlledApplication controlledApplication)
        {
            if (!StateIsRegistered_Application_DocumentOpened)
            {
                controlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(Application_DocumentOpened);
                StateIsRegistered_Application_DocumentOpened = true;
            }

            if (!StateIsRegistered_Application_DocumentCreated)
            {
                controlledApplication.DocumentCreated += new EventHandler<DocumentCreatedEventArgs>(Application_DocumentCreated);
                StateIsRegistered_Application_DocumentCreated = true;
            }
        }

        public static void RemoveEvents(ControlledApplication controlledApplication)
        {
            if (StateIsRegistered_Application_DocumentOpened)
            {
                controlledApplication.DocumentOpened -= new EventHandler<DocumentOpenedEventArgs>(Application_DocumentOpened);
                StateIsRegistered_Application_DocumentOpened = false;
            }

            if (StateIsRegistered_Application_DocumentCreated)
            {
                controlledApplication.DocumentCreated -= new EventHandler<DocumentCreatedEventArgs>(Application_DocumentCreated);
                StateIsRegistered_Application_DocumentCreated = false;
            }

            if (!StateIsRegistered_Application_ViewActivated)
            {
                App.UIApplication.ViewActivated -= Application_ViewActivated;
                StateIsRegistered_Application_ViewActivated = false;
            }
        }
        public static void Application_DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            ResetApp(args.Document);
        }
        public static void Application_DocumentCreated(object sender, DocumentCreatedEventArgs args)
        {
            ResetApp(args.Document);
        }
        private static void ResetApp(Document doc)
        {
            App.CurrentDocument = doc;
            App.UIApplication = new UIApplication(App.CurrentDocument.Application);

            if (!StateIsRegistered_Application_ViewActivated)
            {
                App.UIApplication.ViewActivated += Application_ViewActivated;
                StateIsRegistered_Application_ViewActivated = true;
            }
        }
        internal static void Application_ViewActivated(object sender, ViewActivatedEventArgs e)
        {
            App.CurrentDocument = e.Document;
            App.UIApplication = new UIApplication(App.CurrentDocument.Application);
        }
    }
}