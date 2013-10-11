﻿namespace LanExchange.Intf
{
    public interface IAboutPresenter : IPresenter<IAboutView>
    {
        void LoadFromModel();

        void OpenWebLink();

        void OpenTwitterLink();

        void OpenEmailLink();

        void OpenTranslationWebLink();

        void OpenBugtrackerWebLink();
    }
}
