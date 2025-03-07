-- Seed Ingredients with various categories
INSERT INTO Ingredient (name, Category, Unit, DietTag) VALUES
-- Vegetables
('Carrot', 'Vegetables', 'pcs', 'Vegan'),
('Onion', 'Vegetables', 'pcs', 'Vegan'),
('Garlic', 'Vegetables', 'cloves', 'Vegan'),
('Tomato', 'Vegetables', 'pcs', 'Vegan'),
('Bell Pepper', 'Vegetables', 'pcs', 'Vegan'),
('Spinach', 'Vegetables', 'g', 'Vegan'),
('Potato', 'Vegetables', 'pcs', 'Vegan'),

-- Proteins
('Chicken Breast', 'Meat', 'g', 'Non-Vegetarian'),
('Salmon Fillet', 'Fish', 'g', 'Pescatarian'),
('Tofu', 'Protein', 'g', 'Vegan'),
('Ground Beef', 'Meat', 'g', 'Non-Vegetarian'),
('Chickpeas', 'Legumes', 'g', 'Vegan'),

-- Grains
('Rice', 'Grains', 'g', 'Vegan'),
('Pasta', 'Grains', 'g', 'Vegan'),
('Quinoa', 'Grains', 'g', 'Vegan'),

-- Dairy
('Cheese', 'Dairy', 'g', 'Vegetarian'),
('Milk', 'Dairy', 'ml', 'Vegetarian'),
('Yogurt', 'Dairy', 'g', 'Vegetarian'),

-- Spices
('Black Pepper', 'Spices', 'g', 'Vegan'),
('Salt', 'Spices', 'g', 'Vegan'),
('Cumin', 'Spices', 'g', 'Vegan'),
('Paprika', 'Spices', 'g', 'Vegan'),
('Oregano', 'Spices', 'g', 'Vegan');

-- Seed Recipes
INSERT INTO Recipe (name, data, cookingTime, servings, difficultyLevel) VALUES
-- Easy Recipes
('Classic Spaghetti Bolognese', 
'{"steps": ["1. Brown the ground beef", "Sauté onions and garlic", "Add tomatoes and herbs", "Cook pasta", "Combine and serve"]}',
30, 4, 'Easy'),

('Vegetarian Stir-Fry',
'{"steps": ["Prepare vegetables", "Cook rice", "Stir-fry vegetables", "Add sauce", "Serve over rice"]}',
25, 2, 'Easy'),

-- Medium Recipes
('Grilled Salmon with Quinoa',
'{"steps": ["Cook quinoa", "Season salmon", "Grill salmon", "Prepare vegetables", "Plate and serve"]}',
45, 2, 'Medium'),

('Chicken Curry',
'{"steps": ["Marinate chicken", "Prepare curry sauce", "Cook chicken", "Simmer in sauce", "Serve with rice"]}',
50, 4, 'Medium'),

-- Hard Recipes
('Homemade Pizza',
'{"steps": ["Prepare dough", "Make sauce", "Prepare toppings", "Assemble pizza", "Bake until golden"]}',
90, 4, 'Hard');

-- Link Recipes with Ingredients
-- Spaghetti Bolognese
INSERT INTO RecipeIngredient (recipeId, ingredientId, amount) VALUES
(1, 11, 500),  -- Ground Beef
(1, 2, 1),     -- Onion
(1, 3, 3),     -- Garlic
(1, 4, 4),     -- Tomato
(1, 14, 400),  -- Pasta
(1, 20, 5),    -- Salt
(1, 19, 3);    -- Black Pepper

-- Vegetarian Stir-Fry
INSERT INTO RecipeIngredient (recipeId, ingredientId, amount) VALUES
(2, 1, 2),     -- Carrot
(2, 5, 2),     -- Bell Pepper
(2, 6, 200),   -- Spinach
(2, 13, 300),  -- Rice
(2, 10, 200),  -- Tofu
(2, 20, 3),    -- Salt
(2, 19, 2);    -- Black Pepper

-- Grilled Salmon
INSERT INTO RecipeIngredient (recipeId, ingredientId, amount) VALUES
(3, 9, 400),   -- Salmon Fillet
(3, 15, 200),  -- Quinoa
(3, 1, 2),     -- Carrot
(3, 6, 100),   -- Spinach
(3, 20, 5),    -- Salt
(3, 19, 3),    -- Black Pepper
(3, 23, 2);    -- Oregano

-- Chicken Curry
INSERT INTO RecipeIngredient (recipeId, ingredientId, amount) VALUES
(4, 8, 600),   -- Chicken Breast
(4, 2, 2),     -- Onion
(4, 3, 4),     -- Garlic
(4, 13, 400),  -- Rice
(4, 21, 10),   -- Cumin
(4, 22, 5),    -- Paprika
(4, 20, 5);    -- Salt

-- Homemade Pizza
INSERT INTO RecipeIngredient (recipeId, ingredientId, amount) VALUES
(5, 4, 4),     -- Tomato
(5, 16, 200),  -- Cheese
(5, 3, 2),     -- Garlic
(5, 23, 3),    -- Oregano
(5, 20, 5),    -- Salt
(5, 19, 3);    -- Black Pepper 