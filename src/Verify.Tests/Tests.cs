﻿// Non-nullable field is uninitialized.

#pragma warning disable CS8618

[UsesVerify]
public class Tests
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.AddExtraDatetimeFormat("F");
        VerifierSettings.AddExtraDatetimeOffsetFormat("F");
    }

    [Theory]
    [InlineData("a")]
    public Task ReplaceInvalidParamChar(string value) =>
        Verify("foo")
            .UseParameters(Path.GetInvalidPathChars().First());

    [Theory]
    [InlineData(1, 2)]
    public async Task IncorrectParameterCount_TooFew(int one, int two)
    {
        var exception = await Assert.ThrowsAsync<Exception>(() => Verify("Value").UseParameters(1));
        Assert.Equal("The number of passed in parameters (1) must match the number of parameters for the method (2).", exception.Message);
    }

    [Theory]
    [InlineData(1, 2)]
    public async Task IncorrectParameterCount_TooMany(int one, int two)
    {
        var exception = await Assert.ThrowsAsync<Exception>(() => Verify("Value").UseParameters(1, 2, 3));
        Assert.Equal("The number of passed in parameters (3) must match the number of parameters for the method (2).", exception.Message);
    }

    [Theory]
    [InlineData(1000.9999d)]
    public async Task LocalizedParam(decimal value)
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
        try
        {
            await Verify(value)
                .UseParameters(value);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }

    [Fact]
    public Task WithNewline() =>
        Verify(new {Property = "F\roo"});

    [ModuleInitializer]
    public static void TreatAsStringInit() =>
        VerifierSettings.TreatAsString<ClassWithToString>(
            (target, _) => target.Property);

    [Fact]
    public Task TreatAsString() =>
        Verify(new ClassWithToString {Property = "Foo"});

    class ClassWithToString
    {
        public string Property { get; set; } = null!;
    }

    [Fact]
    // ReSharper disable once IdentifierTypo
    public Task MisMatchcase()
    {
        if (OperatingSystem.IsLinux())
        {
            // No way to caseles File.Exists https://github.com/dotnet/core/issues/4596 on linux
            return Task.CompletedTask;
        }
        return Verify("Value");
    }

    [Fact]
    public async Task OnVerifyMismatch()
    {
        var settings = new VerifySettings();
        settings.DisableDiff();
        var onFirstVerifyCalled = false;
        var onVerifyMismatchCalled = false;
        VerifierSettings.OnFirstVerify(
            filePair =>
            {
                if (filePair.Name.Contains("OnVerifyMismatch"))
                {
                    onFirstVerifyCalled = true;
                }

                return Task.CompletedTask;
            });
        VerifierSettings.OnVerifyMismatch(
            (filePair, _) =>
            {
                if (filePair.Name.Contains("OnVerifyMismatch"))
                {
                    Assert.NotEmpty(filePair.ReceivedPath);
                    Assert.NotNull(filePair.ReceivedPath);
                    Assert.NotEmpty(filePair.VerifiedPath);
                    Assert.NotNull(filePair.VerifiedPath);
                    onVerifyMismatchCalled = true;
                }

                return Task.CompletedTask;
            });
        await Assert.ThrowsAsync<VerifyException>(() => Verify("value", settings));
        Assert.False(onFirstVerifyCalled);
        Assert.True(onVerifyMismatchCalled);
    }

    [Fact]
    public async Task OnFirstVerify()
    {
        var settings = new VerifySettings();
        settings.DisableDiff();
        var onFirstVerifyCalled = false;
        var onVerifyMismatchCalled = false;
        VerifierSettings.OnFirstVerify(
            filePair =>
            {
                if (filePair.Name.Contains("OnFirstVerify"))
                {
                    Assert.NotEmpty(filePair.ReceivedPath);
                    Assert.NotNull(filePair.ReceivedPath);
                    onFirstVerifyCalled = true;
                }

                return Task.CompletedTask;
            });
        VerifierSettings.OnVerifyMismatch(
            (filePair, _) =>
            {
                if (filePair.Name.Contains("OnFirstVerify"))
                {
                    onVerifyMismatchCalled = true;
                }

                return Task.CompletedTask;
            });
        await Assert.ThrowsAsync<VerifyException>(() => Verify("value", settings));
        Assert.True(onFirstVerifyCalled);
        Assert.False(onVerifyMismatchCalled);
    }

    [ModuleInitializer]
    public static void SettingsArePassedInit() =>
        VerifierSettings.RegisterStreamComparer(
            "SettingsArePassed",
            (_, _, _) => Task.FromResult(new CompareResult(true)));

    [Fact]
    public async Task SettingsArePassed()
    {
        var settings = new VerifySettings();
        settings.UseExtension("SettingsArePassed");
        await Verify(new MemoryStream(new byte[] {1}), settings)
            .UseExtension("SettingsArePassed");
    }

    [Fact]
    public Task Throws() =>
        Verifier.Throws(MethodThatThrows);

    static void MethodThatThrows() =>
        throw new("The Message");

    [Fact]
    public Task ThrowsNested() =>
        Verifier.Throws(Nested.MethodThatThrows);

    static class Nested
    {
        public static void MethodThatThrows() =>
            throw new("The Message");
    }

    [Fact]
    public Task ThrowsArgumentException() =>
        Verifier.Throws(MethodThatThrowsArgumentException);

    static void MethodThatThrowsArgumentException() =>
        throw new ArgumentException("The Message", "The parameter");

    [Fact]
    public Task ThrowsInheritedArgumentException() =>
        Verifier.Throws(MethodThatThrowsArgumentNullException);

    static void MethodThatThrowsArgumentNullException() =>
        throw new ArgumentNullException("The parameter", "The Message");

    [Fact]
    public Task ThrowsAggregate()
    {
        var settings = new VerifySettings();
        settings.UniqueForRuntime();
        return Verifier.Throws(MethodThatThrowsAggregate, settings);
    }

    static void MethodThatThrowsAggregate() =>
        throw new AggregateException(new Exception("The Message1"), new Exception("The Message2"));

    [Fact]
    public Task ThrowsTask() =>
        Verifier.ThrowsTask(TaskMethodThatThrows)
            .UniqueForRuntime()
            .ScrubLinesContaining("ThrowsAsync");

    static Task TaskMethodThatThrows() =>
        throw new("The Message");

    [Fact]
    public Task ThrowsTaskGeneric() =>
        Verifier.ThrowsTask(TaskMethodThatThrowsGeneric)
            .UniqueForRuntime()
            .ScrubLinesContaining("ThrowsAsync");

    static Task<string> TaskMethodThatThrowsGeneric() =>
        throw new("The Message");

    [Fact]
    public Task ThrowsValueTask() =>
        Verifier.ThrowsValueTask(ValueTaskMethodThatThrows)
            .UniqueForRuntime()
            .ScrubLinesContaining("ThrowsAsync");

    static ValueTask ValueTaskMethodThatThrows() =>
        throw new("The Message");

    [Fact]
    public Task ThrowsValueTaskGeneric() =>
        Verifier.ThrowsValueTask(ValueTaskMethodThatThrowsGeneric)
            .UniqueForRuntime()
            .ScrubLinesContaining("ThrowsAsync");

    static ValueTask<string> ValueTaskMethodThatThrowsGeneric() =>
        throw new("The Message");

    [Fact]
    public Task StringBuilder() =>
        Verify(new StringBuilder("value"));

    [Fact]
    public Task NestedStringBuilder() =>
        Verify(new {StringBuilder = new StringBuilder("value")});

    [Fact]
    public Task TextWriter()
    {
        var target = new StringWriter();
        target.Write("content");
        return Verify(target);
    }

    [Fact]
    public Task NestedTextWriter()
    {
        var target = new StringWriter();
        target.Write("content");
        return Verify(new {target});
    }

#if NET6_0
    [Fact]
    public async Task StringWithDifferingNewline()
    {
        var fullPath = Path.GetFullPath("../../../Tests.StringWithDifferingNewline.verified.txt");
        File.Delete(fullPath);
        File.WriteAllText(fullPath, "a\r\nb");
        await Verify("a\r\nb");
        PrefixUnique.Clear();
        await Verify("a\rb");
        PrefixUnique.Clear();
        await Verify("a\nb");
        PrefixUnique.Clear();

        File.Delete(fullPath);
        File.WriteAllText(fullPath, "a\nb");
        await Verify("a\r\nb");
        PrefixUnique.Clear();
        await Verify("a\rb");
        PrefixUnique.Clear();
        await Verify("a\nb");
        PrefixUnique.Clear();

        File.Delete(fullPath);
        File.WriteAllText(fullPath, "a\rb");
        await Verify("a\r\nb");
        PrefixUnique.Clear();
        await Verify("a\rb");
        PrefixUnique.Clear();
        await Verify("a\nb");
    }
#endif

    [Fact]
    public Task Stream() =>
        Verify(new MemoryStream(new byte[] {1}));

    [Fact]
    public Task StreamNotAtStart()
    {
        var stream = new MemoryStream(new byte[] {1, 2, 3, 4});
        stream.Position = 2;
        return Verify(stream);
    }

    [Fact]
    public Task StreamNotAtStartAsText()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("foo"));
        stream.Position = 2;
        return Verify(stream).UseExtension("txt");
    }

    [Fact]
    public Task Streams() =>
        Verify(
            new List<Stream>
            {
                new MemoryStream(new byte[] {1}),
                new MemoryStream(new byte[] {2})
            });

    [Fact]
    public Task StreamsWithNull() =>
        Verify(
            new List<Stream?>
            {
                new MemoryStream(new byte[] {1}),
                null
            });

    [Fact]
    public async Task ShouldNotIgnoreCase()
    {
        await Verify("A");
        var settings = new VerifySettings();
        settings.DisableDiff();
        PrefixUnique.Clear();
        await Assert.ThrowsAsync<VerifyException>(() => Verify("a", settings));
    }

    [Fact]
    public Task Newlines() =>
        Verify("a\r\nb\nc\rd\r\n");

    [Fact]
    public async Task TrailingNewlinesRaw()
    {
        var file = Path.Combine(FileEx.GetFileDirectory(), "Tests.TrailingNewlinesRaw.verified.txt");
        File.Delete(file);
        var settings = new VerifySettings();
        settings.DisableRequireUniquePrefix();

        File.WriteAllText(file, "a\r\n");
        await Verify("a\r\n", settings);
        await Verify("a\n", settings);
        await Verify("a", settings);

        File.WriteAllText(file, "a\r\n\r\n");
        await Verify("a\r\n\r\n", settings);
        await Verify("a\n\n", settings);
        await Verify("a\n", settings);

        File.WriteAllText(file, "a\n");
        await Verify("a\n", settings);
        await Verify("a", settings);

        File.WriteAllText(file, "a\n\n");
        await Verify("a\n\n", settings);
        await Verify("a\n", settings);
        File.Delete(file);
    }

    [Fact(Skip = "TODO")]
    public async Task TrailingNewlinesObject()
    {
        var file = Path.Combine(FileEx.GetFileDirectory(), "Tests.TrailingNewlinesObject.verified.txt");
        var settings = new VerifySettings();
        settings.DisableRequireUniquePrefix();
        var target = new
        {
            s = "a"
        };
        File.WriteAllText(file, "{\n  s: a\n}");
        await Verify(target, settings);

        File.WriteAllText(file, "{\n  s: a\r}");
        await Verify(target, settings);

        File.WriteAllText(file, "{\n  s: a\n}\n");
        await Verify(target, settings);

        File.WriteAllText(file, "{\n  s: a\n}\r\n");
        await Verify(target, settings);
    }

    [Fact]
    public async Task DanglingFiles()
    {
        var receivedFile = Path.Combine(FileEx.GetFileDirectory(), "Tests.DanglingFiles.01.received.txt");
        var verifiedFile = Path.Combine(FileEx.GetFileDirectory(), "Tests.DanglingFiles.01.verified.txt");
        File.WriteAllText(receivedFile,"");
        File.WriteAllText(verifiedFile,"");
        await Verify("value").AutoVerify();
        Assert.False(File.Exists(receivedFile));
        Assert.False(File.Exists(verifiedFile));
    }

    [Theory]
    [InlineData("param")]
    public async Task DanglingFilesIgnoreParametersForVerified(string param)
    {
        var receivedFile = Path.Combine(FileEx.GetFileDirectory(), "Tests.DanglingFilesIgnoreParametersForVerified_param=param.01.received.txt");
        var verifiedFile = Path.Combine(FileEx.GetFileDirectory(), "Tests.DanglingFilesIgnoreParametersForVerified.01.verified.txt");
        File.WriteAllText(receivedFile,"");
        File.WriteAllText(verifiedFile,"");
        await Verify("value").IgnoreParametersForVerified(param).AutoVerify();
        Assert.False(File.Exists(receivedFile));
        Assert.False(File.Exists(verifiedFile));
    }

    class Element
    {
        public string? Id { get; set; }
    }

    [Fact]
    public Task ShouldThrowForExtensionOnSerialization()
    {
        var settings = new VerifySettings();
        settings.UseExtension("json");
        settings.UseMethodName("Foo");
        settings.IgnoreStackTrack();
        settings.DisableDiff();

        var element = new Element();
        return Verifier.ThrowsTask(() => Verify(element, settings))
            .IgnoreStackTrack();
    }

    [Fact]
    public Task StringExtension()
    {
        var settings = new VerifySettings();
        settings.UseExtension("xml");

        return Verify("<a>b</a>", settings);
    }

    [Fact]
    public Task TaskResult()
    {
        var target = Task.FromResult("value");
        return Verify(target);
    }

    static async IAsyncEnumerable<string> AsyncEnumerableMethod()
    {
        await Task.Delay(1);
        yield return "one";
        await Task.Delay(1);
        yield return "two";
    }

    [Fact]
    public Task AsyncEnumerable() =>
        Verify(AsyncEnumerableMethod());

    static async IAsyncEnumerable<DisposableTarget> AsyncEnumerableDisposableMethod(DisposableTarget target)
    {
        await Task.Delay(1);
        yield return target;
    }

    [Fact]
    public async Task AsyncEnumerableDisposable()
    {
        var target = new DisposableTarget();
        await Verify(AsyncEnumerableDisposableMethod(target));
        Assert.True(target.Disposed);
    }

    static async IAsyncEnumerable<AsyncDisposableTarget> AsyncEnumerableAsyncDisposableMethod(AsyncDisposableTarget target)
    {
        await Task.Delay(1);
        yield return target;
    }

    [Fact]
    public async Task AsyncEnumerableAsyncDisposable()
    {
        var target = new AsyncDisposableTarget();
        await Verify(AsyncEnumerableAsyncDisposableMethod(target));
        Assert.True(target.AsyncDisposed);
    }

    [Fact]
    public async Task TaskResultAsyncDisposable()
    {
        var disposableTarget = new AsyncDisposableTarget();
        var target = Task.FromResult(disposableTarget);
        await Verify(target);
        Assert.True(disposableTarget.AsyncDisposed);
    }

    class AsyncDisposableTarget :
        IAsyncDisposable,
        IDisposable
    {
#pragma warning disable 414
        public string Property = "Value";
#pragma warning restore 414
        public bool AsyncDisposed;

        public ValueTask DisposeAsync()
        {
            AsyncDisposed = true;
            return new();
        }

        public void Dispose() =>
            throw new();
    }

    [Fact]
    public async Task TaskResultDisposable()
    {
        var disposableTarget = new DisposableTarget();
        var target = Task.FromResult(disposableTarget);
        await Verify(target);
        Assert.True(disposableTarget.Disposed);
    }

    class DisposableTarget :
        IDisposable
    {
#pragma warning disable 414
        public string Property = "Value";
#pragma warning restore 414
        public bool Disposed;

        public void Dispose() =>
            Disposed = true;
    }

#if !NETFRAMEWORK
    [Fact]
    public async Task VerifyBytesAsync()
    {
        var settings = new VerifySettings();
        settings.UseExtension("jpg");
        await Verify(File.ReadAllBytesAsync("sample.jpg"), settings);
    }
#endif

#if NET6_0
    [Fact]
    public async Task VerifyFilePath()
    {
        await VerifyFile("sample.txt");
        Assert.False(FileEx.IsFileLocked("sample.txt"));
    }
#endif

    [Fact]
    public async Task VerifyFileWithAppend() =>
        await VerifyFile("sample.txt")
            .AppendValue("key", "value");

    #region GetFilePath

    string GetFilePath([CallerFilePath] string sourceFile = "") =>
        sourceFile;

    #endregion

    //[Fact(Skip = "explicit")]
    //public async Task ShouldUseExtraSettings()
    //{
    //    ApplyExtraSettings(settings => { settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat; });

    //    var person = new Person
    //    {
    //        Dob = new DateTime(1980, 5, 5, 1, 1, 1)
    //    };
    //    DontScrubDateTimes();
    //    await Verify(person);
    //}
}