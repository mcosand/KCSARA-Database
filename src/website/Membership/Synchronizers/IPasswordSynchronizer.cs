﻿/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Membership
{
    public interface IPasswordSynchronizer
    {
        void SetPassword(string username, string newPassword);      
    }
}
