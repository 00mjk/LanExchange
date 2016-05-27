﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanExchange.SDK.Factories
{
    /// <summary>
    /// The model factory interface.
    /// </summary>
    public interface IModelFactory
    {
        /// <summary>
        /// Creates the panel model.
        /// </summary>
        /// <returns></returns>
        IPanelModel CreatePanelModel();
    }
}