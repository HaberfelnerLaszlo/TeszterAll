using Microsoft.EntityFrameworkCore;

using TeszterLibrary.Model;

using TeszterWeb.Data;

namespace TeszterWeb.Services
{
    /// <summary>
    /// A csoportok (osztályok) helyi nyilvántartása.
    /// Minden művelet saját, rövid életű DbContextet kap a gyártótól: Blazor Serveren a scoped DbContext a teljes
    /// kapcsolat (circuit) idejéig élne, így elavult (követett) adatot adna vissza, és két egyidejű hívás
    /// (pl. időzítő + kattintás) "A second operation was started on this context" hibát okozna.
    /// </summary>
    public class GroupsService(IDbContextFactory<TeszterLocalDB> dbFactory)
    {
        public async Task<List<Group>> GetGroupsAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.Groups.AsNoTracking().ToListAsync();
        }

        public async Task<List<Group>> GetGroupsHasLessonsAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.Groups.AsNoTracking().Where(g => g.HasLessons).ToListAsync();
        }

        public async Task<Group?> GetGroupByIdAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<bool> GroupExistsAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.Groups.AnyAsync(g => g.Id == id);
        }

        /// <summary>Megjelöli, hogy a csoportnak van órai feladata. Hamis, ha nincs ilyen csoport.</summary>
        public async Task<bool> SetHasLessonsAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var group = await db.Groups.FindAsync(id);
            if (group is null)
            {
                return false;
            }
            if (!group.HasLessons)
            {
                group.HasLessons = true;
                await db.SaveChangesAsync();
            }
            return true;
        }

        /// <summary>
        /// Az API szerint órai feladattal rendelkező csoportokat megjelöli a helyi nyilvántartásban.
        /// Az ismeretlen azonosítókat kihagyja. Visszaadja, hány csoport változott.
        /// </summary>
        public async Task<int> MarkHasLessonsAsync(IEnumerable<Guid> ids)
        {
            var idSet = ids.ToHashSet();
            if (idSet.Count == 0)
            {
                return 0;
            }
            await using var db = await dbFactory.CreateDbContextAsync();
            var groups = await db.Groups.Where(g => idSet.Contains(g.Id) && !g.HasLessons).ToListAsync();
            groups.ForEach(g => g.HasLessons = true);
            await db.SaveChangesAsync();
            return groups.Count;
        }

        public async Task<Group> AddGroupAsync(Group group)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            db.Groups.Add(group);
            await db.SaveChangesAsync();
            return group;
        }

        /// <summary>Felveszi a csoportot, ha még nincs ilyen azonosítójú. Igaz, ha most került be.</summary>
        public async Task<bool> EnsureGroupAsync(Group group)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            if (await db.Groups.AnyAsync(g => g.Id == group.Id))
            {
                return false;
            }
            db.Groups.Add(group);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<Group?> UpdateGroupAsync(Group group)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var existingGroup = await db.Groups.FindAsync(group.Id);
            if (existingGroup is null)
            {
                return null;
            }
            existingGroup.Name = group.Name;
            existingGroup.Description = group.Description;
            await db.SaveChangesAsync();
            return existingGroup;
        }

        public async Task<bool> DeleteGroupAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var group = await db.Groups.FindAsync(id);
            if (group is null)
            {
                return false;
            }
            db.Groups.Remove(group);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
