using System;
using Autofac;
using YoloTrain.Mvvm.ApplicationServices;
using YoloTrain.Views;

namespace YoloTrain.Mvvm
{
    public static class IoC
    {
        private static readonly IContainer _container;

        static IoC()
        {
            var builder = new ContainerBuilder();

            // services
            builder.RegisterInstance<IDispatcher>(new ApplicationDispatcher());
            builder.RegisterInstance<IDialogService>(new DialogService());

            // view models
            builder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>();

            // views
            builder.RegisterType<MainWindow>();

            _container = builder.Build();
        }

        public static T Resolve<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
