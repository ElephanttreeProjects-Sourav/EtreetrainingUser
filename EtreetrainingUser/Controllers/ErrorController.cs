using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtreetrainingUser.Controllers
{
    public class ErrorController : Controller
    {
       
        public ViewResult PageNotFountErrorpage()
        {
            Response.StatusCode = 404;
            String url=(Request.Url.Query.TrimStart()).ToString();

            ViewBag.url=url.Substring(15, url.Length - 15);

            return View("PageNotFountErrorpage");
        }
        public ViewResult InternalServerErrorpage()
        {
            Response.StatusCode = 500;
            String url = (Request.Url.Query.TrimStart()).ToString();

            ViewBag.url = url.Substring(15, url.Length - 15);

            return View("InternalServerErrorpage");
        }
        public ViewResult UnauthorizedErrorpage()
        {
            Response.StatusCode = 401;
            String url = (Request.Url.Query.TrimStart()).ToString();

            ViewBag.url = url.Substring(15, url.Length - 15);

            return View("UnauthorizedErrorpage");
        }
        public ViewResult DefaultErrorpage()
        {

            String url = (Request.Url.Query.TrimStart()).ToString();

            ViewBag.url = url.Substring(15, url.Length - 15);

            return View("DefaultErrorpage");
        }
    }
}