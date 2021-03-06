﻿using System;
using System.IO;
using System.Windows.Forms;

using kcptun_gui.Controller;
using kcptun_gui.View;
using Microsoft.Win32;

namespace kcptun_gui
{
    static class Program
    {
        static MainController controller;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Directory.SetCurrentDirectory(Application.StartupPath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!Logging.OpenLogFile())
            {
                MessageBox.Show(string.Format(I18N.GetString("Can't access the file '{0}', it is maybe used by another process."), Logging.LogFilePath),
                    I18N.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            controller = new MainController();
            MenuViewController viewController = new MenuViewController(controller);
            controller.Start();
            Application.ApplicationExit += Application_ApplicationExit;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            Application.Run();
        }

        private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    Console.WriteLine("os wake up");
                    if (controller != null)
                    {
                        System.Timers.Timer timer = new System.Timers.Timer(5000);
                        timer.Elapsed += Timer_Elapsed;
                        timer.AutoReset = false;
                        timer.Enabled = true;
                        timer.Start();
                    }
                    break;
                case PowerModes.Suspend:
                    if (controller != null)
                        controller.Stop();
                    Console.WriteLine("os suspend");
                    break;
            }
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (controller != null)
                    controller.Start();
            }
            catch (Exception ex)
            {
                Logging.LogUsefulException(ex);
            }
            finally
            {
                try
                {
                    System.Timers.Timer timer = (System.Timers.Timer)sender;
                    timer.Enabled = false;
                    timer.Stop();
                    timer.Dispose();
                }
                catch (Exception ex)
                {
                    Logging.LogUsefulException(ex);
                }
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (controller != null)
            {
                controller.Stop();
                controller = null;
            }
        }
    }
}
