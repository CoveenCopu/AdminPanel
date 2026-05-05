using AdminPanel.Models;

namespace AdminPanel.Services
{
    public interface IUserService
    {
        Role GetUserRole(User user);
    }
}