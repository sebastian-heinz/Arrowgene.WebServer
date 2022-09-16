using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Arrowgene.WebServer.Route
{
    /// <summary>
    ///     Implementation of Kestrel server as backend
    /// </summary>
    public abstract class WebRoute : IWebRoute
    {
        public abstract string Route { get; }

        public virtual Task<WebResponse> Get(WebRequest request)
        {
            return WebResponse.NotFound();
        }

        public virtual Task<WebResponse> Post(WebRequest request)
        {
            return WebResponse.NotFound();
        }

        public virtual Task<WebResponse> Put(WebRequest request)
        {
            return WebResponse.NotFound();
        }

        public virtual Task<WebResponse> Delete(WebRequest request)
        {
            return WebResponse.NotFound();
        }

        public virtual Task<WebResponse> Head(WebRequest request)
        {
            return WebResponse.NotFound();
        }

        public List<WebRequestMethod> GetMethods()
        {
            List<WebRequestMethod> methods = new List<WebRequestMethod>();
            if (IsMethodImplemented("Get"))
            {
                methods.Add(WebRequestMethod.Get);
            }
            if (IsMethodImplemented("Post"))
            {
                methods.Add(WebRequestMethod.Post);
            }
            if (IsMethodImplemented("Put"))
            {
                methods.Add(WebRequestMethod.Put);
            }
            if (IsMethodImplemented("Delete"))
            {
                methods.Add(WebRequestMethod.Delete);
            }
            if (IsMethodImplemented("Head"))
            {
                methods.Add(WebRequestMethod.Head);
            }

            return methods;
        }

        private bool IsMethodImplemented(string methodName)
        {
            Type type = GetType();
            MethodInfo methodInfo = type.GetMethod(methodName);
            if (methodInfo == null)
            {
                return false;
            }

            if (methodInfo.IsAbstract)
            {
                return false;
            }

            return methodInfo.DeclaringType == type;
        }
    }
}