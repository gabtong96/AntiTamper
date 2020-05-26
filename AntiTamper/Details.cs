using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiTamper
{
    public class Details
    {
        private string LogDirectoryValue;
        private string ReportNameValue;
        private string CompanyValue;
        private string VersionValue;

        public string LogDirectory
        {
            get { return LogDirectoryValue; }
            set { LogDirectoryValue = value; }
        }
        public string ReportName
        {
            get { return ReportNameValue; }
            set { ReportNameValue = value; }
        }
        public string Company
        {
            get { return CompanyValue; }
            set { CompanyValue = value; }
        }
        public string Version
        {
            get { return VersionValue; }
            set { VersionValue = value; }
        }
    }
}
