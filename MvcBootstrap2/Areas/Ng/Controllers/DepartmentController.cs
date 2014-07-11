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
    public class DepartmentController : Controller
    {
        //
        // GET: /Ng/Department/
        public ActionResult Index(string sortOrder, string searchString, int? page)
        {
            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            if (searchString != null)
                page = 1;

            MongoCursor<Department> c = null;
            IOrderedEnumerable<Department> el = null;
            var departments = Department.GetCollection();

            if (!string.IsNullOrEmpty(keyword))
            {
                var qi = Query<Instructor>.Where(x => x.FirstMidName.ToUpper().Contains(keyword) ||
                    x.LastName.ToUpper().Contains(keyword));
                var instructors = DbHelper.Db.GetCollection<Instructor>("instructors");
                var i = instructors.Find(qi);
                var iid = i.Select(x => x.Id);

                var q = Query<Department>.Where(x => x.Name.ToUpper().Contains(keyword) ||
                    iid.Contains(x.PersonId));
                c = departments.Find(q);
            }

            else
                c = departments.FindAll();

            int count = Convert.ToInt32(c.Count());

            switch (sortOrder)
            {
                case "Name_desc":
                    el = c.OrderByDescending(x => x.Name);
                    break;

                case "Budget":
                    el = c.OrderBy(x => x.Budget);
                    break;

                case "Budget_desc":
                    el = c.OrderByDescending(x => x.Budget);
                    break;

                case "Date":
                    el = c.OrderBy(x => x.StartDate);
                    break;

                case "Date_desc":
                    el = c.OrderByDescending(x => x.StartDate);
                    break;

                case "Admin":
                    el = c.OrderBy(x => x.Administrator.LastName);
                    break;

                case "Admin_desc":
                    el = c.OrderByDescending(x => x.Administrator.LastName);
                    break;

                default:
                    el = c.OrderBy(x => x.Name);
                    break;
            }

            int pageSize = Constants.PAGE_SIZE;
            int pageNumber = (page ?? 1);
            Pager pager = new Pager(count, pageNumber, pageSize);

            var l = el.Skip(pager.LowerBound).Take(pager.PageSize);
            var lx = l.Select(x => new
            {
                Administrator = new { FullName = x.Administrator == null ? null : x.Administrator.FullName },
                Budget = x.Budget,
                Id = x.Id.ToString(),
                Name = x.Name,
                PersonId = x.PersonId.ToString(),
                RowVersion = x.RowVersion == null ? null : Convert.ToBase64String(x.RowVersion),
                StartDate = x.StartDate
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
            var departments = Department.GetCollection();
            var q = Query<Department>.EQ(x => x.Id, new ObjectId(id));
            Department department = departments.FindOne(q);
            var model = new
            {
                Administrator = new { FullName = department.Administrator == null ? null : department.Administrator.FullName },
                Budget = department.Budget,
                Id = department.Id.ToString(),
                Name = department.Name,
                PersonId = department.PersonId,
                StartDate = department.StartDate
            };

            Dictionary<string, object> res = new Dictionary<string, object>
            {
                { "model", model }
            };

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(DepartmentModel department)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                //throw new Exception("Error" + DateTime.Now);
                if (ModelState.IsValid)
                {
                    Department o = new Department();
                    o.Budget = department.Budget;
                    o.Name = department.Name;
                    o.PersonId = new ObjectId(department.PersonId);
                    o.RowVersion = department.RowVersion;
                    o.StartDate = department.StartDate;
                    var departments = Department.GetCollection();
                    departments.Insert(o);
                    res["success"] = 1;
                    res["message"] = string.Format("{0} has been saved", o.Name);
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
            var departments = Department.GetCollection();
            var q = Query<Department>.EQ(x => x.Id, new ObjectId(id));
            Department department = departments.FindOne(q);
            var o = new
            {
                Budget = department.Budget,
                Id = department.Id.ToString(),
                Name = department.Name,
                PersonId = department.PersonId.ToString(),
                RowVersion = department.RowVersion == null ? null : Convert.ToBase64String(department.RowVersion),
                StartDate = department.StartDate,
                PersonIdList = GetInstructors()
            };
            return Json(o, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit([Bind(Include = "Id, Name, Budget, StartDate, RowVersion, PersonId")] DepartmentModel department)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (ModelState.IsValid)
                {
                    var departments = Department.GetCollection();
                    var q = Query<Department>.EQ(x => x.Id, new ObjectId(department.Id));
                    Department o = departments.FindOne(q);
                    o.Budget = department.Budget;
                    o.Name = department.Name;
                    o.PersonId = new ObjectId(department.PersonId);
                    o.RowVersion = department.RowVersion;
                    o.StartDate = department.StartDate;
                    departments.Save(o);
                    res["success"] = 1;
                    res["message"] = string.Format("{0} has been saved", department.Name);
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
        public ActionResult Delete(List<Department> departments)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                var idlist = departments.Select(x => x.Id).ToList();
                var depts = Department.GetCollection();
                var q = Query<Department>.Where(x => idlist.Contains(x.Id));
                depts.Remove(q);
                res["success"] = 1;
                res["message"] = string.Format("{0} department(s) has been successfully deleted", departments.Count);
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Instructors()
        {
            object o = GetInstructors();
            return Json(o, JsonRequestBehavior.AllowGet);
        }

        private object GetInstructors()
        {
            var instructors = Instructor.GetCollection();
            var c = instructors.FindAll();
            var instructorsQuery = c.OrderBy(x => x.LastName);
            List<Instructor> l = instructorsQuery.ToList();
            var o = l.Select(x => new { PersonId = x.Id.ToString(), FullName = x.FullName });
            return o;
        }
	}
}