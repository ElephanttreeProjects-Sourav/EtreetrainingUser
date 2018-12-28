using EtreetrainingUser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using System.Web.Security;
using Zender.Mail;

namespace EtreetrainingUser.Controllers.WebApi
{
    public class HomeController : ApiController
    {
        public IHttpActionResult GetDiscountCoderunning()
        {
            FormsAuthentication.SignOut();
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                var discount = (from s in dbModel.Discount_Calculators
                                where s.Disable_Date > DateTime.Now
                                select new { s.Discount_Code, s.Disable_Date, s.Discount_Price }).Distinct().FirstOrDefault();
                return Ok(discount);
            }
        }
        public IHttpActionResult GetNEETCourses()
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                return Ok(dbModel.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET" && x.EnableDisable == true).Select(z=>z.Secret_Code).Distinct().FirstOrDefault());
            }
        }
        public IHttpActionResult GetNEETCoursesDetails()
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                return Ok(dbModel.Post_Courses.Where(x => x.Course_name.ToUpper() == "NEET" && x.EnableDisable == true).Distinct().ToList());
            }
        }
        public IHttpActionResult GetAcademicCourses()
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                return Ok(dbModel.Post_Courses.Where(x => x.Course_Type == "A" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList());
            }
        }
        public IHttpActionResult GetTechnicalCourses()
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                return Ok(dbModel.Post_Courses.Where(x => x.Course_Type == "T" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList());
            }
        }
        public IHttpActionResult GetManagementCourses()
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                return Ok(dbModel.Post_Courses.Where(x => x.Course_Type == "M" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList());
            }
        }

        public IHttpActionResult GetNEETcourseWithSearch(string search)
        {
            if (search != null)
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    return Ok(dbModel.Post_Courses.Where(x => x.Course_name.ToUpper()==search.ToUpper() && x.EnableDisable == true).Select(z=>z.Secret_Code).Distinct().FirstOrDefault());
                }
            }
            else
            {

                return BadRequest();
            }
        }
        public IHttpActionResult GetAcademicCoursesWithSearch(string search)
        {
            if (search != null)
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    return Ok(dbModel.Post_Courses.Where(x => x.Title.StartsWith(search) && x.Course_Type == "A" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList());
                }
            }else
            {
               
                return BadRequest();
            }
        }
      
        public IHttpActionResult GetTechnicalCoursesWithSearch(string search)
        {
            if (search != null)
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    return Ok(dbModel.Post_Courses.Where(x => x.Title.StartsWith(search) && x.Course_Type == "T" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList());
                }
            }else
            {
                return BadRequest();
            }
        }
        public IHttpActionResult GetManagementCoursesWithSearch(string search)
        {
            if (search != null)
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {
                    return Ok(dbModel.Post_Courses.Where(x => x.Title.StartsWith(search) && x.Course_Type == "M" && x.EnableDisable == true).OrderBy(x => x.Secret_Code).Distinct().ToList());
                }
            }else
            {
                return BadRequest();
            }
        }
        public IHttpActionResult GetUsers()
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                return Ok((from b in dbModel.Tbl_Cand_Main

                           select b).ToList());
            }
        }
        public IHttpActionResult GetUserFeedbacks()
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                return Ok((from a in dbModel.Feedbacks
                           where a.IsActive == true
                           orderby a.Id descending
                           select a).Take(3).ToList());
            }
        }
        public IHttpActionResult GetClearUnusedTests()
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                var usrcntr1 = (from s in dbModel.Use_Test_counters
                                select s).Distinct().ToList();

                var usrcntr = (
                               from sa in dbModel.Use_Mock_Tests

                               select sa).Distinct().ToList();

                var result = usrcntr.Where(p => !usrcntr1.Any(p2 => p2.Exam_Secretcode == p.Exam_Secretcode)).ToList();

                foreach (var vp in result)
                {
                    dbModel.Use_Mock_Tests.Remove(vp);
                    dbModel.SaveChanges();
                }

                return Ok("Successfully cleared");
            }

        }
        public IHttpActionResult GetDemoVideos()
        {
            using (EOTAEntities dbModel = new EOTAEntities())
            {
                return Ok(dbModel.Tbl_Video.ToList());
            }
        }

        public IHttpActionResult GetCrsByCode([FromUri]int id)
        {
            try
            {
                using (EOTAEntities dbModel = new EOTAEntities())
                {

                    return Ok(dbModel.Post_Courses.Where(x => x.Secret_Code == id && x.EnableDisable == true).Distinct().ToList());

                }

            }
            catch
            {
                return BadRequest();
            }

        }
        public IHttpActionResult GetSubscriptionMail([FromUri]string Email,[FromUri]string message)
        {
            SendEmail(Email);
            if (message != null)
            {
                SendEmail(Email, message);
            }
            return Ok();
        }
        [NonAction]
        public void SendEmail(string email, string Message1)
        {
            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress("etreetraining@gmail.com");

            message.From = from;

            message.To.Add(to);
            using (EOTAEntities dbModel = new EOTAEntities())
            {

                var server = Request.RequestUri.Segments;

                message.Subject = "User subscription";

                message.Body = "<br/>User email: " + email +

                    "<br/>" + Message1;

                message.IsBodyHtml = true;

                message.SendMailAsync();

            }

        }

        [NonAction]
        public void SendEmail(string email)
        {
            ZenderMessage message = new ZenderMessage("eaf7f170-f8e8-4391-8e5b-eb1919fca608");

            MailAddress from = new MailAddress("etreetraining@gmail.com");

            MailAddress to = new MailAddress(email);

            message.From = from;

            message.To.Add(to);
            using (EOTAEntities dbModel = new EOTAEntities())
            {

                var server = Request.RequestUri.Segments;

                message.Subject = "EOTA subscription successfull";

                message.Body = "Thank You for your subscription." +
                    "<br/><br/>You will recieve our latest news";


                message.IsBodyHtml = true;

                message.SendMailAsync();

            }

        }
    }
}
