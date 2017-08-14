import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs/Observable';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'app';
  response$: Observable<string>;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.response$ = this.http.get(environment.apiUrl, {responseType: 'text'});
  }
}
