﻿using System;
using System.Threading;
using System.Windows.Forms;

namespace LanExchange.UI
{
    public static class AppView
    {
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            #if DEBUG
            MessageBox.Show(e.Exception.Message+"\n"+e.Exception.StackTrace,
              "Error in "+e.Exception.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
            #endif
            MainForm.Instance.ApplicationExit();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MainForm.Instance.ApplicationExit();
        }

        public static void ApplicationRun()
        {
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;            
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); // must be called before first form created
            Application.Run(new MainForm());
        }
    }
}
