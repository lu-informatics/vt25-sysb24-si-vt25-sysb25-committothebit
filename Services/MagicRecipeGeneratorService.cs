using System;
using Informatics.Appetite.Contexts;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using Microsoft.EntityFrameworkCore;

namespace Informatics.Appetite.Services;

public class MagicRecipeGeneratorService : IMagicRecipeGeneratorService
{
    private readonly string _openAiKey;

    public MagicRecipeGeneratorService(string openAiKey)
    {
        _openAiKey = openAiKey;
    }

    public async Task<string> GenerateRecipeAsync()
    {
        // This is where the magic happens
        return "If you can see this text, it means the code has gotten as far as the GenerateRecipesAsync method in the MagicRecipeGeneratorService class.";
    }
}
