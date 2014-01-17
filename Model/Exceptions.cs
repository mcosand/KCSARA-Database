/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;

    public class RuleViolationException : Exception
    {
        public RuleViolationException(string message) : base(message) { }
    }
    
    public class RuleViolationsException : Exception
    {
        public IList<RuleViolation> Errors { get; private set; }

        public RuleViolationsException(IList<RuleViolation> errors) : base("Multiple errors found")
        {
            this.Errors = errors;
        }
    }


    public class RosterOverlapException : RuleViolationException
    {
        public RosterOverlapException() : base("Member already on a roster at this time") { }
    }

    public class NegativeTimeException : RuleViolationException
    {
        public NegativeTimeException() : base("Time In must be before Time Out") { }
    }
}
