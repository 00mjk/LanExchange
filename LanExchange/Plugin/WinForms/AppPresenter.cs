﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using LanExchange.SDK;
using LanExchange.Interfaces.Services;
using System.Diagnostics.Contracts;
using LanExchange.Presentation.Interfaces;

namespace LanExchange.Plugin.WinForms
{
    public class AppPresenter : IAppPresenter
    {
        private readonly IConfigPersistenceService configService;
        private readonly IPagesPresenter pagesPresenter;
        private readonly ITranslationService translationService;
        private readonly IDisposableManager disposableManager;
        private IMainView mainView;

        public AppPresenter(
            IConfigPersistenceService configService,
            IPagesPresenter pagesPresenter,
            ITranslationService translationService,
            IDisposableManager disposableManager)
        {
            Contract.Requires<ArgumentNullException>(configService != null);
            Contract.Requires<ArgumentNullException>(pagesPresenter != null);
            Contract.Requires<ArgumentNullException>(translationService != null);
            Contract.Requires<ArgumentNullException>(disposableManager != null);

            this.configService = configService;
            this.pagesPresenter = pagesPresenter;
            this.translationService = translationService;
            this.disposableManager = disposableManager;
        }

        public void Init()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.ThreadException += ApplicationOnThreadException;
            Application.ThreadExit += ApplicationOnThreadExit;
            Application.EnableVisualStyles();
            // must be called before first form created
            Application.SetCompatibleTextRenderingDefault(false);
        }

        public void Run(IMainView view)
        {
            mainView = view;
            Application.Run((Form)view);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            mainView.ApplicationExit();
        }

        [Localizable(false)]
        private static void ApplicationOnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            #if DEBUG
            Debug.Fail(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
            #else
            App.MainView.ApplicationExit();
            #endif
        }

        private void ApplicationOnThreadExit(object sender, EventArgs e)
        {
            pagesPresenter.SaveInstant();
            configService.Save(App.Config);
            // dispose instances registered in plugins
            disposableManager.Dispose();
        }

        public void TranslateOpenForms()
        {
            // need for changing rtl
            var forms = Application.OpenForms.Cast<Form>().ToList();
            // enum opened forms
            foreach (var form in forms)
                if (form is ITranslationable)
                {
                    // set rtl
                    var rtlChanged = translationService.RightToLeft != form.RightToLeftLayout;
                    if (rtlChanged)
                        form.Hide();
                    if (translationService.RightToLeft)
                    {
                        form.RightToLeftLayout = true;
                        form.RightToLeft = RightToLeft.Yes;
                    }
                    else
                    {
                        form.RightToLeftLayout = false;
                        form.RightToLeft = RightToLeft.No;
                    }
                    (form as ITranslationable).TranslateUI();
                    if (rtlChanged)
                        form.Show();
                }
        }
    }
}