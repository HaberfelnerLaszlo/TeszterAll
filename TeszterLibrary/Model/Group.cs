using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeszterLibrary.Model
{
    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }=string.Empty;
        public bool HasLessons { get; set; } = false;
        public List<string> Members { get; set; }= [];
        public Group(Guid id,string name, string? description)
        {
            Id = id;
            Name = name;
            Description = description??string.Empty;
            Members = new List<string>();
        }
        public void AddMember(string member)
        {
            if (!Members.Contains(member))
            {
                Members.Add(member);
            }
        }
        public void RemoveMember(string member)
        {
            Members.Remove(member);
        }
    }
}
