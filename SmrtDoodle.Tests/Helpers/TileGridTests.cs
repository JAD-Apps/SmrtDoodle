using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmrtDoodle.Helpers;
using System.Linq;
using Windows.Foundation;

namespace SmrtDoodle.Tests.Helpers;

[TestClass]
public class TileGridTests
{
    [TestMethod]
    public void Constructor_DefaultTileSize_Is512()
    {
        var grid = new TileGrid();
        Assert.AreEqual(512, grid.TileSize);
    }

    [TestMethod]
    public void Constructor_CustomTileSize()
    {
        var grid = new TileGrid(256);
        Assert.AreEqual(256, grid.TileSize);
    }

    [TestMethod]
    public void Constructor_InvalidTileSize_Defaults512()
    {
        var grid = new TileGrid(0);
        Assert.AreEqual(512, grid.TileSize);
    }

    [TestMethod]
    public void Resize_CalculatesCorrectDimensions()
    {
        var grid = new TileGrid(512);
        grid.Resize(1920, 1080);
        // 1920/512 = 3.75 → 4 cols, 1080/512 = 2.109 → 3 rows
        Assert.AreEqual(4, grid.Columns);
        Assert.AreEqual(3, grid.Rows);
        Assert.AreEqual(12, grid.TileCount);
    }

    [TestMethod]
    public void Resize_ExactMultiple()
    {
        var grid = new TileGrid(256);
        grid.Resize(1024, 512);
        Assert.AreEqual(4, grid.Columns);
        Assert.AreEqual(2, grid.Rows);
    }

    [TestMethod]
    public void Resize_SmallCanvas_AtLeast1x1()
    {
        var grid = new TileGrid(512);
        grid.Resize(100, 100);
        Assert.AreEqual(1, grid.Columns);
        Assert.AreEqual(1, grid.Rows);
    }

    [TestMethod]
    public void Resize_MarksAllDirty()
    {
        var grid = new TileGrid(512);
        grid.Resize(1024, 1024);
        for (int r = 0; r < grid.Rows; r++)
            for (int c = 0; c < grid.Columns; c++)
                Assert.IsTrue(grid.IsTileDirty(c, r), $"Tile ({c},{r}) should be dirty after resize");
    }

    [TestMethod]
    public void ClearAll_RemovesDirtyFlags()
    {
        var grid = new TileGrid(512);
        grid.Resize(1024, 1024);
        grid.ClearAll();
        for (int r = 0; r < grid.Rows; r++)
            for (int c = 0; c < grid.Columns; c++)
                Assert.IsFalse(grid.IsTileDirty(c, r), $"Tile ({c},{r}) should be clean after ClearAll");
    }

    [TestMethod]
    public void InvalidateRect_MarksOverlappingTiles()
    {
        var grid = new TileGrid(512);
        grid.Resize(2048, 2048); // 4x4 grid
        grid.ClearAll();

        // Invalidate a rect that overlaps tiles (0,0) and (1,0)
        grid.InvalidateRect(new Rect(400, 100, 200, 100));
        Assert.IsTrue(grid.IsTileDirty(0, 0));
        Assert.IsTrue(grid.IsTileDirty(1, 0));
        Assert.IsFalse(grid.IsTileDirty(0, 1));
        Assert.IsFalse(grid.IsTileDirty(2, 0));
    }

    [TestMethod]
    public void GetTileRect_ReturnsCorrectRect()
    {
        var grid = new TileGrid(256);
        grid.Resize(1024, 1024);

        var rect = grid.GetTileRect(2, 1);
        Assert.AreEqual(512, rect.X);
        Assert.AreEqual(256, rect.Y);
        Assert.AreEqual(256, rect.Width);
        Assert.AreEqual(256, rect.Height);
    }

    [TestMethod]
    public void GetVisibleTiles_FullViewport_ReturnsAll()
    {
        var grid = new TileGrid(512);
        grid.Resize(1024, 1024); // 2x2

        var visible = grid.GetVisibleTiles(new Rect(0, 0, 1024, 1024)).ToList();
        Assert.AreEqual(4, visible.Count);
    }

    [TestMethod]
    public void GetVisibleTiles_PartialViewport_ReturnsSubset()
    {
        var grid = new TileGrid(512);
        grid.Resize(2048, 2048); // 4x4

        // Viewport only covers top-left 2x2 tiles (stop just before tile col/row 2)
        var visible = grid.GetVisibleTiles(new Rect(0, 0, 1023, 1023)).ToList();
        Assert.AreEqual(4, visible.Count);
        Assert.IsTrue(visible.Contains((0, 0)));
        Assert.IsTrue(visible.Contains((1, 0)));
        Assert.IsTrue(visible.Contains((0, 1)));
        Assert.IsTrue(visible.Contains((1, 1)));
    }

    [TestMethod]
    public void GetDirtyVisibleTiles_OnlyReturnsDirtyOnes()
    {
        var grid = new TileGrid(512);
        grid.Resize(2048, 2048); // 4x4
        grid.ClearAll();
        grid.InvalidateRect(new Rect(0, 0, 100, 100)); // Only tile (0,0)

        var dirtyVisible = grid.GetDirtyVisibleTiles(new Rect(0, 0, 1024, 1024)).ToList();
        Assert.AreEqual(1, dirtyVisible.Count);
        Assert.AreEqual((0, 0), dirtyVisible[0]);
    }

    [TestMethod]
    public void IsTileDirty_OutOfBounds_ReturnsTrue()
    {
        var grid = new TileGrid(512);
        grid.Resize(1024, 1024);
        // Out-of-bounds should return true (safe fallback for rendering)
        Assert.IsTrue(grid.IsTileDirty(-1, 0));
        Assert.IsTrue(grid.IsTileDirty(0, 99));
    }

    [TestMethod]
    public void InvalidateAll_AfterClear_RestoredDirty()
    {
        var grid = new TileGrid(512);
        grid.Resize(1024, 1024);
        grid.ClearAll();
        grid.InvalidateAll();
        Assert.IsTrue(grid.IsTileDirty(0, 0));
        Assert.IsTrue(grid.IsTileDirty(1, 1));
    }
}
