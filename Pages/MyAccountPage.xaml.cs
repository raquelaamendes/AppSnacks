using AppSnacks.Services;

namespace AppSnacks.Pages;

public partial class MyAccountPage : ContentPage
{
    private readonly ApiService _apiService;

    private const string UserNameKey = "username";
    private const string UserEmailKey = "useremail";
    private const string UserPhoneKey = "userphone";

    public MyAccountPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadUserInfo();
        ImgBtnProfile.Source = await GetProfileImageAsync();
    }

    private async Task<ImageSource> GetProfileImageAsync()
    {
        string imageDefault = AppConfig.ProfileImageDefault;

        var (response, errorMessage) = await _apiService.GetUserProfileImage();

        if (errorMessage is not null)
        {
            switch (errorMessage)
            {
                case "Unauthorized":
                    await DisplayAlert("Error", "Not Authorized", "OK");
                    return imageDefault;
                default:
                    await DisplayAlert("Error", errorMessage ?? "Error getting image.", "OK");
                    return imageDefault;
            }
        }

        if (response?.UrlImage is not null)
        {
            return response.ImagePath;
        }
        return imageDefault;
    }

    private void LoadUserInfo()
    {
        LblUserName.Text = Preferences.Get(UserNameKey, string.Empty);
        EntName.Text = LblUserName.Text;
        EntEmail.Text = Preferences.Get(UserEmailKey, string.Empty);
        EntPhone.Text = Preferences.Get(UserPhoneKey, string.Empty);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        // Salva as informacoes alteradas pelo user nas preferencias
        Preferences.Set(UserNameKey, EntName.Text);
        Preferences.Set(UserEmailKey, EntEmail.Text);
        Preferences.Set(UserPhoneKey, EntPhone.Text);
        await DisplayAlert("Profile edited", "You edited your profile!", "OK");
    }
}