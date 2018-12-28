 using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtreetrainingUser.Models;
using Zender.Mail;
using System.Net.Mail;
using System.Web.Routing;
using System.Net;
using System.Net.NetworkInformation;
using System.Web.Security;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;
using PagedList;
using System.Net.Http;
using System.Threading.Tasks;

namespace EtreetrainingUser.Controllers
{
    [RequireHttps]
    [HandleError]
    [ValidateInput(false)]
    public class HomeController : Controller
    {
        public ActionResult Index2()
        {
            ViewBag.name = "Indranil Ganguly";
            ViewBag.sub = ".net MVC";
            ViewBag.dt = DateTime.Now.ToShortDateString();
            var dateAndTime = DateTime.Now;
            DateTime date = dateAndTime.Date;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public FileResult Export(string GridHtml, string GridHtml1)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                string imageURL = Server.MapPath(".") + "/img771.jpg";
                Image jpg = Image.GetInstance(imageURL);
                GridHtml += string.Format("<div><div><div style='padding-top: 10px; padding-left: 40px; '> <img  src='http://localhost:28493/Home/img771.jpg'></img></div></div></div> ");

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
                jpg.ScaleToFit(140f, 120f);

                jpg.SpacingAfter = 1f;
                jpg.Alignment = Element.ALIGN_LEFT;
                //pdfDoc.Add(jpg);
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);

                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "Certificate.pdf");
            }
        }

        public ActionResult Index1(string id)
        {
            return View();
        }
        [ValidateInput(false)]
        public ActionResult SearchAll()
        {
            FormsAuthentication.SignOut();

            using (EOTAEntities dbModel = new EOTAEntities())
            {

                ViewBag.academiccourses = dbModel.Post_Courses.Where(x => x.Course_Type == "A" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
                ViewBag.technicalcourses = dbModel.Post_Courses.Where(x => x.Course_Type == "T" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
                ViewBag.managementcourses = dbModel.Post_Courses.Where(x => x.Course_Type == "M" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
                return View();

            }


        }
        [ValidateInput(false)]
        public ActionResult SearchOne(string id, string page)

        {
            FormsAuthentication.SignOut();
            if (id != null)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:25692/api/User");

                    if (id == "A")
                    {
                        var GetAcademicCourses = client.GetAsync("http://localhost:25692/api/Home/GetAcademicCourses");
                        GetAcademicCourses.Wait();
                        var resultforGetAcademicCourses = GetAcademicCourses.Result;
                        if (resultforGetAcademicCourses.IsSuccessStatusCode)
                        {
                            var readTaskaca = resultforGetAcademicCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                            readTaskaca.Wait();
                            ViewBag.academiccourses = readTaskaca.Result;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                        }
                        ViewBag.CourseType = "A";
                        ViewBag.searchname = "Academic Courses";
                    }
                    else if (id == "T")
                    {
                        var GetTechnicalCourses = client.GetAsync("http://localhost:25692/api/Home/GetTechnicalCourses");
                        GetTechnicalCourses.Wait();
                        var resultforGetTechnicalCourses = GetTechnicalCourses.Result;
                        if (resultforGetTechnicalCourses.IsSuccessStatusCode)
                        {
                            var readTaskTech = resultforGetTechnicalCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                            readTaskTech.Wait();
                            ViewBag.technicalcourses = readTaskTech.Result;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                        }
                        ViewBag.CourseType = "T";
                        ViewBag.searchname = "Technical Courses";
                    }
                    else if (id == "M")
                    {
                        var GetManagementCourses = client.GetAsync("http://localhost:25692/api/Home/GetManagementCourses");
                        GetManagementCourses.Wait();
                        var resultforGetManagementCourses = GetManagementCourses.Result;
                        if (resultforGetManagementCourses.IsSuccessStatusCode)
                        {
                            var readTaskmng = resultforGetManagementCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                            readTaskmng.Wait();
                            ViewBag.managementcourses = readTaskmng.Result;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                        }
                        ViewBag.CourseType = "M";
                        ViewBag.searchname = "Management Courses";
                    }
                    else
                    {
                        ViewBag.CourseType = "N";
                        ViewBag.searchname = "Selected course cannot be found";
                    }
                    if (page == "shmr")
                    {
                        ViewBag.more = 1;
                    }
                    else
                    {
                        ViewBag.more = 2;
                    }
                    return View();
                }
                //using (EOTAEntities dbModel = new EOTAEntities())
                //{
                //    ViewBag.aca = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "A").OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    ViewBag.tech = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "T").OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    ViewBag.management = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "M").OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    if (id == "A")
                //    {
                //        ViewBag.CourseType = "A";
                //        ViewBag.searchname = "Academic Courses";
                //        ViewBag.academiccourses = dbModel.Post_Courses.Where(x => x.Course_Type == "A" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    }
                //    else if (id == "T")
                //    {
                //        ViewBag.CourseType = "T";
                //        ViewBag.searchname = "Technical Courses";
                //        ViewBag.technicalcourses = dbModel.Post_Courses.Where(x => x.Course_Type == "T" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    }
                //    else if (id == "M")
                //    {
                //        ViewBag.CourseType = "M";
                //        ViewBag.searchname = "Management Courses";
                //        ViewBag.managementcourses = dbModel.Post_Courses.Where(x => x.Course_Type == "M" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    }
                //    else
                //    {
                //        ViewBag.CourseType = "N";
                //        ViewBag.searchname = "Selected course cannot be found";
                //    }



                //    var crss = (from s in dbModel.Post_Courses
                //               .Where(x => x.Course_Type == id && x.EnableDisable == true)
                //                select s).Distinct();
                //    var crssfirsttwnty = (from s in dbModel.Post_Courses
                //              .Where(x => x.Course_Type == id && x.EnableDisable == true)
                //                          select s).Distinct();
                //    if (page == "shmr")
                //    {
                //        ViewBag.more = 1;
                //    }
                //    else
                //    {
                //        ViewBag.more = 2;
                //    }
                //    // int pageSize = 3;
                //    // int pageNumber = (page ?? 1);
                //    //if(page>20)
                //    // {

                //    // }
                //    // var j =  crss.OrderBy(x => x.Secret_Code).ToPagedList(pageNumber, pageSize);
                //    // ViewBag.pagenum = pageNumber;
                //    // ViewBag.pagecount = pageSize;
                //    // ViewBag.coursecount = crss.Count();
                //    // Post_Cours tt = new Post_Cours();
                //    // ViewBag.pagelist =j;
                //    //Tbl_Cand_Main ttbl = new Tbl_Cand_Main();
                //    //ttbl.ss = j;
                //    return View();


                //}

            }
            else
            {
                return RedirectToAction("Index", new RouteValueDictionary(
                     new { controller = "Home", action = "Index" }));
            }
        }

        public ActionResult singlecrs(string id)
        {
            FormsAuthentication.SignOut();
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

                if (resultforGetCrsByCode.IsSuccessStatusCode)
                {
                    var readTaskcrsbyId = resultforGetCrsByCode.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTaskcrsbyId.Wait();
                    ViewBag.selectcrs = readTaskcrsbyId.Result;
                    var postcrsall= readTaskcrsbyId.Result;
                    ViewBag.newprice= postcrsall.Select(z => z.New_Price).Distinct().FirstOrDefault();
                    ViewBag.firstdemovideo = postcrsall.Select(z => z.Demo_video_Path).Distinct().FirstOrDefault();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                using (EOTAEntities db = new EOTAEntities())
                {
                    ViewBag.trainerinfo = db.Courses_Trainers.Where(z=>z.Course_code==scrtcd).Distinct().FirstOrDefault();
                }
                        
                //    if (resultforGetAcademicCourses.IsSuccessStatusCode)
                //        {
                //            var readTaskaca = resultforGetAcademicCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                //            readTaskaca.Wait();
                //        ViewBag.aca = readTaskaca.Result;
                //        }
                //        else
                //        {
                //            ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                //        }


                //if (resultforGetTechnicalCourses.IsSuccessStatusCode)
                //{
                //    var readTasktech = resultforGetTechnicalCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                //        readTasktech.Wait();
                //        ViewBag.tech = readTasktech.Result;
                //}
                //else
                //{
                //    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                //}


                // if (resultforGetManagementCourses.IsSuccessStatusCode)
                //        {
                //            var readTaskmng = resultforGetManagementCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                //           readTaskmng.Wait();
                //        ViewBag.management = readTaskmng.Result;
                //        }
                //        else
                //        {
                //            ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                //        }
                //    return View();
                //}                    
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
                return View();



                //           using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
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
                //           ViewBag.secretcode = id;
                //           var id1 = int.Parse(id);
                //           ViewBag.aca = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "A").OrderBy(x => x.Secret_Code).Distinct().ToList();
                //           ViewBag.tech = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "T").OrderBy(x => x.Secret_Code).Distinct().ToList();
                //           ViewBag.management = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "M").OrderBy(x => x.Secret_Code).Distinct().ToList();


                //           ViewBag.selectcrs = dbModel.Post_Courses.Where(x => x.Secret_Code == id1 && x.EnableDisable == true).Distinct().ToList();

                //           return View();
                //       }
            }
        }

        
        public ActionResult Search(string search)

        {
            FormsAuthentication.SignOut();
            if (search.Trim().Length != 0)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44305/api/User");
                    var GetNEETcourseWithSearch = client.GetAsync("https://localhost:44305/api/Home/GetNEETcourseWithSearch?search=" + search);
                    GetNEETcourseWithSearch.Wait();
                    //var GetAcademicCourses = client.GetAsync("http://localhost:25692/api/Home/GetAcademicCoursesWithSearch?search=" + search);
                    //GetAcademicCourses.Wait();
                    //var GetTechnicalCourses = client.GetAsync("http://localhost:25692/api/Home/GetTechnicalCoursesWithSearch?search=" + search);
                    //GetTechnicalCourses.Wait();
                    //var GetManagementCourses = client.GetAsync("http://localhost:25692/api/Home/GetManagementCoursesWithSearch?search=" + search);
                    //GetManagementCourses.Wait();
                    var resultforGetNEETcourseWithSearch = GetNEETcourseWithSearch.Result;
                    //var resultforGetAcademicCourses = GetAcademicCourses.Result;
                    //var resultforGetTechnicalCourses = GetTechnicalCourses.Result;
                    //var resultforGetManagementCourses = GetManagementCourses.Result;
                    string scrtcd = null;
                    if (resultforGetNEETcourseWithSearch.IsSuccessStatusCode)
                    {
                        var readTaskaca = resultforGetNEETcourseWithSearch.Content.ReadAsStringAsync();
                        readTaskaca.Wait();
                        scrtcd = readTaskaca.Result;
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                    }

                    //if (resultforGetAcademicCourses.IsSuccessStatusCode)
                    //{
                    //    var readTaskaca = resultforGetAcademicCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                    //    readTaskaca.Wait();
                    //    ViewBag.academiccourses = readTaskaca.Result;
                    //}
                    //else
                    //{
                    //    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                    //}


                    //if (resultforGetTechnicalCourses.IsSuccessStatusCode)
                    //{
                    //    var readTasktech = resultforGetTechnicalCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                    //    readTasktech.Wait();
                    //    ViewBag.technicalcourses = readTasktech.Result;
                    //}
                    //else
                    //{
                    //    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                    //}


                    //if (resultforGetManagementCourses.IsSuccessStatusCode)
                    //{
                    //    var readTaskmng = resultforGetManagementCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                    //    readTaskmng.Wait();
                    //    ViewBag.managementcourses = readTaskmng.Result;
                    //}
                    //else
                    //{
                    //    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                    //}
                    //string str = null;
                    //if (search.Trim().Length > 100)
                    //{
                    //    str = search.Substring(0, 100);
                    //    ViewBag.searchname = str + "...";
                    //    ViewBag.longreq = " is too long a word. Try using a shorter word.";
                    //}
                    //else
                    //{
                    //    ViewBag.searchname = search;
                    //}
                    if (scrtcd == "null")
                    {
                        return RedirectToAction("Index");
                        
                    }else
                    {
                        return RedirectToAction("singlecrs", new RouteValueDictionary(
                          new { controller = "Home", action = "singlecrs", id = scrtcd }));
                    }
                }

                //string str = null;

                //using (EOTAEntities dbModel = new EOTAEntities())
                //{
                //    ViewBag.aca = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "A").OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    ViewBag.tech = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "T").OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    ViewBag.management = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "M").OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    if (search.Trim().Length > 100)
                //    {
                //        str = search.Substring(0, 100);
                //        ViewBag.searchname = str + "...";
                //        ViewBag.longreq = " is too long a word. Try using a shorter word.";
                //    }
                //    else
                //    {
                //        ViewBag.searchname = search;
                //    }

                //    ViewBag.academiccourses = dbModel.Post_Courses.Where(x => x.Title.StartsWith(search) && x.Course_Type == "A" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    ViewBag.technicalcourses = dbModel.Post_Courses.Where(x => x.Title.StartsWith(search) && x.Course_Type == "T" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    ViewBag.managementcourses = dbModel.Post_Courses.Where(x => x.Title.StartsWith(search) && x.Course_Type == "M" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
                //    return View();

                //}

            }
            else
            {
                return RedirectToAction("Index", new RouteValueDictionary(
                     new { controller = "Home", action = "Index" }));
            }
        }

        public ActionResult Index(string id)
        {
            FormsAuthentication.SignOut();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var GetDiscountCoderunning = client.GetAsync("http://localhost:25692/api/Home/GetDiscountCoderunning");
                GetDiscountCoderunning.Wait();
                var GetNEETCourses = client.GetAsync("http://localhost:25692/api/Home/GetNEETCourses");
                GetNEETCourses.Wait();
                //var GetAcademicCourses = client.GetAsync("http://localhost:25692/api/Home/GetAcademicCourses");
                //GetAcademicCourses.Wait();
                //var GetTechnicalCourses = client.GetAsync("http://localhost:25692/api/Home/GetTechnicalCourses");
                //GetTechnicalCourses.Wait();
                //var GetManagementCourses = client.GetAsync("http://localhost:25692/api/Home/GetManagementCourses");
                //GetManagementCourses.Wait();
                var GetUsers = client.GetAsync("http://localhost:25692/api/Home/GetUsers");
                GetUsers.Wait();
                var GetUserFeedbacks = client.GetAsync("http://localhost:25692/api/Home/GetUserFeedbacks");
                GetUserFeedbacks.Wait();
                var GetClearUnusedTests = client.GetAsync("http://localhost:25692/api/Home/GetClearUnusedTests");
                GetClearUnusedTests.Wait();
                var GetDemoVideos = client.GetAsync("http://localhost:25692/api/Home/GetDemoVideos");
                GetDemoVideos.Wait();

                var resultforGetDiscountCoderunning = GetDiscountCoderunning.Result;
                var resultforGetNEETCourses = GetNEETCourses.Result;
                //var resultforGetAcademicCourses = GetAcademicCourses.Result;
                //var resultforGetTechnicalCourses = GetTechnicalCourses.Result;
                //var resultforGetManagementCourses = GetManagementCourses.Result;
                var resultforGetUsers = GetUsers.Result;
                var resultforGetUserFeedbacks = GetUserFeedbacks.Result;
                var resultforGetClearUnusedTests = GetClearUnusedTests.Result;
                var resultforGetDemoVideos = GetDemoVideos.Result;
                if (id == "ok")
                {
                    ViewBag.errrid = "15";
                }
                if (id == "oksub")
                {
                    ViewBag.errrid = "16";
                }
                if (id == "23")
                {
                    ViewBag.errrid = "23";
                }
                if (id == "1")
                {
                    ViewBag.errrid = "1";
                }
                if (id == "2")
                {
                    ViewBag.errrid = "2";
                }
                if (id == "3")
                {
                    ViewBag.errrid = "3";
                }
                if (id == "4")
                {
                    ViewBag.errrid = "4";
                }
                if (id == "5")
                {
                    ViewBag.errrid = "5";
                }
                if (id == "6")
                {
                    ViewBag.errrid = "6";
                }
                if (id == "7")
                {
                    ViewBag.errrid = "7";
                }
                if (id != null)
                {
                    string id1 = (id.Substring(id.LastIndexOf("]") + 1));
                    var iis = id.Contains("]");
                    if (iis == true)
                    {
                        string userid54544 = (id.Substring(0, id.LastIndexOf("]") + 0));
                        if (userid54544 == "12")
                        {
                            ViewBag.errrid = "12";
                            ViewBag.errrmsg14 = id1;
                        }
                        if (userid54544 == "13")
                        {
                            ViewBag.errrid = "13";
                            ViewBag.errrmsg14 = id1;
                        }
                        if (userid54544 == "14")
                        {
                            ViewBag.errrid = "14";
                            ViewBag.errrmsg14 = id1;
                        }
                    }
                }

                if (resultforGetDiscountCoderunning.IsSuccessStatusCode)
                {
                    var readTaskDiscountCalc = resultforGetDiscountCoderunning.Content.ReadAsAsync<Discount_Calculator>();
                    readTaskDiscountCalc.Wait();
                    var discount = readTaskDiscountCalc.Result;
                    if (discount != null)
                    {
                        if (discount.Disable_Date < DateTime.Now)
                        {
                            ViewBag.disabledt = (discount.Disable_Date).Value.Minute;
                            ViewBag.discocde = null;
                            ViewBag.disper = null;
                        }
                        else
                        {
                            ViewBag.disabledt = (discount.Disable_Date - DateTime.Now).Value.TotalMinutes;
                            ViewBag.discocde = discount.Discount_Code;
                            ViewBag.disper = discount.Discount_Price;
                        }
                    }

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
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

                //if (resultforGetAcademicCourses.IsSuccessStatusCode)
                //{
                //    var readTaskaca = resultforGetAcademicCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                //    readTaskaca.Wait();
                //    ViewBag.academiccourses = readTaskaca.Result;

                //}
                //else
                //{
                //    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                //}

                //if (resultforGetTechnicalCourses.IsSuccessStatusCode)
                //{
                //    var readTaskTech = resultforGetTechnicalCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                //    readTaskTech.Wait();
                //    ViewBag.technicalcourses = readTaskTech.Result;

                //}
                //else
                //{
                //    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                //}
                //if (resultforGetManagementCourses.IsSuccessStatusCode)
                //{
                //    var readTaskmanagemnt = resultforGetManagementCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                //    readTaskmanagemnt.Wait();
                //    ViewBag.managementcourses = readTaskmanagemnt.Result;

                //}
                //else
                //{
                //    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                //}
                if (resultforGetUsers.IsSuccessStatusCode)
                {
                    var readTaskUsers = resultforGetUsers.Content.ReadAsAsync<IList<Tbl_Cand_Main>>();
                    readTaskUsers.Wait();
                    ViewBag.usrnm = readTaskUsers.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (resultforGetUserFeedbacks.IsSuccessStatusCode)
                {
                    var readTaskfeedback = resultforGetUserFeedbacks.Content.ReadAsAsync<IList<Feedback>>();
                    readTaskfeedback.Wait();
                    ViewBag.feedback = readTaskfeedback.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (!resultforGetClearUnusedTests.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (resultforGetDemoVideos.IsSuccessStatusCode)
                {
                    var readTaskDemvideos = resultforGetDemoVideos.Content.ReadAsAsync<IList<Tbl_Video>>();
                    readTaskDemvideos.Wait();
                    ViewBag.vidlist = readTaskDemvideos.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }

            }
            //FormsAuthentication.SignOut();
            //using (var client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri("http://localhost:25692/api/User");
            //    var GetDiscountCoderunning = client.GetAsync("http://localhost:25692/api/Home/GetDiscountCoderunning");
            //    GetDiscountCoderunning.Wait();
            //    var GetAcademicCourses = client.GetAsync("http://localhost:25692/api/Home/GetAcademicCourses");
            //    GetAcademicCourses.Wait();
            //    var GetTechnicalCourses = client.GetAsync("http://localhost:25692/api/Home/GetTechnicalCourses");
            //    GetTechnicalCourses.Wait();
            //    var GetManagementCourses = client.GetAsync("http://localhost:25692/api/Home/GetManagementCourses");
            //    GetManagementCourses.Wait();
            //    var GetUsers = client.GetAsync("http://localhost:25692/api/Home/GetUsers");
            //    GetUsers.Wait();
            //    var GetUserFeedbacks = client.GetAsync("http://localhost:25692/api/Home/GetUserFeedbacks");
            //    GetUserFeedbacks.Wait();
            //    var GetClearUnusedTests = client.GetAsync("http://localhost:25692/api/Home/GetClearUnusedTests");
            //    GetClearUnusedTests.Wait();
            //    var GetDemoVideos = client.GetAsync("http://localhost:25692/api/Home/GetDemoVideos");
            //    GetDemoVideos.Wait();

            //    var resultforGetDiscountCoderunning = GetDiscountCoderunning.Result;
            //    var resultforGetAcademicCourses = GetAcademicCourses.Result;
            //    var resultforGetTechnicalCourses = GetTechnicalCourses.Result;
            //    var resultforGetManagementCourses = GetManagementCourses.Result;
            //    var resultforGetUsers = GetUsers.Result;
            //    var resultforGetUserFeedbacks = GetUserFeedbacks.Result;
            //    var resultforGetClearUnusedTests = GetClearUnusedTests.Result;
            //    var resultforGetDemoVideos = GetDemoVideos.Result;
            //    if (id == "ok")
            //    {
            //        ViewBag.errrid = "15";
            //    }
            //    if (id == "oksub")
            //    {
            //        ViewBag.errrid = "16";
            //    }
            //    if (id == "23")
            //    {
            //        ViewBag.errrid = "23";
            //    }
            //    if (id == "1")
            //    {
            //        ViewBag.errrid = "1";
            //    }
            //    if (id == "2")
            //    {
            //        ViewBag.errrid = "2";
            //    }
            //    if (id == "3")
            //    {
            //        ViewBag.errrid = "3";
            //    }
            //    if (id == "4")
            //    {
            //        ViewBag.errrid = "4";
            //    }
            //    if (id == "5")
            //    {
            //        ViewBag.errrid = "5";
            //    }
            //    if (id == "6")
            //    {
            //        ViewBag.errrid = "6";
            //    }
            //    if (id == "7")
            //    {
            //        ViewBag.errrid = "7";
            //    }
            //    if (id != null)
            //    {
            //        string id1 = (id.Substring(id.LastIndexOf("]") + 1));
            //        var iis = id.Contains("]");
            //        if (iis == true)
            //        {
            //            string userid54544 = (id.Substring(0, id.LastIndexOf("]") + 0));
            //            if (userid54544 == "12")
            //            {
            //                ViewBag.errrid = "12";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //            if (userid54544 == "13")
            //            {
            //                ViewBag.errrid = "13";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //            if (userid54544 == "14")
            //            {
            //                ViewBag.errrid = "14";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //        }
            //    }

            //    if (resultforGetDiscountCoderunning.IsSuccessStatusCode)
            //    {
            //        var readTaskDiscountCalc = resultforGetDiscountCoderunning.Content.ReadAsAsync<Discount_Calculator>();
            //        readTaskDiscountCalc.Wait();
            //       var discount= readTaskDiscountCalc.Result;
            //        if (discount != null)
            //        {
            //            if (discount.Disable_Date < DateTime.Now)
            //            {
            //                ViewBag.disabledt = (discount.Disable_Date).Value.Minute;
            //                ViewBag.discocde = null;
            //                ViewBag.disper = null;
            //            }
            //            else
            //            {
            //                ViewBag.disabledt = (discount.Disable_Date - DateTime.Now).Value.TotalMinutes;
            //                ViewBag.discocde = discount.Discount_Code;
            //                ViewBag.disper = discount.Discount_Price;
            //            }
            //        }

            //    }
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
            //    }

            //    if (resultforGetAcademicCourses.IsSuccessStatusCode)
            //    {
            //        var readTaskaca = resultforGetAcademicCourses.Content.ReadAsAsync<IList<Post_Cours>>();
            //        readTaskaca.Wait();
            //        ViewBag.academiccourses = readTaskaca.Result;

            //    }
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
            //    }

            //    if (resultforGetTechnicalCourses.IsSuccessStatusCode)
            //    {
            //        var readTaskTech = resultforGetTechnicalCourses.Content.ReadAsAsync<IList<Post_Cours>>();
            //        readTaskTech.Wait();
            //        ViewBag.technicalcourses = readTaskTech.Result;

            //    }
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
            //    }
            //    if (resultforGetManagementCourses.IsSuccessStatusCode)
            //    {
            //        var readTaskmanagemnt = resultforGetManagementCourses.Content.ReadAsAsync<IList<Post_Cours>>();
            //        readTaskmanagemnt.Wait();
            //        ViewBag.managementcourses = readTaskmanagemnt.Result;

            //    }
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
            //    }
            //    if (resultforGetUsers.IsSuccessStatusCode)
            //    {
            //        var readTaskUsers = resultforGetUsers.Content.ReadAsAsync<IList<Tbl_Cand_Main>>();
            //        readTaskUsers.Wait();
            //        ViewBag.usrnm = readTaskUsers.Result;
            //    }
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
            //    }
            //    if (resultforGetUserFeedbacks.IsSuccessStatusCode)
            //    {
            //        var readTaskfeedback = resultforGetUserFeedbacks.Content.ReadAsAsync<IList<Feedback>>();
            //        readTaskfeedback.Wait();
            //        ViewBag.feedback = readTaskfeedback.Result;
            //    }
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
            //    }
            //    if (!resultforGetClearUnusedTests.IsSuccessStatusCode)
            //    { 
            //        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
            //    }
            //    if (resultforGetDemoVideos.IsSuccessStatusCode)
            //    {
            //        var readTaskDemvideos = resultforGetDemoVideos.Content.ReadAsAsync<IList<Tbl_Video>>();
            //        readTaskDemvideos.Wait();
            //        ViewBag.vidlist = readTaskDemvideos.Result;
            //    }
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
            //    }

            //}

            //using (EOTAEntities dbModel = new EOTAEntities())
            //{

            //    var discount = (from s in dbModel.Discount_Calculators
            //                    where s.Disable_Date > DateTime.Now
            //                    select new { s.Discount_Code, s.Disable_Date, s.Discount_Price }).Distinct().FirstOrDefault();

            //    if (discount != null)
            //    {
            //        if (discount.Disable_Date < DateTime.Now)
            //        {
            //            ViewBag.disabledt = (discount.Disable_Date).Value.Minute;
            //            ViewBag.discocde = null;
            //            ViewBag.disper = null;
            //        }
            //        else
            //        {
            //            ViewBag.disabledt = (discount.Disable_Date - DateTime.Now).Value.TotalMinutes;
            //            ViewBag.discocde = discount.Discount_Code;
            //            ViewBag.disper = discount.Discount_Price;
            //        }
            //    }
            //    ViewBag.aca = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "A").OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.tech = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "T").OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.management = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "M").OrderBy(x => x.Secret_Code).Distinct().ToList();

            //    ViewBag.academiccourses = dbModel.Post_Courses.Where(x => x.Course_Type == "A" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.technicalcourses = dbModel.Post_Courses.Where(x => x.Course_Type == "T" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.managementcourses = dbModel.Post_Courses.Where(x => x.Course_Type == "M" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    var usrcntr1 = (from s in dbModel.Use_Test_counters
            //                    select s).Distinct().ToList();

            //    var usrcntr = (
            //                   from sa in dbModel.Use_Mock_Tests

            //                   select sa).Distinct().ToList();
            //    if (id == "ok")
            //    {
            //        ViewBag.errrid = "15";
            //    }
            //    if (id == "oksub")
            //    {
            //        ViewBag.errrid = "16";
            //    }
            //    if (id == "23")
            //    {
            //        ViewBag.errrid = "23";
            //    }
            //    if (id == "1")
            //    {
            //        ViewBag.errrid = "1";
            //    }
            //    if (id == "2")
            //    {
            //        ViewBag.errrid = "2";
            //    }
            //    if (id == "3")
            //    {
            //        ViewBag.errrid = "3";
            //    }
            //    if (id == "4")
            //    {
            //        ViewBag.errrid = "4";
            //    }
            //    if (id == "5")
            //    {
            //        ViewBag.errrid = "5";
            //    }
            //    if (id == "6")
            //    {
            //        ViewBag.errrid = "6";
            //    }
            //    if (id == "7")
            //    {
            //        ViewBag.errrid = "7";
            //    }
            //    if (id != null)
            //    {
            //        string id1 = (id.Substring(id.LastIndexOf("]") + 1));
            //        var iis = id.Contains("]");
            //        if (iis == true)
            //        {
            //            string userid54544 = (id.Substring(0, id.LastIndexOf("]") + 0));
            //            if (userid54544 == "12")
            //            {
            //                ViewBag.errrid = "12";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //            if (userid54544 == "13")
            //            {
            //                ViewBag.errrid = "13";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //            if (userid54544 == "14")
            //            {
            //                ViewBag.errrid = "14";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //        }
            //    }
            //    var result = usrcntr.Where(p => !usrcntr1.Any(p2 => p2.Exam_Secretcode == p.Exam_Secretcode)).ToList();
            //    ViewBag.feedback = (from a in dbModel.Feedbacks
            //                        where a.IsActive == true
            //                        orderby a.Id descending
            //                        select a).Take(3).ToList();
            //    ViewBag.usrnm = (from b in dbModel.Tbl_Cand_Main

            //                     select b).ToList();
            //    foreach (var vp in result)
            //    {
            //        dbModel.Use_Mock_Tests.Remove(vp);
            //        dbModel.SaveChanges();
            //    }
            //    ViewBag.vidlist = dbModel.Tbl_Video.ToList();
            //}
            //id = null;
            return View();

        }
        public ActionResult BackupTest()
        {
            return View();

        }
        [Authorize]
        public ActionResult TestForInstructor()
        {
            EOTAEntities db = new EOTAEntities();
            ViewBag.resultforGetNEETCourses = db.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault();
            return View();
        }

        [HttpPost]
        public ActionResult TrnrBiography(string Course_code, Post_Cours docModel, HttpPostedFileBase TrnrImage)
        {
            Courses_Trainers courses_trainers = new Courses_Trainers();
            using (EOTAEntities db = new EOTAEntities())
            {
                Course_code = db.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET").Select(z => z.Secret_Code).Distinct().FirstOrDefault().ToString();
                int Coursecode = int.Parse(Course_code);

                //Checking file is available to save.  
                if (TrnrImage != null)
                {
                    bool isentry = db.Courses_Trainers.Where(z => z.Course_code == Coursecode).Any();
                    if (isentry == true)
                    {
                        courses_trainers = db.Courses_Trainers.Where(x => x.Course_code == Coursecode).Distinct().FirstOrDefault();


                    }
                    string fileName = "";
                    fileName = Path.GetFileNameWithoutExtension(TrnrImage.FileName);
                    string extension = Path.GetExtension(TrnrImage.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    courses_trainers.Image = "~/UploadedVideos/" + fileName;

                    fileName = Path.Combine(Server.MapPath("~/UploadedVideos/"), fileName);
                    courses_trainers.TrnrImage = TrnrImage;
                    courses_trainers.TrnrImage.SaveAs(fileName);

                    courses_trainers.Name = docModel.Name;
                    courses_trainers.Course_name = docModel.Coursename= "NEET";
                    courses_trainers.Course_code = Coursecode;
                    courses_trainers.Occupation = docModel.Occupation;
                    
                    courses_trainers.Biography = docModel.Biography;
                    if (isentry == false)
                    {
                        db.Courses_Trainers.Add(courses_trainers);
                    }
                }


                db.SaveChanges();

                return RedirectToAction("TestForInstructor");
            }
        }

        [HttpPost]
        public ActionResult Edit(string id,string New_Price, List<Post_Cours> docModel, HttpPostedFileBase[] VidFile, HttpPostedFileBase[] DemoFile)
        {
            Post_Cours post_Cours = new Post_Cours();
            EOTAEntities db = new EOTAEntities();
            int id1 = int.Parse(id);
            var secretcd = db.Post_Courses.Where(x => x.Id == id1).Select(z => z.Secret_Code).FirstOrDefault();
            docModel= db.Post_Courses.Where(x => x.Secret_Code == secretcd).ToList();
            var count = 0;
            foreach (HttpPostedFileBase file in VidFile)
            {
                //Checking file is available to save.  
                if (file != null)
                {
                    var chpt = count +1;
                    post_Cours = db.Post_Courses.Where(x => x.Chapter_number == chpt).Distinct().FirstOrDefault();
                    string fileName = "";
                    //string fileName1 = "";
                    post_Cours.VidFile = file;
                    fileName = Path.GetFileNameWithoutExtension(post_Cours.VidFile.FileName);
                    string extension = Path.GetExtension(post_Cours.VidFile.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    post_Cours.Video_Url = "~/UploadedVideos/" + fileName;


                    fileName = Path.Combine(Server.MapPath("~/UploadedVideos/"), fileName);

                    post_Cours.VidFile.SaveAs(fileName);
                    post_Cours.EnableDisable = true;
                    docModel[count].Video_Url = post_Cours.Video_Url;

                    docModel[count].New_Price = New_Price;
                }
                count++;
            }
            count = 0;
            var count1 = 0;
            foreach (HttpPostedFileBase file1 in DemoFile)
            {
                //Checking file is available to save.  
                if (file1 != null)
                {
                    var chpt = count1 + 1;
                    post_Cours = db.Post_Courses.Where(x => x.Chapter_number == chpt).Distinct().FirstOrDefault();
                    string fileName = "";
                    //string fileName1 = "";
                    post_Cours.DemoFile = file1;
                    fileName = Path.GetFileNameWithoutExtension(post_Cours.DemoFile.FileName);
                    string extension = Path.GetExtension(post_Cours.DemoFile.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    post_Cours.Demo_video_Path = "~/DemoVideos/" + fileName;


                    fileName = Path.Combine(Server.MapPath("~/DemoVideos/"), fileName);

                    post_Cours.DemoFile.SaveAs(fileName);
                    post_Cours.EnableDisable = true;
                    docModel[count1].Video_Url = post_Cours.Demo_video_Path;

                }
                count1++;
            }
            db.SaveChanges();
            count = 0;
            return RedirectToAction("TestForInstructor");
        }
            public PartialViewResult updateforneet(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                keyword = "NEET";
            }
            using (EOTAEntities db = new EOTAEntities())
            {
                var products = (from pcrs in db.Post_Courses

                                where pcrs.Course_name.ToUpper() == keyword.ToUpper()

                                select pcrs).Distinct().ToList();
                return PartialView("_CourseForNEET", products);
            }
        }
        
        [HttpPost]
        //public ActionResult Doc_upload(Post_Cours docModel, HttpPostedFileBase[] VidFile, HttpPostedFileBase[] Docimage)
        //{

        public async Task<ActionResult> Doc_upload([Bind(Include = "Course_name,Chapter_number,Title,Description")] IList<Post_Cours> docModel)
        {
            using (EOTAEntities db=new EOTAEntities())
            {
                if(docModel[0].Course_name.ToUpper()=="NEET")
                {
                    int i = docModel.Count();
                    Random rd = new Random(1234567890);
                    var secrtcode = int.Parse(rd.Next().ToString());
                    for (int j=0;j<i;j++)
                    {
                        docModel[j].Secret_Code = secrtcode;
                    }
                    
                    
                }
                db.Post_Courses.AddRange(docModel);
                db.SaveChanges();
            }
            return new JsonResult { Data = new { status = true } };
        }

        public ActionResult TestForCoursesADV()
        {
            return View();
        }
        public ActionResult TestForCoursesVideos()
        {
            return View();
        }
        public ActionResult Test1(string id)
        {
            FormsAuthentication.SignOut();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var GetDiscountCoderunning = client.GetAsync("http://localhost:25692/api/Home/GetDiscountCoderunning");
                GetDiscountCoderunning.Wait();
                var GetNEETCourses = client.GetAsync("http://localhost:25692/api/Home/GetNEETCourses");
                GetNEETCourses.Wait();
                //var GetAcademicCourses = client.GetAsync("http://localhost:25692/api/Home/GetAcademicCourses");
                //GetAcademicCourses.Wait();
                //var GetTechnicalCourses = client.GetAsync("http://localhost:25692/api/Home/GetTechnicalCourses");
                //GetTechnicalCourses.Wait();
                //var GetManagementCourses = client.GetAsync("http://localhost:25692/api/Home/GetManagementCourses");
                //GetManagementCourses.Wait();
                var GetUsers = client.GetAsync("http://localhost:25692/api/Home/GetUsers");
                GetUsers.Wait();
                var GetUserFeedbacks = client.GetAsync("http://localhost:25692/api/Home/GetUserFeedbacks");
                GetUserFeedbacks.Wait();
                var GetClearUnusedTests = client.GetAsync("http://localhost:25692/api/Home/GetClearUnusedTests");
                GetClearUnusedTests.Wait();
                var GetDemoVideos = client.GetAsync("http://localhost:25692/api/Home/GetDemoVideos");
                GetDemoVideos.Wait();

                var resultforGetDiscountCoderunning = GetDiscountCoderunning.Result;
                var resultforGetNEETCourses = GetNEETCourses.Result;
                //var resultforGetAcademicCourses = GetAcademicCourses.Result;
                //var resultforGetTechnicalCourses = GetTechnicalCourses.Result;
                //var resultforGetManagementCourses = GetManagementCourses.Result;
                var resultforGetUsers = GetUsers.Result;
                var resultforGetUserFeedbacks = GetUserFeedbacks.Result;
                var resultforGetClearUnusedTests = GetClearUnusedTests.Result;
                var resultforGetDemoVideos = GetDemoVideos.Result;
                if (id == "ok")
                {
                    ViewBag.errrid = "15";
                }
                if (id == "oksub")
                {
                    ViewBag.errrid = "16";
                }
                if (id == "23")
                {
                    ViewBag.errrid = "23";
                }
                if (id == "1")
                {
                    ViewBag.errrid = "1";
                }
                if (id == "2")
                {
                    ViewBag.errrid = "2";
                }
                if (id == "3")
                {
                    ViewBag.errrid = "3";
                }
                if (id == "4")
                {
                    ViewBag.errrid = "4";
                }
                if (id == "5")
                {
                    ViewBag.errrid = "5";
                }
                if (id == "6")
                {
                    ViewBag.errrid = "6";
                }
                if (id == "7")
                {
                    ViewBag.errrid = "7";
                }
                if (id != null)
                {
                    string id1 = (id.Substring(id.LastIndexOf("]") + 1));
                    var iis = id.Contains("]");
                    if (iis == true)
                    {
                        string userid54544 = (id.Substring(0, id.LastIndexOf("]") + 0));
                        if (userid54544 == "12")
                        {
                            ViewBag.errrid = "12";
                            ViewBag.errrmsg14 = id1;
                        }
                        if (userid54544 == "13")
                        {
                            ViewBag.errrid = "13";
                            ViewBag.errrmsg14 = id1;
                        }
                        if (userid54544 == "14")
                        {
                            ViewBag.errrid = "14";
                            ViewBag.errrmsg14 = id1;
                        }
                    }
                }

                if (resultforGetDiscountCoderunning.IsSuccessStatusCode)
                {
                    var readTaskDiscountCalc = resultforGetDiscountCoderunning.Content.ReadAsAsync<Discount_Calculator>();
                    readTaskDiscountCalc.Wait();
                    var discount = readTaskDiscountCalc.Result;
                    if (discount != null)
                    {
                        if (discount.Disable_Date < DateTime.Now)
                        {
                            ViewBag.disabledt = (discount.Disable_Date).Value.Minute;
                            ViewBag.discocde = null;
                            ViewBag.disper = null;
                        }
                        else
                        {
                            ViewBag.disabledt = (discount.Disable_Date - DateTime.Now).Value.TotalMinutes;
                            ViewBag.discocde = discount.Discount_Code;
                            ViewBag.disper = discount.Discount_Price;
                        }
                    }

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (resultforGetNEETCourses.IsSuccessStatusCode)
                {
                    var readTaskNEET = resultforGetNEETCourses.Content.ReadAsStringAsync();
                    readTaskNEET.Wait();
                    ViewBag.resultforGetNEETCourses = readTaskNEET.Result;
                    EOTAEntities db = new EOTAEntities();
                    ViewBag.firstdemovideo =db.Post_Courses.Where(z => z.Course_name.ToUpper()=="NEET").Select(z => z.Demo_video_Path).Distinct().FirstOrDefault();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }

                //if (resultforGetAcademicCourses.IsSuccessStatusCode)
                //{
                //    var readTaskaca = resultforGetAcademicCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                //    readTaskaca.Wait();
                //    ViewBag.academiccourses = readTaskaca.Result;

                //}
                //else
                //{
                //    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                //}

                //if (resultforGetTechnicalCourses.IsSuccessStatusCode)
                //{
                //    var readTaskTech = resultforGetTechnicalCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                //    readTaskTech.Wait();
                //    ViewBag.technicalcourses = readTaskTech.Result;

                //}
                //else
                //{
                //    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                //}
                //if (resultforGetManagementCourses.IsSuccessStatusCode)
                //{
                //    var readTaskmanagemnt = resultforGetManagementCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                //    readTaskmanagemnt.Wait();
                //    ViewBag.managementcourses = readTaskmanagemnt.Result;

                //}
                //else
                //{
                //    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                //}
                if (resultforGetUsers.IsSuccessStatusCode)
                {
                    var readTaskUsers = resultforGetUsers.Content.ReadAsAsync<IList<Tbl_Cand_Main>>();
                    readTaskUsers.Wait();
                    ViewBag.usrnm = readTaskUsers.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (resultforGetUserFeedbacks.IsSuccessStatusCode)
                {
                    var readTaskfeedback = resultforGetUserFeedbacks.Content.ReadAsAsync<IList<Feedback>>();
                    readTaskfeedback.Wait();
                    ViewBag.feedback = readTaskfeedback.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (!resultforGetClearUnusedTests.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (resultforGetDemoVideos.IsSuccessStatusCode)
                {
                    var readTaskDemvideos = resultforGetDemoVideos.Content.ReadAsAsync<IList<Tbl_Video>>();
                    readTaskDemvideos.Wait();
                    ViewBag.vidlist = readTaskDemvideos.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }

            }

            //using (EOTAEntities dbModel = new EOTAEntities())
            //{

            //    var discount = (from s in dbModel.Discount_Calculators
            //                    where s.Disable_Date > DateTime.Now
            //                    select new { s.Discount_Code, s.Disable_Date, s.Discount_Price }).Distinct().FirstOrDefault();

            //    if (discount != null)
            //    {
            //        if (discount.Disable_Date < DateTime.Now)
            //        {
            //            ViewBag.disabledt = (discount.Disable_Date).Value.Minute;
            //            ViewBag.discocde = null;
            //            ViewBag.disper = null;
            //        }
            //        else
            //        {
            //            ViewBag.disabledt = (discount.Disable_Date - DateTime.Now).Value.TotalMinutes;
            //            ViewBag.discocde = discount.Discount_Code;
            //            ViewBag.disper = discount.Discount_Price;
            //        }
            //    }
            //    ViewBag.aca = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "A").OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.tech = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "T").OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.management = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "M").OrderBy(x => x.Secret_Code).Distinct().ToList();

            //    ViewBag.academiccourses = dbModel.Post_Courses.Where(x => x.Course_Type == "A" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.technicalcourses = dbModel.Post_Courses.Where(x => x.Course_Type == "T" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.managementcourses = dbModel.Post_Courses.Where(x => x.Course_Type == "M" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    var usrcntr1 = (from s in dbModel.Use_Test_counters
            //                    select s).Distinct().ToList();

            //    var usrcntr = (
            //                   from sa in dbModel.Use_Mock_Tests

            //                   select sa).Distinct().ToList();
            //    if (id == "ok")
            //    {
            //        ViewBag.errrid = "15";
            //    }
            //    if (id == "oksub")
            //    {
            //        ViewBag.errrid = "16";
            //    }
            //    if (id == "23")
            //    {
            //        ViewBag.errrid = "23";
            //    }
            //    if (id == "1")
            //    {
            //        ViewBag.errrid = "1";
            //    }
            //    if (id == "2")
            //    {
            //        ViewBag.errrid = "2";
            //    }
            //    if (id == "3")
            //    {
            //        ViewBag.errrid = "3";
            //    }
            //    if (id == "4")
            //    {
            //        ViewBag.errrid = "4";
            //    }
            //    if (id == "5")
            //    {
            //        ViewBag.errrid = "5";
            //    }
            //    if (id == "6")
            //    {
            //        ViewBag.errrid = "6";
            //    }
            //    if (id == "7")
            //    {
            //        ViewBag.errrid = "7";
            //    }
            //    if (id != null)
            //    {
            //        string id1 = (id.Substring(id.LastIndexOf("]") + 1));
            //        var iis = id.Contains("]");
            //        if (iis == true)
            //        {
            //            string userid54544 = (id.Substring(0, id.LastIndexOf("]") + 0));
            //            if (userid54544 == "12")
            //            {
            //                ViewBag.errrid = "12";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //            if (userid54544 == "13")
            //            {
            //                ViewBag.errrid = "13";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //            if (userid54544 == "14")
            //            {
            //                ViewBag.errrid = "14";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //        }
            //    }
            //    var result = usrcntr.Where(p => !usrcntr1.Any(p2 => p2.Exam_Secretcode == p.Exam_Secretcode)).ToList();
            //    ViewBag.feedback = (from a in dbModel.Feedbacks
            //                        where a.IsActive == true
            //                        orderby a.Id descending
            //                        select a).Take(3).ToList();
            //    ViewBag.usrnm = (from b in dbModel.Tbl_Cand_Main

            //                     select b).ToList();
            //    foreach (var vp in result)
            //    {
            //        dbModel.Use_Mock_Tests.Remove(vp);
            //        dbModel.SaveChanges();
            //    }
            //    ViewBag.vidlist = dbModel.Tbl_Video.ToList();
            //}
            //id = null;
            return View();

        }
        public ActionResult Test(string id)
        {
            FormsAuthentication.SignOut();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var GetDiscountCoderunning = client.GetAsync("http://localhost:25692/api/Home/GetDiscountCoderunning");
                GetDiscountCoderunning.Wait();
                var GetAcademicCourses = client.GetAsync("http://localhost:25692/api/Home/GetAcademicCourses");
                GetAcademicCourses.Wait();
                var GetTechnicalCourses = client.GetAsync("http://localhost:25692/api/Home/GetTechnicalCourses");
                GetTechnicalCourses.Wait();
                var GetManagementCourses = client.GetAsync("http://localhost:25692/api/Home/GetManagementCourses");
                GetManagementCourses.Wait();
                var GetUsers = client.GetAsync("http://localhost:25692/api/Home/GetUsers");
                GetUsers.Wait();
                var GetUserFeedbacks = client.GetAsync("http://localhost:25692/api/Home/GetUserFeedbacks");
                GetUserFeedbacks.Wait();
                var GetClearUnusedTests = client.GetAsync("http://localhost:25692/api/Home/GetClearUnusedTests");
                GetClearUnusedTests.Wait();
                var GetDemoVideos = client.GetAsync("http://localhost:25692/api/Home/GetDemoVideos");
                GetDemoVideos.Wait();

                var resultforGetDiscountCoderunning = GetDiscountCoderunning.Result;
                var resultforGetAcademicCourses = GetAcademicCourses.Result;
                var resultforGetTechnicalCourses = GetTechnicalCourses.Result;
                var resultforGetManagementCourses = GetManagementCourses.Result;
                var resultforGetUsers = GetUsers.Result;
                var resultforGetUserFeedbacks = GetUserFeedbacks.Result;
                var resultforGetClearUnusedTests = GetClearUnusedTests.Result;
                var resultforGetDemoVideos = GetDemoVideos.Result;
                if (id == "ok")
                {
                    ViewBag.errrid = "15";
                }
                if (id == "oksub")
                {
                    ViewBag.errrid = "16";
                }
                if (id == "23")
                {
                    ViewBag.errrid = "23";
                }
                if (id == "1")
                {
                    ViewBag.errrid = "1";
                }
                if (id == "2")
                {
                    ViewBag.errrid = "2";
                }
                if (id == "3")
                {
                    ViewBag.errrid = "3";
                }
                if (id == "4")
                {
                    ViewBag.errrid = "4";
                }
                if (id == "5")
                {
                    ViewBag.errrid = "5";
                }
                if (id == "6")
                {
                    ViewBag.errrid = "6";
                }
                if (id == "7")
                {
                    ViewBag.errrid = "7";
                }
                if (id != null)
                {
                    string id1 = (id.Substring(id.LastIndexOf("]") + 1));
                    var iis = id.Contains("]");
                    if (iis == true)
                    {
                        string userid54544 = (id.Substring(0, id.LastIndexOf("]") + 0));
                        if (userid54544 == "12")
                        {
                            ViewBag.errrid = "12";
                            ViewBag.errrmsg14 = id1;
                        }
                        if (userid54544 == "13")
                        {
                            ViewBag.errrid = "13";
                            ViewBag.errrmsg14 = id1;
                        }
                        if (userid54544 == "14")
                        {
                            ViewBag.errrid = "14";
                            ViewBag.errrmsg14 = id1;
                        }
                    }
                }

                if (resultforGetDiscountCoderunning.IsSuccessStatusCode)
                {
                    var readTaskDiscountCalc = resultforGetDiscountCoderunning.Content.ReadAsAsync<Discount_Calculator>();
                    readTaskDiscountCalc.Wait();
                    var discount = readTaskDiscountCalc.Result;
                    if (discount != null)
                    {
                        if (discount.Disable_Date < DateTime.Now)
                        {
                            ViewBag.disabledt = (discount.Disable_Date).Value.Minute;
                            ViewBag.discocde = null;
                            ViewBag.disper = null;
                        }
                        else
                        {
                            ViewBag.disabledt = (discount.Disable_Date - DateTime.Now).Value.TotalMinutes;
                            ViewBag.discocde = discount.Discount_Code;
                            ViewBag.disper = discount.Discount_Price;
                        }
                    }

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }

                if (resultforGetAcademicCourses.IsSuccessStatusCode)
                {
                    var readTaskaca = resultforGetAcademicCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTaskaca.Wait();
                    ViewBag.academiccourses = readTaskaca.Result;

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }

                if (resultforGetTechnicalCourses.IsSuccessStatusCode)
                {
                    var readTaskTech = resultforGetTechnicalCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTaskTech.Wait();
                    ViewBag.technicalcourses = readTaskTech.Result;

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (resultforGetManagementCourses.IsSuccessStatusCode)
                {
                    var readTaskmanagemnt = resultforGetManagementCourses.Content.ReadAsAsync<IList<Post_Cours>>();
                    readTaskmanagemnt.Wait();
                    ViewBag.managementcourses = readTaskmanagemnt.Result;

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (resultforGetUsers.IsSuccessStatusCode)
                {
                    var readTaskUsers = resultforGetUsers.Content.ReadAsAsync<IList<Tbl_Cand_Main>>();
                    readTaskUsers.Wait();
                    ViewBag.usrnm = readTaskUsers.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (resultforGetUserFeedbacks.IsSuccessStatusCode)
                {
                    var readTaskfeedback = resultforGetUserFeedbacks.Content.ReadAsAsync<IList<Feedback>>();
                    readTaskfeedback.Wait();
                    ViewBag.feedback = readTaskfeedback.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (!resultforGetClearUnusedTests.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }
                if (resultforGetDemoVideos.IsSuccessStatusCode)
                {
                    var readTaskDemvideos = resultforGetDemoVideos.Content.ReadAsAsync<IList<Tbl_Video>>();
                    readTaskDemvideos.Wait();
                    ViewBag.vidlist = readTaskDemvideos.Result;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something wenmt wrong.");
                }

            }

            //using (EOTAEntities dbModel = new EOTAEntities())
            //{

            //    var discount = (from s in dbModel.Discount_Calculators
            //                    where s.Disable_Date > DateTime.Now
            //                    select new { s.Discount_Code, s.Disable_Date, s.Discount_Price }).Distinct().FirstOrDefault();

            //    if (discount != null)
            //    {
            //        if (discount.Disable_Date < DateTime.Now)
            //        {
            //            ViewBag.disabledt = (discount.Disable_Date).Value.Minute;
            //            ViewBag.discocde = null;
            //            ViewBag.disper = null;
            //        }
            //        else
            //        {
            //            ViewBag.disabledt = (discount.Disable_Date - DateTime.Now).Value.TotalMinutes;
            //            ViewBag.discocde = discount.Discount_Code;
            //            ViewBag.disper = discount.Discount_Price;
            //        }
            //    }
            //    ViewBag.aca = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "A").OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.tech = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "T").OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.management = dbModel.Post_Courses.Where(x => x.EnableDisable == true && x.Course_Type == "M").OrderBy(x => x.Secret_Code).Distinct().ToList();

            //    ViewBag.academiccourses = dbModel.Post_Courses.Where(x => x.Course_Type == "A" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.technicalcourses = dbModel.Post_Courses.Where(x => x.Course_Type == "T" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    ViewBag.managementcourses = dbModel.Post_Courses.Where(x => x.Course_Type == "M" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList();
            //    var usrcntr1 = (from s in dbModel.Use_Test_counters
            //                    select s).Distinct().ToList();

            //    var usrcntr = (
            //                   from sa in dbModel.Use_Mock_Tests

            //                   select sa).Distinct().ToList();
            //    if (id == "ok")
            //    {
            //        ViewBag.errrid = "15";
            //    }
            //    if (id == "oksub")
            //    {
            //        ViewBag.errrid = "16";
            //    }
            //    if (id == "23")
            //    {
            //        ViewBag.errrid = "23";
            //    }
            //    if (id == "1")
            //    {
            //        ViewBag.errrid = "1";
            //    }
            //    if (id == "2")
            //    {
            //        ViewBag.errrid = "2";
            //    }
            //    if (id == "3")
            //    {
            //        ViewBag.errrid = "3";
            //    }
            //    if (id == "4")
            //    {
            //        ViewBag.errrid = "4";
            //    }
            //    if (id == "5")
            //    {
            //        ViewBag.errrid = "5";
            //    }
            //    if (id == "6")
            //    {
            //        ViewBag.errrid = "6";
            //    }
            //    if (id == "7")
            //    {
            //        ViewBag.errrid = "7";
            //    }
            //    if (id != null)
            //    {
            //        string id1 = (id.Substring(id.LastIndexOf("]") + 1));
            //        var iis = id.Contains("]");
            //        if (iis == true)
            //        {
            //            string userid54544 = (id.Substring(0, id.LastIndexOf("]") + 0));
            //            if (userid54544 == "12")
            //            {
            //                ViewBag.errrid = "12";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //            if (userid54544 == "13")
            //            {
            //                ViewBag.errrid = "13";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //            if (userid54544 == "14")
            //            {
            //                ViewBag.errrid = "14";
            //                ViewBag.errrmsg14 = id1;
            //            }
            //        }
            //    }
            //    var result = usrcntr.Where(p => !usrcntr1.Any(p2 => p2.Exam_Secretcode == p.Exam_Secretcode)).ToList();
            //    ViewBag.feedback = (from a in dbModel.Feedbacks
            //                        where a.IsActive == true
            //                        orderby a.Id descending
            //                        select a).Take(3).ToList();
            //    ViewBag.usrnm = (from b in dbModel.Tbl_Cand_Main

            //                     select b).ToList();
            //    foreach (var vp in result)
            //    {
            //        dbModel.Use_Mock_Tests.Remove(vp);
            //        dbModel.SaveChanges();
            //    }
            //    ViewBag.vidlist = dbModel.Tbl_Video.ToList();
            //}
            //id = null;
            return View();

        }
        [HttpGet]
        public ActionResult termscond()
        {
            return View();
        }
        //[HttpPost]
        //public ActionResult Mailsender(FormCollection frm)
        //{
        //   var name = frm["name"];
        //    var email = frm["email"];
        //    var tel1 = frm["tel"].ToString();
        //    var Coursetype = frm["CRSTP"];
        //    string Coursetypetxt = null;
        //    if (Coursetype=="A")
        //    {
        //        Coursetypetxt = "Academic";
        //    }
        //    else if (Coursetype == "T")
        //    {
        //        Coursetypetxt = "Technical";
        //    }
        //    else if (Coursetype == "M")
        //    {
        //        Coursetypetxt = "Management";
        //    }
        //    else
        //    {
        //        Coursetypetxt = "Does not exists";
        //    }

        //    var other = frm["other"];
        //    var Message = frm["Message"];
        //    SendEmail(name, email, tel1, Coursetypetxt, other, Message);

        //    return RedirectToAction("Index", new RouteValueDictionary(
        //             new { controller = "Home", action = "Index", Id = "ok" }));
        //}

        [HttpPost]
        public ActionResult Subscribe(string Email)
        {
            string message = "Dear Sir/Madam," +
                "<br/>I have keen interest in EOTA. Please share me all latest updates of EOTA." +
                "<br/>Thank you";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:25692/api/User");
                var GetSubscriptionwithoutmessage = client.GetAsync("http://localhost:25692/api/Home/GetSubscriptionMail?Email="+ Email + "&&message=" + null);
                GetSubscriptionwithoutmessage.Wait();
                var GetSubscriptionwithmessage = client.GetAsync("http://localhost:25692/api/Home/GetSubscriptionMail?Email=" + Email + "&&message=" +message);
                GetSubscriptionwithmessage.Wait();
            }
            return RedirectToAction("Index", new RouteValueDictionary(
                     new { controller = "Home", action = "Index", Id = "oksub" }));
        }

        //[NonAction]
        //public void SendEmail(string email, string Message1)
        //{
        //    ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

        //    MailAddress from = new MailAddress("etreetraining@gmail.com");

        //    MailAddress to = new MailAddress("etreetraining@gmail.com");

        //    message.From = from;

        //    message.To.Add(to);
        //    using (EOTAEntities dbModel = new EOTAEntities())
        //    {

        //        var server = Request.Url.Segments;

        //        message.Subject = "User subscription";

        //        message.Body = "<br/>User email: " + email +

        //            "<br/>" + Message1;

        //        message.IsBodyHtml = true;

        //        message.SendMailAsync();

        //    }

        //}

        //[NonAction]
        //public void SendEmail(string email)
        //{
        //    ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

        //    MailAddress from = new MailAddress("etreetraining@gmail.com");

        //    MailAddress to = new MailAddress(email);

        //    message.From = from;

        //    message.To.Add(to);
        //    using (EOTAEntities dbModel = new EOTAEntities())
        //    {

        //        var server = Request.Url.Segments;

        //        message.Subject = "EOTA subscription successfull";

        //        message.Body = "Thank You for your subscription." +
        //            "<br/><br/>You will recieve our latest news";


        //        message.IsBodyHtml = true;

        //        message.SendMailAsync();

        //    }

        //}

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}