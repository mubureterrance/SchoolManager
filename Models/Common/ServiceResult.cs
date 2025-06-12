namespace SchoolManager.Models.Common
{
    /// <summary>
    /// Generic result wrapper for service operations
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Creates a successful result with data
        /// </summary>
        public static ServiceResult<T> Success(T data, string? message = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// Creates a successful result without data
        /// </summary>
        public static ServiceResult<T> Success(string? message = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Message = message
            };
        }

        /// <summary>
        /// Creates a failed result with error message
        /// </summary>
        public static ServiceResult<T> Failure(string error, string? errorCode = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Errors = new List<string> { error },
                ErrorCode = errorCode
            };
        }

        /// <summary>
        /// Creates a failed result with multiple error messages
        /// </summary>
        public static ServiceResult<T> Failure(List<string> errors, string? errorCode = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Errors = errors,
                ErrorCode = errorCode
            };
        }

        /// <summary>
        /// Creates a failed result with exception
        /// </summary>
        public static ServiceResult<T> Failure(Exception exception, string? errorCode = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Errors = new List<string> { exception.Message },
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// Generic paged result wrapper for paginated data
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    public class PagedResult2<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int StartIndex => (PageNumber - 1) * PageSize + 1;
        public int EndIndex => Math.Min(PageNumber * PageSize, TotalCount);

        /// <summary>
        /// Creates a paged result
        /// </summary>
        public static PagedResult2<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult2<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Creates an empty paged result
        /// </summary>
        public static PagedResult2<T> Empty(int pageNumber = 1, int pageSize = 10)
        {
            return new PagedResult2<T>
            {
                Items = new List<T>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }

    /// <summary>
    /// Response wrapper for API endpoints
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? ErrorCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a successful API response
        /// </summary>
        public static ApiResponse<T> Ok(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// Creates a successful API response without data
        /// </summary>
        public static ApiResponse<T> Ok(string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message
            };
        }

        /// <summary>
        /// Creates a failed API response
        /// </summary>
        public static ApiResponse<T> Error(string error, string? errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = new List<string> { error },
                ErrorCode = errorCode
            };
        }

        /// <summary>
        /// Creates a failed API response with multiple errors
        /// </summary>
        public static ApiResponse<T> Error(List<string> errors, string? errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = errors,
                ErrorCode = errorCode
            };
        }

        /// <summary>
        /// Creates an API response from a service result
        /// </summary>
        public static ApiResponse<T> FromServiceResult(ServiceResult<T> serviceResult)
        {
            return new ApiResponse<T>
            {
                Success = serviceResult.IsSuccess,
                Data = serviceResult.Data,
                Message = serviceResult.Message,
                Errors = serviceResult.Errors,
                ErrorCode = serviceResult.ErrorCode
            };
        }
    }

    /// <summary>
    /// Common error codes used throughout the application
    /// </summary>
    public static class ErrorCodes
    {
        // General
        public const string NotFound = "NOT_FOUND";
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string Conflict = "CONFLICT";
        public const string InternalError = "INTERNAL_ERROR";

        // Parent specific
        public const string ParentNotFound = "PARENT_NOT_FOUND";
        public const string ParentAlreadyExists = "PARENT_ALREADY_EXISTS";
        public const string ParentInactive = "PARENT_INACTIVE";
        public const string ParentHasActiveChildren = "PARENT_HAS_ACTIVE_CHILDREN";

        // Student specific
        public const string StudentNotFound = "STUDENT_NOT_FOUND";
        public const string StudentAlreadyExists = "STUDENT_ALREADY_EXISTS";
        public const string StudentInactive = "STUDENT_INACTIVE";

        // Relationship specific
        public const string RelationshipNotFound = "RELATIONSHIP_NOT_FOUND";
        public const string RelationshipAlreadyExists = "RELATIONSHIP_ALREADY_EXISTS";
        public const string PrimaryContactExists = "PRIMARY_CONTACT_EXISTS";
        public const string NoPrimaryContact = "NO_PRIMARY_CONTACT";
        public const string CannotRemoveLastParent = "CANNOT_REMOVE_LAST_PARENT";
        public const string InvalidRelationshipType = "INVALID_RELATIONSHIP_TYPE";
    }
}
