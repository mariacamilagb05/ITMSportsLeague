using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SportsLeague.DataAccess.Repositories
{
    public class SponsorRepository : GenericRepository<Sponsor>, ISponsorRepository
    {
        public SponsorRepository(LeagueDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _dbSet.AnyAsync(s => s.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Sponsor>> GetByCategoryAsync(SponsorCategory category)
        {
            return await _dbSet.Where(s => s.Category == category).ToListAsync();
        }

        public async Task<Sponsor?> GetByIdWithTournamentAsync(int id)
        {
            return await _dbSet
                .Where(s => s.Id == id)
                .Include(s => s.TournamentSponsors)
                    .ThenInclude(ts => ts.Tournament)
                .FirstOrDefaultAsync();
        }

        public async Task<Sponsor?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
        }
    }
}
