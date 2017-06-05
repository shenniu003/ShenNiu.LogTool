using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ShenNiu.LogTool.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error(string msg = "迷路了，去首页吧！")
        {

            this.MsgBox(msg);

            return View();
        }

        public void MsgBox(string msg, string key = "msg")
        {
            this.ViewData[key] = msg;
        }
    }
}
