using EtreetrainingUser.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace EtreetrainingUser.Controllers
{
    [RequireHttps]
    [HandleError]
    [Authorize]
    [ValidateInput(false)]
    public class Training_mainController : Controller
    {
        [Authorize]
        // GET: Training_main
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult tst()
        {
            return View();
        }
        public ActionResult Edit(Int32 id, string msg)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                if (isadmn() == true)
                {
                    ViewBag.url = "/ADMIN/Dashboard";
                }
                else if (istrnr() == true)
                {
                    ViewBag.url = "/Trainers/Dashboard";
                }
                else
                {
                    ViewBag.url = "#";
                }
                ViewBag.Techcourse = (from a in dbModel.Post_Courses
                                      where a.Course_Type == "T" && a.EnableDisable == true
                                      select a.Title).Distinct().ToList();

                ViewBag.Acacourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "A" && a.EnableDisable == true
                                     select a.Title).Distinct().ToList();

                ViewBag.Mngcourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "M" && a.EnableDisable == true
                                     select a.Title).Distinct().ToList();
                ViewBag.UploadStatus = msg;
                ViewBag.secretcode = id;
                return View(dbModel.Post_Courses.Where(x => x.Secret_Code == id && x.EnableDisable == true).FirstOrDefault());
            }
        }
        [HttpPost]
        public ActionResult Edit(Int32 id, Post_Cours docModel, HttpPostedFileBase[] VidFile, HttpPostedFileBase[] Docimage)
        {
            string msg1 = null;
            docModel.upldr ="A,T";
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                if (isadmn() == true)
                {
                    ViewBag.url = "/ADMIN/Dashboard";
                }
                else if (istrnr() == true)
                {
                    ViewBag.url = "/Trainers/Dashboard";
                }
                else
                {
                    ViewBag.url = "#";
                }
                var docmodel2 = docModel;
                //if (ModelState.ContainsKey("upldr"))
                //    ModelState["upldr"].Errors.Clear();
                //if (ModelState.IsValid)
                //{
                   
                //}
                
                var cnt = 1;
                var pict = (from a in dbModel.Post_Courses
                            where a.Secret_Code == id
                            select a).FirstOrDefault();
                var pid = (from a in dbModel.Post_Courses
                            where a.Secret_Code == id
                            select a.Id).ToList();
                using (EOTAEntities db = new EOTAEntities())
                {

                    foreach (var ii in pid)
                    {

                        Post_Cours customers = db.Post_Courses.Where(x => x.Id == ii).FirstOrDefault();
                        db.Post_Courses.Remove(customers);
                        db.SaveChanges();
                    }

                }
                if (pict.Admin_Id != null)
                    {
                      
                        
                            docModel.Id = pict.Id;
                            docModel.Admin_Id = pict.Admin_Id;
                    docModel.Secret_Code = pict.Secret_Code;
                         

                            foreach (HttpPostedFileBase file in VidFile)
                            {
                                //Checking file is available to save.  
                                if (file != null)
                                {

                                    string fileName = "";
                                    //string fileName1 = "";
                                    docModel.VidFile = file;
                                    fileName = Path.GetFileNameWithoutExtension(docModel.VidFile.FileName);
                                    string extension = Path.GetExtension(docModel.VidFile.FileName);
                                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                                    docModel.Video_Url = "~/UploadedVideos/" + fileName;


                                    fileName = Path.Combine(Server.MapPath("~/UploadedVideos/"), fileName);

                                    docModel.VidFile.SaveAs(fileName);
                                    docModel.EnableDisable = true;
                                    using (EOTAEntities db1 = new EOTAEntities())
                                    {
                               
                                   
                                db1.Post_Courses.Add(docModel);
                                db1.SaveChanges();
                            }

                                }

                            }

                            foreach (HttpPostedFileBase file1 in Docimage)
                            {
                                //Checking file is available to save.  
                                if (file1 != null)
                                {
                                    string docfileName = "";
                                    docModel.Docimage = file1;
                                    docfileName = Path.GetFileNameWithoutExtension(docModel.Docimage.FileName);
                                    string docextension = Path.GetExtension(docModel.Docimage.FileName);
                                    docfileName = docfileName + DateTime.Now.ToString("yymmssfff") + docextension;
                                    docModel.Document_Image = "~/Uploaded Images/" + docfileName;
                                //if (cnt > 1)
                                //{
                                    docModel.Video_Url = null;
                                //}
                                    docfileName = Path.Combine(Server.MapPath("~/Uploaded Images/"), docfileName);


                                    docModel.Docimage.SaveAs(docfileName);
                                    docModel.EnableDisable = true;
                                    using (EOTAEntities db2 = new EOTAEntities())
                                    {
                               
                                db2.Post_Courses.Add(docModel);
                                db2.SaveChanges();
                            }

                                }
                        cnt++;
                            }

                            using (EOTAEntities db = new EOTAEntities())
                            {
                                ViewBag.Techcourse = (from a in db.Post_Courses
                                                      where a.Course_Type == "T" && a.EnableDisable == true
                                                      select a.Title).Distinct().ToList();

                                ViewBag.Acacourse = (from a in db.Post_Courses
                                                     where a.Course_Type == "A" && a.EnableDisable == true
                                                     select a.Title).Distinct().ToList();

                                ViewBag.Mngcourse = (from a in db.Post_Courses
                                                     where a.Course_Type == "M" && a.EnableDisable == true
                                                     select a.Title).Distinct().ToList();
                            }
                     msg1 = "Your course edited successfully";

                    ViewBag.UploadStatus = "Your course edited successfully";
                        }
                   
                
                else if (pict.Trainer_Id != null)
                {
                 
                        docModel.Id = pict.Id;
                        docModel.Trainer_Id = pict.Trainer_Id;

                    docModel.Secret_Code = pict.Secret_Code;
                    foreach (HttpPostedFileBase file in VidFile)
                        {
                            //Checking file is available to save.  
                            if (file != null)
                            {

                                string fileName = "";
                                //string fileName1 = "";
                                docModel.VidFile = file;
                                fileName = Path.GetFileNameWithoutExtension(docModel.VidFile.FileName);
                                string extension = Path.GetExtension(docModel.VidFile.FileName);
                                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                                docModel.Video_Url = "~/UploadedVideos/" + fileName;
                            
                                fileName = Path.Combine(Server.MapPath("~/UploadedVideos/"), fileName);
                                docModel.VidFile.SaveAs(fileName);
                                docModel.EnableDisable = true;
                                using (EOTAEntities db5 = new EOTAEntities())
                                {
                              
                                db5.Post_Courses.Add(docModel);
                                db5.SaveChanges();
                            }
                            }

                        }

                        foreach (HttpPostedFileBase file1 in Docimage)
                        {
                            //Checking file is available to save.  
                            if (file1 != null)
                            {
                                string docfileName = "";
                                docModel.Docimage = file1;
                                docfileName = Path.GetFileNameWithoutExtension(docModel.Docimage.FileName);
                                string docextension = Path.GetExtension(docModel.Docimage.FileName);
                                docfileName = docfileName + DateTime.Now.ToString("yymmssfff") + docextension;
                                docModel.Document_Image = "~/Uploaded Images/" + docfileName;
                            //if (cnt > 1)
                            //{
                                docModel.Video_Url = null;
                            //}
                            docModel.Video_Url = null;
                                docfileName = Path.Combine(Server.MapPath("~/Uploaded Images/"), docfileName);


                                docModel.Docimage.SaveAs(docfileName);
                                docModel.EnableDisable = true;
                                using (EOTAEntities db4 = new EOTAEntities())
                                {
                               
                                db4.Post_Courses.Add(docModel);
                                db4.SaveChanges();
                            }

                            }
                        cnt++;
                        }
                        
                        using (EOTAEntities db = new EOTAEntities())
                        {
                            ViewBag.Techcourse = (from a in db.Post_Courses
                                                  where a.Course_Type == "T" && a.EnableDisable == true
                                                  select a.Title).Distinct().ToList();

                            ViewBag.Acacourse = (from a in db.Post_Courses
                                                 where a.Course_Type == "A" && a.EnableDisable == true
                                                 select a.Title).Distinct().ToList();

                            ViewBag.Mngcourse = (from a in db.Post_Courses
                                                 where a.Course_Type == "M" && a.EnableDisable == true
                                                 select a.Title).Distinct().ToList();
                        }
                         msg1= "Your course edited successfully";
                    ViewBag.UploadStatus = "Your course edited successfully";
                    }
                
                else
                {
                     msg1 = "Something is going wrong. Please Try again.";
                    ViewBag.UploadStatus = "Something is going wrong. Please Try again.";
                }
            }
            return RedirectToAction("Edit", new RouteValueDictionary(
                 new { controller = "Training_main", action = "Edit", Id = id ,msg= msg1 }));

        }

        [HttpGet]
        public ActionResult CrsDetails(int id)
        {

            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                if (isadmn() == true)
                {
                    ViewBag.url = "/ADMIN/Dashboard";
                }
                else if (istrnr() == true)
                {
                    ViewBag.url = "/Trainers/Dashboard";
                }
                else
                {
                    ViewBag.url = "#";
                }
                ViewBag.ss = dbModel.Post_Courses.Distinct()
     .Where(i => i.Course_Type == "T")
     .ToArray();
                ViewBag.Techcourse = (from a in dbModel.Post_Courses
                                      where a.Course_Type == "T" && a.EnableDisable == true
                                      select a.Title).Distinct().ToList();

                ViewBag.Acacourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "A" && a.EnableDisable == true
                                     select a.Title).Distinct().ToList();

                ViewBag.Mngcourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "M" && a.EnableDisable == true
                                     select a.Title).Distinct().ToList();
                ViewBag.secretcode = id;

                var admid = (from a in dbModel.Post_Courses
                             where a.Secret_Code == id && a.EnableDisable == true
                             select a.Admin_Id).Distinct().FirstOrDefault();
                ViewBag.admnm = (from a in dbModel.Tbl_ADMN
                                 where a.Admin_id == admid
                                 select a.AdminName).Distinct().FirstOrDefault();

                var trnrid = (from a in dbModel.Post_Courses
                              where a.Secret_Code == id && a.EnableDisable == true
                              select a.Trainer_Id).Distinct().FirstOrDefault();
                ViewBag.trnrnm = (from a in dbModel.Trainers
                                  where a.Id == trnrid
                                  select a.Name).Distinct().FirstOrDefault();

                return View(dbModel.Post_Courses.Where(x => x.Secret_Code == id && x.EnableDisable == true).Distinct().ToList());
            }

        }
       
        public ActionResult CrsDetailsDelete(Int32 id, FormCollection collection)
        {
            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {

                    ViewBag.id = dbModel.Tbl_ADMN.Where(i => i.Admin_EmailId == User.Identity.Name ).Distinct().ToList();
                    //Post_Cours customers = dbModel.Post_Courses.Where(x => x.Secret_Code == id).FirstOrDefault();
                    foreach (var customers in dbModel.Post_Courses.Where(x => x.Secret_Code == id ).ToArray())
                    {
                        dbModel.Post_Courses.Remove(customers);
                        dbModel.SaveChanges();
                    }

                }
                return RedirectToAction("AllCourseView");
            }
            catch
            {
                return View();
            }

        }

        public ActionResult AllCourseView()
        {

            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                if (isadmn() == true)
                {
                    ViewBag.url = "/ADMIN/Dashboard";
                }
                else if (istrnr() == true)
                {
                    ViewBag.url = "/Trainers/Dashboard";
                }
                else
                {
                    ViewBag.url = "#";
                }
                ViewBag.Techcourse = (from a in dbModel.Post_Courses
                                      where a.Course_Type == "T" && a.EnableDisable == true

                                      select a.Title).Distinct().ToList();

                ViewBag.Acacourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "A" && a.EnableDisable == true
                                     select a.Title).Distinct().ToList();

                ViewBag.Mngcourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "M" && a.EnableDisable == true
                                     select a.Title).Distinct().ToList();
                return View(dbModel.Post_Courses.ToList());
            }
        }
        [NonAction]
        public bool isadmn()
        {
            using (EOTAEntities dc = new EOTAEntities())
            {
                var v = dc.Tbl_ADMN.Where(x => x.Admin_EmailId == User.Identity.Name).FirstOrDefault();
                return v != null;
            }
        }
        [NonAction]
        public bool istrnr()
        {
            using (EOTAEntities dc = new EOTAEntities())
            {
                var v = dc.Trainers.Where(x => x.Email_Id == User.Identity.Name).FirstOrDefault();
                return v != null;
            }
        }
        [HttpGet]
        public ActionResult Doc_upload()
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                if(isadmn()==true)
                {
                    ViewBag.url = "/ADMIN/Dashboard";
                }else if(istrnr()==true)
                {
                    ViewBag.url = "/Trainers/Dashboard";
                }
                else
                {
                    ViewBag.url = "#";
                }
                //ViewBag.Teachers = dbModel.Tbl_Doc.ToList();
                //ViewBag.course = dbModel.Post_Courses.ToList();
                var vv = (from a in dbModel.Post_Courses
                                  where a.Course_Type == "T" && a.EnableDisable==true
                                  select new { a.Title, a.Secret_Code } ).Distinct().ToList();
                ViewBag.Techcourse = vv;
                ViewBag.Acacourse = (from a in dbModel.Post_Courses
                                      where a.Course_Type == "A" && a.EnableDisable == true
                                     select new { a.Title, a.Secret_Code }).Distinct().ToList();

                ViewBag.Mngcourse = (from a in dbModel.Post_Courses
                                     where a.Course_Type == "M" && a.EnableDisable == true
                                     select new { a.Title, a.Secret_Code }).Distinct().ToList();
                
                ViewBag.id =(from a in dbModel.Tbl_ADMN
                                                  where a.Admin_EmailId == User.Identity.Name
                                                  select a.Admin_id).Distinct().ToList();
                return View();
            }
           
        }

        [HttpPost]
        public ActionResult Doc_upload(Post_Cours docModel, HttpPostedFileBase[] VidFile, HttpPostedFileBase[] Docimage)
        {


            if (ModelState.IsValid)
            {   //iterating through multiple file collection  
                Random random = new Random();
                int value = random.Next(1, 999999999);
                docModel.Secret_Code = value;
                if (isadmn() == true)
                {
                    ViewBag.url = "/ADMIN/Dashboard";
                }
                else if (istrnr() == true)
                {
                    ViewBag.url = "/Trainers/Dashboard";
                }
                else
                {
                    ViewBag.url = "#";
                }
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {
                    if (docModel.upldr == "A")
                    {
                        
                        var userdetails = dbModel.Tbl_ADMN.Select(z=> new { z.Admin_id, z.Admin_EmailId }).Where(x => x.Admin_EmailId == User.Identity.Name).FirstOrDefault();

                        if (userdetails == null)
                        {
                            ViewBag.UploadStatus = "Invalid credentials, Please Check your details and try again";

                        }
                        else
                        {
                            docModel.Admin_Id = userdetails.Admin_id;
                           
                            foreach (HttpPostedFileBase file in VidFile)
                            {
                                //Checking file is available to save.  
                                if (file != null)
                                {

                                    string fileName = "";
                                    //string fileName1 = "";
                                    docModel.VidFile = file;
                                    fileName = Path.GetFileNameWithoutExtension(docModel.VidFile.FileName);
                                    string extension = Path.GetExtension(docModel.VidFile.FileName);
                                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                                    docModel.Video_Url = "~/UploadedVideos/" + fileName;


                                    fileName = Path.Combine(Server.MapPath("~/UploadedVideos/"), fileName);

                                    docModel.VidFile.SaveAs(fileName);
                                    docModel.EnableDisable = true;
                                    using (EOTAEntities db = new EOTAEntities())
                                    {
                                        db.Post_Courses.Add(docModel);
                                        db.SaveChanges();
                                    }

                                    //assigning file uploaded status to ViewBag for showing message to user.  
                                    //ViewBag.UploadStatus = VidFile.Count().ToString() + " files uploaded successfully.";
                                }

                            }

                            foreach (HttpPostedFileBase file1 in Docimage)
                            {
                                //Checking file is available to save.  
                                if (file1 != null)
                                {
                                    string docfileName = "";
                                    docModel.Docimage = file1;
                                    docfileName = Path.GetFileNameWithoutExtension(docModel.Docimage.FileName);
                                    string docextension = Path.GetExtension(docModel.Docimage.FileName);
                                    docfileName = docfileName + DateTime.Now.ToString("yymmssfff") + docextension;
                                    docModel.Document_Image = "~/Uploaded Images/" + docfileName;
                                    docModel.Video_Url = null;
                                    docfileName = Path.Combine(Server.MapPath("~/Uploaded Images/"), docfileName);


                                    docModel.Docimage.SaveAs(docfileName);
                                    docModel.EnableDisable = true;
                                    using (EOTAEntities db = new EOTAEntities())
                                    {
                                        db.Post_Courses.Add(docModel);
                                        db.SaveChanges();
                                    }

                                    //assigning file uploaded status to ViewBag for showing message to user.  
                                    //ViewBag.UploadStatus = VidFile.Count().ToString() + " files uploaded successfully.";
                                }

                            }



                            using (EOTAEntities db = new EOTAEntities())
                            {
                                ViewBag.Techcourse = (from a in db.Post_Courses
                                                      where a.Course_Type == "T" && a.EnableDisable == true
                                                      select a.Title).Distinct().ToList();

                                ViewBag.Acacourse = (from a in db.Post_Courses
                                                     where a.Course_Type == "A" && a.EnableDisable == true
                                                     select a.Title).Distinct().ToList();

                                ViewBag.Mngcourse = (from a in db.Post_Courses
                                                     where a.Course_Type == "M" && a.EnableDisable == true
                                                     select a.Title).Distinct().ToList();
                            }


                            ViewBag.UploadStatus = "Your course uploaded successfully";
                        }
                    }
                    else if (docModel.upldr == "T")
                    {
                        
                        var userdetails = dbModel.Trainers.Select(z => new { z.Id, z.Email_Id }).Where(x => x.Email_Id == User.Identity.Name).FirstOrDefault();
                        if (userdetails == null)
                        {
                            ViewBag.UploadStatus = "Invalid credentials, Please Check your details and try again";

                        }
                        else
                        {
                            docModel.Trainer_Id = userdetails.Id;

                            foreach (HttpPostedFileBase file in VidFile)
                            {
                                //Checking file is available to save.  
                                if (file != null)
                                {

                                    string fileName = "";
                                    //string fileName1 = "";
                                    docModel.VidFile = file;
                                    fileName = Path.GetFileNameWithoutExtension(docModel.VidFile.FileName);
                                    string extension = Path.GetExtension(docModel.VidFile.FileName);
                                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                                    docModel.Video_Url = "~/UploadedVideos/" + fileName;


                                    fileName = Path.Combine(Server.MapPath("~/UploadedVideos/"), fileName);

                                    docModel.VidFile.SaveAs(fileName);
                                    docModel.EnableDisable = true;
                                    using (EOTAEntities db = new EOTAEntities())
                                    {
                                        db.Post_Courses.Add(docModel);
                                        db.SaveChanges();
                                    }

                                    //assigning file uploaded status to ViewBag for showing message to user.  
                                    //ViewBag.UploadStatus = VidFile.Count().ToString() + " files uploaded successfully.";
                                }

                            }

                            foreach (HttpPostedFileBase file1 in Docimage)
                            {
                                //Checking file is available to save.  
                                if (file1 != null)
                                {
                                    string docfileName = "";
                                    docModel.Docimage = file1;
                                    docfileName = Path.GetFileNameWithoutExtension(docModel.Docimage.FileName);
                                    string docextension = Path.GetExtension(docModel.Docimage.FileName);
                                    docfileName = docfileName + DateTime.Now.ToString("yymmssfff") + docextension;
                                    docModel.Document_Image = "~/Uploaded Images/" + docfileName;
                                    docModel.Video_Url = null;
                                    docfileName = Path.Combine(Server.MapPath("~/Uploaded Images/"), docfileName);


                                    docModel.Docimage.SaveAs(docfileName);
                                    docModel.EnableDisable = true;
                                    using (EOTAEntities db = new EOTAEntities())
                                    {
                                        db.Post_Courses.Add(docModel);
                                        db.SaveChanges();
                                    }

                                    //assigning file uploaded status to ViewBag for showing message to user.  
                                    //ViewBag.UploadStatus = VidFile.Count().ToString() + " files uploaded successfully.";
                                }

                            }



                            using (EOTAEntities db = new EOTAEntities())
                            {
                                ViewBag.Techcourse = (from a in db.Post_Courses
                                                      where a.Course_Type == "T" && a.EnableDisable==true
                                                      select a.Title).Distinct().ToList();

                                ViewBag.Acacourse = (from a in db.Post_Courses
                                                     where a.Course_Type == "A" && a.EnableDisable == true
                                                     select a.Title).Distinct().ToList();

                                ViewBag.Mngcourse = (from a in db.Post_Courses
                                                     where a.Course_Type == "M" && a.EnableDisable == true
                                                     select a.Title).Distinct().ToList();
                            }


                            ViewBag.UploadStatus = "Your course uploaded successfully";
                        }
                    }
                    else
                    {
                        ViewBag.UploadStatus = "Something is going wrong. Please Try again.";
                    }
                }
            }



            ModelState.Clear();
            //ViewBag.UploadStatus = VidFile.Count().ToString() + " files uploaded successfully.";
            return View();
        }


        // [HttpPost]
        public FileResult DownloadFile(string filename)
        {
            //if (Path.GetExtension(filename) == ".png")
            //{
                string fullpath = Path.Combine(Server.MapPath("~/Picture/NIIT.pdf"));
                return File(fullpath, "Picture/pdf");

           // }
            //else
            //{
            //    string k = null;
            //    return new HttpStatusCodeResult(  int.Parse(k));
            //}

        }

            //Response.ContentType = "Application/pdf";
            //Response.AppendHeader("Content-Disposition", "attachment;filename=NIIT.pdf");
            //Response.TransmitFile(Server.MapPath("~/NIIT.pdf"));

            //// Response.End();


            //var FileVirtualPath = "~/NIIT.pdf";
            //return File(FileVirtualPath, "application/force-download", Path.GetFileName(FileVirtualPath));



            //return View();
        //}
        public ActionResult Aca_world()
        {
            string path = Server.MapPath("~/Picture/");
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] files = dirInfo.GetFiles("*.*");
            List<string> lst = new List<string>(files.Length);
            foreach(var item in files)
            {
                lst.Add(item.Name);

            }
            
            return View(lst);
        }

        public ActionResult AllView()
        {

            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                return View(dbModel.Tbl_Doc.ToList());
            }
        }


        [HttpGet]
        public ActionResult Details(int id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                return View(dbModel.Tbl_Doc.Where(x => x.Doc_id == id).FirstOrDefault());
            }

        }

        //public ActionResult Edit(Int32 id)
        //{
        //    using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
        //    {
        //        return View(dbModel.Tbl_Doc.Where(x => x.Doc_id == id).FirstOrDefault());
        //    }
        //}
        //[HttpPost]
        //public ActionResult Edit(Int32 id, Tbl_Doc customer)
        //{
        //    string fileName = Path.GetFileNameWithoutExtension(customer.DocFile.FileName);
        //    string extension = Path.GetExtension(customer.DocFile.FileName);
        //    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
        //    customer.Doc_Path = "~/Picture/" + fileName;
        //    fileName = Path.Combine(Server.MapPath("~/Picture/"), fileName);
        //    customer.DocFile.SaveAs(fileName);

            
        //    string docfileName = Path.GetFileNameWithoutExtension(customer.Docimage.FileName);
        //    string docextension = Path.GetExtension(customer.Docimage.FileName);
        //    docfileName = docfileName + DateTime.Now.ToString("yymmssfff") + docextension;
        //    customer.Doc_ImagePath = "~/Doc File/" + docfileName;
        //    docfileName = Path.Combine(Server.MapPath("~/Doc File/"), docfileName);
        //    customer.Docimage.SaveAs(docfileName);
             
        //    try
        //    {
        //        using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
        //        {
                    
                    
        //            dbModel.Entry(customer).State = System.Data.EntityState.Modified;
        //            dbModel.SaveChanges();

        //        }
        //        return RedirectToAction("AllView");
        //    }
        //    catch (Exception ex)
        //    {
        //        //ModelState.AddModelError("Id Error", "id can not change or invalid credentials");
               
        //        return View();
               
        //    }

        //}

        public ActionResult Delete(Int32 id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                return View(dbModel.Tbl_Doc.Where(x => x.Doc_id == id).FirstOrDefault());
            }
        }
        [HttpPost]
        public ActionResult Delete(Int32 id, FormCollection collection)
        {
            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {
                    Tbl_Doc customers = dbModel.Tbl_Doc.Where(x => x.Doc_id == id).FirstOrDefault();
                    dbModel.Tbl_Doc.Remove(customers);
                    dbModel.SaveChanges();

                }
                return RedirectToAction("AllView");
            }
            catch
            {
                return View();
            }

        }
        //public ActionResult Imageview(int id, string filename)
        //{
        //    Pic_Tbl picmodel = new Pic_Tbl();
        //    using (Pic_Tbl db = new Pic_Tbl())
        //    {
        //        picmodel = db.Pic1_Tbl.Where(x => x.pic_id == id).FirstOrDefault();
        //    }
        //    return View(picmodel);

        //}
        //public FileResult Tech_world()
        //{
        //    Response.ContentType = "Application/pdf";
        //    Response.AppendHeader("Content-Disposition", "attachment;filename=NIIT.pdf");
        //    Response.TransmitFile(Server.MapPath("~/NIIT.pdf"));

        //    // Response.End();


        //    var FileVirtualPath = "~/NIIT.pdf";
        //    return File(FileVirtualPath, "application/force-download", Path.GetFileName(FileVirtualPath));

        //}

        [HttpGet]
        public ActionResult Ved_upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Ved_upload(Tbl_Video VidModel)
        {
            string fileName = "";
            fileName = Path.GetFileNameWithoutExtension(VidModel.VidFile.FileName);
            string extension = Path.GetExtension(VidModel.VidFile.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            VidModel.Video_Path = "~/Video/" + fileName;
            fileName = Path.Combine(Server.MapPath("~/Video/"), fileName);
            VidModel.VidFile.SaveAs(fileName);
           
            using (EOTAEntities db = new EOTAEntities())
            {
                db.Tbl_Video.Add(VidModel);
                db.SaveChanges();
            }
            ModelState.Clear();
            return View();
        }

        public ActionResult Vid_AllView()
        {

            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                return View(dbModel.Tbl_Video.ToList());
            }
        }

        [HttpGet]
        public ActionResult Vid_Details(int id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                return View(dbModel.Tbl_Video.Where(x => x.Video_id == id).FirstOrDefault());
            }

        }

        public ActionResult Vid_Edit(Int32 id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                return View(dbModel.Tbl_Video.Where(x => x.Video_id == id).FirstOrDefault());
            }
        }
        [HttpPost]
        public ActionResult Vid_Edit(Int32 id, Tbl_Video customer)
        {
            string fileName = Path.GetFileNameWithoutExtension(customer.VidFile.FileName);
            string extension = Path.GetExtension(customer.VidFile.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            customer.Video_Path = "~/Video/" + fileName;
            fileName = Path.Combine(Server.MapPath("~/Video/"), fileName);
            customer.VidFile.SaveAs(fileName);

            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {


                    dbModel.Entry(customer).State = EntityState.Modified;
                    dbModel.SaveChanges();

                }
                return RedirectToAction("Vid_AllView");
            }
            catch (Exception ex)
            {
                //ModelState.AddModelError("Id Error", "id can not change or invalid credentials");

                return View();

            }

        }

        public ActionResult Vid_Delete(Int32 id)
        {
            using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
            {
                return View(dbModel.Tbl_Video.Where(x => x.Video_id == id).FirstOrDefault());
            }
        }
        [HttpPost]
        public ActionResult Vid_Delete(Int32 id, FormCollection collection)
        {
            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {
                    Tbl_Video customers = dbModel.Tbl_Video.Where(x => x.Video_id == id).FirstOrDefault();
                    dbModel.Tbl_Video.Remove(customers);
                    dbModel.SaveChanges();

                }
                return RedirectToAction("Vid_AllView");
            }
            catch
            {
                return View();
            }

        }
        public ActionResult Mgmt_world()
        {
            return View();
        }
    }
}