import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../services/notification.service';

@Component({
    selector: 'app-notifications',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './notifications.component.html',
    styleUrl: './notifications.component.css'
})
export class NotificationsComponent {
    constructor(public notificationService: NotificationService) { }

    closeNotification(id: number): void {
        this.notificationService.remove(id);
    }
}
