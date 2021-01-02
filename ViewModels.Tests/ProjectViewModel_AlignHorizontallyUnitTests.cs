using System;
using System.Collections.Generic;
using Xunit;
using MixtapeGui.Models;
using MixtapeGui.ViewModels;

namespace ViewModels.Tests
{
    public class AlignHorizontallyTests
    {

        [Fact]
        public void TestAlignEmptySet()
        {
            var emptyList = new List<MusicFile>();
            var vm = new ProjectViewModel(new Project());
            vm.AlignHorizontally(emptyList);
        }

        [Fact]
        public void TestAlignThreeItems()
        {
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var vm = new ProjectViewModel(new Project());
            var list = new List<MusicFile>(){A,B,C};
            A.CanvasX = 200;
            B.CanvasX = 100;
            C.CanvasX = 300;
            vm.AlignHorizontally(list);
            Assert.Equal(100, A.CanvasX);
            Assert.Equal(100, B.CanvasX);
            Assert.Equal(100, C.CanvasX);
        }
    }
}
