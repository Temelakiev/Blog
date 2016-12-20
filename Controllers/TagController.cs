using Blog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class TagController : Controller
    {
        // GET: Tag
        public ActionResult Index()
        {
            return View();
        }

        //GET:Tag
        public ActionResult List(int? id)
        {
            if (id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                //Get articles from database 
                //var articles = database.Tags
                //    .Include(t => t.Articles.Select(a => a.Tags))
                //    .Include(t => t.Articles.Select(a => a.Author))
                //    .FirstOrDefault(t => t.Id == id)
                //    .Articles
                //    .ToList();


                var tag = database.Tags
                    .Include(a => a.Articles)
                    .Include("Articles.Author")
                    .Include("Articles.Tags")
                    .FirstOrDefault(a => a.Id == id);

                if (tag == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Tag does not exist");
                }

                //Return the view
                return View(tag);
            }
        }
    }
}