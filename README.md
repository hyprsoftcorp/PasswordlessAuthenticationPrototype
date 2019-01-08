# PasswordlessAuthenticationPrototype
This app demonstrates a streamlined authentication experience where the user is only required to log into the app one time using a "magic" link sent to their email. No username or password is required and once the user is logged in they don't have to login again. This prototype is ONLY a demonstration and has no real functionality other than showcasing the password-less authentication workflow.

## End-to-End Workflow
1. A user requests an invitation to try the app and an automated email is sent to the user with instructions on how to download the app.
2. Once the app is installed the user opens the magic link in the email.
3. Opening the magic link on the device opens the app and logs the user into the "platform".
4. The user is presented with a personalized greeting displaying their name and email address.

## Technical Details
### Backend
Built using ASP.NET Core 2.2.

### Mobile Apps
Built with Xamarin Forms (Android and iOS).
