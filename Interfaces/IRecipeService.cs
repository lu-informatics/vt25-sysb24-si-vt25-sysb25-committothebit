using System;
using Informatics.Appetite.Models;
using Informatics.Appetite.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace Informatics.Appetite.Interfaces;

public interface IRecipeService
{
    Task<IEnumerable<Recipe>> GetRecipesAsync();
    Task<Recipe?> GetRecipeByIdAsync(int id);
    Task<Recipe?> GetRecipeByNameAsync(string name);
    Task<Recipe> SaveRecipeAsync(Recipe recipe);
    Task<bool> DeleteRecipeByIdAsync(int id);
    Task<bool> DeleteRecipeByNameAsync(string name);
    Task<List<string>> GetDifficultyLevelsAsync();
    Task<List<string>> GetCookingTimesAsync();

    Task<List<string>> GetDietTagsAsync();

    Task<List<string>> GetCategoriesAsync();

}

   
