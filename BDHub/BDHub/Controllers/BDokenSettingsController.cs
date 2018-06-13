using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using BDHub.Models;

namespace BDHub.Controllers
{
    public class BDokenSettingsController : Controller
    {
        private BDEntities db = new BDEntities();
        private BDokenControl BDC = new BDokenControl();

        public ActionResult Index(int mssg = 0)
        {
            switch (mssg)
            {
                case 1:
                    ViewBag.Message = "Account loaded successfully.";
                    break;
                case 2:
                    ViewBag.Message = "Error occured during account save.";
                        break;
                case 3:
                    ViewBag.Message = "Account created successfully.";
                    break;
                case 0:
                default:
                    ViewBag.Message = "";
                    break;
            }
            return View();
        }

        public ActionResult LoadBDokenAccount()
        {
            int id = (int)Session["userID"];

            CertUser addingNewAddress = (from n in db.CertUsers
                                         where n.certUserID == id
                                         select n).SingleOrDefault();

            string filepath = GetFilePath();

            if (filepath == "")
            {
                return RedirectToAction("Index");
            }

            addingNewAddress.beternumAddress = BDC.LoadFromKeystore(filepath);
            try
            {
                 db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Index", new { mssg = 2});
            }

            return RedirectToAction("Index", new { mssg = 1 });
        }

        public ActionResult CreateBDokenAcc(int bdokenAccountFailed = 0)
        {
            switch (bdokenAccountFailed)
            {
                case 2:
                    ViewBag.Message = "Passwords to not match.";
                    break;
                case 3:
                    ViewBag.Message = "BDoken account creation failed.";
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
        public ActionResult CreateBDokenAcc(System.Web.Mvc.FormCollection collection, int bdokenAccountFailed = 0)
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

                        if (path == "")
                            return RedirectToAction("Index");

                        addingNewAddress.beternumAddress = BDC.CreateNew(path, password);
                        try
                        { 
                            db.SaveChanges();
                        }
                        catch
                        {
                            return RedirectToAction("Index", new { mssg = 2 });
                        }
                        return RedirectToAction("Index", new { mssg = 3 });
                    }
                    else
                    {
                        return RedirectToAction("CreateBDokenAcc", new { bdokenAccountFailed = 2 });
                    }
                }
                catch
                {
                    return RedirectToAction("CreateBDokenAcc", new { bdokenAccountFailed = 3 });
                }
            }
            catch
            {
                return Redirect("~/Login/Index");
            }
        }

        public ActionResult GetBDokenData()
        {
            return View();
        }

        public ActionResult BuyBDoken()
        {
            return View();
        }

        public ActionResult SellBDoken()
        {
            return View();
        }

        public ActionResult CheckBDokenBalance()
        {
            return View();
        }

        public string GetDirPath()
        {
            string selectedPath = "";
            var t = new Thread(() =>
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


            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            return selectedPath;

        }

        public string GetFilePath()
        {
            string filePath = "";
            string[] arrAllFiles;
            var t = new Thread(() =>
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "All Files (*.*)|*.*",
                    FilterIndex = 1,
                    Multiselect = false
                };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    filePath = ofd.FileName;
                    arrAllFiles = ofd.FileNames; //used when Multiselect = true           
                }

            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            return filePath;

        }

    }
}