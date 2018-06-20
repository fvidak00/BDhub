using System;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Threading.Tasks;
using System.Web.Mvc;
using BDHub.Models;


namespace BDHub.Controllers
{

    public class NavigationBarController : Controller
    {
        private BDEntities db = new BDEntities();
        private BDokenControl BDC = new BDokenControl();

        public IQueryable<Video> VideoSort(string sortOrder, int stat)
        {
            ViewBag.DefaultSortParm = sortOrder == "videoid_desc" ? "VideoID" : "videoid_desc";
            ViewBag.TitleSortParm = sortOrder == "Title" ? "title_desc" : "Title";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.ViewsSortParm = sortOrder == "Views" ? "views_desc" : "Views";
            IQueryable<Video> videos;

            if (stat == 1)
            {
                videos = from v in db.Videos
                         select v;
            }
            else
            {
                int sid = (int)Session["userID"];
                videos = from v in db.Videos
                         where v.userID == sid
                         select v;
            }

            switch (sortOrder)
            {
                case "VideoID":
                    videos = videos.OrderBy(s => s.videoID);
                    break;
                case "videoid_desc":
                    videos = videos.OrderByDescending(s => s.videoID);
                    break;
                case "Title":
                    videos = videos.OrderBy(s => s.title);
                    break;
                case "title_desc":
                    videos = videos.OrderByDescending(s => s.title);
                    break;
                case "Price":
                    videos = videos.OrderBy(s => s.price);
                    break;
                case "price_desc":
                    videos = videos.OrderByDescending(s => s.price);
                    break;
                case "Views":
                    videos = videos.OrderBy(s => s.viewsCount);
                    break;
                case "views_desc":
                    videos = videos.OrderByDescending(s => s.viewsCount);
                    break;
                default:
                    videos = videos.OrderBy(s => s.videoID);
                    break;
            }
            return videos;
        }

        public ActionResult Index(string sortOrder = "", int insufficientFunds = 0)
        {
            try
            {
                int sid = (int)Session["userID"];
                CertUser noBDokenAccount = (from u in db.CertUsers
                                            where u.certUserID == sid
                                            select u).SingleOrDefault();

                if (noBDokenAccount.beternumAddress.Equals(""))
                    return RedirectToAction("Index", "BDokenSettings");

                switch (insufficientFunds)
                {
                    case 1:
                        ViewBag.Message = "Not enough gold, milord.";
                        break;
                    case 2:
                        ViewBag.Message = "Incorrect passphrase.";
                        break;
                    case 0:
                    default:
                        ViewBag.Message = "";
                        break;
                }

                IQueryable<Video> videos = VideoSort(sortOrder, 1);

                return View(videos.ToList());
            }
            catch
            {
                return Redirect("~/Login/Index");
            }
        }

        [HttpPost]
        public ActionResult SendDataToIncrementViewCount(FormCollection collection)
        {
            try
            {
                int sid = (int)Session["userID"];
                int? id = Int32.Parse(collection["hiddenVideoID"]);
                string passphrase = collection["passphrase"];
                return RedirectToAction("IncrementViewCount", new { id, passphrase });
            }
            catch
            {
                return Redirect("~/Login/Index");
            }
        }

        public async Task<ActionResult> IncrementViewCount(int? id, string passphrase)
        {
            try
            {
                int sid = (int)Session["userID"];

                var result = (from v in db.Videos
                              where v.videoID == id
                              select v).SingleOrDefault();

                var payer = (from p in db.CertUsers
                             where p.certUserID == sid
                             select p).SingleOrDefault();

                var receiver = (from r in db.CertUsers
                                where r.certUserID == result.userID
                                select r).SingleOrDefault();

                BigInteger BDWei = 0;
                if (result.price != 0 && result.userID != sid)
                {
                    BDWei = (BigInteger)(result.price * (decimal)Math.Pow(10, 18));

                    if (!(await BDC.CheckRequiredFunds(payer.beternumAddress, "", BDWei)) && result.userID != sid)
                    {
                        return RedirectToAction("Index", new { sortOrder = "", insufficientFunds = 1 });
                    }
                }
                result.viewsCount++;
                if (result.userID != sid)
                {
                    try
                    {
                        await BDC.PayUp(payer.beternumAddress, passphrase, receiver.beternumAddress, BDWei);
                    }
                    catch
                    {
                        return RedirectToAction("Index", new { sortOrder = "", insufficientFunds = 2 });
                    }
                    Payment payment = new Payment
                    {
                        videoTitle = result.title,
                        payerUsername = payer.username,
                        receiverUsername = receiver.username,
                        paymentSum = result.price,
                        paymentDatetime = DateTime.Now
                    };
                    db.Payments.Add(payment);
                }
                db.SaveChanges();
                return RedirectToAction("VideoPlayer", new { id });
            }
            catch (Exception e)
            {
                if (e.Message == "gas required exceeds allowance or always failing transaction")
                    return RedirectToAction("Index", new { sortOrder = "", insufficientFunds = 1 });
                return Redirect("~/Login/Index");
            }

        }

        public ActionResult VideoPlayer(int? id)
        {
            if (id != null)
            {
                var result = (from v in db.Videos
                              where v.videoID == id
                              select v).FirstOrDefault();
                return View(result);
            }
            else
                return Redirect("~/Login/Index");

        }

        public ActionResult MyProfile(int profileUpdated = 0, int passwordUpdate = 0)
        {
            try
            {
                int id = (int)Session["userID"];
                var result = (from c in db.CertUsers
                              where c.certUserID == id
                              select c).SingleOrDefault();

                switch (profileUpdated)
                {
                    case 1:
                        ViewBag.Message = "Profile updated successfully.";
                        break;
                    case 2:
                        ViewBag.Message = "Profile update failed. Please fill all fields";
                        break;
                    case 0:
                    default:
                        ViewBag.Message = "";
                        break;
                }

                switch (passwordUpdate)
                {
                    case 1:
                        ViewBag.Message = "Password changed successfully.";
                        break;
                    default:
                        break;
                }

                return View(result);
            }
            catch
            {
                return Redirect("~/Login/Index");
            }

        }

        public ActionResult MyVideos(string sortOrder)
        {
            try
            {
                IQueryable<Video> videos = VideoSort(sortOrder, 2);
                return View(videos.ToList());
            }
            catch
            {
                return Redirect("~/Login/Index");
            }
        }

        public ActionResult Upload()
        {
            ViewBag.Message = "Upload page";
            return View();
        }

        public ActionResult Transactions()
        {
            if (Session["userID"] != null)
            {
                ViewBag.Message = "Transactions";
                int sid = (int)Session["userID"];

                var actor = (from a in db.CertUsers
                             where a.certUserID == sid
                             select a).SingleOrDefault();

                var transactions = from t in db.Payments
                                   where (t.payerUsername.Equals(actor.username) || t.receiverUsername.Equals(actor.username))
                                   select t;

                return View(transactions);
            }
            else
                return Redirect("~/Login/Index");
        }

        public ActionResult Edit()
        {
            try
            {
                int id = (int)Session["userID"];
                var result = db.CertUsers.Single(m => m.certUserID == id);
                return View(result);
            }
            catch
            {
                return Redirect("~/Login/Index");
            }
        }
        [HttpPost]
        public ActionResult Edit(FormCollection collection)
        {
            try
            {
                int id = (int)Session["userID"];
                var result = db.CertUsers.Single(m => m.certUserID == id);
                try
                {
                    if (TryUpdateModel(result))
                    {
                        if (result.firstName != null &&
                           result.lastName != null &&
                           result.email != null)
                        {
                            db.SaveChanges();
                            return RedirectToAction("MyProfile", new { profileUpdated = 1, passwordUpdate = 0 });
                        }

                    }
                    return RedirectToAction("MyProfile", new { profileUpdated = 2, passwordUpdate = 0 });
                }
                catch
                {
                    return RedirectToAction("MyProfile", new { profileUpdated = 2, passwordUpdate = 0 });
                }
            }
            catch
            {
                return Redirect("~/Login/Index");
            }

        }

        public ActionResult EditVideo(int? id)
        {
            try
            {
                var result = db.Videos.Single(m => m.videoID == id);
                return View(result);
            }
            catch
            {
                return Redirect("~/Login/Index");
            }
        }
        [HttpPost]
        public ActionResult EditVideo(int? id, FormCollection coll)
        {
            try
            {
                var result = db.Videos.Single(m => m.videoID == id);
                if (TryUpdateModel(result))
                {
                    db.SaveChanges();
                    return RedirectToAction("MyVideos");
                }
                return View(result);
            }
            catch
            {
                return View();
            }

        }

        public ActionResult DeleteVideo(int? id)
        {
            var dVideo = (from v in db.Videos
                          where v.videoID == id
                          select v).SingleOrDefault();

            db.Videos.Remove(dVideo);
            db.SaveChanges();

            return RedirectToAction("MyVideos");
        }

        public ActionResult ChangePassword(int passwordFail = 0)
        {

            switch (passwordFail)
            {
                case 2:
                    ViewBag.Message = "Password change failed.";
                    break;
                case 3:
                    ViewBag.Message = "Incorrect old password";
                    break;
                case 0:
                case 1:
                default:
                    ViewBag.Message = "";
                    break;
            }

            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(FormCollection collection, int passwordFail = 0)
        {
            try
            {
                int id = (int)Session["userID"];
                var result = db.CertUsers.Single(m => m.certUserID == id);

                try
                {
                    string oldPass = collection[2];
                    string newPass = collection[3];

                    if (result.password == oldPass)
                    {
                        result.password = newPass;
                        db.SaveChanges();
                        return RedirectToAction("MyProfile", new { profileUpdated = 0, passwordUpdate = 1 });
                    }
                    else
                    {
                        return RedirectToAction("ChangePassword", new { passwordFail = 3 });
                    }
                }
                catch
                {
                    return RedirectToAction("ChangePassword", new { passwordFail = 2 });
                }

            }
            catch
            {
                return Redirect("~/Login/Index");
            }
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(CertUser userModel)
        {
            var userDetails = db.CertUsers.Where(x => x.email == userModel.email).FirstOrDefault();
            if (userDetails != null)
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.To.Add(userModel.email);

                //Triba mail prominit, al radi ovako
                mailMessage.From = new MailAddress("bdots1@outlook.com");

                mailMessage.Subject = "Do not forget your password this time!";
                mailMessage.Body = "Username: " + userDetails.username + "\nPassword:  " + userDetails.password;

                SmtpClient smtpClient = new SmtpClient("smtp.live.com", 587)
                {
                    EnableSsl = true,

                    Credentials = new System.Net.NetworkCredential("bdots1@outlook.com", "Grf55psf")
                };
                smtpClient.Send(mailMessage);

                return RedirectToAction("../Login/Index", new { flag = 1 });
            }
            else
            {
                ViewBag.Message = "E-mail not sent";
                return View("ForgotPassword", userModel);
            }

        }

        public ActionResult DeleteUser()
        {
            try
            {
                int sid = (int)Session["userID"];

                foreach (var userVideo in db.Videos.Where(x => x.userID == sid))
                {
                    db.Videos.Remove(userVideo);
                }

                var dUser = (from u in db.CertUsers
                             where u.certUserID == sid
                             select u).FirstOrDefault();

                db.CertUsers.Remove(dUser);


                db.SaveChanges();
                Session.Abandon();

                return Redirect("~/Login/Index");
            }
            catch
            {
                return Redirect("~/Login/Index");
            }
        }
    }
}
