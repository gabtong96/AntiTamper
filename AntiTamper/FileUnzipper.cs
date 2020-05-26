using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Windows;

namespace AntiTamper
{
    public class FileUnzipper
    {
        public void Unzip(List<string> zipfiles)                       //unzip zipfiles
        {
            FastZip fastzip = new FastZip();
            int zipcount;
            var win = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            
            foreach (var file in zipfiles)
            {
                string zipname = file + "\\" + "SignallingLogs.zip";
                if (File.Exists(zipname))                                       //check to see zipfile exists
                {
                    ZipFile zip = new ZipFile(zipname);
                    zipcount = Convert.ToInt32(zip.Count);
                    int filecount = Directory.GetFiles(file, "*", SearchOption.AllDirectories).Length;
                    if (filecount < zipcount)                                                               //if unzip already
                    {
                        try
                        {
                            fastzip.ExtractZip(zipname, file, null);
                            win.Debug.Text = win.Debug.Text + zipname + " extracted\n";
                        }
                        catch
                        {
                            win.Debug.Text = win.Debug.Text + "Couldn't extract " + zipname + "\n";
                        }
                    }
                }
                string signalname = file + "\\" + "SignallingLogs";
                if (Directory.Exists(signalname))                                   //check to see zipfile exists
                {
                    string[] dirfiles = Directory.GetFiles(signalname, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (string fileName in dirfiles)                                                        //if unzip already
                    {
                        if (fileName.EndsWith("zip"))
                        {
                            Console.WriteLine("Unzipping Files");
                            ZipFile zip = new ZipFile(fileName);
                            try
                            {
                                fastzip.ExtractZip(fileName, signalname, "TestCaseReport.tcr");
                                fastzip.ExtractZip(fileName, signalname, "TestCaseReport.tcr.sig");
                            }
                            catch
                            {
                                win.Debug.Text = win.Debug.Text + "Couldn't extract " + zipname + "\n";
                            }
                        }
                    }
                }
                string tcrlocation = FindFile(new DirectoryInfo(file), "TestCaseReport.tcr");
                string siglocation = FindFile(new DirectoryInfo(file), "TestCaseReport.tcr.sig");
                Console.WriteLine(tcrlocation);
                Console.WriteLine(siglocation);
                string tcrdest = Path.Combine(file, "TestCaseReport.tcr");
                string sigdest = Path.Combine(file, "TestCaseReport.tcr.sig");
                try
                {
                    File.Copy(tcrlocation, tcrdest, true);
                    File.Copy(siglocation, sigdest, true);
                }
                catch
                {
                    
                }
            }
        }

        public string FindFile(DirectoryInfo root, string filename)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subdirs = null;
            var win = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            try                                             //get all files
            {
                files = root.GetFiles("*.*");
            }
            catch
            {
                win.Debug.Text = win.Debug.Text + "Can't acess files\n";
                //Debug.Text = Debug.Text + "Can't access files\n";
            }


            if (files != null)
            {
                foreach (FileInfo fi in files)    //check against the files and return value
                {
                    string end = fi.Name;
                    if (end.Contains(filename))
                    {
                        //Console.Write(fi.FullName + "\n");
                        return fi.FullName;
                    }
                }

                subdirs = root.GetDirectories();

                foreach (DirectoryInfo dirInfo in subdirs)     //access each subdir recursively
                {
                    string final = FindFile(dirInfo, filename);
                    if (final != "")
                    {
                        return final;
                    }
                }
            }
            return "";
        }

    }
}
