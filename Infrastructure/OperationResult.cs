namespace YumeChan.DreamJockey.Infrastructure;

/// <summary>
/// Represents the result of an operation.
/// </summary>
/// <param name="Status">Status of the Operation</param>
/// <param name="Message">(Optional) Message describing the operation's result</param>
public record OperationResult(OperationStatus Status, string? Message = null)
{
	public static implicit operator OperationResult(OperationStatus status) => new(status);
}

/// <summary>
/// Represents the result of an operation.
/// </summary>
/// <param name="Status">Status of the Operation</param>
/// <param name="Result">Result value of the Operation</param>
/// <param name="Message">(Optional) Message describing the operation's result</param>
/// <typeparam name="TResult">Type of the Operation's Result</typeparam>
public record OperationResult<TResult>(OperationStatus Status, TResult? Result, string? Message = null) : OperationResult(Status, Message)
{
	public static implicit operator TResult?(OperationResult<TResult> result) => result.Result;
	public static implicit operator OperationResult<TResult>(TResult result) => new(OperationStatus.Success, result);
}

/// <summary>
/// Represents the status of an operation.
/// </summary>
public enum OperationStatus
{
	/// <summary>
	/// Operation was successful.
	/// </summary>
	Success,
	
	/// <summary>
	/// Operation failed.
	/// </summary>
	Failure,
	
	/// <summary>
	/// Operation succeeded, but with warnings.
	/// </summary>
	/// <remarks>
	///	Any operation returning this status should have a message describing the warnings,
	/// and consumers should communicate the message to the user.
	/// </remarks>
	Warning
}

/// <summary>
/// Provides static factory methods for creating loose/typed <see cref="OperationResult"/>s.
/// </summary>
public static class OperationResults
{
	/// <summary>
	/// Creates a successful operation result.
	/// </summary>
	/// <param name="message">(Optional) Message describing the operation's result</param>
	/// <returns>A successful operation result</returns>
	public static OperationResult Success(string? message = null) => new(OperationStatus.Success, message);
	
	/// <summary>
	/// Creates a failed operation result.
	/// </summary>
	/// <param name="message">(Optional) Message describing the operation's result</param>
	/// <returns>A failed operation result</returns>
	public static OperationResult Failure(string? message = null) => new(OperationStatus.Failure, message);
	
	/// <summary>
	/// Creates a successful operation result.
	/// </summary>
	/// <param name="result">Result value of the operation</param>
	/// <param name="message">(Optional) Message describing the operation's result</param>
	/// <returns>A successful operation result</returns>
	/// <typeparam name="TResult">Type of the Operation's Result</typeparam>
	public static OperationResult<TResult> Success<TResult>(TResult? result, string? message = null) => new(OperationStatus.Success, result, message);
	
	/// <summary>
	/// Creates a failed operation result.
	/// </summary>
	/// <param name="result">Result value of the operation</param>
	/// <param name="message">(Optional) Message describing the operation's result</param>
	/// <returns>A failed operation result</returns>
	/// <typeparam name="TResult">Type of the Operation's Result</typeparam>
	public static OperationResult<TResult> Failure<TResult>(TResult? result, string? message = null) => new(OperationStatus.Failure, result, message);
}