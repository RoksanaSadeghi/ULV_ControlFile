using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using Ookii.Dialogs.Wpf;
using Microsoft.Win32;
using System.Windows.Automation;

namespace ControlFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static double[] possibleLoc = new double[] { -3, -2, -1, 0, 1, 2, 3 };
        static Random rnd = new();
        int[] mode = new int[3] { 0, 0, 0 };

        public MainWindow()
        {
            InitializeComponent();
            Visual_checkbox.IsChecked = true;
        }

        private static bool CheckInput_Int(string str, out int numInt)
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

        private static bool CheckInput_Double(string str, out double numDouble)
        {
            double num = 0;
            if (!double.TryParse(str, out num) || Double.IsNaN(num) || Double.IsInfinity(num))
            {
                MessageBox.Show("Invalid input, please try again.");
                numDouble = 0;
                return false;
            }

            numDouble = num;
            return true;
        }

        private static List<string> CreatePossibleTrials(string mode, int numTrials, double stimLoc, double stimDiff)
        {
            // apply stimLoc to the possibleLoc
            for (int i = 0; i < possibleLoc.Length; i++)
            {
                possibleLoc[i] = possibleLoc[i] * stimLoc;
            }

            // initiate the trials list
            List<string> listTrials = new List<string>();

            // create trials based on the selected mode
            if (mode.Equals("Visual"))
            {
                for (int i = 0; i < numTrials; i++)
                {
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        listTrials.Add(possibleLoc[j].ToString()+",,");
                    }
                }
            }

            if (mode.Equals("Auditory"))
            {
                for (int i = 0; i < numTrials; i++)
                {
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        listTrials.Add("," + possibleLoc[j].ToString() + ",");
                    }
                }
            }
            
            if (mode.Equals("VisualAuditory"))
            {
                for (int i = 0; i < numTrials; i++)
                {
                    for (int j = 0; j < possibleLoc.Length; j++)
                    {
                        List<string> trial_2stims = new List<string>();
                        trial_2stims.Add((possibleLoc[j] - (stimDiff / 2)).ToString());
                        trial_2stims.Add((possibleLoc[j] + (stimDiff / 2)).ToString());
                        List<string> tmp = Randomize(trial_2stims);
                        listTrials.Add(tmp[0] + "," + tmp[1] + ",");
                    }
                }

            }

            List<string> listTrialsFinal = new List<string>(); 
            for (int i = 0; i < listTrials.Count; i++)
            {
                List<string> trialString = new List<string>();
                if (mode.Equals("Visual"))
                    trialString.Add("0,,");
                else if(mode.Equals("Auditory"))
                    trialString.Add(",0,");
                else if(mode.Equals("VisualAuditory"))
                    trialString.Add("0,0,");
                
                trialString.Add(listTrials[i]);
                List<string> tmp = Randomize(trialString);
                listTrialsFinal.Add(tmp[0] + "," + tmp[1]);
            }

            return listTrialsFinal;
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
            possibleLoc = new double[] { -3, -2, -1, 0, 1, 2, 3 };
            // check the inputs
            int numTrials;
            if (!CheckInput_Int(NumTrials_textbox.Text, out numTrials)) { return; };
            
            double stimLoc = 1;
            if (!CheckInput_Double(StimLoc_textbox.Text, out stimLoc)) { return; };
            
            double stimDiff = 0;
            if (!CheckInput_Double(CongruentAngle_textbox.Text, out stimDiff) && mode[0]+ mode[1]+ mode[2] >= 2) { return; };

            string modeStr = "";
            if(!SelectedMode(out modeStr)) { return; };

            // create the trials
            List<string> trials = CreatePossibleTrials(modeStr, numTrials, stimLoc, stimDiff);

            // randomize the trials order
            List<string> randomOrder = Randomize(trials);
            string filePath = SelectSavingDirectory().Replace(".txt","") + "_controlFile_" + modeStr + ".txt";
            WriteLog2File(randomOrder, filePath, modeStr);
        }

        private bool SelectedMode(out string mode_str)
        {
            mode_str = "";
            if (mode[0] + mode[1] + mode[2] == 0) 
            {
                MessageBox.Show("Please select a mode.");
                return false;
            }
            else if (mode[0] + mode[1] + mode[2] == 3 ) { mode_str = "VisualAuditoryHaptic"; }
            else if (mode[0] + mode[1] + mode[2] == 2 && mode[2] == 0) { mode_str = "VisualAuditory"; }
            else if (mode[0] + mode[1] + mode[2] == 1 && mode[0] == 1) { mode_str = "Visual"; }
            else if (mode[0] + mode[1] + mode[2] == 1 && mode[1] == 1) { mode_str = "Auditory"; }


            return true;

        }

        private void Visual_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            mode[0] = 1;
            if (mode[0] + mode[1] + mode[2] == 2) { TwoModeEnable(); }
            else { TwoModeDisable(); }
        }

        private void Auditory_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            mode[1] = 1;
            if (mode[0] + mode[1] + mode[2] == 2) { TwoModeEnable(); }
            else { TwoModeDisable(); }
        }

        private void Haptic_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            mode[2] = 1;
            if (mode[0] + mode[1] + mode[2] == 2) { TwoModeEnable(); }
            else { TwoModeDisable(); }
        }

        private void Visual_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            mode[0] = 0;
            if (mode[0] + mode[1] + mode[2] == 2) { TwoModeEnable(); }
            else { TwoModeDisable(); }
        }

        private void Auditory_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            mode[1] = 0;
            if (mode[0] + mode[1] + mode[2] == 2) { TwoModeEnable(); }
            else { TwoModeDisable(); }
        }

        private void Haptic_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            mode[2] = 0;
            if (mode[0] + mode[1] + mode[2] == 2) { TwoModeEnable(); }
            else { TwoModeDisable(); }
        }
        private void TwoModeEnable()
        {
            CongruentAngle_textbox.IsEnabled = true;
        }
        private void TwoModeDisable()
        {
            CongruentAngle_textbox.IsEnabled = false;
        }
        
    }
}
