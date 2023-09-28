import { User } from "./user";

export interface Reservation {
    id?: number;
    deskId: number;
    userDTO: User;
    date: string;
    type: string | null;
}