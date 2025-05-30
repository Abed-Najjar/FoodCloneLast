using API.AppResponse;
using API.DTOs;
using API.Models;
using API.UoW;

public class DishManagementService : IDishManagementService
{
    private readonly IUnitOfWork _unitOfWork;

    public DishManagementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppResponse<AdminRestaurantDishDto>> CreateDish(CreateDishDto dishDto)
    {
        try
        {
            var restaurant = await _unitOfWork.RestaurantRepository.GetRestaurantByIdAsync(dishDto.RestaurantId);
            if (restaurant == null)
            {
                return new AppResponse<AdminRestaurantDishDto>(null, "Restaurant not found", 404, false);
            }

            if (dishDto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(dishDto.CategoryId.Value);
                if (category == null)
                {
                    return new AppResponse<AdminRestaurantDishDto>(null, "Category not found", 404, false);
                }
            }

            var dish = new Dish
            {
                Name = dishDto.Name,
                Description = dishDto.Description,
                Price = dishDto.Price,
                ImageUrl = dishDto.ImageUrl,
                RestaurantId = dishDto.RestaurantId,
                Restaurant = restaurant,
                CategoryId = dishDto.CategoryId,
                IsAvailable = dishDto.IsAvailable
            };
            
            await _unitOfWork.DishRepository.AddDishAsync(dish);
            await _unitOfWork.CompleteAsync();

            var responseDto = new AdminRestaurantDishDto
            {
                Id = dish.Id,
                Name = dish.Name,
                Description = dish.Description,
                Price = dish.Price,
                ImageUrl = dish.ImageUrl,
                RestaurantId = dish.RestaurantId,
                RestaurantName = restaurant.Name,
                CategoryId = dish.CategoryId,
                CategoryName = dish.Category?.Name,
                IsAvailable = dish.IsAvailable
            };

            return new AppResponse<AdminRestaurantDishDto>(responseDto, "Dish created successfully", 201, true);
        }
        catch (Exception ex)
        {
            return new AppResponse<AdminRestaurantDishDto>(null, ex.Message, 500, false);
        }
    }

    public async Task<AppResponse<bool>> DeleteDish(int id)
    {
        try
        {
            var dish = await _unitOfWork.DishRepository.GetDishByIdAsync(id);
            if (dish == null)
            {
                return new AppResponse<bool>(false, "Dish not found", 404, false);
            }
            
            await _unitOfWork.DishRepository.DeleteDishAsync(dish.Id);
            await _unitOfWork.CompleteAsync();

            return new AppResponse<bool>(true, "Dish deleted successfully", 200, true);
        }
        catch (Exception ex)
        {
            return new AppResponse<bool>(false, ex.Message, 500, false);
        }
    }

    public async Task<AppResponse<AdminRestaurantDishDto>> GetDishById(int id)
    {
        try
        {
            var dish = await _unitOfWork.DishRepository.GetDishByIdWithIncludesAsync(id);

            if (dish == null)
            {
                return new AppResponse<AdminRestaurantDishDto>(null, "Dish not found", 404, false);
            }
            
            var dishDto = new AdminRestaurantDishDto
            {
                Id = dish.Id,
                Name = dish.Name,
                Description = dish.Description,
                Price = dish.Price,
                ImageUrl = dish.ImageUrl,
                RestaurantId = dish.RestaurantId,
                RestaurantName = dish.Restaurant.Name,
                CategoryId = dish.CategoryId,
                CategoryName = dish.Category?.Name,
                IsAvailable = dish.IsAvailable
            };

            return new AppResponse<AdminRestaurantDishDto>(dishDto, "Dish retrieved successfully", 200, true);
        }
        catch (Exception ex)
        {
            return new AppResponse<AdminRestaurantDishDto>(null, ex.Message, 500, false);
        }
    }

    public async Task<AppResponse<List<AdminRestaurantDishDto>>> GetDishesInRestaurant(int restaurantId)
    {
        try
        {
            var dishes = await _unitOfWork.DishRepository.GetDishesByRestaurantIdAsync(restaurantId);

            if (!dishes.Any())
            {
                return new AppResponse<List<AdminRestaurantDishDto>>(null, "No dishes found for this restaurant", 404, false);
            }

            var dishDtos = dishes.Select(d => new AdminRestaurantDishDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Price = d.Price,
                ImageUrl = d.ImageUrl,
                RestaurantId = d.RestaurantId,
                RestaurantName = d.Restaurant.Name,
                CategoryId = d.CategoryId,
                CategoryName = d.Category?.Name,
                IsAvailable = d.IsAvailable
            }).ToList();

            return new AppResponse<List<AdminRestaurantDishDto>>(dishDtos, "Dishes retrieved successfully", 200, true);
        }
        catch (Exception ex)
        {
            return new AppResponse<List<AdminRestaurantDishDto>>(null, ex.Message, 500, false);
        }
    }

    public async Task<AppResponse<List<AdminRestaurantDishDto>>> GetDishesByCategory(int categoryId)
    {
        try
        {
            var dishes = await _unitOfWork.DishRepository.GetDishesByCategoryIdAsync(categoryId);

            if (!dishes.Any())
            {
                return new AppResponse<List<AdminRestaurantDishDto>>(null, "No dishes found for this category", 404, false);
            }

            var dishDtos = dishes.Select(d => new AdminRestaurantDishDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Price = d.Price,
                ImageUrl = d.ImageUrl,
                RestaurantId = d.RestaurantId,
                RestaurantName = d.Restaurant.Name,
                CategoryId = d.CategoryId,
                CategoryName = d.Category?.Name,
                IsAvailable = d.IsAvailable
            }).ToList();

            return new AppResponse<List<AdminRestaurantDishDto>>(dishDtos, "Dishes retrieved successfully", 200, true);
        }
        catch (Exception ex)
        {
            return new AppResponse<List<AdminRestaurantDishDto>>(null, ex.Message, 500, false);
        }
    }

    public async Task<AppResponse<AdminRestaurantDishDto>> UpdateDish(int id, UpdateDishDto dishDto)
    {
        try
        {
            var dish = await _unitOfWork.DishRepository.GetDishByIdWithIncludesAsync(id);

            if (dish == null)
            {
                return new AppResponse<AdminRestaurantDishDto>(null, "Dish not found", 404, false);
            }

            // Validate restaurant exists if restaurantId is being changed
            if (dishDto.RestaurantId != dish.RestaurantId)
            {
                var restaurant = await _unitOfWork.RestaurantRepository.GetRestaurantByIdAsync(dishDto.RestaurantId);
                if (restaurant == null)
                {
                    return new AppResponse<AdminRestaurantDishDto>(null, "Restaurant not found", 404, false);
                }
                dish.RestaurantId = dishDto.RestaurantId;
            }

            if (dishDto.Name != null) dish.Name = dishDto.Name;
            if (dishDto.Description != null) dish.Description = dishDto.Description;
            dish.Price = dishDto.Price;
            if (dishDto.ImageUrl != null) dish.ImageUrl = dishDto.ImageUrl;
            dish.IsAvailable = dishDto.IsAvailable;
            
            if (dishDto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(dishDto.CategoryId.Value);
                if (category == null)
                {
                    return new AppResponse<AdminRestaurantDishDto>(null, "Category not found", 404, false);
                }
                dish.CategoryId = dishDto.CategoryId;
            }

            await _unitOfWork.CompleteAsync();

            var responseDto = new AdminRestaurantDishDto
            {
                Id = dish.Id,
                Name = dish.Name,
                Description = dish.Description,
                Price = dish.Price,
                ImageUrl = dish.ImageUrl,
                RestaurantId = dish.RestaurantId,
                RestaurantName = dish.Restaurant.Name,
                CategoryId = dish.CategoryId,
                CategoryName = dish.Category?.Name,
                IsAvailable = dish.IsAvailable
            };

            return new AppResponse<AdminRestaurantDishDto>(responseDto, "Dish updated successfully", 200, true);
        }
        catch (Exception ex)
        {
            return new AppResponse<AdminRestaurantDishDto>(null, ex.Message, 500, false);
        }
    }
    
    public async Task<AppResponse<List<DishDto>>> GetAllDishesAsync()
    {
        try
        {
            var dishes = await _unitOfWork.DishRepository.GetAllDishesWithIncludesAsync();

            if (!dishes.Any())
            {
                return new AppResponse<List<DishDto>>(null, "No dishes found", 404, false);
            }
            
            var dishDtos = dishes.Select(d => new DishDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Price = d.Price,
                ImageUrl = d.ImageUrl,
                RestaurantId = d.RestaurantId,
                RestaurantName = d.Restaurant.Name,
                CategoryId = d.CategoryId,
                CategoryName = d.Category?.Name,
                IsAvailable = d.IsAvailable
            }).ToList();

            return new AppResponse<List<DishDto>>(dishDtos, "Dishes retrieved successfully", 200, true);
        }
        catch (Exception ex)
        {
            return new AppResponse<List<DishDto>>(null, ex.Message, 500, false);
        }
    }
}



