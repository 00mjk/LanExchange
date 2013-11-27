﻿using System;
using LanExchange.SDK;

namespace LanExchange.Intf
{
    public interface IPagesModel : IModel
    {
        event EventHandler<PanelModelEventArgs> AfterAppendTab;
        event EventHandler<PanelIndexEventArgs> AfterRemove;
        event EventHandler<PanelModelEventArgs> AfterRename;
        event EventHandler<PanelIndexEventArgs> IndexChanged;

        bool AddTab(IPanelModel model);

        bool TabNameExists(string tabName);

        void DelTab(int index);

        string GenerateTabName();

        int Count { get; }
        int SelectedIndex { get; set; }
        IPanelModel GetItem(int index);
        void RenameTab(int index, string newTabName);
        string GetTabName(int index);

        void SaveSettings();
        void LoadSettings(out IPagesModel model);

        void SetLoadedModel(IPagesModel model);
    }
}
