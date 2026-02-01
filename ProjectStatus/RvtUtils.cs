using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.IFC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using View = Autodesk.Revit.DB.View;

namespace ProjectStatus
{
    public class RvtUtils
    {
        //public static string GenerateStatusReport(Document doc)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine($"Revit File: {doc.Title}");
        //    sb.AppendLine($"Date: {DateTime.Now}");
        //    sb.AppendLine(new string('-', 50));

        //    // 1. File info
        //    sb.AppendLine("🔹 File Info:");
        //    sb.AppendLine($" - Number of Views: {new FilteredElementCollector(doc).OfClass(typeof(View)).Count()}");
        //    sb.AppendLine($" - Number of Levels: {new FilteredElementCollector(doc).OfClass(typeof(Level)).Count()}");
        //    sb.AppendLine($" - Number of Families Loaded: {new FilteredElementCollector(doc).OfClass(typeof(Family)).Count()}");

        //    // 2. Model Health Checks
        //    sb.AppendLine("\n🔹 Model Health Checks:");

        //    // Example: Unplaced or duplicate elements
        //    var unplacedElements = new FilteredElementCollector(doc)
        //        .WhereElementIsNotElementType()
        //        .Where(e => e.Category == null || e.Location == null)
        //        .ToList();

        //    sb.AppendLine($" - Unplaced or Orphan Elements: {unplacedElements.Count}");

        //    // Example: Check for warnings
        //    var warnings = doc.GetWarnings();
        //    sb.AppendLine($" - Warnings Count: {warnings.Count}");

        //    // 3. Recommendations
        //    sb.AppendLine("\n🔹 Recommendations:");
        //    sb.AppendLine(unplacedElements.Count > 0 ? " - Review unplaced/orphan elements" : " - All elements are placed correctly");
        //    sb.AppendLine(warnings.Count > 0 ? " - Resolve Revit warnings to improve model health" : " - No warnings detected");

        //    // Example: Recommend reviewing excessive family types
        //    int familyTypeThreshold = 50;
        //    var familyTypes = new FilteredElementCollector(doc)
        //        .OfClass(typeof(FamilySymbol))
        //        .ToList();

        //    sb.AppendLine(familyTypes.Count > familyTypeThreshold
        //        ? $" - Large number of family types ({familyTypes.Count}) may slow down the model"
        //        : " - Family types count is acceptable");

        //    return sb.ToString();
        //}

        public static int GetPerformanceScore(Document doc)
        {
            int score = 100;

            // File size (approx via element count)
            int elementCount = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .Count();

            if (elementCount > 500_000) score -= 30;
            else if (elementCount > 300_000) score -= 15;

            // CAD imports
            int cadImports = new FilteredElementCollector(doc)
                .OfClass(typeof(ImportInstance))
                .Count();

            score -= cadImports * 5;

            // In-place families
            int inPlaceFamilies = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Count(f => f.IsInPlace);

            score -= inPlaceFamilies * 3;

            return Math.Max(score, 0);
        }

        public static int GetWarningsScore(Document doc)
        {
            int warningCount = doc.GetWarnings().Count;

            if (warningCount == 0) return 100;
            if (warningCount < 50) return 85;
            if (warningCount < 100) return 70;
            if (warningCount < 200) return 50;

            return 25;
        }

        public static int GetCleanlinessScore(Document doc)
        {
            int score = 100;

            int families = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Count();

            int familyTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Count();

            if (familyTypes > families * 10)
                score -= 20;

            int materials = new FilteredElementCollector(doc)
                .OfClass(typeof(Material))
                .Count();

            if (materials > 500)
                score -= 15;

            return Math.Max(score, 0);
        }

        public static int GetViewsScore(Document doc)
        {
            int score = 100;

            var views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate)
                .ToList();

            if (views.Count > 800) score -= 40;
            else if (views.Count > 500) score -= 25;

            int viewsWithoutTemplate = views.Count(v => v.ViewTemplateId == ElementId.InvalidElementId);
            score -= viewsWithoutTemplate / 10;

            return Math.Max(score, 0);
        }

        public static int GetLinksScore(Document doc)
        {
            int score = 100;

            int revitLinks = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Count();

            int cadImports = new FilteredElementCollector(doc)
                .OfClass(typeof(ImportInstance))
                .Count();

            score -= cadImports * 10;

            if (revitLinks > 10)
                score -= (revitLinks - 10) * 3;

            return Math.Max(score, 0);
        }

        public static int GetDataScore(Document doc)
        {
            int score = 100;

            var elements = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .Take(500); // sample for performance

            int missingParamCount = 0;

            foreach (var e in elements)
            {
                Parameter p = e.LookupParameter("Fire Rating");
                if (p != null && !p.HasValue)
                    missingParamCount++;
            }

            if (missingParamCount > 50) score -= 30;
            else if (missingParamCount > 20) score -= 15;

            return Math.Max(score, 0);
        }

        public static int CalculateFinalHealthScore(Document doc)
        {
            double performance = GetPerformanceScore(doc) * 0.25;
            double warnings = GetWarningsScore(doc) * 0.25;
            double cleanliness = GetCleanlinessScore(doc) * 0.15;
            double views = GetViewsScore(doc) * 0.15;
            double links = GetLinksScore(doc) * 0.10;
            double data = GetDataScore(doc) * 0.10;

            return (int)Math.Round(
                performance + warnings + cleanliness + views + links + data
            );
        }

        public static string GetHealthLabel(int score)
        {
            if (score >= 80) return "🟢 Healthy";
            if (score >= 60) return "🟡 Needs Attention";
            return "🔴 Critical";
        }

    }
}
