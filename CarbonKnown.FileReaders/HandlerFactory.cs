using System;
using System.Collections.Generic;
using System.Linq;
using CarbonKnown.FileReaders.FileHandler;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace CarbonKnown.FileReaders
{
    public class HandlerFactory : IHandlerFactory
    {
        private readonly IUnityContainer container;

        public HandlerFactory(IUnityContainer container)
        {
            this.container = container;
        }

        public static void RegisterHandlers(IUnityContainer container)
        {
            container.RegisterType<IHandlerFactory, HandlerFactory>();
            foreach (var handlerType in HandlerTypes())
            {
                container.RegisterType(
                    typeof (IFileHandler),
                    handlerType.Value,
                    handlerType.Key,
                    new InterceptionBehavior<PolicyInjectionBehavior>(),
                    new Interceptor<InterfaceInterceptor>());
            }
        }

        public IFileHandler CreateHandler(string handlerName)
        {
            return CreateHandler(handlerName, string.Empty);
        }

        public  IFileHandler CreateHandler(string handlerName, string host)
        {
            return container
                       .IsRegistered(typeof (IFileHandler), handlerName)
                       ? container.Resolve<IFileHandler>(handlerName, new ParameterOverride("host", host))
                       : null;
        }

        public static IDictionary<string, Type> HandlerTypes()
        {
            return AssemblyExtentions
                .AllAssemblyTypes<IFileHandler>()
                .Where(t =>
                    !t.IsInterface &&
                    !t.IsAbstract &&
                    (t.GetConstructors()
                        .Any(info =>
                            info.IsPublic &&
                            info.GetParameters().Any() &&
                            info.GetParameters()[0].ParameterType == typeof (string))))
                .ToDictionary(t => t.Name.Replace("Handler", string.Empty), t => t);
        }

        private static void RegisterHandler<THandler>(IUnityContainer container, string handlerName)
            where THandler : IFileHandler
        {
            container.RegisterType<IFileHandler, THandler>(
                handlerName,
                new InterceptionBehavior<PolicyInjectionBehavior>(),
                new Interceptor<InterfaceInterceptor>());

        }
    }
}
