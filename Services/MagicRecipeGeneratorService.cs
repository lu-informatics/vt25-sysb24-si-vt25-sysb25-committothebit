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
        string prompt = $"You are an AI that generates recipes strictly in JSON format. You must follow the exact JSON structure provided and output only valid JSON—no explanations, commentary, or extra text is allowed. Here is an example of the expected JSON Structure: {{\"id\": 1,\"name\": \"Greek Salad\",\"details\": {{\"description\": \"Experience a zesty Greek salad that is crisp, fresh, and fabulously flavorful. Legend has it that the feta was so irresistible, even Zeus traded a lightning bolt for a taste. Add a sprinkle of oregano and a dash of myth for a meal that’s as entertaining as it is refreshing.\",\"steps\": [\"Chop fresh cucumbers, tomatoes, red onions, and bell peppers into uniform pieces.\",\"Toss the vegetables with crumbled feta cheese and Kalamata olives.\",\"Drizzle with olive oil and lemon juice, sprinkle with oregano, and serve immediately.\"]}},\"cookingTime\": 15,\"servings\": 2,\"difficultyLevel\": \"Easy\"}}Instructions:Strict JSON Compliance: Output must be valid JSON that exactly matches the structure above. Do not include any keys other than those shown.Ingredient Restriction: Use only the ingredients provided in the list below; do not add any extra ingredients.Field Requirements:id: Must be a unique integer and incremented from the last known recipe ID.name: A creative recipe title.details.description: A concise yet engaging description that reflects the recipe's style.details.steps: An array of strings detailing the cooking steps. The number of steps can vary; ensure they are in logical order.cookingTime: A positive integer representing the preparation time in minutes.servings: A positive integer.difficultyLevel: One of 'Easy', 'Medium', or 'Hard', appropriate for the recipe’s complexity.No Extra Text: Return only the JSON object—nothing else. Available ingredients: {ingredientsList}. Generate a new recipe using only the ingredients listed above.";
        ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);
        string response = completion.Content[0].Text;
        return response;
    }
}
