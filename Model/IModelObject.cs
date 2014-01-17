/*
 * Copyright 2008-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
    using System;

    public interface IModelObject : IValidatedEntity
    {
        DateTime LastChanged { get; set; }
        string ChangedBy { get; set; }
        Guid Id { get; }
        string GetReportHtml();
    }
}
