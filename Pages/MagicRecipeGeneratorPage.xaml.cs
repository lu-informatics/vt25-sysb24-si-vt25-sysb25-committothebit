namespace Informatics.Appetite.Pages;

public partial class MagicRecipeGeneratorPage : ContentPage
{
	public MagicRecipeGeneratorPage()
	{
		InitializeComponent();
	}

	private void GenerateRecipeButton_Clicked(object sender, EventArgs e)
	{
		// Logic to generate a random recipe
		string[] recipes = new string[]
		{
			"Spaghetti Bolognese",
			"Chicken Alfredo",
			"Vegetable Stir Fry",
			"Beef Tacos",
			"Grilled Salmon"
		};

		Random random = new Random();
		int index = random.Next(recipes.Length);
		string selectedRecipe = recipes[index];

		// Update the label with the generated recipe
		RecipeLabel.Text = $"Your random recipe is: {selectedRecipe}";
	}
}