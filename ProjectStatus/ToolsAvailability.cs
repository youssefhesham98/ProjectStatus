using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectStatus
{
    public class ToolsAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication app, CategorySet categories)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\EDECS\Tools");
            if (key == null) return false;

            string value = key.GetValue("IsRegistered", "0").ToString();
            return value == "MEP09691469*/";
            //return RegistrationState.IsRegistered;
        }
    }
}
