﻿using NUnit.Framework;
using System;
using RTree;
using System.Linq;
using System.Collections.Generic;

namespace RTree.Test
{
	[TestFixture()]
	public class RTreeTest
	{
		[Test()]
		public void TestCase()
		{

			var test = new RTreeTestData();
			var data = test.Build1DTestData();

			//			var x = string.Join("\r\n", data.Item1.Select(xx=>xx[0]));
			//			var y = string.Join("\r\n", data.Item2);

			var x = data.Item1.Select(xx => xx[0]).ToArray();
			var y = data.Item2;
			var xy = x.Zip(y, (a,b)=>a+";"+b);
			var z = string.Join("\r\n", xy);


			//			Console.WriteLine(z);


			var settingsWoPruning = new RTreeRegressionSettings(5, PruningType.None, 0.1);
			var settings = new RTreeRegressionSettings(10, PruningType.CostComplexity, 0.1);

			var regWoPruning = new RTreeRegressor(settingsWoPruning);
			var reportWoPruning = regWoPruning.Train(data.Item1, data.Item2);

			var reg = new RTreeRegressor(settings);
			var report = reg.Train(data.Item1, data.Item2);

			var reggedYWoPruning = new List<double>();
			for(int i = 0; i < x.Count(); i++) 
			{
				reggedYWoPruning.Add(regWoPruning.Evaluate(data.Item1[i]));
			}

			var reggedY = new List<double>();
			for(int i = 0; i < x.Count(); i++) 
			{
				reggedY.Add(reg.Evaluate(data.Item1[i]));
			}

			//TODO : test split variable
			var forestSettings = new RForestRegressionSettings(10, 0.6, 5, 0);
			var forestReg = new RForestRegressor(forestSettings);
			forestReg.Train(data.Item1, data.Item2);
			var forestReggedY = new List<double>();
			for(int i = 0; i < x.Count(); i++) 
			{
				forestReggedY.Add(forestReg.Evaluate(data.Item1[i]));
			}

			var xyy = xy.Zip(reggedYWoPruning, (a, b) => a + ";" + b);
			var xyyy = xyy.Zip(reggedY, (a, b) => a + ";" + b);
			var xyyyf = xyyy.Zip(forestReggedY, (a, b) => a + ";" + b);

			var zz = string.Join("\r\n", xyyyf);
			Console.WriteLine("x;y;yTreeNoPruning;yTreePruning;yForest");
			Console.WriteLine(zz);

			Console.WriteLine ("*** Non Pruned tree ***");
			Console.WriteLine(reportWoPruning);
			var treeWoPruning = regWoPruning.Tree;
			Console.WriteLine(treeWoPruning.Print());
			Assert.AreEqual(31, treeWoPruning.Size(), "Tree (no pruning) size changed");


			Console.WriteLine ("*** Pruned tree ***");
			Console.WriteLine(report);
			var tree = reg.Tree;
			Console.WriteLine(tree.Print());
			Assert.AreEqual(11, tree.Size(), "Tree (pruned) size changed");

			var root = tree.GetRoot();
			Console.WriteLine("Root");
			Console.WriteLine(root);

			var leaves = tree.GetLeaves();
			Console.WriteLine("Leaves");
			Console.WriteLine(string.Join("\n", leaves.Select(l=>l.ToString())));
			var expectedLeavesId = new []{ 34, 35, 40, 42, 44, 45};
			var expectedLeavesSize = new []{ 20, 6, 7, 5, 9, 3};
			var expectedLeavesValue = new []{ 0.182, 0.477, 0.804, 0.968, 1.074, 1.110};
			Assert.AreEqual(expectedLeavesId.Count(), leaves.Count, "Tree nb leaves changed");
			for(int i = 0; i < leaves.Count; i++) 
			{
				Assert.AreEqual(expectedLeavesId[i], leaves.ElementAt(i).Id, string.Format("Leaf {0} id changed", i));
				Assert.AreEqual(expectedLeavesSize[i], leaves.ElementAt(i).Data.NSample, string.Format("Leaf {0} nb elements changed", i));
				Assert.AreEqual(expectedLeavesValue[i], leaves.ElementAt(i).Data.Average, 1e-3, string.Format("Leaf {0} value changed", i));
			}

			Console.WriteLine("*** Prune nodes ***");
			var emptyTree = tree.Prune(tree.GetRoot(), true);
			Console.WriteLine(emptyTree.Print());

			var oneLeafLessTree = tree.Prune(tree.GetLeaves().ElementAt(0), true);
			Console.WriteLine(oneLeafLessTree.Print());

			var oneHalfTree = tree.Prune(tree.GetChildren(root).Item1, true);
			Console.WriteLine(oneHalfTree.Print());

			var otherHalfTree = tree.Prune(tree.GetChildren(root).Item2, true);
			Console.WriteLine(otherHalfTree.Print());

			var stillSmallerTree = otherHalfTree.Prune(otherHalfTree.GetChildren(otherHalfTree.GetChildren(root).Item1).Item1, true);
			Console.WriteLine(stillSmallerTree.Print());

			Console.WriteLine("*** Prune nodes (start node not included) ***");
			var oneHalfTreeNodeNotIncluded = tree.Prune(tree.GetChildren(root).Item1, false);
			Console.WriteLine(oneHalfTreeNodeNotIncluded.Print());

			var oneHalfTreeAgain = tree.Prune(tree.GetChildren(root).Item1, true);
			Console.WriteLine(oneHalfTreeAgain.Print());

			Console.WriteLine("*** SubTree ***");
			var subTree = tree.SubTree(tree.GetChildren(root).Item1);
			Console.WriteLine(subTree.Print());

			var subTreeFromLeaves = tree.SubTree(tree.GetParent(tree.GetLeaves().ElementAt(3)));
			Console.WriteLine(subTreeFromLeaves.Print());
		}
	}
}
