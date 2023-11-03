using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeszterLibrary.Model
{
    public class Student
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }

        public string FolderId { get; set; } = string.Empty;
        [NotMapped]
        public string AccessToken { get; set; }

    }
}
