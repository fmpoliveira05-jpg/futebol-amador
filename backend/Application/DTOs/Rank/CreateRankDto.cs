using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
