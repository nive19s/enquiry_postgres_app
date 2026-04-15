import { ApplicationConfig, provideBrowserGlobalErrorListeners, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';
import {
  GoogleLoginProvider,
  SocialAuthServiceConfig,
  SOCIAL_AUTH_CONFIG
} from '@abacritt/angularx-social-login';
import {
  MsalModule,
  MsalService,
  MsalGuard,
  MsalBroadcastService,
  MsalRedirectComponent 
} from '@azure/msal-angular';
import {
  PublicClientApplication,
  InteractionType
} from '@azure/msal-browser';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    // Google Login provider
    {
      provide: SOCIAL_AUTH_CONFIG,
      useValue: {
        autoLogin: false,
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider(
              '262492784884-o61fo0dg5q6uu1gjcunkqgv44j5nug0e.apps.googleusercontent.com',
              {
                oneTapEnabled: false,
                prompt: 'select_account',
                ux_mode: 'popup'
              }
            )
          }
        ],
        onError: (err) => {
          console.error('Social auth error:', err);
        }
      } as SocialAuthServiceConfig
    },

    // Microsoft Entra ID
    importProvidersFrom(
      MsalModule.forRoot(
        new PublicClientApplication({
          auth: {
            clientId: 'a29b08fc-6a48-4484-b920-81c2b06bfc5c',
            authority: `https://login.microsoftonline.com/43256009-68ac-4963-8955-04f2d773371f`,
            redirectUri: 'http://localhost:4200/login'
          }
        }),
        {
          interactionType: InteractionType.Redirect,
          authRequest: {
            scopes: ['user.read']
          }
        },
        {
          interactionType: InteractionType.Redirect,
          protectedResourceMap: new Map([
            ['https://graph.microsoft.com/v1.0/me', ['user.read']]
          ])
        }
      )
    ),
    MsalRedirectComponent,
    MsalService,
    MsalGuard,
    MsalBroadcastService
  ]
};