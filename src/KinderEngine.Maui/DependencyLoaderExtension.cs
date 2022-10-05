using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using KinderEngine.Maui.DependencyInjection;

namespace KinderEngine.Maui
{
    public static class DependencyLoaderExtension
    {
        public static MauiAppBuilder UseKinderDependencyLoaders(this MauiAppBuilder builder, params Assembly[] assemblys)
        {
            foreach (var v in assemblys)
                builder.UseKinderDependencyLoader(v);
            return builder;
        }

        private static void UseKinderDependencyLoader(this MauiAppBuilder builder, Assembly assembly)
        {
            foreach (var vm in assembly.GetTypesWithAttribute<RegisterAttribute>())
                builder.RegisterDependency(vm.Attribute.Lifetime, vm.ClassType);

            foreach (var vm in assembly.GetTypesWithAttribute<RegisterViewModelAttribute>())
                builder.RegisterDependency(vm.Attribute.Lifetime, vm.ClassType, vm.Attribute.ViewTarget);
        }

        private static void RegisterDependency(this MauiAppBuilder builder, DependencyLifetime l, Type t)
        {
            switch (l)
            {
                case DependencyLifetime.Singleton:
                    builder.Services.AddSingleton(t);
                    break;
                case DependencyLifetime.Transient:
                    builder.Services.AddTransient(t);
                    break;
                case DependencyLifetime.Scoped:
                    builder.Services.AddScoped(t);
                    break;
            }
        }
        private static void RegisterDependency(this MauiAppBuilder builder, DependencyLifetime l, Type t1, Type t2)
        {
            switch (l)
            {
                case DependencyLifetime.Singleton:
                    builder.Services.AddSingleton(t1);
                    builder.Services.AddSingleton(t2);
                    break;
                case DependencyLifetime.Transient:
                    builder.Services.AddTransient(t1);
                    builder.Services.AddTransient(t2);
                    break;
                case DependencyLifetime.Scoped:
                    builder.Services.AddScoped(t1);
                    builder.Services.AddScoped(t2);
                    break;
            }
        }
    }
}
