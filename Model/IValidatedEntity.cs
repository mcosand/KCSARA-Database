
namespace Kcsar.Database.Model
{
    using System.Collections.Generic;

    public interface IValidatedEntity
    {
        IList<RuleViolation> Errors { get; }
        bool Validate();
    }
}
