using AppSnacks.Services;

namespace AppSnacks.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService;

    public RegisterPage(ApiService apiService)
	{
		InitializeComponent();
        _apiService = apiService;
    }

    private async void TapLogin_TappedAsync(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(_apiService));
    }

    private async void BtnSignup_Clicked(object sender, EventArgs e)
    {
        var response = await _apiService.UserRegister(EntNome.Text, EntEmail.Text,
                                                          EntPhone.Text, EntPassword.Text);

        if (!response.HasError)
        {
            await DisplayAlert("Alert", "You are one of us !!", "OK");
            await Navigation.PushAsync(new LoginPage(_apiService));
        }
        else
        {
            await DisplayAlert("Error", "Something is wrong!!!", "Cancel");
        }
    }
}