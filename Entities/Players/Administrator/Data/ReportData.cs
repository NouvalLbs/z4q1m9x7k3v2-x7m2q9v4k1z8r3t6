namespace ProjectSMP.Entities.Players.Administrator.Data
{
    public class Report
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string Text { get; set; }
        public int Timestamp { get; set; }
        public bool Handled { get; set; }
        public int AdminId { get; set; } = -1;
        public string AdminName { get; set; } = "";
    }
}