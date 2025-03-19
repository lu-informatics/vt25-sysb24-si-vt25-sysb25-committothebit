using System;

namespace Informatics.Appetite.Interfaces;

public interface IMagicRecipeGeneratorService
{
    Task<string> GenerateRecipeAsync(string ingredientsList);
}
