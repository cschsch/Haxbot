namespace Haxbot;

public static class TaskExtensions
{
    public static Task<TResult> Bind<TSource, TResult>(this Task<TSource> source, Func<TSource, Task<TResult>> bind)
    {
        return source.ContinueWith(async x => await bind(await x)).Unwrap();
    }

    public static Task<TResult> Map<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> map)
    {
        return source.Bind(x => Task.FromResult(map(x)));
    }
}
