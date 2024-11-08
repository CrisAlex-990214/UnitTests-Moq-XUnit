using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IUserRepo
    {
        Task<Guid> CreateUser(User user);
    }
}
