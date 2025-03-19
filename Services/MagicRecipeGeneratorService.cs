using System;
using Informatics.Appetite.Contexts;
using Informatics.Appetite.Interfaces;
using Informatics.Appetite.Models;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;

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

    public async Task<string> GenerateRecipeAsync()
    {
        string prompt = "Hello, is there anybody out there?";
        ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);
        string response = completion.Content[0].Text;
        return response;
    }
}
