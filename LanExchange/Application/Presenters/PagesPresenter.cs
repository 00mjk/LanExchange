﻿using System;
using LanExchange.Application.Interfaces;
using LanExchange.Application.Interfaces.EventArgs;
using LanExchange.Application.Interfaces.Extensions;
using LanExchange.Presentation.Interfaces;

namespace LanExchange.Application.Presenters
{
    internal sealed class PagesPresenter : PresenterBase<IPagesView>, IPagesPresenter
    {
        private readonly IPagesModel model;
        private readonly IPagesPersistenceService pagesService;
        private readonly IImageManager imageManager;
        private readonly IPanelColumnManager panelColumns;
        private readonly IClipboardService clipboardService;

        public event EventHandler PanelViewFocusedItemChanged;
        public event EventHandler PanelViewFilterTextChanged;

        public PagesPresenter(
            IPagesModel model, 
            IPagesPersistenceService pagesService,
            IImageManager imageManager,
            IPanelColumnManager panelColumns,
            IClipboardService clipboardService)
        {
            if (model != null) throw new ArgumentNullException(nameof(model));
            if (pagesService != null) throw new ArgumentNullException(nameof(pagesService));
            if (imageManager != null) throw new ArgumentNullException(nameof(imageManager));
            if (panelColumns != null) throw new ArgumentNullException(nameof(panelColumns));
            if (clipboardService != null) throw new ArgumentNullException(nameof(clipboardService));

            this.model = model;
            this.pagesService = pagesService;
            this.imageManager = imageManager;
            this.panelColumns = panelColumns;
            this.clipboardService = clipboardService;

            this.model.PanelAdded += ModelOnPanelAdded;
            this.model.PanelRemoved += ModelOnPanelRemoved;
            this.model.SelectedIndexChanged += ModelOnSelectedIndexChanged;
            this.model.Cleared += ModelOnCleared;
        }

        public bool CanSendToNewTab()
        {
            var sourcePanel = View.ActivePanelView;
            if (sourcePanel == null) 
                return false;
            var indexes = sourcePanel.SelectedIndexes.GetEnumerator();
            if (!indexes.MoveNext()) 
                return false;

            return false;
            //TODO hide model
            //return sourcePanel.Presenter.Objects.Count > 1;
        }

        public void CommandSendToNewTab()
        {
            //if (!CanSendToNewTab()) return;
            //var newTabName = m_Model.GenerateTabName();
            //var sourcePV = View.ActivePanelView;
            //var sourceObjects = sourcePV.Presenter.Objects;
            //var destObjects = App.Resolve<IPanelModel>();
            //destObjects.TabName = newTabName;
            //destObjects.DataType = sourceObjects.DataType;
            ////destObjects.CurrentPath.Push(PanelItemRoot.ROOT_OF_USERITEMS);

            //foreach (int index in sourcePV.SelectedIndexes)
            //{
            //    var panelItem = sourceObjects.GetItemAt(index);
            //    if (panelItem.GetType().Name.Equals(destObjects.DataType))
            //    {
            //        // add item to new panel
            //        var newItem = (PanelItemBase) panelItem.Clone();
            //        newItem.Parent = null;
            //        destObjects.Items.Add(newItem);
            //    }
            //}
            ////destObjects.SyncRetrieveData(true);
            //// add tab
            //m_Model.AddTab(destObjects);
            ////m_View.SelectedIndex = m_Model.Count - 1;
            //View.ActivePanelView.Presenter.UpdateItemsAndStatus();
        }

        public bool CanPasteItems()
        {
            if (View.ActivePanelView == null)
                return false;
            var obj = clipboardService.GetDataObject();
            if (obj == null)
                return false;
            if (!obj.GetDataPresent(typeof(PanelItemBaseHolder)))
                return false;
            var items = (PanelItemBaseHolder)obj.GetData(typeof(PanelItemBaseHolder));
            if (items == null)
                return false;

            return false;
            //TODO hide model
            //return !View.ActivePanelView.Presenter.Objects.TabName.Equals(items.Context);
        }

        public void CommandPasteItems()
        {
            if (!CanPasteItems()) return;
            var obj = clipboardService.GetDataObject();
            if (obj == null) return;
            var items = (PanelItemBaseHolder)obj.GetData(typeof(PanelItemBaseHolder));
            var destObjects = model.GetAt(model.SelectedIndex);
            destObjects.DataType = items.DataType;
            //destObjects.CurrentPath.Push(PanelItemRoot.ROOT_OF_USERITEMS);
            foreach (var panelItem in items)
                if (panelItem.GetType().Name.Equals(destObjects.DataType))
                {
                    if (destObjects.Contains(panelItem))
                        continue;
                    // add item to new panel
                    var newItem = (PanelItemBase) panelItem.Clone();
                    //newItem.Parent = PanelItemRootBase.ROOT_OF_USERITEMS;
                    destObjects.Items.Add(newItem);
                }
            destObjects.AsyncRetrieveData(true);
            //m_View.ActivePanelView.Presenter.UpdateItemsAndStatus();
        }

        public void CommandDeleteItems()
        {
            // TODO hide model
            //var panelView = View.ActivePanelView;
            //if (panelView == null) return;
            //var indexes = panelView.SelectedIndexes.GetEnumerator();
            //if (!indexes.MoveNext()) return;


            //var modified = false;
            //var firstIndex = -1;

            //foreach (int index in panelView.SelectedIndexes)
            //{
            //    var comp = panelView.Presenter.Objects.GetItemAt(index);
            //    if (comp.ImageName.Contains(PanelImageNames.GREEN_POSTFIX) || comp.ImageName.Contains(PanelImageNames.HIDDEN_POSTFIX))
            //    {
            //        if (firstIndex == -1)
            //            firstIndex = index-1;
            //        panelView.Presenter.Objects.Items.Remove(comp);
            //        modified = true;
            //    }
            //}
            //panelView.ClearSelected();
            //if (modified)
            //{
            //    if (firstIndex < 0 || firstIndex > panelView.Presenter.Objects.FilterCount - 1)
            //        firstIndex = panelView.Presenter.Objects.FilterCount - 1;
            //    if (firstIndex >= 0)
            //        panelView.Presenter.Objects.FocusedItem = panelView.Presenter.Objects.GetItemAt(firstIndex);
            //    panelView.Presenter.Objects.AsyncRetrieveData(false);
            //}
        }

        public void CommandCloseTab()
        {
            var index = View.PopupSelectedIndex;
            model.RemoveAt(index);
            if (index > model.Count - 1)
                model.SelectedIndex = model.Count - 1;
        }

        public void CommanCloseOtherTabs()
        {
            var panel = model.GetAt(View.PopupSelectedIndex);
            model.Clear();
            model.Add(panel);
            model.SelectedIndex = 0;
        }

        public void CommandReRead()
        {
            var pageModel = model.GetAt(SelectedIndex);
            // clear refreshable columns
            if (pageModel.DataType != null)
                foreach (var column in panelColumns.GetColumns(pageModel.DataType))
                    if (column.Callback != null && column.Refreshable)
                        column.LazyDict.Clear();
            //var result = pageModel.RetrieveData(RetrieveMode.Sync, false);
            //pageModel.SetFillerResult(result, false);
            pageModel.AsyncRetrieveData(false);
        }

        public void DoPagesReRead()
        {
            //commandManager.ExecuteCommand<PagesReReadCommand>();
        }

        public void DoPagesCloseTab()
        {
            //commandManager.ExecuteCommand<PagesCloseTabCommand>();
        }

        public void DoPagesCloseOther()
        {
            //commandManager.ExecuteCommand<PagesCloseOtherCommand>();
        }

        public void SetTabImageForModel(IPanelModel theModel, string imageName)
        {
            if (theModel == null) return;
            var index = IndexOf(theModel);
            if (index != -1)
                View.SetTabImage(index, imageManager.IndexOf(imageName));
        }

        public int IndexOf(IPanelModel theModel)
        {
            if (theModel == null)
                return -1;
            for (int index = 0; index < this.model.Count; index++)
                if (model.GetAt(index) == theModel)
                    return index;
            return -1;
        }

        public void ModelOnPanelAdded(object sender, PanelEventArgs e)
        {
            //// TODO hide model
            //// create panel
            //var panelView = View.CreatePanelView(e.Panel);
            //// set update event
            //IPanelPresenter presenter = panelView.Presenter;
            //presenter.Objects = e.Panel;

            ////m_View.SelectedIndex = m_View.TabPagesCount - 1;
            //e.Panel.Changed += (o, args) => presenter.UpdateItemsAndStatus();
            //e.Panel.TabNameUpdated += InfoOnTabNameUpdated;
            //e.Panel.OnTabNameUpdated();
            ////e.Info.SubscriptionChanged += Item_SubscriptionChanged;
            //// update items
            ////e.Info.DataChanged(null, ConcreteSubject.s_UserItems);
            //panelView.Presenter.ResetSortOrder();
            //e.Panel.AsyncRetrieveData(false);
        }

        private void InfoOnTabNameUpdated(object sender, EventArgs eventArgs)
        {
            var model = sender as IPanelModel;
            if (model == null) return;
            var index = IndexOf(model);
            if (index != -1)
            {
                View.SetTabText(index, model.TabName);
                View.SetTabImage(index, imageManager.IndexOf(model.ImageName));
            }
        }

        public void ModelOnPanelRemoved(object sender, PanelIndexEventArgs e)
        {
            View.RemoveTabAt(e.Index);
        }

        public void Model_AfterRename(object sender, PanelIndexEventArgs e)
        {
            var model = this.model.GetAt(e.Index);
            View.SetTabText(e.Index, model.TabName);
        }

        public void ModelOnSelectedIndexChanged(object sender, PanelIndexEventArgs e)
        {
            View.SelectedIndex = e.Index;
            View.FocusPanelView();
        }

        private void ModelOnCleared(object sender, EventArgs e)
        {
            View.Clear();
        }

        public void DoPanelViewFocusedItemChanged(object sender, EventArgs e)
        {
            PanelViewFocusedItemChanged?.Invoke(sender, e);
        }

        public void DoPanelViewFilterTextChanged(object sender, EventArgs e)
        {
            PanelViewFilterTextChanged?.Invoke(sender, e);
        }

        public bool SelectTabByName(string tabName)
        {
            for (int index = 0; index < model.Count; index++ )
                if (model.GetAt(index).TabName.Equals(tabName))
                {
                    SelectedIndex = index;
                    return true;
                }
            return false;
        }

        public int Count
        {
            get { return model.Count; }
        }

        public int SelectedIndex
        {
            get { return model.SelectedIndex; }
            set { model.SelectedIndex = value; }
        }

        public IPanelView ActivePanelView
        {
            get { return View.ActivePanelView; }
        }

        public PanelViewMode ViewMode
        {
            get { return View.ActivePanelView?.ViewMode ?? PanelViewMode.Details; }
            set
            {
                if (View.ActivePanelView != null)
                    View.ActivePanelView.ViewMode = value;
            }
        }

        public void SaveInstant()
        {
            pagesService.SavePages(model.ToDto());
        }

        public void SetupPanelViewEvents(IPanelView panelView)
        {
            panelView.FocusedItemChanged += DoPanelViewFocusedItemChanged;
            panelView.FilterTextChanged += DoPanelViewFilterTextChanged;
        }

        public void LoadSettings()
        {
            model.Assign(pagesService.LoadPages());
        }

        public void UpdateTabName(int index)
        {
            var model = this.model.GetAt(index);
            if (model != null)
                View.SetTabText(index, model.TabName);
        }

        private bool escapeDown;

        public bool PerformEscapeDown()
        {
            var panelView = View.ActivePanelView;
            if (panelView == null) return false;

            if (escapeDown)
            {
                escapeDown = false;
                return false;
            }

            // hide filter panel
            if (panelView.Filter.IsVisible)
                panelView.Filter.SetFilterText(string.Empty);

            escapeDown = true;
            return true;
        }

        public bool PerformEscapeUp()
        {
            if (!escapeDown)
                return false;

            var panelView = View.ActivePanelView;
            panelView.Presenter.CommandLevelUp();

            escapeDown = false;
            return true;
        }

        protected override void InitializePresenter()
        {
        }
    }
}