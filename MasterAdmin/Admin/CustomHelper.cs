using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace Admin
{
    public static class CustomHelper
    {
        public static string GetViewImagePath()
        {
            return System.Configuration.ConfigurationManager.AppSettings["ViewImagePath"].ToString();
        }

        public static string IsActive(this HtmlHelper helper, string controller)
        {
            var routeData = helper.ViewContext.RouteData;

            var current_controller = (string)routeData.Values["controller"];

            bool isTabActive = controller == current_controller;

            return (isTabActive ? "active" : "");
        }
    }
}