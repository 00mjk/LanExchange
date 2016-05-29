﻿namespace LanExchange.SDK.Factories
{
    /// <summary>
    /// The service factory interface.
    /// </summary>
    public interface IServiceFactory
    {
        /// <summary>
        /// Creates the system image list service.
        /// </summary>
        /// <returns></returns>
        ISysImageListService CreateSysImageListService();
    }
}