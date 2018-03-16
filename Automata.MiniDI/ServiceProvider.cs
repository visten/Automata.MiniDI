﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Automata.MiniDI
{
    public class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();
        private IEnumerable<ServiceDescriptor> _serviceDescriptors;

        public ServiceProvider(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            _serviceDescriptors = serviceDescriptors;
        }

        public object GetService(Type serviceType)
        {
            lock (_cache)
            {
                if (_cache.ContainsKey(serviceType))
                {
                    return _cache[serviceType];
                }

                return CreateInstance(serviceType);
            }
        }

        private object CreateInstance(Type serviceType)
        {
            ServiceDescriptor serviceDescriptor = null;
            if (!serviceType.IsInterface && _serviceDescriptors.Any(m => serviceType.IsAssignableFrom(m.ImplementationType)))
            {
                serviceDescriptor = _serviceDescriptors.First(m => serviceType.IsAssignableFrom(m.ImplementationType));
            }
            else if (serviceType.IsInterface)
            {
                serviceDescriptor = _serviceDescriptors.First(m => serviceType.FullName.Equals(m.ServiceType.FullName));
            }

            object serviceInstance = null;

            if (serviceDescriptor.ImplementationInstance == null || serviceDescriptor.Lifetime == ServiceLifetime.Transient)
            {
                serviceInstance = ActivatorUtilities.CreateInstance(this, serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType);
            }
            else
            {
                serviceInstance = serviceDescriptor.ImplementationInstance;
            }

            if (serviceDescriptor.Lifetime == ServiceLifetime.Singleton)
            {
                _cache.Add(serviceType, serviceInstance);
            }

            return serviceInstance;

            //throw new Exception(nameof(serviceType) + " 不存在");
        }
    }
}