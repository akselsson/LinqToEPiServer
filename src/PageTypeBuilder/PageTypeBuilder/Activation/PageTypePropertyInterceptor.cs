using Castle.Core.Interceptor;
using PageTypeBuilder.Reflection;

namespace PageTypeBuilder.Activation
{
    internal class PageTypePropertyInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            TypedPageData page = (TypedPageData)invocation.InvocationTarget;
           
            string propertyName = invocation.Method.GetPropertyName();
            
            if (invocation.Method.IsGetter())
                invocation.ReturnValue = page.GetValue(propertyName);
            else
                page.SetValue(propertyName, invocation.Arguments[0]);
        }
    }
}
