using BillByte.Repositories.Interface;
using System.Security.Claims;

public static class TableAccessHelper
{
    public static async Task<bool> ValidateAsync(
        HttpContext context,
        IUserTableAssignmentRepository repo,
        int tablePreferenceId)
    {
        var role = context.User.FindFirst(ClaimTypes.Role)!.Value;
        if (role == "Owner" || role == "Admin")
            return true;

        var restaurantId = int.Parse(
            context.User.FindFirst("restaurantId")!.Value
        );

        var userId = int.Parse(
            context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        return await repo.HasAccessAsync(
            restaurantId,
            userId,
            tablePreferenceId);
    }
}
