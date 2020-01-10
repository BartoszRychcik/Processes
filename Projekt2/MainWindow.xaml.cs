using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Projekt2
{
    public partial class MainWindow : Window
    {
        List<Process> items = new List<Process>();
        List<KeyValuePair<Process, ProgressBarWindow>> ownItems = new List<KeyValuePair<Process, ProgressBarWindow>>();

        public MainWindow()
        {
            InitializeComponent();
            GetProcesses();
            AddProcessesToListView();
        }

        private void Combobox_Loaded(object sender, RoutedEventArgs e)
        {
            sorting.SelectedIndex = 0;
        }

        private void GetProcesses()
        {
            items.Clear();
            foreach (Process p in Process.GetProcesses())
            {
                items.Add(p);
            }
        }

        void AddProcessesToListView()
        {
            string propertyName;
            ListSortDirection lsd;
            switch (sorting.SelectedIndex)
            {
                case 0:
                    propertyName = "ProcessName";
                    lsd = ListSortDirection.Ascending;
                    break;
                case 1:
                    propertyName = "ProcessName";
                    lsd = ListSortDirection.Descending;
                    break;
                case 2:
                    propertyName = "Id";
                    lsd = ListSortDirection.Ascending;
                    break;
                case 3:
                    propertyName = "Id";
                    lsd = ListSortDirection.Descending;
                    break;
                case 4:
                    propertyName = "BasePriority";
                    lsd = ListSortDirection.Ascending;
                    break;
                case 5:
                    propertyName = "BasePriority";
                    lsd = ListSortDirection.Descending;
                    break;
                default:
                    propertyName = "ProcessName";
                    lsd = ListSortDirection.Ascending;
                    break;
            }

            processes.ItemsSource = items;
            CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(processes.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(propertyName, lsd));
        }

        public bool Is_Number(string a)
        {
            if (a == null || a.Length == 0)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] < '0' || a[i] > '9')
                    return false;
            }

            return true;
        }

        private void SetTextError(string err)
        {
            warning.Content = err;
        }

        private void ClearFields()
        {
            warning.Content = "";
            modules.list.Items.Clear();
            threads.list.Items.Clear();
            memory.list.Items.Clear(); 
            StartProcess.Text = "";
            priority.Text = "";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetProcesses();
            AddProcessesToListView();
        }

        private void Sorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddProcessesToListView();
        }

        private void Processes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearFields();
            if (processes.SelectedIndex != -1)
            {
                Process p = processes.SelectedItem as Process;
                try
                {
                    for (int i = 0; i < priority.Items.Count; i++)
                    {
                        ComboBoxItem it = (ComboBoxItem) priority.Items[i];

                        if (p.PriorityClass.ToString() == it.Content.ToString())
                        {
                            priority.SelectedIndex = i;
                            break;
                        }

                    }
                }
                catch
                {
                    SetTextError("Błąd podczas ustawiania priorytetu.");
                }


                memory.list.Items.Add("NonPagedSystemMemory = " + p.NonpagedSystemMemorySize64);
                memory.list.Items.Add("PagedMemory = " + p.PagedMemorySize64);
                memory.list.Items.Add("PagedSystemMemory = " + p.PagedSystemMemorySize64);
                memory.list.Items.Add("PrivateMemory = " + p.PrivateMemorySize64);
                memory.list.Items.Add("VirtualMemory = " + p.VirtualMemorySize64);
                memory.list.Items.Add("PhysicalMemory = " + p.WorkingSet64);
                
                try
                {
                    foreach (ProcessModule pm in p.Modules)
                    {
                        modules.list.Items.Add(pm.ModuleName);
                    }

                    StartProcess.Text = p.StartTime.ToString();

                    foreach (ProcessThread pm in p.Threads)
                    {
                        threads.list.Items.Add(pm.Id.ToString() + " " + pm.StartTime.ToString() + " " +
                                          pm.BasePriority.ToString());
                    }
                }
                catch
                {
                    SetTextError("Odmowa dostępu do odczytu.");
                }

            }
        }

        public void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
			bool f = true;
            SetTextError("");
            if (processes.SelectedIndex != -1)
            {
                Process p = processes.SelectedItem as Process;
                try
                {
                    foreach (var x in ownItems)
                    {
                        if (p.Id == x.Key.Id)
                        {
                            x.Value.Close();
                            x.Key.Kill();
							f=false;
                            ownItems.RemoveAt(i);
                            break;
                        }
						if(f)
							p.Kill();
                        i++;
                    }
                }
                catch
                {
                    SetTextError("Nie masz uprawnień by zabić ten proces lub zaznaczony proces już nie istnieje.");
                }
            }
        }

        private void Priority_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetTextError("");
            if (processes.SelectedIndex != -1)
            {
                Process p = processes.SelectedItem as Process;
                try
                {
                    ComboBoxItem it = (ComboBoxItem) priority.SelectedValue;
                    switch (it.Content.ToString())
                    {
                        case "Idle":
                            p.PriorityClass = ProcessPriorityClass.Idle;
                            break;
                        case "Normal":
                            p.PriorityClass = ProcessPriorityClass.Normal;
                            break;
                        case "AboveNormal":
                            p.PriorityClass = ProcessPriorityClass.AboveNormal;
                            break;
                        case "BelowNormal":
                            p.PriorityClass = ProcessPriorityClass.BelowNormal;
                            break;
                        case "High":
                            p.PriorityClass = ProcessPriorityClass.High;
                            break;
                        case "RealTime":
                            p.PriorityClass = ProcessPriorityClass.RealTime;
                            break;
                        default:
                            p.PriorityClass = ProcessPriorityClass.Normal;
                            break;
                    }
                }
                catch
                {
                    SetTextError("Nie masz uprawnień by modyfikować proces lub zaznaczony proces już nie istnieje.");
                }
            }
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SetTextError("");
            string path = filename.Text;
            string t = timeout.Text;
            int tm = 0;
            if (Is_Number(t))
            {
                tm = Int32.Parse(timeout.Text);
                try
                {
                    Process process = new Process();
                    process.StartInfo.FileName = path;
                    process.Start();
                    ProgressBarWindow window = new ProgressBarWindow(tm, process);
                    window.Show();
                    ownItems.Add(new KeyValuePair<Process, ProgressBarWindow>(process, window));
                }
                catch
                {
                    if (File.Exists(path) == false)
                        SetTextError("Podana ścieżka do pliku jest nieprawidłowa.");
                    else
                        SetTextError("Wystąpił błąd podczas otwierania pliku.");
                }
            }
            else
                SetTextError("Podaj timeout.");
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".exe";
            dialog.Filter = "Executable file (.exe)|*.exe";

            if (dialog.ShowDialog() == true)
            {
                filename.Text = dialog.FileName;
            }
        }
    }
}