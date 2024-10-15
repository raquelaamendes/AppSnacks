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
        if (IsNavigatingToEmptyCartPage()) return;

        bool hasItems = await GetCartOrderItems();

        if (hasItems)
        {
            ShowAddress();
        }
        else
        {
            await NavigateToEmptyCartPage();
        }

    }

    private async Task NavigateToEmptyCartPage()
    {
        LblAddress.Text = string.Empty;
        _isNavigatingToEmptyCartPage = true;
        await Navigation.PushAsync(new EmptyCartPage());
    }

    private void ShowAddress()
    {

        bool addressExists = Preferences.ContainsKey("address");

        if (addressExists)
        {
            string name = Preferences.Get("name", string.Empty);
            string address = Preferences.Get("address", string.Empty);
            string phone = Preferences.Get("phone", string.Empty);

            //Formatar os dados conforme desejado na label
            LblAddress.Text = $"{name}\n{address}\n{phone}";
        }
        else
        {
            LblAddress.Text = "Enter your address";
        }
    }

    private bool IsNavigatingToEmptyCartPage()
    {
        if (_isNavigatingToEmptyCartPage)
        {
            _isNavigatingToEmptyCartPage = false;
            return true;
        }
        return false;
    }

    private async Task<bool> GetCartOrderItems()
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
                return false;
            }

            if (itemsShoppingCart == null)
            {
                await DisplayAlert("Error", errorMessage ?? "Error getting items from shopping cart!", "OK");
                return false;
            }

            ItemsShoppingCart.Clear();
            foreach (var item in itemsShoppingCart)
            {
                ItemsShoppingCart.Add(item);
            }

            CvCart.ItemsSource = ItemsShoppingCart;
            UpdateTotalPrice();

            if (!ItemsShoppingCart.Any())
            {
                return false;
            }
            return true;

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
            return false;
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

    private async void BtnDecrease_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is CartOrderItem itemCart)
        {
            if (itemCart.Quantity == 1) return;
            else
            {
                itemCart.Quantity--;
                UpdateTotalPrice();
                await _apiService.UpdateItemQuantityCart(itemCart.ProductId, "decrease");
            }
        }
    }

    private async void BtnIncrease_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is CartOrderItem itemCart)
        {
            itemCart.Quantity++;
            UpdateTotalPrice();
            await _apiService.UpdateItemQuantityCart(itemCart.ProductId, "increase");
        }
    }

    private async void BtnDelete_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is CartOrderItem itemCart)
        {
            bool response = await DisplayAlert("Confirmation",
                          "Do you want to delete this item?", "Yes", "No");
            if (response)
            {
                ItemsShoppingCart.Remove(itemCart);
                UpdateTotalPrice();
                await _apiService.UpdateItemQuantityCart(itemCart.ProductId, "delete");
            }
        }
    }

    private void BtnEditAddress_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new AddressPage());
    }

    private async void TapConfirmOrder_Tapped(object sender, TappedEventArgs e)
    {
        if (ItemsShoppingCart == null || !ItemsShoppingCart.Any())
        {
            await DisplayAlert("Info", "Your cart is empty or your order was already confirmed.", "OK");
            return;
        }

        var order = new Order()
        {
            Address = LblAddress.Text,
            UserId = Preferences.Get("userid", 0),
            Total = Convert.ToDecimal(LblPriceTotal.Text)
        };

        var response = await _apiService.ConfirmOrder(order);

        if (response.HasError)
        {
            if (response.ErrorMessage == "Unauthorized")
            {
                // Redirecionar para a pagina de login
                await DisplayLoginPage();
                return;
            }
            await DisplayAlert("Ups!!", $"Error: {response.ErrorMessage}", "Cancel");
            return;
        }

        ItemsShoppingCart.Clear();
        LblAddress.Text = "Enter your address";
        LblPriceTotal.Text = "0.00";

        await Navigation.PushAsync(new OrderConfirmedPage());
    }
}