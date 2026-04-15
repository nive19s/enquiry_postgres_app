import { Routes } from '@angular/router';
import { EnquiryList } from './pages/enquiry-list/enquiry-list';
import { NewEnquiry } from './pages/new-enquiry/new-enquiry';
import { Login } from './pages/login/login';
import { Signup } from './pages/signup/signup';
import { LandingPage } from './pages/landing-page/landing-page';
import { AdminPage } from './pages/admin-page/admin-page';
import { ProfilePage } from './pages/profile-page/profile-page';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.gurad';
import { UserRole } from './models/user.model';
import { EditProfile } from './pages/edit-profile/edit-profile';

export const routes: Routes = [
    
    
   //Public routes

   //default route
   {
   path:'',
   redirectTo:"landingpage" ,
   pathMatch:'full'
   } ,

   //Route for Login
   {
    path:"login",
    component:Login
   },
   //Route for Signup
   {
    path:"signup",
    component:Signup
   },
   //Route For Landing page
   {
      path: "landingpage",
      component: LandingPage
   },

   //User routes

   //Route for EnquiryList
   {
    path:"list",
    component:EnquiryList,
    canActivate:[roleGuard([UserRole.User])]
   },
   //Route for NewEnquiry
   {
    path:"createNew",
    component:NewEnquiry,
    canActivate:[roleGuard([UserRole.User])]
   },

   //Admin routes

   //Route For Admin Page
   {
      path: "admin",
      component: AdminPage,
      canActivate:[roleGuard([UserRole.Admin])]
   },

   //protected Route
   //profile

   {
      path: "profile",
      component: ProfilePage,
      canActivate: [authGuard]
   },
   //Route for Edit Profile
   {
      path: "edit-profile",
      component: EditProfile,
      canActivate: [authGuard]
   }


];
