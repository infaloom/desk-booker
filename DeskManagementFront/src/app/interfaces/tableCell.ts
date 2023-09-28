import { CellStyle } from "./cellStyle";
import { Desk } from "./desk.interface";
import { Reservation } from "./reservation.interface";

export interface TableCellData {
    desk: Desk;
    date: string;
    reservation: Reservation | undefined;
    resUsername?: string | undefined;
    cellStyle: CellStyle; 
}