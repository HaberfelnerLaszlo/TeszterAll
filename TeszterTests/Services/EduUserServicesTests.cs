using System.Net;

using TeszterLibrary.Services;

using TeszterTests.Helpers;

namespace TeszterTests.Services
{
    /// <summary>
    /// A felhasználó létezésének ellenőrzése. Régen a teljes felhasználólistát töltötte le (GET User) — diáknál is —, és
    /// abban kereste a saját azonosítóját; a GET User mostantól tanári végpont, ezért az adott rekordot kéri (GET User/{id}).
    /// </summary>
    public class EduUserServicesTests
    {
        private static readonly Guid Id = Guid.Parse("6f1c4f0e-8f5d-4a52-9d0f-2f0d8b1a1c11");

        private static (ApiTestHost Host, EduUserServices Service) Arrange(HttpStatusCode status, string? json = null)
        {
            var host = new ApiTestHost();
            host.Handler.RespondWith(status, json);
            return (host, new EduUserServices(host.Api));
        }

        [Fact]
        public async Task An_existing_user_is_found_by_its_own_record()
        {
            var (host, service) = Arrange(HttpStatusCode.OK, $$"""{"id":"{{Id}}","displayName":"Diák"}""");
            using var _ = host;

            Assert.True(await service.UserExits(Id));
            Assert.Equal($"https://api.test/User/{Id}", Assert.Single(host.Handler.Requests).Uri);   // nem a teljes lista
        }

        [Fact]
        public async Task A_missing_user_is_reported_as_not_existing_without_an_error()
        {
            var (host, service) = Arrange(HttpStatusCode.NotFound);
            using var _ = host;

            Assert.False(await service.UserExits(Id));
            Assert.Equal(string.Empty, service.ErrorMessage);     // a hívó ilyenkor felveheti (AddUser)
        }

        [Theory]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Forbidden)]
        public async Task Other_failures_set_an_error_so_the_caller_does_not_add_a_duplicate(HttpStatusCode status)
        {
            var (host, service) = Arrange(status);
            using var _ = host;

            Assert.False(await service.UserExits(Id));
            Assert.NotEqual(string.Empty, service.ErrorMessage);
        }
    }
}
