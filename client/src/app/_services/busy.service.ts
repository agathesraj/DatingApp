import { inject, Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class BusyService {

  busyServiceCount = 0;
  private spinnerService = inject(NgxSpinnerService);

  busy(){
    this.busyServiceCount++;
    this.spinnerService.show(undefined,{
      type: 'cube-transition',
      bdColor: 'rgb(255,255,255,0)',
      color: '#333333'
    })
  }

  idle(){
    this.busyServiceCount--;
    if(this.busyServiceCount<=0){
      this.busyServiceCount =0;
      this.spinnerService.hide();
    }
  }
}
