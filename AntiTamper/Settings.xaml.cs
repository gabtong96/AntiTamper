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
using System.Windows.Shapes;

namespace AntiTamper
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {

        private Details detailvalue = new Details();
        public Settings(Details detail)                                     //get the values when first initialize
        {
            InitializeComponent();
            this.detailvalue = detail;
            LogDirectoryText.Text = detailvalue.LogDirectory;
            ReportNameText.Text = detailvalue.ReportName;
            CompanyText.Text = detailvalue.Company;
            VersionText.Text = detailvalue.Version;
        }

        
        public Details detail
        {
            get { return detailvalue; }
            set { detail = detailvalue; }
        }

        private void Set_Details(object sender, RoutedEventArgs e)         //set the values
        {
            detail.LogDirectory = LogDirectoryText.Text.ToString();
            detailvalue.ReportName = ReportNameText.Text.ToString();
            detailvalue.Company = CompanyText.Text.ToString();
            detailvalue.Version = VersionText.Text.ToString();
            this.Close();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
    }
}
