using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BDHub.Models;

namespace BDHub.Controllers
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
            int status = 0;
            BDEntities connection = new BDEntities();
            List<Video> ListOfVideos = connection.Videos.ToList();

            //if(connection.CertUsers.Find.)
            newVideo.userID = (int)Session["userID"];
            newVideo.viewsCount = 0;
            

            foreach (Video d in ListOfVideos)
            {
                if (d.filepath == newVideo.filepath)
                {
                    if (d.userID == newVideo.userID)
                    {
                        status = 1;

                    }
                    else
                    {
                        status = 2;

                    }
                    return Json(status, JsonRequestBehavior.AllowGet);
                }
            }

            connection.Videos.Add(newVideo);
            connection.SaveChanges();

            status = 0;
            return Json(status, JsonRequestBehavior.AllowGet);


        }
    }
}