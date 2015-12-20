using System;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle.Scoped;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace Closure
{
    public static class ComponentRegistrationExtensions
    {
        public static ComponentRegistration<T> Closure<T>(
            this ComponentRegistration<T> component,
            MethodInfo closure) where T : class
        {
            component.UsingFactoryMethod(k =>
            {
                var dependencies = closure.GetParameters().Select(x => k.Resolve(x.ParameterType)).ToArray();
                return (T)closure.Invoke(null, dependencies);
            });

            return component;
        }
    }

    public static class Closure
    {
        public static ClosureRegistration<T> For<T>()
        {
            return new ClosureRegistration<T>();
        }
    }

    public static class WindsorContainerExtensions
    {
        public static IWindsorContainer Register<T>(this IWindsorContainer container,
            ClosureRegistration<T> closureRegistration)
        {
            foreach (var registration in closureRegistration.Registrations)
            {
                container.Register(registration);
            }
            return container;
        }
    }

    public class ClosureRegistration<T>
    {
        public ComponentRegistration<object>[] Registrations { get; private set; }

        public ClosureRegistration()
        {
            Registrations = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Select(closure =>
                    new ComponentRegistration(closure.ReturnType)
                        .UsingFactoryMethod(k =>
                        {
                            var dependencies = closure.GetParameters().Select(x => k.Resolve(x.ParameterType)).ToArray();
                            return closure.Invoke(null, dependencies);
                        })).ToArray();
        }

        public ClosureRegistration<T> LifestyleCustom(Type customLifestyleType)
        {
            Registrations = Registrations.Select(x => x.LifestyleCustom(customLifestyleType)).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestyleCustom<TLifestyleManager>()
            where TLifestyleManager : ILifestyleManager, new()
        {
            Registrations = Registrations.Select(x => x.LifestyleCustom<TLifestyleManager>()).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestylePerThread()
        {
            Registrations = Registrations.Select(x => x.LifestylePerThread()).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestyleScoped(Type scopeAccessorType = null)
        {
            Registrations = Registrations.Select(x => x.LifestyleScoped(scopeAccessorType)).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestyleScoped<TScopeAccessor>() where TScopeAccessor : IScopeAccessor, new()
        {
            Registrations = Registrations.Select(x => x.LifestyleScoped<TScopeAccessor>()).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestyleBoundTo<TBaseForRoot>() where TBaseForRoot : class
        {
            Registrations = Registrations.Select(x => x.LifestyleBoundTo<TBaseForRoot>()).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestyleBoundToNearest<TBaseForRoot>() where TBaseForRoot : class
        {
            Registrations = Registrations.Select(x => x.LifestyleBoundToNearest<TBaseForRoot>()).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestyleBoundTo(Func<IHandler[], IHandler> scopeRootBinder)
        {
            Registrations = Registrations.Select(x => x.LifestyleBoundTo(scopeRootBinder)).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestylePerWebRequest()
        {
            Registrations = Registrations.Select(x => x.LifestylePerWebRequest()).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestylePooled(int? initialSize = null, int? maxSize = null)
        {
            Registrations = Registrations.Select(x => x.LifestylePooled(initialSize, maxSize)).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestyleSingleton()
        {
            Registrations = Registrations.Select(x => x.LifestyleSingleton()).ToArray();
            return this;
        }

        public ClosureRegistration<T> LifestyleTransient()
        {
            Registrations = Registrations.Select(x => x.LifestyleTransient()).ToArray();
            return this;
        }
    }
}
