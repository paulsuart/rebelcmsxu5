using System;
using System.Collections.Generic;
using System.Web.Mvc;
using RebelCms.Cms.Web.Routing;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Mvc
{
    /// <summary>
    /// Used by posted forms to proxy the result to the page in which the current URL matches on
    /// </summary>
    public class RebelCmsPageResult : ActionResult
    {   
        public override void ExecuteResult(ControllerContext context)
        {

            //since we could be returning the current page from a surface controller posted values in which the routing values are changed, we 
            //need to revert these values back to nothing in order for the normal page to render again.
            context.RouteData.DataTokens["area"] = null;
            context.RouteData.DataTokens["Namespaces"] = null;

            //validate that the current page execution is not being handled by the normal RebelCms routing system
            if (!context.RouteData.DataTokens.ContainsKey("RebelCms-route-def"))
            {
                throw new InvalidOperationException("Can only use " + typeof (RebelCmsPageResult).Name + " in the context of an Http POST when using a SurfaceController form");
            }

            var routeDef = (RouteDefinition)context.RouteData.DataTokens["RebelCms-route-def"];
            
            //ensure ModelState is copied across
            routeDef.Controller.ViewData.ModelState.Merge(context.Controller.ViewData.ModelState);
            
            //ensure TempData and ViewData is copied across
            foreach (var d in context.Controller.ViewData)
                routeDef.Controller.ViewData[d.Key] = d.Value;            
            routeDef.Controller.TempData = context.Controller.TempData;
            
            using (DisposableTimer.TraceDuration<RebelCmsPageResult>("Executing RebelCms RouteDefinition controller", "Finished"))
            {
                try
                {
                    ((IController)routeDef.Controller).Execute(context.RequestContext);
                }
                finally
                {
                    if (routeDef.Controller is IDisposable)
                        ((IDisposable)routeDef.Controller).Dispose();
                }
            }
            
        }
    }
}