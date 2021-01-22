using Gma.System.MouseKeyHook;
using Process.NET;
using Process.NET.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BadaBoom_Bots
{
    public partial class Form1 : Form
    {
        private IKeyboardMouseEvents m_GlobalHook;
        public Form1()
        {
            InitializeComponent();
        }
        List<string> all = new List<string>();
        List<string> newst = new List<string>();
        long oldlenght = 0;
        static double otver = -1;
        List<string> newstrs = new List<string>();
        private void Worker()
        {
            while (true)
            {
                string path = textBox1.Text + @":\Users\" + Environment.UserName + @"\AppData\Roaming\Kaboom\modpacks\1.7.10\modpacks\dragonglory\logs\latest.log";
                if (File.Exists(path))
                {
                    using (FileStream sr = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader file = new StreamReader(sr, Encoding.Default))
                        {
                            String line;
                            long lenght = 0;
                            all.Clear();
                            while ((line = file.ReadLine()) != null) //читаем по одной линии(строке) пока не вычитаем все из потока (пока не достигнем конца файла)
                            {
                                lenght++;
                                all.Add(line);
                                if (all.Count() > newst.Count())
                                {
                                    newst.Add(line);
                                    newstrs.Add(line);
                                }
                            }
                            if (oldlenght == 0)
                                oldlenght = lenght;
                            if (oldlenght < lenght)
                            {
                                for (long i = oldlenght; i < lenght; i++)
                                {
                                    string doneline = (newst[(int)i]);
                                    if (doneline.Contains("[Kaboom] Решите пример:"))
                                    {
                                        string expr = Regex.Match(doneline, "Решите пример: (.*)").Groups[1].Value;
                                        double tx = Calculator.Calc(expr);
                                        otver = tx;
                                        if (bunifuCheckbox1.Checked)
                                        {
                                            new Thread(() =>
                                            {
                                                SoundPlayer player = new SoundPlayer("25.wav");//В скобках имя файла в формате wav
                                            player.Play();
                                            }).Start();
                                        }
                                        Console.WriteLine("Ответ: " + tx);
                                        if (bunifuCheckbox2.Checked)
                                        {
                                            new Thread(() =>
                                            {
                                                Thread.Sleep(250);
                                                SendCalc();
                                            }).Start();
                                        }
                                    }
                                    else if (doneline.Contains("[Kaboom] Победил игрок"))
                                    {
                                        string expr = Regex.Match(doneline, "Победил игрок (.*)").Groups[1].Value;
                                        Console.WriteLine("Победил игрок: " + expr);
                                        otver = -1;
                                    }
                                }
                                oldlenght = lenght;
                            }
                        }
                    }
                    Thread.Sleep(25);
                }
            }
        }
        private void SendCalc()
        {
            System.Diagnostics.Process game = FindGame("javaw", "Minecraft");
            if (game != null)
            {
                var process = new ProcessSharp(game, MemoryType.Remote);
                process.WindowFactory.MainWindow.Keyboard.Press(Process.NET.Native.Types.Keys.T);
                Thread.Sleep(16);
                process.WindowFactory.MainWindow.Keyboard.Release(Process.NET.Native.Types.Keys.T);
                Thread.Sleep(16);
                process.WindowFactory.MainWindow.Keyboard.Press(Process.NET.Native.Types.Keys.Back);
                Thread.Sleep(16);
                process.WindowFactory.MainWindow.Keyboard.Release(Process.NET.Native.Types.Keys.Back);
                Thread.Sleep(25);
                foreach (char st in otver.ToString().ToArray())
                {
                    //Console.WriteLine(st.ToString());
                    Process.NET.Native.Types.Keys press = Process.NET.Native.Types.Keys.None;
                    Enum.TryParse("D" + st.ToString().ToUpper(), out press);
                    if (press != Process.NET.Native.Types.Keys.None)
                    {
                        process.WindowFactory.MainWindow.Keyboard.Press(press);
                        Thread.Sleep(22);
                        process.WindowFactory.MainWindow.Keyboard.Release(press);
                        Thread.Sleep(22);
                    }
                }
                Thread.Sleep(35);
                process.WindowFactory.MainWindow.Keyboard.PressRelease(Process.NET.Native.Types.Keys.Enter);

            }
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            Thread th = new Thread(Worker);
            th.Start();

            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.KeyUp += GlobalHookKeyPress;
        }
        public System.Diagnostics.Process FindGame(string process, string title)
        {
            System.Diagnostics.Process[] javas = System.Diagnostics.Process.GetProcessesByName(process);
            foreach (System.Diagnostics.Process java in javas)
            {
                if (java.MainWindowTitle.Contains(title))
                {
                    return java;
                }
            }
            return null;
        }
        private void GlobalHookKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.R)
            {
                Console.WriteLine("Pressed " + otver);
                if (otver != -1)
                {
                    SendCalc();
                }
            }
        }
        private void bunifuCheckbox2_OnChange(object sender, EventArgs e)
        {

        }
    }
    public static class Calculator
    {
        // Создадим статический экземпляр DataTable, чтобы каждый раз не инициализировать его заново
        private static DataTable Table { get; } = new DataTable();

        // Наш метод подсчета
        // Добавьте отлов ошибок по вкусу)
        public static double Calc(string Expression) => Convert.ToDouble(Table.Compute(Expression, string.Empty));
    }
}
