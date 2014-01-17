/*
 * Copyright 2010-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using System.Runtime.Serialization;
    using Kcsar.Database.Model;

    [DataContract]
    public class SubmitResult<T>
    {
        [DataMember]
        public T Result { get; set; }

        [DataMember]
        public SubmitError[] Errors { get; set; }

        public SubmitResult()
        {
            this.Errors = new SubmitError[0];
        }
    }

    [DataContract]
    public class SubmitError
    {
        [DataMember]
        public string Property { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public Guid[] Id { get; set; }
    }
}
