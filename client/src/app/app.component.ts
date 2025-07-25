import { Component, inject, OnInit } from '@angular/core';
import { NavComponent } from "./nav/nav.component";
import { AccountService } from './_services/account.service';
import { Router, RouterOutlet } from '@angular/router';
import { NgxSpinnerComponent } from 'ngx-spinner';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  imports: [NavComponent, RouterOutlet, NgxSpinnerComponent]
})
export class AppComponent implements OnInit{
  private accountService =inject(AccountService)
  title = 'DatingApp'; 

  ngOnInit(): void {
    this.setCurrentUser();
  }
  
  setCurrentUser(){
    const userString = localStorage.getItem('user');
    if(!userString) return;
    const user = JSON.parse(userString);
    this.accountService.setCurrentUser(user);
  }

  
}
