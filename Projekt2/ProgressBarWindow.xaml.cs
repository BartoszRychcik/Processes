using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Projekt2
{
    
    public partial class ProgressBarWindow : Window
    {
        private int timeout;
        private Process process;
        public ProgressBarWindow(int tm, Process p)
        {
            timeout = tm;
            process = p;
            InitializeComponent();
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.DoWork += DoWork;
            worker.ProgressChanged += ProgressChanged;
            worker.RunWorkerAsync();
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Bar.Value = e.ProgressPercentage;
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            worker.ReportProgress(0, "");
            for (int i = 0; i < timeout*10; i++)
            {
                double s = 100.0 / (timeout * 10.0);
                Thread.Sleep(100);
                worker.ReportProgress((int)((i + 1) *s), "");
            }
            worker.ReportProgress(100, "");
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                process.Kill();
            }
            catch
            { }
            this.Close();
        }

    }
}
