using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;

namespace SportsLeague.Domain.Interfaces.Repositories
{
    public interface ISponsorRepository : IGenericRepository<Sponsor>
    {
        Task<IEnumerable<Sponsor>> GetByCategoryAsync(SponsorCategory category);
        Task<Sponsor?> GetByIdWithTournamentAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<Sponsor?> GetByNameAsync(string name);
    }
}
