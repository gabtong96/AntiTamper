using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows;

namespace AntiTamper
{
    public class SignatureVerifier
    {
        private string sigfilevalue;
        private string exeloc = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
        public string sigfile
        {
            get { return sigfilevalue; }
            set { sigfilevalue = value; }
        }
        public SignatureVerifier(string sigfile)
        {
            this.sigfile = sigfile;
        }

        public bool cmdverify()
        {
            var win = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            string ogfile = sigfile.Substring(0, sigfile.Length - 4);
            string key = exeloc + "\\" + "public_key.pem";
            string command = "openssl dgst -sha256 -verify \"" + key + "\" -signature \"" + sigfile + "\" \"" + ogfile;
            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd");
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardInput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process proc = new Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                if (exeloc[0] == '\\')
                {
                    proc.StandardInput.WriteLine("pushd" + exeloc);
                }
                proc.StandardInput.WriteLine(command);
                proc.StandardInput.Close();
                string result = proc.StandardOutput.ReadToEnd().ToString();

                if (result.Contains("Verified OK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                win.Debug.Text = win.Debug.Text + "CMD Error\n";
                return false;
            }

        }
    }
}
