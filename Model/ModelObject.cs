using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsar.Database.Model
{
    public abstract class ModelObject : IModelObject
    {
        public DateTime LastChanged { get; set; }
        public string ChangedBy { get; set; }
        public Guid Id { get; set; }

        protected List<RuleViolation> errors = new List<RuleViolation>();


        public ModelObject()
        {
            this.Id = Guid.NewGuid();
        }
        
        public abstract string GetReportHtml();

        public IList<RuleViolation> Errors
        {
            get { return errors.AsReadOnly(); }
        }

        public virtual bool Validate()
        {
            return true;
        }
    }
}
