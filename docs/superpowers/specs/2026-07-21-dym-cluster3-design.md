# 单机三节点测试集群设计（dym-cluster）

日期：2026-07-21  
主机：`192.168.2.239`  
根路径：`/nvme02/dym/dym-cluster/`

## 目标

在单机上部署 XuguDB 分布式三节点测试集群，供 EF Core Provider 等联调使用。测试版，不导入 license。

## 拓扑

| 节点 | 路径 | MY_NID | ROLE | listen_port | 内部 UDP |
|------|------|--------|------|-------------|---------|
| 1 | `/nvme02/dym/dym-cluster/node1/Server` | 0001 | MSQW | 5287 | 192.168.2.239:15287 |
| 2 | `/nvme02/dym/dym-cluster/node2/Server` | 0002 | MSQW | 5288 | 192.168.2.239:15288 |
| 3 | `/nvme02/dym/dym-cluster/node3/Server` | 0003 | SQWG | 5289 | 192.168.2.239:15289 |

发送端口 = 接收端口 + 20（15307–15309）。

## 资源约束

三节点合计内存不超过 32G：

- `data_buff_mem = 8192`（8G/节点）
- `system_sga_mem = 2048`（2G/节点）
- `LPU = 8`
- `task_thd_num = 32`，`max_parallel = 8`
- `default_copy_num = 3`，`safely_copy_num = 2`

软件源：`/home/xugu/dym/Server`（仅作拷贝模板，不在根分区运行集群）。

## 运维命令

启动（各节点 BIN）：

```bash
cd /nvme02/dym/dym-cluster/nodeN/Server/BIN && ./startdb.sh
```

验收：

```bash
printf 'SHOW CLUSTERS;\n' | xgconsole nssl 192.168.2.239 5287 SYSTEM SYSDBA SYSDBA
```

连接串示例：`192.168.2.239:5287`（节点1），用户 `SYSDBA` / `SYSDBA`，库 `SYSTEM`。

## 文档依据

- `docs/content/deployment/deploy-distributed-version.md`
- `docs/content/reference/system-configuration-parameter/cluster.md`
- `docs/content/management/cluster-management.md`
