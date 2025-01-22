
namespace TeszterLibrary.Model
{
    /// <summary>
    /// A tesztben használt kérdések. A pontszámot és az időt lehet módosítani. Külön mentődik az eredetitől.
    /// </summary>
    public class TestQuestion
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public Guid QuestionId { get; set; }
        public int Score { get; set; }
        public int Time { get; set; }
        //public Test Test { get; set; }
        public Question Question { get; set; }
        /// <summary>
        /// A szakasz azonosítója 0 esetén teszt nincs szakaszolva. A 0 az alapéertelmezett érték.
        /// </summary>
        public int SectionId { get; set; } = 0;

    }
}
