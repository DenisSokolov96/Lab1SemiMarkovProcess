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
    public partial class MainForm : Form
    {
        //вероятности переходов
        private List<double[]> listStahM = new List<double[]>();
        //стартовые типы переходов
        private int[] masTime;
        //стартовые типы переходов
        private int[] masTimeParam;
        //стартовые вероятности переходов
        private double[] startMas;
        //типы переходов
        private List<int[]> listMTime = new List<int[]>();
        //параметры переходов
        private List<int[]> listTParam = new List<int[]>();
        //матрица статистики переходов
        private List<int[]> matrStatist = new List<int[]>();
        //кол-во итераций
        private int countIter = 0;
        private bool flag;
        //кол-во процессов
        private int countProcc = 0;

        public MainForm()
        {
            InitializeComponent();
        }

        private void запуститьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            readDatas();
            if (!flag)
            {            
                //несколько процессов
                for (int i = 0; i < countProcc; i++)
                {
                    computation(i+1);
                    richTextBox1.SelectionStart = richTextBox1.TextLength;
                    richTextBox1.ScrollToCaret();
                }
            }  else richTextBox1.SelectionStart = richTextBox1.TextLength;

        }

        //основная функция
        private void computation(int ifile)
        {            
            double[] variants = startMas.ToArray();
            int[] times = masTime.ToArray();
            int k = 0;
            int t = -1;
            StreamWriter file = new StreamWriter("D:\\Универ\\2й курс магистратура\\Моделирование систем\\TimelineBuilder\\data" + ifile.ToString() + ".txt");
            var sw = new Stopwatch();
            sw.Start();
            int updateT = 0;
            double jumpT = 0;
            /*int updateT = rndSelect(variants);
            double jumpT = timeJumpFunc(t, updateT, times[updateT]);
            funWrite(k, variants, updateT, jumpT, t);
            file.WriteLine(updateT + 1);
            variants = listStahM[updateT];
            times = listMTime[updateT];
            t = updateT;
            k++;*/

            while (k < countIter)
            {

                if (k == 0)
                {
                    sw.Start();
                    /*int*/
                    updateT = rndSelect(variants);
                    /*double*/
                    jumpT = timeJumpFunc(t, updateT, times[updateT]);
                    funWrite(k, variants, updateT, jumpT, t);
                    file.WriteLine(updateT + 1);

                    variants = listStahM[updateT];
                    times = listMTime[updateT];

                    t = updateT;

                    k++;                    
                }
                else if (sw.ElapsedMilliseconds < 1) k++;
                else
                {
                    sw.Restart();
                    updateT = rndSelect(variants);
                    jumpT = timeJumpFunc(t, updateT, times[updateT]);
                    funWrite(k, variants, updateT, jumpT, t);
                    file.WriteLine(updateT + 1);

                    variants = listStahM[updateT];
                    times = listMTime[updateT];

                    t = updateT;

                    k++;
                }
                
                
                

            }
            file.Close();

            //вывожу матрицу прыжков
            for (int i = 0; i < matrStatist.Count; i++)
            {
                for (int j = 0; j < matrStatist[i].Length; j++)
                    richTextBox1.Text += matrStatist[i][j].ToString() + " ";
                richTextBox1.Text += "\n";
            }
            /*int sum = 0;
            for (int i = 0; i<matrStatist.Count; i++)
                sum += matrStatist[i].Sum();
            richTextBox1.Text += k.ToString() + " = " + sum + "\n";*/

        }

        private int rndSelect(double[] mas)
        {
            double a = mas.Sum();
            if (a < 1) mas[0] += 1 - a;
            
            return new Random().Next(0, mas.Length);
        }

        private double timeJumpFunc(int t, int updateT, int zn)
        {
            if (t == -1)
            {
                switch (zn) {
                    case 1: return masTimeParam[updateT];
                    case 2: return rndFunc(masTimeParam[updateT]);
                    default: break;
                }
            } 

            switch (zn)
            {
                case 1: return listTParam[t][updateT];
                case 2: return rndFunc(listTParam[t][updateT]);
                default: break;
            }
            
            return -1;
        }

        private double rndFunc(int data)
        {
            return -Math.Log(1.0 - new Random().NextDouble()) /data;
        }

        private void funWrite(int k, double[] variants, int updateT, double jumpT, int t)
        {            
            if (t != -1) matrStatist[t][updateT] += 1;
                        
            richTextBox1.Text += (k+1).ToString() + ")\t" + (t + 1).ToString() + "-->" + (updateT + 1).ToString() +
                                " <- " + Math.Round(jumpT, 3).ToString() + " -> " + "\t[ ";
          
            for (int i = 0; i < variants.Length - 1; i++)
                richTextBox1.Text += variants[i] + " ";
            richTextBox1.Text += variants[variants.Length - 1] + " ]" + "\n";          
        }

        //получение данных
        private void readDatas()
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

                masTimeParam = new int[richTextBox4.Lines.Length];
                for (int i = 0; i < richTextBox4.Lines.Length; i++)
                    masTimeParam[i] = Convert.ToInt32(richTextBox4.Lines[i]);

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
                flag = true;
            }
        }

    }
}
