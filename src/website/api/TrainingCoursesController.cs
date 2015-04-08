/*
 * Copyright 2012-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.api
{
  using Data = Kcsar.Database.Data;
  using Kcsara.Database.Web.Model;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Net;
  using System.Net.Http;
  using System.Web.Http;
  using log4net;
  using Kcsara.Database.Web.api.Models;

  public class TrainingCoursesController : BaseApiController
  {
    public TrainingCoursesController(Data.IKcsarContext db, ILog log)
      : base(db, log)
    { }

    [HttpGet]
    public IEnumerable<TrainingCourse> Get()
    {
      DateTime now = DateTime.Now;
      return GetCourseViews(f => (f.OfferedFrom ?? DateTime.MaxValue) > now, now);
    }

    [HttpGet]
    public IEnumerable<TrainingCourse> GetAll()
    {
      DateTime now = DateTime.Now;
      return GetCourseViews(f => true, now);
    }

    private IEnumerable<TrainingCourse> GetCourseViews(Expression<Func<Data.TrainingCourseRow, bool>> whereClause, DateTime when)
    {
      return db.TrainingCourses
         .Where(whereClause)
         .OrderBy(f => f.DisplayName)
         .Select(TrainingCourse.GetTrainingCourseConversion(when));
    }


    // GET api/<controller>/5
    [HttpGet]
    public TrainingCourse Get(Guid id)
    {
      // Get the data object (if it exists), and pass it through a conversion to the view model. If it doesn't exist, throw a 404 exception
      return GetObjectOrNotFound(
          () => db.TrainingCourses
          .Where(f => f.Id == id)
          .Select(TrainingCourse.GetTrainingCourseConversion(DateTime.Now))
          .SingleOrDefault());
    }
  }
}
