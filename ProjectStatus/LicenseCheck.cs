using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ProjectStatus
{
    public class LicenseCheck
    {
        /// <summary>
        /// Represents a collection of unique identifier strings in the form of GUIDs.
        /// </summary>
        /// <remarks>This list contains a predefined set of GUIDs, which can be used for various purposes
        /// such as  identifying resources, tracking entities, or other scenarios requiring unique identifiers. Each
        /// GUID is represented as a string in the standard 8-4-4-4-12 format.</remarks>
        List<string> ids = new List<string>()
        {
            "020A679A-FB97-11ED-8C92-088FC3F60393",
            "C13AAF9A-8EFB-ED11-80EB-40C2BA19CCC3",
            "48848C98-E868-081C-A55A-2CF05D77A6EF",
            "4C4C4544-0050-4A10-8048-B8C04F533633",
            "035E02D8-04D3-0520-CF06-C10700080009",
            "4C4C4544-0036-3510-8054-C2C04F4E3933",
            "52D7D14C-2377-11B2-A85C-813B539C9727",
            "4C4C4544-0031-3010-804E-B9C04F4C4332",
            "03560274-043C-0572-6A06-C70700080009",
            "DE29C7F0-75A8-11E6-9C43-BC0000F40000",
            "90D9CAC8-051C-0819-A5FE-D8BBC19BBC1D",
            "D6B8B380-8438-11E2-8C4C-6C3BE51AA40E",
            "893BBBC7-E998-8067-202A-9FA2FF1AD13E",
            "06B76D4C-2334-11B2-A85C-A1103EAC79D1",
            "79DAFB6B-CE0C-9517-A83B-2CF05D5DCFC5",
            "C71B8500-EDB1-11E2-8DD0-8851FB6BD3A4",
            "4C4C4544-0044-3810-8034-C4C04F363232",
            "02D86F0D-60A0-11E6-9C43-BC0000120000",
            "781FB283-5D8A-11E6-9C43-BC0000880000",
            "592580D0-7EBC-EA1D-AAA3-047C1600A973",
            "36043C9C-74EF-11E6-9C43-BC0000220000",
            "4C4C4544-0043-4810-8034-C6C04F574333",
            "4C4C4544-005A-3910-8047-B6C04F464433",
            "4C4C4544-0056-5610-8058-B5C04F543733",
            "4C4C4544-0036-3510-8054-C7C04F4E3933",
            "AD058D47-AC7C-11E6-9C43-BC00004C0000",
            "31D76C35-298A-CA17-ADAC-D8BBC1A1C5AE",
            "92A67A7D-BFE7-11E6-9C43-BC0000820000",
            "4C4C4544-0038-4E10-8047-C4C04F463733",
            "035E02D8-04D3-0520-CF06-C10700080009",
            "4C4C4544-004B-5610-8058-B2C04F543733",
            "4C4C4544-004D-3510-8054-B3C04F4E3933",
            "C20E740C-8C2A-11E6-9C43-BC0000920000",
            "84695665-6806-081C-A87B-2CF05D77A6E2",
            "4C4C4544-0052-4810-8048-C3C04F533633",
            "0EC14565-9EFA-7F1C-AF7A-047C1600A92C",
            "0F6FE0D7-639F-11EA-8104-7C8AE12E29B9",
            "FA7BE939-0C2E-A517-AD81-2CF05D5DD21D",
            "8BABC85B-E88A-C817-A09D-2CF05D5DD7CE",
            "9B38EACB-A99F-11EC-810F-E4A8DFBF2283",
            "658729A6-A5CE-11ED-8C91-088FC3D9D265",
            "8F32FC47-D17C-11EE-A4F6-40C2BA3B402C",
            "EF6EC2C3-E358-11EE-A4F6-40C2BA4154DC",
            "4C4C4544-0053-5610-8047-CAC04F485832",
            "943D90C7-E400-11EE-A4F6-40C2BA415F3E",
            "0F05E8F1-11B7-11EF-A4F6-40C2BA511622"

        };
        /// <summary>
        /// Checks if the current machine's unique identifier (UUID) is present in the predefined list of IDs.
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            string id = GetMachineID();
            if (ids.Contains(id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Get the machine's unique identifier (UUID) using WMI (Windows Management Instrumentation).
        /// </summary>
        /// <returns></returns>
        public static string GetMachineID()
        {
            string machineID = "";
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystemProduct"))
                collection = searcher.Get();
            foreach (var obj in collection)
            {
                machineID = obj["UUID"].ToString();
                break;
            }
            return machineID;
        }
    }
}
