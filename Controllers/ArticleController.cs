using Blog.Models;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using System;
using System.IO;

namespace Blog.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //
        // GET: Article/List
        public ActionResult List()
        {
            var database = new BlogDbContext();
            // Get articles from database
            var articles = database.Articles
                .Include(a => a.Author)
                .Include(a => a.Tags)
                .Include(a => a.Comments)
                .ToList();

            return View(articles);
        }

        //
        // GET:Article/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                // Get the article from database
                var article = database.Articles.Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .Include(a => a.Tags)
                    .Include(a => a.Comments)
                    .FirstOrDefault();

                if (article == null)
                {
                    return HttpNotFound();
                }

                return View(article);
            }
        }
        //
        // GET: Article/Create
        [Authorize]
        public ActionResult Create()
        {
            using (var database = new BlogDbContext())
            {
                var model = new ArticleViewModel();
                model.Categories = database.Categories.OrderBy(c => c.Name).ToList();

                return View(model);

            }
        }
        //
        // POST: Article/Create
        [HttpPost]
        [Authorize]
        public ActionResult Create(ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Upload image to server


                var image = model.ImageUpload;
                var imageUrl = string.Empty;

                if (image != null)
                {
                    if (image.ContentLength > 0)
                    {
                        var uploadDir = "~/Content/Images";

                        var imageFileName = GenerateRandomString(20) + Path.GetExtension(image.FileName);

                        var imagePath = Path.Combine(Server.MapPath(uploadDir), imageFileName);

                        uploadDir = uploadDir.Substring(1).Replace('\\', '/');
                        imageUrl = Path.Combine(uploadDir, imageFileName);

                        image.SaveAs(imagePath);

                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Gledai si rabotata");
                    }
                }

                // Insert article in DB
                using (var database = new BlogDbContext())
                {
                    // Get author id
                    var authorId = database.Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First()
                        .Id;

                    var article = new Article(authorId, model.Title, model.Content, model.Category, imageUrl);


                    // Save article in DB
                    this.SetArticleTags(article, model, database);
                    database.Articles.Add(article);
                    database.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        public ActionResult PostComment(int id, string commentContent)
        {
            using (var context = new BlogDbContext())
            {
                var article = context.Articles.Find(id);
                var authorId = context.Users
                        .FirstOrDefault(u => u.UserName == this.User.Identity.Name)
                        .Id;

                var comment = new Comment
                {
                    Content = commentContent,
                    AuthorId = authorId

                };

                article.Comments.Add(comment);

                context.SaveChanges();
                return RedirectToAction("Details", new { id = article.Id });
            }
        }

        //
        // GET: Article/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                // Get article from database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .First();

                if (!IsUserAuthorizeToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                ViewBag.TagsString = string.Join(", ", article.Tags.Select(t => t.Name));

                // Check if article exist
                if (article == null)
                {
                    return HttpNotFound();
                }

                // Pass article to view
                return View(article);
            }
        }
        //
        // POST:Article/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                // Get articles from database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();

                // Check if article exist

                if (article == null)
                {
                    return HttpNotFound();
                }

                //Delete article from database
                database.Articles.Remove(article);
                database.SaveChanges();

                // Redirect to index page
                return RedirectToAction("Index");
            }
        }

        //
        //GET: Artile/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                // Get articles from database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .First();

                if (!IsUserAuthorizeToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                // Check if article exists
                if (article == null)
                {
                    return HttpNotFound();
                }
                //Create the view model
                var model = new ArticleViewModel();
                model.Id = article.Id;
                model.Title = article.Title;
                model.Content = article.Content;
                model.Category = article.CategoryId;
                model.Categories = database.Categories.OrderBy(c => c.Name).ToList();

                model.Tags = string.Join(", ", article.Tags.Select(t => t.Name));
                //Pass the view model to view
                return View(model);
            }
        }
        //
        //POST: Article/Edit
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            //Check if the model state is valid
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    //Get articles from database
                    var article = database.Articles
                        .FirstOrDefault(a => a.Id == model.Id);
                    //Set article properties
                    article.Title = model.Title;
                    article.Content = model.Content;
                    article.CategoryId = model.Category;
                    this.SetArticleTags(article, model, database);
                    //Save article state in the database
                    database.Entry(article).State = EntityState.Modified;
                    database.SaveChanges();
                    //Redirect to the index page
                    return RedirectToAction("Index");
                }
            }
            //if the model state is valid return the same view
            return View(model);
        }

        private void SetArticleTags(Article article, ArticleViewModel model, BlogDbContext db)
        {
            //Split tags

            var tagStrings = new string[0];

            if (!string.IsNullOrWhiteSpace(model.Tags))
            {
                tagStrings = model.Tags
                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.ToLower())
                    .Distinct()
                    .ToArray();
            }

            //Clear current article tags
            article.Tags.Clear();

            //Set new article tags
            foreach (var tagString in tagStrings)
            {
                //Get tag from db by its name
                Tag tag = db.Tags.FirstOrDefault(t => t.Name.Equals(tagString));

                //if the tag is null ,create new tag
                if (tag == null)
                {
                    tag = new Tag() { Name = tagString };
                    db.Tags.Add(tag);
                }

                //Add tag to article tags
                article.Tags.Add(tag);

            }
        }

        private bool IsUserAuthorizeToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_$#@!";
            var random = new Random();

            var randomString = new string(
              Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)])
              .ToArray()
              );

            return randomString;
        }
    }
}