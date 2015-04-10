/*
 * Copyright 2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.api
{
  using Kcsara.Database.Model.Events;
  using Kcsara.Database.Services;
  using log4net;

  public class TrainingController : SarEventsController<Training>
  {
    public TrainingController(ISarEventsService<Training> trainingService, IAuthService auth, ILog log)
      : base(trainingService, auth, log)
    { }
  }
}
