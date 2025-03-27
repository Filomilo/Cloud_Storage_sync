using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Test
{
    class TestHelpers
    {
        public static string ExampleDataDirectory =
            AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))
            + "testData\\";

        /// <summary>
        /// Clean after usage
        /// </summary>
        public static string TmpDirecotry =
            AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))
            + "tmp\\";
    }
}
