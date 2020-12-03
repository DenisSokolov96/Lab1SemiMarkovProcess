using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Lab1SemiMarkovProcess
{
    /*
     * ормула для состояний =СЧЁТЕСЛИ($B2:$D2;H$1)
     * Лягушка
     * Молекула
     * Блуждание
     * Свое
     */

    public partial class MainForm : Form
    {
        //вероятности переходов
        private List<double[]> listStahM = new List<double[]>();
        //стартовые типы переходов
        private List<int> masTime = new List<int>();
        //стартовые типы переходов
        private List<double> masTimeParam = new List<double>();
        //стартовые вероятности переходов
        private List<double> startMas = new List<double>();
        //типы переходов
        private List<int[]> listMTime = new List<int[]>();
        //параметры переходов
        private List<int[]> listTParam = new List<int[]>();
        //матрица статистики переходов
        private List<int[]> matrStatist = new List<int[]>();
        //кол-во итераций
        private int countIter = 0;        
        //кол-во процессов
        private int countProcc = 0;
        //сохранение состояния
        private StringBuilder stateData = new StringBuilder();
        //запись подробного отчета
        private StringBuilder reportStr = new StringBuilder();
        //счетчик прыжков
        private int jumpCount = 0;

        public MainForm()
        {
            InitializeComponent();
            for (int i = 0; i < 4; i++)
                comboBox1.Items.Add(i+1);
            comboBox1.Text = "1";
        }

        private void запуститьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //очиста списков и полей
            clearData();

            int z = 1;
            try
            {
                z = Convert.ToInt32(comboBox1.Text.ToString());
            }
            catch {}

            //чтение из richTextBox
            //readDatas();

            //чтение из файлов и проверка на корректность данных
            if (readFiles(z))
            {
                //несколько процессов
                for (int i = 0; i < countProcc; i++)
                {
                    start(i + 1);
                    //опускаю бегунок
                    richTextBox1.SelectionStart = richTextBox1.TextLength;
                    richTextBox1.ScrollToCaret();

                    stateData.Clear();
                    reportStr.Clear();
                }
            }
            richTextBox1.Text += "\n\n";

        }

        //основная функция
        private void start(int ifile)
        {            
            double[] variants = startMas.ToArray();
            int[] times = masTime.ToArray();
            int k = 0;
            
            int curStateIndex = -1;
            int newState = rndSelect(variants);
            double jumpT = 0;

            double actionT = jumpT;

            while (k < countIter)
            {
                if (actionT < 0.001)
                {
                    reportStr.Append(writeDatas(k, variants, newState, jumpT, curStateIndex));
                    //подготовка для следующего прыжка
                    variants = listStahM[newState].ToArray();
                    times = listMTime[newState].ToArray();
                    curStateIndex = newState;
                    //подсчет времени для нового шага
                    newState = rndSelect(variants);
                    jumpT = timeJumpFunc(curStateIndex, newState, times[newState]);

                    actionT = jumpT;
                }                
                                
                stateData.AppendLine((curStateIndex+1).ToString());
                actionT = actionT - 0.001;
                k++;

            }

            string path = Directory.GetCurrentDirectory() + "\\TimelineBuilder\\";
            StreamWriter file = new StreamWriter(path + "Data_out_" + ifile.ToString() + ".txt");
            StreamWriter info = new StreamWriter(path + "Info_out_" + ifile.ToString() + ".txt");

            info.WriteLine(reportStr.ToString());
            file.WriteLine(stateData.ToString());

            file.Close();
            info.Close();

            richTextBox1.Text += writeResult();
        }

        //выбираем состояние для прыжка
        private int rndSelect(double[] mas)
        {
            double a = mas.Sum();
            if (a < 1) mas[0] += 1 - a;
            
            return new Random().Next(0, mas.Length);
        }

        //Определяем время прыжка
        private double timeJumpFunc(int iState, int newState, int t)
        {
            if (iState == -1)
            {
                switch (t) {
                    case 1: return masTimeParam[newState];
                    case 2: return rndFunc(masTimeParam[newState]);
                    default: break;
                }
            } 

            switch (t)
            {
                case 1: return listTParam[t][newState];
                case 2: return rndFunc(listTParam[iState][newState]);
                default: break;
            }
            
            return -1;
        }

        private double rndFunc(double data)
        {
            return -Math.Log(1.0 - new Random().NextDouble()) /data;
        }

        //вывод результирующей матрицы на экран
        private string writeResult()
        {
            string str = "";
            //вывожу матрицу прыжков
            for (int i = 0; i < matrStatist.Count; i++)
            {
                for (int j = 0; j < matrStatist[i].Length; j++)
                    str += matrStatist[i][j].ToString() + " ";
               str += "\n";
            }

            int sum = 0;
            for (int i = 0; i<matrStatist.Count; i++)
                sum += matrStatist[i].Sum();
            str += "Прыжков: " + sum + " за " + countIter + " время.\n";

            return str;
        }

        //вывод данных
        private string writeDatas(int index, double[] stah, int newState, double t, int state)
        {
            string str = "";            
            if (state == -1) matrStatist[0][newState] += 1;
            else matrStatist[state][newState] += 1;

            str = (jumpCount+1).ToString() + "\t" + "В момент времени: " + (index + 1) + "\t" + (state + 1) + " -> " + (newState + 1) + "\tЗа время" +
                +Math.Round(t, 3) + "\t Вероятности [ ";

            foreach (double val in stah)
                str += val.ToString() + " ";
            str += "]\n";

            jumpCount++;
            return str;
        }

        //очистка списков и полей
        private void clearData()
        {
            richTextBox2.Text = "";
            richTextBox3.Text = "";
            richTextBox4.Text = "";
            richTextBox5.Text = "";
            richTextBox6.Text = "";
            richTextBox7.Text = "";
            listStahM.Clear();
            masTime.Clear();
            masTimeParam.Clear();
            startMas.Clear();
            listMTime.Clear();
            listTParam.Clear();
            matrStatist.Clear();
            countIter = 0;
            countProcc = 0;
            stateData.Clear();
            reportStr.Clear();
            jumpCount = 0;
    }

        /***********************************************************************/

        //чтение данных из файлаs
        private bool readFiles(int val)
        {
            string[] nameFile = new string[] { "matrix_", "start_time_", "start_time_param_", "start_vector_", "time_matrix_", "time_param_matrix_" };
            string path = Directory.GetCurrentDirectory() + "\\TimelineBuilder\\DataRead\\";
            try
            {
                countIter = Convert.ToInt32(textBox1.Text);
                countProcc = Convert.ToInt32(textBox2.Text);

                //считываю файлы - всего 6                  
                for (int i = 0; i < 6; i++)
                {
                    string name = nameFile[i] + val.ToString() + ".txt";
                    using (StreamReader sr = new StreamReader(path + name, Encoding.Default))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            switch (i)
                            {
                                //файл 1
                                case 0: {
                                        richTextBox2.Text += line.ToString() + "\n";
                                        listStahM.Add(Array.ConvertAll(line.Split(' '), Double.Parse));
                                        break;
                                }
                                //файл 2
                                case 1: {
                                        richTextBox3.Text += line.ToString() + "\n";
                                        masTime.Add(Array.ConvertAll(line.Split(' '), Int32.Parse)[0]);
                                        break;
                                }
                                //файл 3
                                case 2: {
                                        richTextBox4.Text += line.ToString() + "\n";
                                        masTimeParam.Add(Array.ConvertAll(line.Split(' '), Double.Parse)[0]);
                                        break;
                                }
                                //файл 4
                                case 3: {
                                        richTextBox5.Text += line.ToString() + "\n";
                                        startMas.Add(Array.ConvertAll(line.Split(' '), Double.Parse)[0]);
                                        break;
                                }
                                //файл 5
                                case 4: {
                                        richTextBox6.Text += line.ToString() + "\n";
                                        listMTime.Add(Array.ConvertAll(line.Split(' '), Int32.Parse));
                                        break;
                                }
                                //файл 6
                                case 5: {
                                        richTextBox7.Text += line.ToString() + "\n";
                                        listTParam.Add(Array.ConvertAll(line.Split(' '), Int32.Parse));
                                        break;
                                }
                                default: break;
                            }
                        }
                    }
                }

                //формирование матрицы статистики                
                for (int i = 0; i < listStahM.Count(); i++)
                {
                    int[] mas = new int[listStahM[i].Length];
                    for (int j = 0; j < listStahM[i].Length; j++)
                        mas[j] = 0;
                    matrStatist.Add(mas);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        //получение данных из richTextbox в списки
        /*private void readDatas()
        {           
            try
            {
                countIter = Convert.ToInt32(textBox1.Text);
                countProcc = Convert.ToInt32(textBox2.Text);

                for (int i = 0; i < richTextBox2.Lines.Length; i++)                              
                    listStahM.Add(Array.ConvertAll(richTextBox2.Lines[i].Split(' '), Double.Parse));

                masTime = new int[richTextBox3.Lines.Length];
                for (int i = 0; i < richTextBox3.Lines.Length; i++)
                    masTime[i] = Convert.ToInt32(richTextBox3.Lines[i]);

                masTimeParam = new double[richTextBox4.Lines.Length];
                for (int i = 0; i < richTextBox4.Lines.Length; i++)
                    masTimeParam[i] = Convert.ToDouble(richTextBox4.Lines[i]);

                startMas = new double[richTextBox5.Lines.Length];
                for (int i = 0; i < richTextBox5.Lines.Length; i++)
                    startMas[i] = Convert.ToDouble(richTextBox5.Lines[i]);
               
                for (int i = 0; i < richTextBox6.Lines.Length; i++)
                    listMTime.Add(richTextBox6.Lines[i].Split(' ').Select(int.Parse).ToArray());

                for (int i = 0; i < richTextBox7.Lines.Length; i++)
                    listTParam.Add(richTextBox7.Lines[i].Split(' ').Select(int.Parse).ToArray());

                for (int i = 0; i < listStahM.Count(); i++)
                {
                    int[] mas = new int[listStahM[i].Length];
                    for (int j = 0; j < listStahM[i].Length; j++)                    
                        mas[j] = 0;                    
                    matrStatist.Add(mas);
                }
            }
            catch(Exception e) {
                MessageBox.Show(e.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);               
            }
        }*/

    }
}
