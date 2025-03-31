using Homework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkTests
{
    [TestClass()]
    public class AlgorithmInsertTest
    {
        private SortedSet<CustomerData> _sortedSet=new SortedSet<CustomerData>(new CustomerDataComparer());
        private SkipList<CustomerData> _skipList = new SkipList<CustomerData>();
        private RedBlackTree<CustomerData, CustomerIdComparer> _redBlackTree = new RedBlackTree<CustomerData, CustomerIdComparer>();


        [TestMethod]
        public void Test_Sorted_Insert()
        {
            ActionTimer(100000, (data) => { 
            _sortedSet.Add(data);
            });
        }
        [TestMethod]
        public void Test_skiplist_Insert()
        {
            ActionTimer(100000, (data) => {
                _sortedSet.Add(data);
            });
        }
        [TestMethod]
        public void Test_rbt_Insert()
        {
            ActionTimer(100000, (data) => {
                _redBlackTree.Insert(data);
            });
        }
        [TestMethod]
        public void Test_Sorted_update()
        {
            ActionTimer(100000, (data) => {
                _sortedSet.Add(data);
            });
            ActionTimer(100000, (data) => {
              var a=  _sortedSet.First(a=>a.CustomerId==data.CustomerId);
                _sortedSet.Remove(a);
                data.Score += data.Score;
                _sortedSet.Add(data);
            });
        }
        [TestMethod]
        public void Test_skiplist_update()
        {
            ActionTimer(100000, (data) => {
                _skipList.Add(data);
            });
            ActionTimerIndex(100000, (data,index) => {
                var a = _skipList.GetByIndex(index-1);
                _skipList.Remove(a);
                data.Score += data.Score;
                _skipList.Add(data);
            });
        }
        [TestMethod]
        public void Test_rbt_update()
        {
            ActionTimer(100000, (data) => {
                _redBlackTree.Insert(data);
            });
            ActionTimerIndex(100000, (data,index) => {
                var a = _redBlackTree.GetByIndex(index-1);
                _redBlackTree.Delete(a);
                data.Score += data.Score;
                _redBlackTree.Insert(data);
            });
        }
        private void ActionTimer(int count,Action<CustomerData> action)
        {
            
            for (int i = 1; i <= count; i++)
            {
                var customer=new CustomerData(i,i);
                action?.Invoke(customer);
            }
        }
        private void ActionTimerIndex(int count, Action<CustomerData,int> action)
        {

            for (int i = 1; i <= count; i++)
            {
                var customer = new CustomerData(i, i);
                action?.Invoke(customer,i);
            }
        }
    }
}
