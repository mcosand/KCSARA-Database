namespace ExpireNotify
{
    using System;

    public class ParticipatingUnit
    {
        public Guid UnitId { get; set; }
        public string Email { get; set; }
        public bool NotifyMembers { get; set; }
    }
}
