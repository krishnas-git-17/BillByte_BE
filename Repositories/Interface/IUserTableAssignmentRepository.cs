using Billbyte_BE.Models;

namespace BillByte.Repositories.Interface
{
    public interface IUserTableAssignmentRepository
    {
        Task AssignAsync(UserTableAssignment item);

        Task<List<TablePreference>> GetSectionsForUserAsync(
            int restaurantId,
            int userId);

        Task<List<int>> GetAssignedSectionIdsAsync(
           int restaurantId,
           int userId
       );

        Task<bool> HasAccessAsync(
            int restaurantId,
            int userId,
            int tablePreferenceId);
    }
}
