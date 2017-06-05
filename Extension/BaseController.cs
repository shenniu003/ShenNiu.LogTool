using ShenNiu.LogTool.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShenNiu.LogTool.Extension
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            //var user = context.HttpContext.Session.Get<MoAuthEmail>("sid");
            //if (user == null) { context.Result = new RedirectToActionResult(nameof(HomeController.Login), "Home", new { ReturnUrl = context.HttpContext.Request.Path }); }
            //else if (user.Status !=2)
            //{
            //    context.Result = new RedirectToActionResult(nameof(HomeController.Login), "Home", new { ReturnUrl = context.HttpContext.Request.Path });
            //}

        }
    }
    public class MoAuthEmail
    {
        public string Email { get; set; }

        /// <summary>
        /// 1：待接收登录邮件  2：登录成功
        /// </summary>
        public int Status { get; set; }
    }
}
