export interface MembershipRequest {
  requestId: string;
  playerName: string;
  playerId: string;
  teamId: string;
  teamName: string;
  requestDate: string;
  isPlayerSender: boolean;
}