using AppSnacks.Services;
using AppSnacks.Validations;

namespace AppSnacks.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private bool _loginPageDisplayed = false;

    public ProfilePage(ApiService apiService, IValidator validator)
    {
        InitializeComponent();
        LblUserName.Text = Preferences.Get("username", string.Empty);
        _apiService = apiService;
        _validator = validator;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ImgBtnProfile.Source = await GetProfileImage();
    }

    private async Task<ImageSource> GetProfileImage()
    {
        //Obtenha a imagem padrao do AppConfig
        string imageDefault = AppConfig.ProfileImageDefault;

        var (response, errorMessage) = await _apiService.GetUserProfileImage();

        // Lida com casos de erro
        if (errorMessage is not null)
        {
            switch (errorMessage)
            {
                case "Unauthorized":
                    if (!_loginPageDisplayed)
                    {
                        await DisplayLoginPage();
                        return null;
                    }
                    break;
                default:
                    await DisplayAlert("Error", errorMessage ?? "Error getting profile image.", "OK");
                    return imageDefault;
            }
        }

        if (response?.UrlImage is not null)
        {
            return response.ImagePath;
        }

        return imageDefault;
    }

    private async Task<byte[]?> SelectImageAsync()
    {
        try
        {
            var file = await MediaPicker.PickPhotoAsync();

            if (file is null) return null;

            using (var stream = await file.OpenReadAsync())
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Error", "Your device does not support image upload.", "Ok");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Error", "We need permission to access camera and gallery.", "Ok");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error selecting image: {ex.Message}", "Ok");
        }
        return null;
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }

    private async void ImgBtnProfile_Clicked(object sender, EventArgs e)
    {
        try
        {
            var imageArray = await SelectImageAsync();
            if (imageArray is null)
            {
                await DisplayAlert("Error", "Error loading image", "Ok");
                return;
            }
            ImgBtnProfile.Source = ImageSource.FromStream(() => new MemoryStream(imageArray));

            var response = await _apiService.UploadUserImage(imageArray);
            if (response.Data)
            {
                await DisplayAlert("", "Image sent!", "Ok");
            }
            else
            {
                await DisplayAlert("Error", response.ErrorMessage ?? "Error", "Cancel");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "Ok");
        }
    }

    private void TapOrders_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void MyAccount_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void Questions_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void BtnLogout_Clicked(object sender, EventArgs e)
    {

    }
}