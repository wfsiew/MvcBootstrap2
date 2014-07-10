using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcBootstrap2.Models;
using MvcBootstrap2.ViewModels;
using MvcBootstrap2.Helper;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MvcBootstrap2.Areas.Ng.Controllers
{
    public class NgController : Controller
    {
        //
        // GET: /Ng/Ng/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.menu = "About";
            var students = Student.GetCollection();
            List<EnrollmentDateGroup> v = new List<EnrollmentDateGroup>();
            var l = students.Distinct("EnrollmentDate").OrderBy(x => x.ToUniversalTime());
            foreach (var k in l)
            {
                EnrollmentDateGroup o = new EnrollmentDateGroup();
                o.EnrollmentDate = k.ToUniversalTime();
                var q = Query<Student>.Where(x => x.EnrollmentDate == o.EnrollmentDate);
                o.StudentCount = students.Count(q);
                v.Add(o);
            }

            return Json(v, JsonRequestBehavior.AllowGet);
        }
	}
}