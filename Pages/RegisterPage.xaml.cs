using AppSnacks.Services;
using AppSnacks.Validations;

namespace AppSnacks.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;

    public RegisterPage(ApiService apiService, IValidator validator)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
    }

    private async void TapLogin_TappedAsync(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }

    private async void BtnSignup_Clicked(object sender, EventArgs e)
    {
        if (await _validator.Validate(EntNome.Text, EntEmail.Text, EntPhone.Text, EntPassword.Text))
        {
            var response = await _apiService.UserRegister(EntNome.Text, EntEmail.Text,
                                                              EntPhone.Text, EntPassword.Text);

            if (!response.HasError)
            {
                await DisplayAlert("Alert", "You are one of us !!", "OK");
                await Navigation.PushAsync(new LoginPage(_apiService, _validator));
            }
            else
            {
                await DisplayAlert("Error", "Something is wrong!!!", "Cancel");
            } 
        }
        else
        {
            string messageError = "";
            messageError += _validator.NameError != null ? $"\n- {_validator.NameError}" : "";
            messageError += _validator.EmailError != null ? $"\n- {_validator.EmailError}" : "";
            messageError += _validator.PhoneError != null ? $"\n- {_validator.PhoneError}" : "";
            messageError += _validator.PasswordError != null ? $"\n- {_validator.PasswordError}" : "";

            await DisplayAlert("Error", messageError, "OK");
        }
    }
}