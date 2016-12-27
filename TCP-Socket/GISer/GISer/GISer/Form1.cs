using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace GISer
{
    public partial class Form1 : Form
    {
        #region 全局变量
        //最后计算所得矩阵
        float[,] fTheFinalResultMatrix = new float[801, 1401];

        //获取当前程序的bin文件的路径作为最后矩阵文件的输出路径
        private string sOutPath = AppDomain.CurrentDomain.BaseDirectory;

        private string sDirectory;
        private int iYear;
        private int iMonth;
        private int iDay;

        //更新:注意到跨年和跨月
        DateTime datetimeWater;
        //文件的全名
        private string sPathCombine;
        #endregion

        public Form1()
        {
            InitializeComponent();

            #region 按照日期选择文件路径
            //目录--在配置文件里，不同的电脑需要更改文件所在的目录
            sDirectory = ConfigurationManager.AppSettings["WaterPath"].ToString();
            //年
            iYear = int.Parse(textBoxYear.Text.Trim());
            //月
            iMonth = int.Parse(textBoxMonth.Text.Trim());
            //日
            iDay = int.Parse(textBoxDay.Text.Trim());
            //日期指定
            datetimeWater = new DateTime(iYear, iMonth, iDay, 23, 00, 00);
            #endregion
        }

        //开始程序
        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                float[,] fMatrixTemp;
                //按照公式计算15天的矩阵
                for (int i = 0; i < 15; i++)
                {
                    fMatrixTemp = returnMatrixOneDay();
                    float fNum = (float)Math.Pow(0.84, i);
                    calculateMultiplyMatrix(ref fMatrixTemp, fNum);
                    //SaveOneMatrix(fMatrixTemp);
                }
                //最后将最后结果的矩阵写入txt文件
                SaveTheFinalResultMatrix();

                sw.Stop();
                Console.WriteLine("当前耗时:{0}",sw.Elapsed);
                
            }
            catch (Exception ex)
            {
                //会吞掉异常
                MessageBox.Show(ex.Message);
            }

        }

        //关闭窗体
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region 计算函数
        //返回一天对应的矩阵
        [DebuggerStepThroughAttribute]
        private float[,] returnMatrixOneDay()
        {
            //每得到一个矩阵就减1是因为它是根据天数来得到矩阵文件的
            datetimeWater = datetimeWater.AddDays(-1);

            float[,] fMatrixTemp = returnMatrixOneHour();
            for (int i = 1; i < 24; i++)
            {
                float[,] matrix = returnMatrixOneHour();
                calculateMatrix(ref fMatrixTemp, matrix);
            }
            return fMatrixTemp;
        }

        //返回一个小时对应的矩阵
        //[DebuggerStepThroughAttribute]
        private float[,] returnMatrixOneHour()
        {
            sPathCombine = sDirectory + "\\" + datetimeWater.ToString("yyyyMMddHH") + ".000.001h";

            //二维矩阵--用来存储降水的数据
            //每个文件的数据是固定的801行
            float[,] Water = new float[801, 1401];

            //测试文件是否能打开
            if (File.Exists(sPathCombine))
            {
                //Process.Start("explorer.exe", sPathCombine);
                #region 具体到矩阵文件
                using (StreamReader sr = new StreamReader(sPathCombine))
                {
                    string line;
                    //跳过前3行
                    sr.ReadLine();
                    sr.ReadLine();
                    sr.ReadLine();
                    //写入矩阵
                    for (int j = 0; j < 801; j++)
                    {
                        if ((line = sr.ReadLine()) != null)
                        {
                            int k = 0;
                            do
                            {
                                //将一行变换为数组并且省略空字符串
                                string[] sWater = line.Split(new string[] 
                            { 
                                //设置字符间的的间隔符
                                " ",
                                "  ",
                                "   ",
                                "    "
                            }, StringSplitOptions.RemoveEmptyEntries);//经测试，大约10个
                                foreach (string s in sWater)
                                {
                                    Water[j, k] = float.Parse(s);
                                    k++;
                                }
                            } while ((line = sr.ReadLine()) != null && k < 1401);
                        }
                    }
                }
                #endregion
            }

            //每得到一个矩阵就减1是因为它是根据小时来得到矩阵文件的
            datetimeWater = datetimeWater.AddHours(-1);
            //如果没有这个小时对应的矩阵，则为空矩阵（全为0）
            return Water;
        }

        //计算2个矩阵并返回矩阵
        [DebuggerStepThroughAttribute]
        private void calculateMatrix(ref float[,] MatrixTemp, float[,] Matrix)
        {
            for (int j = 0; j < 801; j++)
            {
                for (int k = j; k < 1401; k++)
                {
                    MatrixTemp[j, k] = MatrixTemp[j, k] + Matrix[j, k];
                }
            }
        }

        //测试函数--打印出来
        [DebuggerStepThroughAttribute]
        private void printMatrix(float[,] Matrix)
        {
            for (int j = 0; j < 801; j++)
            {
                for (int k = j; k < 1401; k++)
                {
                    Console.Write(Matrix[j, k].ToString() + "     ");
                }
                Console.WriteLine();
            }
            Console.WriteLine(datetimeWater.Hour);
        }

        //给矩阵乘以一个数并保存到最终的矩阵里
        [DebuggerStepThroughAttribute]
        private void calculateMultiplyMatrix(ref float[,] MatrixTemp, float fNum)
        {
            for (int j = 0; j < 801; j++)
            {
                for (int k = j; k < 1401; k++)
                {
                    MatrixTemp[j, k] = MatrixTemp[j, k] * fNum;//计算
                    fTheFinalResultMatrix[j, k] = fTheFinalResultMatrix[j, k] + MatrixTemp[j, k];//保存到矩阵
                }
            }
        }

        //输出结果
        private void SaveOneMatrix(float[,] fOneMatrix)
        {
            if (!Directory.Exists(sOutPath))
            {
                #region
                using (StreamWriter sw = File.CreateText(Path.Combine(sOutPath, datetimeWater.ToString("yyyyMMdd"),".txt")))
                {
                    //先输出前3行
                    sw.WriteLine(" 测试  ");
                    sw.WriteLine(" 10 01 01 08 0 9999");
                    sw.WriteLine("   0.050   0.050    70.0   140.0    15.0    55.0  1401   801      1.00       0.0      41.1   1   0");
                    sw.Write("\t\t");
                    sw.Flush();

                    int iCount = 0;
                    //输出矩阵到文件
                    foreach (float f in fOneMatrix)
                    {
                        iCount++;
                        //每行10个
                        sw.Write(f + "\t");
                        if (iCount % 10 == 0)
                        {
                            sw.WriteLine();
                            sw.Write("\t\t");
                            sw.Flush();
                        }
                    }
                    sw.Flush();
                }
                #endregion
            }
            else
            {
                #region
                using (StreamWriter sw = File.CreateText(Path.Combine(sOutPath, "FinalResult.txt")))
                {
                    //先输出前3行
                    sw.WriteLine(" diamond 4 16062400.000.001h");
                    sw.WriteLine(" 10 01 01 08 0 9999");
                    sw.WriteLine("   0.050   0.050    70.0   140.0    15.0    55.0  1401   801      1.00       0.0      41.1   1   0");
                    sw.Write("\t\t");
                    sw.Flush();

                    int iCount = 0;
                    //输出矩阵到文件
                    foreach (float f in fTheFinalResultMatrix)
                    {
                        iCount++;
                        //每行10个
                        sw.Write(f + "\t");
                        if (iCount % 10 == 0)
                        {
                            sw.WriteLine();
                            sw.Write("\t\t");
                            sw.Flush();
                        }
                    }
                    sw.Flush();
                }
                #endregion
            }
        }

        private void SaveTheFinalResultMatrix()
        {

            if (!Directory.Exists(sOutPath))
            {
                #region 
                using (StreamWriter sw = File.CreateText(Path.Combine(sOutPath, "FinalResult.txt")))
                {
                    //先输出前3行
                    sw.WriteLine(" diamond 4 16121800.000.001h");
                    sw.WriteLine(" 10 01 01 08 0 9999");
                    sw.WriteLine("   0.050   0.050    70.0   140.0    15.0    55.0  1401   801      1.00       0.0      41.1   1   0");
                    sw.Write("\t\t");
                    sw.Flush();

                    int iCount = 0;
                    //输出矩阵到文件
                    foreach (float f in fTheFinalResultMatrix)
                    {
                        iCount++;
                        //每行10个
                        sw.Write(f + "\t");
                        if (iCount % 10 == 0)
                        {
                            sw.WriteLine();
                            sw.Write("\t\t");
                            sw.Flush();
                        }
                    }
                    sw.Flush();
                }
                #endregion
            }
            else
            {
                #region 
                using (StreamWriter sw = File.CreateText(Path.Combine(sOutPath, "FinalResult.txt")))
                {
                    //先输出前3行
                    sw.WriteLine(" diamond 4 16062400.000.001h");
                    sw.WriteLine(" 10 01 01 08 0 9999");
                    sw.WriteLine("   0.050   0.050    70.0   140.0    15.0    55.0  1401   801      1.00       0.0      41.1   1   0");
                    sw.Write("\t\t");
                    sw.Flush();

                    int iCount = 0;
                    //输出矩阵到文件
                    foreach (float f in fTheFinalResultMatrix)
                    {
                        iCount++;
                        //每行10个
                        sw.Write(f + "\t");
                        if (iCount % 10 == 0)
                        {
                            sw.WriteLine();
                            sw.Write("\t\t");
                            sw.Flush();
                        }
                    }
                    sw.Flush();
                }
                #endregion
            }
        }

        //所有的函数
        #endregion
    }
}
        