using EtreetrainingUser.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Zender.Mail;

namespace EtreetrainingUser.Controllers
{
    [HandleError]
    [RequireHttps]
    [ValidateInput(false)]
    [Authorize]
    public class ADMINController : Controller
    {
        public ActionResult DummyVideos()
        {
            using (EOTAEntities eota = new EOTAEntities())
            {
                ViewBag.admnm = eota.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                int count = eota.Tbl_Video.Distinct().Count();
                if(count==3 || count>3)
                {
                    ViewBag.error1 = "1";
                }
                return View(eota.Tbl_Video.Distinct().ToList());
            }
        }
        [HttpPost]
        public ActionResult DummyVideos(Tbl_Video vid,HttpPostedFileBase file)
        {
            using (EOTAEntities eota = new EOTAEntities())
            {
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                string extension = Path.GetExtension(file.FileName);
                fileName = fileName + Guid.NewGuid() + extension;
                vid.Video_Path = "~/UploadedVideos/" + fileName;
                vid.Video_Title = file.FileName;
                fileName = Path.Combine(Server.MapPath("~/UploadedVideos/"), fileName);
                vid.IsActive = true;
                vid.VidFile = file;
                file.SaveAs(fileName);
                eota.Tbl_Video.Add(vid);
                eota.SaveChanges();
                return RedirectToAction("DummyVideos");
            }
            
        }

        public ActionResult DeleteVideo(int id)
        {
            using (EOTAEntities eota = new EOTAEntities())
            {
                var vid = eota.Tbl_Video.Where(a => a.Video_id == id).FirstOrDefault();
                eota.Tbl_Video.Remove(vid);
                eota.SaveChanges();
            }
            return RedirectToAction("DummyVideos");
        }
        public ActionResult star(string star)
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Forgotpass(string myModalfgps1)
        {
            try
            {
                using (EOTAEntities dbmodel = new EOTAEntities())
                {
                    var checkdata = dbmodel.Tbl_ADMN.Where(z => z.Admin_EmailId == myModalfgps1).FirstOrDefault();
                    string adpass = null;
                    string adname = null;
                    if (checkdata != null)
                    {
                        adpass = (from a in dbmodel.Tbl_ADMN
                                  where a.Admin_EmailId == myModalfgps1
                                  select a.Admin_Pass).FirstOrDefault();
                        adname = (from a in dbmodel.Tbl_ADMN
                                  where a.Admin_EmailId == myModalfgps1
                                  select a.AdminName).FirstOrDefault();
                        SendAdminverification(myModalfgps1, adpass, adname);
                        return RedirectToAction("Adm_Login", new RouteValueDictionary(
                        new { controller = "ADMIN", action = "errorpg", Id = "2" }));
                    }
                    else
                    {
                        return RedirectToAction("Adm_Login", new RouteValueDictionary(
                        new { controller = "ADMIN", action = "errorpg", Id = "3" }));
                    }

                }


            }
            catch (Exception ex)
            {
                return RedirectToAction("Adm_Login", new RouteValueDictionary(
                       new { controller = "ADMIN", action = "errorpg", Id = "4" }));
            }
        }

        public ActionResult Report()
        {
            using (EOTAEntities dbmodel = new EOTAEntities())
            {
                ViewBag.admnm = dbmodel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                ViewBag.inst = (from a in dbmodel.Institute_Infoes
                                select a).Distinct().ToList();

                ViewBag.crs = (from ab in dbmodel.Post_Courses

                               select ab).Distinct().ToList();
                return View(dbmodel.Institute_Paymnts.ToList());

            }
        }

        public ActionResult ReferenceDiscount()
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
            }
            return View();
        }

        [HttpPost]
        public ActionResult ReferenceDiscount([Bind(Exclude = " IsEmailVerified,ActivationCode")] Reference_Calculator discount)
        {
            bool Status = false;
            string Message = "";
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                var rfrl = (from a in dbModel.Tbl_Cand_Main
                            where a.Cand_EmailId == discount.EmailId
                            select a.cand_ReferenceCode).Distinct().FirstOrDefault();
                var chkemil = (from a in dbModel.Reference_Calculators
                               where a.EmailId == discount.EmailId
                               select a.EmailId).Distinct().FirstOrDefault();
                if (chkemil != null)
                {
                    if (rfrl != null)
                    {
                        var adid = (from a in dbModel.Tbl_ADMN
                                    where a.Admin_EmailId == User.Identity.Name
                                    select new { a.Admin_id, a.Admin_EmailId }).Distinct().FirstOrDefault();
                        var trid = (from a in dbModel.Trainers
                                    where a.Email_Id == User.Identity.Name
                                    select new { a.Id, a.Email_Id }).Distinct().FirstOrDefault();
                        if (adid.Admin_EmailId != null)
                        {

                            discount.Admin_Id = adid.Admin_id;
                            discount.Reference_Code = rfrl.ToString();
                            discount.EnableDisable = true;
                            dbModel.Reference_Calculators.Add(discount);
                            dbModel.SaveChanges();
                            if (discount.EmailId == null)
                            {

                                Message = "Successfully posted.";
                            }
                            else
                            {
                                SendVerificationlinkEmaildis(discount.EmailId, discount.Reference_Code, discount.msgbox);

                                Message = "Message is sent successfully to:" + discount.EmailId;
                            }

                            ViewBag.UploadStatus = "Your discount saved successfully";

                            ModelState.Clear();

                            ViewBag.code = discount.Reference_Code;
                            ViewBag.per = discount.Referral_Discount_Price;

                            if (discount.EnableDisable == true)
                            {
                                ViewBag.enablediable = 1;
                            }
                            else
                            {
                                ViewBag.enablediable = 2;
                            }

                            return View(new Reference_Calculator());

                        }
                        else if (trid != null)
                        {
                            discount.Trainer_Id = trid.Id;
                            discount.Reference_Code = rfrl.ToString();
                            discount.EnableDisable = true;
                            dbModel.Reference_Calculators.Add(discount);
                            dbModel.SaveChanges();
                            if (discount.EmailId == null)
                            {

                                Message = "Successfully posted.";
                            }
                            else
                            {
                                SendVerificationlinkEmaildis(discount.EmailId, discount.Reference_Code);

                                Message = "Message is sent successfully to:" + discount.EmailId;
                            }

                            Status = true;

                            ViewBag.UploadStatus = "Your discount saved successfully";

                            ModelState.Clear();

                            ViewBag.code = discount.Reference_Code;
                            ViewBag.per = discount.Referral_Discount_Price;

                            if (discount.EnableDisable == true)
                            {
                                ViewBag.enablediable = 1;
                            }
                            else
                            {
                                ViewBag.enablediable = 2;
                            }

                            return View(new Reference_Calculator());

                        }
                        else
                        {
                            ViewBag.UploadStatus = "Invalid Entry, Please try again";
                        }

                    }
                    else
                    {
                        ViewBag.UploadStatus = "Email id does not exists, Please try again";
                    }
                }
                else
                {
                    ViewBag.UploadStatus = "Email id already have exists, Please try with another";
                }

                return View();
            }
        }


        [HttpGet]
        public ActionResult Institution()
        {

            using (EOTAEntities dbmodel = new EOTAEntities())
            {
                ViewBag.admnm = dbmodel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
            }
            return View();
        }
        [HttpPost]
        public ActionResult Institution(Institute_Info ifo)
        {
            try
            {
                using (EOTAEntities dbmodel = new EOTAEntities())
                {
                    ViewBag.admnm = dbmodel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                    if (ifo.option == true)
                    {
                        if (ifo.Promocode != null && ifo.Discount_Percent != null)
                        {
                            var dbcheck = dbmodel.Institute_Infoes.Where(x => x.Institute_Id == ifo.Institute_Id || x.Promocode == ifo.Promocode).FirstOrDefault();
                            if (dbcheck == null)
                            {
                                ifo.IsHOD = true;
                                ifo.IsActive = true;

                                dbmodel.Institute_Infoes.Add(ifo);
                                dbmodel.SaveChanges();
                                ViewBag.message = ifo.Institute_Name_ + " has successfully added. Thank You.";
                                ModelState.Clear();
                                return View(new Institute_Info());
                            }
                            else
                            {
                                ViewBag.message = "Error! Institute ID or discount code exists. Please try again with another code. Thank You.";

                                return View(ifo);
                            }
                        }
                        else
                        {
                            ViewBag.message = "Discount code or discount percentage can not be null.";


                            return View(ifo);
                        }
                    }
                    else
                    {
                        var dbcheck = dbmodel.Institute_Infoes.Where(x => x.Institute_Id == ifo.Institute_Id || x.Promocode == ifo.Promocode).FirstOrDefault();
                        if (dbcheck == null)
                        {
                            dbmodel.Institute_Infoes.Add(ifo);
                            dbmodel.SaveChanges();
                            ViewBag.message = ifo.Institute_Name_ + " has successfully added. Thank You.";
                            ModelState.Clear();
                            return View(new Institute_Info());
                        }
                        else
                        {
                            ViewBag.message = "Error! Institute ID or discount code exists. Please try again with another code. Thank You.";
                            return View(ifo);
                        }
                    }


                }

            }

            catch (Exception ex)
            {
                ViewBag.message = "Error! Check out your entry and try again. Thank You.";
                return View(ifo);
            }


        }
        [AllowAnonymous]
        public ActionResult Adm_Login(string id)
        {
            if (id == "1")
            {
                ViewBag.successmessage = "Successfully Registered. Thank you.";
            }
            if (id == "2")
            {
                ViewBag.successmessage1 = "Password has been sent to your email-ID. Please check. Thank you";
            }
            if (id == "3")
            {
                ViewBag.error = "Sorry! Email-ID does not exists. Please try again.";
            }
            if (id == "4")
            {
                ViewBag.error = "Something went wrong! Plaese try again.";
            }

            return View();
        }

        public ActionResult ii()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ExamTest(string id, string error)
        {
            ViewBag.UploadStatus = error;
            ViewBag.pgno = 1;

            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
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
        public ActionResult ExamTestpgsnd(int id, Mock_Test test, string searchBy)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                if (id > 1)
                {
                    try
                    {
                        var i = (from a in dbModel.Mock_Tests
                                 where a.Exam_Code == test.Exam_Code
                                 select new { a.Course_Code, a.Exam_Time, a.IsMocktest, a.Pass_mark }).FirstOrDefault();
                        var userdetails = dbModel.Mock_Tests.Where(x => x.Exam_Code == test.Exam_Code && x.Course_Code == i.Course_Code && x.Ques_No != id).FirstOrDefault();

                        if (userdetails == null)
                        {
                            ViewBag.UploadStatus = "Error";
                            return RedirectToAction("ExamTest", new RouteValueDictionary(
                    new { controller = "ADMIN", action = "ExamTest", error = ViewBag.UploadStatus }));
                        }

                        else
                        {
                            test.Pass_mark = i.Pass_mark;
                            test.IsMocktest = i.IsMocktest;
                            test.Ques_No = id;
                            test.Exam_Posting_Date = DateTime.Now;
                            test.EnableDisable = true;

                            test.Course_Code = i.Course_Code;
                            test.Exam_Time = i.Exam_Time;

                            dbModel.Mock_Tests.Add(test);
                            dbModel.SaveChanges();
                            ModelState.Clear();
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
                            return RedirectToAction("ExamTest", new RouteValueDictionary(
                     new { controller = "ADMIN", action = "ExamTest", id = ViewBag.pgno + "]" + ViewBag.exmcd, error = ViewBag.UploadStatus }));
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.UploadStatus = " Something went wrong. Please check your given data.";
                        return RedirectToAction("ExamTest", new RouteValueDictionary(
                     new { controller = "ADMIN", action = "ExamTest", error = "Something went wrong. Please check your given data." }));

                    }
                }
                else
                {
                    if (searchBy == "Course")
                    {
                        if (test.Pass_mark != null)
                        {
                            var chkpssmrk = dbModel.Mock_Tests.Where(x => x.Course_Code == test.Course_Code && x.IsMocktest == false).FirstOrDefault();
                            if (chkpssmrk == null)
                            {
                                var userdetails = dbModel.Mock_Tests.Where(x => x.Exam_Code == test.Exam_Code && x.Course_Code == test.Course_Code).FirstOrDefault();

                                if (userdetails == null)
                                {
                                    test.Ques_No = id;
                                    test.Exam_Posting_Date = DateTime.Now;
                                    test.EnableDisable = true;
                                    test.IsMocktest = false;
                                    dbModel.Mock_Tests.Add(test);
                                    dbModel.SaveChanges();
                                    ModelState.Clear();
                                    if (id == 1)
                                    {
                                        ViewBag.UploadStatus = " Question is successfully posted. Thank you.";
                                    }
                                    else
                                    {
                                        ViewBag.UploadStatus = " Exam Code or Course code are already exists.";
                                    }

                                    ViewBag.pgno = id + 1;
                                    ViewBag.exmcd = test.Exam_Code;
                                    ViewBag.crscd = test.Course_Code;
                                    ViewBag.exmtm = test.Exam_Time;
                                    return RedirectToAction("ExamTest", new RouteValueDictionary(
                     new { controller = "ADMIN", action = "ExamTest", id = ViewBag.pgno + "]" + ViewBag.exmcd, error = ViewBag.UploadStatus }));

                                }

                                else
                                {
                                    ViewBag.UploadStatus = "Error";

                                    return RedirectToAction("ExamTest", new RouteValueDictionary(
                     new { controller = "ADMIN", action = "ExamTest", error = ViewBag.UploadStatus }));

                                }
                            }
                            else
                            {
                                ViewBag.UploadStatus = "Exam Code or Course code are already exists.";
                                return RedirectToAction("ExamTest", new RouteValueDictionary(
                       new { controller = "ADMIN", action = "ExamTest", error = "Exam Code or Course code are already exists." }));


                            }

                        }
                        else
                        {
                            ViewBag.UploadStatus = "Pass mark cannot be blank for test as course.";

                            return RedirectToAction("ExamTest", new RouteValueDictionary(
                       new { controller = "ADMIN", action = "ExamTest", error = "Pass mark cannot be blank for test as course." }));


                        }

                    }
                    else
                    {
                        var chkpssmrk = dbModel.Mock_Tests.Where(x => x.Course_Code == test.Course_Code && x.IsMocktest == false).FirstOrDefault();
                        if (chkpssmrk == null)
                        {
                            var userdetails = dbModel.Mock_Tests.Where(x => x.Exam_Code == test.Exam_Code && x.Course_Code == test.Course_Code).FirstOrDefault();

                            if (userdetails == null)
                            {
                                test.Ques_No = id;
                                test.Exam_Posting_Date = DateTime.Now;
                                test.EnableDisable = true;
                                test.IsMocktest = true;
                                dbModel.Mock_Tests.Add(test);
                                dbModel.SaveChanges();
                                ModelState.Clear();
                                if (id == 1)
                                {
                                    ViewBag.UploadStatus = " Question is successfully posted. Thank you.";
                                }
                                else
                                {
                                    ViewBag.UploadStatus = " Exam Code or Course code are already exists.";
                                }

                                ViewBag.pgno = id + 1;
                                ViewBag.exmcd = test.Exam_Code;
                                ViewBag.crscd = test.Course_Code;
                                ViewBag.exmtm = test.Exam_Time;
                                return RedirectToAction("ExamTest", new RouteValueDictionary(
                     new { controller = "ADMIN", action = "ExamTest", id = ViewBag.pgno + "]" + ViewBag.exmcd, error = ViewBag.UploadStatus }));

                            }

                            else
                            {
                                ViewBag.UploadStatus = "Error";

                                return RedirectToAction("ExamTest", new RouteValueDictionary(
                      new { controller = "ADMIN", action = "ExamTest", error = "Sorry, Something error occurred." }));


                            }

                        }
                        else
                        {
                            ViewBag.UploadStatus = "Sorry, you cannot set exam in this course code.";

                            return RedirectToAction("ExamTest", new RouteValueDictionary(
                      new { controller = "ADMIN", action = "ExamTest", error = "Sorry, you cannot set exam in this course code." }));

                        }

                    }
                }


                //return View("ExamTest", new Mock_Test());
            }
        }

        [HttpPost]
        public ActionResult ExamTest(Mock_Test test)
        {

            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
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
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
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
                    ViewBag.admnm = dbModel.Tbl_ADMN.Where(l => l.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                    var i = (from a in dbModel.Mock_Tests
                             where a.Exam_Code == Exmcd && a.Course_Code == test.Course_Code && a.Ques_No == Qno
                             select new { a.Id, a.IsMocktest, a.Pass_mark }).FirstOrDefault();
                    test.Id = i.Id;
                    test.Pass_mark = i.Pass_mark;
                    test.IsMocktest = i.IsMocktest;
                    test.Exam_Code = Exmcd;
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

        public ActionResult Discount()
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
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
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
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
                            SendVerificationlinkEmaildis(discount.EmailId, discount.Discount_Code, discount.msgbox);

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
                            SendVerificationlinkEmaildis(discount.EmailId, discount.Discount_Code);

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

        public ActionResult AftrDiscount(string id, Discount_Calculator discount)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                var checkingdis = dbModel.Discount_Calculators.Where(x => x.Discount_Code == id);
                if (checkingdis != null)
                {
                    var i = (from a in dbModel.Discount_Calculators
                             where a.Discount_Code == id
                             select new { a.Id, a.Discount_Code, a.Admin_Id, a.Trainer_Id, a.Enable_Date, a.Disable_Date, a.Discount_Price }).FirstOrDefault();
                    discount.Id = i.Id;
                    discount.Discount_Code = i.Discount_Code;
                    discount.Admin_Id = i.Admin_Id;
                    discount.Trainer_Id = i.Trainer_Id;
                    discount.Enable_Date = i.Enable_Date;
                    discount.Disable_Date = i.Disable_Date;
                    discount.Discount_Price = i.Discount_Price;

                    discount.EnableDisable = false;
                    dbModel.Entry(discount).State = EntityState.Modified;
                    dbModel.SaveChanges();
                }

                ModelState.Clear();

                ViewBag.disabledt = discount.Disable_Date;
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
        }

        public ActionResult Useracc()
        {
            using (EOTAEntities db = new EOTAEntities())
            {
                return View(db.Users.Where(z=>z.EmailId=="SNL_users@gmail.com").Distinct().FirstOrDefault());
            }
                
        }
        [HttpPost]
        public ActionResult Useracc(string email,User user)
        {
            using (EOTAEntities db = new EOTAEntities())
            {
                
                User usr = db.Users.Where(z => z.EmailId == "SNL_users@gmail.com").Distinct().FirstOrDefault();
                Tbl_Cand_Main cand_Main = db.Tbl_Cand_Main.Where(z => z.Cand_EmailId == "SNL_users@gmail.com").Distinct().FirstOrDefault();
                if (usr == null)
                {
                    db.Users.Add(user);

                    Tbl_Cand_Main tbl_Cand_Main = new Tbl_Cand_Main();

                    tbl_Cand_Main.CandName = user.Name;
                    tbl_Cand_Main.Cand_EmailId = user.EmailId;
                    tbl_Cand_Main.Cand_Pass = user.Password;
                    tbl_Cand_Main.Cand_RePass = user.Password;
                    tbl_Cand_Main.Cand_MNum = "1111111111";
                    tbl_Cand_Main.Cand_IsEmailVerified = true;

                    db.Tbl_Cand_Main.Add(tbl_Cand_Main);

                }
                else
                {
                    usr.Name = user.Name;
                    usr.EmailId = user.EmailId;
                    usr.Password = user.Password;

                    cand_Main.CandName = user.Name;
                    cand_Main.Cand_EmailId = user.EmailId;
                    cand_Main.Cand_Pass = user.Password;
                    cand_Main.Cand_RePass = user.Password;
                    cand_Main.Cand_MNum = "1111111111";
                    cand_Main.Cand_IsEmailVerified = true;
                    
                }
                db.SaveChanges();
                return View(db.Users.Where(z => z.EmailId == "SNL_users@gmail.com").Distinct().FirstOrDefault());
            }

        }

        [HttpGet]
        public ActionResult SelectedAddBlocker(int id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                ViewBag.Techcourse = (from a in dbModel.Post_Courses
                                      where a.Course_Type == "T"
                                      select a.Title).Distinct().ToList();

                ViewBag.Acacourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "A"
                                     select a.Title).Distinct().ToList();

                ViewBag.Mngcourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "M"
                                     select a.Title).Distinct().ToList();

                ViewBag.id = (from a in dbModel.Tbl_ADMN
                              where a.Admin_EmailId == User.Identity.Name
                              select a.Admin_id).Distinct().ToList();

                ViewBag.ttl = (from a in dbModel.Post_Courses

                               select new { a.Title, a.Id, a.Course_Type }).Distinct().ToList();
                var scrt = (from a in dbModel.Post_Courses

                            select a.Secret_Code).Distinct().ToList();

                ViewBag.slctditm = id;
                return View(dbModel.Post_Courses.Distinct().ToList());
            }

        }

        [HttpGet]
        public ActionResult AddBlocker( string searchBy, string searchBycrs)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                ViewBag.Techcourse = (from a in dbModel.Post_Courses
                                      where a.Course_Type == "T"
                                      select a.Title).Distinct().ToList();

                ViewBag.Acacourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "A"
                                     select a.Title).Distinct().ToList();

                ViewBag.Mngcourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "M"
                                     select a.Title).Distinct().ToList();

                ViewBag.id = (from a in dbModel.Tbl_ADMN
                              where a.Admin_EmailId == User.Identity.Name
                              select a.Admin_id).Distinct().ToList();

                ViewBag.ttl = (from a in dbModel.Post_Courses

                               select new { a.Title, a.Id, a.Course_Type }).Distinct().ToList();
                var scrt = (from a in dbModel.Post_Courses

                            select a.Secret_Code).Distinct().ToList();
                
             
                if(searchBycrs=="True")
                {
                    ViewBag.allcrs = "AC";
                    return View(dbModel.Post_Courses.Distinct().ToList());
                }else if (searchBycrs == "False")
                {
                    if (searchBy == "True")
                    {
                        ViewBag.allcrs = null;
                        ViewBag.EC = "EC";
                        return View(dbModel.Post_Courses.Where(a => a.EnableDisable == true).Distinct().ToList());

                    }
                    else if (searchBy == "False")
                    {
                        ViewBag.allcrs = null;
                        return View(dbModel.Post_Courses.Where(a => a.EnableDisable == false || a.EnableDisable == null).Distinct().ToList());
                    }
                    else
                    {
                        ViewBag.allcrs = null;
                        return View(dbModel.Post_Courses.Where(a => a.EnableDisable == false || a.EnableDisable == null).Distinct().ToList());
                    }
                }
                else
                    if (searchBy == "True")
                {
                    ViewBag.allcrs = null;
                    ViewBag.EC = "EC";
                    return View(dbModel.Post_Courses.Where(a => a.EnableDisable == true).Distinct().ToList());

                }
                else if (searchBy == "False")
                {
                    ViewBag.allcrs = null;
                    return View(dbModel.Post_Courses.Where(a => a.EnableDisable == false || a.EnableDisable == null).Distinct().ToList());
                }
                else
                {
                    ViewBag.allcrs = null;
                    return View(dbModel.Post_Courses.Where(a => a.EnableDisable == false || a.EnableDisable == null).Distinct().ToList());
                }
            }

        }

        [HttpPost]
        public ActionResult AddBlocker(string av, Post_Cours hobbies, Int32 id)
        {

            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {

                    ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                    var jbpst = dbModel.Post_Courses.Where(x => x.Secret_Code == id).Distinct().FirstOrDefault();
                    var jbpstlst = dbModel.Post_Courses.Where(x => x.Secret_Code == id).Distinct().ToList();
                    foreach (var i in jbpstlst)
                    {
                        //var dbset = dbModel.Post_Courses.Select(s => new { s.EnableDisable, s.Secret_Code }).Where(x => x.Secret_Code == id).FirstOrDefault();
                        if (i.EnableDisable == false)
                        {
                            i.EnableDisable = true;
                            dbModel.Entry(i).State = EntityState.Modified;
                            dbModel.SaveChanges();
                            ViewBag.abl = "Course is successfully Enabled";
                        }
                        else if (i.EnableDisable == true)
                        {
                           
                            i.EnableDisable = false;
                            dbModel.Entry(i).State = EntityState.Modified;
                            dbModel.SaveChanges();
                            ViewBag.abl = "Course is successfully disabled";
                        }
                        else
                        {
                            ViewBag.abl = "Something is in problem. Please check it";
                        }

                    }
                  
                }

                return RedirectToAction("AddBlocker");
            }
            catch(Exception ex)
            {
                ViewBag.abl = "Something is in problem. Please check it";
            }
            return RedirectToAction("AddBlocker");
        }

        [HttpGet]
        public ActionResult CFUploader()
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                return View();
            }

        }

        [HttpPost]
        public ActionResult CFUploader(Tbl_Cand_Main cert, Paid_Courses_Certificates cf)
        {


            var isExist = IsEmailExistforCErtificate(cert.Cand_EmailId);

            using (EOTAEntities dbModel = new EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                if (isExist)
                {

                    if (dbModel.Tbl_Cand_Main.Any(x => x.Cand_EmailId.Contains(cert.Cand_EmailId) && x.Cand_id == cert.Cand_id))
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

                        ViewBag.Message = "Certificate has been sent to the user's email ID: " + cert.Cand_EmailId + ". Thank you.";

                    }

                }



                else
                {
                    ViewBag.Message = "Email does not Exist";
                    return View(cert);
                }
            }
            ModelState.Clear();

            return View("CFUploader", new Tbl_Cand_Main());


        }
        [Authorize]
        [HttpGet]
        public ActionResult pdcanddetails(int id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                return View(dbModel.Tbl_Cand_Main.Where(x => x.Cand_id == id).FirstOrDefault());
            }

        }
        [Authorize]
        [HttpGet]
        public ActionResult TRainersdetails(int id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                ViewBag.crs = (from a in dbModel.Post_Courses
                               where a.Trainer_Id == id
                               select a.Title).Distinct().ToList();
                return View(dbModel.Trainers.Where(x => x.Id == id).FirstOrDefault());
            }

        }
        [Authorize]
        public ActionResult Dashboard()
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.Paidcand = dbModel.Tbl_Cand_Main.Distinct().ToList();

                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();
                ViewBag.Nonpaid = dbModel.Tbl_Cand_Main.Distinct().ToList();

                ViewBag.trainers = dbModel.Trainers.Distinct().ToList();

                return View(dbModel.Tbl_ADMN.FirstOrDefault());
            }




        }
        public ActionResult allviews(string id, string searchBy, string search, string searchBytrnr, string searchByUsr)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
               
                ViewBag.chkpaidid = dbModel.Paid_Courses.Select
                    (x => x.User_id).Distinct().ToList();
                ViewBag.admnm = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name).Distinct().ToList();

                    ViewBag.allusrs = dbModel.Tbl_Cand_Main.Distinct().ToList();
                ViewBag.ispaid = null;
                if (id != null)
                {
                    if (id == "17")
                    {
                        ViewBag.error = "Something went problem. Please try again";
                    }
                    else
                    {
                        ViewBag.error = id;
                    }
                }
                if (searchByUsr == "True" && search == null)
                {
                    ViewBag.ispaid = "IP";
                }
                else if (searchByUsr == "False" && search == null)
                {
                    ViewBag.ispaid = null;
                }

                else if (searchByUsr != null && search != null )
                {
                   
                    ViewBag.allusrs = dbModel.Tbl_Cand_Main.Where(a => a.CandName.StartsWith(search)).Distinct().ToList();
                }
                else
                {
                    
                    ViewBag.allusrs = dbModel.Tbl_Cand_Main.Distinct().ToList();
                }

                if (searchBytrnr!=null && search!=null)
                {
                    ViewBag.trainers = dbModel.Trainers.Where(a=>a.Name.StartsWith(search)).Distinct().ToList();
                }
                else
                {
                    ViewBag.trainers = dbModel.Trainers.Distinct().ToList();
                }

                if (searchBy == "True")
                {
                    ViewBag.institutions = dbModel.Institute_Infoes.Where(a => a.IsHOD == true).Distinct().ToList();
                    ViewBag.ishod = "T";
                }
                else if (searchBy == "False")
                {
                    ViewBag.institutions = dbModel.Institute_Infoes.Where(a => a.IsHOD == false || a.IsHOD==null).Distinct().ToList();
                }
                else if (searchBy == "Instnm" && search!=null)
                {
                    ViewBag.institutions = dbModel.Institute_Infoes.Where(a => a.Institute_Name_.StartsWith(search)).Distinct().ToList();
                }
                else if (searchBy == "InstId" && search != null)
                {
                    ViewBag.institutions = dbModel.Institute_Infoes.Where(a => a.Institute_Id == search).Distinct().ToList();
                }
                else if (searchBy == "Prmcd" && search != null)
                {
                    ViewBag.institutions = dbModel.Institute_Infoes.Where(a => a.Promocode.StartsWith(search)).Distinct().ToList();
                }
                else if (searchBy == "Prmper" && search != null)
                {
                    ViewBag.institutions = dbModel.Institute_Infoes.Where(a => a.Discount_Percent==search).Distinct().ToList();
                }
                else
                {
                    ViewBag.institutions = dbModel.Institute_Infoes.Distinct().ToList();
                }
                return View(dbModel.Tbl_ADMN.FirstOrDefault());
            }


        }
        [HttpGet]
        public ActionResult trainerenabledisable(int id)
        {
           
            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {
                    var dbset = dbModel.Trainers.Where(x => x.Id == id).FirstOrDefault();
                    var dbset1 = (from a in dbModel.Trainers
                                  where a.Id == id
                                  select a.IsActive).Distinct().FirstOrDefault();
                    if (dbset != null)
                    {
                        if (dbset1 == false)
                        {
                            dbset.IsActive = true;
                            dbModel.Entry(dbset).State =EntityState.Modified;
                            dbModel.SaveChanges();
                            ViewBag.abl = "Successfully Enabled";
                        }
                        else if (dbset1 == true)
                        {

                            dbset.IsActive = false;
                            dbModel.Entry(dbset).State = EntityState.Modified;
                            dbModel.SaveChanges();
                            ViewBag.abl = "Successfully disabled";
                        }
                        else
                        {
                            ViewBag.abl = "Something went problem. Please try again";
                        }

                    }



                }

                return RedirectToAction("allviews", new RouteValueDictionary(
                        new { controller = "ADMIN", action = "allviews", Id = ViewBag.abl }));
            }
            catch (Exception ex)
            {
                return RedirectToAction("allviews", new RouteValueDictionary(
                       new { controller = "ADMIN", action = "allviews", Id = "17" }));
            }



        }
        [Authorize]
        [HttpPost]
        public ActionResult Adm_Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Adm_Login", "ADMIN");
        }

        [HttpGet]
        public ActionResult signin(EtreetrainingUser.Models.Cand_login userModel)
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adm_Login(Tbl_ADMN ADMIN, string ReturnUrl)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                var userdetails = dbModel.Tbl_ADMN.Where(x => x.Admin_EmailId == ADMIN.Admin_EmailId && x.Admin_Pass == ADMIN.Admin_Pass).FirstOrDefault();
                if (userdetails == null)
                {
                    ViewBag.Duplicatemessage = "Invalid credentials, Please Check your details and try again";
                    return View("Adm_Login", ADMIN);
                }
                else
                {
                    int timeout = 60;
                    var ticket = new FormsAuthenticationTicket(ADMIN.Admin_EmailId, ADMIN.rememberMe, timeout);
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
                        return RedirectToAction("TestForInstructor", "Home");

                    }

                }

            }
        }

        public ActionResult ADM_Forgotpass()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Adm_reg(string emid)
        {
            Tbl_ADMN usermodel = new Tbl_ADMN();


            return View(usermodel);
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adm_reg([Bind(Exclude = " IsEmailVerified,ActivationCode")]  Tbl_ADMN userModel)
        {
            try
            {


                string fileName = Path.GetFileNameWithoutExtension(userModel.image.FileName);
                string extension = Path.GetExtension(userModel.image.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                userModel.Admin_ImagePath = "~/Picture/" + fileName;
                fileName = Path.Combine(Server.MapPath("~/Picture/"), fileName);
                userModel.image.SaveAs(fileName);


                bool Status = false;
                string Message = "";
                if (ModelState.IsValid)
                {
                    var isExist = IsEmailExist(userModel.Admin_EmailId);
                    if (isExist)
                    {
                        ModelState.AddModelError("EmailExist", "Email Already Exist");
                        return View(userModel);
                    }

                    userModel.ActivationCode = Guid.NewGuid();

                    userModel.IsEmailVerified = false;
                    using (EOTAEntities dbModel = new EOTAEntities())
                    {
                        dbModel.Tbl_ADMN.Add(userModel);

                        dbModel.SaveChanges();
                        SendVerificationlinkEmail(userModel.Admin_EmailId, userModel.ActivationCode.ToString());
                        MobVarification(userModel.Admin_EmailId, userModel.ActivationCode.ToString());
                        Message = "Registration Successfully done for " + userModel.Admin_EmailId;
                        Status = true;
                    }
                    ViewBag.Message = Message;
                    ViewBag.Status = Status;
                    return RedirectToAction("Adm_Login", "ADMIN", new { id = "1" });
                }
                else
                {

                    ViewBag.Message = "Invalid registration,Please try again.";
                    ViewBag.Status = Status;
                    return View(userModel);
                }

            }
            catch (Exception ex)
            {
                ViewBag.Message = "Something went wrong! Please try again. Thank you.";
                ViewBag.Status = false;
                return View(userModel);
            }




        }

        [NonAction]
        public bool IsEmailExist(string emailId)
        {
            using (EOTAEntities dc = new EOTAEntities())
            {
                var v = dc.Tbl_ADMN.Where(x => x.Admin_EmailId == emailId).FirstOrDefault();
                return v != null;
            }
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
        public void MobVarification(string admin_phn, string ActivationCode)
        {
            admin_phn = "8334895299";
            Random random = new Random();
            int value = random.Next(1001, 9999);
            String SenderName = "Sourav";
            string destinationaddr = "91" + admin_phn;
            string message = "Your OTP number is " + value + "(Sent By:EOTA TRAINNING APP)";
            string message1 = HttpUtility.UrlEncode(message);

            string url = "https://api.textlocal.in/send/?";
            string apikey = "rxtDny7qo+Y-7ViMjoxIQmblNjXBS9AbzySZ2E6vFj";
            string numbers = destinationaddr;

            string sender = "TXTLCL";

            String PostData = "apikey=rxtDny7qo+Y-7ViMjoxIQmblNjXBS9AbzySZ2E6vFj" +
                    "numbers=destinationaddr" +
                    "message=message1" +
                    "sender=TXTLCL";

            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.Method = "POST";

            var encoding = new ASCIIEncoding();
            Byte[] byte1 = encoding.GetBytes(PostData);
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = byte1.Length;
            Stream newStream = req.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);

            {

                try

                {
                    var resp = req.GetResponse();
                    var sr = new StreamReader(resp.GetResponseStream());
                    String results = sr.ReadToEnd();
                    sr.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }


        [NonAction]
        public ViewResult SendVerificationlinkEmaildis(string emailId, string reffcd)
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
        [NonAction]
        public ViewResult SendAdminverification(string emailId, string pass, string name)
        {

            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);

            var server = Request.Url.Segments;

            message.Subject = "Admin Password";

            message.Body = "<br/><br/> Hello " + name + ",<br/><br/> your Admin password is :: " + pass +
                "<br/><br/> Thank you.";



            message.IsBodyHtml = true;

            message.SendMailAsync();

            return View();

        }

        [NonAction]
        public ViewResult SendVerificationlinkEmail(string emailId, string activationCode)
        {

            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);
            int adid = 0;
            string adpass = null;
            using (EOTAEntities tad = new EOTAEntities())
            {
                adid = (from a in tad.Tbl_ADMN
                        where a.Admin_EmailId == emailId
                        select a.Admin_id).Distinct().FirstOrDefault();
                adpass = (from a in tad.Tbl_ADMN
                          where a.Admin_EmailId == emailId
                          select a.Admin_Pass).Distinct().FirstOrDefault();
            }

            var server = Request.Url.Segments;
            var verifyUrl = "/user/verifyaccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            message.Subject = "Your account is successfully created";

            message.Body = "<br/><br/>We are exited to tell you that your EOTA Admin account is successfully created. <br/> Your login details:" +
                 "<br/><br/>Admin id: " + adid +
                "<br/><br/>Email id: " + emailId +
                "<br/><br/>Password: " + adpass;

            message.IsBodyHtml = true;

            message.SendMailAsync();

            return View();

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


        [NonAction]
        public ViewResult SendVerificationlinkEmaildis(string emailId, string Code, string msg)
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
    }

}
