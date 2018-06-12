using System;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Threading.Tasks;
using System.Web.Mvc;
using BDHub.Models;
using System.Windows.Forms;
using System.Threading;


namespace BDHub.Controllers
{

    public class NavigationBarController : Controller
    {
        private BDEntities db = new BDEntities();
        private BDokenControl BDC = new BDokenControl();

        public ActionResult Index(string sortOrder)
        {
            ViewBag.TitleSortParm = String.IsNullOrEmpty(sortOrder) ? "Title" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.ViewsSortParm = sortOrder == "Views" ? "views_desc" : "Views";

            //if (insufficientFunds == 0)
            //    ViewBag.Message = "";
            //else
            //    ViewBag.Message = "Not enough gold, milord.";

            var videos = from v in db.Videos
                         select v;

            switch (sortOrder)
            {
                case "Title":
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
                    videos = videos.OrderBy(s => s.title);
                    break;
            }
            return View(videos.ToList());

            //return View(videos);
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

                BigInteger BDWei = (BigInteger)(result.price * (decimal)Math.Pow(10, 18));

                //napravit unos passworda za BDoken
                if (!(await BDC.CheckRequiredFunds(payer.beternumAddress, "password", BDWei)) && result.userID != sid)
                {
                    return RedirectToAction("Index", new { insufficientFunds = 1 });
                }
                else
                {
                    result.viewsCount++;
                    if (result.userID != sid)
                    {
                        await BDC.Transfer(payer.beternumAddress, "password", receiver.beternumAddress, BDWei);
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
            }
            catch (Exception e)
            {
                if (e.Message == "gas required exceeds allowance or always failing transaction")
                    return RedirectToAction("Index", new { insufficientFunds = 1 });
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

        public async Task<ActionResult> MyProfile(int profileUpdated = 0)
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
                        result.infoMessage = "Profile updated successfully.";
                        break;
                    case 2:
                        result.infoMessage = "Profile update failed. Please fill all fields";
                        break;
                    case 0:
                    default:
                        result.infoMessage = "";
                        break;
                }

                //Add password input
                try
                {
                    BigInteger userBalance = await BDC.CheckBalance(result.beternumAddress, "password");
                    result.balance = (decimal)userBalance / 1000000000000000000;
                }
                catch
                {
                    result.balance = 0;
                }

                return View(result);
            }
            catch
            {
                return Redirect("~/Login/Index");
            }

        }

        public ActionResult LoadBDokenAccount()
        {
            int id = (int)Session["userID"];

            CertUser addingNewAddress = (from n in db.CertUsers
                                         where n.certUserID == id
                                         select n).SingleOrDefault();

            //Need path and filename
            string path = "";
            string filename = "";

            //Password
            addingNewAddress.beternumAddress = BDC.LoadFromKeystore(path, filename, "password");
            db.SaveChanges();

            return RedirectToAction("MyProfile");
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
        public ActionResult Edit(System.Web.Mvc.FormCollection collection)
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
        public ActionResult EditVideo(int? id, System.Web.Mvc.FormCollection coll)
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
        public ActionResult ChangePassword(System.Web.Mvc.FormCollection collection, int nesto = 0)
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

        public ActionResult CreateBDokenAcc(int fid = 0)
        {
            switch (fid)
            {

                case 1:
                    ViewBag.Message = "BDoken account created successfully";
                    break;
                case 2:
                    ViewBag.Message = "Passwords to not match.";
                    break;
                case 3:
                    ViewBag.Message = "BDoken account creation failed.";
                    break;
                case 0:
                default:
                    ViewBag.Message = "";
                    break;
            }
            return View();
        }
        [HttpPost]
        public ActionResult CreateBDokenAcc(System.Web.Mvc.FormCollection collection)
        {
            try
            {
                int id = (int)Session["userID"];
                CertUser addingNewAddress = (from n in db.CertUsers
                                             where n.certUserID == id
                                             select n).SingleOrDefault();
                string path = "";

                try
                {
                    string password = collection[2];
                    string retyped = collection[3];
                    if (password == retyped)
                    {
                        path = GetDirPath();
                        addingNewAddress.beternumAddress = BDC.CreateNew(path, password);
                        db.SaveChanges();
                        addingNewAddress.infoMessage = "BDoken account created successfully.";
                        return RedirectToAction("CreateBDokenAcc", new { fid = 1 });
                    }
                    else
                    {
                        addingNewAddress.infoMessage = "Passwords to not match.";
                        return RedirectToAction("CreateBDokenAcc", new { fid = 2 });
                    }
                }
                catch
                {
                    addingNewAddress.infoMessage = "BDoken account creation failed.";
                    return RedirectToAction("CreateBDokenAcc", new { fid = 3 });
                }
               
            }
            catch
            {
                return Redirect("~/Login/Index");
            }
        }

        public string GetDirPath()
        {
            string selectedPath = "";
            var t = new Thread((ThreadStart)(() =>
            {

                FolderBrowserDialog fbd = new FolderBrowserDialog
                {
                    ShowNewFolderButton = false,
                    RootFolder = System.Environment.SpecialFolder.MyComputer
                };



                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // the code here will be executed if the user presses Open in
                    // the dialog.
                }
                selectedPath = fbd.SelectedPath;


            }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            return selectedPath;
        }

    }
}
