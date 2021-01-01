using System;
using Xunit;
using MixtapeGui.Models;
using MixtapeGui.ViewModels;

namespace ViewModels.Tests
{
    public class Tests
    {
        [Fact]
        public void TestConnectTwoFiles()
        {
            // Given A and B
            // When I connect A and B
            // Then A.next == B
            // And B.prev == A
            var A = new MusicFile();
            var B = new MusicFile();
            var proj = new Project();
            var vm = new ProjectViewModel(proj);

            Assert.Null(A.PrevMusicFile);
            Assert.Null(A.NextMusicFile);
            Assert.Null(B.PrevMusicFile);
            Assert.Null(B.NextMusicFile);

            vm.AddConnection(A, B);

            Assert.Null(A.PrevMusicFile);
            Assert.Equal(B, A.NextMusicFile);
            Assert.Equal(A, B.PrevMusicFile);
            Assert.Null(B.NextMusicFile);
        }
    }
}
