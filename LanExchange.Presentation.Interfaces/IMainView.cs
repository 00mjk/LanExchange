﻿using System;

namespace LanExchange.Presentation.Interfaces
{
    public interface IMainView : IWindow
    {
        IntPtr Handle { get; }
        string TrayText { get; set; }
        bool TrayVisible { get; set; }
        string ShowWindowKey { get; set; }

        void ShowStatusText(string format, params object[] args);
        void SetToolTip(object control, string tipText);

        void SetupMenuLanguages();

        void SetupPages();
    }
}