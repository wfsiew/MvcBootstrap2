using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MvcBootstrap2.Helper;

namespace MvcBootstrap2.Models
{
    public class OfficeAssignment
    {
        public ObjectId Id { get; set; }

        [ForeignKey("Instructor")]
        public ObjectId PersonId { get; set; }

        [StringLength(50)]
        [Display(Name = "Office Location")]
        public string Location { get; set; }

        public Instructor Instructor
        {
            get
            {
                var q = Query<Instructor>.EQ(x => x.Id, PersonId);
                var a = DbHelper.Db.GetCollection<Instructor>("instructors");
                Instructor o = a.FindOne(q);
                return o;
            }
        }
    }
}