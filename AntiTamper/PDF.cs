using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;
using System.Diagnostics;
using System.Windows;

namespace AntiTamper
{
    public class PDF
    {
        private MyProcessor processorvalues;
        private PdfDocument pdfvalues = new PdfDocument();
        private string titlevalue;
        private string savenamevalue;
        private Details detailvalue;
        private int pagecountvalue = 0;
        
        public MyProcessor proc
        {
            get { return processorvalues; }
            set { processorvalues = value; }
        }
        public PdfDocument mypdf
        {
            get { return pdfvalues; }
            set { pdfvalues = value; }
        }
        public string title
        {
            get { return titlevalue; }
            set { titlevalue = value; }
        }
        public string savename
        {
            get { return savenamevalue; }
            set { savenamevalue = value; }
        }
        public Details detail
        {
            get { return detailvalue; }
            set { detailvalue = value; }
        }
        public int pagecount
        {
            get { return pagecountvalue; }
            set { pagecountvalue = value; }
        }

        
        public void AddReportPage()
        {
            var win = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;

            PdfPage page = mypdf.AddPage();       //pdf settings
            page.Size = PageSize.A4;
            XGraphics graph = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Helvitica", 10);
            XPen pen = new XPen(XColors.Black, 1);

            int ogheight = 100;
            int spacing = 22;
            int maxheight = 800;
            int boxes = (maxheight - ogheight) / spacing - 1;

            for (int x = 0; x < proc.Name.Count + 1; x++)   //draw rectangles
            {
                if (ogheight + (x+1)*spacing <= maxheight)
                {
                    XRect rect1 = new XRect(50, ogheight + x * spacing, 45, spacing);
                    XRect rect2 = new XRect(95, ogheight + x * spacing, 150, spacing);
                    XRect rect3 = new XRect(245, ogheight + x * spacing, 65, spacing);
                    XRect rect4 = new XRect(310, ogheight + x * spacing, 60, spacing);
                    XRect rect5 = new XRect(370, ogheight + x * spacing, 60, spacing);
                    XRect rect6 = new XRect(430, ogheight + x * spacing, 65, spacing);
                    XRect rect7 = new XRect(495, ogheight + x * spacing, 50, spacing);
                    graph.DrawRectangle(pen, rect1);
                    graph.DrawRectangle(pen, rect2);
                    graph.DrawRectangle(pen, rect3);
                    graph.DrawRectangle(pen, rect4);
                    graph.DrawRectangle(pen, rect5);
                    graph.DrawRectangle(pen, rect6);
                    graph.DrawRectangle(pen, rect7);

                    if (x == 0)
                    {
                        graph.DrawString("NO.", font, XBrushes.Black, rect1, XStringFormats.BottomCenter);
                        graph.DrawString("TESTCASE NAME", font, XBrushes.Black, rect2, XStringFormats.BottomCenter);
                        graph.DrawString("SIGNATURE", font, XBrushes.Black, rect3, XStringFormats.BottomCenter);
                        graph.DrawString("DATE", font, XBrushes.Black, rect4, XStringFormats.BottomCenter);
                        graph.DrawString("MODIFY", font, XBrushes.Black, rect5, XStringFormats.BottomCenter);
                        graph.DrawString("VERDICT", font, XBrushes.Black, rect6, XStringFormats.BottomCenter);
                        graph.DrawString("VERSION", font, XBrushes.Black, rect7, XStringFormats.BottomCenter);
                    }
                    else                                                                                                // fill in info of rows below
                    {
                        try
                        {
                            graph.DrawString((x + boxes*pagecount).ToString(), font, XBrushes.Black, rect1, XStringFormats.Center);
                            graph.DrawString(proc.Name[x - 1], font, XBrushes.Black, rect2, XStringFormats.Center);
                            graph.DrawString(proc.Signature[x - 1], font, XBrushes.Black, rect3, XStringFormats.Center);
                            graph.DrawString(proc.Date[x - 1], font, XBrushes.Black, rect4, XStringFormats.Center);
                            graph.DrawString(proc.Modify[x - 1], font, XBrushes.Black, rect5, XStringFormats.Center);
                            graph.DrawString(proc.Verdict[x - 1], font, XBrushes.Black, rect6, XStringFormats.Center);
                            graph.DrawString(proc.Version[x - 1], font, XBrushes.Black, rect7, XStringFormats.Center);
                        }
                        catch
                        {
                            win.Debug.Text = win.Debug.Text + x + " Value Error\n";
                        }
                    }
                }
                else
                {
                    proc.Name.RemoveRange(0, boxes);
                    proc.Signature.RemoveRange(0, boxes);
                    proc.Date.RemoveRange(0, boxes);
                    proc.Modify.RemoveRange(0, boxes);
                    proc.Verdict.RemoveRange(0, boxes);
                    proc.Version.RemoveRange(0, boxes);
                    pagecount += 1;
                    this.AddReportPage();
                }
                
            }
            graph.DrawString(title, font, XBrushes.Black, 315, 80);
            System.Drawing.Bitmap logo = Properties.Resources.R_SLogo;
            MemoryStream strm = new MemoryStream();
            logo.Save(strm, System.Drawing.Imaging.ImageFormat.Png);
            XImage image = XImage.FromStream(strm);
            graph.DrawImage(image, 60, 20, 250, 80);

            proc.Name.Clear();
            proc.Signature.Clear();
            proc.Date.Clear();
            proc.Modify.Clear();
            proc.Verdict.Clear();
            proc.Version.Clear();
            pagecount = 0;
        }

        public void AddCoverPage()
        {
            PdfPage page = mypdf.AddPage();
            page.Size = PageSize.A4;
            XGraphics cover = XGraphics.FromPdfPage(page);
            XFont bigfont = new XFont("Arial", 35, XFontStyle.Bold);
            XPen pen = new XPen(XColors.Black, 1);

            cover.DrawString("Test Case Report", bigfont, XBrushes.Black, new XRect(0, 0, page.Width.Point, page.Height.Point / 1.5), XStringFormats.Center);

            int row = 3;
            int ogheight = 525;
            int spacing = 75;
            for (int x = 0; x < row; x++)                       //draw rectangles and details
            {
                XRect rect1 = new XRect(75, ogheight + x * spacing, 150, spacing);
                XRect rect2 = new XRect(225, ogheight + x * spacing, 285, spacing);
                cover.DrawRectangle(pen, rect1);
                cover.DrawRectangle(pen, rect2);
                XFont boxfont = new XFont("Helvetica", 15);
                if (x == 0)
                {
                    cover.DrawString("DST Version", boxfont, XBrushes.Black, rect1, XStringFormats.Center);
                    cover.DrawString(detail.Version, boxfont, XBrushes.Black, rect2, XStringFormats.Center);
                }
                else if (x == 1)
                {
                    cover.DrawString("Company", boxfont, XBrushes.Black, rect1, XStringFormats.Center);
                    cover.DrawString(detail.Company, boxfont, XBrushes.Black, rect2, XStringFormats.Center);
                }
                else if (x == 2)
                {
                    cover.DrawString("Date", boxfont, XBrushes.Black, rect1, XStringFormats.Center);
                    cover.DrawString(DateTime.UtcNow.Date.ToString("yyyy-MM-dd") + "    (YYYY-MM-DD)", boxfont, XBrushes.Black, rect2, XStringFormats.Center);
                }
            }
            System.Drawing.Bitmap logo = Properties.Resources.R_SLogo;
            MemoryStream strm = new MemoryStream();
            logo.Save(strm, System.Drawing.Imaging.ImageFormat.Png);
            XImage image = XImage.FromStream(strm); cover.DrawImage(image, 60, 20, 250, 80);
        }

        public void SavePDF()
        {
            mypdf.Save(savename);
        }

        public void OpenPDF()
        {
            Process.Start(savename);
        }

        public void AttachKeyword(string keyword)
        {
            mypdf.Info.Keywords = keyword;
            this.SavePDF();
        }
    }
}
