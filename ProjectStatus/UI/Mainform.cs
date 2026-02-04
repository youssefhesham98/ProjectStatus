using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Form = System.Windows.Forms.Form;
using Point = System.Drawing.Point;

namespace ProjectStatus
{
    public partial class Mainform : Form
    {
        public Mainform()
        {
            InitializeComponent();
            var result = RvtUtils.CheckProjectBasePointAgainstFirstGridIntersection(ExCmd.doc);
            var units = RvtUtils.GetProjectUnit(ExCmd.doc, SpecTypeId.Length);
            var (isCentral, hasWorksets) = RvtUtils.CheckCentralAndWorksets(ExCmd.doc);
            bool hasPurge = RvtUtils.HasPurgeableElements(ExCmd.doc);
            var file_size = RvtUtils.GetRevitFileSizeMB(ExCmd.doc);
            bool hasDuplicates = RvtUtils.HasDuplicatedElements(ExCmd.doc);

            align.Text = $"Project Base Aligned: {(result.IsAligned ? "YES" : "NO")}";
            projectbase.Text = $"Project Base Point: \n{RvtUtils.FormatXYZ(result.SurveyPoint)}";
            surveypoint.Text = $"Survey Point: \n{RvtUtils.FormatXYZ(result.ProjectBasePoint)}";
            gridintersection.Text = $"Grid Intersection: \n{RvtUtils.FormatXYZ(result.GridIntersection)}";
            units_.Text = $"Length Units: {units}";
            iscentral.Text = $"Central File: {(isCentral ? "Yes" : "No")}";
            worksets.Text = $"User Worksets Exist: {(hasWorksets ? "Yes" : "No")}";
            size.Text = $"File Size: {file_size} MB";
            purge.Text = $"Purgeable Elements Exist: {(hasPurge ? "Yes" : "No")}";
            duplicate.Text = $"Duplicate Elements Exist: {(hasDuplicates ? "Yes" : "No")}";
        }

        private void gnrte_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    // Generate report
            //    string report = RvtUtils.GenerateStatusReport(ExCmd.doc);

            //    // Save report
            //    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RevitStatusReport.txt");
            //    File.WriteAllText(path, report);

            //    TaskDialog.Show("Status Report", $"Report generated successfully!\nSaved to:\n{path}");
            //}
            //catch (Exception ex)
            //{
            //    TaskDialog.Show("Error", $"An error occurred while generating the report:\n{ex.Message}");
            //}

            int finalScore = RvtUtils.CalculateFinalHealthScore(ExCmd.doc);
            string status = RvtUtils.GetHealthLabel(finalScore);

            TaskDialog.Show(
                "Revit Health Status",
                $"Final Score: {finalScore}/100\nStatus: {status}"
            );
        }

        private void test_Click(object sender, EventArgs e)
        {
            var (x, y) = RvtUtils.GetGridDimensionsXY(ExCmd.doc);
            var levelDims = RvtUtils.GetLevelDimensions(ExCmd.doc);
            RvtUtils.PrintGridandLevelsDimensions(x,y,levelDims);

            #region Old
            //var result = RvtUtils.CheckProjectBasePointAgainstFirstGridIntersection(ExCmd.doc);

            //string msg =
            //    $"Project Base Point:\n{RvtUtils.FormatXYZ(result.ProjectBasePoint)}\n\n" +
            //    $"Grid Intersection:\n{RvtUtils.FormatXYZ(result.GridIntersection)}\n\n" +
            //    $"Aligned: {(result.IsAligned ? "YES" : "NO")}";

            //TaskDialog.Show("Base Point Check", msg);

            ////ExCmd.doc.GetUnits();

            //TaskDialog.Show("Units", $"{RvtUtils.GetProjectUnit(ExCmd.doc, SpecTypeId.Length)}");

            //var (isCentral, hasWorksets) = RvtUtils.CheckCentralAndWorksets(ExCmd.doc);

            //string resu =
            //    $"Central File: {(isCentral ? "Yes" : "No")}\n" +
            //    $"User Worksets Exist: {(hasWorksets ? "Yes" : "No")}";

            //TaskDialog.Show("Worksharing Health", resu);

            //bool hasPurge = RvtUtils.HasPurgeableElements(ExCmd.doc);

            //TaskDialog.Show("Purgeable Elements", $"Purgeable Elements Exist: {(hasPurge ? "Yes" : "No")}");

            //TaskDialog.Show("File Size: ", $"{RvtUtils.GetRevitFileSizeMB(ExCmd.doc)}");

            //bool hasDuplicates = RvtUtils.HasDuplicatedElements(ExCmd.doc);

            //TaskDialog.Show("Duplicate Elements Check", hasDuplicates ? "Yes" : "No");
            #endregion
        }

        private void Mainform_Load(object sender, EventArgs e)
        {

        }
    }
}
