namespace SportsLeague.API.DTOs.Request
{
    public class AddSponsorToTournamentDTO
    {
        public int TournamentId { get; set; }
        public decimal ContractAmount { get; set; }
    }
}
