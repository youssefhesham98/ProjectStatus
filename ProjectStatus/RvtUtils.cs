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
    public class BasePointGridCheckResult
    {
        public XYZ ProjectBasePoint { get; set; }
        public XYZ SurveyPoint { get; set; }
        public XYZ GridIntersection { get; set; }
        public bool IsAligned { get; set; }
    }
    public class RvtUtils
    {
        // OLD
        public static string GenerateStatusReport(Document doc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Revit File: {doc.Title}");
            sb.AppendLine($"Date: {DateTime.Now}");
            sb.AppendLine(new string('-', 50));

            // 1. File info
            sb.AppendLine("🔹 File Info:");
            sb.AppendLine($" - Number of Views: {new FilteredElementCollector(doc).OfClass(typeof(View)).Count()}");
            sb.AppendLine($" - Number of Levels: {new FilteredElementCollector(doc).OfClass(typeof(Level)).Count()}");
            sb.AppendLine($" - Number of Families Loaded: {new FilteredElementCollector(doc).OfClass(typeof(Family)).Count()}");

            // 2. Model Health Checks
            sb.AppendLine("\n🔹 Model Health Checks:");

            // Example: Unplaced or duplicate elements
            var unplacedElements = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .Where(e => e.Category == null || e.Location == null)
                .ToList();

            sb.AppendLine($" - Unplaced or Orphan Elements: {unplacedElements.Count}");

            // Example: Check for warnings
            var warnings = doc.GetWarnings();
            sb.AppendLine($" - Warnings Count: {warnings.Count}");

            // 3. Recommendations
            sb.AppendLine("\n🔹 Recommendations:");
            sb.AppendLine(unplacedElements.Count > 0 ? " - Review unplaced/orphan elements" : " - All elements are placed correctly");
            sb.AppendLine(warnings.Count > 0 ? " - Resolve Revit warnings to improve model health" : " - No warnings detected");

            // Example: Recommend reviewing excessive family types
            int familyTypeThreshold = 50;
            var familyTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .ToList();

            sb.AppendLine(familyTypes.Count > familyTypeThreshold
                ? $" - Large number of family types ({familyTypes.Count}) may slow down the model"
                : " - Family types count is acceptable");

            return sb.ToString();
        }
        /// <summary>
        /// creating score according to elements created in the model
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Retturn warnings score
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static int GetWarningsScore(Document doc)
        {
            int warningCount = doc.GetWarnings().Count;

            if (warningCount == 0) return 100;
            if (warningCount < 50) return 85;
            if (warningCount < 100) return 70;
            if (warningCount < 200) return 50;

            return 25;
        }
        /// <summary>
        /// Returns cleanliness score
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Return views score
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Returns links score
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Gets data score randomly on missing parameters
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public static string GetHealthLabel(int score)
        {
            if (score >= 80) return "🟢 Healthy";
            if (score >= 60) return "🟡 Needs Attention";
            return "🔴 Critical";
        }



        public static (List<double> xDistances, List<double> yDistances) GetGridDimensionsXY(Document doc)
        {
            const int precision = 8;

            var grids = new FilteredElementCollector(doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .Where(g => g.Curve is Line)
                .ToList();

            var xGrids = new List<Grid>();
            var yGrids = new List<Grid>();

            foreach (var grid in grids)
            {
                Line line = grid.Curve as Line;
                XYZ dir = line.Direction.Normalize();

                // Vertical grids → X spacing (direction ~ Y axis)
                if (Math.Abs(dir.X) < 0.01 && Math.Abs(dir.Y) > 0.9)
                    xGrids.Add(grid);

                // Horizontal grids → Y spacing (direction ~ X axis)
                else if (Math.Abs(dir.Y) < 0.01 && Math.Abs(dir.X) > 0.9)
                    yGrids.Add(grid);
            }

            // Sort by position
            xGrids = xGrids
                .OrderBy(g => ((Line)g.Curve).Origin.X)
                .ToList();

            yGrids = yGrids
                .OrderBy(g => ((Line)g.Curve).Origin.Y)
                .ToList();

            var xDistances = CalculateDistances(xGrids, axis: "X", precision);
            var yDistances = CalculateDistances(yGrids, axis: "Y", precision);

            return (xDistances, yDistances);
        }
        private static List<double> CalculateDistances(List<Grid> grids, string axis, int precision)
        {
            var distances = new List<double>();

            for (int i = 1; i < grids.Count; i++)
            {
                Line prev = grids[i - 1].Curve as Line;
                Line curr = grids[i].Curve as Line;

                double distance = axis == "X"
                    ? Math.Abs(curr.Origin.X - prev.Origin.X)
                    : Math.Abs(curr.Origin.Y - prev.Origin.Y);

                distances.Add(Math.Round(ToMillimeters(distance),8,MidpointRounding.AwayFromZero));
            }
            
            return distances;
        }
        private static double ToMillimeters(double feet)
        {
            return UnitUtils.ConvertFromInternalUnits(
                feet,
                UnitTypeId.Millimeters);
        }
        public static List<double> GetLevelDimensions(Document doc)
        {
            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .ToList();

            var distances = new List<double>();

            for (int i = 1; i < levels.Count; i++)
            {
                double elevationFeet =
                    levels[i].Elevation - levels[i - 1].Elevation;

                distances.Add(Math.Round(ToMillimeters(elevationFeet), 8, MidpointRounding.AwayFromZero));
            }

            return distances;
        }
        public static void PrintGridandLevelsDimensions(List<double> xDistances, List<double> yDistances,List<double> levelDistances)
        {
            var sb = new StringBuilder();

            sb.AppendLine("X Grid Spacing (mm):");
            foreach (double d in xDistances)
                sb.AppendLine(d.ToString("F8"));

            sb.AppendLine();
            sb.AppendLine("Y Grid Spacing (mm):");
            foreach (double d in yDistances)
                sb.AppendLine(d.ToString("F8"));
            sb.AppendLine();
            for (int i = 0; i < levelDistances.Count; i++)
            {
                sb.AppendLine(
                    $"Level {i + 1} → Level {i + 2}: {levelDistances[i]:F8}"
                );
            }

            TaskDialog.Show("Grid Dimensions", sb.ToString());
        }


        public static BasePointGridCheckResult CheckProjectBasePointAgainstFirstGridIntersection(Document doc)
        {
            BasePoint projectBasePoint = null;
            BasePoint surveyPoint = null;

            var basePoints = new FilteredElementCollector(doc)
                .OfClass(typeof(BasePoint))
                .Cast<BasePoint>();

            foreach (var bp in basePoints)
            {
                if (bp.IsShared)
                    surveyPoint = bp;
                else
                    projectBasePoint = bp;
            }

            if (projectBasePoint == null)
                throw new InvalidOperationException("Project Base Point not found.");

            var grids = new FilteredElementCollector(doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .Where(g => g.Curve is Line)
                .ToList();

            // Vertical grids (Y direction → X position)
            var verticalGrids = grids
                .Where(g =>
                {
                    XYZ d = ((Line)g.Curve).Direction.Normalize();
                    return Math.Abs(d.X) < 0.01 && Math.Abs(d.Y) > 0.9;
                })
                .OrderBy(g => ((Line)g.Curve).Origin.X)
                .ToList();

            // Horizontal grids (X direction → Y position)
            var horizontalGrids = grids
                .Where(g =>
                {
                    XYZ d = ((Line)g.Curve).Direction.Normalize();
                    return Math.Abs(d.Y) < 0.01 && Math.Abs(d.X) > 0.9;
                })
                .OrderBy(g => ((Line)g.Curve).Origin.Y)
                .ToList();

            if (!verticalGrids.Any() || !horizontalGrids.Any())
                throw new InvalidOperationException("Insufficient grids found.");

            Grid leftGrid = verticalGrids.First();
            Grid bottomGrid = horizontalGrids.Last();

            Line vLine = (Line)leftGrid.Curve;
            Line hLine = (Line)bottomGrid.Curve;

            IntersectionResultArray ira;
            SetComparisonResult result =
                vLine.Intersect(hLine, out ira);

            if (result != SetComparisonResult.Overlap || ira == null)
                throw new InvalidOperationException("Grids do not intersect.");

            XYZ intersection = ira.get_Item(0).XYZPoint;

            XYZ pbp = projectBasePoint.Position;

            bool isAligned = ArePointsEqual(
                pbp,
                intersection,
                tolerance: 0.001); // feet ≈ 0.3 mm

            return new BasePointGridCheckResult
            {
                ProjectBasePoint = pbp,
                SurveyPoint = surveyPoint?.Position,
                GridIntersection = intersection,
                IsAligned = isAligned
            };
        }
        private static bool ArePointsEqual(XYZ p1, XYZ p2, double tolerance)
        {
            return p1.DistanceTo(p2) <= tolerance;
        }
        public static string FormatXYZ(XYZ p)
        {
            return
                $"X: {UnitUtils.ConvertFromInternalUnits(p.X, UnitTypeId.Millimeters):F8}\n" +
                $"Y: {UnitUtils.ConvertFromInternalUnits(p.Y, UnitTypeId.Millimeters):F8}\n" +
                $"Z: {UnitUtils.ConvertFromInternalUnits(p.Z, UnitTypeId.Millimeters):F8}";
        }

        public static string GetProjectUnit(Document doc, ForgeTypeId specTypeId)
        {
            Units units = doc.GetUnits();

            FormatOptions fo =
                units.GetFormatOptions(specTypeId);

            return LabelUtils.GetLabelForUnit(
                fo.GetUnitTypeId());
        }

        public static (bool isCentral, bool hasWorksets) CheckCentralAndWorksets(Document doc)
        {
            if (!doc.IsWorkshared)
                return (false, false);

            bool isCentral = doc.IsWorkshared;

            bool hasWorksets =
                new FilteredWorksetCollector(doc)
                    .OfKind(WorksetKind.UserWorkset)
                    .Any();

            return (isCentral, hasWorksets);
        }


        public static bool HasPurgeableElements(Document doc)
        {
            // Collect all element types (families, system families)
            var familysymbols = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .ToList();

            foreach (FamilySymbol s in familysymbols)
            {
                var instances = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType()
                    .Where(e => ((FamilyInstance)e).Symbol.Id == e.Id);

                if (!instances.Any())
                    return true;
            }

            // No purgeable elements found
            return false;
        }

        public static double GetRevitFileSizeMB(Document doc)
        {
            string path = doc.PathName;

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                return 0.0; // File not saved yet

            var fileInfo = new System.IO.FileInfo(path);

            // Convert bytes → megabytes
            double sizeMB = fileInfo.Length / (1024.0 * 1024.0);

            // Round to 2 decimals for reporting
            return Math.Round(sizeMB, 2);
        }

        public static bool HasDuplicatedElements(Document doc, double tolerance = 0.001)
        {
            // We'll check all elements that have a location point
            var elements = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .Where(e => e.Location is LocationPoint)
                .ToList();

            // Use a dictionary to group by (Category + TypeId)
            var groups = elements.GroupBy(e => new
            {
                CategoryId = e.Category?.Id.IntegerValue ?? -1,
                TypeId = e.GetTypeId().IntegerValue
            });

            foreach (var group in groups)
            {
                var points = group
                    .Select(e => ((LocationPoint)e.Location).Point)
                    .ToList();

                // Compare all pairs in the group
                for (int i = 0; i < points.Count; i++)
                {
                    for (int j = i + 1; j < points.Count; j++)
                    {
                        if (points[i].DistanceTo(points[j]) <= tolerance)
                        {
                            // Found two elements at same location with same type
                            return true;
                        }
                    }
                }
            }

            // No duplicates found
            return false;
        }

    }
}
