import { Pitch } from "./Pitch";
import { Player } from "./Player";

export class Team{
    id!: string;
    name!: string;
    description!: string;
    iconName!:string | null | undefined;
    iconPath?: string;
    pitch!: Pitch;
    foundationDate!: Date;
    members!: Player[];
    //membershipRequests!: MembershipRequest[];
    //sentInvites!: MatchInvite[];
    //receivedInvites!: MatchInvite[];
    //calendar!: Calendar;
}