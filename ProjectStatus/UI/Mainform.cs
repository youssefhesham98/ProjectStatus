using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Color = System.Drawing.Color;
using Form = System.Windows.Forms.Form;
using Panel = System.Windows.Forms.Panel;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace ProjectStatus
{
    public partial class Mainform : Form
    {
        private void MakePanelRounded(Panel panel, int radius)
        {
            Rectangle r = panel.ClientRectangle;
            int d = radius * 2;

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            panel.Region = new Region(path);
        }
        public Mainform()
        {
            InitializeComponent();
            MakePanelRounded(panel6, 5);
            MakePanelRounded(panel7, 5);
            MakePanelRounded(panel8, 5);
            MakePanelRounded(panel9, 5);
            MakePanelRounded(panel10, 5);
            MakePanelRounded(panel11, 5);
            MakePanelRounded(panel12, 5);
            MakePanelRounded(panel3, 7);
            MakePanelRounded(panel2, 7);
            MakePanelRounded(panel4, 7);
            MakePanelRounded(panel5, 7);
            var result = RvtUtils.CheckProjectBasePointAgainstFirstGridIntersection(ExCmd.doc);
            var units = RvtUtils.GetProjectUnit(ExCmd.doc, SpecTypeId.Length);
            var (isCentral, hasWorksets) = RvtUtils.CheckCentralAndWorksets(ExCmd.doc);
            bool hasPurge = RvtUtils.HasPurgeableElements(ExCmd.doc);
            var file_size = RvtUtils.GetRevitFileSizeMB(ExCmd.doc);
            bool hasDuplicates = RvtUtils.HasDuplicatedElements(ExCmd.doc);
            var (x, y) = RvtUtils.GetGridDimensionsXY(ExCmd.doc);
            var levelDims = RvtUtils.GetLevelDimensions(ExCmd.doc);
            string dims = RvtUtils.PrintGridandLevelsDimensions(x, y, levelDims);
            //StringBuilder gridsx = new StringBuilder();
            //StringBuilder gridsy = new StringBuilder();
            //StringBuilder levels = new StringBuilder();
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
            //gridsx.Append($"Grids in X direction.");
            //foreach (var grid in x)
            //{
            //    gridsx.AppendLine(grid.ToString());
            //}
            //gridsy.Append($"Grids in Y direction.");
            //foreach (var grid in y)
            //{
            //    gridsy.AppendLine(grid.ToString());
            //}
            //levels.Append($"Levels diemsnions.");
            //foreach (var level in levelDims)
            //{
            //    levels.AppendLine(level.ToString());
            //}
            gridsxdim.Text = dims;

            int finalScore = RvtUtils.CalculateFinalHealthScore(ExCmd.doc);
            string status = RvtUtils.GetHealthLabel(finalScore);
            // Clamp just to be safe
            finalScore = Math.Max(0, Math.Min(100, finalScore));
            score.Value = finalScore;
            progress.Text = $"Progress: {finalScore}%";
            status__.Text = $"Status: {status}";

            #region try
            //score.Value = 0;
            //score.BackColor = Color.FromArgb(50, 50, 50);
            //for (int i = 0; i <= finalScore; i++)
            //{
            //    score.Value = i;
            //    Application.DoEvents(); // Allow UI to update
            //    System.Threading.Thread.Sleep(5); // Adjust for animation speed
            //}

            //if (finalScore < 40)
            //{
            //    score.ForeColor = Color.Red;
            //}
            //else if (finalScore < 70)
            //{
            //    score.ForeColor = Color.Orange;
            //}
            //else
            //{
            //    score.ForeColor = Color.Green;
            //}
            #endregion
            #region try
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
            #endregion
        }
        private void Mainform_Load(object sender, EventArgs e)
        {

        }
        #region Old
        //int finalScore = RvtUtils.CalculateFinalHealthScore(ExCmd.doc);
        //string status = RvtUtils.GetHealthLabel(finalScore);
        //TaskDialog.Show("Revit Health Status",$"Final Score: {finalScore}/100\nStatus: {status}");      

        //var (x, y) = RvtUtils.GetGridDimensionsXY(ExCmd.doc);
        //var levelDims = RvtUtils.GetLevelDimensions(ExCmd.doc);
        //RvtUtils.PrintGridandLevelsDimensions(x, y, levelDims);

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
}
