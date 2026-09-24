/** Uma linha da classificação geral (`GET /Leaderboard`). */
export interface TeamLeaderboardDto {
  id: string;
  position: number;
  teamName: string;
  currentPoints: number;
  rankName: string;
}
