using Microsoft.Maui.Controls;
using Informatics.Appetite.ViewModels;

namespace Informatics.Appetite.Pages
{
    public partial class MagicRecipeGeneratorPage : ContentPage
    {
        public MagicRecipeGeneratorPage(MagicRecipeGeneratorViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}