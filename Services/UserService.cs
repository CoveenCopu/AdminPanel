using AdminPanel.Models;

namespace AdminPanel.Services
{
    public class UserService : IUserService
    {
        public Role GetUserRole(User user)
        {
            return user.Role;
        }
    }
}