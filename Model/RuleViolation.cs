
namespace Kcsar.Database.Model
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [NotMapped]
    public class RuleViolation
    {
        public Guid EntityKey { get; private set; }
        public string PropertyName { get; private set; }
        public string PropertyValue { get; private set; }
        public string ErrorMessage { get; private set; }

        public RuleViolation(Guid entityKey, string propertyName, string value, string message)
        {
            this.EntityKey = entityKey;
            this.PropertyName = propertyName;
            this.PropertyValue = value;
            this.ErrorMessage = message;
        }
    }
}
