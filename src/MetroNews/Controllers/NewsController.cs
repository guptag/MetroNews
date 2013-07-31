using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NewsExtractor;
using MetroNews.Models;

namespace MetroNews.Controllers
{
    public class NewsController : Controller
    {
        //
        // GET: /News/

        public ActionResult Home()
        {
            NewsModel model = new NewsModel();
            Logger.GetInstance().Flush();
            return View(model);
        }

    }
}
