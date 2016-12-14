using Blog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class ImageController : Controller
    {
        // POST: Image
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            Console.WriteLine("Ivan");

            if (file!=null && file.ContentLength>0)
            {
                try
                {
                    string path = Path.Combine(Server.MapPath("~/images"),
                        Path.GetFileName(file.FileName));
                }
                catch (Exception ex)
                {

                    ViewBag.Message="ERROR:" + ex.Message.ToString();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View();
        }
       
    }
}