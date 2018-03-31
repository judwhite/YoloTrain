using Autofac;
using YoloTrain.Mvvm.ApplicationServices;
using YoloTrain.Views;
using YoloTrain.Views.Project;

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
            builder.RegisterInstance<IMainWindowViewModel>(new MainWindowViewModel());
            builder.RegisterType<NewProjectViewModel>().As<INewProjectViewModel>();

            // views
            builder.RegisterType<MainWindow>();
            builder.RegisterType<NewProjectWindow>();

            _container = builder.Build();
        }

        public static T Resolve<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
