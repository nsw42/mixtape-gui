using System;
using Xunit;
using MixtapeGui.Models;
using MixtapeGui.ViewModels;

namespace ViewModels.Tests
{
    public class Tests
    {
        // Scenarios to test relating to separate links:
        //    FROM (node, chain head, chain tail)
        //      x
        //     TO  (node, chain head, chain tail, nothing)
        // Plus scenarios relating to changing links within a chain

        private static void CheckLinksInChain(MusicFile[] list)
        {
            var currentNode = list[0];
            int index = 0;
            while (index < list.Length && currentNode != null)
            {
                var expectedPrev = (index == 0) ? null : list[index - 1];
                Assert.Equal(expectedPrev, currentNode.PrevMusicFile);
                // System.Console.WriteLine($"Equal: {currentNode.Title}.PrevMusicFile == expectedPrev {expectedPrev?.Title ?? "null"}");
                var expectedNext = (index == list.Length - 1) ? null : list[index + 1];
                Assert.Equal(expectedNext, currentNode.NextMusicFile);
                // System.Console.WriteLine($"Equal: {currentNode.Title}.NextMusicFile == expectedNext {expectedNext?.Title ?? "null"}");

                index++;
                currentNode = currentNode.NextMusicFile;
            }
        }

        [Fact]
        public void TestNodeToNode()
        {
            // Given A and B
            // When I connect A and B
            // Then A.next == B
            // And B.prev == A
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var vm = new ProjectViewModel(new Project());

            Assert.Null(A.PrevMusicFile);
            Assert.Null(A.NextMusicFile);
            Assert.Null(B.PrevMusicFile);
            Assert.Null(B.NextMusicFile);

            vm.AddConnection(A, B);

            CheckLinksInChain(new MusicFile[]{A, B});
        }

        [Fact]
        public void TestNodeToChainHead()
        {
            // Given A and B <-> C
            // When I connect A -> B
            // Then A <-> B <-> C
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(B, C);

            vm.AddConnection(A, B);

            CheckLinksInChain(new MusicFile[]{A, B, C});
        }

        [Fact]
        public void TestNodeToChainTail()
        {
            // Given A and B <-> C
            // When I connect A -> C
            // Then B <-> A <-> C
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(B, C);

            vm.AddConnection(A, C);

            CheckLinksInChain(new MusicFile[]{B, A, C});
        }

        [Fact]
        public void TestNodeToNothing()
        {
            // Given A
            // When I connect A to null
            // Then A points to nulll (and no exceptions occur)
            var A = new MusicFile(){ Title="A" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, null);

            CheckLinksInChain(new MusicFile[]{A});
        }

        [Fact]
        public void TestChainHeadToNode()
        {
            // Given A <-> B and C
            // When I connect A to C
            // Then A <-> C <-> B
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);

            vm.AddConnection(A, C);

            CheckLinksInChain(new MusicFile[]{A, C, B});
        }

        [Fact]
        public void TestChainHeadToChainHead()
        {
            // Given A <-> B and C <-> D
            // When I connect A to C
            // Then A <-> C <-> D <-> B
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var D = new MusicFile(){ Title="D" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);
            vm.AddConnection(C, D);

            vm.AddConnection(A, C);

            CheckLinksInChain(new MusicFile[]{A, C, D, B});
        }

        [Fact]
        public void TestChainHeadToChainTail()
        {
            // Given A <-> B and C <-> D
            // When I connect A to D
            // Then C <-> A <-> D <-> B
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var D = new MusicFile(){ Title="D" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);
            vm.AddConnection(C, D);

            vm.AddConnection(A, D);

            CheckLinksInChain(new MusicFile[]{C, A, D, B});
        }

        [Fact]
        public void TestChainHeadToNothing()
        {
            // Given A <-> B
            // When I connect A to null
            // Then A and B are separated
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);

            vm.AddConnection(A, null);

            CheckLinksInChain(new MusicFile[]{A});
            CheckLinksInChain(new MusicFile[]{B});
        }

        [Fact]
        public void TestChainTailToNode()
        {
            // Given A <-> B and C
            // When I connect B to C
            // Then A <-> B <-> C
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);

            vm.AddConnection(B, C);

            CheckLinksInChain(new MusicFile[]{A, B, C});
        }

        [Fact]
        public void TestChainTailToChainHead()
        {
            // Given A <-> B and C <-> D
            // When I connect B to C
            // Then A <-> B <-> C <-> D
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var D = new MusicFile(){ Title="D" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);
            vm.AddConnection(C, D);

            vm.AddConnection(B, C);

            CheckLinksInChain(new MusicFile[]{A, B, C, D});
        }

        [Fact]
        public void TestChainTailToChainTail()
        {
            // Given A <-> B and C <-> D
            // When I connect D to B
            // Then A <-> C <-> D <-> B
            // So that I don't have two files pointing to B
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var D = new MusicFile(){ Title="D" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);
            vm.AddConnection(C, D);

            vm.AddConnection(D, B);

            CheckLinksInChain(new MusicFile[]{A, C, D, B});
        }

        [Fact]
        public void TestChainTailToNothing()
        {
            // Given A <-> B
            // When I connect B to nothing
            // Then A <-> B
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);

            vm.AddConnection(B, null);

            CheckLinksInChain(new MusicFile[]{A, B});
        }

        [Fact]
        public void TestSwapNodeWithNextInChain()
        {
            // Given A <-> B <-> C <-> D
            // When I connect B to D
            // Then A <-> C <-> B <-> D
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var D = new MusicFile(){ Title="D" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);
            vm.AddConnection(B, C);
            vm.AddConnection(C, D);

            vm.AddConnection(B, D);

            CheckLinksInChain(new MusicFile[]{A, C, B, D});
        }

        [Fact]
        private void TestSwapNodeWithPrevInChain()
        {
            // Given A <-> B <-> C <-> D
            // When I connect C to B
            // Then A <-> C <-> B <-> D
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var D = new MusicFile(){ Title="D" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);
            vm.AddConnection(B, C);
            vm.AddConnection(C, D);

            vm.AddConnection(C, B);

            CheckLinksInChain(new MusicFile[]{A, C, B, D});
        }

        // This is essential to prevent problems when trying to add another link
        [Fact]
        public void TestCreatingCircularChainIsPrevented()
        {
            // Given A <-> B <-> C
            // When I connect C to A
            // Then C <-> A <-> B
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);
            vm.AddConnection(B, C);

            vm.AddConnection(C, A);
            CheckLinksInChain(new MusicFile[]{C, A, B});
        }

        [Fact]
        public void TestRecreatingExistingNodeIsIdempotent()
        {
            // Given A <-> B <-> C
            // When I connect A to B
            // Then A <-> B <-> C
            var A = new MusicFile(){ Title="A" };
            var B = new MusicFile(){ Title="B" };
            var C = new MusicFile(){ Title="C" };
            var vm = new ProjectViewModel(new Project());
            vm.AddConnection(A, B);
            vm.AddConnection(B, C);

            vm.AddConnection(A, B);
            CheckLinksInChain(new MusicFile[]{A, B, C});
        }
    }
}
