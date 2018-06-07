using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Bdots1;

namespace Bdots1.Controllers
{
    public class NavigationBarController : Controller
    {
        private BDEntities db = new BDEntities();
        private BDokenControl BDC = new BDokenControl();

        public ActionResult Index(int insufficientFunds = 0)
        {
            if (insufficientFunds == 0)
                ViewBag.Message = "";
            else
                ViewBag.Message = "Not enough gold, milord.";
            var videos = from v in db.Videos
                         select v;


            return View(videos);
        }

        public async Task<ActionResult> IncrementViewCount(int? id)
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
                BigInteger bal = await BDC.CheckBalance("0xbb79cc5e10fadfa398b7be548c331e2181499ce3", "password");
                BigInteger bal1 = await BDC.CheckBalance("0x2119ad81730f7da63b56d5d4eecc82d26c226db7", "password");

                if (payer.balance < result.price && result.userID != sid)
                {
                    return RedirectToAction("Index", new { insufficientFunds = 1 });
                }
                else
                {
                    result.viewsCount++;
                    if (result.userID != sid)
                    {
                        Payment payment = new Payment
                        {
                            videoID = result.videoID,
                            payer = payer.certUserID,
                            receiver = receiver.certUserID,
                            paymentSum = result.price,
                            paymentDatetime = DateTime.Now
                        };

                        db.Payments.Add(payment);
                        db.SaveChanges();

                        receiver.balance += result.price;
                        payer.balance -= result.price;

                        await BDC.Transfer("0xbb79cc5e10fadfa398b7be548c331e2181499ce3", "password", "0x2119ad81730f7da63b56d5d4eecc82d26c226db7", 5);
                    }
                    db.SaveChanges();

                    return RedirectToAction("VideoPlayer", new { id });
                }
            }
            catch
            {
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

        public ActionResult MyProfile(int profileUpdated = 0)
        {
            try
            {
                int id = (int)Session["userID"];
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

                var result = (from c in db.CertUsers
                              where c.certUserID == id
                              select c).SingleOrDefault();
                return View(result);
            }
            catch
            {
                return Redirect("~/Login/Index");
            }

        }

        public ActionResult MyVideos()
        {
            try
            {
                int sid = (int)Session["userID"];
                var videos = from v in db.Videos
                             where v.userID == sid
                             select v;


                return View(videos);
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
                var transactions = from t in db.Payments
                                   where (t.Payers.certUserID == sid || t.Receivers.certUserID == sid)
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
                        if(result.firstName != null && 
                           result.lastName != null &&
                           result.email != null)
                        {
                            db.SaveChanges();
                            return RedirectToAction("MyProfile", new { profileUpdated = 1 });
                        }
                        
                    }
                    return RedirectToAction("MyProfile", new { profileUpdated = 2 });
                }
                catch
                {
                    return RedirectToAction("MyProfile", new { profileUpdated = 2 });
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

        public ActionResult ChangePassword(int nesto = 0)
        {

            switch (nesto)
            {

                case 1:
                    ViewBag.Message = "Password changed successfully.";
                    break;
                case 2:
                    ViewBag.Message = "Password change failed.";
                    break;
                case 3:
                    ViewBag.Message = "Incorrect old password";
                    break;
                case 0:
                default:
                    ViewBag.Message = "";
                    break;
            }

            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(FormCollection collection, int nesto = 0)
        {


            try
            {
                int id = (int)Session["userID"];
                var result = db.CertUsers.Single(m => m.certUserID == id);

                try
                {

                    //string[] passwordi = collection[2].Split(',');

                    string oldPass = collection[2];
                    string newPass = collection[3];

                    if (result.password == oldPass)
                    {

                        result.password = newPass;

                        db.SaveChanges();

                        //if (TryUpdateModel(result))
                        //{
                        //    db.SaveChanges();
                        //    return RedirectToAction("ChangePassword", new { profileUpdated = 1 });
                        //}
                        //return RedirectToAction("ChangePassword", new { profileUpdated = 2 });



                        return RedirectToAction("ChangePassword", new { nesto = 1 });

                    }
                    else
                    {

                        return RedirectToAction("ChangePassword", new { nesto = 3 });
                    }
                }
                catch
                {
                    return RedirectToAction("ChangePassword", new { nesto = 2 });
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
                //Response.Write("E-mail sent!");
            }
            else
            {
                ViewBag.Message = "E-mail not sent";
                return View("ForgotPassword", userModel);
            }
        }
    }
}
