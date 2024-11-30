export interface User {
    id: number;
    email: string;
    isAdmin: boolean;
}

export interface Competition {
    id: number;
    name: string;
    startDate: string;
    endDate: string;
    isActive: boolean;
}

export interface DailyChallenge {
    id: number;
    competitionId: number;
    dayNumber: number;
}

export interface ChallengeCompletion {
    id: number;
    dailyChallengeId: number;
    userId: number;
    partNumber: number;
    completionTime: string;
    position: number;
}

export interface CompetitionParticipant {
    id: number;
    userId: number;
    competitionId: number;
    joinDate: string;
    totalPoints: number;
    user?: User;
}
