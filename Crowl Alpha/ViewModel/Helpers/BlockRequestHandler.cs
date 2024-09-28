using CefSharp.Handler;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crowl_Alpha.ViewModel.Helpers
{
    //ALLOW
    public class DefaultRequestHandler : RequestHandler
    {
        // In the default request handler, you allow all resource requests
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            // Return null to allow all requests (no custom request handling)
            return null;
        }
    }

    //BLOCK
    public class BlockRequestHandler : RequestHandler
    {
        // Override the GetResourceRequestHandler method to return a custom resource handler
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            // Return a custom ResourceRequestHandler that blocks requests
            return new CustomResourceRequestHandler();
        }
    }

    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            // Cancel all new network requests by returning Cancel
            return CefReturnValue.Cancel;
        }
    }

}
