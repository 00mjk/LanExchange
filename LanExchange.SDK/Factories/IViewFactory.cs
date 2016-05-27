﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanExchange.SDK.Factories
{
    /// <summary>
    /// The view factory interface.
    /// </summary>
    public interface IViewFactory
    {
        /// <summary>
        /// Creates the panel view.
        /// </summary>
        /// <returns></returns>
        IPanelView CreatePanelView();

        /// <summary>
        /// Gets the pages view.
        /// TODO: need change to create
        /// </summary>
        /// <returns></returns>
        IPagesView GetPagesView();
    }
}