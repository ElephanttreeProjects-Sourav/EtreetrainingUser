using EtreetrainingUser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtreetrainingUser.Controllers
{
    [RequireHttps]
    [HandleError]
    [ValidateInput(false)]
    public class PaymentController : Controller
    {
        // GET: Payment

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Success(string id, Paid_Cours pc)
        {
            try
            {
                var id1 = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
                int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    var usermodel = dbModel.Tbl_Cand_Main.Where(z => z.Cand_id == userid).FirstOrDefault();
                    if (usermodel != null)
                    {
                        var coursename = (from x in dbModel.Post_Courses
                                          where x.Secret_Code == id1
                                          select x.Course_name).Distinct().FirstOrDefault();
                        var useremail = (from x in dbModel.Tbl_Cand_Main
                                         where x.Cand_id == userid
                                         select x.Cand_EmailId).Distinct().FirstOrDefault();
                        bool chkprsntdata = dbModel.Paid_Courses.Where(z => z.Course_Code == id1 && z.User_EmailId == useremail).Any();
                        if (chkprsntdata == false)
                        {
                            pc.Course_Code = id1;
                            pc.Course_Name = coursename;
                            pc.User_id = userid;
                            pc.User_EmailId = useremail;
                            pc.IsPaid = true;
                            dbModel.Paid_Courses.Add(pc);
                            dbModel.SaveChanges();
                        }
                        ViewBag.coursename = coursename;
                        ViewBag.userid = userid;
                    }
                    else
                    {
                        var coursename = (from x in dbModel.Post_Courses
                                          where x.Secret_Code == id1
                                          select x.Course_name).Distinct().FirstOrDefault();
                        ViewBag.coursename = coursename;
                        ViewBag.error = "You did not login or registered. Sorry to say You cannot access the course. Please contact with Admin. Thank You";
                    }
                }

                return View();
            }
            catch(Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }

        }

        public ActionResult Successinst(string id, Paid_Cours pc,Institute_Paymnt ip)
        {
            try
            {
                string instidcd = (id.Substring(id.LastIndexOf("^") + 1));
                string tst = (id.Substring(0, id.LastIndexOf("^") + 0));
                int id1 = int.Parse(tst.Substring(tst.LastIndexOf("`") + 1));
                int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    var usermodel = dbModel.Tbl_Cand_Main.Where(z => z.Cand_id == userid).FirstOrDefault();
                    if (usermodel != null)
                    {
                        var instchecking = dbModel.Institute_Infoes.Where(z => z.Institute_Id == instidcd).FirstOrDefault();
                        if (instchecking != null)
                        {
                            var coursename = (from x in dbModel.Post_Courses
                                              where x.Secret_Code == id1
                                              select x.Title).Distinct().FirstOrDefault();
                            var useremail = (from x in dbModel.Tbl_Cand_Main
                                             where x.Cand_id == userid
                                             select x.Cand_EmailId).Distinct().FirstOrDefault();
                            pc.Course_Code = id1;
                            pc.Course_Name = coursename;
                            pc.User_id = userid;
                            pc.User_EmailId = useremail;
                            pc.IsPaid = true;
                            ip.Institute_Id = instidcd;
                            ip.IsPaid_ = true;
                            ip.Course_Code = id1.ToString();
                            dbModel.Paid_Courses.Add(pc);
                            dbModel.SaveChanges();
                            dbModel.Institute_Paymnts.Add(ip);
                            dbModel.SaveChanges();

                            ViewBag.coursename = coursename;
                            ViewBag.userid = userid;
                        }else
                        {
                            var coursename = (from x in dbModel.Post_Courses
                                              where x.Secret_Code == id1
                                              select x.Title).Distinct().FirstOrDefault();
                            ViewBag.coursename = coursename;
                            ViewBag.error = "You have wrong Institute ID Code. Please contact with Admin. Thank You";
                        }
                           
                    }
                    else
                    {
                        var coursename = (from x in dbModel.Post_Courses
                                          where x.Secret_Code == id1
                                          select x.Title).Distinct().FirstOrDefault();
                        ViewBag.coursename = coursename;
                        ViewBag.error = "You did not login or registered. Sorry to say You cannot access the course. Please contact with Admin. Thank You";
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }

        }
    }
}