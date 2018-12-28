using EtreetrainingUser.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Zender.Mail;

namespace EtreetrainingUser.Controllers
{
    //[RequireHttps]
    //[HandleError]
    [ValidateInput(false)]
    public class StudentController : Controller
    {
        public ActionResult Certificate(string userid, string crscd)
        {
            ViewBag.dt = DateTime.Now.ToShortDateString();
            int corscd = int.Parse(crscd);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + userid);
                postTaskUname.Wait();
                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                postTaskUId.Wait();
                var GetCoursename = client.GetAsync("http://localhost:25692/api/User/GetCrsNameBySecretCD?scrtcd=" + corscd);
                GetCoursename.Wait();


                var resultName = postTaskUname.Result;
                var resultId = postTaskUId.Result;
                var resultGetCoursename = GetCoursename.Result;


                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    var idchk = readTaskId.Result;
                    if (idchk != userid)
                    {

                        return RedirectToAction("AccessDeny", "Student");
                    }

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. please try again.");
                    return View();
                }

                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ///var readTaskName = resultName.Content.ReadAsAsync<Tbl_Cand_Main>();
                    ViewBag.name = readTaskName.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. please try again.");
                    return View();
                }

                if (resultGetCoursename.IsSuccessStatusCode)
                {
                    var readTaskCrsname = resultGetCoursename.Content.ReadAsStringAsync();
                    readTaskCrsname.Wait();
                    ViewBag.sub = readTaskCrsname.Result;
                    return View();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. please try again.");
                    return View();
                }
            }
            //EOTAEntities db = new EOTAEntities();
            //var idchk = ((from a in db.Tbl_Cand_Main
            //              where a.Cand_EmailId == User.Identity.Name
            //              select a.Cand_id).Distinct().FirstOrDefault()).ToString();
            //if (idchk != userid)
            //{
            //    return RedirectToAction("AccessDeny", "Student");
            //}
            //else
            //{

            //    int uid = int.Parse(userid);
            //    int corscd = int.Parse(crscd);
            //    var nm = (from a in db.Tbl_Cand_Main
            //              where a.Cand_id == uid
            //              select a.CandName).Distinct().FirstOrDefault();
            //    var crsttl = (from a in db.Post_Courses
            //                  where a.Secret_Code == corscd
            //                  select a.Title).Distinct().FirstOrDefault();
            //    ViewBag.name = nm;
            //    ViewBag.sub = crsttl;
            //    ViewBag.dt = DateTime.Now.ToShortDateString();

            //    return View();
            //}
        }
        [HttpPost]
        [ValidateInput(false)]
        public FileResult Export(string GridHtml, string GridHtml1)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                string imageURL = Server.MapPath(".") + "/img771.jpg";
                Image jpg = Image.GetInstance(imageURL);
                GridHtml += string.Format("<div><div><div style='padding-top: 10px; padding-left: 40px; '> <img  src='https://www.etreetraining.com/Student/img771.jpg'></img></div></div></div> ");

                GridHtml += GridHtml1;
                StringReader sr = new StringReader(GridHtml);


                //Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                Rectangle envelope = new Rectangle(580, 420);
                envelope.BackgroundColor = new BaseColor(System.Drawing.Color.LightYellow);
                envelope.BorderColorLeft = BaseColor.CYAN;
                Document pdfDoc = new Document(envelope, 15f, 15f, 15f, 5f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                //string imageURL = Server.MapPath(".") + "/close.png";
                //Image jpg = Image.GetInstance(imageURL);
                //jpg.ScaleToFit(140f, 120f);

                //jpg.SpacingAfter = 1f;
                //jpg.Alignment = Element.ALIGN_LEFT;
                //pdfDoc.Add(jpg);
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);

                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "Certificate.pdf");
            }
        }

        [Authorize]
        public ActionResult star(string star, string cmmnts, Feedback db, string userid, string crscd, string rsltid)
        {
            EOTAEntities fdbck = new EOTAEntities();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
            postTaskUId.Wait();
            var resultId = postTaskUId.Result;
                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    var idchk = readTaskId.Result;
                    if (idchk != userid)
                    {

                        return RedirectToAction("AccessDeny", "Student");
                    }
                    else
                    {
                        if (star == "1")
                        {
                            db.Review = "5";
                        }
                        else if (star == "2")
                        {
                            db.Review = "4";
                        }
                        else if (star == "3")
                        {
                            db.Review = star;
                        }
                        else if (star == "4")
                        {
                            db.Review = "2";
                        }
                        else if (star == "5")
                        {
                            db.Review = "5";
                        }
                        else
                        {
                            db.Review = "0";
                            ViewBag.error = "Sorry, you have to choose atleast one star. Thank you.";
                            return RedirectToAction("Tstresult", new RouteValueDictionary(
                                 new { controller = "Student", action = "Tstresult", error = "You have to select atleast one star", id = rsltid }));
                        }
                        db.Comment = cmmnts;
                        db.Course_Code = crscd;
                        db.User_Email = User.Identity.Name;
                        db.IsActive = true;
                        var PostUserFeedback = client.PostAsJsonAsync("http://localhost:25692/api/User/PostUserFeedback",db);
                        PostUserFeedback.Wait();
                        var resultPostUserFeedback = PostUserFeedback.Result;
                        if (resultPostUserFeedback.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Tstresult", new RouteValueDictionary(
                                  new { controller = "Student", action = "Tstresult", sccssid = "OK", id = rsltid }));
                        }else
                        {
                            ViewBag.error = "Something went wrong. Please try again later. Thank you";
                            return RedirectToAction("Tstresult", new RouteValueDictionary(
                                 new { controller = "Student", action = "Tstresult", error = "You have to select atleast one star", id = rsltid }));
                        }
                        //fdbck.Feedbacks.Add(db);
                        //fdbck.SaveChanges();

                        //return RedirectToAction("Tstresult", new RouteValueDictionary(
                        //          new { controller = "Student", action = "Tstresult", sccssid = "OK", id = rsltid }));
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. please try again.");
                    return View();
                }
        }
            //var idchk = ((from a in fdbck.Tbl_Cand_Main
            //              where a.Cand_EmailId == User.Identity.Name
            //              select a.Cand_id).Distinct().FirstOrDefault()).ToString();
            //if (idchk != userid)
            //{
            //    return RedirectToAction("AccessDeny", "Student");
            //}
           
        }
        [Authorize]
        public ActionResult InstPaymnt(string id, string instid)
        {
            try
            {
                int id1 = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
                int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:25692/api/User");
                    var postTask = client.GetAsync("http://localhost:25692/api/User/GetINSTCrsDetails?id=" + id + "&&instid=" + instid);
                    postTask.Wait();

                    var result = postTask.Result;

                    var statuscode = (int)result.StatusCode;

                    ViewBag.secretcode = id1;
                    ViewBag.userid = userid;
                    ViewBag.insid = instid;

                    if (statuscode == 404)
                    {
                        return RedirectToAction("errorpg", new RouteValueDictionary(
                         new { controller = "Student", action = "errorpg", Id = id, errorpage = "Code does not match with existing data. Please try again with another." }));
                    }
                    else if (statuscode == 406)
                    {
                        return RedirectToAction("errorpg", new RouteValueDictionary(
                        new { controller = "Student", action = "errorpg", Id = id, errorpage = "Institute Id cannot be blank" }));

                    }
                    else if (statuscode == 400)
                    {
                        return RedirectToAction("errorpg", new RouteValueDictionary(
                             new { controller = "Student", action = "errorpg", Id = id, errorpage = "Something went wrong. Please try again." }));
                    }
                    else
                    {
                        if (result.IsSuccessStatusCode)
                        {
                            if (statuscode == 202)
                            {
                                ViewBag.ishod = true;
                            }
                            else
                            {
                                ViewBag.ishod = false;
                            }
                            var readTask1 = result.Content.ReadAsAsync<IList<Post_Cours>>();
                            readTask1.Wait();
                            return View(readTask1.Result);

                        }
                        else
                        {
                            return RedirectToAction("User_Profile");
                        }
                    }


                }
            }
            catch
            {
                string instidcd = (id.Substring(id.LastIndexOf("^") + 1));
                string tst = (id.Substring(0, id.LastIndexOf("^") + 0));

                return RedirectToAction("InstPaymnt", new RouteValueDictionary(
                          new { controller = "Student", action = "InstPaymnt", id = tst, instid = instidcd }));
            }


            //try
            //{
            //    if (instid != null)
            //    {
            //        int id1 = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
            //        int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
            //        using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            //        {
            //            var db = dbModel.Institute_Infoes.Where(x => x.Institute_Id == instid).FirstOrDefault();
            //            if (db != null)
            //            {
            //                ViewBag.ss = dbModel.Post_Courses.Distinct()
            //     .Where(i => i.Course_Type == "T")
            //     .ToArray();
            //                ViewBag.ishod = (from a in dbModel.Institute_Infoes
            //                                 where a.Institute_Id == instid && a.IsHOD == true
            //                                 select a).Distinct().FirstOrDefault();
            //                ViewBag.Techcourse = (from a in dbModel.Post_Courses
            //                                      where a.Course_Type == "T" && a.EnableDisable == true
            //                                      select a.Title).Distinct().ToList();

            //                ViewBag.Acacourse = (from a in dbModel.Post_Courses
            //                                     where a.Course_Type == "A" && a.EnableDisable == true
            //                                     select a.Title).Distinct().ToList();

            //                ViewBag.Mngcourse = (from a in dbModel.Post_Courses
            //                                     where a.Course_Type == "M" && a.EnableDisable == true
            //                                     select a.Title).Distinct().ToList();

            //                ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                                where a.Cand_id == userid
            //                                select a.CandName).FirstOrDefault();
            //                ViewBag.secretcode = id1;
            //                ViewBag.userid = userid;
            //                ViewBag.insid = instid;
            //                var admid = (from a in dbModel.Post_Courses
            //                             where a.Secret_Code == id1 && a.EnableDisable == true
            //                             select a.Admin_Id).Distinct().FirstOrDefault();

            //                ViewBag.admnm = (from a in dbModel.Tbl_ADMN
            //                                 where a.Admin_id == admid
            //                                 select a.AdminName).Distinct().FirstOrDefault();

            //                var trnrid = (from a in dbModel.Post_Courses
            //                              where a.Secret_Code == id1 && a.EnableDisable == true
            //                              select a.Trainer_Id).Distinct().FirstOrDefault();
            //                ViewBag.trnrnm = (from a in dbModel.Trainers
            //                                  where a.Id == trnrid
            //                                  select a.Name).Distinct().FirstOrDefault();
            //                return View(dbModel.Post_Courses.Where(x => x.Secret_Code == id1 && x.EnableDisable == true).Distinct().ToList());
            //            }
            //            else
            //            {
            //                return RedirectToAction("errorpg", new RouteValueDictionary(
            //             new { controller = "Student", action = "errorpg", Id = id }));
            //            }

            //        }
            //    }
            //    else
            //    {
            //        return RedirectToAction("errorpg", new RouteValueDictionary(
            //           new { controller = "Student", action = "errorpg", Id = id, errorpage = "Institute Id cannot be blank" }));


            //    }
            //}
            //catch (Exception ex)
            //{
            //    return RedirectToAction("errorpg", new RouteValueDictionary(
            //         new { controller = "Student", action = "errorpg", Id = id }));
            //}

        }
        [Authorize]
        public ActionResult errorpg(string id, string errorpage)
        {
            ViewBag.id = id;
            if (errorpage == null)
            {
                ViewBag.error = "Error! Sorry your institute ID code is not missmatched. Please try again";
            }
            else
            {
                ViewBag.error = errorpage;
            }

            return View();
        }

        [HttpPost]
        public ActionResult InstPaymnt(string id, Post_Cours pc, FormCollection frm, string Learning_Point)
        {
            string instidcd = (id.Substring(id.LastIndexOf("^") + 1));
            string tst = (id.Substring(0, id.LastIndexOf("^") + 0));
            int id1 = int.Parse(tst.Substring(tst.LastIndexOf("`") + 1));
            int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
            ViewBag.userid = userid;
            ViewBag.secretcode = id1;
            ViewBag.insid = instidcd;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var postTask = client.GetAsync("http://localhost:25692/api/User/GetCrsBySecretCD?id=" + id1);
                postTask.Wait();

                var postTaskamt = client.GetAsync("http://localhost:25692/api/User/GetInstCrsDiscount?id=" + id + "&&Discountcd=" + Learning_Point);
                postTaskamt.Wait();


                var resultamt = postTaskamt.Result;
                var result = postTask.Result;

                var statuscode = (int)resultamt.StatusCode;
                if (Learning_Point == null || Learning_Point == "")
                {
                    ViewBag.error = "Sorry discount cannot be blank. Please try again";
                }
                else
                {
                    if (statuscode == 204)
                    {
                        ViewBag.error = "Sorry You input wrong entry. Discount is not Calculated. Please try again";
                    }
                    else if (statuscode == 406)
                    {
                        ViewBag.error = " Amount cannot be negetive.";
                    }
                    else if (statuscode == 400)
                    {
                        ViewBag.error = "Sorry Something went wrong. Please try again";
                    }
                    else
                    {
                        if (resultamt.IsSuccessStatusCode)
                        {
                            var readTaskamt = resultamt.Content.ReadAsStringAsync();
                            readTaskamt.Wait();
                            ViewBag.finalamount = double.Parse(readTaskamt.Result);
                        }
                    }
                }
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTask.Wait();
                    return View(readTask.Result);

                }
                else
                {
                    return RedirectToAction("User_Profile");
                }


            }
            //       using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            //       {
            //           try
            //           {
            //               try
            //               {
            //                   //var dismodel = dbModel.Institute_Infoes.Where(x => x.Promocode == Learning_Point && x.Institute_Id== instidcd);
            //                   var dismodel = (from a in dbModel.Institute_Infoes
            //                                   where a.Institute_Id == instidcd && a.Promocode == Learning_Point
            //                                   select a).Distinct().FirstOrDefault();
            //                   if (dismodel == null)
            //                   {
            //                       ViewBag.userid = userid;
            //                       ViewBag.secretcode = id1;

            //                       ViewBag.insid = instidcd;
            //                       ViewBag.error = "Sorry You input wrong entry. Discount is not Calculated. Please try again";
            //                   }
            //                   else if (dismodel != null)
            //                   {
            //                       double postprice = int.Parse((from a in dbModel.Post_Courses
            //                                                     where a.Secret_Code == id1
            //                                                     select a.New_Price).Distinct().FirstOrDefault());
            //                       double disper = int.Parse((from a in dbModel.Institute_Infoes
            //                                                  where a.Promocode == Learning_Point
            //                                                  select a.Discount_Percent).Distinct().FirstOrDefault());

            //                       ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                                       where a.Cand_id == userid
            //                                       select a.CandName).FirstOrDefault();
            //                       double amount = postprice * ((disper) / 100);

            //                       double finalamount = postprice - amount;
            //                       if (finalamount != 0)
            //                       {
            //                           ViewBag.finalamount = finalamount;
            //                       }
            //                       ViewBag.secretcode = id1;
            //                       ViewBag.userid = userid;
            //                       ViewBag.insid = instidcd;
            //                   }

            //                   else
            //                   { ViewBag.userid = userid; }
            //               }
            //               catch
            //               {
            //                   ViewBag.userid = userid;
            //                   ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                                   where a.Cand_id == userid
            //                                   select a.CandName).FirstOrDefault();
            //                   ViewBag.error = "Sorry Something went wrong. Please try again";

            //               }
            //           }
            //           catch (Exception ex)
            //           {

            //               ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                               where a.Cand_id == userid
            //                               select a.CandName).FirstOrDefault();
            //               ViewBag.userid = userid;
            //               ViewBag.error = "Sorry You input wrong entry. Discount is not Calculated. Please try again";
            //           }

            //           ViewBag.ss = dbModel.Post_Courses.Distinct()
            //.Where(i => i.Course_Type == "T")
            //.ToArray();
            //           ViewBag.Techcourse = (from a in dbModel.Post_Courses
            //                                 where a.Course_Type == "T" && a.EnableDisable == true
            //                                 select a.Title).Distinct().ToList();

            //           ViewBag.Acacourse = (from a in dbModel.Post_Courses
            //                                where a.Course_Type == "A" && a.EnableDisable == true
            //                                select a.Title).Distinct().ToList();

            //           ViewBag.Mngcourse = (from a in dbModel.Post_Courses
            //                                where a.Course_Type == "M" && a.EnableDisable == true
            //                                select a.Title).Distinct().ToList();
            //           ViewBag.secretcode = id1;

            //           var admid = (from a in dbModel.Post_Courses
            //                        where a.Secret_Code == id1 && a.EnableDisable == true
            //                        select a.Admin_Id).Distinct().FirstOrDefault();
            //           ViewBag.admnm = (from a in dbModel.Tbl_ADMN
            //                            where a.Admin_id == admid
            //                            select a.AdminName).Distinct().FirstOrDefault();

            //           var trnrid = (from a in dbModel.Post_Courses
            //                         where a.Secret_Code == id1 && a.EnableDisable == true
            //                         select a.Trainer_Id).Distinct().FirstOrDefault();
            //           ViewBag.trnrnm = (from a in dbModel.Trainers
            //                             where a.Id == trnrid
            //                             select a.Name).Distinct().FirstOrDefault();

            //           ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                           where a.Cand_id == userid
            //                           select a.CandName).FirstOrDefault();
            //           ViewBag.userid = userid;
            //           return View(dbModel.Post_Courses.Where(x => x.Secret_Code == id1 && x.EnableDisable == true).Distinct().ToList());
            //       }

        }


        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult tstupdate(string id)
        {
             EOTAEntities db = new EOTAEntities();
                ViewBag.resultforGetNEETCourses = db.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault();

            var upadte = (id.Substring(id.LastIndexOf(")") + 1));
            var idfinal = (id.Substring(0, id.LastIndexOf(")") + 0));
            string nuller = null;
            object nuller1 = null;
            var ans = nuller;
            var exmscrtcdfrupdt = (idfinal.Substring(idfinal.LastIndexOf("`") + 1));
            var secretcd1 = (idfinal.Substring(0, idfinal.LastIndexOf("]") + 0));
            var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
            var examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
            var uid = (examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
            var secretcd = int.Parse(secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
            string secretcode = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
            string cut = (idfinal.Substring(idfinal.LastIndexOf("]") + 1));

            var qno = int.Parse(cut.Substring(0, cut.LastIndexOf("^") + 0));
            string remaintime123 = (cut.Substring(cut.LastIndexOf("^") + 1));
            string remaintime = (remaintime123.Substring(0, remaintime123.LastIndexOf("`") + 0));
            var exmscrtcd = nuller1;
            if (upadte == "up")
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("http://localhost:25692/");
                        var GetCrsNameBySecretCD = client.GetAsync("http://localhost:25692/api/User/GetCrsNameBySecretCD?scrtcd=" + secretcd);
                        GetCrsNameBySecretCD.Wait();
                        var resultGetCrsNameBySecretCD = GetCrsNameBySecretCD.Result;
                        if (resultGetCrsNameBySecretCD.IsSuccessStatusCode)
                        {
                            var readTaskCrsname = resultGetCrsNameBySecretCD.Content.ReadAsStringAsync();
                            ViewBag.crsnm = readTaskCrsname.Result;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                            return View();
                        }

                        //ViewBag.crsnm = (from c in dbModel.Post_Courses
                        //             where c.Secret_Code == secretcd
                        //             select c.Title).FirstOrDefault();

                        var id1 = secretcd.ToString();
                        var GetMockTestData = client.GetAsync("http://localhost:25692/api/User/GetMockTestData?scrtcd=" + id1 + "&&Qnomax=" + "Null");
                        GetMockTestData.Wait();
                        var resultGetMockTestData = GetMockTestData.Result;
                        if (resultGetMockTestData.IsSuccessStatusCode)
                        {
                            var readTaskGetMockTestData = resultGetMockTestData.Content.ReadAsAsync<Mock_Test>();
                            var tstdetails = readTaskGetMockTestData.Result;
                            if (tstdetails != null)
                            {
                                ViewBag.tstcd = tstdetails.Exam_Code;
                                ViewBag.tsttm = int.Parse(tstdetails.Exam_Time);

                                var GetMockTestDataByCrsCdExamCd = client.GetAsync("http://localhost:25692/api/User/GetMockTestDataByCrsCdExamCd?scrtcd=" + id1 + "&&Qnumber=" + qno + "&&Exmcd=" + tstdetails.Exam_Code + "&&Pssmrk=" + "Nok");
                                GetMockTestDataByCrsCdExamCd.Wait();
                                var resultGetMockTestDataByCrsCdExamCd = GetMockTestDataByCrsCdExamCd.Result;
                                if (resultGetMockTestDataByCrsCdExamCd.IsSuccessStatusCode)
                                {
                                    var readTaskgetdta = resultGetMockTestDataByCrsCdExamCd.Content.ReadAsAsync<IList<Mock_Test>>();
                                    readTaskgetdta.Wait();

                                    ViewBag.mctstlst = readTaskgetdta.Result;

                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                    return View();
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                            return View();
                        }

                        //    var tstdetails = (from x in dbModel.Mock_Tests
                        //                  where x.Course_Code == id1 && x.EnableDisable == true
                        //                  select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault();
                        //ViewBag.tstcd = tstdetails.Exam_Code;
                        //ViewBag.tsttm = tstdetails.Exam_Time;
                        ViewBag.frst = "2";
                        ViewBag.rmtm = remaintime;
                        var qnofinl = 0;
                        if (qno > 1)
                        {
                            qnofinl = qno - 1;
                        }
                        var exmscrtcd1 = int.Parse(exmscrtcdfrupdt.ToString());
                        var GetMockTestDataQnoMax = client.GetAsync("http://localhost:25692/api/User/GetMockTestData?scrtcd=" + id1 + "&&Qnomax=" + "OK");
                        GetMockTestDataQnoMax.Wait();
                        var resultGetMockTestDataQnoMax = GetMockTestDataQnoMax.Result;
                        if (resultGetMockTestDataQnoMax.IsSuccessStatusCode)
                        {
                            var readTaskQnomax = resultGetMockTestDataQnoMax.Content.ReadAsStringAsync();
                            readTaskQnomax.Wait();
                            var mxqs = int.Parse(readTaskQnomax.Result);
                            ViewBag.tstmaxques = int.Parse(readTaskQnomax.Result);
                            
                            if (qno > mxqs)
                            {
                                ViewBag.qno = qno - 1;
                                var GetMockTestQData = client.GetAsync("http://localhost:25692/api/User/GetMockTestQTypeOrData?Exmcd="
                    + examcode + "&&scrtcd=" + id1 + "&&Qnumber=" + qnofinl + "&&ExamSecretcode=" + exmscrtcd1);
                                GetMockTestQData.Wait();
                                var resultasGetMockTestQData = GetMockTestQData.Result;

                                if (resultasGetMockTestQData.IsSuccessStatusCode)
                                {
                                    var resultQData = resultasGetMockTestQData.Content.ReadAsStringAsync();
                                    ans = resultQData.Result.Replace("\"", "");
                                }
                            }
                            else
                            {
                                ViewBag.qno = qno;
                                var GetMockTestQData = client.GetAsync("http://localhost:25692/api/User/GetMockTestQTypeOrData?Exmcd="
                    + examcode + "&&scrtcd=" + id1 + "&&Qnumber=" + qno + "&&ExamSecretcode=" + exmscrtcd1);
                                GetMockTestQData.Wait();
                                var resultasGetMockTestQData = GetMockTestQData.Result;

                                if (resultasGetMockTestQData.IsSuccessStatusCode)
                                {
                                    var resultQData = resultasGetMockTestQData.Content.ReadAsStringAsync();
                                    ans = resultQData.Result.Replace("\"", "");
                                    
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                            return View();
                        }
                        //var mxqs = (from x in dbModel.Mock_Tests
                        //            where x.Course_Code == id1 && x.EnableDisable == true
                        //            select x.Ques_No).Max();
                        //ViewBag.tstmaxques = mxqs;

                        //var qtp = (from x in dbModel.Mock_Tests
                        //           where x.Course_Code == id1 && x.Exam_Code == examcode && x.EnableDisable == true && x.Question_Type == "T"
                        //           select x.Question_Type).FirstOrDefault();
                        //if (qtp != null)
                        //{
                        //    ViewBag.qtp = "Y";
                        //}
                        //else
                        //{
                        //    ViewBag.qtp = "N";
                        //}
                       
                        string tsttp = null;
                        var GetMockTestQTypeOrData = client.GetAsync("http://localhost:25692/api/User/GetMockTestQTypeOrData?Exmcd="
                             + examcode + "&&scrtcd=" + id1 + "&&Qnumber=" + qno + "&&ExamSecretcode=" + -1);
                        GetMockTestQTypeOrData.Wait();
                    var resultasGetMockTestQTypeOrData = GetMockTestQTypeOrData.Result;
                   
                    if (resultasGetMockTestQTypeOrData.IsSuccessStatusCode)
                    {
                                var resultQtype = resultasGetMockTestQTypeOrData.Content.ReadAsStringAsync();
                                tsttp = resultQtype.Result.Replace("\"", "");
                        }
                         //tsttp = (from x in dbModel.Mock_Tests
                         //            where x.Course_Code == id1 && x.Exam_Code == examcode && x.Ques_No == qno && x.EnableDisable == true
                         //            select x.Question_Type).Distinct().FirstOrDefault();
                       
                        //if (qno > mxqs)
                        //{
                        //    ans = (from x in dbModel.Use_Mock_Tests
                        //           where x.Course_Code == id1 && x.Exam_Code == examcode && x.Ques_No == qnofinl && x.Exam_Secretcode == exmscrtcd1
                        //           select x.Choosed_Option).Distinct().FirstOrDefault();
                        //}
                        //else
                        //{
                        //    ans = (from x in dbModel.Use_Mock_Tests
                        //           where x.Course_Code == id1 && x.Exam_Code == examcode && x.Ques_No == qno && x.Exam_Secretcode == exmscrtcd1
                        //           select x.Choosed_Option).Distinct().FirstOrDefault();
                        //}
                        ViewBag.exmsecretcd = exmscrtcdfrupdt;


                        if (ans != null)
                        {
                            if (tsttp.ToString() == "O")
                            {
                                ViewBag.anschecked = ans;
                            }
                            else if (tsttp == "MO")
                            {
                                string[] strArray = ans.Split(',');

                                foreach (var obj in strArray)
                                {
                                    if (obj == "1")
                                    {
                                        ViewBag.op1 = 1;
                                    }
                                    else if (obj == "2")
                                    {
                                        ViewBag.op2 = 2;
                                    }
                                    else if (obj == "3")
                                    {
                                        ViewBag.op3 = 3;
                                    }
                                    else if (obj == "4")
                                    {
                                        ViewBag.op4 = 4;
                                    }
                                    else if (obj == "5")
                                    {
                                        ViewBag.op5 = 5;
                                    }
                                    else if (obj == "6")
                                    {
                                        ViewBag.op6 = 6;
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            else if (tsttp == "T")
                            {
                                ViewBag.txt = ans;
                            }
                            else
                            {

                            }
                        }

                        //ICollection<Mock_Test> mctt = dbModel.Mock_Tests.Where(x => x.Ques_No == qno && x.Course_Code == id1 && x.Exam_Code == tstdetails.Exam_Code).ToList();
                        //ViewBag.mctstlst = mctt;
                        ViewBag.id = secretcd.ToString();
                        ViewBag.uid = uid;
                        int nmid = int.Parse(uid);
                        //ViewBag.name = (from a in dbModel.Tbl_Cand_Main
                        //                where a.Cand_id == nmid
                        //                select a.CandName).FirstOrDefault();

                        //if (qno > mxqs)
                        //{
                        //    ViewBag.qno = qno - 1;
                        //}
                        //else
                        //{
                        //    ViewBag.qno = qno;
                        //}
                    }
                }

            }
            if (ans == null)
            {
                id = uid + "^" + examcode + "!" + secretcd + "]" + qno + "^" + remaintime + "`" + exmscrtcd + ")" + "up";
                TempData["model"] = id;
                return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
                new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
            }
            else
            {
                return View();
            }

        }
        //[HttpGet]
        //[Authorize]
        //public ActionResult tstupdate(string id)
        //{
        //    var upadte = (id.Substring(id.LastIndexOf(")") + 1));
        //    var idfinal = (id.Substring(0, id.LastIndexOf(")") + 0));
        //    string nuller = null;
        //    object nuller1 = null;
        //    var ans = nuller;
        //    var exmscrtcdfrupdt = (idfinal.Substring(idfinal.LastIndexOf("`") + 1));
        //    var secretcd1 = (idfinal.Substring(0, idfinal.LastIndexOf("]") + 0));
        //    var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
        //    var examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
        //    var uid = (examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
        //    var secretcd = int.Parse(secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
        //    string secretcode = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
        //    string cut = (idfinal.Substring(idfinal.LastIndexOf("]") + 1));

        //    var qno = int.Parse(cut.Substring(0, cut.LastIndexOf("^") + 0));
        //    string remaintime123 = (cut.Substring(cut.LastIndexOf("^") + 1));
        //    string remaintime = (remaintime123.Substring(0, remaintime123.LastIndexOf("`") + 0));
        //    var exmscrtcd = nuller1;
        //    if (upadte == "up")
        //    {
        //        using (EOTAEntities dbModel = new EOTAEntities())
        //        {
        //            ViewBag.crsnm = (from c in dbModel.Post_Courses
        //                             where c.Secret_Code == secretcd
        //                             select c.Title).FirstOrDefault();

        //            var id1 = secretcd.ToString();
        //            var tstdetails = (from x in dbModel.Mock_Tests
        //                              where x.Course_Code == id1 && x.EnableDisable == true
        //                              select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault();
        //            ViewBag.tstcd = tstdetails.Exam_Code;
        //            ViewBag.tsttm = tstdetails.Exam_Time;
        //            ViewBag.frst = "2";
        //            ViewBag.rmtm = remaintime;
        //            var mxqs = (from x in dbModel.Mock_Tests
        //                        where x.Course_Code == id1 && x.EnableDisable == true
        //                        select x.Ques_No).Max();
        //            ViewBag.tstmaxques = mxqs;

        //            var qtp = (from x in dbModel.Mock_Tests
        //                       where x.Course_Code == id1 && x.Exam_Code == examcode && x.EnableDisable == true && x.Question_Type == "T"
        //                       select x.Question_Type).FirstOrDefault();
        //            if (qtp != null)
        //            {
        //                ViewBag.qtp = "Y";
        //            }
        //            else
        //            {
        //                ViewBag.qtp = "N";
        //            }
        //            var tsttp = (from x in dbModel.Mock_Tests
        //                         where x.Course_Code == id1 && x.Exam_Code == examcode && x.Ques_No == qno && x.EnableDisable == true
        //                         select x.Question_Type).Distinct().FirstOrDefault();
        //            var qnofinl = 0;
        //            if (qno > 1)
        //            {
        //                qnofinl = qno - 1;
        //            }

        //            var exmscrtcd1 = int.Parse(exmscrtcdfrupdt.ToString());
        //            if (qno > mxqs)
        //            {

        //                ans = (from x in dbModel.Use_Mock_Tests
        //                       where x.Course_Code == id1 && x.Exam_Code == examcode && x.Ques_No == qnofinl && x.Exam_Secretcode == exmscrtcd1
        //                       select x.Choosed_Option).Distinct().FirstOrDefault();

        //            }
        //            else
        //            {

        //                ans = (from x in dbModel.Use_Mock_Tests
        //                       where x.Course_Code == id1 && x.Exam_Code == examcode && x.Ques_No == qno && x.Exam_Secretcode == exmscrtcd1
        //                       select x.Choosed_Option).Distinct().FirstOrDefault();
        //            }

        //            ViewBag.exmsecretcd = exmscrtcdfrupdt;


        //            if (ans != null)
        //            {
        //                if (tsttp == "O")
        //                {
        //                    ViewBag.anschecked = ans;
        //                }
        //                else if (tsttp == "MO")
        //                {
        //                    string[] strArray = ans.Split(',');

        //                    foreach (var obj in strArray)
        //                    {
        //                        if (obj == "1")
        //                        {
        //                            ViewBag.op1 = 1;
        //                        }
        //                        else if (obj == "2")
        //                        {
        //                            ViewBag.op2 = 2;
        //                        }
        //                        else if (obj == "3")
        //                        {
        //                            ViewBag.op3 = 3;
        //                        }
        //                        else if (obj == "4")
        //                        {
        //                            ViewBag.op4 = 4;
        //                        }
        //                        else if (obj == "5")
        //                        {
        //                            ViewBag.op5 = 5;
        //                        }
        //                        else if (obj == "6")
        //                        {
        //                            ViewBag.op6 = 6;
        //                        }
        //                        else
        //                        {

        //                        }
        //                    }
        //                }
        //                else if (tsttp == "T")
        //                {
        //                    ViewBag.txt = ans;
        //                }
        //                else
        //                {

        //                }
        //            }

        //            ICollection<Mock_Test> mctt = dbModel.Mock_Tests.Where(x => x.Ques_No == qno && x.Course_Code == id1 && x.Exam_Code == tstdetails.Exam_Code).ToList();
        //            ViewBag.mctstlst = mctt;
        //            ViewBag.id = secretcd.ToString();
        //            ViewBag.uid = uid;
        //            int nmid = int.Parse(uid);
        //            ViewBag.name = (from a in dbModel.Tbl_Cand_Main
        //                            where a.Cand_id == nmid
        //                            select a.CandName).FirstOrDefault();

        //            if (qno > mxqs)
        //            {
        //                ViewBag.qno = qno - 1;
        //            }
        //            else
        //            {
        //                ViewBag.qno = qno;
        //            }

        //        }

        //    }
        //    if (ans == null)
        //    {
        //        id = uid + "^" + examcode + "!" + secretcd + "]" + qno + "^" + remaintime + "`" + exmscrtcd + ")" + "up";
        //        TempData["model"] = id;
        //        return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
        //        new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
        //    }
        //    else
        //    {
        //        return View();
        //    }

        //}

        [HttpPost]
        public ActionResult tstupdate(string id, Mock_Test mctst, Use_Mock_Test umt)
        {
            try
            {
                EOTAEntities db = new EOTAEntities();
                ViewBag.resultforGetNEETCourses = db.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault();

                var upadte = (id.Substring(id.LastIndexOf(")") + 1));
                var idfinal = (id.Substring(0, id.LastIndexOf(")") + 0));
                string nuller = null;
                var ans = nuller;

                var secretcd1 = (idfinal.Substring(0, idfinal.LastIndexOf("]") + 0));
                var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
                var examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
                var uid = (examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
                var secretcd = (secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
                //string secretcode = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
                string cut = (idfinal.Substring(idfinal.LastIndexOf("]") + 1));

                var qno = int.Parse(cut.Substring(0, cut.LastIndexOf("^") + 0));
                var qnofinal = qno - 1;


                string remaintime = null;
                string remaintime1233 = null;
                //if (qno - 1 == 1)
                if (qno == 1)
                {
                    remaintime = (cut.Substring(cut.LastIndexOf("^") + 1));
                }
                else
                {
                    remaintime1233 = (cut.Substring(cut.LastIndexOf("^") + 1));
                    remaintime = (remaintime1233.Substring(0, remaintime1233.LastIndexOf("`") + 0));
                }
                var chkqno = qno - 1;
                var exmscrtcd = 0;
                if (qno == 1)
                {
                }
                else
                {
                    exmscrtcd = int.Parse((remaintime1233.Substring(remaintime1233.LastIndexOf("`") + 1)).ToString());
                }
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    
                    if (upadte == "up")
                    {
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri("http://localhost:25692/");
                            var GetCrsNameBySecretCD = client.GetAsync("http://localhost:25692/api/User/GetCrsNameBySecretCD?scrtcd=" + secretcd);
                            GetCrsNameBySecretCD.Wait();
                            var resultGetCrsNameBySecretCD = GetCrsNameBySecretCD.Result;
                            if (resultGetCrsNameBySecretCD.IsSuccessStatusCode)
                            {
                                var readTaskCrsname = resultGetCrsNameBySecretCD.Content.ReadAsStringAsync();
                                ViewBag.crsnm = readTaskCrsname.Result;
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                return View();
                            }
                            //ViewBag.crsnm = (from c in dbModel.Post_Courses
                            //                 //where c.Secret_Code == secretcd
                            //             select c.Title).FirstOrDefault();

                            var id1 = examcode.ToString();
                            var GetMockTestData = client.GetAsync("http://localhost:25692/api/User/GetMockTestData?scrtcd=" + secretcd + "&&Qnomax=" + "Null");
                            GetMockTestData.Wait();
                            var resultGetMockTestData = GetMockTestData.Result;
                            if (resultGetMockTestData.IsSuccessStatusCode)
                            {
                                var readTaskGetMockTestData = resultGetMockTestData.Content.ReadAsAsync<Mock_Test>();
                                var tstdetails = readTaskGetMockTestData.Result;
                                if (tstdetails != null)
                                {
                                    ViewBag.tstcd = tstdetails.Exam_Code;
                                    ViewBag.tsttm = int.Parse(tstdetails.Exam_Time);

                                    var GetMockTestDataByCrsCdExamCd = client.GetAsync("http://localhost:25692/api/User/GetMockTestDataByCrsCdExamCd?scrtcd="
                                        + secretcd + "&&Qnumber=" + qnofinal + "&&Exmcd=" + tstdetails.Exam_Code + "&&Pssmrk=" + "Nok");

                                    GetMockTestDataByCrsCdExamCd.Wait();
                                    var resultGetMockTestDataByCrsCdExamCd = GetMockTestDataByCrsCdExamCd.Result;
                                    if (resultGetMockTestDataByCrsCdExamCd.IsSuccessStatusCode)
                                    {
                                        var readTaskgetdta = resultGetMockTestDataByCrsCdExamCd.Content.ReadAsAsync<IList<Mock_Test>>();
                                        readTaskgetdta.Wait();

                                        ViewBag.mctstlst = readTaskgetdta.Result;

                                    }
                                    else
                                    {
                                        ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                        return View();
                                    }
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                return View();
                            }

                            //var tstdetails = (from x in dbModel.Mock_Tests
                            //                  where x.Course_Code == secretcd && x.EnableDisable == true
                            //                  select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault();
                            //ViewBag.tstcd = tstdetails.Exam_Code;
                            //ViewBag.tsttm = tstdetails.Exam_Time;
                            ViewBag.remtm = remaintime;
                            var maxqs = 0;
                            var GetMockTestDataQnoMax = client.GetAsync("http://localhost:25692/api/User/GetMockTestData?scrtcd=" + secretcd + "&&Qnomax=" + "OK");
                            GetMockTestDataQnoMax.Wait();
                            var resultGetMockTestDataQnoMax = GetMockTestDataQnoMax.Result;
                            if (resultGetMockTestDataQnoMax.IsSuccessStatusCode)
                            {
                                var readTaskQnomax = resultGetMockTestDataQnoMax.Content.ReadAsStringAsync();
                                readTaskQnomax.Wait();
                                maxqs = int.Parse(readTaskQnomax.Result);
                                ViewBag.tstmaxques = int.Parse(readTaskQnomax.Result);
                            }
                            
                            //var maxqs = (from x in dbModel.Mock_Tests
                            //             where x.Course_Code == secretcd && x.EnableDisable == true
                            //             select x.Ques_No).Max();
                            //ViewBag.tstmaxques = maxqs;
                            ViewBag.id = examcode.ToString();

                            //ICollection<Mock_Test> mctt = dbModel.Mock_Tests.Where(x => x.Ques_No == qnofinal && x.Course_Code == secretcd && x.Exam_Code == tstdetails.Exam_Code).ToList();
                            //ViewBag.mctstlst = mctt;
                            ViewBag.id = secretcd.ToString();

                            ViewBag.qno = (qno);
                            int chk1, chk2, chk3, chk4, chk5, chk6 = 0;
                            string chkfnl = null;
                            if (mctst.Checked == null && mctst.Textbx == null)
                            {

                                if (mctst.Optio1 == true)
                                {
                                    chk1 = 1;
                                    chkfnl = chk1.ToString();
                                }
                                if (mctst.Optio2 == true)
                                {
                                    chk2 = 2;
                                    if (chkfnl == null)
                                    {
                                        chkfnl = chk2.ToString();
                                    }
                                    else
                                    {
                                        chkfnl = chkfnl + "," + chk2.ToString();
                                    }

                                }
                                if (mctst.Optio3 == true)
                                {
                                    chk3 = 3;
                                    if (chkfnl == null)
                                    {
                                        chkfnl = chk3.ToString();
                                    }
                                    else
                                    {
                                        chkfnl = chkfnl + "," + chk3.ToString();
                                    }

                                }
                                if (mctst.Optio4 == true)
                                {
                                    chk4 = 4;
                                    if (chkfnl == null)
                                    {
                                        chkfnl = chk4.ToString();
                                    }
                                    else
                                    {
                                        chkfnl = chkfnl + "," + chk4.ToString();
                                    }

                                }
                                if (mctst.Optio5 == true)
                                {
                                    chk5 = 5;
                                    if (chkfnl == null)
                                    {
                                        chkfnl = chk5.ToString();
                                    }
                                    else
                                    {
                                        chkfnl = chkfnl + "," + chk5.ToString();
                                    }

                                }
                                if (mctst.Optio6 == true)
                                {
                                    chk6 = 6;
                                    if (chkfnl == null)
                                    {
                                        chkfnl = chk6.ToString();
                                    }
                                    else
                                    {
                                        chkfnl = chkfnl + "," + chk6.ToString();
                                    }

                                }

                            }

                            object nuller12 = null;
                            var userop = nuller12;
                            var userchk = nuller12;
                            var usertxt = nuller12;
                            if (mctst.Checked != null)
                            {
                                var GetMockTestUMockTstData = client.GetAsync("http://localhost:25692/api/User/GetMockTestUMockTstData?scrtcd="
                                       + secretcd + "&&Qnumber=" + qnofinal + "&&Exmcd=" + id1 + "&&mcktstchk=" + "ok" + "&&UmcktstId=" + "null" + "&&ExamSecretcode=" + -1);

                                GetMockTestUMockTstData.Wait();
                                var resultGetMockTestUMockTstData = GetMockTestUMockTstData.Result;
                                if (resultGetMockTestUMockTstData.IsSuccessStatusCode)
                                {
                                    userop = "notnull";
                                }else
                                {
                                    userop = null;
                                }
                                //userop = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal/* && x.Currect_Option == mctst.Checked*/).FirstOrDefault();
                            }
                            else if (chkfnl != null)
                            {
                                var GetMockTestUMockTstData = client.GetAsync("http://localhost:25692/api/User/GetMockTestUMockTstData?scrtcd="
                                       + secretcd + "&&Qnumber=" + qnofinal + "&&Exmcd=" + id1 + "&&mcktstchk=" + "ok" + "&&UmcktstId=" + "null" + "&&ExamSecretcode=" + -1);

                                GetMockTestUMockTstData.Wait();
                                var resultGetMockTestUMockTstData = GetMockTestUMockTstData.Result;
                                if (resultGetMockTestUMockTstData.IsSuccessStatusCode)
                                {
                                    userchk = "notnull";
                                }
                                else
                                {
                                    userchk = null;
                                }
                                //userchk = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal /*&& x.Currect_Option == chkfnl*/).FirstOrDefault();
                            }
                            else if (mctst.Textbx != null)
                            {
                                var GetMockTestUMockTstData = client.GetAsync("http://localhost:25692/api/User/GetMockTestUMockTstData?scrtcd="
                                       + secretcd + "&&Qnumber=" + qnofinal + "&&Exmcd=" + id1 + "&&mcktstchk=" + "ok" + "&&UmcktstId=" + "null" + "&&ExamSecretcode=" + -1);

                                GetMockTestUMockTstData.Wait();
                                var resultGetMockTestUMockTstData = GetMockTestUMockTstData.Result;
                                if (resultGetMockTestUMockTstData.IsSuccessStatusCode)
                                {
                                    usertxt = "notnull";
                                }
                                else
                                {
                                    usertxt = null;
                                }
                                //usertxt = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal).FirstOrDefault();
                            }
                            else{}
                            
                            if (userop == null && userchk == null && usertxt == null)
                            {
                                string aqwe = null;
                                var GetMockTestUMockTstDatafrops = client.GetAsync("http://localhost:25692/api/User/GetMockTestUMockTstData?scrtcd="
                                    + secretcd + "&&Qnumber=" + qno + "&&Exmcd=" + examcode + "&&mcktstchk=" + "null" + "&&UmcktstId=" + "null" +
                                    "&&ExamSecretcode=" + -1);

                                GetMockTestUMockTstDatafrops.Wait();
                                var resultGetMockTestUMockTstDatafrops = GetMockTestUMockTstDatafrops.Result;
                                if (resultGetMockTestUMockTstDatafrops.IsSuccessStatusCode)
                                {
                                    var readresultGetMockTestUMockTstDatafrops = resultGetMockTestUMockTstDatafrops.Content.ReadAsStringAsync();
                                    aqwe = readresultGetMockTestUMockTstDatafrops.Result.Replace("\"", "");
                                }
                               
                                 //aqwe = (from x in dbModel.Use_Mock_Tests
                                 //           where x.Course_Code == secretcd && x.Exam_Code == examcode && x.Ques_No == qno
                                 //           select x.Choosed_Option).Distinct().FirstOrDefault();
                                if (aqwe != null)
                                {
                                    if (qno > maxqs)
                                    {
                                        id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime + ")" + "up";
                                        TempData["model"] = id;

                                        return RedirectToAction("tstupdate", new RouteValueDictionary(
                            new { controller = "Student", action = "tstupdate", Id = id }));
                                    }
                                    else
                                    {
                                        id = uid + "^" + examcode + "!" + secretcd + "]" + qno + "^" + remaintime + ")" + "up";
                                        TempData["model"] = id;

                                        return RedirectToAction("tstupdate", new RouteValueDictionary(
                             new { controller = "Student", action = "tstupdate", Id = id }));
                                    }
                                }
                                else
                                {

                                    ViewBag.UploadStatus = "Error";
                                    if (qno > maxqs)
                                    {
                                        id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime;
                                    }
                                    TempData["model"] = id;

                                    return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
                    new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
                                }
                            }

                            else
                            {
                                var i = 0;
                                if (exmscrtcd == 0)
                                {
                                    int sc = int.Parse(secretcd);
                                    var GetMockTestUMockTstDatafrid = client.GetAsync("http://localhost:25692/api/User/GetMockTestUMockTstData?scrtcd="
                                   + secretcd + "&&Qnumber=" + qnofinal + "&&Exmcd=" + examcode + "&&mcktstchk=" + "null" + "&&UmcktstId=" + "ok" +
                                   "&&ExamSecretcode=" + sc);

                                    GetMockTestUMockTstDatafrid.Wait();
                                    var resultGetMockTestUMockTstDatafrid = GetMockTestUMockTstDatafrid.Result;
                                    if (resultGetMockTestUMockTstDatafrid.IsSuccessStatusCode)
                                    {
                                        var readresultGetMockTestUMockTstDatafrid = resultGetMockTestUMockTstDatafrid.Content.ReadAsStringAsync();
                                        i =int.Parse(readresultGetMockTestUMockTstDatafrid.Result);
                                    }
                                    //i = (from a in dbModel.Use_Mock_Tests
                                    //     where a.Exam_Code == examcode && a.Course_Code == secretcd && a.Ques_No == qnofinal && a.Exam_Secretcode == sc
                                    //     select a.Id).FirstOrDefault();
                                }
                                else
                                {
                                    var GetMockTestUMockTstDatafrid = client.GetAsync("http://localhost:25692/api/User/GetMockTestUMockTstData?scrtcd="
                                 + secretcd + "&&Qnumber=" + qnofinal + "&&Exmcd=" + examcode + "&&mcktstchk=" + "null" + "&&UmcktstId=" + "ok" +
                                 "&&ExamSecretcode=" + exmscrtcd);

                                    GetMockTestUMockTstDatafrid.Wait();
                                    var resultGetMockTestUMockTstDatafrid = GetMockTestUMockTstDatafrid.Result;
                                    if (resultGetMockTestUMockTstDatafrid.IsSuccessStatusCode)
                                    {
                                        var readresultGetMockTestUMockTstDatafrid = resultGetMockTestUMockTstDatafrid.Content.ReadAsStringAsync();
                                        i = int.Parse(readresultGetMockTestUMockTstDatafrid.Result.Replace("\"", ""));
                                    }
                                    //i = (from a in dbModel.Use_Mock_Tests
                                    //     where a.Exam_Code == examcode && a.Course_Code == secretcd && a.Ques_No == qnofinal && a.Exam_Secretcode == exmscrtcd
                                    //     select a.Id).FirstOrDefault();
                                }
                                umt.Id = i;
                                umt.User_Id = int.Parse(uid);
                                var uidfn = int.Parse(uid);
                                umt.User_Email = User.Identity.Name;
                                umt.Exam_Code = examcode;
                                umt.Course_Code = secretcd;
                                umt.Ques_No = qnofinal;
                                if (chkfnl != null)
                                {
                                    umt.Choosed_Option = chkfnl;
                                }
                                if (mctst.Checked != null)
                                {
                                    umt.Choosed_Option = mctst.Checked;
                                }
                                if (mctst.Checked == null && chkfnl == null)
                                {
                                    umt.Choosed_Option = mctst.Textbx;
                                }
                                object marks = nuller;

                                if (mctst.Checked != null)
                                {
                                    var GetMockTestDatachecking = client.GetAsync("http://localhost:25692/api/User/GetMockTestDatachecking?scrtcd="
                                 + secretcd + "&&Qnumber=" + qnofinal + "&&Exmcd=" + id1 + "&&crctops=" + mctst.Checked);

                                    GetMockTestDatachecking.Wait();
                                    var resultGetMockTestDatachecking = GetMockTestDatachecking.Result;
                                    if (resultGetMockTestDatachecking.IsSuccessStatusCode)
                                    {
                                        umt.Marks = 1;
                                    }else
                                    {
                                        umt.Marks = 0;
                                    }
                                    //marks = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal && x.Currect_Option == mctst.Checked).FirstOrDefault();
                                    //if (marks != null)
                                    //{
                                    //    umt.Marks = 1;
                                    //}
                                    //else
                                    //{
                                    //    umt.Marks = 0;
                                    //}
                                }
                                else if (chkfnl != null)
                                {
                                    var GetMockTestDatachecking = client.GetAsync("http://localhost:25692/api/User/GetMockTestDatachecking?scrtcd="
                                + secretcd + "&&Qnumber=" + qnofinal + "&&Exmcd=" + id1 + "&&crctops=" + chkfnl);

                                    GetMockTestDatachecking.Wait();
                                    var resultGetMockTestDatachecking = GetMockTestDatachecking.Result;
                                    if (resultGetMockTestDatachecking.IsSuccessStatusCode)
                                    {
                                        umt.Marks = 1;
                                    }
                                    else
                                    {
                                        umt.Marks = 0;
                                    }
                                    //marks = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal && x.Currect_Option == chkfnl).FirstOrDefault();
                                    //if (marks != null)
                                    //{
                                    //    umt.Marks = 1;
                                    //}
                                    //else
                                    //{
                                    //    umt.Marks = 0;
                                    //}
                                }
                                umt.Exam_Date = DateTime.Now;

                                umt.Exam_Secretcode = exmscrtcd;
                                var PutUserMockTest = client.PutAsJsonAsync("http://localhost:25692/api/User/PutUserMockTest",umt);
                                PutUserMockTest.Wait();
                                var resultasPutUserMockTest = PutUserMockTest.Result;
                                if(!resultasPutUserMockTest.IsSuccessStatusCode)
                                {
                                    ModelState.AddModelError(string.Empty, "Something went wrong");
                                    return View();
                                }
                                
                                //dbModel.Entry(umt).State = EntityState.Modified;
                                //dbModel.SaveChanges();
                            }
                            var scrtctexm = umt.Exam_Secretcode;
                            if (qno > maxqs)
                            {
                               
                                ViewBag.qno = qno - 1;
                                var GetMockTestQData = client.GetAsync("http://localhost:25692/api/User/GetMockTestQTypeOrData?Exmcd="
                    + examcode + "&&scrtcd=" + secretcd + "&&Qnumber=" + qnofinal + "&&ExamSecretcode=" + scrtctexm);
                                GetMockTestQData.Wait();
                                var resultasGetMockTestQData = GetMockTestQData.Result;

                                if (resultasGetMockTestQData.IsSuccessStatusCode)
                                {
                                    var resultQData = resultasGetMockTestQData.Content.ReadAsStringAsync();
                                    ans = resultQData.Result.Replace("\"", "");
                                }
                            }
                            else
                            {
                                ViewBag.qno = qno;
                                var GetMockTestQData = client.GetAsync("http://localhost:25692/api/User/GetMockTestQTypeOrData?Exmcd="
                    + examcode + "&&scrtcd=" + secretcd + "&&Qnumber=" + qno + "&&ExamSecretcode=" + scrtctexm);
                                GetMockTestQData.Wait();
                                var resultasGetMockTestQData = GetMockTestQData.Result;

                                if (resultasGetMockTestQData.IsSuccessStatusCode)
                                {
                                    var resultQData = resultasGetMockTestQData.Content.ReadAsStringAsync();
                                    ans = resultQData.Result.Replace("\"", "");

                                }
                            }
                            //ans = (from x in dbModel.Use_Mock_Tests
                            //       where x.Course_Code == secretcd && x.Exam_Code == examcode && x.Ques_No == qno && x.Exam_Secretcode == scrtctexm
                            //       select x.Choosed_Option).Distinct().FirstOrDefault();
                            //if (qno > maxqs)
                            //{
                            //    ans = (from x in dbModel.Use_Mock_Tests
                            //           where x.Course_Code == secretcd && x.Exam_Code == examcode && x.Ques_No == qnofinal && x.Exam_Secretcode == scrtctexm
                            //           select x.Choosed_Option).Distinct().FirstOrDefault();
                            //}
                            if (ans != null)
                            {
                                if (qno > maxqs)
                                {
                                    id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime + "`" + scrtctexm + ")" + "up";
                                    TempData["model"] = id;

                                    return RedirectToAction("tstupdate", new RouteValueDictionary(
                        new { controller = "Student", action = "tstupdate", Id = id }));
                                }
                                else
                                {
                                    id = uid + "^" + examcode + "!" + secretcd + "]" + qno + "^" + remaintime + "`" + scrtctexm + ")" + "up";
                                    TempData["model"] = id;

                                    return RedirectToAction("tstupdate", new RouteValueDictionary(
                         new { controller = "Student", action = "tstupdate", Id = id }));
                                }
                            }
                            else
                            {
                                if (qno > maxqs)
                                {
                                    id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime + "`" + scrtctexm;
                                    TempData["model"] = id;

                                    return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
                        new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
                                }
                                else
                                {
                                    id = uid + "^" + examcode + "!" + secretcd + "]" + qno + "^" + remaintime + "`" + scrtctexm;
                                    TempData["model"] = id;

                                    return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
                        new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
                                }
                            }
                        }
                    }
                    else
                    {
                        return View();
                    }


                }
            }
           
            catch (Exception)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:25692/api/User");
                    var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                    postTaskUId.Wait();
                    var resultId = postTaskUId.Result;
                    if (resultId.IsSuccessStatusCode)
                    {
                        var readTaskId = resultId.Content.ReadAsStringAsync();
                        var userid = readTaskId.Result;
                        return RedirectToAction("Mocktst", new RouteValueDictionary(
                  new { controller = "Student", action = "Mocktst", Id = userid }));

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Something went wrong. please try again.");
                        return View();
                    }
                }
            }
        }

        //[HttpPost]
        //public ActionResult tstupdate(string id, Mock_Test mctst, Use_Mock_Test umt)
        //{
        //    try
        //    {
        //        var upadte = (id.Substring(id.LastIndexOf(")") + 1));
        //        var idfinal = (id.Substring(0, id.LastIndexOf(")") + 0));
        //        string nuller = null;
        //        var ans = nuller;

        //        var secretcd1 = (idfinal.Substring(0, idfinal.LastIndexOf("]") + 0));
        //        var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
        //        var examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
        //        var uid = (examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
        //        var secretcd = (secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
        //        //string secretcode = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
        //        string cut = (idfinal.Substring(idfinal.LastIndexOf("]") + 1));

        //        var qno = int.Parse(cut.Substring(0, cut.LastIndexOf("^") + 0));
        //        var qnofinal = qno - 1;


        //        string remaintime = null;
        //        string remaintime1233 = null;
        //        //if (qno - 1 == 1)
        //        if (qno == 1)
        //        {
        //            remaintime = (cut.Substring(cut.LastIndexOf("^") + 1));
        //        }
        //        else
        //        {
        //            remaintime1233 = (cut.Substring(cut.LastIndexOf("^") + 1));
        //            remaintime = (remaintime1233.Substring(0, remaintime1233.LastIndexOf("`") + 0));
        //        }
        //        var chkqno = qno - 1;
        //        var exmscrtcd = 0;
        //        if (qno == 1)
        //        {
        //        }
        //        else
        //        {
        //            exmscrtcd = int.Parse((remaintime1233.Substring(remaintime1233.LastIndexOf("`") + 1)).ToString());
        //        }
        //        using (EOTAEntities dbModel = new EOTAEntities())
        //        {
        //            if (upadte == "up")
        //            {
        //                ViewBag.crsnm = (from c in dbModel.Post_Courses
        //                                     //where c.Secret_Code == secretcd
        //                                 select c.Title).FirstOrDefault();

        //                var id1 = examcode.ToString();
        //                var tstdetails = (from x in dbModel.Mock_Tests
        //                                  where x.Course_Code == secretcd && x.EnableDisable == true
        //                                  select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault();
        //                ViewBag.tstcd = tstdetails.Exam_Code;
        //                ViewBag.tsttm = tstdetails.Exam_Time;
        //                ViewBag.remtm = remaintime;
        //                var maxqs = (from x in dbModel.Mock_Tests
        //                             where x.Course_Code == secretcd && x.EnableDisable == true
        //                             select x.Ques_No).Max();
        //                ViewBag.tstmaxques = maxqs;
        //                ViewBag.id = examcode.ToString();

        //                ICollection<Mock_Test> mctt = dbModel.Mock_Tests.Where(x => x.Ques_No == qnofinal && x.Course_Code == secretcd && x.Exam_Code == tstdetails.Exam_Code).ToList();
        //                ViewBag.mctstlst = mctt;
        //                ViewBag.id = secretcd.ToString();

        //                ViewBag.qno = (qno);
        //                int chk1, chk2, chk3, chk4, chk5, chk6 = 0;
        //                string chkfnl = null;
        //                if (mctst.Checked == null && mctst.Textbx == null)
        //                {

        //                    if (mctst.Optio1 == true)
        //                    {
        //                        chk1 = 1;
        //                        chkfnl = chk1.ToString();
        //                    }
        //                    if (mctst.Optio2 == true)
        //                    {
        //                        chk2 = 2;
        //                        if (chkfnl == null)
        //                        {
        //                            chkfnl = chk2.ToString();
        //                        }
        //                        else
        //                        {
        //                            chkfnl = chkfnl + "," + chk2.ToString();
        //                        }

        //                    }
        //                    if (mctst.Optio3 == true)
        //                    {
        //                        chk3 = 3;
        //                        if (chkfnl == null)
        //                        {
        //                            chkfnl = chk3.ToString();
        //                        }
        //                        else
        //                        {
        //                            chkfnl = chkfnl + "," + chk3.ToString();
        //                        }

        //                    }
        //                    if (mctst.Optio4 == true)
        //                    {
        //                        chk4 = 4;
        //                        if (chkfnl == null)
        //                        {
        //                            chkfnl = chk4.ToString();
        //                        }
        //                        else
        //                        {
        //                            chkfnl = chkfnl + "," + chk4.ToString();
        //                        }

        //                    }
        //                    if (mctst.Optio5 == true)
        //                    {
        //                        chk5 = 5;
        //                        if (chkfnl == null)
        //                        {
        //                            chkfnl = chk5.ToString();
        //                        }
        //                        else
        //                        {
        //                            chkfnl = chkfnl + "," + chk5.ToString();
        //                        }

        //                    }
        //                    if (mctst.Optio6 == true)
        //                    {
        //                        chk6 = 6;
        //                        if (chkfnl == null)
        //                        {
        //                            chkfnl = chk6.ToString();
        //                        }
        //                        else
        //                        {
        //                            chkfnl = chkfnl + "," + chk6.ToString();
        //                        }

        //                    }

        //                }

        //                object nuller12 = null;
        //                var userop = nuller12;
        //                var userchk = nuller12;
        //                var usertxt = nuller12;
        //                if (mctst.Checked != null)
        //                {
        //                    userop = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal/* && x.Currect_Option == mctst.Checked*/).FirstOrDefault();
        //                }
        //                else if (chkfnl != null)
        //                {
        //                    userchk = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal /*&& x.Currect_Option == chkfnl*/).FirstOrDefault();
        //                }
        //                else if (mctst.Textbx != null)
        //                {
        //                    usertxt = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal).FirstOrDefault();
        //                }
        //                else
        //                {

        //                }
        //                if (userop == null && userchk == null && usertxt == null)
        //                {
        //                    var aqwe = (from x in dbModel.Use_Mock_Tests
        //                                where x.Course_Code == secretcd && x.Exam_Code == examcode && x.Ques_No == qno
        //                                select x.Choosed_Option).Distinct().FirstOrDefault();
        //                    if (aqwe != null)
        //                    {
        //                        if (qno > maxqs)
        //                        {
        //                            id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime + ")" + "up";
        //                            TempData["model"] = id;

        //                            return RedirectToAction("tstupdate", new RouteValueDictionary(
        //                new { controller = "Student", action = "tstupdate", Id = id }));
        //                        }
        //                        else
        //                        {
        //                            id = uid + "^" + examcode + "!" + secretcd + "]" + qno + "^" + remaintime + ")" + "up";
        //                            TempData["model"] = id;

        //                            return RedirectToAction("tstupdate", new RouteValueDictionary(
        //                 new { controller = "Student", action = "tstupdate", Id = id }));
        //                        }
        //                    }
        //                    else
        //                    {

        //                        ViewBag.UploadStatus = "Error";
        //                        if (qno > maxqs)
        //                        {
        //                            id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime;
        //                        }
        //                        TempData["model"] = id;

        //                        return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
        //        new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
        //                    }
        //                }

        //                else
        //                {

        //                    var i = 0;
        //                    if (exmscrtcd == 0)
        //                    {
        //                        int sc = int.Parse(secretcd);
        //                        i = (from a in dbModel.Use_Mock_Tests
        //                             where a.Exam_Code == examcode && a.Course_Code == secretcd && a.Ques_No == qnofinal && a.Exam_Secretcode == sc
        //                             select a.Id).FirstOrDefault();
        //                    }
        //                    else
        //                    {
        //                        i = (from a in dbModel.Use_Mock_Tests
        //                             where a.Exam_Code == examcode && a.Course_Code == secretcd && a.Ques_No == qnofinal && a.Exam_Secretcode == exmscrtcd
        //                             select a.Id).FirstOrDefault();
        //                    }


        //                    umt.Id = i;
        //                    umt.User_Id = int.Parse(uid);
        //                    var uidfn = int.Parse(uid);
        //                    umt.User_Email = (from c in dbModel.Tbl_Cand_Main
        //                                      where c.Cand_id == uidfn
        //                                      select c.Cand_EmailId).FirstOrDefault();
        //                    umt.Exam_Code = examcode;
        //                    umt.Course_Code = secretcd;
        //                    umt.Ques_No = qnofinal;
        //                    if (chkfnl != null)
        //                    {
        //                        umt.Choosed_Option = chkfnl;
        //                    }
        //                    if (mctst.Checked != null)
        //                    {
        //                        umt.Choosed_Option = mctst.Checked;
        //                    }
        //                    if (mctst.Checked == null && chkfnl == null)
        //                    {
        //                        umt.Choosed_Option = mctst.Textbx;
        //                    }
        //                    object marks = nuller;

        //                    if (mctst.Checked != null)
        //                    {
        //                        marks = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal && x.Currect_Option == mctst.Checked).FirstOrDefault();
        //                        if (marks != null)
        //                        {
        //                            umt.Marks = 1;
        //                        }
        //                        else
        //                        {
        //                            umt.Marks = 0;
        //                        }
        //                    }
        //                    else if (chkfnl != null)
        //                    {
        //                        marks = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal && x.Currect_Option == chkfnl).FirstOrDefault();
        //                        if (marks != null)
        //                        {
        //                            umt.Marks = 1;
        //                        }
        //                        else
        //                        {
        //                            umt.Marks = 0;
        //                        }
        //                    }
        //                    umt.Exam_Date = DateTime.Now;

        //                    umt.Exam_Secretcode = exmscrtcd;
        //                    dbModel.Entry(umt).State = System.Data.EntityState.Modified;
        //                    dbModel.SaveChanges();

        //                }
        //                var scrtctexm = umt.Exam_Secretcode;
        //                ans = (from x in dbModel.Use_Mock_Tests
        //                       where x.Course_Code == secretcd && x.Exam_Code == examcode && x.Ques_No == qno && x.Exam_Secretcode == scrtctexm
        //                       select x.Choosed_Option).Distinct().FirstOrDefault();
        //                if (qno > maxqs)
        //                {
        //                    ans = (from x in dbModel.Use_Mock_Tests
        //                           where x.Course_Code == secretcd && x.Exam_Code == examcode && x.Ques_No == qnofinal && x.Exam_Secretcode == scrtctexm
        //                           select x.Choosed_Option).Distinct().FirstOrDefault();
        //                }
        //                if (ans != null)
        //                {
        //                    if (qno > maxqs)
        //                    {
        //                        id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime + "`" + scrtctexm + ")" + "up";
        //                        TempData["model"] = id;

        //                        return RedirectToAction("tstupdate", new RouteValueDictionary(
        //            new { controller = "Student", action = "tstupdate", Id = id }));
        //                    }
        //                    else
        //                    {
        //                        id = uid + "^" + examcode + "!" + secretcd + "]" + qno + "^" + remaintime + "`" + scrtctexm + ")" + "up";
        //                        TempData["model"] = id;

        //                        return RedirectToAction("tstupdate", new RouteValueDictionary(
        //             new { controller = "Student", action = "tstupdate", Id = id }));
        //                    }
        //                }
        //                else
        //                {
        //                    if (qno > maxqs)
        //                    {
        //                        id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime + "`" + scrtctexm;
        //                        TempData["model"] = id;

        //                        return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
        //            new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
        //                    }
        //                    else
        //                    {
        //                        id = uid + "^" + examcode + "!" + secretcd + "]" + qno + "^" + remaintime + "`" + scrtctexm;
        //                        TempData["model"] = id;

        //                        return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
        //            new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
        //                    }
        //                }

        //            }
        //            else
        //            {
        //                return View();
        //            }


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        using (EOTAEntities dbModel = new EOTAEntities())
        //        {
        //            var userid = (from x in dbModel.Tbl_Cand_Main
        //                          where x.Cand_EmailId == User.Identity.Name
        //                          select x.Cand_id).Distinct().FirstOrDefault();
        //            return RedirectToAction("Mocktst", new RouteValueDictionary(
        //           new { controller = "Student", action = "Mocktst", Id = userid }));
        //        }
        //    }
        //}

        [HttpGet]
        [Authorize]
        public ActionResult Mockttststartpagefirst(string id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    EOTAEntities db = new EOTAEntities();
                    ViewBag.resultforGetNEETCourses = db.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault();

                    client.BaseAddress = new Uri("http://localhost:25692/api/User");
                    if (TempData["model"] != null)
                    {
                        int nuller = 0;
                        object nuller5 = null;
                        string nuller1 = null;
                        var exmscrtcdfrupdt = nuller;
                        var upadte = (id.Substring(id.LastIndexOf(")") + 1));
                        var idfinal = nuller1;
                        if (upadte == "up")
                        {
                            idfinal = (id.Substring(0, id.LastIndexOf(")") + 0));
                            exmscrtcdfrupdt = int.Parse(idfinal.Substring(idfinal.LastIndexOf("`") + 1));
                        }
                        else
                        {
                            if (id.Contains("`"))
                            {
                                exmscrtcdfrupdt = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
                            }

                        }
                        ViewBag.exmsecretcd = exmscrtcdfrupdt;
                        var examcode = nuller5;
                        var secretcd = nuller;
                        var uid = nuller1;
                        var qno = nuller;
                        var remaintime = nuller1;

                        if (upadte == null)
                        {
                            var secretcd1 = (id.Substring(0, id.LastIndexOf("]") + 0));
                            var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
                            examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
                            uid = (examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
                            secretcd = int.Parse(secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
                            string secretcode = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
                            string cut = (id.Substring(id.LastIndexOf("]") + 1));

                            qno = int.Parse(cut.Substring(0, cut.LastIndexOf("^") + 0));
                            remaintime = (cut.Substring(cut.LastIndexOf("^") + 1));

                        }
                        else
                        {
                            var secretcd1 = (id.Substring(0, id.LastIndexOf("]") + 0));
                            var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
                            examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
                            uid = (examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
                            secretcd = int.Parse(secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
                            string secretcode = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
                            string cut = (id.Substring(id.LastIndexOf("]") + 1));

                            qno = int.Parse(cut.Substring(0, cut.LastIndexOf("^") + 0));
                            var remaintime1 = (cut.Substring(cut.LastIndexOf("^") + 1));

                            if (upadte == "up")
                            {
                                var exmscrtcdfrupdtrt = (remaintime1.Substring(remaintime1.LastIndexOf(")") + 1));
                                remaintime = (exmscrtcdfrupdtrt.Substring(0, exmscrtcdfrupdtrt.LastIndexOf("`") + 0));
                            }
                            else
                            {
                                if (remaintime1.Contains("`"))
                                {
                                    remaintime = (remaintime1.Substring(0, remaintime1.LastIndexOf("`") + 0));
                                }
                                else
                                {
                                    remaintime = remaintime1;
                                }
                            }
                        }
                        using (EOTAEntities dbModel = new EOTAEntities())
                        {
                            var GetCrsNameBySecretCD = client.GetAsync("http://localhost:25692/api/User/GetCrsNameBySecretCD?scrtcd=" + secretcd);
                            GetCrsNameBySecretCD.Wait();
                            var resultGetCrsNameBySecretCD = GetCrsNameBySecretCD.Result;
                            if (resultGetCrsNameBySecretCD.IsSuccessStatusCode)
                            {
                                var readTaskCrsname = resultGetCrsNameBySecretCD.Content.ReadAsStringAsync();
                                ViewBag.crsnm = readTaskCrsname.Result;
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                return View();
                            }
                            //ViewBag.crsnm = (from c in dbModel.Post_Courses
                            //                 where c.Secret_Code == secretcd
                            //                 select c.Title).FirstOrDefault();

                            var id1 = secretcd.ToString();
                            var GetMockTestData = client.GetAsync("http://localhost:25692/api/User/GetMockTestData?scrtcd=" + id1 + "&&Qnomax=" + "Null");
                            GetMockTestData.Wait();
                            var resultGetMockTestData = GetMockTestData.Result;
                            if (resultGetMockTestData.IsSuccessStatusCode)
                            {
                                var readTaskGetMockTestData = resultGetMockTestData.Content.ReadAsAsync<Mock_Test>();
                                var tstdetails = readTaskGetMockTestData.Result;
                                if (tstdetails != null)
                                {
                                    ViewBag.tstcd = tstdetails.Exam_Code;
                                    ViewBag.tsttm = int.Parse(tstdetails.Exam_Time);

                                    var GetMockTestDataByCrsCdExamCd = client.GetAsync("http://localhost:25692/api/User/GetMockTestDataByCrsCdExamCd?scrtcd=" + id1 + "&&Qnumber=" + qno + "&&Exmcd=" + tstdetails.Exam_Code + "&&Pssmrk=" + "Nok");
                                    GetMockTestDataByCrsCdExamCd.Wait();
                                    var resultGetMockTestDataByCrsCdExamCd = GetMockTestDataByCrsCdExamCd.Result;
                                    if (resultGetMockTestDataByCrsCdExamCd.IsSuccessStatusCode)
                                    {
                                        var readTaskgetdta = resultGetMockTestDataByCrsCdExamCd.Content.ReadAsAsync<IList<Mock_Test>>();
                                        readTaskgetdta.Wait();

                                        ViewBag.mctstlst = readTaskgetdta.Result;

                                    }
                                    else
                                    {
                                        ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                        return View();
                                    }
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                return View();
                            }
                            //var tstdetails = (from x in dbModel.Mock_Tests
                            //                  where x.Course_Code == id1 && x.EnableDisable == true
                            //                  select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault();
                            //ViewBag.tstcd = tstdetails.Exam_Code;
                            //ViewBag.tsttm = tstdetails.Exam_Time;
                            ViewBag.frst = "2";
                            ViewBag.rmtm = remaintime;

                            var GetMockTestDataQnoMax = client.GetAsync("http://localhost:25692/api/User/GetMockTestData?scrtcd=" + id1 + "&&Qnomax=" + "OK");
                            GetMockTestDataQnoMax.Wait();
                            var resultGetMockTestDataQnoMax = GetMockTestDataQnoMax.Result;
                            if (resultGetMockTestDataQnoMax.IsSuccessStatusCode)
                            {
                                var readTaskQnomax = resultGetMockTestDataQnoMax.Content.ReadAsStringAsync();
                                readTaskQnomax.Wait();
                                var mxqs = int.Parse(readTaskQnomax.Result);
                                ViewBag.tstmaxques = int.Parse(readTaskQnomax.Result);
                                if (qno > mxqs)
                                {
                                    ViewBag.qno = qno - 1;
                                }
                                else
                                {
                                    ViewBag.qno = qno;
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                return View();
                            }
                            //var mxqs = (from x in dbModel.Mock_Tests
                            //            where x.Course_Code == id1 && x.EnableDisable == true
                            //            select x.Ques_No).Max();
                            //ViewBag.tstmaxques = mxqs;
                            //var exmcdtostrng = examcode.ToString();
                            //var qtp = (from x in dbModel.Mock_Tests
                            //           where x.Course_Code == id1 && x.Exam_Code == exmcdtostrng && x.EnableDisable == true && x.Question_Type == "T"
                            //           select x.Question_Type).FirstOrDefault();
                            //if (qtp != null)
                            //{
                            //    ViewBag.qtp = "Y";
                            //}
                            //else
                            //{
                            //    ViewBag.qtp = "N";
                            //}

                            //ICollection<Mock_Test> mctt = dbModel.Mock_Tests.Where(x => x.Ques_No == qno && x.Course_Code == id1 && x.Exam_Code == tstdetails.Exam_Code).ToList();
                            //ViewBag.mctstlst = mctt;
                            ViewBag.id = secretcd.ToString();
                            ViewBag.uid = uid;
                            int intid = int.Parse(uid);
                            //ViewBag.name = (from a in dbModel.Tbl_Cand_Main
                            //                where a.Cand_id == intid
                            //                select a.CandName).FirstOrDefault();
                            //if (qno > mxqs)
                            //{
                            //    ViewBag.qno = qno - 1;
                            //}
                            //else
                            //{
                            //    ViewBag.qno = qno;
                            //}
                        }

                    }

                    else
                    {
                        var secretcd1 = (id.Substring(0, id.LastIndexOf("]") + 0));
                        var secretcd = int.Parse(secretcd1.Substring(0, secretcd1.LastIndexOf("^") + 0));
                        string uid = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
                        int qno = int.Parse((id.Substring(id.LastIndexOf("]") + 1)).ToString());
                        var exmscrtcdfrupdt = 0;
                        if ((id.LastIndexOf("`") + 1) != 0)
                        {
                            var exmscrtcdfrupdt12 = (id.LastIndexOf("`") + 1);
                            exmscrtcdfrupdt = int.Parse(exmscrtcdfrupdt12.ToString());
                        }


                        ViewBag.exmsecretcd = exmscrtcdfrupdt;
                        using (EOTAEntities dbModel = new EOTAEntities())
                        {
                            var GetCrsNameBySecretCD = client.GetAsync("http://localhost:25692/api/User/GetCrsNameBySecretCD?scrtcd=" + secretcd);
                            GetCrsNameBySecretCD.Wait();
                            var resultGetCrsNameBySecretCD = GetCrsNameBySecretCD.Result;
                            if (resultGetCrsNameBySecretCD.IsSuccessStatusCode)
                            {
                                var readTaskCrsname = resultGetCrsNameBySecretCD.Content.ReadAsStringAsync();
                                ViewBag.crsnm = readTaskCrsname.Result;
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                return View();
                            }
                            //ViewBag.crsnm = (from c in dbModel.Post_Courses
                            //                 where c.Secret_Code == secretcd
                            //                 select c.Title).FirstOrDefault();

                            var id1 = secretcd.ToString();
                            var GetMockTestData = client.GetAsync("http://localhost:25692/api/User/GetMockTestData?scrtcd=" + id1 + "&&Qnomax=" + "Null");
                            GetMockTestData.Wait();
                            var resultGetMockTestData = GetMockTestData.Result;
                            if (resultGetMockTestData.IsSuccessStatusCode)
                            {
                                var readTaskGetMockTestData = resultGetMockTestData.Content.ReadAsAsync<Mock_Test>();
                                var tstdetails = readTaskGetMockTestData.Result;
                                if (tstdetails != null)
                                {
                                    ViewBag.tstcd = tstdetails.Exam_Code;
                                    ViewBag.tsttm =int.Parse(tstdetails.Exam_Time);

                                    var GetMockTestDataByCrsCdExamCd = client.GetAsync("http://localhost:25692/api/User/GetMockTestDataByCrsCdExamCd?scrtcd=" + id1 + "&&Qnumber=" + qno + "&&Exmcd=" + tstdetails.Exam_Code + "&&Pssmrk=" + "Nok");
                                    GetMockTestDataByCrsCdExamCd.Wait();
                                    var resultGetMockTestDataByCrsCdExamCd = GetMockTestDataByCrsCdExamCd.Result;
                                    if (resultGetMockTestDataByCrsCdExamCd.IsSuccessStatusCode)
                                    {
                                        var readTaskgetdta = resultGetMockTestDataByCrsCdExamCd.Content.ReadAsAsync<IList<Mock_Test>>();
                                        readTaskgetdta.Wait();

                                        ViewBag.mctstlst = readTaskgetdta.Result;

                                    }
                                    else
                                    {
                                        ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                        return View();
                                    }
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                return View();
                            }
                            //var tstdetails = (from x in dbModel.Mock_Tests
                            //                  where x.Course_Code == id1 && x.EnableDisable == true
                            //                  select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault();
                            //ViewBag.tstcd = tstdetails.Exam_Code;
                            //ViewBag.tsttm = tstdetails.Exam_Time;
                            ViewBag.frst = "1";
                            ViewBag.rmtm = 60000;
                            var GetMockTestDataQnoMax = client.GetAsync("http://localhost:25692/api/User/GetMockTestData?scrtcd=" + id1 + "&&Qnomax=" + "OK");
                            GetMockTestDataQnoMax.Wait();
                            var resultGetMockTestDataQnoMax = GetMockTestDataQnoMax.Result;
                            if (resultGetMockTestDataQnoMax.IsSuccessStatusCode)
                            {
                                var readTaskQnomax = resultGetMockTestDataQnoMax.Content.ReadAsStringAsync();
                                readTaskQnomax.Wait();
                                var mxqs = int.Parse(readTaskQnomax.Result);
                                ViewBag.tstmaxques = int.Parse(readTaskQnomax.Result);
                                if (qno > mxqs)
                                {
                                    ViewBag.qno = qno - 1;
                                }
                                else
                                {
                                    ViewBag.qno = qno;
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                                return View();
                            }
                            //ViewBag.tstmaxques = (from x in dbModel.Mock_Tests
                            //                      where x.Course_Code == id1 && x.EnableDisable == true
                            //                      select x.Ques_No).Max();
                            //var qtp = (from x in dbModel.Mock_Tests
                            //           where x.Course_Code == id1 && x.Exam_Code == tstdetails.Exam_Code && x.EnableDisable == true && x.Question_Type == "T"
                            //           select x.Question_Type).FirstOrDefault();
                            //if (qtp != null)
                            //{
                            //    ViewBag.qtp = "Y";
                            //}
                            //else
                            //{
                            //    ViewBag.qtp = "N";
                            //}

                            //ICollection<Mock_Test> mctt = dbModel.Mock_Tests.Where(x => x.Ques_No == qno && x.Course_Code == id1 && x.Exam_Code == tstdetails.Exam_Code).ToList();
                            //ViewBag.mctstlst = mctt;
                            ViewBag.id = secretcd.ToString();
                            ViewBag.uid = uid;
                            int intid = int.Parse(uid);
                            ViewBag.qno = qno;
                            //ViewBag.name = (from a in dbModel.Tbl_Cand_Main
                            //                where a.Cand_id == intid
                            //                select a.CandName).FirstOrDefault();


                        }


                    }

                    return View();
                }
            }
            catch (Exception)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:25692/api/User");
                    var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                    postTaskUId.Wait();
                    var resultId = postTaskUId.Result;
                    if (resultId.IsSuccessStatusCode)
                    {
                        var readTaskId = resultId.Content.ReadAsStringAsync();
                        var userid = readTaskId.Result;
                        return RedirectToAction("Mocktst", new RouteValueDictionary(
                  new { controller = "Student", action = "Mocktst", Id = userid }));

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Something went wrong. please try again.");
                        return View();
                    }
                }
                //using (EOTAEntities dbModel = new EOTAEntities())
                //{
                //    var userid = (from x in dbModel.Tbl_Cand_Main
                //                  where x.Cand_EmailId == User.Identity.Name
                //                  select x.Cand_id).Distinct().FirstOrDefault();
                //    return RedirectToAction("Mocktst", new RouteValueDictionary(
                //   new { controller = "Student", action = "Mocktst", Id = userid }));
                //}
            }
        }
        //[HttpGet]
        //[Authorize]
        //public ActionResult Mockttststartpagefirst(string id)
        //{
        //    try
        //    {
        //        if (TempData["model"] != null)
        //        {
        //            int nuller = 0;
        //            object nuller5 = null;
        //            string nuller1 = null;
        //            var exmscrtcdfrupdt = nuller;
        //            var upadte = (id.Substring(id.LastIndexOf(")") + 1));
        //            var idfinal = nuller1;
        //            if (upadte == "up")
        //            {
        //                idfinal = (id.Substring(0, id.LastIndexOf(")") + 0));
        //                exmscrtcdfrupdt = int.Parse(idfinal.Substring(idfinal.LastIndexOf("`") + 1));
        //            }
        //            else
        //            {
        //                if (id.Contains("`"))
        //                {
        //                    exmscrtcdfrupdt = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
        //                }

        //            }
        //            ViewBag.exmsecretcd = exmscrtcdfrupdt;
        //            var examcode = nuller5;
        //            var secretcd = nuller;
        //            var uid = nuller1;
        //            var qno = nuller;
        //            var remaintime = nuller1;

        //            if (upadte == null)
        //            {
        //                var secretcd1 = (id.Substring(0, id.LastIndexOf("]") + 0));
        //                var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
        //                examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
        //                uid = (examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
        //                secretcd = int.Parse(secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
        //                string secretcode = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
        //                string cut = (id.Substring(id.LastIndexOf("]") + 1));

        //                qno = int.Parse(cut.Substring(0, cut.LastIndexOf("^") + 0));
        //                remaintime = (cut.Substring(cut.LastIndexOf("^") + 1));

        //            }
        //            else
        //            {
        //                var secretcd1 = (id.Substring(0, id.LastIndexOf("]") + 0));
        //                var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
        //                examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
        //                uid = (examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
        //                secretcd = int.Parse(secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
        //                string secretcode = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
        //                string cut = (id.Substring(id.LastIndexOf("]") + 1));

        //                qno = int.Parse(cut.Substring(0, cut.LastIndexOf("^") + 0));
        //                var remaintime1 = (cut.Substring(cut.LastIndexOf("^") + 1));

        //                if (upadte == "up")
        //                {
        //                    var exmscrtcdfrupdtrt = (remaintime1.Substring(remaintime1.LastIndexOf(")") + 1));
        //                    remaintime = (exmscrtcdfrupdtrt.Substring(0, exmscrtcdfrupdtrt.LastIndexOf("`") + 0));
        //                }
        //                else
        //                {
        //                    if (remaintime1.Contains("`"))
        //                    {
        //                        remaintime = (remaintime1.Substring(0, remaintime1.LastIndexOf("`") + 0));
        //                    }
        //                    else
        //                    {
        //                        remaintime = remaintime1;
        //                    }
        //                }
        //            }
        //            using (EOTAEntities dbModel = new EOTAEntities())
        //            {
        //                ViewBag.crsnm = (from c in dbModel.Post_Courses
        //                                 where c.Secret_Code == secretcd
        //                                 select c.Title).FirstOrDefault();

        //                var id1 = secretcd.ToString();
        //                var tstdetails = (from x in dbModel.Mock_Tests
        //                                  where x.Course_Code == id1 && x.EnableDisable == true
        //                                  select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault();
        //                ViewBag.tstcd = tstdetails.Exam_Code;
        //                ViewBag.tsttm = tstdetails.Exam_Time;
        //                ViewBag.frst = "2";
        //                ViewBag.rmtm = remaintime;
        //                var mxqs = (from x in dbModel.Mock_Tests
        //                            where x.Course_Code == id1 && x.EnableDisable == true
        //                            select x.Ques_No).Max();
        //                ViewBag.tstmaxques = mxqs;
        //                var exmcdtostrng = examcode.ToString();
        //                var qtp = (from x in dbModel.Mock_Tests
        //                           where x.Course_Code == id1 && x.Exam_Code == exmcdtostrng && x.EnableDisable == true && x.Question_Type == "T"
        //                           select x.Question_Type).FirstOrDefault();
        //                if (qtp != null)
        //                {
        //                    ViewBag.qtp = "Y";
        //                }
        //                else
        //                {
        //                    ViewBag.qtp = "N";
        //                }

        //                ICollection<Mock_Test> mctt = dbModel.Mock_Tests.Where(x => x.Ques_No == qno && x.Course_Code == id1 && x.Exam_Code == tstdetails.Exam_Code).ToList();
        //                ViewBag.mctstlst = mctt;
        //                ViewBag.id = secretcd.ToString();
        //                ViewBag.uid = uid;
        //                int intid = int.Parse(uid);
        //                ViewBag.name = (from a in dbModel.Tbl_Cand_Main
        //                                where a.Cand_id == intid
        //                                select a.CandName).FirstOrDefault();
        //                if (qno > mxqs)
        //                {
        //                    ViewBag.qno = qno - 1;
        //                }
        //                else
        //                {
        //                    ViewBag.qno = qno;
        //                }
        //            }

        //        }

        //        else
        //        {
        //            var secretcd1 = (id.Substring(0, id.LastIndexOf("]") + 0));
        //            var secretcd = int.Parse(secretcd1.Substring(0, secretcd1.LastIndexOf("^") + 0));
        //            string uid = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
        //            int qno = int.Parse((id.Substring(id.LastIndexOf("]") + 1)).ToString());
        //            var exmscrtcdfrupdt = 0;
        //            if ((id.LastIndexOf("`") + 1) != 0)
        //            {
        //                var exmscrtcdfrupdt12 = (id.LastIndexOf("`") + 1);
        //                exmscrtcdfrupdt = int.Parse(exmscrtcdfrupdt12.ToString());
        //            }


        //            ViewBag.exmsecretcd = exmscrtcdfrupdt;
        //            using (EOTAEntities dbModel = new EOTAEntities())
        //            {
        //                ViewBag.crsnm = (from c in dbModel.Post_Courses
        //                                 where c.Secret_Code == secretcd
        //                                 select c.Title).FirstOrDefault();

        //                var id1 = secretcd.ToString();
        //                var tstdetails = (from x in dbModel.Mock_Tests
        //                                  where x.Course_Code == id1 && x.EnableDisable == true
        //                                  select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault();
        //                ViewBag.tstcd = tstdetails.Exam_Code;
        //                ViewBag.tsttm = tstdetails.Exam_Time;
        //                ViewBag.frst = "1";
        //                ViewBag.rmtm = 60000;
        //                ViewBag.tstmaxques = (from x in dbModel.Mock_Tests
        //                                      where x.Course_Code == id1 && x.EnableDisable == true
        //                                      select x.Ques_No).Max();
        //                var qtp = (from x in dbModel.Mock_Tests
        //                           where x.Course_Code == id1 && x.Exam_Code == tstdetails.Exam_Code && x.EnableDisable == true && x.Question_Type == "T"
        //                           select x.Question_Type).FirstOrDefault();
        //                if (qtp != null)
        //                {
        //                    ViewBag.qtp = "Y";
        //                }
        //                else
        //                {
        //                    ViewBag.qtp = "N";
        //                }

        //                ICollection<Mock_Test> mctt = dbModel.Mock_Tests.Where(x => x.Ques_No == qno && x.Course_Code == id1 && x.Exam_Code == tstdetails.Exam_Code).ToList();
        //                ViewBag.mctstlst = mctt;
        //                ViewBag.id = secretcd.ToString();
        //                ViewBag.uid = uid;
        //                int intid = int.Parse(uid);
        //                ViewBag.qno = qno;
        //                ViewBag.name = (from a in dbModel.Tbl_Cand_Main
        //                                where a.Cand_id == intid
        //                                select a.CandName).FirstOrDefault();


        //            }


        //        }

        //        return View();

        //    }
        //    catch (Exception ex)
        //    {
        //        using (EOTAEntities dbModel = new EOTAEntities())
        //        {
        //            var userid = (from x in dbModel.Tbl_Cand_Main
        //                          where x.Cand_EmailId == User.Identity.Name
        //                          select x.Cand_id).Distinct().FirstOrDefault();
        //            return RedirectToAction("Mocktst", new RouteValueDictionary(
        //           new { controller = "Student", action = "Mocktst", Id = userid }));
        //        }
        //    }
        //}

        [HttpGet]
        [Authorize]
        public ActionResult tt(Use_Mock_Test tt)
        {
            return View();
        }

        [HttpPost]

        public ActionResult tstnext(string id, Mock_Test mctst, Use_Mock_Test umt)
        {
            try
            {
                var secretcd1 = (id.Substring(0, id.LastIndexOf("]") + 0));
                var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
                var examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
                var uid = (examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
                var secretcd = (secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
                //string secretcode = (secretcd1.Substring(secretcd1.LastIndexOf("^") + 1));
                string cut = (id.Substring(id.LastIndexOf("]") + 1));

                var qno = int.Parse(cut.Substring(0, cut.LastIndexOf("^") + 0));
                string remaintime = null;
                string remaintime1233 = null;
                if (qno - 1 == 1)
                {
                    remaintime = (cut.Substring(cut.LastIndexOf("^") + 1));
                }
                else
                {
                    remaintime1233 = (cut.Substring(cut.LastIndexOf("^") + 1));
                    remaintime = (remaintime1233.Substring(0, remaintime1233.LastIndexOf("`") + 0));
                }

                var chkqno = qno - 1;
                var exmscrtcd = 0;
                if (chkqno == 1)
                {

                }
                else
                {
                    exmscrtcd = int.Parse((remaintime1233.Substring(remaintime1233.LastIndexOf("`") + 1)).ToString());
                }

                var rndm = 0;

                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    ViewBag.crsnm = (from c in dbModel.Post_Courses
                                         //where c.Secret_Code == secretcd
                                     select c.Title).FirstOrDefault();

                    var id1 = examcode.ToString();
                    var tstdetails = (from x in dbModel.Mock_Tests
                                      where x.Course_Code == secretcd && x.EnableDisable == true
                                      select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault();
                    ViewBag.tstcd = tstdetails.Exam_Code;
                    ViewBag.tsttm = tstdetails.Exam_Time;
                    ViewBag.remtm = remaintime;
                    var maxqs = (from x in dbModel.Mock_Tests
                                 where x.Course_Code == secretcd && x.EnableDisable == true
                                 select x.Ques_No).Max();
                    ViewBag.tstmaxques = maxqs;
                    ViewBag.id = examcode.ToString();
                    var qnofinal = qno - 1;
                    ICollection<Mock_Test> mctt = dbModel.Mock_Tests.Where(x => x.Ques_No == qnofinal && x.Course_Code == secretcd && x.Exam_Code == tstdetails.Exam_Code).ToList();
                    ViewBag.mctstlst = mctt;
                    ViewBag.id = secretcd.ToString();

                    ViewBag.qno = (qno);
                    int chk1, chk2, chk3, chk4, chk5, chk6 = 0;
                    string chkfnl = null;
                    if (mctst.Checked == null && mctst.Textbx == null)
                    {

                        if (mctst.Optio1 == true)
                        {
                            chk1 = 1;
                            chkfnl = chk1.ToString();
                        }
                        if (mctst.Optio2 == true)
                        {
                            chk2 = 2;
                            if (chkfnl == null)
                            {
                                chkfnl = chk2.ToString();
                            }
                            else
                            {
                                chkfnl = chkfnl + "," + chk2.ToString();
                            }

                        }
                        if (mctst.Optio3 == true)
                        {
                            chk3 = 3;
                            if (chkfnl == null)
                            {
                                chkfnl = chk3.ToString();
                            }
                            else
                            {
                                chkfnl = chkfnl + "," + chk3.ToString();
                            }

                        }
                        if (mctst.Optio4 == true)
                        {
                            chk4 = 4;
                            if (chkfnl == null)
                            {
                                chkfnl = chk4.ToString();
                            }
                            else
                            {
                                chkfnl = chkfnl + "," + chk4.ToString();
                            }

                        }
                        if (mctst.Optio5 == true)
                        {
                            chk5 = 5;
                            if (chkfnl == null)
                            {
                                chkfnl = chk5.ToString();
                            }
                            else
                            {
                                chkfnl = chkfnl + "," + chk5.ToString();
                            }

                        }
                        if (mctst.Optio6 == true)
                        {
                            chk6 = 6;
                            if (chkfnl == null)
                            {
                                chkfnl = chk6.ToString();
                            }
                            else
                            {
                                chkfnl = chkfnl + "," + chk6.ToString();
                            }

                        }

                    }

                    object nuller = null;
                    var userop = nuller;
                    var userchk = nuller;
                    var usertxt = nuller;
                    if (mctst.Checked != null)
                    {
                        userop = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal/* && x.Currect_Option == mctst.Checked*/).FirstOrDefault();
                    }
                    else if (chkfnl != null)
                    {
                        userchk = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal /*&& x.Currect_Option == chkfnl*/).FirstOrDefault();
                    }
                    else if (mctst.Textbx != null)
                    {
                        usertxt = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal).FirstOrDefault();
                    }
                    else
                    {

                    }
                    if (userop == null && userchk == null && usertxt == null)
                    {
                        var aqwe = nuller;

                        if (qno > maxqs)
                        {
                            aqwe = (from x in dbModel.Use_Mock_Tests
                                    where x.Course_Code == secretcd && x.Exam_Code == examcode && x.Ques_No == qnofinal && x.Exam_Secretcode == rndm
                                    select x.Choosed_Option).Distinct().FirstOrDefault();

                        }
                        else
                        {
                            aqwe = (from x in dbModel.Use_Mock_Tests
                                    where x.Course_Code == secretcd && x.Exam_Code == examcode && x.Ques_No == qno && x.Exam_Secretcode == rndm
                                    select x.Choosed_Option).Distinct().FirstOrDefault();
                        }


                        if (aqwe != null)
                        {
                            if (qno > maxqs)
                            {
                                id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime + "`" + rndm + ")" + "up";
                                TempData["model"] = id;

                                return RedirectToAction("tstupdate", new RouteValueDictionary(
                    new { controller = "Student", action = "tstupdate", Id = id }));
                            }
                            else
                            {
                                id = uid + "^" + examcode + "!" + secretcd + "]" + qno + "^" + remaintime + "`" + rndm + ")" + "up";
                                TempData["model"] = id;

                                return RedirectToAction("tstupdate", new RouteValueDictionary(
                     new { controller = "Student", action = "tstupdate", Id = id }));
                            }
                        }
                        else
                        {

                            ViewBag.UploadStatus = "Error";
                            if (qno > maxqs)
                            {
                                id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime + "`" + rndm;
                            }
                            TempData["model"] = id;

                            return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
            new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
                        }
                    }

                    else
                    {


                        umt.User_Id = int.Parse(uid);
                        var uidfn = int.Parse(uid);
                        umt.User_Email = (from c in dbModel.Tbl_Cand_Main
                                          where c.Cand_id == uidfn
                                          select c.Cand_EmailId).FirstOrDefault();
                        umt.Exam_Code = examcode;
                        umt.Course_Code = secretcd;
                        umt.Ques_No = qnofinal;
                        if (chkfnl != null)
                        {
                            umt.Choosed_Option = chkfnl;
                        }
                        if (mctst.Checked != null)
                        {
                            umt.Choosed_Option = mctst.Checked;
                        }
                        if (mctst.Checked == null && chkfnl == null)
                        {
                            umt.Choosed_Option = mctst.Textbx;
                        }
                        var marks = nuller;

                        if (mctst.Checked != null)
                        {
                            marks = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal && x.Currect_Option == mctst.Checked).FirstOrDefault();
                            if (marks != null)
                            {
                                umt.Marks = 1;
                            }
                            else
                            {
                                umt.Marks = 0;
                            }
                        }
                        else if (chkfnl != null)
                        {
                            marks = dbModel.Mock_Tests.Where(x => x.Exam_Code == id1 && x.Course_Code == secretcd && x.Ques_No == qnofinal && x.Currect_Option == chkfnl).FirstOrDefault();
                            if (marks != null)
                            {
                                umt.Marks = 1;
                            }
                            else
                            {
                                umt.Marks = 0;
                            }
                        }

                        if (qnofinal == 1)
                        {
                            Random random = new Random();
                            umt.Exam_Secretcode = random.Next(1, 9999);

                        }
                        if (exmscrtcd == 0)
                        {
                            Random random = new Random();

                            rndm = random.Next(1, 9999);
                            umt.Exam_Secretcode = rndm;
                        }
                        else
                        {
                            if (qnofinal > 1)
                            {

                                umt.Exam_Secretcode = exmscrtcd;

                            }
                        }

                        umt.Exam_Date = DateTime.Now;

                        dbModel.Use_Mock_Tests.Add(umt);
                        dbModel.SaveChanges();

                    }

                    var ans = nuller;

                    if (qno > maxqs)
                    {
                        ans = (from x in dbModel.Use_Mock_Tests
                               where x.Course_Code == secretcd && x.Exam_Code == examcode && x.Ques_No == qnofinal && x.Exam_Secretcode == umt.Exam_Secretcode
                               select x.Choosed_Option).Distinct().FirstOrDefault();

                    }
                    else
                    {
                        ans = (from x in dbModel.Use_Mock_Tests
                               where x.Course_Code == secretcd && x.Exam_Code == examcode && x.Ques_No == qno && x.Exam_Secretcode == umt.Exam_Secretcode
                               select x.Choosed_Option).Distinct().FirstOrDefault();
                    }
                    var exsc = umt.Exam_Secretcode;

                    ViewBag.exmsecretcd = umt.Exam_Secretcode;
                    if (ans != null)
                    {
                        if (qno > maxqs)
                        {
                            id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime + "`" + exsc + ")" + "up";
                            TempData["model"] = id;

                            return RedirectToAction("tstupdate", new RouteValueDictionary(
                new { controller = "Student", action = "tstupdate", Id = id }));
                        }
                        else
                        {
                            id = uid + "^" + examcode + "!" + secretcd + "]" + qno + "^" + remaintime + "`" + exsc + ")" + "up";
                            TempData["model"] = id;

                            return RedirectToAction("tstupdate", new RouteValueDictionary(
                 new { controller = "Student", action = "tstupdate", Id = id }));
                        }
                    }
                    else
                    {
                        if (qno > maxqs)
                        {
                            id = uid + "^" + examcode + "!" + secretcd + "]" + qnofinal + "^" + remaintime + "`" + exsc;
                            TempData["model"] = id;

                            return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
                new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
                        }
                        else
                        {

                            TempData["model"] = id;
                            if (qnofinal == 1)
                            {
                                id = id + "`" + exsc;
                            }

                            return RedirectToAction("Mockttststartpagefirst", new RouteValueDictionary(
                new { controller = "Student", action = "Mockttststartpagefirst", Id = id }));
                        }
                    }


                }
            }
            catch (Exception ex)
            {

                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    var userid = (from x in dbModel.Tbl_Cand_Main
                                  where x.Cand_EmailId == User.Identity.Name
                                  select x.Cand_id).Distinct().FirstOrDefault();
                    return RedirectToAction("Mocktst", new RouteValueDictionary(
                   new { controller = "Student", action = "Mocktst", Id = userid }));
                }
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult Tstresult(string id, Use_Test_counter utc, string sccssid, string error, Feedback_overview fdbovw)
        {
            EOTAEntities db = new EOTAEntities();
            ViewBag.resultforGetNEETCourses = db.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault();

            try
            {
               
                var secretcd1 = (id.Substring(0, id.LastIndexOf("]") + 0));
                var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
                var examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
                var uid = int.Parse(examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
                var secretcd = (secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
                var exmscrtcd = (id.Substring(id.LastIndexOf("]") + 1));
                int exmsc = int.Parse(exmscrtcd.ToString());
                int total = 0;
                //using (EOTAEntities dbModel = new EOTAEntities())
                //{
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:25692/api/User");
                    var exmsecrtcd = int.Parse(exmscrtcd.ToString());
                    var GetUserTestTotal = client.GetAsync("http://localhost:25692/api/User/GetUserTestTotal?UserId="
                        + uid + "&&ExamCode=" + examcode + "&&CourseCode=" + secretcd + "&&ExamSecretcode=" + exmsecrtcd);
                    GetUserTestTotal.Wait();
                    var resultGetUserTestTotal = GetUserTestTotal.Result;
                    if (resultGetUserTestTotal.IsSuccessStatusCode)
                    {
                        var readTaskId = resultGetUserTestTotal.Content.ReadAsStringAsync();
                        total = int.Parse(readTaskId.Result);
                        ViewBag.totalmarks = total;
                    }

                    //var tstdta = (from c in dbModel.Use_Mock_Tests
                    //              where c.User_Id == uid && c.Course_Code == secretcd && c.Exam_Code == examcode && c.Exam_Secretcode == exmsc
                    //              select new { c.Marks }).ToList();
                    //foreach (var i in tstdta)
                    //{
                    //    if (i.Marks != null)
                    //    {
                    //        total = total + int.Parse((i.Marks).ToString());
                    //    }
                    //}

                    var GetMockTestDataByCrsCdExamCd = client.GetAsync("http://localhost:25692/api/User/GetMockTestDataByCrsCdExamCd?Exmcd=" + examcode + "&&scrtcd=" + secretcd + "&&Qnumber=" + -1 + "&&Pssmrk=" + "Nok");
                    GetMockTestDataByCrsCdExamCd.Wait();
                    var resultasGetMockTestDataByCrsCdExamCd = GetMockTestDataByCrsCdExamCd.Result;
                    var statuscode = (int)resultasGetMockTestDataByCrsCdExamCd.StatusCode;
                    if (resultasGetMockTestDataByCrsCdExamCd.IsSuccessStatusCode)
                    {
                        if (statuscode == 200)
                        {
                            ViewBag.qtp = "Y";
                        }
                        else
                        {
                            ViewBag.qtp = "N";
                        }

                    }
                    //    var qtp = (from x in dbModel.Mock_Tests
                    //           where x.Course_Code == secretcd && x.Exam_Code == examcode && x.EnableDisable == true && x.Question_Type == "T"
                    //           select x.Question_Type).FirstOrDefault();
                    //if (qtp != null)
                    //{
                    //    ViewBag.qtp = "Y";
                    //    ViewBag.totalmarks = total;
                    //}
                    //else
                    //{
                    //    ViewBag.qtp = "N";
                    //    ViewBag.totalmarks = total;
                    //}
                    var GetMockTestDataByCrsCdExamCdPssmrk = client.GetAsync("http://localhost:25692/api/User/GetMockTestDataByCrsCdExamCd?Exmcd=" + examcode + "&&scrtcd=" + secretcd + "&&Qnumber=" + -1 + "&&Pssmrk=" + "ok");
                    GetMockTestDataByCrsCdExamCdPssmrk.Wait();
                    var resultasGetMockTestDataByCrsCdExamCdPssmrk = GetMockTestDataByCrsCdExamCdPssmrk.Result;
                    statuscode = (int)resultasGetMockTestDataByCrsCdExamCdPssmrk.StatusCode;
                    if (resultasGetMockTestDataByCrsCdExamCdPssmrk.IsSuccessStatusCode)
                    {
                        var resultformrks = resultasGetMockTestDataByCrsCdExamCdPssmrk.Content.ReadAsStringAsync();
                        int? pssmrk = int.Parse(resultformrks.Result);
                        //pssmrk = (from x in dbModel.Mock_Tests
                        //             where x.Course_Code == secretcd && x.Exam_Code == examcode && x.EnableDisable == true
                        //             select x.Pass_mark).FirstOrDefault();
                        if (pssmrk > total)
                        {
                            ViewBag.passmrkmssg = "Sorry, you did not cross the pass marks. You have to cross " + pssmrk + " to get certificate.";
                        }
                        else
                        {
                            ViewBag.passmrkmssgsuccess = "Congrats, you cross the pass marks. You can generate certificate ";
                            var GetFeedback = client.GetAsync("http://localhost:25692/api/User/GetFeedback?UserEmail=" + User.Identity.Name + "&&secretcd=" + secretcd);
                            GetFeedback.Wait();
                            var resultasGetFeedback = GetMockTestDataByCrsCdExamCdPssmrk.Result;
                            if (resultasGetFeedback.IsSuccessStatusCode)
                            {
                                var resultchkfeedbck = resultasGetFeedback.Content.ReadAsStringAsync();
                                var chkfdbk = (resultchkfeedbck.Result).ToUpper();
                                if (chkfdbk == "TRUE")
                                {
                                    ViewBag.successid = "1";
                                }

                            }
                            //string chkfdbk = (from x in dbModel.Feedbacks
                            //                  where x.User_Email == User.Identity.Name && x.Course_Code == secretcd && x.IsActive == true
                            //                  select x.Id).FirstOrDefault().ToString();
                            //if (chkfdbk != Null)
                            //{

                            //}
                            //else
                            //{
                            //    ViewBag.successid = "1";
                            //}

                        }
                    }

                    ViewBag.uid = uid;

                    //ViewBag.name = (from a in dbModel.Tbl_Cand_Main
                    //                where a.Cand_id == uid
                    //                select a.CandName).FirstOrDefault();

                    var PostUserTestCount = client.PostAsJsonAsync("http://localhost:25692/api/User/PostUserTestCount?UserId="
                        + uid + "&&ExamCode=" + examcode + "&&CourseCode=" + secretcd + "&&ExamSecretcode=" + exmsecrtcd + "&&Marks=" + total, utc);
                    PostUserTestCount.Wait();
                    ViewBag.Course_Code = secretcd;
                    //utc.User_Id = uid;
                    //utc.Exam_Code = examcode;
                    //utc.Course_Code = secretcd;
                    //ViewBag.Course_Code = secretcd;
                    //utc.Exam_Secretcode = int.Parse(exmscrtcd.ToString());
                    //utc.Marks = total;
                    //utc.IsFinished = true;

                    //dbModel.Use_Test_counters.Add(utc);
                    //dbModel.SaveChanges();

                    var PostFeedbackOverview = client.PostAsJsonAsync("http://localhost:25692/api/User/PostFeedbackOverview?UserEmail=" + User.Identity.Name + "&&secretcd=" + secretcd, fdbovw);
                    PostFeedbackOverview.Wait();
                    var resultAsPostFeedbackOverview = PostFeedbackOverview.Result;
                    statuscode = (int)resultAsPostFeedbackOverview.StatusCode;
                    if (resultAsPostFeedbackOverview.IsSuccessStatusCode)
                    {
                        if (statuscode == 200)
                        {
                            ViewBag.ok = "1";
                            ViewBag.error = sccssid;
                        }
                        else
                        {
                            ViewBag.ok = "2";
                            ViewBag.error = error;
                            ViewBag.rsltid = id;
                        }

                    }
                }
                //EOTAEntities fdbck = new EOTAEntities();

                //fdbovw.Course_Code = secretcd;
                //fdbovw.User_Email = User.Identity.Name;
                //var chk = (from a in dbModel.Feedbacks
                //           where a.Course_Code == secretcd && a.User_Email == User.Identity.Name
                //           select a.Id).Any();
                //if (chk == true)
                //{
                //    ViewBag.ok = "1";
                //    ViewBag.error = sccssid;
                //    fdbovw.IsActive = true;
                //}
                //else
                //{
                //    ViewBag.ok = "2";
                //    ViewBag.error = error;
                //    ViewBag.rsltid = id;
                //    fdbovw.IsActive = false;

                //}
                //fdbck.Feedback_overviews.Add(fdbovw);
                //fdbck.SaveChanges();

                return View();
            //}
            }
            catch (Exception)
            {

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:25692/api/User");
                    var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                    postTaskUId.Wait();
                    var resultId = postTaskUId.Result;
                    if (resultId.IsSuccessStatusCode)
                    {
                        var readTaskId = resultId.Content.ReadAsStringAsync();
                        var userid = readTaskId.Result;
                        return RedirectToAction("Mocktst", new RouteValueDictionary(
                  new { controller = "Student", action = "Mocktst", Id = userid }));

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Something went wrong. please try again.");
                        return View();
                    }
                }

            }
        }
        //[HttpGet]
        //[Authorize]
        //public ActionResult Tstresult(string id, Use_Test_counter utc, string sccssid, string error, Feedback_overview fdbovw)
        //{
        //    try
        //    {
        //        var secretcd1 = (id.Substring(0, id.LastIndexOf("]") + 0));
        //        var examcode1 = (secretcd1.Substring(0, secretcd1.LastIndexOf("!") + 0));
        //        var examcode = (examcode1.Substring(examcode1.LastIndexOf("^") + 1));
        //        var uid = int.Parse(examcode1.Substring(0, examcode1.LastIndexOf("^") + 0));
        //        var secretcd = (secretcd1.Substring(secretcd1.LastIndexOf("!") + 1));
        //        var exmscrtcd = (id.Substring(id.LastIndexOf("]") + 1));
        //        int exmsc = int.Parse(exmscrtcd.ToString());
        //        decimal total = 0;
        //        using (EOTAEntities dbModel = new EOTAEntities())
        //        {
        //            var tstdta = (from c in dbModel.Use_Mock_Tests
        //                          where c.User_Id == uid && c.Course_Code == secretcd && c.Exam_Code == examcode && c.Exam_Secretcode == exmsc
        //                          select new { c.Marks }).ToList();
        //            foreach (var i in tstdta)
        //            {
        //                if (i.Marks != null)
        //                {
        //                    total = total + int.Parse((i.Marks).ToString());
        //                }
        //            }
        //            var qtp = (from x in dbModel.Mock_Tests
        //                       where x.Course_Code == secretcd && x.Exam_Code == examcode && x.EnableDisable == true && x.Question_Type == "T"
        //                       select x.Question_Type).FirstOrDefault();
        //            if (qtp != null)
        //            {
        //                ViewBag.qtp = "Y";
        //                ViewBag.totalmarks = total;
        //            }
        //            else
        //            {
        //                ViewBag.qtp = "N";
        //                ViewBag.totalmarks = total;
        //            }
        //            var pssmrk = (from x in dbModel.Mock_Tests
        //                          where x.Course_Code == secretcd && x.Exam_Code == examcode && x.EnableDisable == true
        //                          select x.Pass_mark).FirstOrDefault();
        //            if (pssmrk > total)
        //            {
        //                ViewBag.passmrkmssg = "Sorry, you did not cross the pass marks. You have to cross " + pssmrk + " to get certificate.";
        //            }
        //            else
        //            {
        //                ViewBag.passmrkmssgsuccess = "Congrats, you cross the pass marks. You can generate certificate ";
        //                string chkfdbk = (from x in dbModel.Feedbacks
        //                                  where x.User_Email == User.Identity.Name && x.Course_Code == secretcd && x.IsActive == true
        //                                  select x.Id).FirstOrDefault().ToString();
        //                if (chkfdbk != null)
        //                {

        //                }
        //                else
        //                {
        //                    ViewBag.successid = "1";
        //                }
        //            }
        //            ViewBag.uid = uid;

        //            ViewBag.name = (from a in dbModel.Tbl_Cand_Main
        //                            where a.Cand_id == uid
        //                            select a.CandName).FirstOrDefault();

        //            utc.User_Id = uid;
        //            utc.Exam_Code = examcode;
        //            utc.Course_Code = secretcd;
        //            ViewBag.Course_Code = secretcd;
        //            utc.Exam_Secretcode = int.Parse(exmscrtcd.ToString());
        //            utc.Marks = total;
        //            utc.IsFinished = true;

        //            dbModel.Use_Test_counters.Add(utc);
        //            dbModel.SaveChanges();
        //            EOTAEntities fdbck = new EOTAEntities();

        //                //fdbovw .Id= (from a in dbModel.Feedback_overviews
        //                //              where a.Course_Code == secretcd && a.User_Email == User.Identity.Name
        //                //              select a.Id).FirstOrDefault();

        //                //    fdbck.Feedback_overviews.Remove(fdbovw);
        //                //    fdbck.SaveChanges();
        //                fdbovw.Course_Code = secretcd;
        //                fdbovw.User_Email = User.Identity.Name;
        //                var chk = (from a in dbModel.Feedbacks
        //                           where a.Course_Code == secretcd && a.User_Email == User.Identity.Name
        //                           select a.Id).Any();
        //                if (chk == true)
        //                {
        //                    ViewBag.ok = "1";
        //                    ViewBag.error = sccssid;
        //                    fdbovw.IsActive = true;
        //                }
        //                else
        //                {
        //                    ViewBag.ok = "2";
        //                    ViewBag.error = error;
        //                    ViewBag.rsltid = id;
        //                    fdbovw.IsActive = false;

        //                }
        //                fdbck.Feedback_overviews.Add(fdbovw);
        //                fdbck.SaveChanges();

        //            return View();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        using (EOTAEntities dbModel = new EOTAEntities())
        //        {
        //            var userid = (from x in dbModel.Tbl_Cand_Main
        //                          where x.Cand_EmailId == User.Identity.Name
        //                          select x.Cand_id).Distinct().FirstOrDefault();
        //            return RedirectToAction("Mocktst", new RouteValueDictionary(
        //           new { controller = "Student", action = "Mocktst", Id = userid }));
        //        }
        //    }
        //}
        [Authorize]
        public ActionResult MocktstInstrction(string id)
        {
            var secretcd = int.Parse(id.Substring(0, id.LastIndexOf("]") + 0));
            var uid = int.Parse(id.Substring(id.LastIndexOf("]") + 1));
            var id1 = secretcd.ToString();
            ViewBag.id = secretcd;
            ViewBag.uid = uid;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                var GetCrsNameBySecretCD = client.GetAsync("http://localhost:25692/api/User/GetCrsNameBySecretCD?scrtcd=" + secretcd);
                GetCrsNameBySecretCD.Wait();

                var GetMockTestDataQnoMax = client.GetAsync("http://localhost:25692/api/User/GetMockTestData?scrtcd=" + id1 + "&&Qnomax=" + "OK");
                GetMockTestDataQnoMax.Wait();

                var GetMockTestData = client.GetAsync("http://localhost:25692/api/User/GetMockTestData?scrtcd=" + id1 + "&&Qnomax=" + "Null");
                GetMockTestData.Wait();


                var resultGetCrsNameBySecretCD = GetCrsNameBySecretCD.Result;
                var resultGetMockTestDataQnoMax = GetMockTestDataQnoMax.Result;
                var resultGetMockTestData = GetMockTestData.Result;

                if (resultGetCrsNameBySecretCD.IsSuccessStatusCode)
                {
                    var readTaskCrsname = resultGetCrsNameBySecretCD.Content.ReadAsStringAsync();
                    ViewBag.crsnm = readTaskCrsname.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                    return View();
                }
                if (resultGetMockTestDataQnoMax.IsSuccessStatusCode)
                {
                    var readTaskQnomax = resultGetMockTestDataQnoMax.Content.ReadAsStringAsync();
                    readTaskQnomax.Wait();
                    if (readTaskQnomax.Result == "null")
                    {
                        ModelState.AddModelError(string.Empty, "Test still not uploaded. We will update soon.");
                        return View();
                    }
                    else
                    {
                        ViewBag.tstmaxques = int.Parse(readTaskQnomax.Result);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                    return View();
                }

                if (resultGetMockTestData.IsSuccessStatusCode)
                {
                    var readTaskmckdata = resultGetMockTestData.Content.ReadAsAsync<Mock_Test>();
                    readTaskmckdata.Wait();
                    var mockdata = readTaskmckdata.Result;

                    if (mockdata != null)
                    {
                        ViewBag.tstcd = mockdata.Exam_Code;
                        ViewBag.tsttm = mockdata.Exam_Time;
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                    return View();
                }

            }
          

            TempData["model"] = null;
            return View();
            //    using (EOTAEntities dbModel = new EOTAEntities())
            //{
            //    var crsdetails = (from c in dbModel.Post_Courses
            //                      where c.Secret_Code == secretcd
            //                      select new { c.Title, c.Trainer_Id, c.Admin_Id }).FirstOrDefault();
            //    ViewBag.crsnm = crsdetails.Title;
            //    var id1 = secretcd.ToString();
            //    var tstdetails = (from x in dbModel.Mock_Tests
            //                      where x.Course_Code == id1 && x.EnableDisable == true
            //                      select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault();
            //    if (tstdetails != null)
            //    {
            //        ViewBag.tstcd = tstdetails.Exam_Code;
            //        ViewBag.tsttm = tstdetails.Exam_Time;
            //    }
            //    ViewBag.trnrnm = (from p in dbModel.Trainers
            //                      where p.Id == crsdetails.Trainer_Id
            //                      select p.Name).Distinct().FirstOrDefault();
            //    ViewBag.admnnm = (from q in dbModel.Tbl_ADMN
            //                      where q.Admin_id == crsdetails.Admin_Id
            //                      select q.AdminName).Distinct().FirstOrDefault();
            //    ViewBag.tstmaxques = (from x in dbModel.Mock_Tests
            //                          where x.Course_Code == id1 && x.EnableDisable == true
            //                          select x.Ques_No).Max();
            //    ViewBag.id = secretcd;
            //    ViewBag.uid = uid;
            //    ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                    where a.Cand_id == uid
            //                    select a.CandName).FirstOrDefault();

            //    TempData["model"] = null;
            //    return View();
            //}
        }

        [Authorize]
        [HttpGet]
        public ActionResult Mocktst(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                var getTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                getTaskUId.Wait();

                var getTaskpdcrsByid = client.GetAsync("http://localhost:25692/api/User/GetPaidCourseListByUserId?id=" + id);
                getTaskpdcrsByid.Wait();

                var getTaskMcktstById = client.GetAsync("http://localhost:25692/api/User/GetMockTestsByUserId?id=" + id);
                getTaskMcktstById.Wait();

                var getTaskCrslst = client.GetAsync("http://localhost:25692/api/User/GetCourseList");
                getTaskCrslst.Wait();
                EOTAEntities db = new EOTAEntities();
                ViewBag.resultforGetNEETCourses = db.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault();
                var resultId = getTaskUId.Result;
                var resultgetTaskpdcrsByid = getTaskpdcrsByid.Result;
                var resultgetTaskMcktstById = getTaskMcktstById.Result;
                var resultgetTaskCrslst = getTaskCrslst.Result;

                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    var idchk = int.Parse(readTaskId.Result);

                    if (idchk != id)
                    {
                        return RedirectToAction("AccessDeny", "Student");
                    }
                    else
                    {
                        ViewBag.uid = id;
                    }

                }

                if (resultgetTaskpdcrsByid.IsSuccessStatusCode)
                {
                    var readTaskpaidcrs = resultgetTaskpdcrsByid.Content.ReadAsAsync<IList<Paid_Cours>>();
                    readTaskpaidcrs.Wait();
                    ViewBag.pdcrs = readTaskpaidcrs.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                    return View();
                }

                if (resultgetTaskMcktstById.IsSuccessStatusCode)
                {
                    //var readTaskmck = resultgetTaskMcktstById.Content.ReadAsAsync<IList<Mock_Test>>();
                    var readTaskmck = resultgetTaskMcktstById.Content.ReadAsAsync<IList<string>>();
                    readTaskmck.Wait();
                    ViewBag.mctst = readTaskmck.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                    return View();
                }

                if (resultgetTaskCrslst.IsSuccessStatusCode)
                {
                    var readTaskresultgetTaskCrslst = resultgetTaskCrslst.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTaskresultgetTaskCrslst.Wait();
                    return View(readTaskresultgetTaskCrslst.Result);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                    return View();
                }
            }




            //using (EOTAEntities dbModel = new EOTAEntities())
            //{
            //    var idchk = (from a in dbModel.Tbl_Cand_Main
            //                 where a.Cand_EmailId == User.Identity.Name
            //                 select a.Cand_id).Distinct().FirstOrDefault();
            //    if (idchk != id)
            //    {
            //        return RedirectToAction("AccessDeny", "Student");
            //    }
            //    else
            //    {
            //        ViewBag.id = id;
            //        ViewBag.Tid = (from c in dbModel.Trainers

            //                       select new { c.Id, c.Name }).ToList();

            //        var crscd = (from p in dbModel.Post_Courses

            //                     select p.Secret_Code).Distinct().ToList();
            //        var usrcntr1 = (from p in dbModel.Mock_Tests
            //                        where p.IsMocktest == true
            //                        select p.Course_Code).Distinct().ToList();
            //        foreach (var i in usrcntr1)
            //        {
            //            var usrcntr = (from p in dbModel.Use_Test_counters
            //                           where p.Course_Code == i && p.User_Id == id
            //                           select p.Id).Distinct().Count();
            //            if (usrcntr == 3)
            //            {
            //                usrcntr1 = (from p in usrcntr1
            //                            where p != i
            //                            select p).Distinct().ToList();

            //            }
            //        }

            //        ViewBag.pdcrs = (from p in dbModel.Paid_Courses
            //                         where p.User_id == id
            //                         select p.Course_Code).Distinct().ToList();
            //        ViewBag.mctst = usrcntr1;

            //        ViewBag.pcrs = crscd;
            //        ViewBag.uid = id;
            //        ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                        where a.Cand_id == id
            //                        select a.CandName).FirstOrDefault();

            //        return View(dbModel.Post_Courses.ToList());
            //    }
            //}
        }

        [Authorize]
        [HttpGet]
        public ActionResult Certificates(string id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                postTaskUId.Wait();


                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + id);
                postTaskUname.Wait();

                var postTask = client.GetAsync("http://localhost:25692/api/User/GetUserResult");
                postTask.Wait();

                var resultId = postTaskUId.Result;
                var resultName = postTaskUname.Result;
                var result = postTask.Result;

                EOTAEntities db = new EOTAEntities();
                ViewBag.resultforGetNEETCourses = db.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault();

                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    var idchk = readTaskId.Result;
                    if (idchk != id)
                    {

                        return RedirectToAction("AccessDeny", "Student");
                    }
                    else
                    {
                        ViewBag.id = int.Parse(id);
                        ViewBag.uid = int.Parse(id);
                    }

                }
                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ///var readTaskName = resultName.Content.ReadAsAsync<Tbl_Cand_Main>();
                    ViewBag.name = readTaskName.Result;


                }
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<Paid_Courses_Certificates>>();
                    readTask.Wait();
                    return View(readTask.Result);


                }
                else
                {
                    return RedirectToAction("User_Profile");
                }

            }
        }

        [Authorize]
        public ActionResult AllCanSub(string id, string searchBy, string search)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                postTaskUId.Wait();


                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + id);
                postTaskUname.Wait();

                var postTask = client.GetAsync("http://localhost:25692/api/User/GetUserCrsAll?id=" + id + "&&searchBy=" + searchBy + "&&search=" + search);
                postTask.Wait();

                var resultId = postTaskUId.Result;
                var resultName = postTaskUname.Result;
                var result = postTask.Result;

                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    var idchk = readTaskId.Result;
                    if (idchk != id)
                    {

                        return RedirectToAction("AccessDeny", "Student");
                    }
                    else
                    {
                        ViewBag.id = id;
                    }

                }
                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ///var readTaskName = resultName.Content.ReadAsAsync<Tbl_Cand_Main>();
                    ViewBag.name = readTaskName.Result;


                }
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTask.Wait();
                    return View(readTask.Result);


                }
                else
                {
                    return RedirectToAction("User_Profile");
                }

            }

        }

        [Authorize]
        public ActionResult Allcrs(string id, string searchBy, string search)
        {
            var crstp = (id.Substring(0, id.LastIndexOf("`") + 0));
            var uid = (id.Substring(id.LastIndexOf("`") + 1));
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                postTaskUId.Wait();


                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + uid);
                postTaskUname.Wait();

                var postTask = client.GetAsync("http://localhost:25692/api/User/GetUserCrs?id=" + id + "&&searchBy=" + searchBy + "&&search=" + search);
                postTask.Wait();

                var resultId = postTaskUId.Result;
                var resultName = postTaskUname.Result;
                var result = postTask.Result;

                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    ViewBag.id = readTaskId.Result;
                    ViewBag.userid = readTaskId.Result;
                    // readTask.Wait();


                }
                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ViewBag.name = readTaskName.Result;


                }
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTask.Wait();
                    return View(readTask.Result);


                }
                else
                {
                    return RedirectToAction("User_Profile");
                }

            }



            //using (EOTAEntities dbModel = new EOTAEntities())
            //{
            //    ViewBag.userid = (from x in dbModel.Tbl_Cand_Main
            //                      where x.Cand_EmailId == User.Identity.Name
            //                      select x.Cand_id).Distinct().FirstOrDefault();
            //    ViewBag.id = uid;
            //    int nmid = int.Parse(uid);
            //    ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                    where a.Cand_id == nmid
            //                    select a.CandName).FirstOrDefault();
            //    if (searchBy == "Course")
            //    {
            //        return View(dbModel.Post_Courses.Where(x => x.Course_Type == crstp && x.Title.StartsWith(search) || x.Title == null).ToList());
            //    }
            //    else
            //    {
            //        return View(dbModel.Post_Courses.Where(x => x.Course_Type == crstp).ToList());
            //    }
            //}
        }

        [Authorize]
        public ActionResult CrsDetails(string id, string errorpage)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                //var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                //postTaskUId.Wait();


                //var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + id);
                //postTaskUname.Wait();

                var postTask = client.GetAsync("http://localhost:25692/api/User/GetCrsDetails?id=" + id);
                postTask.Wait();

                var postTaskpduser = client.GetAsync("http://localhost:25692/api/User/GetUserpfcrs?id=" + id);
                postTaskpduser.Wait();

                //var resultId = postTaskUId.Result;
                //var resultName = postTaskUname.Result;
                var result = postTask.Result;
                var resultpduser = postTaskpduser.Result;

                int id1 = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
                int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));

                ViewBag.secretcode = id1;
                ViewBag.userid = userid;
                if (resultpduser.IsSuccessStatusCode)
                {
                    var readTask = resultpduser.Content.ReadAsAsync<Paid_Cours>();
                    readTask.Wait();
                    ViewBag.pdcrschk = readTask.Result;
                }
                if (result.IsSuccessStatusCode)
                {
                    var readTask1 = result.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTask1.Wait();
                    return View(readTask1.Result);
                }
                else
                {
                    return RedirectToAction("User_Profile");
                }

            }
            //       using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            //       {
            //           ViewBag.ss = dbModel.Post_Courses.Distinct()
            //.Where(i => i.Course_Type == "T")
            //.ToArray();
            //           ViewBag.Techcourse = (from a in dbModel.Post_Courses
            //                                 where a.Course_Type == "T" && a.EnableDisable == true
            //                                 select a.Title).Distinct().ToList();

            //           ViewBag.Acacourse = (from a in dbModel.Post_Courses
            //                                where a.Course_Type == "A" && a.EnableDisable == true
            //                                select a.Title).Distinct().ToList();

            //           ViewBag.Mngcourse = (from a in dbModel.Post_Courses
            //                                where a.Course_Type == "M" && a.EnableDisable == true
            //                                select a.Title).Distinct().ToList();
            //           ViewBag.secretcode = id1;
            //           ViewBag.userid = userid;
            //           var admid = (from a in dbModel.Post_Courses
            //                        where a.Secret_Code == id1 && a.EnableDisable == true
            //                        select a.Admin_Id).Distinct().FirstOrDefault();
            //           ViewBag.admnm = (from a in dbModel.Tbl_ADMN
            //                            where a.Admin_id == admid
            //                            select a.AdminName).Distinct().FirstOrDefault();

            //           var trnrid = (from a in dbModel.Post_Courses
            //                         where a.Secret_Code == id1 && a.EnableDisable == true
            //                         select a.Trainer_Id).Distinct().FirstOrDefault();
            //           ViewBag.trnrnm = (from a in dbModel.Trainers
            //                             where a.Id == trnrid
            //                             select a.Name).Distinct().FirstOrDefault();

            //           ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                           where a.Cand_id == userid
            //                           select a.CandName).FirstOrDefault();
            //           ViewBag.pdcrschk = dbModel.Paid_Courses.Where(y => y.Course_Code == id1 && y.User_EmailId == User.Identity.Name && y.IsPaid == true).Distinct().FirstOrDefault();
            //           return View(dbModel.Post_Courses.Where(x => x.Secret_Code == id1 && x.EnableDisable == true).Distinct().ToList());
            //       }

        }


        [HttpPost]
        public ActionResult CrsDetails(string id, Post_Cours pc)
        {
            int id1 = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
            int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                postTaskUId.Wait();


                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + id);
                postTaskUname.Wait();

                var postTaskcrsdtils = client.GetAsync("http://localhost:25692/api/User/GetCrsDetails?id=" + id);
                postTaskcrsdtils.Wait();

                var postTaskcrsdscnt = client.PostAsJsonAsync("http://localhost:25692/api/User/PostCrsDiscount?id=" + id, pc);
                postTaskcrsdscnt.Wait();

                var resultId = postTaskUId.Result;
                var resultName = postTaskUname.Result;
                var resultcrsdtils = postTaskcrsdtils.Result;
                var resultcrsdscnt = postTaskcrsdscnt.Result;
                var statuscode = (int)resultcrsdscnt.StatusCode;
                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    ViewBag.id = readTaskId.Result;
                    ViewBag.userid = readTaskId.Result;
                    // readTask.Wait();

                }
                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ViewBag.name = readTaskName.Result;

                }


                if (resultcrsdtils.IsSuccessStatusCode)
                {
                    var readTaskcrsdtls = resultcrsdtils.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTaskcrsdtls.Wait();
                    var vw = readTaskcrsdtls.Result;


                    if (statuscode == 404)
                    {
                        ViewBag.error = "Sorry You input wrong entry. Discount is not Calculated. Please try again";

                    }
                    else if (statuscode == 406)
                    {
                        ViewBag.error = "Sorry your code is expired.";
                    }
                    else if (statuscode == 400)
                    {
                        ViewBag.error = "Something went wrong. Please try again.";
                    }

                    else
                    {
                        if (resultcrsdscnt.IsSuccessStatusCode)
                        {
                            var readTaskdscnt = resultcrsdscnt.Content.ReadAsStringAsync();
                            readTaskdscnt.Wait();
                            ViewBag.finalamount = decimal.Parse(readTaskdscnt.Result);
                            ViewBag.secretcode = id1;
                        }
                    }
                    //return View(vw);
                    EOTAEntities dbModel = new EOTAEntities();
                    //               ViewBag.ss = dbModel.Post_Courses.Distinct()
                    //.Where(i => i.Course_Type == "T")
                    //.ToArray();
                    //               ViewBag.Techcourse = (from a in dbModel.Post_Courses
                    //                                     where a.Course_Type == "T" && a.EnableDisable == true
                    //                                     select a.Title).Distinct().ToList();

                    //               ViewBag.Acacourse = (from a in dbModel.Post_Courses
                    //                                    where a.Course_Type == "A" && a.EnableDisable == true
                    //                                    select a.Title).Distinct().ToList();

                    //               ViewBag.Mngcourse = (from a in dbModel.Post_Courses
                    //                                    where a.Course_Type == "M" && a.EnableDisable == true
                    //                                    select a.Title).Distinct().ToList();
                    //               ViewBag.secretcode = id1;

                    //               var admid = (from a in dbModel.Post_Courses
                    //                            where a.Secret_Code == id1 && a.EnableDisable == true
                    //                            select a.Admin_Id).Distinct().FirstOrDefault();
                    //               ViewBag.admnm = (from a in dbModel.Tbl_ADMN
                    //                                where a.Admin_id == admid
                    //                                select a.AdminName).Distinct().FirstOrDefault();

                    //               var trnrid = (from a in dbModel.Post_Courses
                    //                             where a.Secret_Code == id1 && a.EnableDisable == true
                    //                             select a.Trainer_Id).Distinct().FirstOrDefault();
                    //               ViewBag.trnrnm = (from a in dbModel.Trainers
                    //                                 where a.Id == trnrid
                    //                                 select a.Name).Distinct().FirstOrDefault();

                    return View(dbModel.Post_Courses.Where(x => x.Secret_Code == id1 && x.EnableDisable == true).Distinct().ToList());
                }
                else
                {
                    return RedirectToAction("User_Profile");
                }
            }
            //       using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            //       {
            //           try
            //           {
            //               try
            //               {
            //                   var lp = Guid.Parse(pc.Learning_Point);
            //                   var refmodel = dbModel.Tbl_Cand_Main.Where(x => x.cand_ReferenceCode == lp);
            //                   if (refmodel == null)
            //                   {
            //                       ViewBag.error = "Sorry You input wrong entry. Discount is not Calculated. Please try again";
            //                   }
            //                   else if (refmodel != null)
            //                   {
            //                       var postprice = int.Parse((from a in dbModel.Post_Courses
            //                                                  where a.Secret_Code == id1
            //                                                  select a.New_Price).Distinct().FirstOrDefault());
            //                       var disper = int.Parse((from a in dbModel.Discount_Calculators
            //                                               where a.Discount_Code == pc.Learning_Point
            //                                               select a.Discount_Price).Distinct().FirstOrDefault());
            //                       var distm = (from a in dbModel.Discount_Calculators
            //                                    where a.Discount_Code == pc.Learning_Point
            //                                    select a.Disable_Date).Distinct().FirstOrDefault();

            //                       ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                                       where a.Cand_id == userid
            //                                       select a.CandName).FirstOrDefault();
            //                       if (distm != null)
            //                       {
            //                           if (distm < DateTime.Now)
            //                           {
            //                               ViewBag.error = "Sorry your code is expired.";
            //                           }
            //                           else
            //                           {
            //                               var amount = postprice * ((disper) / 100);
            //                               var finalamount = postprice - amount;
            //                               if (finalamount != 0)
            //                               {
            //                                   ViewBag.finalamount = finalamount;
            //                               }
            //                           }
            //                       }

            //                   }
            //               }
            //               catch
            //               {

            //                   var dismodel = dbModel.Discount_Calculators.Where(x => x.Discount_Code == pc.Learning_Point);
            //                   if (dismodel == null)
            //                   {
            //                       ViewBag.error = "Sorry You input wrong entry. Discount is not Calculated. Please try again";
            //                   }
            //                   else if (dismodel != null)
            //                   {
            //                       double postprice = int.Parse((from a in dbModel.Post_Courses
            //                                                     where a.Secret_Code == id1
            //                                                     select a.New_Price).Distinct().FirstOrDefault());
            //                       double disper = int.Parse((from a in dbModel.Discount_Calculators
            //                                                  where a.Discount_Code == pc.Learning_Point
            //                                                  select a.Discount_Price).Distinct().FirstOrDefault());
            //                       var distm = (from a in dbModel.Discount_Calculators
            //                                    where a.Discount_Code == pc.Learning_Point
            //                                    select a.Disable_Date).Distinct().FirstOrDefault();

            //                       ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                                       where a.Cand_id == userid
            //                                       select a.CandName).FirstOrDefault();
            //                       if (distm != null)
            //                       {
            //                           if (distm < DateTime.Now)
            //                           {
            //                               ViewBag.error = "Sorry your code is expired.";
            //                           }
            //                           else
            //                           {
            //                               double amount = postprice * ((disper) / 100);

            //                               double finalamount = postprice - amount;
            //                               if (finalamount != 0)
            //                               {
            //                                   ViewBag.finalamount = finalamount;
            //                               }
            //                           }
            //                       }


            //                   }

            //                   else
            //                   { }
            //               }
            //           }
            //           catch (Exception ex)
            //           {

            //               ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                               where a.Cand_id == userid
            //                               select a.CandName).FirstOrDefault();
            //               ViewBag.error = "Sorry You input wrong entry. Discount is not Calculated. Please try again";
            //           }

            //           ViewBag.ss = dbModel.Post_Courses.Distinct()
            //.Where(i => i.Course_Type == "T")
            //.ToArray();
            //           ViewBag.Techcourse = (from a in dbModel.Post_Courses
            //                                 where a.Course_Type == "T" && a.EnableDisable == true
            //                                 select a.Title).Distinct().ToList();

            //           ViewBag.Acacourse = (from a in dbModel.Post_Courses
            //                                where a.Course_Type == "A" && a.EnableDisable == true
            //                                select a.Title).Distinct().ToList();

            //           ViewBag.Mngcourse = (from a in dbModel.Post_Courses
            //                                where a.Course_Type == "M" && a.EnableDisable == true
            //                                select a.Title).Distinct().ToList();
            //           ViewBag.secretcode = id1;

            //           var admid = (from a in dbModel.Post_Courses
            //                        where a.Secret_Code == id1 && a.EnableDisable == true
            //                        select a.Admin_Id).Distinct().FirstOrDefault();
            //           ViewBag.admnm = (from a in dbModel.Tbl_ADMN
            //                            where a.Admin_id == admid
            //                            select a.AdminName).Distinct().FirstOrDefault();

            //           var trnrid = (from a in dbModel.Post_Courses
            //                         where a.Secret_Code == id1 && a.EnableDisable == true
            //                         select a.Trainer_Id).Distinct().FirstOrDefault();
            //           ViewBag.trnrnm = (from a in dbModel.Trainers
            //                             where a.Id == trnrid
            //                             select a.Name).Distinct().FirstOrDefault();

            //           return View(dbModel.Post_Courses.Where(x => x.Secret_Code == id1 && x.EnableDisable == true).Distinct().ToList());
            //       }

        }

        [Authorize]
        public ActionResult Help(Int32 id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                postTaskUId.Wait();


                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + id);
                postTaskUname.Wait();

                var postTask = client.GetAsync("http://localhost:25692/api/User/GetUserAsId?id=" + id);
                postTask.Wait();

                var resultId = postTaskUId.Result;
                var resultName = postTaskUname.Result;
                var result = postTask.Result;
                string idtotring = id.ToString();
                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    var idchk = readTaskId.Result;
                    if (idchk != idtotring)
                    {

                        return RedirectToAction("AccessDeny", "Student");
                    }
                    else
                    {
                        ViewBag.id = id;
                        ViewBag.uid = id;
                    }

                }
                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ///var readTaskName = resultName.Content.ReadAsAsync<Tbl_Cand_Main>();
                    ViewBag.name = readTaskName.Result;


                }
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<Tbl_Cand_Main>();
                    readTask.Wait();
                    return View(readTask.Result);


                }
                else
                {
                    return RedirectToAction("User_Profile");
                }

            }
        }

        [Authorize]
        public ActionResult Frndportal(Int32 id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                postTaskUId.Wait();


                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + id);
                postTaskUname.Wait();

                var postTask = client.GetAsync("http://localhost:25692/api/User/GetUserAsId?id=" + id);
                postTask.Wait();

                var resultId = postTaskUId.Result;
                var resultName = postTaskUname.Result;
                var result = postTask.Result;
                string idtotring = id.ToString();
                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    var idchk = readTaskId.Result;
                    if (idchk != idtotring)
                    {

                        return RedirectToAction("AccessDeny", "Student");
                    }
                    else
                    {
                        ViewBag.id = id;
                        ViewBag.uid = id;
                    }

                }
                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ///var readTaskName = resultName.Content.ReadAsAsync<Tbl_Cand_Main>();
                    ViewBag.name = readTaskName.Result;


                }
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<Tbl_Cand_Main>();
                    readTask.Wait();
                    return View(readTask.Result);


                }
                else
                {
                    return RedirectToAction("User_Profile");
                }

            }
        }
        [HttpPost]

        public ActionResult Frndportal(Int32 id, Tbl_Cand_Main customer)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                postTaskUId.Wait();


                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + id);
                postTaskUname.Wait();

                customer.Cand_id = id;
                var postTaskgetusr = client.GetAsync("http://localhost:25692/api/User/GetUserAsId?id=" + id);
                postTaskgetusr.Wait();
                var postTask = client.PostAsJsonAsync("http://localhost:25692/api/User/PostUserReferral?id=" + id, customer);
                postTask.Wait();

                var resultId = postTaskUId.Result;
                var resultName = postTaskUname.Result;
                var result = postTask.Result;
                var resultusr = postTaskgetusr.Result;
                var statuscd = (int)result.StatusCode;
                string idtotring = id.ToString();
                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    var idchk = readTaskId.Result;
                    if (idchk != idtotring)
                    {

                        return RedirectToAction("AccessDeny", "Student");
                    }
                    else
                    {
                        ViewBag.id = id;
                        ViewBag.uid = id;
                    }

                }
                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ///var readTaskName = resultName.Content.ReadAsAsync<Tbl_Cand_Main>();
                    ViewBag.name = readTaskName.Result;


                }
                if (resultusr.IsSuccessStatusCode)
                {
                    var readTaskuser = resultusr.Content.ReadAsAsync<Tbl_Cand_Main>();
                    readTaskuser.Wait();


                    if (statuscd == 204)
                    {
                        ViewBag.message = "Sorry! You can not proceed with blank Email Id";
                        return View(readTaskuser.Result);
                    }
                    else if (statuscd == 406)
                    {
                        ViewBag.message = "Sorry! You can not refer yourself.Try to proceed with another Email Id";
                        return View(readTaskuser.Result);
                    }
                    else if (statuscd == 400)
                    {
                        ViewBag.message = "Something went wrong! Please try again.";

                        ModelState.AddModelError("Id Error", "Invalid Credentials, Please check it.");

                        return View(readTaskuser.Result);
                    }
                    else if (statuscd == 203)
                    {
                        ViewBag.message = "Sorry! You are not authenticated for this service.";
                        return View(readTaskuser.Result);
                    }
                    else
                    {
                        if (result.IsSuccessStatusCode)
                        {
                            ViewBag.message1 = "Successfull, Email has been sent successfully sent with Reference Code.";

                            return View(readTaskuser.Result);

                        }
                        else
                        {
                            ViewBag.message = "Something went wrong. Please try again.";
                            return View(readTaskuser.Result);
                        }

                    }
                }
                else
                {
                    return RedirectToAction("User_Profile");
                }
            }

        }

        public ActionResult Semail5467verification(string id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                try
                {
                    var newaActivecd = Guid.Parse(id.Substring(0, id.LastIndexOf("-") + 0));
                    string cutting1 = id.Substring(id.LastIndexOf("-") + 1);
                    string cutting2 = cutting1.Substring(cutting1.LastIndexOf("c") + 1);

                    var newID = int.Parse(cutting2.Substring(0, cutting2.LastIndexOf("D") + 0));

                    if (dbModel.Tbl_Cand_Main.Any(x => x.Cand_id == newID && x.Cand_ActivationCode == newaActivecd))
                    {
                        ViewBag.activationmsg = "Congratulations. Your Account is Successfully Verified.";
                        var emlvrfd = (from c in dbModel.Tbl_Cand_Main
                                       where c.Cand_id == newID
                                       select c.Cand_IsEmailVerified).FirstOrDefault();

                        if (emlvrfd == false)
                        {
                            var objCourse = dbModel.Tbl_Cand_Main.Single(course => course.Cand_id == newID);

                            objCourse.Cand_IsEmailVerified = true;

                            dbModel.SaveChanges();
                        }
                        else
                        {
                            ViewBag.activationmsg = "Your Account Already verified. Thank you";
                        }
                    }

                    else
                    {
                        ViewBag.activationmsg = "Sorry, Something problem. Please click on link that you have sent.";
                    }
                    return View();
                }
                catch (Exception ex)
                {
                    ViewBag.activationmsg = "Sorry,This page is for Activation. Please click the link that already sennt To your mail or plaese register..";
                    return View();
                }
            }
        }

        [HttpGet]

        public ActionResult Semailpassrcvyverification(string id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                try
                {
                    var newaActivecd = Guid.Parse(id.Substring(0, id.LastIndexOf("-") + 0));
                    string cutting1 = id.Substring(id.LastIndexOf("-") + 1);
                    string cutting2 = cutting1.Substring(cutting1.LastIndexOf("c") + 1);

                    var newID = int.Parse(cutting2.Substring(0, cutting2.LastIndexOf("D") + 0));

                    return View(dbModel.Tbl_Cand_Main.Where(x => x.Cand_id == newID).FirstOrDefault());

                }
                catch (Exception ex)
                {
                    ViewBag.activationmsg = "Sorry,This page is for Activation. Please click the link that already sennt To your mail or plaese register..";

                    return RedirectToAction("Semail5467verification", "Student");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Semailpassrcvyverification(string id, Tbl_Cand_Main usermodels)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                try
                {
                    var newaActivecd = Guid.Parse(id.Substring(0, id.LastIndexOf("-") + 0));
                    string cutting1 = id.Substring(id.LastIndexOf("-") + 1);
                    string cutting2 = cutting1.Substring(cutting1.LastIndexOf("c") + 1);

                    var newID = int.Parse(cutting2.Substring(0, cutting2.LastIndexOf("D") + 0));

                    if (dbModel.Tbl_Cand_Main.Any(x => x.Cand_id == newID))
                    {

                        var emlvrfd = (from c in dbModel.Tbl_Cand_Main
                                       where c.Cand_id == newID
                                       select c.Cand_IsEmailVerified).FirstOrDefault();

                        if (emlvrfd == true)
                        {
                            if (dbModel.Tbl_Cand_Main.Any(x => x.Cand_id == newID && x.Cand_ActivationCode == newaActivecd))
                            {
                                if (usermodels.Cand_Pass == usermodels.Cand_RePass)
                                {
                                    if (usermodels.Cand_Pass.Length > 6)
                                    {
                                        var objCourse = dbModel.Tbl_Cand_Main.Single(course => course.Cand_id == newID && course.Cand_ActivationCode == newaActivecd);

                                        objCourse.Cand_Pass = usermodels.Cand_Pass;
                                        objCourse.Cand_RePass = usermodels.Cand_Pass;
                                        ViewBag.activationmsg = "Password has changed successfully. Thank you. ";

                                        dbModel.SaveChanges();
                                    }
                                    else
                                    {
                                        ViewBag.activationmsg = "New Password and Retype Password should be greater than 6 digits.Please try again.Thank you. ";
                                    }
                                }
                                else
                                {
                                    ViewBag.activationmsg = "New Password and Retype Password missmatch.Please try again.Thank you. ";
                                }
                            }
                            else
                            {
                                ViewBag.activationmsg = "Sorry! Something is going wrong. Try again. Thank you. ";
                            }
                        }
                        else
                        {
                            ViewBag.activationmsg = "Sorry! Something is going wrong. Please Click on link that is already sent in your registered email id. Thank you. ";
                        }
                    }

                    else
                    {
                        ViewBag.activationmsg = "Sorry, Something problem. Please click on link that you have sent.";
                    }
                    return RedirectToAction("Semail5467verification", "Student");
                }
                catch (Exception ex)
                {
                    ViewBag.activationmsg = "Sorry,This page is for Activation. Please click the link that already sennt To your mail or plaese register..";
                    return RedirectToAction("afterforgry", "Student");
                }
            }
        }
        [Authorize]
        public ActionResult afterforgry()
        {
            return View();
        }
        [Authorize]
        public ActionResult AccessDeny()
        {
            return View();
        }
        [Authorize]
        public ActionResult Update(string id)
        {
            if(User.Identity.Name== "SNL_users@gmail.com")
            {
                return RedirectToAction("User_Profile");
            }
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                postTaskUId.Wait();

                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + id);
                postTaskUname.Wait();

                var postTask = client.GetAsync("http://localhost:25692/api/User/GetUserById?Email=" + User.Identity.Name);
                postTask.Wait();

                var resultId = postTaskUId.Result;
                var resultName = postTaskUname.Result;
                var result = postTask.Result;

                EOTAEntities db = new EOTAEntities();
                ViewBag.resultforGetNEETCourses = db.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault();
                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    var idchk = readTaskId.Result;
                    if (idchk != id)
                    {
                        return RedirectToAction("AccessDeny", "Student");
                    }
                    else
                    {
                        ViewBag.id = idchk;
                    }


                }
                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ViewBag.name = readTaskName.Result;


                }
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<Tbl_Cand_Main>();
                    readTask.Wait();
                    return View(readTask.Result);


                }
                else
                {
                    return RedirectToAction("User_Profile");
                }
            }




        }

        [HttpPost]
        public ActionResult Update(Int32 id, Tbl_Cand_Main customer)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                string uid = id.ToString();
                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + uid);
                postTaskUname.Wait();

                var postTask = client.PutAsJsonAsync("http://localhost:25692/api/User/PutUserDetails?id=" + id, customer);
                postTask.Wait();
                var resultName = postTaskUname.Result;
                var result = postTask.Result;
                ViewBag.id = id;
                var statuscd = (int)result.StatusCode;
                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ViewBag.name = readTaskName.Result;
                }

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<Tbl_Cand_Main>();
                    readTask.Wait();
                    ViewBag.Duplicatemessage = "Account has been updated.";
                    ModelState.Clear();
                    return View(readTask.Result);

                }
                else if (statuscd == 304)
                {
                    ModelState.AddModelError("Id Error", "Invalid credentials. Please try again");
                    ViewBag.Duplicatemessage = "Invalid credentials. Please try again";
                    return View(customer);
                }
                else if (statuscd == 403)
                {
                    ViewBag.Duplicatemessage = "Existing email and password does not match.";
                    return View(customer);

                }
                else if (statuscd == 404)
                {
                    ModelState.AddModelError("EmailisnotExist", "Email does not Exist");
                    ViewBag.Duplicatemessage = "This email and password does not match.";
                    return View(customer);
                }
                else if (statuscd == 406)
                {
                    string Message = null;
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {

                            Message = error.ErrorMessage;
                            if (Message != null)
                            {

                                ViewBag.Duplicatemessage = Message;
                                return View(customer);
                            }

                        }
                    }
                    ViewBag.Duplicatemessage = Message;
                    return View(customer);
                }
                else
                {
                    return View(customer);
                }
            }

        }

        [Authorize]
        public ActionResult ChangePass(Int32 id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                ViewBag.id = id;
                ViewBag.name = (from a in dbModel.Tbl_Cand_Main
                                where a.Cand_id == id
                                select a.CandName).FirstOrDefault();

                return View(dbModel.Tbl_Cand_Main.Where(x => x.Cand_id == id).FirstOrDefault());
            }
        }

        [HttpPost]

        public ActionResult ChangePass(Int32 id, Tbl_Cand_Main userModel)
        {
            bool Status = false;
            string Message = "";
            ViewBag.id = id;

            using (EOTAEntities dbModel = new EOTAEntities())
            {
                if (dbModel.Tbl_Cand_Main.Any(x => x.Cand_id == id && x.Cand_Pass == userModel.Cand_Pass))
                {

                    userModel.Cand_Pass = userModel.Cand_ResetPassCode;

                    userModel.Cand_RePass = userModel.Cand_ResetPassCode;

                    userModel.Cand_ResetPassCode = null;

                    dbModel.Entry(userModel).State = EntityState.Modified;
                    dbModel.SaveChanges();

                    ViewBag.Duplicatemessagepass = "Password has been updated.";
                    ModelState.Clear();
                    return View(userModel);

                }
                else
                {
                    ViewBag.Duplicatemessagepass = "Password is not match";
                    return View(userModel);
                }
            }



        }
        [Authorize]
        public ActionResult chssub()
        {
            return View();
        }
        //public JsonResult States(string Country)

        [HttpPost]
        public ActionResult States(string Country)

        {
            if (Country == "india")
            {
                return View("Index", "Home");
            }
            else
            {
                return View("Index", "Home");
            }

        }

        [HttpPost]
        public ActionResult slct(string Country)
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
        [Authorize]
        public ActionResult Vid_CanAllView(string id)
        {
            var secrtcd = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
            int usrid = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
            ViewBag.secretcode = secrtcd;
            ViewBag.id = usrid;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var GetCrsBySecretCD = client.GetAsync("http://localhost:25692/api/User/GetCrsBySecretCD?id=" + secrtcd);
                GetCrsBySecretCD.Wait();

                var GetUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + usrid);
                GetUname.Wait();
                var getChkUserFeedback = client.GetAsync("http://localhost:25692/api/User/GetChkUserFeedback?email=" + User.Identity.Name);
                getChkUserFeedback.Wait();

                var resultGetCrsBySecretCD = GetCrsBySecretCD.Result;
                var resultGetUname = GetUname.Result;
                var resultgetChkUserFeedback = getChkUserFeedback.Result;

                if (resultgetChkUserFeedback.IsSuccessStatusCode)
                {
                    var readTaskFeedbackchk = resultgetChkUserFeedback.Content.ReadAsStringAsync();

                    if (readTaskFeedbackchk.Result == "true")
                    {
                        ViewBag.succ = "1";
                    }
                    else if (readTaskFeedbackchk.Result == "false")
                    {
                        ViewBag.succ = "2";
                    }
                    else
                    {
                        ViewBag.succ = "2";
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again later.");
                }
                if (resultGetUname.IsSuccessStatusCode)
                {
                    var readTaskUName = resultGetUname.Content.ReadAsStringAsync();
                    ViewBag.name = readTaskUName.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again later.");
                }
                if (resultGetCrsBySecretCD.IsSuccessStatusCode)
                {
                    var readTaskCrsByscrtcd = resultGetCrsBySecretCD.Content.ReadAsAsync<IList<Post_Cours>>();
                    return View(readTaskCrsByscrtcd.Result);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again later.");
                    return View();
                }
            }
            //    using (EOTAEntities dbModel = new EOTAEntities())
            //{
            //    string scrt = secrtcd.ToString();
            //    var chk = (from a in dbModel.Feedback_overviews
            //               where a.User_Email == User.Identity.Name && a.IsActive == false
            //               select a).Any();
            //    if (chk == true)
            //    {
            //        ViewBag.succ = "1";
            //    }
            //    else if (chk == false)
            //    {
            //        ViewBag.succ = "2";
            //    }
            //    else
            //    {
            //        ViewBag.succ = "2";
            //    }
            //    ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                    where a.Cand_id == usrid
            //                    select a.CandName).FirstOrDefault();

            //    ViewBag.secretcode = secrtcd;
            //    ViewBag.id = usrid;
            //    return View(dbModel.Post_Courses.Where(s => s.Secret_Code == secrtcd).ToList());
            //}
        }
        [Authorize]
        public ActionResult CourseView(string id)
        {
            var secrtcd = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
            int usrid = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
            ViewBag.secretcode = secrtcd;
            ViewBag.id = usrid;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var GetCrsBySecretCD = client.GetAsync("http://localhost:25692/api/User/GetCrsBySecretCD?id=" + secrtcd);
                GetCrsBySecretCD.Wait();

                var GetUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + usrid);
                GetUname.Wait();
                var getChkUserFeedback = client.GetAsync("http://localhost:25692/api/User/GetChkUserFeedback?email=" + User.Identity.Name);
                getChkUserFeedback.Wait();

                var resultGetCrsBySecretCD = GetCrsBySecretCD.Result;
                var resultGetUname = GetUname.Result;
                var resultgetChkUserFeedback = getChkUserFeedback.Result;

                if (resultgetChkUserFeedback.IsSuccessStatusCode)
                {
                    var readTaskFeedbackchk = resultgetChkUserFeedback.Content.ReadAsStringAsync();

                    if (readTaskFeedbackchk.Result == "true")
                    {
                        ViewBag.succ = "1";
                    }
                    else if (readTaskFeedbackchk.Result == "false")
                    {
                        ViewBag.succ = "2";
                    }
                    else
                    {
                        ViewBag.succ = "2";
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again later.");
                }
                if (resultGetUname.IsSuccessStatusCode)
                {
                    var readTaskUName = resultGetUname.Content.ReadAsStringAsync();
                    ViewBag.name = readTaskUName.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again later.");
                }
                if (resultGetCrsBySecretCD.IsSuccessStatusCode)
                {
                    var readTaskCrsByscrtcd = resultGetCrsBySecretCD.Content.ReadAsAsync<IList<Post_Cours>>();
                    //return View(readTaskCrsByscrtcd.Result);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again later.");
                    //return View();
                }
            }
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var GetNEETCourses = client.GetAsync("http://localhost:25692/api/Home/GetNEETCoursesDetails");
                GetNEETCourses.Wait();

                int scrtcd = secrtcd;
                var GetCrsByCode = client.GetAsync("http://localhost:25692/api/Home/GetCrsByCode?id=" + scrtcd);
                GetCrsByCode.Wait();

                var resultforGetNEETCourses = GetNEETCourses.Result;

                var resultforGetCrsByCode = GetCrsByCode.Result;
                List<Post_Cours> postcrsall = new List<Post_Cours>();
                if (resultforGetCrsByCode.IsSuccessStatusCode)
                {
                    var readTaskcrsbyId = resultforGetCrsByCode.Content.ReadAsAsync<List<Post_Cours>>();
                    readTaskcrsbyId.Wait();
                    ViewBag.selectcrs = readTaskcrsbyId.Result;
                    postcrsall = readTaskcrsbyId.Result;
                    ViewBag.newprice = postcrsall.Select(z => z.New_Price).Distinct().FirstOrDefault();
                    ViewBag.firstdemovideo = postcrsall.Select(z => z.Demo_video_Path).Distinct().FirstOrDefault();
                    ViewBag.secretcd = postcrsall.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                EOTAEntities db = new EOTAEntities();
                ViewBag.userid = db.Tbl_Cand_Main.Where(a => a.Cand_EmailId == User.Identity.Name).Select(z => z.Cand_id).Distinct().FirstOrDefault();
                if (resultforGetNEETCourses.IsSuccessStatusCode)
                {
                    var readTaskNEET = resultforGetNEETCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTaskNEET.Wait();
                    ViewBag.resultforGetNEETCourses = readTaskNEET.Result;

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                return View(postcrsall);

            }

        }
        [Authorize]
        public ActionResult paidcourse(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var postTaskUname = client.GetAsync("http://localhost:25692/api/User/GetUserName?uid=" + id);
                postTaskUname.Wait();
                var postTaskUId = client.GetAsync("http://localhost:25692/api/User/GetUserId?crrntemail=" + User.Identity.Name);
                postTaskUId.Wait();
                var GetCourseList = client.GetAsync("http://localhost:25692/api/User/GetCourseList");
                GetCourseList.Wait();
                var GetPaidCourseListByUserId = client.GetAsync("http://localhost:25692/api/User/GetPaidCourseListByUserId?id=" + id);
                GetPaidCourseListByUserId.Wait();

                var resultName = postTaskUname.Result;
                var resultId = postTaskUId.Result;
                var resultGetCourseList = GetCourseList.Result;
                var resultGetPaidCourseListByUserId = GetPaidCourseListByUserId.Result;
                EOTAEntities db = new EOTAEntities();
                ViewBag.resultforGetNEETCourses = db.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault();
                if (resultId.IsSuccessStatusCode)
                {
                    var readTaskId = resultId.Content.ReadAsStringAsync();
                    var idchk = int.Parse(readTaskId.Result);
                    if (idchk != id)
                    {

                        return RedirectToAction("AccessDeny", "Student");
                    }
                    else
                    {
                        ViewBag.id = id;

                    }

                }


                if (resultName.IsSuccessStatusCode)
                {
                    var readTaskName = resultName.Content.ReadAsStringAsync();
                    ///var readTaskName = resultName.Content.ReadAsAsync<Tbl_Cand_Main>();
                    ViewBag.name = readTaskName.Result;
                }

                if (resultGetPaidCourseListByUserId.IsSuccessStatusCode)
                {
                    var readTaskpaidcrsusers = resultGetPaidCourseListByUserId.Content.ReadAsAsync<IList<Paid_Cours>>();
                    //var readTaskpaidcrsusers = resultGetPaidCourseListByUserId.Content.ReadAsStringAsync();
                    readTaskpaidcrsusers.Wait();
                    ViewBag.courselist = readTaskpaidcrsusers.Result;
                    ViewBag.secretcdforNEET = readTaskpaidcrsusers.Result.Where(z=>z.Course_Name.ToUpper()=="NEET").Select(z=>z.Course_Code).FirstOrDefault();

                }
                if (resultGetCourseList.IsSuccessStatusCode)
                {
                    var readTaskCrsList = resultGetCourseList.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTaskCrsList.Wait();
                    return View(readTaskCrsList.Result);
                }
                else
                {
                    return RedirectToAction("User_Profile");
                }
            }


            //    using (EOTAEntities dbModel = new EOTAEntities())
            //{
            //    var idchk = (from a in dbModel.Tbl_Cand_Main
            //                 where a.Cand_EmailId == User.Identity.Name
            //                 select a.Cand_id).Distinct().FirstOrDefault();
            //    if (idchk != id)
            //    {

            //        return RedirectToAction("AccessDeny", "Student");
            //    }
            //    else
            //    {
            //        ViewBag.id = id;
            //        ViewBag.name = (from a in dbModel.Tbl_Cand_Main
            //                        where a.Cand_id == id
            //                        select a.CandName).FirstOrDefault();

            //        ViewBag.courselist = (from x in dbModel.Paid_Courses
            //                              where x.User_id == id
            //                              select x.Course_Code).Distinct().ToList();

            //        return View(dbModel.Post_Courses.ToList());
            //    }
            //}
        }

        [Authorize]
        public ActionResult Pdcrs()
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {

                return View(dbModel.Post_Courses.ToList());
            }
        }

        [Authorize]
        public ActionResult singlecrs(string id)
        {
            
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var GetNEETCourses = client.GetAsync("http://localhost:25692/api/Home/GetNEETCoursesDetails");
                GetNEETCourses.Wait();
             
                int scrtcd = int.Parse(id);
                var GetCrsByCode = client.GetAsync("http://localhost:25692/api/Home/GetCrsByCode?id=" + scrtcd);
                GetCrsByCode.Wait();

                var resultforGetNEETCourses = GetNEETCourses.Result;
              
                var resultforGetCrsByCode = GetCrsByCode.Result;
                List<Post_Cours> postcrsall = new List<Post_Cours>();
                if (resultforGetCrsByCode.IsSuccessStatusCode)
                {
                    var readTaskcrsbyId = resultforGetCrsByCode.Content.ReadAsAsync<List<Post_Cours>>();
                    readTaskcrsbyId.Wait();
                    ViewBag.selectcrs = readTaskcrsbyId.Result;
                     postcrsall = readTaskcrsbyId.Result;
                    ViewBag.newprice = postcrsall.Select(z => z.New_Price).Distinct().FirstOrDefault();
                    ViewBag.firstdemovideo = postcrsall.Select(z => z.Demo_video_Path).Distinct().FirstOrDefault();
                    ViewBag.secretcd = int.Parse(postcrsall.Where(x=>x.Course_name.ToUpper()=="NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault().ToString());
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                EOTAEntities db = new EOTAEntities();
                ViewBag.userid = db.Tbl_Cand_Main.Where(a=>a.Cand_EmailId==User.Identity.Name).Select(z=>z.Cand_id).Distinct().FirstOrDefault();
                if (resultforGetNEETCourses.IsSuccessStatusCode)
                {
                    var readTaskNEET = resultforGetNEETCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTaskNEET.Wait();
                    ViewBag.resultforGetNEETCourses = readTaskNEET.Result;

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
               
                    ViewBag.trainerinfo = db.Courses_Trainers.Where(z => z.Course_code == scrtcd).Distinct().FirstOrDefault();
               

                return View(postcrsall);

            }
        }

        [HttpPost]
        public ActionResult singlecrs(string id, Post_Cours pc)
        {

           
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri("http://localhost:25692/api/User");

                    var postTaskcrsdscnt = client.PostAsJsonAsync("http://localhost:25692/api/User/PostCrsDiscount?id=" + id, pc);
                    postTaskcrsdscnt.Wait();


                    var resultcrsdscnt = postTaskcrsdscnt.Result;
                    var statuscode = (int)resultcrsdscnt.StatusCode;
                    var GetNEETCourses = client.GetAsync("http://localhost:25692/api/Home/GetNEETCoursesDetails");
                    GetNEETCourses.Wait();

                    int scrtcd = int.Parse(id);
                    var GetCrsByCode = client.GetAsync("http://localhost:25692/api/Home/GetCrsByCode?id=" + scrtcd);
                    GetCrsByCode.Wait();

                    var resultforGetNEETCourses = GetNEETCourses.Result;

                    var resultforGetCrsByCode = GetCrsByCode.Result;
                    List<Post_Cours> postcrsall = new List<Post_Cours>();

                    using (EOTAEntities db1 = new EOTAEntities())
                    {
                        ViewBag.trainerinfo = db1.Courses_Trainers.Where(z => z.Course_code == scrtcd).Distinct().FirstOrDefault();
                    }


                    if (statuscode == 404)
                    {
                        ViewBag.error = "Sorry You input wrong entry. Discount is not Calculated. Please try again";

                    }
                    else if (statuscode == 406)
                    {
                        ViewBag.error = "Sorry your code is expired.";
                    }
                    else if (statuscode == 400)
                    {
                        ViewBag.error = "Something went wrong. Please try again.";
                    }

                    else
                    {
                        if (resultcrsdscnt.IsSuccessStatusCode)
                        {
                            var readTaskdscnt = resultcrsdscnt.Content.ReadAsStringAsync();
                            readTaskdscnt.Wait();
                            ViewBag.finalamount = decimal.Parse(readTaskdscnt.Result);
                            
                        }
                    }
                    if (resultforGetCrsByCode.IsSuccessStatusCode)
                    {
                        var readTaskcrsbyId = resultforGetCrsByCode.Content.ReadAsAsync<List<Post_Cours>>();
                        readTaskcrsbyId.Wait();
                        ViewBag.selectcrs = readTaskcrsbyId.Result;
                        postcrsall = readTaskcrsbyId.Result;
                        ViewBag.newprice = postcrsall.Select(z => z.New_Price).Distinct().FirstOrDefault();
                        ViewBag.firstdemovideo = postcrsall.Select(z => z.Demo_video_Path).Distinct().FirstOrDefault();
                        ViewBag.secretcd = scrtcd;
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                    }
                    EOTAEntities db = new EOTAEntities();
                    ViewBag.userid = db.Tbl_Cand_Main.Where(a => a.Cand_EmailId == User.Identity.Name).Select(z => z.Cand_id).Distinct().FirstOrDefault();
                    if (resultforGetNEETCourses.IsSuccessStatusCode)
                    {
                        var readTaskNEET = resultforGetNEETCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                        readTaskNEET.Wait();
                        ViewBag.resultforGetNEETCourses = readTaskNEET.Result;

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                    }
                    return View(postcrsall);
                }
                catch
                {
                    return RedirectToAction("User_Profile");
                }
            }
     

        }

        [Authorize]
        public ActionResult User_Profile()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
          
                var GetNEETCourses = client.GetAsync("http://localhost:25692/api/Home/GetNEETCourses");
                GetNEETCourses.Wait();
              
              
                var resultforGetNEETCourses = GetNEETCourses.Result;
              
                if (resultforGetNEETCourses.IsSuccessStatusCode)
                {
                    var readTaskNEET = resultforGetNEETCourses.Content.ReadAsStringAsync();
                    readTaskNEET.Wait();
                    ViewBag.resultforGetNEETCourses = readTaskNEET.Result;
                    EOTAEntities db = new EOTAEntities();
                    ViewBag.firstdemovideo = db.Post_Courses.Where(z => z.Course_name.ToUpper() == "NEET").Select(z => z.Demo_video_Path).Distinct().FirstOrDefault();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                
            }
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                ViewBag.name = (from a in dbModel.Tbl_Cand_Main
                                where a.Cand_EmailId == User.Identity.Name
                                select a.CandName).FirstOrDefault();
                ViewBag.institute = (from a in dbModel.Institute_Infoes

                                     select a.Id).Distinct().Count();
                ViewBag.trainer = (from a in dbModel.Trainers

                                   select a.Id).Distinct().Count();

                return View(dbModel.Tbl_Cand_Main.Where(x => x.Cand_EmailId == User.Identity.Name).FirstOrDefault());
            }

        }

        [Authorize]
        public ActionResult AllView()
        {

            using (EOTAEntities dbModel = new EOTAEntities())
            {
                return View(dbModel.Tbl_Doc.ToList());
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult Details(int id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                return View(dbModel.Tbl_Doc.Where(x => x.Doc_id == id).FirstOrDefault());
            }

        }
        [Authorize]
        public ActionResult Su_login()
        {
            return View();
        }

        public ActionResult Cand_Login()
        {
            return View();
        }



        [HttpPost]
        public ActionResult Cand_Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]

        public ActionResult Mailsender(string email)
        {

            string Message = "Thank you for your subscription.";
            return RedirectToAction("Index", "Home");

        }
        [HttpGet]
        [Authorize]
        public ActionResult Cand_signin(Cand_login userModel)
        {
            return View();

        }

        [HttpPost]

        //[ValidateAntiForgeryToken]
        public ActionResult Cand_Login(Tbl_Cand_Main cand, string ReturnUrl)
        {
         
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");

                //HTTP POST
                var postTask = client.PostAsJsonAsync("http://localhost:25692/api/User/PostUserLogin", cand);
                postTask.Wait();

                var result = postTask.Result;
                int ss = (int)result.StatusCode;
                if (ss == 401)
                {
                    ViewBag.activationmsg = "Invalid credentials, Please Check your details and try again";

                    return RedirectToAction("Index", "Home", new { id = "1" });
                }
                else if (ss == 302)
                {
                    ViewBag.activationmsg = "Your Account till now not verified. Please verify your email id. Link is sent to your registered email id.";

                    return View(cand);
                }
                else if (result.IsSuccessStatusCode)
                {
                    if (ss == 200)
                    {
                        int timeout =/*cand.rememberMe ?*/ 60;
                        cand.rememberMe = true;
                        var ticket = new FormsAuthenticationTicket(cand.Cand_EmailId, cand.rememberMe, timeout);
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
                            var s = User.Identity.Name;
                            return RedirectToAction("User_Profile", "Student");

                        }
                    }

                }

                else if (ss == 406)
                {
                    string Message = null;
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {

                            Message = error.ErrorMessage;
                            if (Message != null)
                            {


                                return RedirectToAction("Index", "Home", new { id = "12]" + Message });
                            }

                        }
                    }
                    return RedirectToAction("Index", "Home", new { id = "12]" + Message });

                }
                else if (ss == 400)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("Index", "Home");

        }

        public ActionResult Error()
        {
            return View();
        }
        [HttpPost]

        public ActionResult Cand_Forgotpass(Tbl_Cand_Main customer)
        {
            string Message = null;
            var state = ViewData.ModelState.FirstOrDefault(x => x.Key.Equals("Cand_EmailId"));

            if (state.Value != null &&
                state.Value.Errors.Count == 0)
            {
                //foreach (ModelState modelState in ViewData.ModelState.values)
                //{
                //    foreach (ModelError error in modelState.Errors)
                //    {

                //        Message = error.ErrorMessage;
                //        if (Message == null)
                //        {

                //        }
                //    }
                //}

                var isExist = IsEmailExist(customer.Cand_EmailId);
                if (isExist)
                {
                    using (EOTAEntities dbModel = new EOTAEntities())
                    {
                        try
                        {
                            var c_id = (from c in dbModel.Tbl_Cand_Main
                                        where c.Cand_EmailId == customer.Cand_EmailId
                                        select c.Cand_id).FirstOrDefault();

                            var objCourse = dbModel.Tbl_Cand_Main.Single(course => course.Cand_id == c_id && course.Cand_EmailId == customer.Cand_EmailId);

                            objCourse.Cand_ActivationCode = Guid.NewGuid();
                            dbModel.SaveChanges();

                            customer.Cand_ActivationCode = objCourse.Cand_ActivationCode;
                            SendfrgtpsslinkEmail(customer.Cand_EmailId, (customer.Cand_ActivationCode).ToString());
                            ViewBag.activationmsg = "Please check Your Email. Password recovery link has been sent to you.";

                            return RedirectToAction("Index", "Home", new { id = "5" });
                        }

                        catch (Exception ex)
                        {

                            return RedirectToAction("Index", "Home", new { id = "6" });

                        }

                    }
                }
                else
                {
                    ModelState.AddModelError("EmailisnotExist", "Email does not Exist");
                    return RedirectToAction("Index", "Home", new { id = "7" });
                }
            }
            else
            {
                return RedirectToAction("Index", "Home", new { id = "14]" + "Invalid Email Id" });
            }

        }


        [HttpGet]
        [Authorize]
        public ActionResult Cand_reg(string emid)
        {
            Tbl_Cand_Main usermodel = new Tbl_Cand_Main();

            return View(usermodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cand_reg([Bind(Exclude = " IsEmailVerified,ActivationCode")]  Tbl_Cand_Main userModel)
        {

            string Message = "";
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:25692/api/User");

                    //HTTP POST
                    var postTask = client.PostAsJsonAsync("http://localhost:25692/api/User/PostUser", userModel);
                    postTask.Wait();

                    var result = postTask.Result;
                    int ss = (int)result.StatusCode;
                    if (ss == 302)
                    {
                        ModelState.AddModelError("EmailExist", "Email Already Exist");
                        ViewBag.Message = "EMAIL ALREADY EXISTS";

                        return RedirectToAction("Index", "Home", new { id = "2" });
                    }
                    if (result.IsSuccessStatusCode)
                    {
                        if (ss == 201)
                        {
                            Message = "Registration Successfully done. Account activation link has been sent to your email id:" + userModel.Cand_EmailId;
                            return RedirectToAction("Index", "Home", new { id = "3" });
                        }
                    }
                }

            }
            else
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {

                        Message = error.ErrorMessage;
                        if (Message != null)
                        {


                            return RedirectToAction("Index", "Home", new { id = "13]" + Message });
                        }

                    }
                }
                return RedirectToAction("Index", "Home", new { id = "4]" + Message });
            }
            return RedirectToAction("Index");
        }
        //[ValidateAntiForgeryToken]
        //    public ActionResult Cand_reg([Bind(Exclude = " IsEmailVerified,ActivationCode")]  Tbl_Cand_Main userModel)
        //    {
        //        bool Status = false;
        //        string Message = "";
        //        if (ModelState.IsValid)
        //        {
        //            var isExist = IsEmailExist(userModel.Cand_EmailId);
        //            if (isExist)
        //            {
        //                ModelState.AddModelError("EmailExist", "Email Already Exist");
        //            ViewBag.Message = "EMAIL ALREADY EXISTS";

        //            return RedirectToAction("Index", "Home", new { id = "2" });

        //        }

        //            userModel.Cand_ActivationCode = Guid.NewGuid();

        //        Guid? candidate_reference = Guid.NewGuid();
        //        userModel.cand_ReferenceCode = candidate_reference;

        //        userModel.Cand_IsEmailVerified = false;
        //            using (EOTAEntities dbModel = new EOTAEntities())
        //            {
        //                dbModel.Tbl_Cand_Main.Add(userModel);
        //                dbModel.SaveChanges();
        //                SendVerificationlinkEmail(userModel.Cand_EmailId, userModel.Cand_ActivationCode.ToString());

        //                Message = "Registration Successfully done. Account activation link has been sent to your email id:" + userModel.Cand_EmailId;
        //                Status = true;
        //            return RedirectToAction("Index", "Home", new { id = "3" });
        //        }
        //        }
        //        else
        //        {
        //        foreach (ModelState modelState in ViewData.ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {

        //                Message = error.ErrorMessage;
        //                if(Message!=null)
        //                {


        //                return RedirectToAction("Index", "Home", new { id = "13]" + Message });
        //                }

        //            }
        //        }
        //        return RedirectToAction("Index", "Home", new { id = "4]"+ Message });
        //    }
        //       // //close for preventing to redirect other page and show the message. This is instructed that error and message should be shown in single message//
        //       //// ViewBag.Message = Message;
        //       //// ViewBag.Status = Status;
        //       // //return View(userModel);

        //    }

        [NonAction]
        public bool IsEmailExist(string emailId)
        {
            using (EOTAEntities dc = new EOTAEntities())
            {
                var v = dc.Tbl_Cand_Main.Where(x => x.Cand_EmailId == emailId).FirstOrDefault();
                return v != null;
            }
        }



        [NonAction]
        public void SendVerificationlinkEmail(string emailId, string activationCode)
        {

            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                var activatinCode = (from c in dbModel.Tbl_Cand_Main
                                     where c.Cand_EmailId == emailId
                                     select c.Cand_id).FirstOrDefault();

                activationCode = activationCode + "-5a23c" + activatinCode.ToString() + "De932b5";

                var server = Request.Url.Segments;

                //  var verifyUrl = HttpContext.Request. + activationCode;
                var verifyUrl = "/Student/Semail5467verification/" + activationCode;
                var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);


                message.Subject = "Your account is successfully created";

                message.Body = "<br/><br/>We are exited to tell you that your EOTA account is successfully created. Please click on the link for verification." +

                //"<br/><br/>Click on the link: " + link;
                "<br /><a href = '" + link + "'>Click here to activate your account.</a>";
                //"<br/><br/><a href='" + link + "'>" + link + "</a>";
                //"<br/><br/><a href='" + link + "'>" + click + "</a>";
                //"<br/><br/> < a href ='" + click1+ "' >"+ click+" </a>";
                //"< a href =' " +"~/Home/Index"+"'>"+ click+"</a>";

                message.IsBodyHtml = true;

                message.SendMailAsync();

            }

        }

        [NonAction]
        public void SendfrgtpsslinkEmail(string emailId, string activationCode)
        {
            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                var activatinCode = (from c in dbModel.Tbl_Cand_Main
                                     where c.Cand_EmailId == emailId
                                     select c.Cand_id).FirstOrDefault();

                activationCode = activationCode + "-5a23c" + activatinCode.ToString() + "De932b5";

                var server = Request.Url.Segments;
                var verifyUrl = "/Student/Semailpassrcvyverification/" + activationCode;
                var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

                message.Subject = "Password Recovery";

                message.Body = "<br/><br/>Please click on the link for recover your password." +

                "<br /><a href = '" + link + "'>Click here to recover your account password.</a>";

                message.IsBodyHtml = true;

                message.SendMailAsync();

            }

        }

        [NonAction]
        public void SendVerificationlinkEmail(string emailId)
        {


            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);


            var server = Request.Url.Segments;
            var verifyUrl = "/Home/Index/";
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);



            message.Subject = "Your EOTA account credentials are changed";

            message.Body = "<br/><br/>Congrates you have successfully changed your EOTA account credentials. " +

            " <br /><br />" + " Not You? Please Let us know. You can mail at hr@elephanttreetech.com or info @elephanttreetech.com";

            message.IsBodyHtml = true;

            message.SendMailAsync();

        }

        [NonAction]
        public void SendVerificationlinkEmail(Int32 id, string emailId, string Referenceemailfrom, string Refencecode)
        {


            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);

            var myreferencecd = Refencecode;
            var server = Request.Url.Segments;
            var verifyUrl = "/Home/Index/";
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var click = "click here";
            var click1 = " ~/Home /Index";
            var link1 = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/Home/Index");
            message.Subject = "You are refered to EOTA Training Application";

            message.Body = "<br /><br />Congrates You are successfully refered by " + Referenceemailfrom + " to EOTA Online Training Application." +

                "<br /><br />Try our site and please visit " + link + " for demo basis." +
                "<br />Your friends reference code = " + myreferencecd +
                " <br /> Use this reference code to Register EOTA Online Training Application,  this reference code will make discount if you buy any tutorials or videos." +
                "<br />During any problem, Please mail at etreetraining@gmail.com .We will be always available for you and give support happily";

            message.IsBodyHtml = true;

            message.SendMailAsync();

        }
        [NonAction]
        public void SendVerificationlinkEmailrefrfromemail(Int32 id, string emailId, string Referenceemailfrom, string Refencecode)
        {


            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(Referenceemailfrom);



            message.From = from;

            message.To.Add(to);

            var myreferencecd = Refencecode;
            var server = Request.Url.Segments;
            var verifyUrl = "/user/verifyaccount/" + myreferencecd;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            var url = "~/Home/Index";
            var click = "click here";
            var click1 = " ~/Home /Index";
            var link1 = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/Home/Index");
            message.Subject = "Reference successfull to EOTA Training Application";

            message.Body = "Congrates You refered successfully your friend " + emailId + " to EOTA Online Training Application. " +
                "<br />Thank you for your this step. We already sent a email with your refernce id (" + myreferencecd + "). " +

                 "<br />We glad to let you know that we will give you discount for your course study after your friend's registration and payment" +
                " <br />Thank You and please be with us." +
                " <br />If you facing any problem, Please mail at hr@elephanttreetech.com or info@elephanttreetech.com .We will be always available for you and give support happily";

            message.IsBodyHtml = true;

            message.SendMailAsync();

        }

        public ActionResult Confirmation()
        {
            return View();
        }
    }



}
