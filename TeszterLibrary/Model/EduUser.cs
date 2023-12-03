using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeszterLibrary.Model
{
    public class EduUser
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
        public string FolderId { get; set; } = string.Empty;
    }
    public class Student:EduUser
    {
        [NotMapped]
        public string AccessToken { get; set; }=string.Empty;

    }
}
