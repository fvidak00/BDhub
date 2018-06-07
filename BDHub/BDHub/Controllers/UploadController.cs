using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bdots1.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]

        public JsonResult SaveVideo(Video newVideo)
        {
            BDEntities connection = new BDEntities();
            newVideo.userID = (int)Session["userID"];
            newVideo.viewsCount = 0;
            connection.Videos.Add(newVideo);
            connection.SaveChanges();

            List<Video> ListOfVideos = connection.Videos.ToList();

            return Json(newVideo, JsonRequestBehavior.AllowGet);


        }
    }
}