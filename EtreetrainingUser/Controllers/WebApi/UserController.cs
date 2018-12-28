
using EtreetrainingUser.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using Zender.Mail;

namespace EtreetrainingUser.Controllers.WebApi
{
    public class UserController : ApiController
    {

        public IHttpActionResult GetUserId(string crrntemail)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    var UID = (from x in dbModel.Tbl_Cand_Main
                               where x.Cand_EmailId == crrntemail
                               select x.Cand_id).Distinct().FirstOrDefault();
                    return Ok(UID);
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        public IHttpActionResult GetUserName([FromUri]string uid)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    var userID = int.Parse(uid);
                    var Uname = (from a in dbModel.Tbl_Cand_Main
                                 where a.Cand_id == userID
                                 select a.CandName).FirstOrDefault();
                    return Ok(Uname);
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        public IHttpActionResult GetUserCrs([FromUri]string id, [FromUri]string searchBy, [FromUri]string search)
        {
            try
            {
                var crstp = (id.Substring(0, id.LastIndexOf("`") + 0));
                var uid = (id.Substring(id.LastIndexOf("`") + 1));
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    if (searchBy == "Course")
                    {
                        return Ok(dbModel.Post_Courses.Where(x => x.Course_Type == crstp && x.Title.StartsWith(search) || x.Title == null).ToList());

                    }
                    else
                    {
                        return Ok(dbModel.Post_Courses.Where(x => x.Course_Type == crstp).ToList());
                    }
                }
            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult GetCrsDetails([FromUri]string id)
        {
            try
            {

                var id1 = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
                int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {

                    return Ok(dbModel.Post_Courses.Where(x => x.Secret_Code == id1 && x.EnableDisable == true).Distinct().ToList());

                }

            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult GetCrsBySecretCD([FromUri]int id)
        {
            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {

                    return Ok(dbModel.Post_Courses.Where(x => x.Secret_Code == id && x.EnableDisable == true).Distinct().ToList());

                }

            }
            catch
            {
                return BadRequest();
            }

        }

        public IHttpActionResult GetCrsNameBySecretCD([FromUri]int scrtcd)
        {
            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {

                    return Ok((from a in dbModel.Post_Courses
                               where a.Secret_Code == scrtcd
                               select a.Course_name).Distinct().FirstOrDefault());

                }

            }
            catch
            {
                return BadRequest();
            }

        }

        public IHttpActionResult GetMockTestQTypeOrData([FromUri]string scrtcd, [FromUri]int Qnumber, [FromUri]string Exmcd, [FromUri]int ExamSecretcode)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    if (ExamSecretcode < 0)
                    {
                        var tsttp = (from x in dbModel.Mock_Tests
                                     where x.Course_Code == scrtcd && x.Exam_Code == Exmcd && x.Ques_No == Qnumber && x.EnableDisable == true
                                     select x.Question_Type).Distinct().FirstOrDefault();
                        if (tsttp != null)
                        {
                            return Content(HttpStatusCode.OK, tsttp);
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, "Question type is not found");
                        }
                    }
                    else
                    {
                        var ans = (from x in dbModel.Use_Mock_Tests
                                   where x.Course_Code == scrtcd && x.Exam_Code == Exmcd && x.Ques_No == Qnumber && x.Exam_Secretcode == ExamSecretcode
                                   select x.Choosed_Option).Distinct().FirstOrDefault();
                        if (ans != null)
                        {
                            return Content(HttpStatusCode.OK, ans);
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, "No options is/are choosed");
                        }
                    }
                }
            }
            catch
            {
                return BadRequest();
            }

        }
         
        public IHttpActionResult GetMockTestUMockTstData([FromUri]string scrtcd, [FromUri]int Qnumber, [FromUri]string Exmcd, [FromUri]int ExamSecretcode, [FromUri]string UmcktstId, [FromUri]string mcktstchk)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    if (UmcktstId.ToLower() == "ok" && mcktstchk.ToLower() == "null" && ExamSecretcode > 0)
                    {
                        var mckid = (from a in dbModel.Use_Mock_Tests
                                     where a.Exam_Code == Exmcd && a.Course_Code == scrtcd
                                     && a.Ques_No == Qnumber && a.Exam_Secretcode == ExamSecretcode
                                     select a.Id).FirstOrDefault();
                        if (mckid != 0)
                        {
                            return Content(HttpStatusCode.OK, mckid);
                        }
                    }
                    else if (UmcktstId.ToLower() == "null" && mcktstchk.ToLower() == "ok" && ExamSecretcode == -1)
                    {
                        var mckdta = dbModel.Mock_Tests.Where(x => x.Exam_Code == Exmcd && x.Course_Code == scrtcd && x.Ques_No == Qnumber).Distinct().FirstOrDefaultAsync();
                        if (mckdta != null)
                        {
                            return Ok();
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        var chsdoption = (from x in dbModel.Use_Mock_Tests
                                          where x.Course_Code == scrtcd && x.Exam_Code == Exmcd
                                         && x.Ques_No == Qnumber
                                          select x.Choosed_Option).Distinct().FirstOrDefault();
                        if (chsdoption != null)
                        {
                            return Content(HttpStatusCode.OK, chsdoption);
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, "No option is choosed");
                        }
                    }
                    return Ok();
                }
            }
            catch
            {
                return BadRequest();
            }

        }

        public IHttpActionResult GetMockTestDatachecking([FromUri]string scrtcd, [FromUri]int Qnumber, [FromUri]string Exmcd, [FromUri]string crctops)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    var marks = dbModel.Mock_Tests.Where(x => x.Exam_Code == Exmcd
                      && x.Course_Code == scrtcd && x.Ques_No == Qnumber && x.Currect_Option == crctops).Distinct().FirstOrDefault();
                    if (marks != null)
                    {
                        return Content(HttpStatusCode.OK, marks);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
            catch
            {
                return BadRequest();
            }

        }

        public IHttpActionResult PostUserFeedback([FromBody]Feedback feedback)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    dbModel.Feedbacks.Add(feedback);
                    dbModel.SaveChanges();
                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }
        public IHttpActionResult PutUserMockTest([FromBody]Use_Mock_Test UMT)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    Use_Mock_Test usemcktst = dbModel.Use_Mock_Tests.Where(z => z.Id == UMT.Id).Distinct().FirstOrDefault();
                    usemcktst.Checked = UMT.Checked;
                    usemcktst.Choosed_Option = UMT.Choosed_Option;
                    usemcktst.Course_Code = UMT.Course_Code;
                    usemcktst.Exam_Code = UMT.Exam_Code;

                    usemcktst.Exam_Date = UMT.Exam_Date;
                    usemcktst.Exam_Secretcode = UMT.Exam_Secretcode;
                    usemcktst.Id = UMT.Id;
                    usemcktst.Marks = UMT.Marks;
                    usemcktst.Ques_No = UMT.Ques_No;
                    usemcktst.User_Email = UMT.User_Email;
                    usemcktst.User_Id = UMT.User_Id;
                    //dbModel.Entry(UMT).State =  EntityState.Modified;
                    dbModel.SaveChanges();
                    return Ok();
                }
            }
            catch(Exception e)
            {
                return BadRequest();
            }

        }
        public IHttpActionResult GetMockTestDataByCrsCdExamCd([FromUri]string scrtcd, [FromUri]int Qnumber, [FromUri]string Exmcd, [FromUri]string Pssmrk)
        {
            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {
                    if (Pssmrk.ToLower() == "nok")
                    {
                        if (Qnumber > 0)
                        {
                            return Ok(dbModel.Mock_Tests.Where(x => x.Ques_No == Qnumber && x.Course_Code == scrtcd && x.Exam_Code == Exmcd).ToList());
                        }
                        else
                        {
                            var qtp = (from x in dbModel.Mock_Tests
                                       where x.Course_Code == scrtcd && x.Exam_Code == Exmcd && x.EnableDisable == true && x.Question_Type == "T"
                                       select x.Question_Type).FirstOrDefault();
                            if (qtp != null)
                            {
                                return Content(HttpStatusCode.OK, "Question type is text");
                            }
                            else
                            {
                                return Content(HttpStatusCode.NoContent, "Question type is not text");
                            }
                        }
                    }
                    else
                    {
                        var pssmrk = (from x in dbModel.Mock_Tests
                                      where x.Course_Code == scrtcd && x.Exam_Code == Exmcd && x.EnableDisable == true
                                      select x.Pass_mark).FirstOrDefault();
                        return Content(HttpStatusCode.OK, pssmrk);
                    }
                }

            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult GetFeedback([FromUri]string secretcd, [FromUri]string UserEmail)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    bool chkfeedbck = (from x in dbModel.Feedbacks
                                       where x.User_Email == UserEmail && x.Course_Code == secretcd && x.IsActive == true
                                       select x).Any();
                    return Content(HttpStatusCode.OK, chkfeedbck);

                }

            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult PostFeedbackOverview([FromUri]string secretcd, [FromUri]string UserEmail, [FromBody]Feedback_overview feedback_Overview)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    feedback_Overview.Course_Code = secretcd;
                    feedback_Overview.User_Email = UserEmail;
                    var chk = (from a in dbModel.Feedbacks
                               where a.Course_Code == secretcd && a.User_Email == UserEmail
                               select a.Id).Any();
                    if (chk == true)
                    {
                        feedback_Overview.IsActive = true;
                        dbModel.Feedback_overviews.Add(feedback_Overview);
                        dbModel.SaveChanges();
                        return Content(HttpStatusCode.OK, "Successfully saved");
                    }
                    else
                    {
                        feedback_Overview.IsActive = false;
                        dbModel.Feedback_overviews.Add(feedback_Overview);
                        dbModel.SaveChanges();
                        return Content(HttpStatusCode.Created, "Successfully saved");
                    }

                }

            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult PostUserTestCount([FromUri]int UserId, [FromUri]string ExamCode, [FromUri]string CourseCode, [FromUri]int ExamSecretcode, [FromUri]decimal Marks, [FromBody]Use_Test_counter use_Test_Counter)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    use_Test_Counter.User_Id = UserId;
                    use_Test_Counter.Exam_Code = ExamCode;
                    use_Test_Counter.Course_Code = CourseCode;

                    use_Test_Counter.Exam_Secretcode = ExamSecretcode;
                    use_Test_Counter.Marks = Marks;
                    use_Test_Counter.IsFinished = true;

                    dbModel.Use_Test_counters.Add(use_Test_Counter);
                    dbModel.SaveChanges();
                    return Content(HttpStatusCode.OK, "Successfully saved");
                }
            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult GetUserTestTotal([FromUri]int UserId, [FromUri]string ExamCode, [FromUri]string CourseCode, [FromUri]int ExamSecretcode)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    int total = 0;
                    var tstdta = (from c in dbModel.Use_Mock_Tests
                                  where c.User_Id == UserId && c.Course_Code == CourseCode && c.Exam_Code == ExamCode && c.Exam_Secretcode == ExamSecretcode
                                  select new { c.Marks }).ToList();
                    foreach (var i in tstdta)
                    {
                        if (i.Marks != null)
                        {
                            total = total + int.Parse((i.Marks).ToString());
                        }
                    }
                    return Content(HttpStatusCode.OK, total);
                }
            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult GetMockTestData([FromUri]string scrtcd, [FromUri]string Qnomax)
        {
            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {
                    if (Qnomax == "OK")
                    {
                        return Ok((from x in dbModel.Mock_Tests
                                   where x.Course_Code == scrtcd && x.EnableDisable == true
                                   select x.Ques_No).Max());

                    }
                    else
                    {
                        return Ok((from x in dbModel.Mock_Tests
                                   where x.Course_Code == scrtcd && x.EnableDisable == true
                                   select new { x.Exam_Code, x.Exam_Time }).Distinct().FirstOrDefault());
                    }
                }

            }
            catch
            {
                return BadRequest();
            }

        }


        public IHttpActionResult GetChkUserFeedback([FromUri]String email)
        {
            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {


                    return Ok((from a in dbModel.Feedback_overviews
                               where a.User_Email == email && a.IsActive == false
                               select a).Any());

                }

            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult GetINSTCrsDetails([FromUri]string id, [FromUri]string instid)
        {
            try
            {
                if (instid != null)
                {
                    int id1 = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
                    int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
                    using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                    {
                        var db = dbModel.Institute_Infoes.Where(x => x.Institute_Id == instid).FirstOrDefault();
                        if (db != null)
                        {
                            bool ishod = (from a in dbModel.Institute_Infoes
                                          where a.Institute_Id == instid && a.IsHOD == true
                                          select a).Distinct().Any();
                            if (ishod == true)
                            {
                                return Ok(dbModel.Post_Courses.Where(x => x.Secret_Code == id1 && x.EnableDisable == true).Distinct().ToList());
                            }
                            else
                            {
                                return Content(HttpStatusCode.Accepted, dbModel.Post_Courses.Where(x => x.Secret_Code == id1 && x.EnableDisable == true).Distinct().ToList());
                            }


                        }
                        else
                        {

                            return new System.Web.Http.Results.ResponseMessageResult(
                          Request.CreateErrorResponse(
                            (HttpStatusCode)404,
                            new HttpError("Code does not match with existing data. Please try again with another.")
                            )
                        );

                        }

                    }
                }
                else
                {

                    return new System.Web.Http.Results.ResponseMessageResult(
                         Request.CreateErrorResponse(
                           (HttpStatusCode)406,
                           new HttpError("Code must not be nothing. Please put a code and try again.")
                           )
                       );
                }
            }
            catch (Exception)
            {
                return new System.Web.Http.Results.ResponseMessageResult(
                        Request.CreateErrorResponse(
                          (HttpStatusCode)400,
                          new HttpError("Something went wrong. Please try again.")
                          )
                      );
            }


        }

        public IHttpActionResult GetInstCrsDiscount([FromUri]string id, [FromUri]string Discountcd)
        {

            try
            {
                using (Models.EOTAEntities dbModel = new Models.EOTAEntities())
                {

                    string instidcd = (id.Substring(id.LastIndexOf("^") + 1));
                    string tst = (id.Substring(0, id.LastIndexOf("^") + 0));
                    int id1 = int.Parse(tst.Substring(tst.LastIndexOf("`") + 1));
                    int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
                    //var dismodel = dbModel.Institute_Infoes.Where(x => x.Promocode == Learning_Point && x.Institute_Id== instidcd);
                    var dismodel = (from a in dbModel.Institute_Infoes
                                    where a.Institute_Id == instidcd && a.Promocode == Discountcd
                                    select a).Distinct().FirstOrDefault();
                    if (dismodel == null)
                    {
                        return Content(HttpStatusCode.NoContent, "Sorry, your entry doesnot match with any data.");
                    }
                    else if (dismodel != null)
                    {
                        double postprice = int.Parse((from a in dbModel.Post_Courses
                                                      where a.Secret_Code == id1
                                                      select a.New_Price).Distinct().FirstOrDefault());
                        double disper = int.Parse((from a in dbModel.Institute_Infoes
                                                   where a.Promocode == Discountcd
                                                   select a.Discount_Percent).Distinct().FirstOrDefault());
                        double amount = postprice * ((disper) / 100);

                        double finalamount = postprice - amount;
                        if (finalamount > -1)
                        {
                            return Ok(finalamount);
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotAcceptable, "Amount cannot be negetive.");
                        }

                    }

                    else
                    {
                        return Content(HttpStatusCode.BadRequest, "Sorry Something went wrong. Please try again.");
                    }
                }
            }
            catch
            {
                return Content(HttpStatusCode.BadRequest, "Sorry Something went wrong. Please try again.");
            }


        }

        public IHttpActionResult GetUserpfcrs([FromUri]string id)
        {
            try
            {
                int id1 = int.Parse(id.Substring(id.LastIndexOf("`") + 1));
                int userid = int.Parse(id.Substring(0, id.LastIndexOf("`") + 0));
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    var uemail = dbModel.Tbl_Cand_Main.Select(a => new { a.Cand_EmailId, a.Cand_id }).Where(z => z.Cand_id == userid).Distinct().FirstOrDefault();
                    return Ok(dbModel.Paid_Courses.Where(y => y.Course_Code == id1 && y.User_EmailId == uemail.Cand_EmailId && y.IsPaid == true).Distinct().FirstOrDefault());
                }
            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult GetUserAsId(int id)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    return Ok(dbModel.Tbl_Cand_Main.Where(x => x.Cand_id == id).FirstOrDefault());
                }
            }
            catch
            {
                return BadRequest();
            }
        }
        public IHttpActionResult GetUserResult()
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    return Ok(dbModel.Paid_Courses_Certificates.ToList());
                }
            }
            catch
            {
                return BadRequest();
            }
        }
        public IHttpActionResult GetUserCrsAll([FromUri]string id, [FromUri]string searchBy, [FromUri]string search)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    if (search == null)
                    {
                        search = "none";
                    }

                    if (search != "none")
                    {
                        if (search.Trim().Length != 0)
                        {

                            return Ok(dbModel.Post_Courses.Where(x => x.Title.StartsWith(search) || x.Title == null).ToList());
                        }
                        else
                        {
                            return Ok(dbModel.Post_Courses.ToList());
                        }
                    }
                    else
                    {
                        return Ok(dbModel.Post_Courses.ToList());
                    }
                }
            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult GetCourseList()
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    return Ok(dbModel.Post_Courses.ToList());
                }
            }
            catch
            {
                return BadRequest();
            }
        }
        public IHttpActionResult GetPaidCourseListByUserId([FromUri]int id)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    return Ok((from x in dbModel.Paid_Courses
                               where x.User_id == id
                               select new { x.Course_Code, x.Course_Name }).Distinct().ToList());
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        public IHttpActionResult GetMockTestsByUserId([FromUri]int id)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    var usrcntr1 = (from p in dbModel.Mock_Tests
                                    where p.IsMocktest == true
                                    select p.Course_Code).Distinct().ToList();
                    foreach (var i in usrcntr1)
                    {
                        var usrcntr = (from p in dbModel.Use_Test_counters
                                       where p.Course_Code == i && p.User_Id == id
                                       select p.Id).Distinct().Count();
                        if (usrcntr == 3)
                        {
                            usrcntr1 = (from p in usrcntr1
                                        where p != i
                                        select p).Distinct().ToList();
                        }
                    }
                    return Ok(usrcntr1);
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        public IHttpActionResult PostUserLogin([FromBody]Tbl_Cand_Main cand)
        {
            try
            {
                foreach (var key in ModelState.Keys)
                {
                    if (key == "Cand_Pass" || key == "Cand_EmailId")
                    {

                    }
                    else
                    {
                        ModelState[key].Errors.Clear();
                    }

                }

                if (ModelState.IsValid)
                {
                    using (EOTAEntities dbModel = new EOTAEntities())
                    {
                        var userdetails = dbModel.Tbl_Cand_Main.Where(x => x.Cand_EmailId == cand.Cand_EmailId && x.Cand_Pass == cand.Cand_Pass).FirstOrDefault();


                        if (userdetails == null)
                        {

                            return new System.Web.Http.Results.ResponseMessageResult(
                            Request.CreateErrorResponse(
                              (HttpStatusCode)401,
                              new HttpError("Email and password does not match with existing data.")
                              )
                          );
                        }
                        else
                        {
                            if (userdetails.Cand_IsEmailVerified == true)
                            {

                                return new System.Web.Http.Results.ResponseMessageResult(
                            Request.CreateErrorResponse(
                              (HttpStatusCode)200,
                              new HttpError("Thanks for Login.")
                              )
                            );
                            }
                            else
                            {
                                return new System.Web.Http.Results.ResponseMessageResult(
                          Request.CreateErrorResponse(
                            (HttpStatusCode)302,
                            new HttpError("Your Account till now not verified. Please verify your email id. Link is sent to your registered email id.")
                            )
                        );

                            }

                        }
                    }
                }

                else
                {

                    return new System.Web.Http.Results.ResponseMessageResult(
                          Request.CreateErrorResponse(
                            (HttpStatusCode)406,
                            new HttpError("Plesae check your credentials again.")
                            )
                        );
                }
            }
            catch (Exception ex)
            {

                return new System.Web.Http.Results.ResponseMessageResult(
                         Request.CreateErrorResponse(
                           (HttpStatusCode)400,
                           new HttpError("Something went wrong or plesae check your credentials again.")
                           )
                       );
            }
        }

        public IHttpActionResult PostUser([FromBody]Tbl_Cand_Main userModel)
        {
            bool Status = false;
            string Message = "";
            if (ModelState.IsValid)
            {
                var isExist = IsEmailExist(userModel.Cand_EmailId);
                if (isExist)
                {
                    return new System.Web.Http.Results.ResponseMessageResult(
                Request.CreateErrorResponse(
                    (HttpStatusCode)302,
                    new HttpError("Email have already exists")
                )
            );

                }

                userModel.Cand_ActivationCode = Guid.NewGuid();

                Guid? candidate_reference = Guid.NewGuid();
                userModel.cand_ReferenceCode = candidate_reference;

                userModel.Cand_IsEmailVerified = false;
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    dbModel.Tbl_Cand_Main.Add(userModel);
                    dbModel.SaveChanges();
                    SendVerificationlinkEmail(userModel.Cand_EmailId, userModel.Cand_ActivationCode.ToString());

                    Message = "Registration Successfully done. Account activation link has been sent to your email id:" + userModel.Cand_EmailId;
                    Status = true;
                    return new System.Web.Http.Results.ResponseMessageResult(
               Request.CreateErrorResponse(
                   (HttpStatusCode)201,
                   new HttpError("Successfully Created")
               ));

                }
            }
            else
            {
                return BadRequest("Something went wrong.");
            }

        }
        public IHttpActionResult PostUserReferral([FromBody]Tbl_Cand_Main customer, [FromUri]int id)
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                var referenceemailfrom = (from c in dbModel.Tbl_Cand_Main
                                          where c.Cand_id == id
                                          select c.Cand_EmailId).FirstOrDefault();
                if (dbModel.Tbl_Cand_Main.Any(x => x.Cand_id == customer.Cand_id))
                {
                    if (customer.inputEmail == null)
                    {
                        return new System.Web.Http.Results.ResponseMessageResult(
                         Request.CreateErrorResponse(
                           (HttpStatusCode)204,
                           new HttpError("Blank email. Please give a email-id")
                           )
                       );

                    }
                    else if (customer.inputEmail == referenceemailfrom)
                    {
                        return new System.Web.Http.Results.ResponseMessageResult(
                         Request.CreateErrorResponse(
                           (HttpStatusCode)406,
                           new HttpError("Sorry! You can not refer yourself.Try to proceed with another Email Id")
                           )
                       );
                    }
                    else
                    {
                        try
                        {
                            var refencecode = (from c in dbModel.Tbl_Cand_Main
                                               where c.Cand_id == id
                                               select c.cand_ReferenceCode).FirstOrDefault();
                            SendVerificationlinkEmail(customer.Cand_id, customer.inputEmail, referenceemailfrom.ToString(), refencecode.ToString());
                            SendVerificationlinkEmailrefrfromemail(customer.Cand_id, customer.inputEmail, referenceemailfrom.ToString(), refencecode.ToString());


                            return Ok();

                        }

                        catch (Exception ex)
                        {
                            ModelState.AddModelError("Id Error", "Invalid Credentials, Please check it.");

                            return new System.Web.Http.Results.ResponseMessageResult(
                        Request.CreateErrorResponse(
                          (HttpStatusCode)400,
                          new HttpError("Something went wrong or plesae check your credentials again.")
                          )
                      );

                        }
                    }

                }
                else
                {
                    return new System.Web.Http.Results.ResponseMessageResult(
                        Request.CreateErrorResponse(
                          (HttpStatusCode)203,
                          new HttpError("sorry! You are not authenticated for this service.")
                          )
                      );

                }
            }

        }

        public IHttpActionResult PostCrsDiscount([FromUri]string id, [FromBody]Post_Cours pc)
        {
            int id1 = int.Parse(id.ToString());
       
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                try
                {
                    try
                    {
                        var lp = Guid.Parse(pc.Learning_Point);
                        var refmodel = dbModel.Tbl_Cand_Main.Where(x => x.cand_ReferenceCode == lp);
                        if (refmodel == null)
                        {
                            return new System.Web.Http.Results.ResponseMessageResult(
                        Request.CreateErrorResponse(
                          (HttpStatusCode)404,
                          new HttpError("Sorry, wrong entry. Please check your code and try again.")
                          )
                      );
                        }
                        else
                        {
                            var postprice = int.Parse((from a in dbModel.Post_Courses
                                                       where a.Secret_Code == id1
                                                       select a.New_Price).Distinct().FirstOrDefault());
                            var disper = int.Parse((from a in dbModel.Discount_Calculators
                                                    where a.Discount_Code == pc.Learning_Point
                                                    select a.Discount_Price).Distinct().FirstOrDefault());
                            var distm = (from a in dbModel.Discount_Calculators
                                         where a.Discount_Code == pc.Learning_Point
                                         select a.Disable_Date).Distinct().FirstOrDefault();

                            if (distm != null)
                            {
                                if (distm < DateTime.Now)
                                {
                                    return new System.Web.Http.Results.ResponseMessageResult(
                      Request.CreateErrorResponse(
                        (HttpStatusCode)406,
                        new HttpError("Sorry your code is expired.")
                        )
                    );

                                }
                                else
                                {
                                    var amount = postprice * ((disper) / 100);
                                    var finalamount = postprice - amount;
                                    if (finalamount != 0)
                                    {
                                        return Ok(finalamount);

                                    }
                                }
                            }

                        }
                    }
                    catch
                    {
                        var dismodel = dbModel.Discount_Calculators.Where(x => x.Discount_Code == pc.Learning_Point);
                        if (dismodel == null)
                        {
                            return new System.Web.Http.Results.ResponseMessageResult(
                    Request.CreateErrorResponse(
                      (HttpStatusCode)404,
                      new HttpError("Sorry, wrong entry. Please check your code and try again.")
                      )
                  );
                        }
                        else
                        {
                            double postprice = int.Parse((from a in dbModel.Post_Courses
                                                          where a.Secret_Code == id1
                                                          select a.New_Price).Distinct().FirstOrDefault());
                            double disper = int.Parse((from a in dbModel.Discount_Calculators
                                                       where a.Discount_Code == pc.Learning_Point
                                                       select a.Discount_Price).Distinct().FirstOrDefault());
                            var distm = (from a in dbModel.Discount_Calculators
                                         where a.Discount_Code == pc.Learning_Point
                                         select a.Disable_Date).Distinct().FirstOrDefault();


                            if (distm != null)
                            {
                                if (distm < DateTime.Now)
                                {
                                    return new System.Web.Http.Results.ResponseMessageResult(
                      Request.CreateErrorResponse(
                        (HttpStatusCode)406,
                        new HttpError("Sorry your code is expired.")
                        )
                    );
                                }
                                else
                                {
                                    double amount = postprice * ((disper) / 100);

                                    double finalamount = postprice - amount;
                                    if (finalamount != 0)
                                    {
                                        return Ok(finalamount);
                                    }
                                }
                            }


                        }
                    }

                }
                catch (Exception)
                {
                    return new System.Web.Http.Results.ResponseMessageResult(
                       Request.CreateErrorResponse(
                         (HttpStatusCode)400,
                         new HttpError("Something went wrong. Please try again.")
                         )
                     );

                }
                return new System.Web.Http.Results.ResponseMessageResult(
                    Request.CreateErrorResponse(
                      (HttpStatusCode)400,
                      new HttpError("Something went wrong. Please check your code and try again.")
                      )
                  );
            }

        }
        public IHttpActionResult GetUserById(string Email)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    var idchk = (from a in dbModel.Tbl_Cand_Main
                                 where a.Cand_EmailId == Email
                                 select a.Cand_id).Distinct().FirstOrDefault();


                    return Ok(dbModel.Tbl_Cand_Main.Where(x => x.Cand_id == idchk).FirstOrDefault());


                }
            }
            catch
            {
                return BadRequest();
            }
        }
        public IHttpActionResult PutUserDetails([FromBody]Tbl_Cand_Main customer, [FromUri]int id)
        {
            try
            {
                foreach (var key in ModelState.Keys)
                {
                    if (key == "customer.Cand_MNum" || key == "customer.Cand_RePass" || key == "customer.Cand_EmailId")
                    {
                        ModelState[key].Errors.Clear();
                    }
                    else { }

                }

                if (ModelState.IsValid)
                {
                    var isExist = IsEmailExist(customer.Cand_EmailId);
                    //isExist = true;
                    if (isExist)
                    {
                        using (EOTAEntities dbModel = new EOTAEntities())
                        {
                            customer.Cand_EmailId = (from a in dbModel.Tbl_Cand_Main
                                                     where a.Cand_id == id
                                                     select a.Cand_EmailId).FirstOrDefault();

                            customer.Cand_id = id;
                            if (dbModel.Tbl_Cand_Main.Any(x => x.Cand_EmailId == customer.Cand_EmailId && x.Cand_Pass == customer.Cand_Pass && x.Cand_id == customer.Cand_id))
                            {
                                if (customer.Cand_ResetPassCode == null)
                                {

                                    customer.Cand_RePass = customer.Cand_Pass;

                                    customer.Cand_ResetPassCode = null;
                                    customer.Cand_IsEmailVerified = true;
                                    customer.Cand_ActivationCode = Guid.NewGuid();

                                    Guid? candidate_reference = Guid.NewGuid();
                                    customer.cand_ReferenceCode = candidate_reference;
                                    dbModel.Entry(customer).State = EntityState.Modified;
                                    dbModel.SaveChanges();
                                    SendVerificationlinkEmail(customer.Cand_EmailId);


                                    ModelState.Clear();
                                    return Ok(customer);

                                }
                                else
                                {
                                    customer.Cand_Pass = customer.Cand_ResetPassCode;

                                    customer.Cand_RePass = customer.Cand_ResetPassCode;
                                    try
                                    {
                                        customer.Cand_ResetPassCode = null;
                                        customer.Cand_IsEmailVerified = true;
                                        customer.Cand_ActivationCode = Guid.NewGuid();

                                        Guid? candidate_reference = Guid.NewGuid();
                                        customer.cand_ReferenceCode = candidate_reference;
                                        dbModel.Entry(customer).State = EntityState.Modified;
                                        dbModel.SaveChanges();
                                        SendVerificationlinkEmail(customer.Cand_EmailId);
                                        ModelState.Clear();
                                        return Ok(customer);
                                    }
                                    catch (Exception)
                                    {

                                        ModelState.AddModelError("Id Error", "Something went in problem. Please try again");
                                        return new System.Web.Http.Results.ResponseMessageResult(
                      Request.CreateErrorResponse(
                        (HttpStatusCode)304,
                        new HttpError("Something went wrong or plesae check your credentials again.")
                        )
                    );

                                    }
                                }
                            }
                            else
                            {

                                return new System.Web.Http.Results.ResponseMessageResult(
                       Request.CreateErrorResponse(
                         (HttpStatusCode)403,
                         new HttpError("Existing email and password does not match.")
                         )
                     );

                            }
                        }
                    }
                    else
                    {

                        return new System.Web.Http.Results.ResponseMessageResult(
                    Request.CreateErrorResponse(
                      (HttpStatusCode)404,
                      new HttpError("Email not found. Please try again...")
                      )
                  );
                    }
                }
                else
                {
                    return new System.Web.Http.Results.ResponseMessageResult(
                       Request.CreateErrorResponse(
                         (HttpStatusCode)406,
                         new HttpError("Model invalid, cannot be acceptable. Please try again.")
                         )
                     );
                }

            }
            catch
            {
                return new System.Web.Http.Results.ResponseMessageResult(
                       Request.CreateErrorResponse(
                         (HttpStatusCode)417,
                         new HttpError("Something went wrong. The server cannot meet the requirements.")
                         )
                     );
            }
        }
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
        public void SendVerificationlinkEmail(string emailId)
        {

            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(emailId);

            message.From = from;

            message.To.Add(to);
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                //var server = Request.RequestUri.Segments;
                //var verifyUrl = "/Home/Index/";
                //var link = Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.PathAndQuery, verifyUrl);

                message.Subject = "Your EOTA account credentials are changed";

                message.Body = "<br/><br/>Congrates you have successfully changed your EOTA account credentials. " +

                " <br /><br />" + " Not You? Please Let us know. You can mail at hr@elephanttreetech.com or info @elephanttreetech.com";

                message.IsBodyHtml = true;

                message.SendMailAsync();
            }

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
            var server = Request.RequestUri.Segments;


            var link = Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.AbsoluteUri, "https://www.etreetraining.com");


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
            var server = Request.RequestUri.Segments;
            //var verifyUrl = "/user/verifyaccount/" + myreferencecd;
            //var link = Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.PathAndQuery, verifyUrl);
            //var url = "~/Home/Index";
            //var click = "click here";
            //var click1 = " ~/Home /Index";
            //var link1 = Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.PathAndQuery, "/Home/Index");
            message.Subject = "Reference successfull to EOTA Training Application";

            message.Body = "Congrates You refered successfully your friend " + emailId + " to EOTA Online Training Application. " +
                "<br />Thank you for your this step. We already sent a email with your refernce id (" + myreferencecd + "). " +

                 "<br />We glad to let you know that we will give you discount for your course study after your friend's registration and payment" +
                " <br />Thank You and please be with us." +
                " <br />If you facing any problem, Please mail at hr@elephanttreetech.com or info@elephanttreetech.com .We will be always available for you and give support happily";

            message.IsBodyHtml = true;

            message.SendMailAsync();

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

                var server = Request.RequestUri.Segments;

                //  var verifyUrl = HttpContext.Request. + activationCode;
                var verifyUrl = "https://www.etreetraining.com/Student/Semail5467verification/" + activationCode;
                //var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
                var link = Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.AbsoluteUri, verifyUrl);

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

    }
}
