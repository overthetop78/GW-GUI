using GWGUI.MediaFileSystems.Exploration;

namespace GWGUI.Tests.Media;

public sealed class FileEnumerationTests
{
    [Fact]
    public void KeepsFileIdentityAndDepthFirstOrderIncludingChildrenOfFiles()
    {
        var nested = new Entry("nested", true, []);
        var first = new Entry("first", true, [nested]);
        var last = new Entry("last", true, []);
        var root = new Entry("root", false,
            [new Entry("empty", false, []), first, new Entry("folder", false, [last])]);

        var files = MediaExplorer.EnumerateFiles(root, static item => item.IsFile,
            static item => item.Children).ToArray();

        Assert.Equal(3, files.Length);
        Assert.Same(first, files[0]);
        Assert.Same(nested, files[1]);
        Assert.Same(last, files[2]);
    }

    [Fact]
    public void DefersPredicatesAndChildAccessUntilTheNextItemIsRequested()
    {
        var child = new Entry("child", true, []);
        var root = new Entry("root", true, [child]);
        var calls = new List<string>();
        var files = MediaExplorer.EnumerateFiles(root,
            item => { calls.Add("file:" + item.Name); return item.IsFile; },
            item => { calls.Add("children:" + item.Name); return item.Children; });
        Assert.Empty(calls);
        var enumerator = files.GetEnumerator();
        try
        {
            Assert.Empty(calls);
            Assert.True(enumerator.MoveNext());
            Assert.Same(root, enumerator.Current);
            Assert.Equal(["file:root"], calls);
            Assert.True(enumerator.MoveNext());
            Assert.Same(child, enumerator.Current);
            Assert.Equal(["file:root", "children:root", "file:child"], calls);
        }
        finally { enumerator.Dispose(); }
        Assert.Equal(["file:root", "children:root", "file:child"], calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PropagatesTheDeferredFailureWithoutVisitingLaterEntries(bool failPredicate)
    {
        var root = new Entry("root", true, []);
        var expected = new InvalidOperationException("Enumeration failed.");
        var childrenRead = false;
        var files = MediaExplorer.EnumerateFiles(root,
            item => failPredicate ? throw expected : item.IsFile,
            _ => { childrenRead = true; throw expected; });
        var enumerator = files.GetEnumerator();
        try
        {
            if (!failPredicate)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Same(root, enumerator.Current);
            }
            var observed = Assert.Throws<InvalidOperationException>(() => { _ = enumerator.MoveNext(); });
            Assert.Same(expected, observed);
            Assert.Equal(!failPredicate, childrenRead);
        }
        finally { enumerator.Dispose(); }
    }

    private sealed record Entry(string Name, bool IsFile, IReadOnlyList<Entry> Children);
}
