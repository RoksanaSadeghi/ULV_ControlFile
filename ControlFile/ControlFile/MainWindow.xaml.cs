using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using Ookii.Dialogs.Wpf;
using Microsoft.Win32;


namespace ControlFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string[] possibleLoc = new string[] { "-3", "-2", "-1", "0", "1", "2", "3" };
        static Random rnd = new();

        public MainWindow()
        {
            InitializeComponent();
            Visual_checkbox.IsChecked = true;
        }

        private static bool CheckInput(string str, out int numInt)
        {
            double num = 0;
            if (!double.TryParse(str,out num) || Double.IsNaN(num) || Double.IsInfinity(num))
            {
                MessageBox.Show("Invalid input, please try again.");
                numInt = 0;
                return false;
            }
            
            numInt = Convert.ToInt32(num);
            return true;
        }

        private static List<string> CreatePossibleTrials(string mode, int numTrials)
        {
            List<string> listTrials = new List<string>();

            if (mode.Equals("Visual"))
            {
                for (int i = 0; i < numTrials; i++)
                {
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        listTrials.Add("0,,," + possibleLoc[j]+",,");
                    }
                }
            }

            if (mode.Equals("Auditory"))
            {
                for (int i = 0; i < numTrials; i++)
                {
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        listTrials.Add(",0,," + ","+possibleLoc[j] + ",");
                    }
                }
            }

            if (mode.Equals("Haptic"))
            {
                for (int i = 0; i < numTrials; i++)
                {
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        listTrials.Add(",,0," + ",," + possibleLoc[j]);
                    }
                }
            }

            if (mode.Equals("VisualAuditory"))
            {
                string tmp = "0";
                for (int i = 0; i < numTrials; i++)
                {
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        if (rnd.NextDouble() < 0.5) { tmp = possibleLoc[(possibleLoc.Length-1) - j]; }
                        else { tmp = "0"; }
                        listTrials.Add("0,0,," + possibleLoc[j] + "," + tmp + ",");
                    }
                }

            }

            if (mode.Equals("VisualHaptic"))
            {
                string tmp = "0";
                for (int i = 0; i < numTrials; i++)
                {
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        if (rnd.NextDouble() < 0.5) { tmp = possibleLoc[(possibleLoc.Length-1) - j]; }
                        else { tmp = "0"; }
                        listTrials.Add("0,,0," + possibleLoc[j] + ",," + tmp);
                    }
                }
            }

            if (mode.Equals("AuditoryHaptic"))
            {
                string tmp = "0";
                for (int i = 0; i < numTrials; i++)
                {
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        if (rnd.NextDouble() < 0.5) { tmp = possibleLoc[(possibleLoc.Length-1) - j]; }
                        else { tmp = "0"; }
                        listTrials.Add(",0,0," + "," + tmp + "," + possibleLoc[j]);
                    }
                }
            }

            if (mode.Equals("VisualAuditoryHaptic"))
            {

                List<string> listtmp = new List<string>();
                for (int i = 0; i < numTrials; i++)
                {
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        listtmp.Add(possibleLoc[j]);
                    }
                }
                
                string tmp = "0";
                for (int i = 0; i < numTrials; i++)
                {
                    List<string> randHap = Randomize(listtmp);
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        if (rnd.NextDouble() < 0.5) { tmp = possibleLoc[(possibleLoc.Length-1) - j]; }
                        else { tmp = "0"; }
                        listTrials.Add("0,0,0," + possibleLoc[j] + "," + tmp + "," + randHap[i+j]);
                    }
                }
            }

            return listTrials;
        } 

        private static List<string> Randomize(List<string> input)
        {
            List<string> output = new List<string>();

            Dictionary<int, int> randIndx = new();

            int n = input.Count;
            int r;
            for (int i = 0; i < n; i++)
            {
                r = rnd.Next(n);
                while (randIndx.ContainsKey(r))
                {
                    r = rnd.Next(n);
                }
                if (!randIndx.ContainsKey(r))
                {
                    randIndx.Add(r, i);
                    output.Add(input[r]); 
                }

            }

            return output;
        }

        private static string SelectSavingDirectory()
        {
            string output_directory = "";
            //var ookiiDialog = new VistaFolderBrowserDialog();
            //if (ookiiDialog.ShowDialog() == true)
            //{
            //    output_directory = ookiiDialog.SelectedPath;
            //}

            SaveFileDialog dlg = new();
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    output_directory = dlg.FileName; 
                }
                catch { MessageBox.Show("Error: No filename selected."); }

            }
            else { MessageBox.Show("Error: No filename selected."); }

            return output_directory;
        }

        public static void WriteLog2File(List<string> inputList, string filePath,string mode)
        {

            string txt = "";

            for (int i = 0; i < inputList.Count; i++)
            {
                txt = txt + inputList[i] + "\n";
            }
            txt = txt.Trim('\n');

            if (!File.Exists(filePath))
            {
                try
                {
                    using StreamWriter file = File.CreateText(filePath);
                    file.Write(txt);
                    MessageBox.Show("The file created in: " + filePath);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error creating the file for " + mode + " mode");
                }
            }
        }

        private void Generate_button_Click(object sender, RoutedEventArgs e)
        {
            int numTrials;
            if (!CheckInput(NumTrials_textbox.Text, out numTrials)) { return; };

            string mode = "";
            if(!SelectedMode(out mode)) { return; };

            List<string> randomOrder = Randomize(CreatePossibleTrials(mode, numTrials));
            string filePath = SelectSavingDirectory().Replace(".txt","") + "_controlFile_" + mode + ".txt";
            WriteLog2File(randomOrder, filePath, mode);
        }

        private bool SelectedMode(out string mode)
        {
            mode = "";
            if (Visual_checkbox.IsChecked == true && Auditory_checkbox.IsChecked == false && Haptic_checkbox.IsChecked == false)
            {
                mode = "Visual";
            }
            else if (Visual_checkbox.IsChecked == false && Auditory_checkbox.IsChecked == true && Haptic_checkbox.IsChecked == false)
            {
                mode = "Auditory";
            }
            else if (Visual_checkbox.IsChecked == false && Auditory_checkbox.IsChecked == false && Haptic_checkbox.IsChecked == true)
            {
                mode = "Haptic";
            }
            else if (Visual_checkbox.IsChecked == true && Auditory_checkbox.IsChecked == true && Haptic_checkbox.IsChecked == false)
            {
                mode = "VisualAuditory";
            }
            else if (Visual_checkbox.IsChecked == true && Auditory_checkbox.IsChecked == false && Haptic_checkbox.IsChecked == true)
            {
                mode = "VisualHaptic";
            }
            else if (Visual_checkbox.IsChecked == false && Auditory_checkbox.IsChecked == true && Haptic_checkbox.IsChecked == true)
            {
                mode = "AuditoryHaptic";
            }
            else if (Visual_checkbox.IsChecked == true && Auditory_checkbox.IsChecked == true && Haptic_checkbox.IsChecked == true)
            {
                mode = "VisualAuditoryHaptic";
            }
            else
            {
                MessageBox.Show("Invalid input, please select a mode.");
            }

            return (!mode.Equals(""));
        }


    }
}
