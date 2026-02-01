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

namespace ProjectStatus
{
    public partial class Mainform : Form
    {
        public Mainform()
        {
            InitializeComponent();
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
    }
}
