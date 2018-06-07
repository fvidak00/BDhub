using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Security;
using System.Web.Security;
using System.Net.Mail;

namespace Bdots1.Controllers
{


    public class RegistrationController : Controller
    {
        public ActionResult Registration()
        {
            ViewBag.Message = "Registration";
            return View();
        }

        [HttpPost]

        public JsonResult SaveUser(CertUser newCertUser)
        {
            int state;


            BDEntities connection = new BDEntities();
            //string poruka = "amen chao";
            List<CertUser> ListOfUsers = connection.CertUsers.ToList();

            foreach (CertUser d in ListOfUsers)
            {
                if (newCertUser.email == d.email)
                {
                    state = 2;
                    return Json(state, JsonRequestBehavior.AllowGet);
                }
                else if (newCertUser.username == d.username)
                {
                    state = 1;
                    return Json(state, JsonRequestBehavior.AllowGet);
                }

            }

            //String password = Membership.GeneratePassword(8, 0);
            String password = Guid.NewGuid().ToString().Substring(0, 8);
            state = MessageSend(newCertUser.email, password,newCertUser.username);
            if (state == 0)
            {
                newCertUser.password = password;
                newCertUser.balance = 0;
                connection.CertUsers.Add(newCertUser);
                connection.SaveChanges();
            }
            else
            {
                state = 3;
                return Json(state, JsonRequestBehavior.AllowGet);
            }

            
            return Json(state, JsonRequestBehavior.AllowGet);
        }

        public int MessageSend(String mail, String password,String username)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.To.Add(mail);
                mailMessage.From = new MailAddress("bdots1@outlook.com");

                mailMessage.Subject = "Welcome to bdots server";
                mailMessage.Body = "Username: "+username+"\nPassword:  " + password;

                SmtpClient smtpClient = new SmtpClient("smtp.live.com", 587)
                {
                    EnableSsl = true,

                    Credentials = new System.Net.NetworkCredential("bdots1@outlook.com", "Grf55psf")
                };
                smtpClient.Send(mailMessage);
                return 0;
                //Response.Write("E-mail sent!");
            }
            catch
            {
                return -1;
            }

        }
    }
}