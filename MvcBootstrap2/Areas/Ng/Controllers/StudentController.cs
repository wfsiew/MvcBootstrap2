using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcBootstrap2.Models;
using MvcBootstrap2.Helper;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using PagedList;

namespace MvcBootstrap2.Areas.Ng.Controllers
{
    public class StudentController : Controller
    {
        //
        // GET: /Ng/Student/
        public ActionResult Index(string sortOrder, string searchString, int? page)
        {
            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            if (searchString != null)
                page = 1;

            MongoCursor<Student> c = null;
            IOrderedEnumerable<Student> el = null;
            var students = Student.GetCollection();

            if (!string.IsNullOrEmpty(keyword))
            {
                var q = Query<Student>.Where(x => x.LastName.ToUpper().Contains(keyword) ||
                    x.FirstMidName.ToUpper().Contains(keyword));
                c = students.Find(q);
            }

            else
                c = students.FindAll();

            switch (sortOrder)
            {
                case "Name_desc":
                    el = c.OrderByDescending(x => x.LastName);
                    break;

                case "FirstName":
                    el = c.OrderBy(x => x.FirstMidName);
                    break;

                case "FirstName_desc":
                    el = c.OrderByDescending(x => x.FirstMidName);
                    break;

                case "Date":
                    el = c.OrderBy(x => x.EnrollmentDate);
                    break;

                case "Date_desc":
                    el = c.OrderByDescending(x => x.EnrollmentDate);
                    break;

                default:
                    el = c.OrderBy(x => x.LastName);
                    break;
            }

            int pageSize = Constants.PAGE_SIZE;
            int pageNumber = (page ?? 1);

            var l = el.ToPagedList(pageNumber, pageSize);
            var lx = l.Select(x => new
            {
                EnrollmentDate = x.EnrollmentDate,
                FirstMidName = x.FirstMidName,
                LastName = x.LastName,
                PersonID = x.Id.ToString()
            });
            Pager pager = new Pager(l.TotalItemCount, l.PageNumber, l.PageSize);
            Dictionary<string, object> res = new Dictionary<string, object>
            {
                { "pager", pager },
                { "model", lx }
            };

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(string id)
        {
            var students = Student.GetCollection();
            var q = Query<Student>.EQ(x => x.Id, new ObjectId(id));
            Student student = students.FindOne(q);
            var enrollments = student.Enrollments.Select(x => new
            {
                Course = new Course { Title = x.Course.Title },
                Grade = x.Grade == null ? Enrollment.NO_GRADE : Enum.GetName(typeof(Grade), x.Grade)
            });

            var model = new
            {
                EnrollmentDate = student.EnrollmentDate,
                FirstMidName = student.FirstMidName,
                FullName = student.FullName,
                LastName = student.LastName,
                PersonID = student.Id.ToString(),
                Enrollments = enrollments
            };

            Dictionary<string, object> res = new Dictionary<string, object>
            {
                { "model", model },
                { "enrollments", enrollments }
            };

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "LastName, FirstMidName, EnrollmentDate")] StudentModel student)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                //throw new Exception("Error" + DateTime.Now);
                if (ModelState.IsValid)
                {
                    Student o = new Student();
                    o.EnrollmentDate = student.EnrollmentDate;
                    o.FirstMidName = student.FirstMidName;
                    o.LastName = student.LastName;
                    var students = Student.GetCollection();
                    students.Insert(o);
                    res["success"] = 1;
                    res["message"] = string.Format("{0} has been saved", o.FullName);
                }
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(string id)
        {
            var students = Student.GetCollection();
            var q = Query<Student>.EQ(x => x.Id, new ObjectId(id));
            Student student = students.FindOne(q);
            StudentModel o = new StudentModel
            {
                EnrollmentDate = student.EnrollmentDate,
                FirstMidName = student.FirstMidName,
                LastName = student.LastName,
                Id = student.Id.ToString()
            };
            return Json(o, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(StudentModel student)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (ModelState.IsValid)
                {
                    var students = Student.GetCollection();
                    var q = Query<Student>.EQ(x => x.Id, new ObjectId(student.Id));
                    Student o = students.FindOne(q);
                    o.EnrollmentDate = student.EnrollmentDate;
                    o.FirstMidName = student.FirstMidName;
                    o.LastName = student.LastName;
                    students.Save(o);
                    res["success"] = 1;
                    res["message"] = string.Format("{0} has been saved", o.FullName);
                }
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(List<string> ids)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                var idlist = ids.Select(x => new ObjectId(x)).ToList();
                var students = Student.GetCollection();
                var q = Query<Student>.Where(x => idlist.Contains(x.Id));
                students.Remove(q);
                res["success"] = 1;
                res["message"] = string.Format("{0} student(s) has been successfully deleted", ids.Count);
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }
	}
}