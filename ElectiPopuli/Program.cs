using System.Collections.Generic;
using System.Configuration;

namespace ElectiPopuli
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFile = ConfigurationManager.AppSettings["FamilyMemberFile"];
            var families = Family.FromXML(inputFile);
            var pairings = new Dictionary<string, Dictionary<Person, Person>>();

            foreach (var family in families)
                pairings[family.Name] = family.PairUp();
        }
    }
}