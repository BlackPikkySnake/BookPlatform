using BookPlatform.DTOs;
using BookPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookPlatform.Endpoints
{
    public class OrderEndpoints : IEndpointDefinition
    {
        public void MapEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/orders")
                .WithTags("Orders")
                .RequireAuthorization();

            // Корзина
            group.MapGet("/cart", GetCart)
                .WithName("GetCart");

            group.MapPost("/cart/add", AddToCart)
                .WithName("AddToCart");

            group.MapDelete("/cart/remove/{itemId:int}", RemoveFromCart)
                .WithName("RemoveFromCart");

            // Оформление заказа
            group.MapPost("/checkout", Checkout)
                .WithName("Checkout");

            // История заказов
            group.MapGet("/history", GetOrderHistory)
                .WithName("GetOrderHistory");
        }

        private static async Task<IResult> GetCart(
            HttpContext httpContext,
            IOrderService orderService)
        {
            var userId = GetUserId(httpContext);
            var cart = await orderService.GetCartAsync(userId);

            if (cart == null)
                return Results.Ok(new { message = "Cart is empty" });

            return Results.Ok(cart);
        }

        private static async Task<IResult> AddToCart(
            HttpContext httpContext,
            [FromBody] AddToCartDto addDto,
            IOrderService orderService)
        {
            try
            {
                var userId = GetUserId(httpContext);
                var cart = await orderService.AddToCartAsync(userId, addDto);
                return Results.Ok(cart);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }

        private static async Task<IResult> RemoveFromCart(
            int itemId,
            HttpContext httpContext,
            IOrderService orderService)
        {
            var userId = GetUserId(httpContext);
            var cart = await orderService.RemoveFromCartAsync(userId, itemId);
            return Results.Ok(cart);
        }

        private static async Task<IResult> Checkout(
            HttpContext httpContext,
            [FromBody] CheckoutDto checkoutDto,
            IOrderService orderService)
        {
            var userId = GetUserId(httpContext);
            var order = await orderService.CheckoutAsync(userId, checkoutDto);
            return Results.Ok(order);
        }

        private static async Task<IResult> GetOrderHistory(
            HttpContext httpContext,
            IOrderService orderService)
        {
            var userId = GetUserId(httpContext);
            var orders = await orderService.GetUserOrdersAsync(userId);
            return Results.Ok(orders);
        }

        private static int GetUserId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}