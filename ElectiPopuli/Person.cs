using System.Diagnostics;

namespace ElectiPopuli
{
    [DebuggerDisplay("{FirstName,nq} {LastName,nq}")]
    class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Family DirectFamily { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != typeof(Person))
                return false;

            return this.Equals((Person)obj);
        }

        public bool Equals(Person p)
        {
            return this.FirstName == p.FirstName
                && this.LastName == p.LastName;
        }
    }
}