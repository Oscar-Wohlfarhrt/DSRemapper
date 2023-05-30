
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DSRemapper.DualShock.NativeMethods;

namespace DSRemapper.HID
{

    internal class DSHidDevice
    {
        private const bool defaultExclusiveMode = false;
        #region DSHidDevice


        #endregion DSHidDevice

        private static Guid hidClassGuid = Guid.Empty;
        private static Guid HidClassGuid
        {
            get
            {
                if (hidClassGuid.Equals(Guid.Empty)) HidD_GetHidGuid(ref hidClassGuid);
                return hidClassGuid;
            }
        }

        

    }
    
}
