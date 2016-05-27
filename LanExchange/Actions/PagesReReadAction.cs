﻿using LanExchange.SDK;

namespace LanExchange.Actions
{
    internal sealed class PagesReReadAction : PagesActionBase
    {
        public PagesReReadAction(IPagesPresenter pagesPresenter) : base(pagesPresenter)
        {
        }

        public override void Execute()
        {
            pagesPresenter.CommandReRead();
        }
    }
}