/*
 * Copyright 2012-2014 Matthew Cosand
 */
using Kcsar.Database.Model;
using Kcsara.Database.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;

namespace Kcsara.Database.Web.api
{
  public class TrainingCoursesController : BaseApiController
  {
    public TrainingCoursesController(IKcsarContext db, ILog log)
      : base(db, log)
    { }

    [HttpGet]
    public IEnumerable<TrainingCourseView> Get()
    {
      DateTime now = DateTime.Now;
      return GetCourseViews(f => (f.OfferedFrom ?? DateTime.MaxValue) > now, now);
    }

    [HttpGet]
    public IEnumerable<TrainingCourseView> GetAll()
    {
      DateTime now = DateTime.Now;
      return GetCourseViews(f => true, now);
    }

    private IEnumerable<TrainingCourseView> GetCourseViews(Expression<Func<TrainingCourse, bool>> whereClause, DateTime when)
    {
      return db.TrainingCourses
         .Where(whereClause)
         .OrderBy(f => f.DisplayName)
         .Select(TrainingCourseView.GetTrainingCourseConversion(when));
    }


    // GET api/<controller>/5
    [HttpGet]
    public TrainingCourseView Get(Guid id)
    {
      // Get the data object (if it exists), and pass it through a conversion to the view model. If it doesn't exist, throw a 404 exception
      return GetObjectOrNotFound(
          () => db.TrainingCourses
          .Where(f => f.Id == id)
          .Select(TrainingCourseView.GetTrainingCourseConversion(DateTime.Now))
          .SingleOrDefault());
    }

    // POST api/<controller>
    public void Post([FromBody]string value)
    {
    }

    // PUT api/<controller>/5
    public void Put(int id, [FromBody]string value)
    {
    }

    // DELETE api/<controller>/5
    public void Delete(int id)
    {
    }
  }
}
