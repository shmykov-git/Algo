using System;
using System.Linq;
using AI.Model;
using AI.NBrain;
using FluentAssertions;
using Model.Extensions;
using NUnit.Framework;

namespace Model.Test
{
    public class ReversesTests
    {
        [Test]
        public void ReverseForwardTest()
        {
            int[] a = [7, 8, 9, 12, 17];
            int[] expected = [8, 7, 12, 17, 9];

            int[] reverse = [1, 0, 3, 4, 2];
            var ar = a.ReverseForward(reverse);

            ar.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }

        [Test]
        public void ReverseBackTest()
        {
            int[] a = [7, 8, 9, 12, 17];
            int[] expected = [8, 7, 17, 9, 12];

            int[] backReverse = [1, 0, 3, 4, 2];

            var arb = a.ReverseBack(backReverse);

            arb.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }

        [Test]
        public void FullReverseTest()
        {
            int[] a = [7, 8, 9, 12, 17];
            int[] reverse = [1, 0, 3, 4, 2];

            var aa = a.ReverseForward(reverse).ReverseBack(reverse);
            var aaa = a.ReverseForward(reverse).ReverseForward(reverse.ReverseReverses());

            aa.Should().BeEquivalentTo(a, options => options.WithStrictOrdering());
            aaa.Should().BeEquivalentTo(a, options => options.WithStrictOrdering());
        }

        [Test]
        public void ReverseABTest()
        {
            // use ToTranslateReverses to get indices to pass from a1 to a2

            // a0 = [7, 8, 9, 12, 17]
            int[] a1 = [8, 9, 17, 12, 7];
            int[] a2 = [12, 7, 8, 17, 9];

            var a1R = a1.ToBackReverses();
            var a2R = a2.ToBackReverses();

            var r1 = a1R.JoinReverses(a2R.ReverseReverses());
            var r2 = a2R.JoinReverses(a1R.ReverseReverses()).ReverseReverses(); // check this out, this is the same
            var r3 = a1.ToTranslateReverses(a2);

            r1.Should().BeEquivalentTo(r2, options => options.WithStrictOrdering());
            r1.Should().BeEquivalentTo(r3, options => options.WithStrictOrdering());

            var aa2 = a1.ReverseForward(r1);

            aa2.Should().BeEquivalentTo(a2, options => options.WithStrictOrdering());
        }

        [Test]
        public void ReverseABCTest()
        {
            // it doesn`t matter what middle array to use pass from a1 to a2, just use ToTranslateReverses

            // a0 = [7, 8, 9, 12, 17]
            int[] a1 = [8, 9, 17, 12, 7];
            int[] a2 = [12, 7, 8, 17, 9];
            int[] b = [17, 7, 8, 9, 12];

            var a1bR = a1.ToTranslateReverses(b);
            var a2bR = a2.ToTranslateReverses(b);

            var r1 = a1bR.JoinReverses(a2bR.ReverseReverses());
            var r2 = a1.ToTranslateReverses(a2);

            r1.Should().BeEquivalentTo(r2, options => options.WithStrictOrdering());
        }

        [Test]
        public void JoinReversesTest()
        {
            // a0 = [7, 8, 9, 12, 17]
            int[] a = [17, 7, 8, 9, 12];
            int[] b = [8, 9, 17, 12, 7];
            int[] c = [12, 7, 8, 17, 9];

            var ab = a.ToTranslateReverses(b);
            var bc = b.ToTranslateReverses(c);

            var abc = ab.JoinReverses(bc);
            var cc = a.ReverseForward(abc);

            c.Should().BeEquivalentTo(cc, options => options.WithStrictOrdering());
        }

        [Test]
        public void NGraphReversesTest()
        {
            (int, int) FindRv((int i, int j)[][] gA, (int i, int j)[][] gB)
            {
                var i = (gA[0], gB[0]).SelectBoth().First(v => !gB[0].Contains(v.a)).a.j;
                var j = (gA[0], gB[0]).SelectBoth().First(v => !gA[0].Contains(v.b)).b.j;

                return (i, j);
            }

            int[] GetReverses((int i, int j)[][] gA, (int i, int j)[][] gB)
            {
                var aa = gA[0].Select(v => v.j).Distinct().OrderBy(j => j).ToList();
                var r = (9).Range().ToArray();

                while (!(gA[0], gB[0]).SelectBoth().All(v => v.a == v.b))
                {
                    var (i, j) = FindRv(gA, gB);

                    gA[0].Index().Where(k => gA[0][k].j == i).ToArray().ForEach(k => gA[0][k] = (gA[0][k].i, -1));
                    gA[0].Index().Where(k => gA[0][k].j == j).ToArray().ForEach(k => gA[0][k] = (gA[0][k].i, i));
                    gA[0].Index().Where(k => gA[0][k].j == -1).ToArray().ForEach(k => gA[0][k] = (gA[0][k].i, j));
                    gA[0] = gA[0].OrderBy(v => v).ToArray();

                    var ii = aa.IndexOf(i);
                    var jj = aa.IndexOf(j);
                    (r[ii], r[jj]) = (r[jj], r[ii]);
                }

                return r;
            }

            (int i, int count)[] GetBackLvCounts((int i, int j)[][] graph, int lv) => graph[lv - 1].GroupBy(e => e.j).Select(gv => (i: gv.Key, c: gv.Count())).OrderBy(v => (v.c, v.i)).ToArray();

            (int i, int j)[][] gA = [[(0, 2), (0, 3), (0, 4), (0, 5), (0, 6), (0, 8), (0, 10), (1, 2), (1, 3), (1, 4), (1, 5), (1, 6), (1, 7), (1, 8), (1, 9)], [(2, 11), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11)]];
            (int i, int j)[][] gB = [[(0, 2), (0, 3), (0, 5), (0, 6), (0, 7), (0, 9), (0, 10), (1, 2), (1, 3), (1, 4), (1, 5), (1, 7), (1, 8), (1, 9), (1, 10)], [(2, 11), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11)]];
            // 4-7, 8-9, 9-10, 7-9, 6-7
            // 2-5, 6-7, 7-8, 5-7, 4-5 --- 2-5, 6-8, 4-7 -- 2-5, 6-7, 4-8
            (int, int)[][] gR = [[(0, 2), (0, 3), (0, 5), (0, 6), (0, 7), (0, 9), (0, 10), (1, 2), (1, 3), (1, 4), (1, 5), (1, 7), (1, 8), (1, 9), (1, 10)]];
            
            var trainer = new NTrainer(new NOptions { Graph = gA });
            trainer.Init();
            var model = trainer.model;

            var r = GetReverses(gA, gB);
            var bCs = GetBackLvCounts(gB, 1);

            model.ReverseLevelNodes(1, r);

            var gAA = model.GetGraph();
            var aaCs = GetBackLvCounts(gAA, 1);

            gA.Should().BeEquivalentTo(model.GetGraph(), options => options.WithStrictOrdering());
            bCs.Should().BeEquivalentTo(aaCs, options => options.WithStrictOrdering());
        }
    }
}