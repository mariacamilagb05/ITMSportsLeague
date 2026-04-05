using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ILogger<SponsorService> _logger;
        
        public SponsorService(
            ISponsorRepository sponsorRepository, 
            ITournamentSponsorRepository tournamentSponsorRepository, 
            ITournamentRepository tournamentRepository,
            ILogger<SponsorService> logger)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
            _tournamentRepository = tournamentRepository;
            _logger = logger;
        }
        public async Task AddSponsorToTournamentAsync(int tournamentId, int sponsorId, decimal contractAmount)
        {
            //Validar que el patrocinador existe
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null)
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID {sponsorId}");

            //Validar que el torneo existe
            var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
            if (!tournamentExists)
                throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");

            //Validar que no esté ya vinculado
            var existing = await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if(existing != null)            
                throw new InvalidOperationException("Este sponsor ya está vinculado al torneo");

            //Validación del negocio: ContractAmount > 0
            if (contractAmount <= 0)
            {
                _logger.LogWarning("Contract amount must be greater than zero for sponsor '{SponsorName}'", sponsor.Name);
                throw new InvalidOperationException("El monto del contrato debe ser mayor que cero");
            }

            var tournamentSponsor = new TournamentSponsor
            {
                TournamentId = tournamentId,
                SponsorId = sponsorId,
                ContractAmount = contractAmount,
                JoinedAt = DateTime.UtcNow,
            };

            _logger.LogInformation("Adding sponsor {SponsorId} to tournament {TournamentId}", sponsorId, tournamentId);
            await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
        }

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            //Validación del negocio: nombre único
            if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
            {
                _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.Name);
                throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
            }
            //Validación del negocio: ContactEmail válido
            try
            {
                var email = new System.Net.Mail.MailAddress(sponsor.ContactEmail);
            }
            catch
            {
                _logger.LogWarning("Invalid contact email '{ContactEmail}' for sponsor '{SponsorName}'", sponsor.ContactEmail, sponsor.Name);
                throw new InvalidOperationException("El correo electrónico de contacto no es válido");
            }

            _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _sponsorRepository.GetByIdAsync(id);
            if(existing == null)                            
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID {id}");
            _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
            await _sponsorRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Sponsor>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all sponsors");
            return await _sponsorRepository.GetAllAsync();
        }

        public async Task<Sponsor?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
            var sponsor = await _sponsorRepository.GetByIdWithTournamentAsync(id);
            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with ID: {SponsorId} not found", id);
            }
            return sponsor;
        }

        public async Task<IEnumerable<Tournament>> GetTournamentsBySponsorAsync(int sponsorId)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null)
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

            var tournamentSponsors = await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);

            return tournamentSponsors.Select(ts => ts.Tournament);
        }

        public async Task RemoveSponsorFromTournamentAsync(int tournamentSponsorId)
        {
            //Validar que la relación existe
            var existing = await _tournamentSponsorRepository
                .GetByIdAsync(tournamentSponsorId);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró la relación con ID {tournamentSponsorId}");

            _logger.LogInformation("Removing sponsor {SponsorId} from tournament {TournamentId} ", existing.SponsorId, existing.TournamentId);
            await _tournamentSponsorRepository.DeleteAsync(tournamentSponsorId);
        }

        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            var existing = await _sponsorRepository.GetByIdAsync(id);
            if(existing == null)                            
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID {id}");
            
            if (existing.Name != sponsor.Name)
            {
                if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
                {
                    _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.Name);
                    throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
                }
            }

            try
            {
                var email = new System.Net.Mail.MailAddress(sponsor.ContactEmail);
            }
            catch
            {
                _logger.LogWarning("Invalid contact email '{ContactEmail}' for sponsor '{SponsorName}'", sponsor.ContactEmail, sponsor.Name);
                throw new InvalidOperationException("El correo electrónico de contacto no es válido");
            }

            existing.Name = sponsor.Name;
            existing.ContactEmail = sponsor.ContactEmail;
            existing.Phone = sponsor.Phone;
            existing.WebsiteUrl = sponsor.WebsiteUrl;
            existing.Category = sponsor.Category;

            _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
            await _sponsorRepository.UpdateAsync(existing);
        }
    }
}
