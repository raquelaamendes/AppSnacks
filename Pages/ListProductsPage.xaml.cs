using AppSnacks.Models;
using AppSnacks.Services;
using AppSnacks.Validations;

namespace AppSnacks.Pages;

public partial class ListProductsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private int _categoryId;
    private bool _loginPageDisplayed = false;

    public ListProductsPage(int categoryId, string categoryName, ApiService apiService, IValidator validador)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validador;
        _categoryId = categoryId;
        Title = categoryName ?? "Products";  // Definindo o título da página

    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetListProducts(_categoryId);
    }

    private async Task<IEnumerable<Product>> GetListProducts(int categoryId)
    {
        try
        {
            var (products, errorMessage) = await _apiService.GetProducts("category", categoryId.ToString());

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return Enumerable.Empty<Product>();
            }

            if (products is null)
            {
                await DisplayAlert("Error", errorMessage ?? "Error getting categories!", "OK");
                return Enumerable.Empty<Product>();
            }

            CvProducts.ItemsSource = products;
            return products;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
            return Enumerable.Empty<Product>();
        }
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }

    private void CvProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var currentSelection = e.CurrentSelection.FirstOrDefault() as Product;

        if (currentSelection is null)
            return;

        Navigation.PushAsync(new ProductDetailsPage(_apiService,
                                                    _validator,
                                                    currentSelection.Id,
                                                     currentSelection.Name!));

        ((CollectionView)sender).SelectedItem = null;
    }
}