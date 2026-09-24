namespace Application.DTOs.Rank
{
    internal class CreateRankDto
    {
        //Acabar de ver!!
        public int Id { get; set; }
        public string RankName { get; set; }
        public int WinPoints { get; set; }
        public int DrawPoints { get; set; }
        public int LosePoints { get; set; }
        public int PointsToPromotion { get; set; }
    }
}
