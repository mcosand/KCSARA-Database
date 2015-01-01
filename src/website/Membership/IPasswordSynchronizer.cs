﻿/*
 * Copyright 2009-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kcsar.Membership
{
    public interface IPasswordSynchronizer
    {
        void SetPassword(string username, string newPassword);
    }
}
