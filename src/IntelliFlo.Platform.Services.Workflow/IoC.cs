using System;
using System.Collections.Generic;
using Autofac;

namespace IntelliFlo.Platform.Services.Workflow
{
    /// <summary>
    /// Because the Workflow engine doesn't allow hooking into IoC only its own service locator pattern.
    /// As a simpler alternative we will use our own service locator instead
    /// </summary>
    public static class IoC
    {
        private const string Default = "default";
        private static readonly IDictionary<string, IContainer> containers = new Dictionary<string, IContainer>();

        public static void Initialize(IContainer container)
        {
            Initialize(Default, container);
        }

        public static void Initialize(string containerId, IContainer container)
        {
            if (containers.ContainsKey(containerId))
                containers.Remove(containerId);

            containers.Add(containerId, container);
        }

        /// <summary>
        /// Resolves objects of type <see cref="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
        {
            return Resolve<T>(Default);
        }

        public static T Resolve<T>(string containerId)
        {
            IContainer container = null;
            if (containers.ContainsKey(containerId))
                container = containers[containerId];

            return container.Resolve<T>();
        }

        /// <summary>
        /// Returns the container
        /// </summary>
        public static IContainer Container
        {
            get { return GetContainer(Default); }
        }

        public static IContainer GetContainer(string containerId)
        {
            IContainer container = null;
            if (containers.ContainsKey(containerId)) 
                container = containers[containerId];
            
            if (container == null)
                throw new InvalidOperationException("The container has not been initialized!");
            return container;
        }


        /// <summary>
        /// Returns <c>true</c> if the container is initialised.
        /// </summary>
        public static bool IsInitialized
        {
            get { return IsIntialized(Default); }
        }

        public static bool IsIntialized(string containerId)
        {
            IContainer container = null;
            if (containers.ContainsKey(containerId))
                container = containers[containerId];

            return container != null;
        }

        /// <summary>
        /// Resets the container by setting it to null
        /// </summary>
        public static void Reset()
        {
            Reset(Default);
        }

        /// <summary>
        /// Resets the container by setting it to null
        /// </summary>
        public static void Reset(string containerId)
        {
            if (containers.ContainsKey(containerId))
                containers.Remove(containerId);
        }
    }
}
