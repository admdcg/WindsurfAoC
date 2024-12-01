export interface Competition {
    id: number;
    name: string;
    startDate: string;
    endDate: string;
    isActive: boolean;
    participants: Array<any>; // You might want to define a Participant interface as well
}

export interface CreateCompetitionRequest {
    name: string;
    startDate: string;
    endDate: string;
}

export interface UpdateCompetitionRequest {
    id: number;
    name: string;
    startDate: string;
    endDate: string;
    isActive: boolean;
}
