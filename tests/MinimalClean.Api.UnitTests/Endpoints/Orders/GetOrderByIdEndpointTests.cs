using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using MinimalClean.Api.Endpoints.Orders;
using MinimalClean.Api.Endpoints.Orders.GetById;
using Moq;

namespace MinimalClean.Api.UnitTests.Endpoints.Orders;

public class GetOrderByIdEndpointTests
{
    [Fact]
    public async Task HandleAsync_ReturnsOk_WhenOrderExists()
    {
        var handler = new Mock<GetOrderByIdHandler>(MockBehavior.Strict, new Mock<Infrastructure.Persistence.Repositories.IOrderRepository>().Object);
        handler.Setup(h => h.Handle(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new OrderDto(Guid.NewGuid(), "Alice", 100, "Pending"));

        var logger = new Mock<ILogger<GetOrderByIdEndpoint>>();

        var endpoint = new GetOrderByIdEndpoint(handler.Object, logger.Object);

        var result = await endpoint.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<Ok<OrderDto>>();
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFound_WhenOrderMissing()
    {
        var handler = new Mock<GetOrderByIdHandler>(MockBehavior.Strict, new Mock<Infrastructure.Persistence.Repositories.IOrderRepository>().Object);
        handler.Setup(h => h.Handle(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((OrderDto?)null);

        var logger = new Mock<ILogger<GetOrderByIdEndpoint>>();

        var endpoint = new GetOrderByIdEndpoint(handler.Object, logger.Object);


        var result = await endpoint.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFound<string>>();
    }
}