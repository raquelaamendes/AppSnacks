using AppSnacks.Models;
using AppSnacks.Services;
using AppSnacks.Validations;

namespace AppSnacks.Pages;

public partial class ProductDetailsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private int _productId;
    private bool _loginPageDisplayed = false;

    public ProductDetailsPage(ApiService apiService, IValidator validator, int productId, string productName)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        _productId = productId;
        Title = productName ?? "Product Details";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetProductDetails(_productId);
    }

    private async Task<Product?> GetProductDetails(int productId)
    {
        var (productDetails, errorMessage) = await _apiService.GetProductDetails(productId);

        if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
        {
            await DisplayLoginPage();
            return null;
        }

        // Verificar se houve algum erro na obtenção das produtos
        if (productDetails == null)
        {
            // Lidar com o erro, exibir mensagem ou logar
            await DisplayAlert("Error", errorMessage ?? "Couldn't get the product!", "OK");
            return null;
        }

        if (productDetails != null)
        {
            // Atualizar as propriedades dos controles com os dados do produto
            ImageProduct.Source = productDetails.UrlImage;
            LblProductName.Text = productDetails.Name;
            LblProductPrice.Text = productDetails.Price.ToString();
            LblProductDescription.Text = productDetails.Details;
            LblPriceTotal.Text = productDetails.Price.ToString();
        }
        else
        {
            await DisplayAlert("Error", errorMessage ?? "Error getting product details!", "OK");
            return null;
        }
        return productDetails;
    }

    private void ImageBtnFavorite_Clicked(object sender, EventArgs e)
    {

    }

    private void BtnRemove_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(LblQuantity.Text, out int quantity) &&
            decimal.TryParse(LblProductPrice.Text, out decimal priceUnit))
        {
            // Decrementa a quantidade, e nao permite que seja menor que 1
            quantity = Math.Max(1, quantity - 1);
            LblQuantity.Text = quantity.ToString();

            // Calcula o total
            var priceTotal = quantity * priceUnit;
            LblPriceTotal.Text = priceTotal.ToString();
        }
        else
        {
            // Tratar caso as conversoes falhem
            DisplayAlert("Error", "Invalid Values", "OK");
        }
    }

    private void BtnAdd_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(LblQuantity.Text, out int quantity) &&
       decimal.TryParse(LblProductPrice.Text, out decimal priceUnit))
        {
            // Incrementa a quantidade
            quantity++;
            LblQuantity.Text = quantity.ToString();

            // Calcula o total
            var priceTotal = quantity * priceUnit;
            LblPriceTotal.Text = priceTotal.ToString(); 
        }
        else
        {
            // Tratar caso as conversoes falhem
            DisplayAlert("Error", "Invalid Values", "OK");
        }

    }

    private async void BtnAddToCart_Clicked(object sender, EventArgs e)
    {
        try
        {
            var shoppingCart = new ShoppingCart()
            {
                Quantity = Convert.ToInt32(LblQuantity.Text),
                Price = Convert.ToDecimal(LblProductPrice.Text),
                Total = Convert.ToDecimal(LblPriceTotal.Text),
                ProductId = _productId,
                ClientId = Preferences.Get("userid", 0)
            };
            var response = await _apiService.AddItemToCart(shoppingCart);
            if (response.Data)
            {
                await DisplayAlert("Success", "Item added to cart!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", $"Error adding item to cart: {response.ErrorMessage}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;

        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }
}