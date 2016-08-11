using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NecroBot_GUI
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        private const short SWP_NOZORDER = 0X4;

        Process bot;
        bool hidBotWindow = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;

            startBot();
        }

        private void startBot()
        {
            bot = new Process();
            bot.StartInfo.FileName = "NecroBot.exe";
            bot.StartInfo.UseShellExecute = false;
            bot.StartInfo.RedirectStandardOutput = true;

            bot.OutputDataReceived += new DataReceivedEventHandler(outputHandler);

            bot.Start();
            bot.BeginOutputReadLine();
        }

        private void outputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try {
                Process botProcess = (Process)sendingProcess;

                StringBuilder stringBuilder = new StringBuilder(256);
                GetWindowText(botProcess.MainWindowHandle, stringBuilder, stringBuilder.Capacity);
                updateStats(stringBuilder.ToString());

                updateLogs(outLine.Data);
            }
            catch (Exception e)
            {
                logToBox(outputBoxAll, Environment.NewLine + e.ToString(), Color.Red);
            }
        }

        private void updateStats(String title)
        {
            if (title.Contains("Runtime"))
            {
                title = title.Substring(title.IndexOf("]") + 2);

                String[] getBaseSections = title.Split('-');

                accLabel.Text = getBaseSections[0].Trim();
                runtimeLabel.Text = getSectionInfo(getBaseSections[1], "Runtime ", " ");
                lvlLabel.Text = getSectionInfo(getBaseSections[2], "Lvl: ", "(A");
                xpLabel.Text = getSectionInfo(getBaseSections[2], " | ", "XP");
                advInLabel.Text = getSectionInfo(getBaseSections[2], "(Advance in ", "|");

                expHrLabel.Text = getSectionInfo(getBaseSections[2], "EXP/H: ", "|");
                pHrLabel.Text = getSectionInfo(getBaseSections[2], "| P/H: ", "|");
                stardustLabel.Text = getSectionInfo(getBaseSections[2], "Stardust: ", "|");
                transferLabel.Text = getSectionInfo(getBaseSections[2], "Transfered: ", "|");
                recycleLabel.Text = getSectionInfo(getBaseSections[2], "Recycled: ");
            }
        }

        private void updateLogs(String line)
        {
            if (line != null)
            {
                Color lineColor = Color.Snow;

                if (line.Contains("New update detected, would you like to update"))
                {
                    MessageBox.Show("New update detected. Please open the original NecroBot.exe file and update before reopening the GUI.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (line.Contains("your first start"))
                {
                    MessageBox.Show("Please setup NecroBot through the original file first before reopening the GUI.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (line.Contains("(POKESTOP)"))
                    {
                        lineColor = Color.Aqua;
                        logToBox(outputBoxPokestop, line, lineColor);
                    }
                    else if (line.Contains("(SNIPER)"))
                    {
                        lineColor = Color.Yellow;
                        logToBox(outputBoxSniper, line, lineColor);
                    }
                    else if (line.Contains("(TRANSFERED)"))
                    {
                        lineColor = Color.Green;
                        logToBox(outputBoxTransfer, line, lineColor);
                    }
                    else if (line.Contains("(PKMN)"))
                    {
                        lineColor = Color.Lime;
                        logToBox(outputBoxPkmn, line, lineColor);
                    }
                    else if (line.Contains("(INFO)"))
                    {
                        lineColor = Color.DarkCyan;
                        logToBox(outputBoxInfo, line, lineColor);

                        if (!hidBotWindow)
                        {
                            SetWindowPos(bot.MainWindowHandle, 0, -999, -999, 20, 20, SWP_NOZORDER);
                            hidBotWindow = true;
                        }
                    }
                    else if (line.Contains("(RECYCLING)"))
                    {
                        lineColor = Color.DarkViolet;
                        logToBox(outputBoxRecycle, line, lineColor);
                    }
                    else if (line.Contains("(ATTENTION)"))
                    {
                        lineColor = Color.Red;
                    }
                }
                
                logToBox(outputBoxAll, line, lineColor);
            }
        }

        private void logToBox(RichTextBox richBox, String line, Color lineColor)
        {
            if (richBox.InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(
                    () => logToBox(richBox, line, lineColor)
                ));

                return;
            }

            richBox.SelectionStart = richBox.TextLength;
            richBox.SelectionLength = 0;
            richBox.SelectionColor = lineColor;
            richBox.AppendText(line + Environment.NewLine);
            richBox.SelectionColor = richBox.ForeColor;

            richBox.SelectionStart = richBox.TextLength;
            richBox.ScrollToCaret();
        }

        private String getSectionInfo(String section, String sectionTitle, String endAt = null) {
            String info = section.Substring(section.IndexOf(sectionTitle) + sectionTitle.Length);
            if (endAt != null)
            {
                info = info.Substring(0, info.IndexOf(endAt));
            }

            return info.Trim();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                bot.Kill();
            }
            catch (Exception) { }

            Environment.Exit(0);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/weidizhang");
        }
    }
}
