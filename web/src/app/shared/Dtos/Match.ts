export class Match{
    id!: string;
    matchStatus!: MatchStatus;
}

export enum MatchStatus {
  SCHEDULED = 'Scheduled',
  IN_PROGRESS = 'In Progress',
  DONE = 'Done',
  POST_PONED = 'Post Poned',
  CANCELLED = 'Cancelled',
}
