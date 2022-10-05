namespace KinderEngine.Maui.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterViewModelAttribute : Attribute
    {
        public DependencyLifetime Lifetime { get; init; }
        public Type ViewTarget { get; init; }
        public RegisterViewModelAttribute(Type targetView, DependencyLifetime lifetime = DependencyLifetime.Singleton)
        {
            ViewTarget = targetView;
            Lifetime = lifetime;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterAttribute : Attribute
    {
        public DependencyLifetime Lifetime { get; init; }
        public RegisterAttribute( DependencyLifetime lifetime = DependencyLifetime.Singleton)
        {
            Lifetime = lifetime;
        }
    }


    public enum DependencyLifetime
    {
        Scoped,
        Singleton,
        Transient
    }
}
