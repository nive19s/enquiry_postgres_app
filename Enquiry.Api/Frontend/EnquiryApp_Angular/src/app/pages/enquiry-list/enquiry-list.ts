import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { Master } from '../../service/master';
import { Router, RouterLink } from  '@angular/router'
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-enquiry-list',
  imports: [DatePipe,RouterLink],
  templateUrl: './enquiry-list.html',
  styleUrl: './enquiry-list.css',
})
export class EnquiryList implements OnInit {

  masterSrc = inject(Master)
  cdr = inject(ChangeDetectorRef)
  router = inject(Router)
  enquiryList: any[] = [];

  ngOnInit(): void {
    this.masterSrc.getAllEnquiries().subscribe((Res: any) => {
      this.enquiryList = Res;
      this.cdr.detectChanges(); // Manually trigger change detection
    })
  }

  onDelete(id: number) {
  // Confirm before deleting
  if (confirm('Are you sure you want to delete this enquiry?')) {
    this.masterSrc.deleteEnquiry(id).subscribe({
      next: (Res: any) => {
        alert('Enquiry deleted successfully!');
        // Refresh the list after deletion
        this.getAllEnquiries();
      },
      error: (error) => {
        alert('Failed to delete enquiry. Please try again.');
        console.error('Delete error:', error);
      }
    });
  }
}

// Refresh the list
getAllEnquiries() {
  this.masterSrc.getAllEnquiries().subscribe((res: any) => {
    this.enquiryList = res;
    this.cdr.detectChanges();
  });
}
  
}
