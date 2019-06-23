using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Extras.Ordering;
using Brite.App.Win.Services;
using Brite.App.Win.ViewModels;

namespace Brite.App.Win
{
    public static class Bootstrapper
    {
        private static string _title;
        private static ILifetimeScope _rootScope;
        private static IShellViewModel _shellViewModel;

        public static IViewModel RootVisual
        {
            get
            {
                if (_rootScope == null)
                    throw new InvalidOperationException("Bootstrapper not initialized");

                var parameters = new Parameter[] { new NamedParameter("title", _title) };
                _shellViewModel = _rootScope.Resolve<IShellViewModel>(parameters);
                return _shellViewModel;
            }
        }

        public static void Initialize(string title)
        {
            if (_rootScope != null)
                return;

            _title = title;

            var builder = new ContainerBuilder();
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            // Register services
            builder.RegisterAssemblyTypes(assemblies)
                  .Where(t => typeof(IService).IsAssignableFrom(t))
                  .SingleInstance()
                  .AsImplementedInterfaces();

            // Register all non-transient types
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => typeof(IViewModel).IsAssignableFrom(t) && !typeof(ITransientViewModel).IsAssignableFrom(t))
                .AsImplementedInterfaces();

            // Register in-order types
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => typeof(IChildViewModel).IsAssignableFrom(t) && !typeof(ITransientViewModel).IsAssignableFrom(t))
                .OrderBy(t => ((IChildViewModel)t).Order)
                .AsImplementedInterfaces();

            // Several view model instances are transitory and created on the fly, if these are tracked by the container then they
            // won't be disposed of in a timely manner
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => typeof(IChildViewModel).IsAssignableFrom(t))
                .Where(t => typeof(ITransientViewModel).IsAssignableFrom(t))
                .OrderBy(t => ((IChildViewModel)t).Order)
                .AsImplementedInterfaces()
                .ExternallyOwned();

            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => typeof(IViewModel).IsAssignableFrom(t))
                .Where(t => typeof(ITransientViewModel).IsAssignableFrom(t))
                .Where(t => !typeof(IChildViewModel).IsAssignableFrom(t))
                .AsImplementedInterfaces()
                .ExternallyOwned();

            builder.RegisterSource(new OrderedRegistrationSource());

            _rootScope = builder.Build();
        }

        public static void Shutdown()
        {
            _rootScope.Dispose();
        }

        public static T Resolve<T>()
        {
            if (_rootScope == null)
            {
                throw new Exception("Bootstrapper hasn't been started!");
            }

            return _rootScope.Resolve<T>(new Parameter[0]);
        }

        public static T Resolve<T>(Parameter[] parameters)
        {
            if (_rootScope == null)
            {
                throw new Exception("Bootstrapper hasn't been started!");
            }

            return _rootScope.Resolve<T>(parameters);
        }
    }
}
