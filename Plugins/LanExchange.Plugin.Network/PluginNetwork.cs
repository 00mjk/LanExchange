﻿using System;
using System.ComponentModel.Composition;

using LanExchange.Plugin.Network.Properties;
using LanExchange.SDK;
using LanExchange.SDK.Extensions;

namespace LanExchange.Plugin.Network
{
    [Export(typeof(IPlugin))]
    public sealed class PluginNetwork : IPlugin
    {
        public static IMACAddressSerivice macAddressService { get; private set; }

        public void Initialize(IServiceProvider serviceProvider)
        {         
            // Setup resource manager
            var translationService = serviceProvider.Resolve<ITranslationService>();
            translationService.SetResourceManagerTo<Resources>();

            // Register new panel item types
            var factoryManager = serviceProvider.Resolve<IPanelItemFactoryManager>();
            factoryManager.RegisterFactory<DomainRoot>(new PanelItemRootFactory<DomainRoot>());
            factoryManager.RegisterFactory<DomainPanelItem>(new DomainFactory());
            factoryManager.RegisterFactory<ComputerPanelItem>(new ComputerFactory());
            factoryManager.RegisterFactory<SharePanelItem>(new ShareFactory());

            // Register new panel fillers
            var fillerManager = serviceProvider.Resolve<IPanelFillerManager>();
            fillerManager.RegisterFiller<DomainPanelItem>(new DomainFiller());
            fillerManager.RegisterFiller<ComputerPanelItem>(new ComputerFiller());
            fillerManager.RegisterFiller<SharePanelItem>(new ShareFiller());

            macAddressService = serviceProvider.Resolve<IMACAddressSerivice>();
        }
    }
}