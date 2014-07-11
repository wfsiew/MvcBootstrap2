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
using PagedList;

namespace MvcBootstrap2.Areas.Ng.Controllers
{
    public class InstructorController : Controller
    {
        //
        // GET: /Ng/Instructor/
        public ActionResult Index(string sortOrder, string searchString, int? page)
        {
            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            InstructorIndexData viewModel = new InstructorIndexData();

            if (searchString != null)
                page = 1;

            MongoCursor<Instructor> c = null;
            IOrderedEnumerable<Instructor> el = null;
            var instructors = Instructor.GetCollection();

            if (!string.IsNullOrEmpty(keyword))
            {
                var q = Query<Instructor>.Where(x => x.LastName.ToUpper().Contains(keyword) ||
                    x.FirstMidName.ToUpper().Contains(keyword));
                c = instructors.Find(q);
            }

            else
                c = instructors.FindAll();

            int count = Convert.ToInt32(c.Count());

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
                    el = c.OrderBy(x => x.HireDate);
                    break;

                case "Date_desc":
                    el = c.OrderByDescending(x => x.HireDate);
                    break;

                case "Loc":
                    el = c.OrderBy(x => x.OfficeAssignment.Location);
                    break;

                case "Loc_desc":
                    el = c.OrderByDescending(x => x.OfficeAssignment.Location);
                    break;

                default:
                    el = c.OrderBy(x => x.LastName);
                    break;
            }

            int pageSize = Constants.PAGE_SIZE;
            int pageNumber = (page ?? 1);
            Pager pager = new Pager(count, pageNumber, pageSize);

            var l = el.Skip(pager.LowerBound).Take(pager.PageSize);
            var lx = l.Select(x => new
            {
                Id = x.Id.ToString(),
                LastName = x.LastName,
                FirstMidName = x.FirstMidName,
                HireDate = x.HireDate,
                OfficeAssignment = new { Location = x.OfficeAssignment == null ? null : x.OfficeAssignment.Location },
                Courses = x.Courses == null ? null : x.Courses.Select(k => new { CourseID = k.CourseID, Title = k.Title })
            });
            
            Dictionary<string, object> res = new Dictionary<string, object>
            {
                { "pager", pager },
                { "model", lx }
            };

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Courses(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var q = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
                var instructor = Instructor.GetCollection().FindOne(q);
                var courses = instructor.Courses;
                var lx = courses.Select(x => new
                {
                    Id = x.Id.ToString(),
                    PersonID = id,
                    CourseID = x.CourseID,
                    Title = x.Title,
                    Department = new { Name = x.Department == null ? null : x.Department.Name }
                });

                return Json(lx, JsonRequestBehavior.AllowGet);
            }

            return Json(new List<byte>(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Enrollments(string id, string courseID)
        {
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(courseID))
            {
                var qi = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
                var instructor = Instructor.GetCollection().FindOne(qi);
                var enrollments = instructor.Courses.Where(x => x.Id == new ObjectId(courseID))
                    .Single().Enrollments;
                var lx = enrollments.Select(x => new
                {
                    Student = new { FullName = x.Student == null ? null : x.Student.FullName },
                    Grade = x.Grade == null ? Enrollment.NO_GRADE : Enum.GetName(typeof(Grade), x.Grade)
                });

                return Json(lx, JsonRequestBehavior.AllowGet);
            }

            return Json(new List<byte>(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(string id)
        {
            var instructors = Instructor.GetCollection();
            var q = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
            Instructor instructor = instructors.FindOne(q);
            var model = new
            {
                FirstMidName = instructor.FirstMidName,
                FullName = instructor.FullName,
                HireDate = instructor.HireDate,
                LastName = instructor.LastName,
                OfficeAssignment = new { Location = instructor.OfficeAssignment == null ? null : instructor.OfficeAssignment.Location },
                Id = instructor.Id.ToString()
            };

            Dictionary<string, object> res = new Dictionary<string, object>
            {
                { "model", model }
            };

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(InstructorModel instructor, string[] selectedCourses)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (TryUpdateModel(instructor, "",
                    new string[] { "LastName", "FirstMidName", "HireDate", "OfficeAssignment" }))
                {
                    Instructor o = new Instructor();
                    o.FirstMidName = instructor.FirstMidName;
                    o.HireDate = instructor.HireDate;
                    o.LastName = instructor.LastName;

                    if (selectedCourses == null)
                        o.CourseIdList = new List<ObjectId>();

                    else
                        o.CourseIdList = selectedCourses.Select(k => new ObjectId(k)).ToList();

                    var instructors = Instructor.GetCollection();
                    instructors.Insert(o);

                    var officeAssignments = DbHelper.Db.GetCollection<OfficeAssignment>("officeassignments");
                    OfficeAssignment oa = new OfficeAssignment();
                    oa.PersonId = o.Id;
                    oa.Location = instructor.OfficeAssignment.Location;
                    officeAssignments.Insert(oa);

                    instructors.Save(o);
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
            var instructors = Instructor.GetCollection();
            var q = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
            Instructor instructor = instructors.FindOne(q);

            var o = new
            {
                Courses = GetCourses(instructor),
                FirstMidName = instructor.FirstMidName,
                HireDate = instructor.HireDate,
                LastName = instructor.LastName,
                OfficeAssignment = new { Location = instructor.OfficeAssignment == null ? null : instructor.OfficeAssignment.Location },
                Id = instructor.Id.ToString()
            };
            return Json(o, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(string id, string[] selectedCourses)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                var instructors = Instructor.GetCollection();
                var q = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
                Instructor instructorToUpdate = instructors.FindOne(q);
                InstructorModel m = new InstructorModel();

                if (TryUpdateModel(m, "",
                    new string[] { "LastName", "FirstMidName", "HireDate", "OfficeAssignment" }))
                {
                    if (selectedCourses == null)
                        instructorToUpdate.CourseIdList = new List<ObjectId>();

                    else
                        instructorToUpdate.CourseIdList = selectedCourses.Select(k => new ObjectId(k)).ToList();

                    instructorToUpdate.LastName = m.LastName;
                    instructorToUpdate.FirstMidName = m.FirstMidName;
                    instructorToUpdate.HireDate = m.HireDate;

                    var officeAssignments = DbHelper.Db.GetCollection<OfficeAssignment>("officeassignments");
                    var qo = Query<OfficeAssignment>.EQ(x => x.PersonId, instructorToUpdate.Id);
                    OfficeAssignment oa = officeAssignments.FindOne(qo);

                    if (oa != null)
                    {
                        if (string.IsNullOrWhiteSpace(m.OfficeAssignment.Location))
                        {
                            officeAssignments.Remove(qo);
                        }

                        else
                        {
                            oa.Location = m.OfficeAssignment.Location;
                            officeAssignments.Save(oa);
                        }
                    }

                    else
                    {
                        if (!string.IsNullOrWhiteSpace(m.OfficeAssignment.Location))
                        {
                            oa = new OfficeAssignment();
                            oa.PersonId = instructorToUpdate.Id;
                            oa.Location = m.OfficeAssignment.Location;
                            officeAssignments.Insert(oa);
                        }
                    }

                    instructors.Save(instructorToUpdate);
                    res["success"] = 1;
                    res["message"] = string.Format("{0} has been saved", instructorToUpdate.FullName);
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
                var instructors = Instructor.GetCollection();
                var q = Query<Instructor>.Where(x => idlist.Contains(x.Id));
                instructors.Remove(q);
                res["success"] = 1;
                res["message"] = string.Format("{0} instructor(s) has been successfully deleted", ids.Count);
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AllCourses()
        {
            List<AssignedCourseData> o = GetCourses();
            return Json(o, JsonRequestBehavior.AllowGet);
        }

        private List<AssignedCourseData> GetCourses(Instructor instructor = null)
        {
            MongoCursor<Course> mc = Course.GetCollection().FindAll();
            List<Course> allCourses = mc.ToList();
            HashSet<ObjectId> instructorCourses = null;

            if (instructor != null)
                instructorCourses = new HashSet<ObjectId>(instructor.Courses.Select(c => c.Id));

            List<AssignedCourseData> viewModel = new List<AssignedCourseData>();
            foreach (Course course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    CourseId = course.Id.ToString(),
                    Title = course.Title,
                    Assigned = instructor == null ? false : instructorCourses.Contains(course.Id)
                });
            }

            return viewModel;
        }
	}
}