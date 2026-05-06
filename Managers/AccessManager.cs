using AdminPanel.Models;
using AdminPanel.Services;

namespace AdminPanel.Managers
{
    public class AccessManager
    {
        private readonly IUserService _userService;

        public AccessManager(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public bool CanAccessAdminPanel(User user)
        {
            if (user == null)
                return false;

            return _userService.GetUserRole(user) == Role.Administrator;
        }
    }
}