export interface MembershipRequest {
  requestId: string;
  player: {
    id: string;
    name: string;
  };
  team: {
    idTeam: string;
    name: string;
  };
  requestDate: string;
  isPlayerSender: boolean;
}