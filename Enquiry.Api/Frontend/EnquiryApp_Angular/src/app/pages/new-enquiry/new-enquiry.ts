import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Master } from '../../service/master';
import { Observable } from 'rxjs';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-new-enquiry',
  imports: [FormsModule,AsyncPipe],
  templateUrl: './new-enquiry.html',
  styleUrl: './new-enquiry.css',
})
export class NewEnquiry {

  router = inject(Router);
  newEnquiryObj: any = {
    enquiryId:0,
    enquiryTypeId:0,
    enquiryStatusId:0,
    customerName:'',
    mobileNo:'',
    email:'',
    message:'',
    createdDate:new Date(),
    resolution:''

  };
  masterSrv= inject(Master);

  typeList: Observable<any> = new Observable<any>();
  statusList: Observable<any> = new Observable<any>();

  constructor() {
    this.typeList = this.masterSrv.getTypes();
    this.statusList = this.masterSrv.getStatus();
  }

  onSave() {
  this.masterSrv.createEnquiry(this.newEnquiryObj).subscribe({
    next: (res: any) => {
      alert('Enquiry created successfully!');
      // Reset the form
      this.resetForm();
      this.router.navigateByUrl('/list');
    },
    error: (error) => {
      alert('Failed to create enquiry. Please try again.');
      console.error('Create error:', error);
    }
  });
}

//Reset the form
resetForm() {
  this.newEnquiryObj = {
    enquiryId:0,
    enquiryTypeId:0,
    enquiryStatusId:0,
    customerName:'',
    mobileNo:'',
    email:'',
    message:'',
    createdDate:new Date(),
    resolution:''
  };
}
}
