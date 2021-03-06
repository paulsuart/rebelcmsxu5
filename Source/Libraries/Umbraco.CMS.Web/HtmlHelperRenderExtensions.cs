﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Management.Instrumentation;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;
using Microsoft.Web.Mvc;
using Umbraco.Framework.Dynamics;
using System.Linq;
using Umbraco.Cms.Web.System;

namespace Umbraco.Cms.Web
{
    public enum UmbracoRenderItemCaseType
    {
        Upper,
        Lower,
        Title,
        Unchanged
    }

    public enum UmbracoRenderItemEncodingType
    {
        Url,
        Html,
        Unchanged
    }

    public static class HtmlHelperRenderExtensions
    {
        /// <summary>
        /// Used for rendering out the Form for BeginUmbracoForm
        /// </summary>
        internal class UmbracoForm : MvcForm
        {
            public UmbracoForm(
                ViewContext viewContext,
                string surfaceController,
                string surfaceAction,
                string area,
                Guid? surfaceId,
                object additionalRouteVals = null)
                : base(viewContext)
            {
                //need to create a params string as Base64 to put into our hidden field to use during the routes
                string surfaceRouteParams;
                if (surfaceId.HasValue)
                {
                    surfaceRouteParams = string.Format("c={0}&a={1}&i={2}&ar={3}",
                            viewContext.HttpContext.Server.UrlEncode(surfaceController),
                            viewContext.HttpContext.Server.UrlEncode(surfaceAction),
                            viewContext.HttpContext.Server.UrlEncode(surfaceId.Value.ToString("N")),
                            area);
                }
                else
                {
                    surfaceRouteParams = string.Format("c={0}&a={1}&ar={2}",
                        viewContext.HttpContext.Server.UrlEncode(surfaceController),
                        viewContext.HttpContext.Server.UrlEncode(surfaceAction),
                        area);
                }

                var additionalRouteValsAsQuery = additionalRouteVals.ToDictionary<object>().ToQueryString();
                if (!additionalRouteValsAsQuery.IsNullOrWhiteSpace())
                    surfaceRouteParams = "&" + additionalRouteValsAsQuery;

                _base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(surfaceRouteParams));

                _textWriter = viewContext.Writer;
            }


            private bool _disposed;
            private readonly string _base64String;
            private readonly TextWriter _textWriter;

            protected override void Dispose(bool disposing)
            {
                if (this._disposed)
                    return;
                this._disposed = true;

                //write out the hidden surface form routes
                _textWriter.Write("<input name='uformpostroutevals' type='hidden' value='" + _base64String + "' />");

                base.Dispose(disposing);
            }
        }

        public static MvcHtmlString EditorFor<T>(this HtmlHelper htmlHelper, string templateName = "", string htmlFieldName = "", object additionalViewData = null)
            where T : new()
        {
            var model = new T();
            var typedHelper = new HtmlHelper<T>(
                htmlHelper.ViewContext.CopyWithModel(model),
                htmlHelper.ViewDataContainer.CopyWithModel(model));

            return typedHelper.EditorFor(x => model, templateName, htmlFieldName, additionalViewData);
        }

        /// <summary>
        /// A validation summary that lets you pass in a prefix so that the summary only displays for elements 
        /// containing the prefix. This allows you to have more than on validation summary on a page.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="prefix"></param>
        /// <param name="excludePropertyErrors"></param>
        /// <param name="message"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString ValidationSummary(this HtmlHelper htmlHelper,
            string prefix = "",
            bool excludePropertyErrors = false,
            string message = "",
            IDictionary<string, object> htmlAttributes = null)
        {
            if (prefix.IsNullOrWhiteSpace())
            {
                return htmlHelper.ValidationSummary(excludePropertyErrors, message, htmlAttributes);
            }

            //if there's a prefix applied, we need to create a new html helper with a filtered ModelState collection so that it only looks for 
            //specific model state with the prefix.
            var filteredHtmlHelper = new HtmlHelper(htmlHelper.ViewContext, htmlHelper.ViewDataContainer.FilterContainer(prefix));
            return filteredHtmlHelper.ValidationSummary(excludePropertyErrors, message, htmlAttributes);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName)
        {
            return html.BeginUmbracoForm(action, controllerName, null, new Dictionary<string, object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName,
            object additionalRouteVals)
        {
            return html.BeginUmbracoForm(action, controllerName, additionalRouteVals, new Dictionary<string, object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName,
            object additionalRouteVals,
            object htmlAttributes)
        {
            return html.BeginUmbracoForm(action, controllerName, additionalRouteVals, htmlAttributes.ToDictionary<object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline against a locally declared controller
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="controllerName"></param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, string controllerName,
            object additionalRouteVals,
            IDictionary<string, object> htmlAttributes)
        {
            var settings = DependencyResolver.Current.GetService<UmbracoSettings>();
            var area = settings.UmbracoPaths.BackOfficePath;
            var formAction = html.ViewContext.HttpContext.Request.Url.AbsolutePath;

            return html.RenderForm(formAction, FormMethod.Post, htmlAttributes, controllerName, action, area, null);
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceId">The surface controller to route to</param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Guid surfaceId)
        {
            return html.BeginUmbracoForm(action, surfaceId, null, new Dictionary<string, object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceId">The surface controller to route to</param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Guid surfaceId,
            object additionalRouteVals)
        {
            return html.BeginUmbracoForm(action, surfaceId, additionalRouteVals, new Dictionary<string, object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceId">The surface controller to route to</param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Guid surfaceId,
            object additionalRouteVals,
            object htmlAttributes)
        {
            return html.BeginUmbracoForm(action, surfaceId, additionalRouteVals, htmlAttributes.ToDictionary<object>());
        }

        /// <summary>
        /// Helper method to create a new form to execute in the Umbraco request pipeline to a surface controller plugin
        /// </summary>
        /// <param name="html"></param>
        /// <param name="action"></param>
        /// <param name="surfaceId">The surface controller to route to</param>
        /// <param name="additionalRouteVals"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginUmbracoForm(this HtmlHelper html, string action, Guid surfaceId,
            object additionalRouteVals,
            IDictionary<string, object> htmlAttributes)
        {
            var settings = DependencyResolver.Current.GetService<UmbracoSettings>();
            var area = settings.UmbracoPaths.BackOfficePath;
            var formAction = html.ViewContext.HttpContext.Request.Url.AbsolutePath;
            
            var surfaceMetadata = DependencyResolver.Current.GetService<ComponentRegistrations>()
                    .SurfaceControllers
                    .Where(x => x.Metadata.Id == surfaceId)
                    .SingleOrDefault();
            if (surfaceMetadata == null)
                throw new InvalidOperationException("Could not find the surface controller with id " + surfaceId);
            //now, need to figure out what area this surface controller belongs too...
            var pluginDefition = surfaceMetadata.Metadata.PluginDefinition;
            if (pluginDefition.HasRoutablePackageArea())
            {
                //a plugin def CAN be null, if the plugin is actually in our Web DLL folder or if someone drops their 
                //dll into the bin... though if they do  that it still wont work since they wont get an area registered.
                //area = PluginManager.GetPackageFolderFromPluginDll(pluginDefition.OriginalAssemblyFile).Name;
                area = pluginDefition.PackageName;
            }

            //render the form
            return html.RenderForm(formAction, FormMethod.Post, htmlAttributes, surfaceMetadata.Metadata.ControllerName, action, area, surfaceId);
        }

        /// <summary>
        /// This renders out the form for us
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="formAction"></param>
        /// <param name="method"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="surfaceController"></param>
        /// <param name="surfaceAction"></param>
        /// <param name="area"></param>
        /// <param name="surfaceId"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        /// <remarks>
        /// This code is pretty much the same as the underlying MVC code that writes out the form
        /// </remarks>
        private static MvcForm RenderForm(this HtmlHelper htmlHelper,
            string formAction,
            FormMethod method,
            IDictionary<string, object> htmlAttributes,
            string surfaceController,
            string surfaceAction,
            string area,
            Guid? surfaceId,
            object additionalRouteVals = null)
        {

            var tagBuilder = new TagBuilder("form");
            tagBuilder.MergeAttributes(htmlAttributes);
            // action is implicitly generated, so htmlAttributes take precedence.
            tagBuilder.MergeAttribute("action", formAction);
            // method is an explicit parameter, so it takes precedence over the htmlAttributes. 
            tagBuilder.MergeAttribute("method", HtmlHelper.GetFormMethodString(method), true);
            var traditionalJavascriptEnabled = htmlHelper.ViewContext.ClientValidationEnabled && !htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled;
            if (traditionalJavascriptEnabled)
            {
                // forms must have an ID for client validation
                tagBuilder.GenerateId("form" + Guid.NewGuid().ToString("N"));
            }
            htmlHelper.ViewContext.Writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));

            //new UmbracoForm:
            var theForm = new UmbracoForm(htmlHelper.ViewContext, surfaceController, surfaceAction, area, surfaceId, additionalRouteVals);

            if (traditionalJavascriptEnabled)
            {
                htmlHelper.ViewContext.FormContext.FormId = tagBuilder.Attributes["id"];
            }
            return theForm;
        }


    }
}