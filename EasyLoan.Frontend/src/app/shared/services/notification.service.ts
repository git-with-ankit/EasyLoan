import { Injectable, signal } from '@angular/core';

export interface Notification {
    id: number;
    message: string;
    type: 'success' | 'error' | 'info' | 'warning';
    duration?: number;
}

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private notifications = signal<Notification[]>([]);
    private nextId = 0;

    getNotifications = this.notifications.asReadonly();

    show(message: string, type: 'success' | 'error' | 'info' | 'warning' = 'info', duration: number = 5000): void {
        const notification: Notification = {
            id: this.nextId++,
            message,
            type,
            duration
        };
        console.log(notification);
        console.log(this.notifications);
        console.log(notification.message);
        this.notifications.update(notifs => [...notifs, notification]);

        if (duration > 0) {
            setTimeout(() => this.remove(notification.id), duration);
        }
    }

    success(message: string, duration?: number): void {
        this.show(message, 'success', duration);
    }

    error(message: string, duration?: number): void {
        this.show(message, 'error', duration);
    }

    info(message: string, duration?: number): void {
        this.show(message, 'info', duration);
    }

    warning(message: string, duration?: number): void {
        this.show(message, 'warning', duration);
    }

    remove(id: number): void {
        this.notifications.update(notifs => notifs.filter(n => n.id !== id));
    }

    clear(): void {
        this.notifications.set([]);
    }
}
