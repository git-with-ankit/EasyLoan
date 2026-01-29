import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,LoginComponent,RegisterComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('EasyLoan.Frontend');

}
//loan-type-> create -> ??
        // -> list   -> ??
//nested folder structure for parent and child