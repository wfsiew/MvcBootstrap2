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
    public class CourseController : Controller
    {
        //
        // GET: /Ng/Course/
        public ActionResult Index(string sortOrder, string searchString, int? page)
        {
            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            if (searchString != null)
                page = 1;

            MongoCursor<Course> c = null;
            IOrderedEnumerable<Course> el = null;
            var courses = Course.GetCollection();

            if (!string.IsNullOrEmpty(keyword))
            {
                var qd = Query<Department>.Where(x => x.Name.Contains(keyword));
                var departments = DbHelper.Db.GetCollection<Department>("departments");
                var d = departments.Find(qd);
                var did = d.Select(x => x.Id);

                var q = Query<Course>.Where(x => x.Title.Contains(keyword) ||
                    did.Contains(x.DepartmentId));
                c = courses.Find(q);
            }

            else
                c = courses.FindAll();

            int count = Convert.ToInt32(c.Count());

            switch (sortOrder)
            {
                case "Title_desc":
                    el = c.OrderByDescending(x => x.Title);
                    break;

                case "Dept":
                    el = c.OrderBy(x => x.Department.Name);
                    break;

                case "Dept_desc":
                    el = c.OrderByDescending(x => x.Department.Name);
                    break;

                case "CourseID":
                    el = c.OrderBy(x => x.CourseID);
                    break;

                case "CourseID_desc":
                    el = c.OrderByDescending(x => x.CourseID);
                    break;

                case "Credits":
                    el = c.OrderBy(x => x.Credits);
                    break;

                case "Credits_desc":
                    el = c.OrderByDescending(x => x.Credits);
                    break;

                default:
                    el = c.OrderBy(x => x.Title);
                    break;
            }

            int pageSize = Constants.PAGE_SIZE;
            int pageNumber = (page ?? 1);
            Pager pager = new Pager(count, pageNumber, pageSize);

            var l = el.Skip(pager.LowerBound).Take(pager.PageSize);
            var lx = l.Select(x => new
            {
                Id = x.Id.ToString(),
                CourseID = x.CourseID,
                Credits = x.Credits,
                Department = new { Name = x.Department == null ? null : x.Department.Name },
                Title = x.Title
            });
            
            Dictionary<string, object> res = new Dictionary<string, object>
            {
                { "pager", pager },
                { "model", lx }
            };

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(string id)
        {
            var courses = Course.GetCollection();
            var q = Query<Course>.EQ(x => x.Id, new ObjectId(id));
            Course course = courses.FindOne(q);
            var model = new
            {
                Id = course.Id.ToString(),
                CourseID = course.CourseID,
                Credits = course.Credits,
                Department = new { Name = course.Department == null ? null : course.Department.Name },
                Title = course.Title
            };

            Dictionary<string, object> res = new Dictionary<string, object>
            {
                { "model", model }
            };

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "CourseID,Title,Credits,DepartmentId")] CourseModel course)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                //throw new Exception("Error" + DateTime.Now);
                if (ModelState.IsValid)
                {
                    Course o = new Course();
                    o.CourseID = course.CourseID;
                    o.Title = course.Title;
                    o.Credits = course.Credits;
                    o.DepartmentId = new ObjectId(course.DepartmentId);
                    var courses = Course.GetCollection();
                    courses.Insert(o);
                    res["success"] = 1;
                    res["message"] = string.Format("{0} has been saved", course.Title);
                }

                else
                {
                    return Json(ModelState, JsonRequestBehavior.AllowGet);
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
            var courses = Course.GetCollection();
            var q = Query<Course>.EQ(x => x.Id, new ObjectId(id));
            Course course = courses.FindOne(q);
            var o = new
            {
                Id = course.Id.ToString(),
                CourseID = course.CourseID,
                Credits = course.Credits,
                DepartmentId = course.DepartmentId.ToString(),
                Title = course.Title,
                DepartmentIdList = GetDepartments()
            };
            return Json(o, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(CourseModel course)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (ModelState.IsValid)
                {
                    var courses = Course.GetCollection();
                    var q = Query<Course>.EQ(x => x.Id, new ObjectId(course.Id));
                    Course o = courses.FindOne(q);
                    o.CourseID = course.CourseID;
                    o.Credits = course.Credits;
                    o.DepartmentId = new ObjectId(course.DepartmentId);
                    o.Title = course.Title;
                    courses.Save(o);
                    res["success"] = 1;
                    res["message"] = string.Format("{0} has been saved", course.Title);
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
                var courses = Course.GetCollection();
                var q = Query<Course>.Where(x => idlist.Contains(x.Id));
                courses.Remove(q);
                res["success"] = 1;
                res["message"] = string.Format("{0} course(s) has been successfully deleted", ids.Count);
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Departments()
        {
            object o = GetDepartments();
            return Json(o, JsonRequestBehavior.AllowGet);
        }

        private object GetDepartments()
        {
            var departments = Department.GetCollection().FindAll().OrderBy(x => x.Name);
            List<Department> l = departments.ToList();
            var o = l.Select(x => new { DepartmentId = x.Id.ToString(), Name = x.Name });
            return o;
        }
	}
}