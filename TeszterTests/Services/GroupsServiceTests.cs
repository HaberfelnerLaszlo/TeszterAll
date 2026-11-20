using TeszterLibrary.Model;

using TeszterTests.Helpers;

using TeszterWeb.Services;

namespace TeszterTests.Services
{
    /// <summary>
    /// A csoport-nyilvántartás. Régen: int azonosítóval keresett a Guid kulcsú táblában (futásidejű ArgumentException),
    /// és a scoped DbContext a Blazor-kapcsolat egész idején élt (elavult adat, ütköző egyidejű műveletek).
    /// </summary>
    public class GroupsServiceTests
    {
        private static Group G(string name, bool hasLessons = false) => new(Guid.NewGuid(), name, null) { HasLessons = hasLessons };

        [Fact]
        public async Task A_group_can_be_found_and_deleted_by_its_guid()
        {
            var a = G("8.a");
            var service = GroupsServiceFactory.Create(a, G("8.b"));

            Assert.Equal("8.a", (await service.GetGroupByIdAsync(a.Id))?.Name);
            Assert.True(await service.DeleteGroupAsync(a.Id));

            Assert.Null(await service.GetGroupByIdAsync(a.Id));
            Assert.False(await service.DeleteGroupAsync(a.Id));
            Assert.Single(await service.GetGroupsAsync());
        }

        [Fact]
        public async Task A_change_made_in_another_scope_is_visible_immediately()
        {
            var a = G("8.a");
            var factory = GroupsServiceFactory.CreateDbFactory(a);
            var projector = new GroupsService(factory);     // pl. a kivetítős nézet kapcsolata
            var teacher = new GroupsService(factory);       // a tanár másik kapcsolata

            Assert.False((await projector.GetGroupsAsync()).Single().HasLessons);
            Assert.True(await teacher.SetHasLessonsAsync(a.Id));

            var offered = await projector.GetGroupsHasLessonsAsync();
            Assert.True(Assert.Single(offered).HasLessons);  // a hosszú életű DbContext a régi, követett példányt adta vissza
            Assert.True((await projector.GetGroupByIdAsync(a.Id))!.HasLessons);
        }

        [Fact]
        public async Task Simultaneous_calls_on_one_instance_do_not_collide()
        {
            var service = GroupsServiceFactory.Create();

            await Task.WhenAll(Enumerable.Range(0, 40).Select(i => Task.Run(async () =>
            {
                await service.EnsureGroupAsync(G($"csoport {i}"));
                await service.GetGroupsAsync();
                await service.GroupExistsAsync(Guid.NewGuid());
            })));

            Assert.Equal(40, (await service.GetGroupsAsync()).Count);
        }

        [Fact]
        public async Task EnsureGroup_adds_only_once()
        {
            var service = GroupsServiceFactory.Create();
            var a = G("8.a");

            Assert.True(await service.EnsureGroupAsync(a));
            Assert.False(await service.EnsureGroupAsync(new Group(a.Id, "másik név", null)));

            Assert.Equal("8.a", Assert.Single(await service.GetGroupsAsync()).Name);
            Assert.True(await service.GroupExistsAsync(a.Id));
        }

        [Fact]
        public async Task MarkHasLessons_marks_known_groups_and_ignores_unknown_ids()
        {
            var a = G("8.a");
            var b = G("8.b", hasLessons: true);
            var c = G("8.c");
            var service = GroupsServiceFactory.Create(a, b, c);

            var changed = await service.MarkHasLessonsAsync([a.Id, b.Id, Guid.NewGuid()]);

            Assert.Equal(1, changed);                        // b már jelölt volt, az ismeretlen kimarad
            var offered = (await service.GetGroupsHasLessonsAsync()).Select(g => g.Name).Order();
            Assert.Equal(["8.a", "8.b"], offered);
            Assert.Equal(0, await service.MarkHasLessonsAsync([]));
        }

        [Fact]
        public async Task SetHasLessons_reports_an_unknown_group()
        {
            var service = GroupsServiceFactory.Create(G("8.a"));

            Assert.False(await service.SetHasLessonsAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Update_changes_name_and_description_only_for_an_existing_group()
        {
            var a = G("8.a");
            var service = GroupsServiceFactory.Create(a);

            var updated = await service.UpdateGroupAsync(new Group(a.Id, "8.A", "új leírás"));

            Assert.Equal("8.A", updated?.Name);
            Assert.Equal("új leírás", (await service.GetGroupByIdAsync(a.Id))?.Description);
            Assert.Null(await service.UpdateGroupAsync(G("ismeretlen")));
        }
    }
}
