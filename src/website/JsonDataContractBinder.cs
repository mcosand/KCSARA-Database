/*
 * Copyright 2012-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web {
    using System;
    using System.Runtime.Serialization.Json;
    using System.Web.Mvc;
    using System.IO;

    public class JsonDataContractBinder<T> : IModelBinder where T : class
    {
        #region IModelBinder Members

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            if (bindingContext == null)
            {
                throw new ArgumentNullException("bindingContext");
            }

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            controllerContext.HttpContext.Request.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
            return serializer.ReadObject(controllerContext.HttpContext.Request.InputStream);
        }

        #endregion
    }
}
