import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class Master {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Helper method to get authorization headers with JWT token
  private getAuthHeaders(): HttpHeaders {
    const authState = localStorage.getItem('authState');
    if (authState) {
      const { token } = JSON.parse(authState);
      return new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      });
    }
    return new HttpHeaders({
      'Content-Type': 'application/json'
    });
  }

  //Api for create Enquiry
  createEnquiry(obj: any) {
    return this.http.post(`${this.baseUrl}/CreateNewEnquiry`, obj, {
      headers: this.getAuthHeaders()
    });
  }

  //Api for get status
  getStatus() {
    return this.http.get(`${this.baseUrl}/GetAllStatus`, {
      headers: this.getAuthHeaders()
    });
  }

  //Api for get Types
  getTypes() {
    return this.http.get(`${this.baseUrl}/GetAllTypes`, {
      headers: this.getAuthHeaders()
    });
  }

  //Api for get All Enquiries
  getAllEnquiries() {
    return this.http.get(`${this.baseUrl}/GetAllEnquiry`, {
      headers: this.getAuthHeaders()
    });
  }

  //Api for delete Enquiry
  deleteEnquiry(id: number) {
    return this.http.delete(`${this.baseUrl}/DeleteEnquiryById/${id}`, {
      headers: this.getAuthHeaders()
    });
  }
  
}