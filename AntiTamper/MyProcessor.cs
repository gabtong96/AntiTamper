using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows;

namespace AntiTamper
{
    public class MyProcessor
    {
        private List<string> NameValues = new List<string>();
        private List<string> SignatureValues = new List<string>();
        private List<string> ModifyValues = new List<string>();
        private List<string> DateValues = new List<string>();
        private List<string> VerdictValues = new List<string>();
        private List<string> VersionValues = new List<string>();
        private string DirNameValue;
        private string SummaryFile;
        private string TCRFile;
        private string SigFile;
        private Details detailvalue;

        public List<string> Name
        {
            get { return NameValues; }
            set { NameValues = value; }
        }
        public List<string> Signature
        {
            get { return SignatureValues; }
            set { SignatureValues = value; }
        }
        public List<string> Modify
        {
            get { return ModifyValues; }
            set { ModifyValues = value; }
        }
        public List<string> Date
        {
            get { return DateValues; }
            set { DateValues = value; }
        }
        public List<string> Verdict
        {
            get { return VerdictValues; }
            set { VerdictValues = value; }
        }
        public List<string> Version
        {
            get { return VersionValues; }
            set { VersionValues = value; }
        }
        public string Summary
        {
            get { return SummaryFile; }
            set { SummaryFile = value; }
        }
        public string TCR
        {
            get { return TCRFile; }
            set { TCRFile = value; }
        }
        public string Sig
        {
            get { return SigFile; }
            set { SigFile = value; }
        }
        public Details detail
        {
            get { return detailvalue; }
            set { detailvalue = value; }
        }

        public String Dirname
        {
            get { return DirNameValue; }
            set { DirNameValue = value; }
        }

        public void ProcessFiles()
        {
            string[] subdirs = Directory.GetDirectories(Dirname, "*.*", SearchOption.AllDirectories);           //get all directories
            bool isTTCNfolder = false;
            bool isTTCN = false;
            bool isMLAPI = false;
            List<string> TTCNdir = new List<string>();
            for (int x = 0; x < subdirs.Length; x++)
            {
                string[] subdirfiles = Directory.GetFiles(subdirs[x], "*.*", SearchOption.TopDirectoryOnly);    //get folder in each directory
                foreach (var file in subdirfiles)
                {
                    if (file.EndsWith("testsuite.tsp"))
                    {
                        TTCNdir.Add(subdirs[x]);                                                                //put all TTCN folder into list
                    }
                }
            }
            FileUnzipper zip = new FileUnzipper();
            zip.Unzip(TTCNdir);
            for (int x = 0; x < subdirs.Length; x++)
            {
                foreach (var dir in TTCNdir)                                                                    //compare all files against TTCNfiles
                {
                    if (dir == subdirs[x])                                                                      //seperate the TTCN from MLAPI and isolates the top folder
                    {
                        isTTCN = true;
                    }
                    if (subdirs[x].Contains(dir))
                    {
                        isTTCNfolder = true;
                    }
                }
                if (isTTCN && isTTCNfolder)                                                     //only search from top TTCNfolder (signalling logs)
                {
                    Summary = FindFile(new DirectoryInfo(subdirs[x]), "SummaryReport");
                    TCR = FindFile(new DirectoryInfo(subdirs[x]), "TestCaseReport.tcr");
                    Sig = FindFile(new DirectoryInfo(subdirs[x]), "TestCaseReport.tcr.sig");
                }
                else if (!isTTCNfolder)                                                         //test whether folders are MLAPI
                {
                    TCR = FindFile(new DirectoryInfo(subdirs[x]), "TestCaseReport.tcr");
                    Sig = FindFile(new DirectoryInfo(subdirs[x]), "TestCaseReport.tcr.sig");
                    if (!string.IsNullOrEmpty(TCR) && !string.IsNullOrEmpty(Sig))
                    {
                        isMLAPI = true;
                    }
                }
                if (isTTCN)
                {
                    ProcessTTCN();
                }
                else if (isMLAPI)
                {
                    ProcessMLAPI();
                }
                isMLAPI = false;
                isTTCN = false;
            }
        }

        public string FindFile(DirectoryInfo root, string filename)
        {
            FileInfo[] files = null;
            var win = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            try                                             //get all files
            {
                files = root.GetFiles("*.*");
            }
            catch
            {
                win.Debug.Text = win.Debug.Text + "Can't acess files\n";
            }


            if (files != null)
            {
                foreach (FileInfo fi in files)    //check against the files and return value
                {
                    string end = fi.Name;
                    if (end.Contains(filename))
                    {
                        return fi.FullName;
                    }
                }
            }
            return "";
        }

        public void ProcessTTCN()
        {
            string verdictcheck = "NONE";
            string inputdate;
            string inputverdict;
            string inputversion;
            int modifycount = 0;
            List<string> filecontent = new List<string>();
            try
            {
                filecontent = File.ReadAllLines(TCR).ToList<string>();        //create list and put each line into it
            }
            catch
            {
                return;
            }
            for (int i = 0; i < filecontent.Count; i++)
            {
                if (filecontent[i].Contains("NOT VALID for UE certification"))              //search through list for all the given values
                {
                    modifycount = modifycount | 1;
                }
                if (filecontent[i].Contains("NTT docomo DST"))
                {
                    int index = filecontent[i].IndexOf("NTT docomo DST");
                    string removed = filecontent[i].Remove(0, index);
                    string[] nospace = removed.Split(' ');
                    inputversion = nospace[3];
                    inputversion = inputversion.Remove(0, 1);
                    if (string.IsNullOrEmpty(detail.Version))
                    {
                        Version.Add(inputversion);
                    }
                    else if (detail.Version.Contains(inputversion))
                    {
                        Version.Add("");
                    }
                    else
                    {
                        Version.Add("NG");
                    }
                }
                if (filecontent[i].Contains("Final Verdict: PASS"))
                {
                    verdictcheck = "PASS";
                }
                else if (filecontent[i].Contains("Final Verdict: FAIL"))
                {
                    verdictcheck = "FAIL";
                }
                else if (filecontent[i].Contains("Final Verdict: INCONC"))
                {
                    verdictcheck = "INCONC";
                }
            }
            try
            {
                filecontent = File.ReadAllLines(Summary).ToList<string>();
            }
            catch
            {
                return;
            }
            for (int x = 0; x < filecontent.Count; x++)
            {
                if (filecontent[x].Contains("Test:"))
                {
                    string[] temp = filecontent[x].Split('.');
                    Name.Add(temp[temp.Length - 1]);
                    int listlength = Name.Count;
                }
                if (filecontent[x].Contains("Result:"))
                {
                    string[] temp = filecontent[x].Split(':');
                    inputverdict = temp[temp.Length - 1].Trim();
                    if (verdictcheck != "")
                    {
                        if (inputverdict.Contains("PASS") && !verdictcheck.Contains("PASS"))
                        {
                            Verdict.Add("NONE");
                        }
                        else
                        {
                            Verdict.Add(inputverdict);
                        }
                    }
                    else
                    {
                        Verdict.Add(inputverdict);
                    }
                }
                if (filecontent[x].Contains("Date:"))
                {
                    if (filecontent[x].Contains('/'))
                    {
                        string[] temp = filecontent[x].Split(':');
                        inputdate = temp[temp.Length - 1].Trim();
                        temp = inputdate.Split('/');
                        inputdate = temp[2] + '-' + temp[0] + '-' + temp[1];
                        Date.Add(inputdate);
                    }
                }
                if (filecontent[x].Contains("Invalid for UE certification"))
                {
                    modifycount = modifycount | 1;
                }
                if (filecontent[x].Contains("ENGINEERING"))
                {
                    modifycount = modifycount | 2;
                }
            }

            if (modifycount == 1 || modifycount == 3)
            {
                Modify.Add("MODIFIED");
            }
            else
            {
                Modify.Add(" ");
            }

            if (!string.IsNullOrEmpty(Sig))
            {
                SignatureVerifier versig = new SignatureVerifier(Sig);
                bool valid = versig.cmdverify();

                if (valid)
                {
                    Signature.Add("VALID");
                }
                else
                {
                    Signature.Add("INVALID");
                }
            }

        }

        public void ProcessMLAPI()
        {
            string inputname;
            string inputdate;
            string inputverdict;
            string inputversion;
            int modifycount = 0;
            List<string> filecontent = new List<string>();
            if (TCR.EndsWith(".sig"))
            {
                //do nothing
            }
            else
            {
                try
                {
                    filecontent = File.ReadAllLines(TCR).ToList<string>();       //create list to put each line into it
                }
                catch
                {
                    return;
                }
                for (int x = 0; x < filecontent.Count; x++)
                {
                    if (filecontent[x].Contains("TCR_TestCaseReport name"))                     //go through list to find values and put them in
                    {
                        string[] temp = filecontent[x].Split('=');
                        inputname = temp[1];
                        inputname = inputname.Substring(1, inputname.Length - 3);
                        string firstletter = inputname.Substring(0, 1);
                        if (firstletter != "I")
                        {
                            {
                                if (inputname.Length > 22)
                                {
                                    Name.Add(inputname.Substring(0, 22));
                                }
                                else
                                {
                                    Name.Add(inputname);
                                }
                            }
                        }
                    }
                    if (filecontent[x].Contains("TCR_ExecutionDate value"))
                    {
                        string[] temp = filecontent[x].Split('=');
                        inputdate = temp[1];
                        inputdate = inputdate.Substring(1, inputdate.Length - 4);
                        if (inputdate.Length > 10)
                        {
                            Date.Add(inputdate.Substring(0, inputdate.Length - 1));
                        }
                        else
                        {
                            Date.Add(inputdate);
                        }
                    }
                    if (filecontent[x].Contains("TCR_Verdict value"))
                    {
                        string[] temp = filecontent[x].Split('=');
                        inputverdict = temp[1];
                        inputverdict = inputverdict.Substring(1, inputverdict.Length - 4);
                        if (inputverdict.Length > 4)
                        {
                            Verdict.Add(inputverdict.Substring(0, inputverdict.Length - 1));
                        }
                        else
                        {
                            Verdict.Add(inputverdict);
                        }
                    }
                    if (filecontent[x].Contains("NOT VALID for UE certification"))
                    {
                        modifycount = modifycount | 1;
                    }
                    if (filecontent[x].Contains("ENGINEERING BuildID"))
                    {
                        modifycount = modifycount | 2;
                    }
                    if (modifycount == 1 || modifycount == 3)
                    {
                        Modify.Add("MODIFIED");
                    }
                    else
                    {
                        Modify.Add(" ");
                    }
                    if (filecontent[x].Contains("NTT DOCOMO DST"))
                    {
                        int index = filecontent[x].IndexOf("NTT DOCOMO DST");
                        string removed = filecontent[x].Remove(0, index);
                        string[] nospace = removed.Split(' ');
                        inputversion = nospace[3];
                        inputversion = inputversion.Remove(0, 1);
                        if (string.IsNullOrEmpty(detail.Version))
                        {
                            Version.Add(inputversion);
                        }
                        else if (detail.Version == inputversion)
                        {
                            Version.Add("");
                        }
                        else
                        {
                            Version.Add("NG");
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(Sig))
            {
                SignatureVerifier versig = new SignatureVerifier(Sig);
                bool valid = versig.cmdverify();

                if (valid)
                {
                    Signature.Add("VALID");
                }
                else
                {
                    Signature.Add("INVALID");
                }
            }
        }
    }
}
