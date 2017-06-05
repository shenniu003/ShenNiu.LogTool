using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using ShenNiu.LogTool.Extension;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace ShenNiu.LogTool.Controllers
{
    public class LogController : BaseController
    {
        //
        /// <summary>
        /// 磁盘列表
        /// </summary>
        /// <param name="path">磁盘路径</param>
        /// <returns></returns>
        public IActionResult Index(string path)
        {
            Console.WriteLine($"IP:{HttpContext.Connection.RemoteIpAddress}正在查看磁盘：{path}");
            var list = new List<FileSystemInfo>();
            MoSearch moSerach = new MoSearch { Txt1 = path };
            ViewData["Search"] = moSerach;

            if (string.IsNullOrWhiteSpace(path)) { return View(list); }
            if (path.StartsWith("c:", StringComparison.OrdinalIgnoreCase)) { this.MsgBox($"无权限访问：{path}"); return View(list); }
            if (!System.IO.Directory.Exists(path)) { this.MsgBox($"磁盘路径：{path}不存在！"); return View(list); }
            DirectoryInfo dic = new DirectoryInfo(path);
            list = dic.GetFileSystemInfos().OrderByDescending(b => b.LastWriteTime).ToList();

            return View(list);
        }

        /// <summary>
        /// 查看内容 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<IActionResult> Read(string path)
        {
            Console.WriteLine($"IP:{HttpContext.Connection.RemoteIpAddress}正在查看文件：{path}");

            var moFile = new MoFile { Path = path };
            if (string.IsNullOrWhiteSpace(path)) { this.MsgBox($"文件路径：{path}不存在。"); return View(moFile); }
            if (!System.IO.File.Exists(path)) { this.MsgBox($"文件路径：{path}不存在！"); return View(moFile); }

            try
            {
                FileInfo info = new FileInfo(path);
                //if (!ExtensionClass._AllowExtension.Any(b => b.ToUpper() == info.Extension.ToUpper()))
                //{
                //    this.MsgBox($"无法访问{info.Extension}的文件"); return View(moFile);
                // }

                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp");
                DirectoryInfo dic = new DirectoryInfo(basePath);
                var nCount = dic.GetFiles().Count();
                var nMaxCount = 10;
                if (nCount > nMaxCount)  //大于nMaxCount个文件清空临时目录
                {
                    foreach (var item in dic.GetFiles().OrderBy(b => b.LastWriteTime).Take(nCount - nMaxCount))
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch (Exception ex) { }
                    }
                }

                var tempPath = Path.Combine(basePath, info.Name);
                var newInfo = info.CopyTo(tempPath, true);
                if (newInfo == null) { this.MsgBox($"文件：{path}查看失败，请稍后重试！"); return View(moFile); }

                moFile.Name = newInfo.Name;
                moFile.Url = $"/{moFile.Name}";
                moFile.Attributes = newInfo.Attributes;
                if (moFile.Attributes == FileAttributes.Archive && !ExtensionClass._FileExtension.Any(b => b == newInfo.Extension))
                {
                    using (var stream = newInfo.OpenRead())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            moFile.Content = await reader.ReadToEndAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.MsgBox($"文件：{path}查看失败，请稍后重试！");
            }
            return View(moFile);
        }

        public async Task<ContentResult> GetSelData()
        {
            var apiUrl = $"http://{Request.Host.Host}:{Request.Host.Port}/js/tooldata/logconf.json";
            var str = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                str = await client.GetStringAsync(apiUrl);
            }
            return Content(str);
        }

        /// <summary>
        /// 本查看系统具有上传文件的功能    
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> AjaxFileUp()
        {
            var data = new MoData { Msg = "上传失败" };
            try
            {
                var upPath = Request.Form["txt1"];
                if (string.IsNullOrWhiteSpace(upPath)) { data.Msg = "请在【磁盘路径】输入框输入上传路径。"; return Json(data); }
                if (!System.IO.Directory.Exists(upPath)) { data.Msg = $"磁盘路径：{upPath}不存在！"; return Json(data); }
                upPath = upPath.ToString().TrimEnd('\\');

                var files = Request.Form.Files.Where(b => b.Name == "upFile");
                //非空限制
                if (files == null || files.Count() <= 0) { data.Msg = "请选择上传的文件。"; return Json(data); }

                //格式限制
                //var allowType = new string[] { "image/jpeg", "image/png" };
                //if (files.Any(b => !allowType.Contains(b.ContentType)))
                //{
                //    data.Msg = $"只能上传{string.Join(",", allowType)}格式的文件。";
                //    return Json(data);
                //}

                //大小限制
                var nMax = 20;
                if (files.Sum(b => b.Length) >= 1024 * 1024 * nMax)
                {
                    data.Msg = $"上传文件的总大小只能在{nMax}M以下。"; return Json(data);
                }

                //删除过去备份的文件
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tempbak");
                DirectoryInfo dic = new DirectoryInfo(basePath);
                var nCount = dic.GetFiles().Count();
                var nMaxCount = 10;
                if (nCount > nMaxCount)  //大于nMaxCount个文件清空临时目录
                {
                    foreach (var item in dic.GetFiles().OrderBy(b => b.LastWriteTime).Take(nCount - nMaxCount))
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch (Exception ex) { }
                    }
                }

                //写入服务器磁盘
                var upLog = new StringBuilder(string.Empty);
                foreach (var file in files)
                {

                    var fileName = file.FileName;
                    var path = Path.Combine(upPath, fileName);
                    upLog.AppendFormat("文件：{0};", path);

                    //存在文件需要备份
                    if (System.IO.File.Exists(path))
                    {
                        FileInfo info = new FileInfo(path);
                        var tempPath = Path.Combine(basePath, info.Name); //备份目录
                        var newInfo = info.CopyTo(tempPath, true);
                        if (newInfo == null) { upLog.Append($"备份：失败，请稍后重试！"); }
                        else { upLog.Append($"备份：成功！"); }
                    }

                    using (var stream = System.IO.File.Create(path))
                    {
                        await file.CopyToAsync(stream);
                    }
                    upLog.Append($"上传：成功;<br/>");
                }
                data.Msg = upLog.ToString();
                data.Status = 2;
            }
            catch (Exception ex)
            {
                data.Msg += ex.Message;
            }
            Console.WriteLine($"IP:{HttpContext.Connection.RemoteIpAddress}正在上传：{data.Msg}");
            return Json(data);
        }

        public void MsgBox(string msg, string key = "msg")
        {
            this.ViewData[key] = msg;
        }
    }

    /// <summary>
    /// 接口统一类
    /// </summary>
    public class MoData
    {
        public string Msg { get; set; }

        public int Status { get; set; }
    }

    /// <summary>
    /// 搜索类
    /// </summary>
    public class MoSearch
    {
        public string Txt1 { get; set; }

        public string Sel1 { get; set; }
    }

    /// <summary>
    /// 文件
    /// </summary>
    public class MoFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
        public string Content { get; set; }
        public FileAttributes Attributes { get; set; }
    }


}
