using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeszterLibrary.Model;

using TeszterWeb.Data;

namespace TeszterWeb.Services
{
    public class GroupsService(TeszterLocalDB db)
    {
        public async Task<List<Group>> GetGroupsAsync()
        {
            return await db.Groups.ToListAsync();
        }
        public async Task<List<Group>> GetGroupsHasLessonsAsync()
        {
            return await db.Groups.Where(g=>g.HasLessons).ToListAsync();
        }
        public async Task<Group?> GetGroupByIdAsync(int id)
        {
            return await db.Groups.FindAsync(id);
        }
        public bool GroupExists(Guid id)
        {
            return db.Groups.Any(g => g.Id == id);
        }
        public bool SetHasLessons(Guid id)
        {
            var group = db.Groups.Find(id);
            if (group == null)
            {
                return false; // Group not found
            }
            group.HasLessons = true;
            db.Groups.Update(group);
            db.SaveChanges();
            return group?.HasLessons ?? false;
        }
        public async Task<Group> AddGroupAsync(Group group)
        {
            db.Groups.Add(group);
            await db.SaveChangesAsync();
            return group;
        }
        public async Task<Group?> UpdateGroupAsync(Group group)
        {
            var existingGroup = await db.Groups.FindAsync(group.Id);
            if (existingGroup == null)
            {
                return null; // Group not found
            }
            existingGroup.Name = group.Name;
            existingGroup.Description = group.Description;
            // Update other properties as needed
            await db.SaveChangesAsync();
            return existingGroup;
        }
        public async Task<bool> DeleteGroupAsync(int id)
        {
            var group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return false; // Group not found
            }
            db.Groups.Remove(group);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
