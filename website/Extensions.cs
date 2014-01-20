/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Security;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Xml.Linq;
    using Kcsar.Database.Model;
    using Kcsara.Database.Web.Controllers;
    using Microsoft.Web.Mvc;

    public static class Extensions
    {
        public static string ToXmlAttr(this string str)
        {
            return SecurityElement.Escape(str);
        }

        public static SelectList ToSelectList(this Enum enumeration)
        {
            List<object> list = new List<object>();
            foreach (int value in Enum.GetValues(enumeration.GetType()))
            {
                list.Add(new { Value = value, Name = Enum.GetName(enumeration.GetType(), value) });
            }
            return new SelectList(list, "Value", "Name", enumeration);
        }

        public static IEnumerable ToEnumerable(this Enum enumeration)
        {
            List<object> list = new List<object>();
            foreach (int value in Enum.GetValues(enumeration.GetType()))
            {
                list.Add(new { Value = value, Name = Enum.GetName(enumeration.GetType(), value) });
            }
            return list;
        }

        private static string _ModelData(string imgUrl, IModelObject obj)
        {
            return string.Format("<img src=\"{0}\" title=\"Changed {1}, by {2}\" alt=\"metadata\" style=\"float:right\" />",
                imgUrl, obj.LastChanged, obj.ChangedBy);
        }

        public static string UrlFrom<TController>(this HtmlHelper helper, Expression<Action<TController>> action) where TController: BaseController
        {
            RouteValueDictionary routeValuesFromExpression = GetRouteValuesFromExpression<TController>(action);
            VirtualPathData virtualPath = helper.RouteCollection.GetVirtualPath(helper.ViewContext.RequestContext, routeValuesFromExpression);
            if (virtualPath != null)
            {
                return virtualPath.VirtualPath;
            }
            return null;
        }

        private static RouteValueDictionary GetRouteValuesFromExpression<TController>(Expression<Action<TController>> action) where TController : Controller
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            MethodCallExpression body = action.Body as MethodCallExpression;
            if (body == null)
            {
                throw new ArgumentException("Must be a method call", "action");
            }
            string name = typeof(TController).Name;
            if (!name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Target must be a controller", "action");
            }
            name = name.Substring(0, name.Length - "Controller".Length);
            if (name.Length == 0)
            {
                throw new ArgumentException("Can't find route to controller", "action");
            }
            RouteValueDictionary rvd = new RouteValueDictionary();
            rvd.Add("Controller", name);
            rvd.Add("Action", body.Method.Name);
            AddParameterValuesFromExpressionToDictionary(rvd, body);
            return rvd;
        }

        private static void AddParameterValuesFromExpressionToDictionary(RouteValueDictionary rvd, MethodCallExpression call)
        {
            ParameterInfo[] parameters = call.Method.GetParameters();
            if (parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    Expression expression = call.Arguments[i];
                    object obj2 = null;
                    ConstantExpression expression2 = expression as ConstantExpression;
                    if (expression2 != null)
                    {
                        obj2 = expression2.Value;
                    }
                    else
                    {
                        Expression<Func<object>> expression3 = Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object)), new ParameterExpression[0]);
                        obj2 = expression3.Compile()();
                    }
                    rvd.Add(parameters[i].Name, obj2);
                }
            }
        }

        public static string ModelData(this ViewPage control, IModelObject obj)
        {
            return _ModelData(control.ResolveUrl("~/Content/images/info.png"), obj);
        }

        public static string ModelData(this ViewUserControl control, IModelObject obj)
        {
            return _ModelData(control.ResolveUrl("~/Content/images/info.png"), obj);
        }

        public static string ToEventTime(this DateTime? time, DateTime eventStart)
        {
            if (time.HasValue == false)
            {
                return "";
            }

            return time.Value.ToEventTime(eventStart);
        }

        public static string ToEventTime(this DateTime time, DateTime eventStart)
        {
            int dateOffset = (time.Date - eventStart.Date).Days;
            
            return ((dateOffset == 0) ? "" : (dateOffset.ToString() + '+')) + time.ToString("HHmm");
        }

        public static XElement ElementOrDefault(this XElement element, XName name)
        {
            if (element == null)
                return null;

            return element.Element(name);
        }

        public static XElement ElementOrDefault(this XDocument element, XName name)
        {
            return ((XElement)element.FirstNode).ElementOrDefault(name);
        }

        public static string SafeValue(this XElement element)
        {
            if (element == null)
                return null;

            return element.Value;
        }

        public static MvcHtmlString PopupActionLink<C>(this HtmlHelper helper, Expression<Action<C>> action, string linkText, int height) where C : Controller
        {
            return helper.PopupActionLink<C>(action, linkText, 540, height);
        }

        public static MvcHtmlString PopupActionLink<C>(this HtmlHelper helper, Expression<Action<C>> action, string linkText) where C : Controller
        {
            return helper.PopupActionLink<C>(action, linkText, 540, 400);
        }

        public static MvcHtmlString PopupActionLink<C>(this HtmlHelper helper, Expression<Action<C>> action, string linkText, int width, int height) where C : Controller
        {
            return MvcHtmlString.Create(string.Format(@"<a href=""{0}"" onclick=""window.open(this.href, '{1}', 'width={2},height={3},scrollbars=1'); return false;"" target=""_blank"">{4}</a>",
                helper.BuildUrlFromExpression(action),
                System.Text.RegularExpressions.Regex.Replace(linkText, "[^a-z0-9]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).ToLowerInvariant(),
                width, height, linkText));
        }

        public static MvcHtmlString PopupActionButton<C>(this HtmlHelper helper, Expression<Action<C>> action, string linkText, int width, int height) where C : Controller
        {
            return MvcHtmlString.Create(string.Format(@"<button class=""button"" onclick=""window.open('{0}', '{1}', 'width={2},height={3},scrollbars=1'); return false;"">{4}</button>",
                helper.BuildUrlFromExpression(action),
                System.Text.RegularExpressions.Regex.Replace(linkText, "[^a-z0-9]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).ToLowerInvariant(),
                width, height, linkText));
        }

        static DateTime ZeroTime = new DateTime(1, 1, 1);
        public static int? Years(this TimeSpan? span)
        {
            if (span == null) return null;
            // Because "Zero" time is rooted in year one, we subject 1 here.
            return (Extensions.ZeroTime + span.Value).Year - 1;
        }
    }
}
