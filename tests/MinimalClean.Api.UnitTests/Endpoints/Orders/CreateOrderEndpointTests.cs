using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using MinimalClean.Api.Endpoints.Orders.Create;
using Moq;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace MinimalClean.Api.UnitTests.Endpoints.Orders;

public class CreateOrderEndpointTests
{
    [Fact]
    public async Task HandleAsync_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
        var validator = new Mock<IValidator<CreateOrderRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ValidationResult());

        var handler = new Mock<CreateOrderHandler>(MockBehavior.Strict, new Mock<Infrastructure.Persistence.Repositories.IOrderRepository>().Object);
        handler.Setup(h => h.Handle(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(Guid.NewGuid());

        var logger = new Mock<ILogger<CreateOrderEndpoint>>();

        var endpoint = new CreateOrderEndpoint(validator.Object, handler.Object, logger.Object);

        var ctx = new DefaultHttpContext();

        // Act
        var result = await endpoint.HandleAsync(new CreateOrderRequest("Alice", 100), ctx, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Created<string>>();
    }

    [Fact]
    public async Task HandleAsync_ReturnsBadRequest_WhenValidationFails()
    {
        var validator = new Mock<IValidator<CreateOrderRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("CustomerName", "Required") }));

        var handler = new Mock<CreateOrderHandler>(MockBehavior.Strict, new Mock<Infrastructure.Persistence.Repositories.IOrderRepository>().Object);

        var logger = new Mock<ILogger<CreateOrderEndpoint>>();

        var endpoint = new CreateOrderEndpoint(validator.Object, handler.Object, logger.Object);

        var ctx = new DefaultHttpContext();

        var result = await endpoint.HandleAsync(new CreateOrderRequest("", 0), ctx, CancellationToken.None);

        result.Should().BeOfType<BadRequest<object>>();
    }
}
