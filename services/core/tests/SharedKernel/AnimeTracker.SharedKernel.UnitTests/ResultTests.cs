using AnimeTracker.SharedKernel;
using Shouldly;

namespace AnimeTracker.SharedKernel.UnitTests;

public sealed class ResultOfTTests
{
    [Fact]
    public void Success_sets_IsSuccess_and_Value()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe(42);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_sets_IsFailure_and_Error()
    {
        var result = Result<int>.Failure("something went wrong");

        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("something went wrong");
    }

    [Fact]
    public void Failure_accessing_Value_throws_InvalidOperationException()
    {
        var result = Result<int>.Failure("error");

        Should.Throw<InvalidOperationException>(() => _ = result.Value)
            .Message.ShouldBe("Cannot access Value of a failed result.");
    }
}

public sealed class ResultTests
{
    [Fact]
    public void Success_sets_IsSuccess()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_sets_IsFailure_and_Error()
    {
        var result = Result.Failure("error message");

        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("error message");
    }
}
