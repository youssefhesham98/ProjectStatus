using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace ProjectStatus
{
    public class ExEvt : IExternalEventHandler
    {
        public Request request { get; set; }
        public void Execute(UIApplication app)
        {
            switch (request)
            {
                case Request.XXXX:
                  
                    break;
            }
        }

        public string GetName()
        {
            return "EDECS Toolkit";
        }
        public enum Request
        {
            XXXX,
        }
    }
}
