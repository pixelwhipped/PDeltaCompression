using System;

namespace Compression.Services
{
    /// <summary>
    /// 	Defines an object that provides a service instance or instances.
    /// </summary>
    public interface IServiceProvider<out TService>
    {
        /// <summary>
        /// 	Gets the instance of the service.
        /// </summary>
        TService Instance { get; }
    }

    /// <summary>
    /// 	Standard Service Provider implementation.
    /// 	<code language = "c#">
    /// 		public class Service : ServiceProvider
    /// 		{
    /// 		public Service() : base()
    /// 		{
    /// 		}
    /// 		}
    /// 		public static void TestServiceProvider()
    /// 		{
    /// 		Console.Out.WriteLine("TestServiceProvider()");
    /// 		Service a = new Service();          // Acceptable.
    /// 		Service b = new Service();          // Acceptable.
    /// 		Service c = a.Instance;             // Acceptable calls the Service.GetInstance().
    /// 		Service d = Service.GetInstance();  // Best practice.            
    /// 		}</code>
    /// </summary>
    public class ServiceProvider<TService> : IServiceProvider<TService> where TService : new()
    {
        #region IServiceProvider<TService> Members

        /// <summary>
        /// 	Accessor to a new instance.
        /// </summary>
        public TService Instance
        {
            get { return GetInstance(); }
        }

        #endregion

        /// <summary>
        /// 	Gets the instance of the service.
        /// </summary>
        public static TService GetInstance()
        {
            return Activator.CreateInstance<TService>();
        }
    }

    /// <summary>
    /// 	Restricts the instantiation of a class to one object at any given time.
    /// 	<code language = "c#">
    /// 		class CachedService : CachedServiceProvider
    /// 		{
    /// 		public CachedService() : base()
    /// 		{
    /// 		}
    /// 		}
    /// 		public static void TestCachedServiceProvider()
    /// 		{
    /// 		Console.Out.WriteLine("TestCachedServiceProvider()");
    /// 		CachedService a;
    /// 		CachedService b;
    /// 		try
    /// 		{
    /// 		a = new CachedService();      // Allowed.
    /// 		b = new CachedService();      // Not allowed as instance exists.
    /// 		}
    /// 		catch (TypeAccessException exception)
    /// 		{
    /// 		Console.Out.WriteLine("Expected Excetion as: " + exception.Message);
    /// 		a = null;
    /// 		b = null;
    /// 		CachedService c = CachedService.GetInstance();  // Best practice.
    /// 		c = null;                
    /// 		CachedService d = new CachedService();          // Acceptable as no references exist.
    /// 		}
    /// 		}</code>
    /// </summary>
    public class CachedServiceProvider<TService> : IServiceProvider<TService>
        where TService : CachedServiceProvider<TService>
    {
        /// <summary>
        /// 	Instance.
        /// </summary>
        // ReSharper disable StaticFieldInGenericType
        private static volatile WeakReference _instance = new WeakReference(null);

        // ReSharper restore StaticFieldInGenericType

        /// Constructor protected to avoid any possible calls such as.
        /// <code language = "c#">
        /// 	CachedService[] providers = new CachedService[]{new CachedService(),new CachedService()};   //Not allowed as 1 instance of the type already exists.
        /// </code>
        /// <exception cref="TypeAccessException"><c>TypeAccessException</c>.</exception>
        protected CachedServiceProvider()
        {
            GC.Collect();
            if (_instance.Target != null) throw new TypeAccessException();
            _instance.Target = this;
        }

        #region IServiceProvider<TService> Members

        /// <summary>
        /// 	Accessor to the instance.
        /// </summary>
        public TService Instance
        {
            get { return GetInstance(); }
        }

        #endregion

        /// <summary>
        /// 	Gets the instance of the chached service.
        /// </summary>
        /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
        public static TService GetInstance()
        {
            if (_instance.Target == null)
                //throw new InvalidOperationException(Globalization.Validation.OperationUnavailabe);
                _instance.Target = Activator.CreateInstance<TService>();
            return (TService)_instance.Target;
        }
    }

    /// <summary>
    /// 	Singleton provider implementation.
    /// 	<code language = "c#">
    /// 		class SingletonService : SingletonServiceProvider
    /// 		{
    /// 		public SingletonService() : base()
    /// 		{
    /// 		}
    /// 		}
    /// 		public static void TestSingletonServiceProvider()
    /// 		{
    /// 		Console.Out.WriteLine("TestSingletonServiceProvider()");
    /// 		try
    /// 		{
    /// 		SingletonService a = new SingletonService();      // Allowed.
    /// 		SingletonService b = new SingletonService();      // Not allowed as instance exists.
    /// 		}
    /// 		catch (TypeAccessException exception)
    /// 		{
    /// 		Console.Out.WriteLine("Expected Excetion as: " + exception.Message);
    /// 		SingletonService c = SingletonService.GetInstance();  // Best practice.
    /// 		SingletonService d = c.Instance;                      // Acceptable.
    /// 		}  
    /// 		}</code>
    /// </summary>
    public class SingletonServiceProvider<TService> : IServiceProvider<TService>
        where TService : SingletonServiceProvider<TService>
    {
        /// <summary>
        /// 	Instance.
        /// </summary>
        // ReSharper disable StaticFieldInGenericType
        private static volatile TService _instance;

        // ReSharper restore StaticFieldInGenericType

        /// <summary>
        /// 	Lock.
        /// </summary>
        // ReSharper disable StaticFieldInGenericType
        private static readonly object Lock = new object();

        // ReSharper restore StaticFieldInGenericType

        /// <summary>
        /// 	Restricts the instantiation of a class to one object ever.
        /// 	Constructor protected to avoid any possible calls such as.
        /// 	<code language = "c#">
        /// 		Singleton[] providers = new Singleton[]{new Singleton(),new Singleton()};   // Would invalidate the premis for singleton.
        /// 	</code>
        /// </summary>
        /// <exception cref="TypeAccessException"><c>TypeAccessException</c>.</exception>
        protected SingletonServiceProvider()
        {
            if (_instance != null) throw new TypeAccessException();
            _instance = (TService)this;
        }

        #region IServiceProvider<TService> Members

        /// <summary>
        /// 	Accessor to the instance.
        /// </summary>
        public TService Instance
        {
            get { return GetInstance(); }
        }

        #endregion

        /// <summary>
        /// 	Gets the instance of the singleton.
        /// </summary>
        /// <exception cref="TypeAccessException"><c>TypeAccessException</c>.</exception>
        public static TService GetInstance()
        {
            if (_instance != null) return _instance;
            lock (Lock)
            {
                _instance = Activator.CreateInstance<TService>();
                if (_instance == null) throw new TypeAccessException();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 	Service Factory creates instances of a given service.
    /// </summary>
    public class ServiceFactory<TDefault> : CachedServiceProvider<ServiceFactory<TDefault>>
        where TDefault : IServiceProvider<TDefault>
    {
        /// <summary>
        /// 	Returns the preffered service.
        /// </summary>
        /// <returns>The service.</returns>
        public static TDefault GetService()
        {
            return GetService<TDefault>();
        }

        /// <summary>
        /// 	Constructs an instance of a service type.
        /// </summary>
        /// <returns>The service.</returns>
        public static TService GetService<TService>() where TService : IServiceProvider<TService>
        {
            return Activator.CreateInstance<TService>();
        }
    }
}