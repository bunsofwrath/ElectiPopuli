using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ElectiPopuli
{
    [DebuggerDisplay("SubFamilies = {SubFamilies.Count}, DirectMembers = {DirectMembers.Count}")]
    class Family : IEnumerable<Family>, IEnumerable<Person>
    {
        public List<Family> SubFamilies { get; set; }
        public List<Person> DirectMembers { get; set; }
        public List<Person> AllMembers
        {
            get
            {
                var members = new List<Person>();
                members.AddRange(this.DirectMembers);

                foreach (var subFamily in this.SubFamilies)
                    members.AddRange(subFamily.AllMembers);

                return members;
            }
        }

        public string Name { get; private set; }

        public Family()
        {
            this.SubFamilies = new List<Family>();
            this.DirectMembers = new List<Person>();
        }

        public void Add(string firstName, string lastName)
        {
            this.Add(new Person() { FirstName = firstName, LastName = lastName });
        }

        public void Add(Person person)
        {
            if (SubFamilies.Any(f => f.AllMembers.Contains(person)))
                throw new InvalidOperationException($"{person} is already a member of a sub-family of this family and cannot be added again.");

            this.DirectMembers.Add(person);
            person.DirectFamily = this;
        }

        public void Add(Family subFamily)
        {
            if (SubFamilies.Any(f => f.AllMembers.Intersect(subFamily.AllMembers).Count() != 0))
                throw new InvalidOperationException($"A member of the sub-family to be added is already a direct member of the family or a member of an existing sub-family.");

            this.SubFamilies.Add(subFamily);
        }

        public Dictionary<Person, Person> PairUp()
        {
            var pairings = new Dictionary<Person, Person>();
            var attempts = 0;
            var maxAttempts = 100;
            var random = new Random();

            while (attempts < maxAttempts)
            {
                pairings = new Dictionary<Person, Person>();

                foreach (var person in this.AllMembers)
                {
                    var candidates = this.AllMembers.Except(person.DirectFamily.DirectMembers)
                                                    .Except(pairings.Values)
                                                    .ToList();

                    if (candidates.Count == 0)
                        break;

                    var index = random.Next(0, candidates.Count);
                    pairings.Add(person, candidates[index]);
                }

                if (pairings.Count == this.AllMembers.Count)
                    return pairings;

                attempts++;
            }

            throw new Exception($"Unable to pair every member after {maxAttempts} attempts.");
        }

        public static IEnumerable<Family> FromXML(string filePath)
        {
            var doc = XDocument.Load(filePath);
            var xFamilies = doc.Element("root")
                               ?.Element("Families")
                               ?.Elements("Family");

            var families = new List<Family>();

            foreach (var xfamily in xFamilies)
                families.Add(Family.FromXML(xfamily));

            return families;
        }

        public static Family FromXML(XElement xfamily)
        {
            var family = new Family();
            
            family.Name = xfamily.Attribute("Name")?.Value;

            family.SubFamilies = xfamily.Elements("Subfamilies")
                                        ?.Elements("Family")
                                        ?.Select(sf => Family.FromXML(sf))
                                        ?.ToList();

            family.DirectMembers = xfamily.Elements("Members")
                                          ?.Elements("Member")
                                          ?.Select(m => new Person()
                                          {
                                              DirectFamily = family,
                                              FirstName = m.Value.Split(' ')[0],
                                              LastName = m.Value.Split(' ')[1]
                                          })
                                          ?.ToList();

            return family;
        }

        #region IEnumerable
        public IEnumerator<Family> GetEnumerator()
        {
            return ((IEnumerable<Family>)this.SubFamilies).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Family>)this.SubFamilies).GetEnumerator();
        }

        IEnumerator<Person> IEnumerable<Person>.GetEnumerator()
        {
            return ((IEnumerable<Person>)this.AllMembers).GetEnumerator();
        }
        #endregion
    }
}
