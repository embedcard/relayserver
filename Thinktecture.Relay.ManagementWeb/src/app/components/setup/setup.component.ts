import {Component} from '@angular/core';
import {BackendService} from '../../services/backend.service';

@Component({
  selector: 'trs-setup',
  templateUrl: './setup.component.html',
  styleUrls: ['./setup.component.scss'],
})
export class SetupComponent {
  userName: string;
  password: string;
  verifyPassword: string;
  submitting: boolean;

  constructor(private readonly _backend: BackendService) {
  }

  submit() {
    this.submitting = true;
    this._backend.createFirstTimeUser(this.userName, this.password).subscribe({ error: () => this.submitting = false });
  }
}