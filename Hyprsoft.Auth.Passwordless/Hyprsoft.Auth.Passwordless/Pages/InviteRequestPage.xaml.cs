using Hyprsoft.Auth.Passwordless.Models;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hyprsoft.Auth.Passwordless
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InviteRequestPage : BaseContentPage
    {
        #region Constructors

        public InviteRequestPage()
        {
            InitializeComponent();
            Title = "Invitation Request";
        }

        #endregion

        #region Properties

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public Command SendInviteCommand => new Command(async () => await SendInviteAsync(new InvitationRequest { Name = Name, Email = Email }));

        #endregion

        #region Methods

        private async Task SendInviteAsync(InvitationRequest request)
        {
            try
            {
                if (!Validator.TryValidateObject(request, new ValidationContext(request), null))
                {
                    await DisplayAlert("Help Us Out", $"It looks like we're missing some information.  Please ensure your name and email are correct.", "Dismiss");
                    return;
                }

                IsBusy = true;
                System.Diagnostics.Debug.WriteLine($"[{nameof(SendInviteAsync)}] - Name: '{request.Name}', Email: '{request.Email}'.");
                var response = await App.HttpClient.PostAsync($"api/auth/invite",
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                IsBusy = false;
                if (response.IsSuccessStatusCode)
                {
                    Analytics.TrackEvent(nameof(SendInviteAsync), new Dictionary<string, string> { { "Status", $"[SUCCESS] Name: '{request.Name}', Email: '{request.Email}'." } });
                    await DisplayAlert("Well Done", $"Your invitation was successfully sent to '{request.Email}'.  Please check your email.", "Dismiss");
                }
                else
                {
                    Analytics.TrackEvent(nameof(SendInviteAsync), new Dictionary<string, string> { { "Status", $"[FAILED] Name: '{request.Name}', Email: '{request.Email}'." } });
                    await DisplayAlert("Oops", $"We were unable to send your invitation to '{request.Email}'.  Please check youer email address and try again later.", "Dismiss");
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await DisplayAlert("Error", $"We appologize, but it looks like something bad happened.  Please try again later.  Details: {ex.Message}.", "Dismiss");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }

}