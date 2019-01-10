# PasswordlessAuthenticationPrototype
This repo demonstrates the typical password-less authentication experience.  The user is only required to log into the app one time using a "magic" link sent to their email. No username or password is required and once the user is logged in, they don't have to login again. This prototype is ONLY a demonstration and has no real functionality other than showcasing the password-less authentication workflow.

## End-to-End Workflow
1. A user requests an invitation to try the app and an automated email is sent containing instructions on how to download the app.
2. Once the app is installed the user opens the "magic" link on the device.
3. Opening the magic link opens the app and logs the user into the "platform".
4. The user is presented with a personalized greeting displaying their name and email address.

## Technical Details
### [Backend](https://pwdlessauth.azurewebsites.net/)
Built using C# and ASP.NET Core 2.2.  To generate our "magic" links we used a timed-one-time-password (TOTP) algorithm. We set the built-in DataProtectorTokenProvider's TokenLifespan to 15 minutes.  It's important to note that this change affects other workflows such as email confirmation, password reset, etc. which may not be desired.  In our case, since our "platform" is accessible only by invitation, there is no user signup, email confirmation, password reset, etc.  

Once the user is emailed the "magic" link and opened on the device we authenticate the mobile app using standard bearer token authentication.  The access token and refresh token are both persisted to secure storage on the device for subsequent use.  The access token currently has a lifespan of 24 hours so the refresh workflow is invoked once a day.  This allows us to deactivate users in the backend to prevent future access without having to periodically query the database on certain requests to make sure the user is still active.

### Mobile Apps ([Android](https://play.google.com/store/apps/details?id=com.hyprsoft.Auth.Passwordless.Prototype) and [iOS](https://www.apple.com/ios/app-store))
Built using C# and Xamarin Forms (100% shared code).  To initiate the password-less authentication workflow we leveraged the app links/deep linking functionality on Android and universal links on iOS.
