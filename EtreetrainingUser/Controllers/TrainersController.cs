
using EtreetrainingUser.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Zender.Mail;

namespace EtreetrainingUser.Controllers
{
    [RequireHttps]
    [HandleError]
    [ValidateInput(false)]
    [Authorize]
    public class TrainersController : Controller
    {
        [Authorize]
        public ActionResult MyUsers(string searchBy, string search)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();
                //  ViewBag.Teachers = dbModel.Tbl_Doc.ToList();
                //  ViewBag.course = dbModel.Post_Courses.ToList();
                var id = (from a in dbModel.Trainers
                              where a.Email_Id == User.Identity.Name
                              select a.Id).Distinct().FirstOrDefault();
                if (searchBy == "Course")
                {
                    
                    var personlist = (from ep in dbModel.Post_Courses
                                      join e in dbModel.Paid_Courses on ep.Secret_Code equals e.Course_Code
                                      join t in dbModel.Tbl_Cand_Main on e.User_id equals t.Cand_id
                                      where ep.Trainer_Id == id && ep.Title.StartsWith(search) || ep.Title == null
                                      select new MyCategory
                                      {
                                          uid = t.Cand_id,
                                          uemail = t.Cand_EmailId,
                                          ctitle = ep.Title

                                      }).Distinct().ToList();
                    ViewBag.myusers = personlist;
                }
                else
                {


                    var personlist = (from ep in dbModel.Post_Courses
                                      join e in dbModel.Paid_Courses on ep.Secret_Code equals e.Course_Code
                                      join t in dbModel.Tbl_Cand_Main on e.User_id equals t.Cand_id
                                      where ep.Trainer_Id == id
                                      select new MyCategory
                                      {
                                          uid = t.Cand_id,
                                          uemail = t.Cand_EmailId,
                                          ctitle = ep.Title

                                      }).Distinct().ToList();
                    ViewBag.myusers = personlist;
                }
               //ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();


                return View();
            }

        }
        public ActionResult Index()
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                //ViewBag.Teachers = dbModel.Tbl_Doc.ToList();
                //ViewBag.course = dbModel.Post_Courses.ToList();
                //ViewBag.Paidcand = (from a in dbModel.Tbl_Cand_Main
                //                        //where a.Course_Type == "T"
                //                    select a.CandName).Distinct().ToList();
               

                ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive==true).Distinct().ToList();
               

                return View(dbModel.Trainers.Where(j => j.IsActive == true).FirstOrDefault());
            }

        }
        [AllowAnonymous]
        public ActionResult Frgtpass(string email)
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                string message = null;
               var emailck= dbModel.Trainers.Where(i => i.Email_Id == email && i.IsActive == true).Distinct().FirstOrDefault();
                if (emailck != null)
                {
                    var pass = (from a in dbModel.Trainers
                                        where a.Email_Id == email
                                        select a.Password).Distinct().FirstOrDefault();
                    frgtpassverification(email, pass);
                    

                    return RedirectToAction("Trainrs_Login", new RouteValueDictionary(
                        new { controller = "Trainers", action = "Trainrs_Login", message = "1" }));
                }
                else
                {
                    
                    return RedirectToAction("Trainrs_Login", new RouteValueDictionary(
                         new { controller = "Trainers", action = "Trainrs_Login", message = "2" }));
                }
              
            }

        }
        [Authorize]
        [HttpPost]
        public ActionResult Trnr_Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Trainrs_Login", "Trainers");
        }

        [AllowAnonymous]
        public ActionResult Trainrs_Login(string message)
        {
            if(message!=null)
            {
                if (message =="1")
                {
                    ViewBag.success = "Password has been sent to your email. Please check your inbox or spam. Thank you.";
                }else if(message == "2")
                {
                    ViewBag.error = "Email does not exists";
                }else
                {
                    ViewBag.error = "Something went wrong. Please try again";
                }
            }
            
           
            return View();
        }
        public ActionResult abc()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Trainrs_Login(Trainer trnr, string ReturnUrl)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                var userdetails = dbModel.Trainers.Where(x => x.Email_Id == trnr.Email_Id && x.Password == trnr.Password && x.IsActive==true).FirstOrDefault();
                if (userdetails == null)
                {
                    ViewBag.Duplicatemessage = "Invalid credentials, Please Check your details and try again";
                    return View("Trainrs_Login", trnr);
                }
                else
                {
                    int timeout = 60;
                    var ticket = new FormsAuthenticationTicket(trnr.Email_Id, trnr.rememberMe, timeout);
                    string encrypted = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                    cookie.Expires = DateTime.Now.AddMinutes(timeout);
                    cookie.HttpOnly = true;
                    Response.Cookies.Add(cookie);
                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Dashboard", "Trainers");

                    }
                    //  return View("");
                }

            }
        }

        [HttpGet]
        public ActionResult Trnr_reg(string emid)
        {
            Trainer usermodel = new Trainer();
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
            }
            return View(usermodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Trnr_reg([Bind(Exclude = " IsEmailVerified,ActivationCode")]  Trainer userModel)
        {
            try
            {

            string fileName = Path.GetFileNameWithoutExtension(userModel.image.FileName);
            string extension = Path.GetExtension(userModel.image.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            userModel.Photo_Path = "~/Picture/" + fileName;
            fileName = Path.Combine(Server.MapPath("~/Picture/"), fileName);
            userModel.image.SaveAs(fileName);

            bool Status = false;
            string Message = "";
           
                var isExist = IsEmailExist(userModel.Email_Id);
                if (isExist)
                {
                    
                    ViewBag.Message = "Email Already Exist";
                    return View(userModel);
                }
                
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                    if (userModel.Password==userModel.Re_Password)
                    {
                        userModel.IsActive = true;
                        dbModel.Trainers.Add(userModel);

                        dbModel.SaveChanges();
                        Message = " Registered successfully. Login mail has been sent to " + userModel.Email_Id + ". Thank you.";
                        SendVerificationlinkEmailtrainer(userModel.Email_Id, userModel.Id,userModel.Password);
                        Status = true;
                      
                        ViewBag.Message = Message;
                        ViewBag.Status = Status;
                        ViewBag.sccess = "1";
                        ModelState.Clear();
                       
                        return View("Trnr_reg", new Trainer());
                    }
                    else
                    {
                        Message = "Invalid registration, Your password and repassword should be same.";
                        ViewBag.Message = Message;
                        ViewBag.Status = Status;
                       
                        return View(userModel);
                    }
                   
                }
            
            }
            catch(Exception ex)
            {
               
                ViewBag.Message = "Something went wrong. Please try again.";
                ViewBag.Status = false;
               
                
                return View(userModel);
            }
          
        }
        public ViewResult frgtpassverification(string emailId,string pass)
        {
            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);

            var server = Request.Url.Segments;

            message.Subject = "Your EOTA password";

            message.Body = "<br/> Your EOTA password:" +
                
                "<br/>Password: " + pass;

            message.IsBodyHtml = true;

            message.SendMailAsync();

            return View();

        }

        [NonAction]
        public ViewResult SendVerificationlinkEmailtrainer(string emailId, Int32 tid,string pass)
        {
            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);
           
            var server = Request.Url.Segments;
           
            message.Subject = "Your account is successfully created";

            message.Body = "<br/><br/>We are exited to tell you that your EOTA Trainer account is successfully created. <br/> Your login details:" +
                 "<br/><br/>Trainer id: " + tid +
                "<br/><br/>Email id: " + emailId +
                "<br/><br/>Password: " + pass;

            message.IsBodyHtml = true;

            message.SendMailAsync();

            return View();

        }

        [NonAction]
        public bool IsEmailExist(string emailId)
        {
            using (EOTAEntities dc = new EOTAEntities())
            {
                var v = dc.Trainers.Where(x => x.Email_Id == emailId).FirstOrDefault();
                return v != null;
            }
        }

        public ActionResult Dashboard()
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();
                
                return View(dbModel.Trainers.Where(i => i.IsActive == true).FirstOrDefault());
            }
        }
        public ActionResult courses()
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();

                return View(dbModel.Trainers.Where(i => i.IsActive == true).FirstOrDefault());
            }
        }
        [HttpGet]
        public ActionResult CFUploader()
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();
                return View();
            }

        }

        [HttpPost]
        public ActionResult CFUploader(Tbl_Cand_Main cert, Paid_Courses_Certificates cf)
        {
            var isExist = IsEmailExistforCErtificate(cert.Cand_EmailId);
            if (isExist)
            {

                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    if (dbModel.Tbl_Cand_Main.Any(x => x.Cand_EmailId.Contains(cert.Cand_EmailId) && x.Cand_id == cert.Cand_id ))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(cert.Certificate.FileName);

                        string extension = Path.GetExtension(cert.Certificate.FileName);
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        cf.Certificate = fileName;
                        fileName = Path.Combine(Server.MapPath("~/UserCertificate/"), fileName);
                        cert.Certificate.SaveAs(fileName);
                        cf.User_id = cert.Cand_id;
                        cf.User_EmailId = cert.Cand_EmailId;
                        cf.Course_Name = cert.Cand_DocTitle;

                        dbModel.Paid_Courses_Certificates.Add(cf);
                        dbModel.SaveChanges();

                        SendEmailForCertificate(cert.Cand_EmailId, cert.msgbox);

                        ViewBag.Message = "A mail for Certificate is sent to your given email id: " + cert.Cand_EmailId + ". Thank you.";
                        ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();
                    }

                }
            }
            else
            {
                ViewBag.Message = "Email does not Exist";
                return View(cert);
            }

            ModelState.Clear();
           
            return View("CFUploader", new Tbl_Cand_Main());


        }
        [NonAction]
        public ViewResult SendEmailForCertificate(string emailId, string msg)
        {
            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);


            var server = Request.Url.Segments;
            var verifyUrl = "/user/verifyaccount/" + msg;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            message.Subject = "EOTA Test Certificate";

            if (msg == null)
            {
                message.Body = "<br/><br/>Your certificate is activate in your EOTA account." +
               "<br/><br/> Please login and download certificate.'";
            }
            else
            {
                message.Body = "<br/><br/>" + msg;


            }


            message.IsBodyHtml = true;

            message.SendMailAsync();

            return View();

        }

        public ActionResult Discount()
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Discount([Bind(Exclude = " IsEmailVerified,ActivationCode")] Discount_Calculator discount)
        {

            bool Status = false;
            string Message = "";
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();
                var adid = (from a in dbModel.Tbl_ADMN
                            where a.Admin_EmailId == User.Identity.Name
                            select new { a.Admin_id, a.Admin_EmailId }).Distinct().FirstOrDefault();

                var trid = (from a in dbModel.Trainers
                            where a.Email_Id == User.Identity.Name
                            select new { a.Id, a.Email_Id }).Distinct().FirstOrDefault();
                var chkdiscd = (from a in dbModel.Discount_Calculators
                                where a.Discount_Code == discount.Discount_Code
                                select a.Discount_Code).Distinct().FirstOrDefault();
                if (chkdiscd == null)
                {
                    if (adid.Admin_EmailId != null)
                    {
                        discount.Admin_Id = adid.Admin_id;
                        discount.EnableDisable = true;
                        dbModel.Discount_Calculators.Add(discount);
                        dbModel.SaveChanges();
                        if (discount.EmailId == null)
                        {
                            Message = "Successfully posted.";
                        }
                        else
                        {
                            SendVerificationlinkEmail(discount.EmailId, discount.Discount_Code, discount.msgbox);

                            Message = "Message is sent successfully to:" + discount.EmailId;
                        }

                        ViewBag.UploadStatus = "Your Discount Saved Successfully";
                        ViewBag.enabledt = discount.Enable_Date;

                        ViewBag.disabledt = discount.Disable_Date;

                        ViewBag.Status = Status;
                        ModelState.Clear();

                        ViewBag.code = discount.Discount_Code;
                        ViewBag.per = discount.Discount_Price;

                        if (discount.EnableDisable == true)
                        {
                            ViewBag.enablediable = 1;
                        }
                        else
                        {
                            ViewBag.enablediable = 2;
                        }

                        return View("Discount", new Discount_Calculator());

                    }
                    else if (trid.Email_Id != null)
                    {
                        discount.Trainer_Id = trid.Id;
                        discount.EnableDisable = true;
                        dbModel.Discount_Calculators.Add(discount);
                        dbModel.SaveChanges();
                        if (discount.EmailId == null)
                        {
                            Message = "Successfully posted.";
                        }
                        else
                        {
                            SendVerificationlinkEmail(discount.EmailId, discount.Discount_Code);

                            Message = "Message is sent successfully to:" + discount.EmailId;
                        }

                        Status = true;

                        ViewBag.UploadStatus = "Your Discount Saved Successfully";
                        ViewBag.enabledt = discount.Enable_Date;

                        ViewBag.disabledt = discount.Disable_Date;

                        ViewBag.Status = Status;
                        ModelState.Clear();

                        ViewBag.code = discount.Discount_Code;
                        ViewBag.per = discount.Discount_Price;

                        if (discount.EnableDisable == true)
                        {
                            ViewBag.enablediable = 1;
                        }
                        else
                        {
                            ViewBag.enablediable = 2;
                        }

                        return View("Discount", new Discount_Calculator());

                    }
                    else
                    {
                        ViewBag.UploadStatus = "Invalid Entry, Please try again";
                    }
                }
                else
                {
                    ViewBag.UploadStatus = chkdiscd + " as discount code already exists, Please try with another";
                }

                return View();
            }
        }

        [NonAction]
        public ViewResult SendVerificationlinkEmail(string emailId, string reffcd)
        {

            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);
            message.From = from;
            message.To.Add(to);
            var server = Request.Url.Segments;
            var verifyUrl = "/user/verifyaccount/" + reffcd;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            message.Subject = "Your EOTA course Discount";
            message.Body = "<br/><br/> We are Very much exited to let you know that your discount promo code is( " + reffcd + " ) " +
           "<br/><br/> Please activate the code in EOTA discount section.";
            message.IsBodyHtml = true;
            message.SendMailAsync();

            return View();

        }


        [HttpGet]
        public ActionResult ExamTest(string id)
        {
            ViewBag.pgno = 1;
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = dbModel.Trainers.Where(L => L.Email_Id == User.Identity.Name && L.IsActive == true).Distinct().ToList();
                if (id != null)
            {
                
                    
                    var Qno = int.Parse(id.Substring(0, id.LastIndexOf("]") + 0));
                    string Exmcd = id.Substring(id.LastIndexOf("]") + 1);
                    ViewBag.pgno = Qno;
                    var i = (from a in dbModel.Mock_Tests
                             where a.Exam_Code == Exmcd
                             select new { a.Course_Code, a.Exam_Time }).FirstOrDefault();
                    ViewBag.exmcd = Exmcd;
                    ViewBag.crscd = i.Course_Code;
                    ViewBag.exmtm = i.Exam_Time;
                    if (Qno - 1 == 1)
                    {
                        ViewBag.UploadStatus = " question is successfully posted. Thank you.";
                    }
                    else
                    {
                        ViewBag.UploadStatus = " questions are successfully posted. Thank you.";
                    }
                }

            }
            return View();
        }

        [HttpPost]
        public ActionResult ExamTestpgsnd(int id, Mock_Test test)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();
                if (id > 1)
                {
                    try
                    {
                        var i = (from a in dbModel.Mock_Tests
                                 where a.Exam_Code == test.Exam_Code
                                 select new { a.Course_Code, a.Exam_Time }).FirstOrDefault();
                        var userdetails = dbModel.Mock_Tests.Where(x => x.Exam_Code == test.Exam_Code && x.Course_Code == i.Course_Code && x.Ques_No != id).FirstOrDefault();

                        if (userdetails == null)
                        {
                            ViewBag.UploadStatus = "Error";
                            return RedirectToAction("ExamTest");
                        }

                        else
                        {

                            test.Ques_No = id;
                            test.Exam_Posting_Date = DateTime.Now;
                            test.EnableDisable = true;

                            test.Course_Code = i.Course_Code;
                            test.Exam_Time = i.Exam_Time;
                            test.Pass_mark = 1;
                            dbModel.Mock_Tests.Add(test);
                            dbModel.SaveChanges();

                            if (id == 1)
                            {
                                ViewBag.UploadStatus = " question is successfully posted. Thank you.";
                            }
                            else
                            {
                                ViewBag.UploadStatus = " questions are successfully posted. Thank you.";
                            }
                            ViewBag.pgno = id + 1;
                            ViewBag.exmcd = test.Exam_Code;
                            ViewBag.crscd = i.Course_Code;
                            ViewBag.exmtm = i.Exam_Time;
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.UploadStatus = " Something went wrong. Please check your given data.";
                    }
                }
                else
                {

                    var userdetails = dbModel.Mock_Tests.Where(x => x.Exam_Code == test.Exam_Code && x.Course_Code == test.Course_Code).FirstOrDefault();

                    if (userdetails == null)
                    {
                        test.Ques_No = id;
                        test.Exam_Posting_Date = DateTime.Now;
                        test.EnableDisable = true;
                        ModelState.Values.ToList();
                        test.Pass_mark = 1;
                        dbModel.Mock_Tests.Add(test);
                        dbModel.SaveChanges();
                        if (id == 1)
                        {
                            ViewBag.UploadStatus = " question is successfully posted. Thank you.";
                        }
                        else
                        {
                            ViewBag.UploadStatus = " questions are successfully posted. Thank you.";
                        }

                        ViewBag.pgno = id + 1;
                        ViewBag.exmcd = test.Exam_Code;
                        ViewBag.crscd = test.Course_Code;
                        ViewBag.exmtm = test.Exam_Time;
                    }

                    else
                    {
                        ViewBag.UploadStatus = "Error";

                        return RedirectToAction("ExamTest");

                    }
                }
                ModelState.Clear();

                return View("ExamTest", new Mock_Test());
            }
        }

        [HttpPost]
        public ActionResult ExamTest(Mock_Test test)
        {

            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();

                var userdetails = dbModel.Mock_Tests.Where(x => x.Exam_Code == test.Exam_Code && x.Course_Code == test.Course_Code && x.Ques_No == 1).FirstOrDefault();

                if (userdetails == null)
                {
                    test.Ques_No = 1;
                    test.Exam_Posting_Date = DateTime.Now;
                    test.EnableDisable = true;
                    dbModel.Mock_Tests.Add(test);
                    dbModel.SaveChanges();

                    ViewBag.UploadStatus = "Your course uploaded successfully";
                }

                else
                {

                    ViewBag.UploadStatus = "Your course uploaded successfully";
                }

                ModelState.Clear();

                return View("ExamTest", new Mock_Test());
            }
        }
        public ActionResult exmupdate(string id)
        {

            var Qno = int.Parse(id.Substring(0, id.LastIndexOf("]") + 0));
            string Exmcd = id.Substring(id.LastIndexOf("]") + 1);

            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = dbModel.Trainers.Where(i => i.Email_Id == User.Identity.Name && i.IsActive == true).Distinct().ToList();
                var lstno = (from a in dbModel.Mock_Tests
                             where a.Exam_Code == Exmcd
                             select a.Ques_No).Max();
                if (Qno > 1)
                {
                    if (Qno == lstno)
                    {
                        ViewBag.lpgno = Qno;
                        ViewBag.exmcd = Exmcd;
                        ViewBag.pgno = Qno;
                        ViewBag.UploadStatus = "Your last page of posting.";
                    }
                    else
                    {
                        ViewBag.pgno = Qno;
                        ViewBag.exmcd = Exmcd;
                    }
                    return View(dbModel.Mock_Tests.Where(x => x.Exam_Code == Exmcd && x.Ques_No == Qno).FirstOrDefault());
                }
                else if (Qno == 1)
                {
                    if (Qno == lstno)
                    {
                        ViewBag.lpgno = Qno;
                        ViewBag.exmcd = Exmcd;
                        ViewBag.pgno = Qno;
                        ViewBag.UploadStatus = "This is your first page";
                    }
                    else
                    {
                        ViewBag.pgno = Qno;
                        ViewBag.exmcd = Exmcd;
                        ViewBag.UploadStatus = "This is your first page";
                    }
                    return View(dbModel.Mock_Tests.Where(x => x.Exam_Code == Exmcd && x.Ques_No == 1).FirstOrDefault());
                }
                else
                {
                    ViewBag.UploadStatus = "Something went wrong. Please try again!";
                    return View(dbModel.Mock_Tests.Where(x => x.Exam_Code == Exmcd && x.Ques_No == 1).FirstOrDefault());
                }
            }
        }
        [HttpPost]
        public ActionResult exmupdate(string id, Mock_Test test)
        {
            var Qno = int.Parse(id.Substring(0, id.LastIndexOf("]") + 0));
            string Exmcd = id.Substring(id.LastIndexOf("]") + 1);


            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {
                    ViewBag.id = dbModel.Trainers.Where(L => L.Email_Id == User.Identity.Name && L.IsActive == true).Distinct().ToList();
                    var i = (from a in dbModel.Mock_Tests
                             where a.Exam_Code == Exmcd && a.Course_Code == test.Course_Code && a.Ques_No == Qno
                             select a.Id).FirstOrDefault();
                    test.Id = i;
                    test.Exam_Code = Exmcd;
                    test.Pass_mark = 1;
                    dbModel.Entry(test).State = EntityState.Modified;
                    dbModel.SaveChanges();

                }
                return RedirectToAction("exmupdate");
            }
            catch (Exception ex)
            {
                return View();

            }

        }

        [NonAction]
        public ViewResult SendVerificationlinkEmail(string emailId, string Code, string msg)
        {


            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);


            var server = Request.Url.Segments;
            var verifyUrl = "/user/verifyaccount/" + Code;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            message.Subject = "Your EOTA course Discount";
            if (msg == null)
            {
                message.Body = "<br/><br/> We are Very much exited to let you know that your discount promo code is( " + Code + " ) " +
               "<br/><br/> Please activate the code in EOTA discount section.";

            }
            else
            {
                message.Body = "<br/><br/>" + msg +
                "<br/><br/>'" + Code + "</a>";

            }

            message.IsBodyHtml = true;

            message.SendMailAsync();

            return View();

        }
        [NonAction]
        public bool IsEmailExistforCErtificate(string emailId)
        {
            using (EOTAEntities dc = new EOTAEntities())
            {
                var v = dc.Tbl_Cand_Main.Where(x => x.Cand_EmailId == emailId).FirstOrDefault();
                return v != null;
            }
        }

    }

}