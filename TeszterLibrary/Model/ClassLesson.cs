using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TeszterLibrary.Model
{
    public class ClassLesson
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Guid ClassId { get; set; }
        [ForeignKey("TaskId")]
        public int TaskId { get; set; }
        /// <summary>
        /// Az adott feladatból mennyi kell.
        /// </summary>
        public byte TaskCount { get; set; }
        /// <summary>
        /// Hányszor használták ezt az adott osztályban
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Az utolsó használat időpontja
        /// </summary>
        public DateTime LastDate { get; set; }
        /// <summary>
        /// Az utolsó módosítás időpontja
        /// </summary>
        public DateTime Update { get; set; }

    }
}
