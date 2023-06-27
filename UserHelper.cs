using System.Security.Claims;

namespace Hajusly;

public class UserHelper {
    public int GetCurrentUserId(ClaimsPrincipal user) {
        string userId = user.Claims.FirstOrDefault(e => (e.Subject != null) && e.Subject.Claims.Any(r => r.Type.Equals("UserId")))?.Value ?? "";
        int.TryParse(userId, out int x);
        return x;
    }
}