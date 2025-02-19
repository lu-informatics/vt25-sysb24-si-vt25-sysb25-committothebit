using Appetite;

namespace Appetite;

public partial class MainPage : ContentPage
{
    private List<string> ingredients = new List<string>(); // Stores ingredients
    private int currentIngredientIndex = 0; // Tracks the displayed ingredient
    private TestDataAccessLayer dataAccess = new TestDataAccessLayer(); // Data access object

    public MainPage()
    {
        InitializeComponent();
        LoadIngredients(); // Fetch ingredients when the page loads
    }

    private void LoadIngredients()
    {
        ingredients = dataAccess.GetAllIngredients();

        if (ingredients.Count == 0)
        {
            CounterBtn.Text = "No ingredients found.";
        }
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        if (ingredients.Count == 0)
        {
            CounterBtn.Text = "No ingredients found.";
            return;
        }

        // Display the next ingredient
        CounterBtn.Text = ingredients[currentIngredientIndex];

        // Move to the next ingredient, looping back to the start if at the end
        currentIngredientIndex = (currentIngredientIndex + 1) % ingredients.Count;

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}