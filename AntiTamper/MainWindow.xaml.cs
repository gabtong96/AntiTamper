using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Diagnostics;

namespace AntiTamper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Details detail = new Details();             //settings box details
        string path;                                //directory path
        string[] groupname = new string[] { "LTE", "UMTS", "EM", "VoLTE", "Advanced", "DCMSpec", "DCMVoLTE", "DRFL" };
        string exeloc = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            detail.ReportName = "TestReport";
        }

        private void Browse_Files(object sender, RoutedEventArgs e)     //Open Browse Folder
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog openFolderDlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (openFolderDlg.ShowDialog(this).GetValueOrDefault())
            {
                this.path = openFolderDlg.SelectedPath;
                this.detail.LogDirectory = openFolderDlg.SelectedPath;
            }

            if (!string.IsNullOrEmpty(detail.LogDirectory))
            {
                Debug.Text = Debug.Text + "Selected folder is: " + detail.LogDirectory + "\n";
            }

        }

        private void Open_Settings(object sender, RoutedEventArgs e)    //Open Settings Page and keep settings
        {
            Settings set = new Settings(detail);
            set.Owner = this;
            set.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            set.ShowDialog();
            detail.LogDirectory = set.detail.LogDirectory;
            detail.ReportName = set.detail.ReportName;
            detail.Company = set.detail.Company;
            detail.Version = set.detail.Version;
            if (!string.IsNullOrEmpty(detail.ReportName) || !string.IsNullOrEmpty(detail.Company) || !string.IsNullOrEmpty(detail.Version))
            {
                Debug.Text = Debug.Text + "Settings inputted are as follows: \n";
                Debug.Text = Debug.Text + "Report Name: " + detail.ReportName + "\n";
                Debug.Text = Debug.Text + "Company: " + detail.Company + "\n";
                Debug.Text = Debug.Text + "Version: " + detail.Version + "\n";
            }
        }

        private void Generate(object sender, RoutedEventArgs e)         //generate the 2 PDFs
        {
            int multiplepackage = 0;
            MyProcessor processor = new MyProcessor();
            processor.detail = detail;
            PDF testreport = new PDF();
            testreport.proc = processor;
            testreport.detail = detail;

            string time = gettime();

            //check whether all settings are filled up
            if (string.IsNullOrEmpty(detail.ReportName))
            {
                Debug.Text = Debug.Text + "Enter Report Name\n";
            }
            else if(string.IsNullOrEmpty(detail.Company))
            {
                Debug.Text = Debug.Text + "Enter Company\n";
            }
            else if (string.IsNullOrEmpty(detail.LogDirectory))
            {
                Debug.Text = Debug.Text + "Select folder\n";
            }
            else if (!File.Exists(exeloc + "/" + "openssl.exe"))
            {
                Debug.Text = Debug.Text + "Put openssl.exe into folder\n";
            }
            else if (!File.Exists(exeloc + "/" + "public_key.pem"))
            {
                Debug.Text = Debug.Text + "Put public.key_pem in fodler\n";
            }
            else
            {
                Debug.Text = Debug.Text + "Processing files... \n";
                if (Directory.Exists(path))
                {
                    foreach (var x in groupname)                 //check for grouptye (LTE, VoLTE, UMS, etc...)
                    {
                        string tempdir = path + "\\" + x;
                        if (Directory.Exists(tempdir))
                        {
                            processor.Dirname = tempdir;
                            multiplepackage = 1;
                            processor.ProcessFiles();
                            testreport.title = "(GROUP " + x + ")";
                            testreport.AddReportPage();
                        }
                    }
                }
                if (multiplepackage == 0)                       //no grouptype
                {
                    processor.Dirname = path;
                    processor.ProcessFiles();
                    testreport.title = "(" + path + ")";
                    testreport.AddReportPage();
                }

                string testreportsavename = detail.ReportName + time + ".pdf";
                testreportsavename = detail.LogDirectory + "\\" + testreportsavename;
                testreport.savename = testreportsavename;
                try     //try saving pdf
                {
                    testreport.SavePDF();
                    testreport.OpenPDF();
                }
                catch (System.InvalidOperationException)   //send back error
                {
                    Debug.Text = Debug.Text + "File is open";
                    TestReport error = new TestReport();
                    error.Owner = this;
                    error.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    error.ShowDialog();
                }

                Debug.Text = Debug.Text + "PDF Generated\n";

                PDF coverpage = new PDF();
                coverpage.detail = detail;
                coverpage.AddCoverPage();

                string coverpagesavename = detail.ReportName + time + "_CP.pdf";
                coverpagesavename = detail.LogDirectory + "\\" + coverpagesavename;
                coverpage.savename = coverpagesavename;
                try             //try saving PDF
                {
                    coverpage.SavePDF();
                    coverpage.OpenPDF();
                }
                catch           //error shown
                {
                    Debug.Text = Debug.Text + ("File is open\n");
                    CoverPageError error = new CoverPageError();
                    error.Owner = this;
                    error.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    error.ShowDialog();
                }
                MD5HashGenerator mD5Hash = new MD5HashGenerator();
                string hash = mD5Hash.GenerateHash(testreport.savename);
                coverpage.AttachKeyword(hash);

                Process.Start(detail.LogDirectory);
            }

        }

        private string gettime()                                        //obtain current time for savename
        {
            string time = DateTime.Now.ToString("_yyyy-MM-ddThhmmss");
            return time;
        }                       

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Clear_Box(object sender, RoutedEventArgs e)
        {
            Debug.Text = "";
        }
        private void About_Page(object sender, RoutedEventArgs e)
        {
            InfoPage info = new InfoPage();
            info.Owner = this;
            info.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            info.ShowDialog();
        }
        private void Restore_Defaults(object sender, RoutedEventArgs e)
        {
            detail.ReportName = "TestReport";
            detail.LogDirectory = "";
            detail.Company = "";
            detail.Version = "";
            Debug.Text = Debug.Text + "Values restored to default.\n";
        }
        private void Scroll_To_End(object sender, TextChangedEventArgs e)
        {
            Debug.ScrollToEnd();
        }
        
    }
    

}
