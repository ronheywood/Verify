﻿[UsesVerify]
public class RecordingTests
{
    [Fact]
    public Task Simple()
    {
        Recording.Start();
        Recording.Add("name", "value");
        return Verify("TheValue");
    }

    [Fact]
    public Task SimpleIdentifier()
    {
        Recording.Start("identifier");
        Recording.Add("identifier", "name", "value");
        return Verify(Recording.Stop("identifier"));
    }

    [Fact]
    public void IsRecording()
    {
        Assert.False(Recording.IsRecording());
        Recording.Start();
        Assert.True(Recording.IsRecording());
    }

    [Fact]
    public void IsRecordingIdentifier()
    {
        Assert.False(Recording.IsRecording("identifier"));
        Recording.Start("identifier");
        Assert.True(Recording.IsRecording("identifier"));
    }

    [Fact]
    public Task NoRecording() =>
        Throws(() => Recording.Add("name", "value"))
            .IgnoreStackTrace();

    [Fact]
    public Task NoRecordingIdentifier() =>
        Throws(() => Recording.Add("identifier", "name", "value"))
            .IgnoreStackTrace();

    [Fact]
    public Task Stop()
    {
        Recording.Start();
        Recording.Add("name", "value");
        return Verify(Recording.Stop());
    }

    [Fact]
    public Task StopIdentifier()
    {
        Recording.Start("identifier");
        Recording.Add("identifier", "name", "value");
        return Verify(Recording.Stop("identifier"));
    }

    [Fact]
    public Task TryStopNoStart()
    {
        var result = Recording.TryStop(out var recorded);
        return Verify(
            new
            {
                result,
                recorded
            });
    }

    [Fact]
    public Task TryStopNoStartIdentifier()
    {
        var result = Recording.TryStop("identifier", out var recorded);
        return Verify(
            new
            {
                result,
                recorded
            });
    }

    [Fact]
    public Task Clear()
    {
        Recording.Start();
        Recording.Add("name1", "value");
        Recording.Clear();
        Recording.Add("name2", "value");
        return Verify("TheValue");
    }

    [Fact]
    public Task ClearIdentifier()
    {
        Recording.Start("identifier");
        Recording.Add("identifier", "name1", "value");
        Recording.Clear("identifier");
        Recording.Add("identifier", "name2", "value");
        return Verify(Recording.Stop("identifier"));
    }


    [Fact]
    public Task PauseResume()
    {
        Recording.Start();
        Recording.Pause();
        Recording.Add("name1", "value");
        Recording.Resume();
        Recording.Add("name2", "value");
        Recording.Pause();
        Recording.Add("name3", "value");
        return Verify("TheValue");
    }

    [Fact]
    public Task PauseResumeIdentifier()
    {
        Recording.Start("identifier");
        Recording.Pause("identifier");
        Recording.Add("identifier", "name1", "value");
        Recording.Resume("identifier");
        Recording.Add("identifier", "name2", "value");
        Recording.Pause("identifier");
        Recording.Add("identifier", "name3", "value");
        return Verify(Recording.Stop("identifier"));
    }

    [Fact]
    public Task NoValue()
    {
        Recording.Start();
        Recording.Add("name", "value");
        return Verify();
    }

    [Fact]
    public Task Multiple()
    {
        Recording.Start();
        Recording.Add("name1", "value1");
        Recording.Add("name2", "value2");
        return Verify("TheValue");
    }
    [Fact]
    public Task MultipleIdentifier()
    {
        Recording.Start("identifier");
        Recording.Add("identifier","name1", "value1");
        Recording.Add("identifier","name2", "value2");
        return Verify(Recording.Stop("identifier"));
    }

    [Fact]
    public Task Append()
    {
        Recording.Start();
        Recording.Add("name", "value1");
        Recording.Add("name", "value2");
        return Verify("TheValue");
    }
    [Fact]
    public Task AppendIdentifier()
    {
        Recording.Start("identifier");
        Recording.Add("identifier", "name", "value1");
        Recording.Add("identifier", "name", "value2");
        return Verify(Recording.Stop("identifier"));
    }

    [Fact]
    public Task Case()
    {
        Recording.Start();
        Recording.Add("name", "value1");
        Recording.Add("Name", "value2");
        return Verify("TheValue");
    }

    [Fact]
    public Task CaseIdentifier()
    {
        Recording.Start("identifier");
        Recording.Add("identifier", "name", "value1");
        Recording.Add("identifier", "Name", "value2");
        return Verify(Recording.Stop("identifier"));
    }
}