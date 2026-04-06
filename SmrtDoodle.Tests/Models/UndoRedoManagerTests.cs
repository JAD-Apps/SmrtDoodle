using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Models;

namespace SmrtDoodle.Tests;

[TestClass]
public class UndoRedoManagerTests
{
    [TestMethod]
    public void NewManager_CannotUndoOrRedo()
    {
        using var manager = new UndoRedoManager();
        Assert.IsFalse(manager.CanUndo);
        Assert.IsFalse(manager.CanRedo);
    }

    [TestMethod]
    public void Push_EnablesUndo()
    {
        using var manager = new UndoRedoManager();
        manager.Push(new MockUndoAction("Action 1"));
        Assert.IsTrue(manager.CanUndo);
        Assert.IsFalse(manager.CanRedo);
    }

    [TestMethod]
    public void Undo_EnablesRedo()
    {
        using var manager = new UndoRedoManager();
        manager.Push(new MockUndoAction("Action 1"));
        manager.Undo();
        Assert.IsFalse(manager.CanUndo);
        Assert.IsTrue(manager.CanRedo);
    }

    [TestMethod]
    public void Redo_AfterUndo_RestoresState()
    {
        using var manager = new UndoRedoManager();
        manager.Push(new MockUndoAction("Action 1"));
        manager.Undo();
        manager.Redo();
        Assert.IsTrue(manager.CanUndo);
        Assert.IsFalse(manager.CanRedo);
    }

    [TestMethod]
    public void Push_AfterUndo_ClearsRedoStack()
    {
        using var manager = new UndoRedoManager();
        manager.Push(new MockUndoAction("Action 1"));
        manager.Push(new MockUndoAction("Action 2"));
        manager.Undo();
        Assert.IsTrue(manager.CanRedo);
        manager.Push(new MockUndoAction("Action 3"));
        Assert.IsFalse(manager.CanRedo);
    }

    [TestMethod]
    public void MultipleUndoRedo()
    {
        using var manager = new UndoRedoManager();
        var a1 = new MockUndoAction("A1");
        var a2 = new MockUndoAction("A2");
        var a3 = new MockUndoAction("A3");
        manager.Push(a1);
        manager.Push(a2);
        manager.Push(a3);

        manager.Undo();
        Assert.AreEqual(1, a3.UndoCount);

        manager.Undo();
        Assert.AreEqual(1, a2.UndoCount);

        manager.Redo();
        Assert.AreEqual(1, a2.RedoCount);
    }

    [TestMethod]
    public void Clear_RemovesAll()
    {
        using var manager = new UndoRedoManager();
        manager.Push(new MockUndoAction("A"));
        manager.Push(new MockUndoAction("B"));
        manager.Clear();
        Assert.IsFalse(manager.CanUndo);
        Assert.IsFalse(manager.CanRedo);
    }

    [TestMethod]
    public void StateChanged_FiresOnPush()
    {
        using var manager = new UndoRedoManager();
        int count = 0;
        manager.StateChanged += (_, _) => count++;
        manager.Push(new MockUndoAction("A"));
        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void StateChanged_FiresOnUndo()
    {
        using var manager = new UndoRedoManager();
        manager.Push(new MockUndoAction("A"));
        int count = 0;
        manager.StateChanged += (_, _) => count++;
        manager.Undo();
        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void StateChanged_FiresOnRedo()
    {
        using var manager = new UndoRedoManager();
        manager.Push(new MockUndoAction("A"));
        manager.Undo();
        int count = 0;
        manager.StateChanged += (_, _) => count++;
        manager.Redo();
        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void MaxHistory_TrimsSilently()
    {
        using var manager = new UndoRedoManager { MaxHistory = 3 };
        manager.Push(new MockUndoAction("A"));
        manager.Push(new MockUndoAction("B"));
        manager.Push(new MockUndoAction("C"));
        manager.Push(new MockUndoAction("D"));
        // Should still work, oldest pruned
        Assert.IsTrue(manager.CanUndo);
    }

    [TestMethod]
    public void Undo_WhenEmpty_NoOp()
    {
        using var manager = new UndoRedoManager();
        manager.Undo(); // Should not throw
        Assert.IsFalse(manager.CanRedo);
    }

    [TestMethod]
    public void Redo_WhenEmpty_NoOp()
    {
        using var manager = new UndoRedoManager();
        manager.Redo(); // Should not throw
        Assert.IsFalse(manager.CanUndo);
    }

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var manager = new UndoRedoManager();
        manager.Push(new MockUndoAction("A"));
        manager.Dispose();
        manager.Dispose(); // Should not throw
    }

    private class MockUndoAction : IUndoRedoAction
    {
        public string Description { get; }
        public long EstimatedBytes { get; set; }
        public int UndoCount { get; private set; }
        public int RedoCount { get; private set; }
        public bool IsDisposed { get; private set; }

        public MockUndoAction(string desc) => Description = desc;
        public void Undo() => UndoCount++;
        public void Redo() => RedoCount++;
        public void Dispose() => IsDisposed = true;
    }
}
