/*
 * Copyright 2008-2014 Matthew Cosand
 */
namespace System.Web.Mvc
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Web.Mvc.Html;

    public static class HtmlHelperExtensions
    {
        public static string EnumToDropDown(this HtmlHelper helper, Type enumType, string dropdownName, Nullable<int> current, string nullEntry)
        {
            Func<int, bool> test = (enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                 ? (Func<int, bool>)(f => (current | f) == f)
                 : (Func<int, bool>)(f => (current == f));

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<select name=\"{0}\">", dropdownName);
            if (!string.IsNullOrEmpty(nullEntry))
            {
                sb.AppendFormat("<option value=\"\"{1}>{0}</option>", nullEntry, current.HasValue ? "" : " selected=\"selected\"");
            }

            foreach (object v in Enum.GetValues(enumType))
            {
                sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", (int)v, Enum.GetName(enumType, v),
                    test((int)v) ? " selected=\"selected\"" : "");
            }
            sb.AppendLine("</select>");

            return sb.ToString();
        }

        public static bool IsCurrentAction(this HtmlHelper helper, string actionName, string controllerName)
        {
            string currentControllerName = (string)helper.ViewContext.RouteData.Values["controller"];
            string currentActionName = (string)helper.ViewContext.RouteData.Values["action"];

            if (currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase) && currentActionName.Equals(actionName, StringComparison.CurrentCultureIgnoreCase))
                return true;

            return false;
        }

        #region CheckBoxList -- Adapted from http://blog.tylergarlick.com/index.php/2009/03/checkboxlist-for-aspnet-mvc/
        /// <summary>
        /// Builds a CheckBoxList with selected items.
        /// </summary>
        /// <param name="Name">Used in to name the collection of checkboxes</param>
        /// <param name="items">List<ListItems> items you want selected</param>
        /// <returns>Html</returns>
        public static string CheckBoxList(this HtmlHelper helper, string name, IEnumerable<SelectListItem> items)
        {
            return CheckBoxList(helper, name, items, null, null);
        }

        public static string CheckBoxList(this HtmlHelper helper, string name, IEnumerable<SelectListItem> items, string additionalCssClasses, IDictionary<string, object> checkboxHtmlAttributes)
        {
            StringBuilder output = new StringBuilder();

            int i = 0;
            foreach (var item in items)
            {
                output.Append("<div class=\"fields");
                if (!string.IsNullOrEmpty(additionalCssClasses))
                {
                    output.Append(" " + additionalCssClasses);
                }
                output.Append("\" style=\"white-space:nowrap; padding-right:.8em;\">");
                var checkboxList = new TagBuilder("input");
                checkboxList.MergeAttribute("type", "checkbox");
                string id = string.Format("{0}_{1}", name, i);
                i++;
                checkboxList.MergeAttribute("id", id);
                checkboxList.MergeAttribute("name", name);
                checkboxList.MergeAttribute("value", item.Value ?? item.Text);

                // Check to see if it's checked
                if (item.Selected)
                    checkboxList.MergeAttribute("checked", "checked");

                // Add any attributes
                if (checkboxHtmlAttributes != null)
                    checkboxList.MergeAttributes(checkboxHtmlAttributes);

                checkboxList.SetInnerText(item.Text);
                output.Append(checkboxList.ToString(TagRenderMode.SelfClosing));
                output.Append(string.Format("<label for=\"{0}\" style=\"display:inline\">{1}</label></div>", id, item.Text));
            }

            return output.ToString();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static string DateTimePickerFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes) where TModel : class
        {
            if (typeof(TProperty) != typeof(DateTime) && typeof(TProperty) != typeof(DateTime?))
            {
                throw new InvalidOperationException("Property type must be DateTime or DateTime?");
            }


            string inputName = GetInputName<TModel, TProperty>(expression);
            TProperty local = HtmlHelperExtensions.GetValue<TModel, TProperty>(htmlHelper, expression);
            string localString = "";
            if (!string.IsNullOrEmpty(local.ToString()))
            {
                localString = DateTime.Parse(local.ToString()).ToString("yyyy-MM-dd HH:mm");
            }
            return htmlHelper.TextBox(inputName, localString, htmlAttributes) +
                "<script type=\"text/javascript\">dateTimePickers['" + inputName + "'] = true;</script>";
            //"<script type=\"text/javascript\">applyDTP('" + inputName + "',true);</script>";
        }

        private static string GetInputName<TModel, TProperty>(this Expression<Func<TModel, TProperty>> expression)
        {
            return expression.Body.ToString().Substring(expression.Parameters[0].Name.Length + 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static string DatePickerFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes) where TModel : class
        {
            if (typeof(TProperty) != typeof(DateTime) && typeof(TProperty) != typeof(DateTime?))
            {
                throw new InvalidOperationException("Property type must be DateTime or DateTime?");
            }


            string inputName = GetInputName<TModel, TProperty>(expression);
            TProperty local = HtmlHelperExtensions.GetValue<TModel, TProperty>(htmlHelper, expression);
            string localString = "";
            if (!string.IsNullOrEmpty(local.ToString()))
            {
                localString = DateTime.Parse(local.ToString()).ToString("yyyy-MM-dd");
            }
            return htmlHelper.TextBox(inputName, localString, htmlAttributes) +
                  "<script type=\"text/javascript\">dateTimePickers['" + inputName + "'] = false;</script>";
        }

        private static TProperty GetValue<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression) where TModel : class
        {
            TModel model = htmlHelper.ViewData.Model;
            if (model == null)
            {
                return default(TProperty);
            }
            return expression.Compile()(model);
        }



        private static T GetModelStateValue<T>(HtmlHelper helper, string key)
        {
            ModelState state;
            if (helper.ViewData.ModelState.TryGetValue(key, out state))
            {
                return (T)state.Value.ConvertTo(typeof(T), null);
            }

            return (T)helper.ViewData.Eval(key);
        }

        public static string RelativeDateTimeString(this HtmlHelper helper, DateTime? time, DateTime relativeTo)
        {
            if (time == null) return "";
            int relativeDate = (time.Value.Date - relativeTo.Date).Days;
            string val = time.Value.ToString("HHmm");
            if (relativeDate != 0) val = string.Format("{0}+{1}", relativeDate, val);
            return val;
        }
    }
}
