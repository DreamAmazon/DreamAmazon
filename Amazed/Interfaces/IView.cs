using System;
using System.Collections.Generic;
using System.Net;
using DreamAmazon.Models;

namespace DreamAmazon.Interfaces
{
    public delegate void VoidHandler();
    public delegate void VoidHandler<in T>(T args);

    public interface IBaseView
    {
        void Show();
        void ShowMessage(string text, MessageType type);
        SelectFileResult GetUserFileToSave(SelectFileOptions options);
        SelectFileResult GetUserFileToOpen(SelectFileOptions options);
    }

    public interface IMainView : IBaseView
    {
        event VoidHandler StartCheckingRequested;
        event VoidHandler StopCheckingRequested;
        event VoidHandler DisplaySettingsRequested;
        event VoidHandler AboutRequested;
        event VoidHandler ShowProxiesRequested;
        event VoidHandler LoadAccountsRequested;
        event VoidHandler<bool> CopyToClipboardRequested;
        event VoidHandler<bool> ExportToFileRequested;
        void DisplayAccount(Account account);
        void EnableStartRequest(bool b);
        void EnableExportRequest(bool b);
        void EnableStopRequest(bool b);
        void EnableLoadAccountsRequest(bool b);
        void EnableLoadProxiesRequest(bool b);
        void DisplayAccountsStatistic(AccountsStatistic stat);
        void DisplayProxiesStatistic(ProxiesStatistic stat);
        IEnumerable<Account> GetAccounts();
        IEnumerable<Account> GetSelectedAccounts();
        void ClearAccounts();
        void ShowBalance(double balance);
    }

    public interface ISettingsView : IBaseView
    {
        void BindSettings(SettingModel setting);
        event VoidHandler ValidateAccountRequested;
        void EnableValidateAccount(bool b);
    }

    public interface ILicenseView : IBaseView
    {
        void BindSettings(SettingModel setting);
        event EventHandler<string> ValidateLicenseRequested;
        void DisableFileds();
        void EnableFields();
        void EnableValidateLicense(bool b);
    }

    public interface IAboutView : IBaseView
    {
        
    }

    public interface IProxiesView : IBaseView
    {
        event VoidHandler ImportProxiesRequested;
        event VoidHandler ClearProxiesRequested;
        event VoidHandler TestProxiesRequested;
        void ClearProxies();
        void DisplayProxy(Uri proxyAddress);
        void RemoveProxy(Uri proxyAddress);
        void EnableTestProxiesRequest(bool b);
        void EnableImportProxiesRequested(bool b);
    }
}