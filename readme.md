# 实现思路：
 
`` 
客户ID和得分使用安全字典存储。

排名由红黑树排序得到。

在单线程中，插入客户字典的同时插入红黑树，使用时间较长，各算法数据结构，在10万条数据顺序插入后，几乎需要2分钟以上。

更新客户字典的同时，发送队列。由服务批量更新红黑树，效果明显。

对比了红黑树，跳表，SortedSet三种数据结构，红黑树在一万条插入时最快。
``
# 最终方案：
``
1.红黑树数据结构处理排名。

2.字典存储客户信息。

3.安全消息队列用于消息传递。

4.net core 后台服务用于消费队列，并插入红黑树。

``
#### 备注：所有interface接口无实际作用，仅为统一算法。
# 增加各算法十万测试

# 测试截图：

<img src="https://github.com/coolbader/Homework/blob/master/%E6%B5%8B%E8%AF%95%E6%88%AA%E5%9B%BE.png" />