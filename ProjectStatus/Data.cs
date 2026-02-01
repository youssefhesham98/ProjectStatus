using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

namespace ProjectStatus
{
    
    internal class Data
    {
        public static List<Element> XXXX { get; set; }

        public static void Intialize()
        {
            XXXX = new List<Element>();
        }
    }
}
