using System;
using Microsoft.EntityFrameworkCore;
using Informatics.Appetite.Models;

namespace Informatics.Appetite.Contexts;

public class RecipeContext : DbContext
{
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Recipe> Recipes { get; set;}
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set;}

    public DbSet<UserIngredient> UserIngredients { get; set; }

    public RecipeContext(DbContextOptions<RecipeContext> options)
        : base(options) { }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<AppUser>()
        .HasKey(u => u.Id);
    
    modelBuilder.Entity<RecipeIngredient>()
        .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

    modelBuilder.Entity<RecipeIngredient>()
        .HasOne(ri => ri.Recipe)
        .WithMany(r => r.RecipeIngredients)
        .HasForeignKey(ri => ri.RecipeId);

    modelBuilder.Entity<RecipeIngredient>()
        .HasOne(ri => ri.Ingredient)
        .WithMany(i => i.RecipeIngredients)
        .HasForeignKey(ri => ri.IngredientId);

    modelBuilder.Entity<UserIngredient>()
        .HasKey(ui => new { ui.AppUserId, ui.IngredientId });

    modelBuilder.Entity<UserIngredient>()
        .HasOne(ui => ui.AppUser)
        .WithMany(u => u.UserIngredients)
        .HasForeignKey(ui => ui.AppUserId);

    modelBuilder.Entity<UserIngredient>()
        .HasOne(ui => ui.Ingredient)
        .WithMany(i => i.UserIngredients)
        .HasForeignKey(ui => ui.IngredientId);

    base.OnModelCreating(modelBuilder);
}
}
