namespace ProjectBrain.Api.Common;

/// <summary>
/// 统一接口返回结构。前端约定：code == 0 表示成功。
/// </summary>
public class ApiResult<T>
{
    public int Code { get; set; }

    public string Message { get; set; } = "ok";

    public T? Data { get; set; }

    public static ApiResult<T> Ok(T? data, string message = "ok")
        => new() { Code = 0, Message = message, Data = data };

    public static ApiResult<T> Fail(string message, int code = 1)
        => new() { Code = code, Message = message, Data = default };
}

/// <summary>
/// 无数据载荷的快捷返回。
/// </summary>
public static class ApiResult
{
    public static ApiResult<object?> Ok(string message = "ok")
        => new() { Code = 0, Message = message, Data = null };

    public static ApiResult<object?> Fail(string message, int code = 1)
        => new() { Code = code, Message = message, Data = null };
}
