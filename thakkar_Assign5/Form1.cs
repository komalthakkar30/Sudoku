using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace thakkar_Assign5
{
    public partial class Form1 : Form
    {
        private static int[,] initialInputArrays = new int[9, 9];
        private static int[,] savedInputArrays = new int[9, 9];
        private static int[,] solutionInputArrays = new int[9, 9];
        private static List<string> directory = new List<string>();
        private static int currentTime;
        private static bool isSavedPuzzle = false;
        private static bool isCompletedPuzzle = false;
        private static List<int> playedTimings = new List<int>();
        private static String currentFile = "";
        private static int totalEmptyCells;
        private static int currentEmptyCells;
        private static int totalIncorrectCells;

        public Form1()
        {
            InitializeComponent();
        }

        private void Reset_Button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            currentTime = 0;
            NewPuzzle_Button_Click(sender, e);

        }

        private void NewPuzzle_Button_Click(object sender, EventArgs e)
        {
            Easy_Button.BackColor = SystemColors.Control;
            Medium_Button.BackColor = SystemColors.Control;
            Hard_Button.BackColor = SystemColors.Control;

            StartPuzzle();
            GetPuzzleFromDir(sender, e);
            LoadPuzzle();
            LoadTextBoxes();
        }

        private void TextBox_Click(object sender, KeyPressEventArgs ke)
        {
            Saved_Label.Text = "You have unsaved work.";
            Saved_Label.ForeColor = Color.Red;
        }

        private void Save_Button_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    TextBox txtBox = panel1.Controls.Find("textBox" + (i + 1).ToString() + (j + 1).ToString(), true).FirstOrDefault() as TextBox;
                    savedInputArrays[i, j] = txtBox.Text != "" ? Convert.ToInt32(txtBox.Text) : 0;
                }
            }
            using (StreamWriter strWriteLine = new StreamWriter("..\\..\\puzzles\\" + currentFile))
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        strWriteLine.Write(initialInputArrays[i, j]);
                    }
                    strWriteLine.WriteLine();
                }
                strWriteLine.WriteLine();
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        strWriteLine.Write(solutionInputArrays[i, j]);
                    }
                    strWriteLine.WriteLine();
                }
                if (!isCompletedPuzzle)
                {
                    strWriteLine.WriteLine();
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            strWriteLine.Write(savedInputArrays[i, j]);
                        }
                        strWriteLine.WriteLine();
                    }
                }
            }

            using (StreamWriter strWriteLine = new StreamWriter("..\\..\\puzzles\\directory.txt"))
            {
                var puzzlePath = directory.FirstOrDefault(x => x.Contains(currentFile));
                directory.Remove(puzzlePath);

                if (isCompletedPuzzle)
                {
                    playedTimings.Add(currentTime);
                    directory.Add(currentFile + ";0;" + String.Join(",", playedTimings.ToArray()));
                }
                else
                {
                    directory.Insert(0, currentFile + ";" + currentTime + ";" + String.Join(",", playedTimings.ToArray()));
                }
                for (int i = 0; i < directory.Count; i++)
                {
                    strWriteLine.WriteLine(directory[i]);
                }
            }

            Saved_Label.Text = "Your work is saved.";
            Saved_Label.ForeColor = Color.ForestGreen;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            currentTime += 1;
            Time_Label.Text = TimeSpan.FromSeconds(currentTime).ToString(@"hh\:mm\:ss");
        }

        private void Pause_Button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Text == "Pause")
            {
                timer1.Stop();
                btn.Text = "Resume";
                panel1.BackColor = Color.White;

                Save_Button.Enabled = false;
                Progress_Button.Enabled = false;
                Help_Button.Enabled = false;
                Reset_Button.Enabled = false;

                panel1.Visible = false;
                panel2.Visible = true;
            }
            else
            {
                timer1.Start();
                btn.Text = "Pause";
                panel1.BackColor = Color.Black;
                panel1.Visible = true;
                panel2.Visible = false;

                Save_Button.Enabled = true;
                Progress_Button.Enabled = true;
                Help_Button.Enabled = true;
                Reset_Button.Enabled = true;
            }
        }

        private void GetPuzzleFromDir(object sender, EventArgs e)
        {
            directory.Clear();
            currentFile = "";

            using (StreamReader sr = new StreamReader("..\\..\\puzzles\\directory.txt"))
            {
                String line = sr.ReadLine();
                String fileName = "";
                String[] tokens;

                Button diffButton = sender as Button;
                diffButton.BackColor = Color.DarkGray;

                while (line != null)
                {
                    directory.Add(line);
                    tokens = line.Split(';');
                    fileName = tokens[0];

                    if (fileName.Contains(diffButton.Text.ToLower()) && currentFile == "")
                    {
                        currentTime = tokens.Length >= 2 ? Convert.ToInt32(tokens[1]) : 0;
                        currentFile = fileName;
                        if (tokens.Length >= 3 && tokens[2] != "") playedTimings = tokens[2].Split(',').Select(Int32.Parse).ToList();

                        isSavedPuzzle = false;
                        if (currentTime != 0)
                        {
                            isSavedPuzzle = true;
                        }
                    }
                    line = sr.ReadLine();
                }
            }
        }

        private void LoadPuzzle()
        {
            if (currentFile != "")
            {
                using (StreamReader sr1 = new StreamReader("..\\..\\puzzles\\" + currentFile))
                {
                    String line = sr1.ReadLine();
                    while (line != null)
                    {
                        /*********************  Storing Intial State    ********************/
                        for (int i = 0; i < 9; i++)
                        {
                            string[] inp = line.Select(x => x.ToString()).ToArray();

                            for (int j = 0; j < inp.Length; j++)
                            {
                                initialInputArrays[i, j] = Convert.ToInt32(inp[j]);
                            }
                            line = sr1.ReadLine();
                        }

                        /*********************  Storing Solution State    ********************/
                        line = sr1.ReadLine();  //Reading new line which is a solution
                        for (int i = 0; i < 9; i++)
                        {
                            string[] inp = line.Select(x => x.ToString()).ToArray();

                            for (int j = 0; j < inp.Length; j++)
                            {
                                solutionInputArrays[i, j] = Convert.ToInt32(inp[j]);
                            }
                            line = sr1.ReadLine();
                        }

                        /*********************  Storing Saved State    ********************/
                        line = sr1.ReadLine();  //Reading new line which is a solution
                        if (line != null)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                string[] inp = line.Select(x => x.ToString()).ToArray();

                                for (int j = 0; j < inp.Length; j++)
                                {
                                    savedInputArrays[i, j] = Convert.ToInt32(inp[j]);
                                }
                                line = sr1.ReadLine();
                            }
                        }
                        else
                        {
                            Array.Clear(savedInputArrays, 0, 81);
                        }
                    }
                }
            }
        }

        private void LoadTextBoxes()
        {
            /*********************  Filling Textboxes    ********************/
            if (!isSavedPuzzle)
            {
                Array.Copy(initialInputArrays, savedInputArrays, initialInputArrays.Length);
            }
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    TextBox txtBox = panel1.Controls.Find("textBox" + (i + 1).ToString() + (j + 1).ToString(), true).FirstOrDefault() as TextBox;
                    if (txtBox != null)
                    {
                        if (initialInputArrays[i, j] != 0)
                        {
                            txtBox.Text = initialInputArrays[i, j].ToString();
                            txtBox.Font = new Font(txtBox.Font, FontStyle.Bold);
                            txtBox.ReadOnly = true;
                        }
                        else if (savedInputArrays[i, j] != 0)
                        {
                            txtBox.Text = savedInputArrays[i, j].ToString();
                            txtBox.Font = new Font(txtBox.Font, FontStyle.Regular);
                            txtBox.ReadOnly = false;
                            totalEmptyCells += 1;
                        }
                        else
                        {
                            txtBox.Text = "";
                            txtBox.Font = new Font(txtBox.Font, FontStyle.Regular);
                            txtBox.ReadOnly = false;
                            totalEmptyCells += 1;
                        }
                    }
                }
            }
        }

        private void ClearPuzzle()
        {
            currentTime = 0;
            timer1.Stop();
            Time_Label.Text = "00:00:00";
            isSavedPuzzle = false;
            isCompletedPuzzle = false;

            Progress_Button.Enabled = false;
            Help_Button.Enabled = false;
            Reset_Button.Enabled = false;
            Save_Button.Enabled = false;
            Pause_Button.Enabled = false;

            Saved_Label.Text = "";
            Status_Label.Text = "";
        }

        private void StartPuzzle()
        {
            timer1.Interval = (1000) * (1);
            timer1.Enabled = true;
            timer1.Start();

            Progress_Button.Enabled = true;
            Help_Button.Enabled = true;
            Reset_Button.Enabled = true;
            Save_Button.Enabled = true;
            Pause_Button.Enabled = true;

            Saved_Label.Text = "";
            Status_Label.Text = "";
        }

        private void Progress_Button_Click(object sender, EventArgs e)
        {
            // Check all values
            currentEmptyCells = 0;
            totalIncorrectCells = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    TextBox txtBox = panel1.Controls.Find("textBox" + (i + 1).ToString() + (j + 1).ToString(), true).FirstOrDefault() as TextBox;
                    if (txtBox.Text == "")
                    {
                        currentEmptyCells += 1;
                    }
                    else if (solutionInputArrays[i, j].ToString() != txtBox.Text)
                    {
                        totalIncorrectCells += 1;
                    }
                }
            }
            if (totalIncorrectCells != 0)
            {
                Status_Label.Text = "You have " + totalIncorrectCells + " incorrect cells";
            }
            else if (currentEmptyCells != 0)
            {
                Status_Label.Text = "You have " + currentEmptyCells + " cells to go";
            }
            else
            {
                isCompletedPuzzle = true;
                timer1.Stop();
                Save_Button_Click(sender, e);
                var avg = playedTimings.Sum() / playedTimings.Count;
                var fastest = playedTimings.Min();
                MessageBox.Show("Congratulations! You solved this Sudoku in " + TimeSpan.FromSeconds(currentTime) + "\nYour average time within this difficulty:    " + TimeSpan.FromSeconds(avg) + "\nYour fastest time within this difficulty:    " + TimeSpan.FromSeconds(fastest));
                ClearPuzzle();
            }
        }

        private void Help_Button_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            int i = rnd.Next(1, 9); // creates a number between 1 and 9
            int j = rnd.Next(1, 9);
            Save_Button_Click(sender, e);
            if (savedInputArrays[i, j] == 0 || savedInputArrays[i, j] != solutionInputArrays[i, j])
            {
                TextBox txtBox = panel1.Controls.Find("textBox" + (i + 1).ToString() + (j + 1).ToString(), true).FirstOrDefault() as TextBox;
                if (txtBox != null)
                {
                    txtBox.Text = solutionInputArrays[i, j].ToString();
                    txtBox.Font = new Font(txtBox.Font, FontStyle.Regular);
                    txtBox.ReadOnly = false;
                }
            }
            else
            {
                Help_Button_Click(sender, e);
            }
        }
    }
}
