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
            builder.RegisterType<DeathByCaptchaService>()
                .As<ICaptchaService>()
                .SingleInstance()
                .WithParameter("debug", true);

            var container = builder.Build();
            var csl = new Autofac.Extras.CommonServiceLocator.AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);
        }
    }
}