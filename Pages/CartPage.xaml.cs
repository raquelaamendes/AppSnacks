using AppSnacks.Models;
using AppSnacks.Services;
using AppSnacks.Validations;
using System.Collections.ObjectModel;

namespace AppSnacks.Pages;

public partial class CartPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private bool _loginPageDisplayed = false;
    private bool _isNavigatingToEmptyCartPage = false;

    private ObservableCollection<CartOrderItem>
        ItemsShoppingCart = new ObservableCollection<CartOrderItem>();

    public CartPage(ApiService apiService, IValidator validator)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetCartOrderItems();
    }

    private async Task<IEnumerable<CartOrderItem>> GetCartOrderItems()
    {
        try
        {
            var userId = Preferences.Get("userid", 0);
            var (itemsShoppingCart, errorMessage) = await
                     _apiService.GetCartOrderItems(userId);

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                // Redirecionar para a pagina de login
                await DisplayLoginPage();
                return Enumerable.Empty<CartOrderItem>();
                //return false;
            }

            if (itemsShoppingCart == null)
            {
                await DisplayAlert("Error", errorMessage ?? "Error getting items from shopping cart!", "OK");
                return Enumerable.Empty<CartOrderItem>();
                //return false;
            }

            ItemsShoppingCart.Clear();
            foreach (var item in itemsShoppingCart)
            {
                ItemsShoppingCart.Add(item);
            }

            CvCart.ItemsSource = ItemsShoppingCart;
            UpdateTotalPrice();
            return itemsShoppingCart;

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
            return Enumerable.Empty<CartOrderItem>();
            //return false;
        }
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;

        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }

    private void UpdateTotalPrice()
    {
        try
        {
            var total = ItemsShoppingCart.Sum(item => item.Price * item.Quantity);
            LblPriceTotal.Text = total.ToString();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Error updating total price: {ex.Message}", "OK");
        }
    }

    private void BtnDecrease_Clicked(object sender, EventArgs e)
    {

    }

    private void BtnIncrease_Clicked(object sender, EventArgs e)
    {

    }

    private void BtnDelete_Clicked(object sender, EventArgs e)
    {

    }

    private void BtnEditAddress_Clicked(object sender, EventArgs e)
    {

    }

    private void TapConfirmOrder_Tapped(object sender, TappedEventArgs e)
    {

    }
}