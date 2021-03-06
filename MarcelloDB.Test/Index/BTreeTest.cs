﻿using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using MarcelloDB.Index;
using MarcelloDB.Index.BTree;

namespace MarcelloDB.Test.Index
{
    [TestFixture]
    public class BTreeTest
    {
        private IBTreeDataProvider<int> _mockDataProvider;
        private const int Degree = 2;

        private readonly int[] testKeyData = new int[] { 10, 20, 30, 50 };
        private readonly int[] testPointerData = new int[] { 50, 60, 40, 20 };

        [SetUp]
        public void Initialize()
        {
            _mockDataProvider = new MockBTreeDataProvider<int>();
        }

        [Test]
        public void Create_BTree()
        {
            var btree = new BTree<int>(_mockDataProvider, Degree);

            Node<int> root = btree.Root;
            Assert.IsNotNull(root);
            Assert.IsNotNull(root.EntryList);
            Assert.IsNotNull(root.ChildrenAddresses);
            Assert.AreEqual(0, root.EntryList.Count);
            Assert.AreEqual(0, root.ChildrenAddresses.Count);
        }

        [Test]
        public void Insert_One_Node()
        {
            var btree = new BTree<int>(_mockDataProvider, Degree);
            this.InsertTestDataAndValidateTree(btree, 0);
            Assert.AreEqual(1, btree.Height);
        }

        [Test]
        public void Insert_Multiple_Nodes_To_Split()
        {
            var btree = new BTree<int>(_mockDataProvider, Degree);

            for (int i = 0; i < this.testKeyData.Length; i++)
            {
                this.InsertTestDataAndValidateTree(btree, i);
            }

            Assert.AreEqual(2, btree.Height);
        }

        [Test]
        public void Insert_Identical_Nodes_Throw_ArgumentException()
        {
            var btree = new BTree<int>(_mockDataProvider, Degree);
            for (int i = 0; i < this.testKeyData.Length; i++)
            {
                this.InsertTestDataAndValidateTree(btree, i);
            }

            Assert.Throws(typeof(ArgumentException), () =>
                {
                    btree.Insert(this.testKeyData[0], 0);
                });

            Assert.Throws(typeof(ArgumentException), () =>
                {
                    btree.Insert(this.testKeyData[this.testKeyData.Length / 2], 0);
                });
            Assert.Throws(typeof(ArgumentException), () =>
                {
                    btree.Insert(this.testKeyData[this.testKeyData.Length - 1], 0);
                });
        }

        [Test]
        public void Delete_Nodes()
        {
            var btree = new BTree<int>(_mockDataProvider, Degree);

            for (int i = 0; i < this.testKeyData.Length; i++)
            {
                this.InsertTestData(btree, i);
            }

            for (int i = 0; i < this.testKeyData.Length; i++)
            {
                btree.Delete(this.testKeyData[i]);
                ValidateTree(btree.Root, Degree, this.testKeyData.Skip(i + 1).ToArray());
            }

            Assert.AreEqual(1, btree.Height);
        }

        [Test]
        public void DeleteNodeBackwards()
        {
            var btree = new BTree<int>(_mockDataProvider, Degree);

            for (int i = 0; i < this.testKeyData.Length; i++)
            {
                this.InsertTestData(btree, i);
            }

            for (int i = this.testKeyData.Length - 1; i > 0; i--)
            {
                btree.Delete(this.testKeyData[i]);
                ValidateTree(btree.Root, Degree, this.testKeyData.Take(i).ToArray());
            }

            Assert.AreEqual(1, btree.Height);
        }

        [Test]
        public void Delete_NonExisting_Node()
        {
            var btree = new BTree<int>(_mockDataProvider, Degree);

            for (int i = 0; i < this.testKeyData.Length; i++)
            {
                this.InsertTestData(btree, i);
            }

            btree.Delete(99999);
            ValidateTree(btree.Root, Degree, this.testKeyData.ToArray());
        }

        [Test]
        public void Remove_From_Last_Node_Which_Reached_Its_Minimum()
        {
            var btree = new BTree<int>(_mockDataProvider, Degree);
            var leftNode = _mockDataProvider.CreateNode(Degree);
            var rightNode = _mockDataProvider.CreateNode(Degree);

            btree.Root.ChildrenAddresses.Add(leftNode.Address);
            btree.Root.ChildrenAddresses.Add(rightNode.Address);

            leftNode.EntryList.Add(new Entry<int> (){ Key = 1, Pointer = 1 });
            leftNode.EntryList.Add(new Entry<int> (){ Key = 2, Pointer = 2 });
            btree.Root.EntryList.Add(new Entry<int> (){ Key = 3, Pointer = 3 });
            rightNode.EntryList.Add(new Entry<int> (){ Key = 4, Pointer = 4 });

            btree.Delete(4);
        }

        [Test]
        public void Search_Nodes()
        {
            var btree = new BTree<int>(_mockDataProvider, Degree);

            for (int i = 0; i < this.testKeyData.Length; i++)
            {
                this.InsertTestData(btree, i);
                this.SearchTestData(btree, i);
            }
        }

        [Test]
        public void Search_NonExisting_Node()
        {
            var btree = new BTree<int>(_mockDataProvider, Degree);

            // search an empty tree
            Entry<int> nonExisting = btree.Search(9999);
            Assert.IsNull(nonExisting);

            for (int i = 0; i < this.testKeyData.Length; i++)
            {
                this.InsertTestData(btree, i);
                this.SearchTestData(btree, i);
            }

            // search a populated tree
            nonExisting = btree.Search(9999);
            Assert.IsNull(nonExisting);
        }


        [Test]
        public void Test_Btree_Bug_Remove_From_Subtree_Fix()
        {
            var degree = 2;
            var mockDataProvider = new MockBTreeDataProvider<int>();
            var btree = new BTree<int>(mockDataProvider, degree);

            for (int i = 0; i < 10; i++)
            {
                btree.Insert(i, i);
            }

            new List<int>(){ 3, 4 }.ForEach((i) => {
                btree.Delete(i);
            });

        }

        [Test]
        public void Brute_Force_Test()
        {
            for (int i = 0; i < 1000; i++)
            {
                RunBruteForce();
            }
        }

        #region Private Helper Methods
        private void InsertTestData(BTree<int> btree, int testDataIndex)
        {
            btree.Insert(this.testKeyData[testDataIndex], this.testPointerData[testDataIndex]);
        }

        private void InsertTestDataAndValidateTree(BTree<int> btree, int testDataIndex)
        {
            btree.Insert(this.testKeyData[testDataIndex], this.testPointerData[testDataIndex]);
            ValidateTree(btree.Root, Degree, this.testKeyData.Take(testDataIndex + 1).ToArray());
        }

        private void SearchTestData(BTree<int> btree, int testKeyDataIndex)
        {
            for (int i = 0; i <= testKeyDataIndex; i++)
            {
                Entry<int> entry = btree.Search(this.testKeyData[i]);
                Assert.IsNotNull(this.testKeyData[i]);
                Assert.AreEqual(this.testKeyData[i], entry.Key);
                Assert.AreEqual(this.testPointerData[i], entry.Pointer);
            }
        }

        internal void ValidateTree(Node<int> tree, int degree, params int[] expectedKeys)
        {
            var foundKeys = new Dictionary<int, List<Entry<int>>>();
            ValidateSubtree(tree, tree, degree, int.MinValue, int.MaxValue, foundKeys);

            Assert.AreEqual(0, expectedKeys.Except(foundKeys.Keys).Count());
            foreach (var keyValuePair in foundKeys)
            {
                Assert.AreEqual(1, keyValuePair.Value.Count);
            }
        }

        private void UpdateFoundKeys(Dictionary<int, List<Entry<int>>> foundKeys, Entry<int> entry)
        {
            List<Entry<int>> foundEntries;
            if (!foundKeys.TryGetValue(entry.Key, out foundEntries))
            {
                foundEntries = new List<Entry<int>>();
                foundKeys.Add(entry.Key, foundEntries);
            }

            foundEntries.Add(entry);
        }

        private void ValidateSubtree(Node<int> root, Node<int> node, int degree, int nodeMin, int nodeMax, Dictionary<int, List<Entry<int>>> foundKeys)
        {
            if (root != node)
            {
                Assert.IsTrue(node.EntryList.Count >= degree - 1);
                Assert.IsTrue(node.EntryList.Count <= (2 * degree) - 1);
            }

            for (int i = 0; i <= node.EntryList.Count; i++)
            {
                int subtreeMin = nodeMin;
                int subtreeMax = nodeMax;

                if (i < node.EntryList.Count)
                {
                    var entry = node.EntryList[i];
                    UpdateFoundKeys(foundKeys, entry);
                    Assert.IsTrue(entry.Key >= nodeMin && entry.Key <= nodeMax);

                    subtreeMax = entry.Key;
                }

                if (i > 0)
                {
                    subtreeMin = node.EntryList[i - 1].Key;
                }

                if (!node.IsLeaf)
                {
                    Assert.IsTrue(node.ChildrenAddresses.Count >= degree);
                    ValidateSubtree(root, _mockDataProvider.GetNode(node.ChildrenAddresses[i]), degree, subtreeMin, subtreeMax, foundKeys);
                }
            }
        }

        public void RunBruteForce()
        {
            var degree = 2;

            var mockDataProvider = new MockBTreeDataProvider<string>();
            var btree = new BTree<string>(mockDataProvider, degree);

            var rand = new Random();
            for (int i = 0; i < 1000; i++)
            {
                var value = (int)rand.Next() % 100;
                var key = value.ToString();

                if (rand.Next() % 2 == 0)
                {
                    if (btree.Search(key) == null)
                    {
                        btree.Insert(key, value);
                    }
                    Assert.AreEqual(value, btree.Search(key).Pointer);
                }
                else
                {
                    btree.Delete(key);
                    Assert.IsNull(btree.Search(key));
                }
                CheckNode(btree.Root, mockDataProvider);
            }
        }

        private void CheckNode(Node<string> node, IBTreeDataProvider<string> btreeData)
        {
            if (node.ChildrenAddresses.Count > 0 &&
               node.ChildrenAddresses.Count != node.EntryList.Count + 1)
            {
                Assert.Fail("There are children, but they don't match the number of entries.");
            }

            if (node.EntryList.Count > Node.MaxEntriesForDegree(node.Degree))
            {
                Assert.Fail("Too much entries in node");
            }

            if (node.EntryList.Count > Node.MaxChildrenForDegree(node.Degree))
            {
                Assert.Fail("Too much children in node");
            }

            foreach (var childAddress in node.ChildrenAddresses.Addresses)
            {
                var childNode = btreeData.GetNode(childAddress);
                CheckNode(childNode, btreeData);
            }
        }

        #endregion
    }
}