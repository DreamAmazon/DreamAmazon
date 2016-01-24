using Autofac;
using DreamAmazon.Interfaces;
using DreamAmazon.Services;
using EventAggregatorNet;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon.Configs
{
    public static class DependencyConfig
    {
        public static void Config()
        {
            //todo: configure dependecies here
            var builder = new Autofac.ContainerBuilder();

            builder.RegisterType<Log4NetLogger>()
                .As<ILogger>()
                .SingleInstance();

            builder.RegisterType<EventAggregator>()
                .As<IEventAggregator>()
                .SingleInstance();

            builder.RegisterType<AccountManager>()
                .As<IAccountManager>()
                .SingleInstance();

            builder.RegisterType<DefaultSettingsService>()
                .As<ISettingsService>()
                .SingleInstance();

            builder.RegisterType<LoggedProxyManager>()
                .As<IProxyManager>()
                .SingleInstance()
                .WithParameter("proxyManager", new ProxyManager());

            builder
//#if DEBUG
//                .RegisterType<TestCaptchaService>()
//#else
                .RegisterType<DeathByCaptchaService>()
//#endif
                .As<ICaptchaService>()
                .SingleInstance()
//#if DEBUG
//                ;
//#else
                .WithParameter("debug", false);
//#endif

            var container = builder.Build();
            var csl = new Autofac.Extras.CommonServiceLocator.AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);
        }
    }
}