using System;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using EPiServer.Core;

namespace PageTypeBuilder.Activation
{
    public class TypedPageActivator
    {
        ProxyGenerator _generator;
        ProxyGenerationOptions _options;
        IInterceptor[] _interceptors;

        public TypedPageActivator()
        {
            _generator = new ProxyGenerator();
            _options = new ProxyGenerationOptions(new PageTypePropertiesProxyGenerationHook());
            _interceptors = new IInterceptor[] 
                               {
                                   new PageTypePropertyInterceptor()
                               };
        }

        public virtual TypedPageData CreateAndPopulateTypedInstance(PageData originalPage, Type typedType)
        {
            TypedPageData typedPage = CreateInstance(typedType);
            TypedPageData.PopuplateInstance(originalPage, typedPage);
            return typedPage;
        }

        protected TypedPageData CreateInstance(Type typedType)
        {
            return (TypedPageData)_generator.CreateClassProxy(typedType, _options, _interceptors);
        }
    }
}