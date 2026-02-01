using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;
using System.Reflection;
using Form = System.Windows.Forms.Form;

namespace ProjectStatus
{
    [Transaction(TransactionMode.Manual)]
    public class ExCmd : IExternalCommand
    {
        private static Mainform maininterface = null;
        public static Document doc { get; set; }
        public static UIDocument uidoc { get; set; }
        public static UIApplication uiapp { get; set; }
        public static ExEvt exevt { get; set; }
        public static ExternalEvent exevthan { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            uiapp = commandData.Application;
            //uiapp.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.AlignedToSelectedLevels));

            //Data.Intialize();

            // If the form is already open, close it before opening a new one
            //if (maininterface != null && !maininterface.IsDisposed)
            //{
            //    maininterface.Close();
            //    maininterface.Dispose();
            //}

            maininterface = new Mainform();
            ShowSingle(maininterface);
            //maininterface.Show();

            #region ex_ev&ev_han&tns
            exevt = new ExEvt();
            exevthan = ExternalEvent.Create(exevt);

            //using (Transaction tns = new Transaction(doc, "Renamer"))
            //{
            //    tns.Start();

            //    tns.Commit();
            //}
            #endregion

            return Result.Succeeded;
        }
        //WF
        public static void ShowSingle(Form form)
        {
            var existing = System.Windows.Forms.Application.OpenForms
                .OfType<Form>()
                .FirstOrDefault(f => f.GetType() == form.GetType());

            if (existing != null)
            {
                if (existing.WindowState == FormWindowState.Minimized)
                    existing.WindowState = FormWindowState.Normal;

                existing.BringToFront();
                existing.Activate();
                existing.Focus();
                return;
            }

            form.Show();
        }
    }
}
