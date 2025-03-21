using System;
using Informatics.Appetite.Contexts;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Informatics.Appetite.Services;

public class MagicRecipeGeneratorService : IMagicRecipeGeneratorService
{
    private readonly string _openAiKey;
    private readonly ChatClient _chatClient;

    public MagicRecipeGeneratorService(string openAiKey)
    {
        _openAiKey = openAiKey;
        _chatClient = new ChatClient("gpt-4o-mini", _openAiKey);
    }

    public async Task<string> GenerateRecipeAsync(string ingredientsList)
    {
        string prompt = $"You are an AI that generates recipes strictly in JSON format. You must follow the exact JSON structure provided and output only valid JSON—no explanations, commentary, or extra text is allowed. Here is an example of the expected JSON Structure: {{ \"id\": 1, \"name\": \"Classic Spaghetti Bolognese\", \"data\": \"{{\"description\": \"Dive into a classic Italian adventure with our Spaghetti Bolognese! This hearty dish is so legendary that even Nonna would blush when you serve it. Rumor has it the sauce once made a tomato sing – not to mention that every bite comes with a free side of Italian charm (and maybe a cheesy joke or two).\", \"steps\": [\"Preheat a large pan with a drizzle of olive oil over medium-high heat and add the ground beef to brown it evenly.\", \"Once browned, add chopped onions and minced garlic, cooking until they are soft and translucent.\", \"Season the mixture with garlic pepper and salt, then deglaze the pan with a splash of water to lift the caramelized bits.\", \"Stir in crushed tomatoes and a mix of basil, oregano, and other herbs, allowing the sauce to simmer and develop rich flavors.\", \"Meanwhile, boil spaghetti in salted water until al dente.\", \"Drain the pasta and combine it with the sauce; garnish with Parmesan cheese and fresh basil before serving.\"]}}\", \"cookingTime\": 30, \"servings\": 4, \"difficultyLevel\": \"Easy\", \"ingredientNames\": [ \"Onion\", \"Garlic\", \"Tomato\", \"Ground Beef\", \"Pasta\", \"Black Pepper\", \"Salt\" ] }}Instructions:Strict JSON Compliance: Output must be valid JSON that exactly matches the structure above. Do not include any keys other than those shown.Ingredient Restriction: Use only the ingredients provided in the list below; do not add any extra ingredients.Field Requirements:id: Must be a unique integer and incremented from the last known recipe ID.name: A creative recipe title.data.description: A concise yet engaging description that reflects the recipe's style.data.steps: An array of strings detailing the cooking steps. The number of steps can vary; ensure they are in logical order. Also, specify amount of each ingredient in the cooking steps. All ingredients in ingrientNames must be mentioned in the steps. cookingTime: A positive integer representing the preparation time in minutes.servings: A positive integer.difficultyLevel: One of 'Easy', 'Medium', or 'Hard', appropriate for the recipe’s complexity.No Extra Text: Return only the JSON object—nothing else. Available ingredients: {ingredientsList}. Generate a new recipe using only the ingredients listed above.";
        ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);
        string response = completion.Content[0].Text;
        return response;
    }
}
