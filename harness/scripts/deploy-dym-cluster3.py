#!/usr/bin/env python3
"""Deploy single-host 3-node Xugu test cluster on 192.168.2.239."""

from __future__ import annotations

import re
import sys

import paramiko

HOST = "192.168.2.239"
USER = "xugu"
PASSWORD = "rdb@2025"
CLUSTER_ROOT = "/nvme02/dym/dym-cluster"
BINARY = "xugu_linux_x86_64_20260521"

NODES = {
    1: {"listen": 5287, "my_nid": "0001"},
    2: {"listen": 5288, "my_nid": "0002"},
    3: {"listen": 5289, "my_nid": "0003"},
}

COMMON_REPLACEMENTS = {
    r"(?m)^\s*data_buff_mem\s*=\s*[^;]+;": "    data_buff_mem = 8192;  data buffer(M)",
    r"(?m)^\s*system_sga_mem\s*=\s*[^;]+;": "    system_sga_mem = 2048;  system sga(M)",
    r"(?m)^\s*max_malloc_once\s*=\s*[^;]+;": "    max_malloc_once = 1024;  max malloc once(M)",
    r"(?m)^\s*task_thd_num\s*=\s*[^;]+;": "    task_thd_num = 32;  task threads",
    r"(?m)^\s*max_parallel\s*=\s*[^;]+;": "    max_parallel = 8;  max parallel",
    r"(?m)^\s*cata_parti_num\s*=\s*[^;]+;": "    cata_parti_num = 32;  catalog partitions",
    r"(?m)^\s*rsync_thd_num\s*=\s*[^;]+;": "    rsync_thd_num = 16;  rsync threads",
    r"(?m)^\s*rtran_thd_num\s*=\s*[^;]+;": "    rtran_thd_num = 16;  rtran threads",
    r"(?m)^\s*def_data_space_size\s*=\s*[^;]+;": "    def_data_space_size = 1024;  data space(M)",
    r"(?m)^\s*def_temp_space_size\s*=\s*[^;]+;": "    def_temp_space_size = 1024;  temp space(M)",
    r"(?m)^\s*def_undo_space_size\s*=\s*[^;]+;": "    def_undo_space_size = 1024;  undo space(M)",
    r"(?m)^\s*def_redo_file_size\s*=\s*[^;]+;": "    def_redo_file_size = 1024;  redo file(M)",
    r"(?m)^\s*default_copy_num\s*=\s*[^;]+;": "    default_copy_num = 3;  default copy num",
    r"(?m)^\s*safely_copy_num\s*=\s*[^;]+;": "    safely_copy_num = 2;  safely copy num",
}


def make_cluster_ini(my_nid: str) -> str:
    return (
        "#MAX_NODES=32    MASTER_GRPS=1    PROTOCOL='UDP'    MSG_PORT_NUM=1    MAX_SEND_WIN=128 \n"
        " MSG_HAVE_CRC=0    MERGE_SMALL_MSG=0    MSG_SIZE=32768    TIMEOUT=10000    RPC_WINDOW=8 \n"
        f" EJE_WINDOW=8    MAX_SHAKE_TIME=1200    MY_NID={my_nid}    CHECK_RACK=0  ADDR_TYPE='IPV4'\n"
        "\n"
        " NID=0001  RACK=0001  PORTS='192.168.2.239:15287'  ROLE='MSQW'  LPU=8  STORE_WEIGHT=3  STATE=DETECT;\n"
        " NID=0002  RACK=0001  PORTS='192.168.2.239:15288'  ROLE='MSQW'  LPU=8  STORE_WEIGHT=3  STATE=DETECT;\n"
        " NID=0003  RACK=0001  PORTS='192.168.2.239:15289'  ROLE='SQWG'  LPU=8  STORE_WEIGHT=3  STATE=DETECT;\n"
    )


def make_startdb() -> str:
    return f"""#!/usr/bin/bash

echo "start xugu with --server"

chmod +x {BINARY}
$PWD/{BINARY} --server
"""


def apply_common(content: str) -> str:
    for pat, repl in COMMON_REPLACEMENTS.items():
        content, n = re.subn(pat, repl, content)
        if n == 0:
            print(f"WARN: no match for {pat}", file=sys.stderr)
    return content


def main() -> int:
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(HOST, username=USER, password=PASSWORD, timeout=15)
    sftp = client.open_sftp()

    base_xugu = sftp.file(f"{CLUSTER_ROOT}/node1/Server/SETUP/xugu.ini", "r").read().decode(
        "utf-8", errors="replace"
    )
    common = apply_common(base_xugu)

    for nid, cfg in NODES.items():
        base = f"{CLUSTER_ROOT}/node{nid}/Server"
        xugu = re.sub(
            r"(?m)^\s*listen_port\s*=\s*[^;]+;",
            f"    listen_port = {cfg['listen']};  listen port",
            common,
            count=1,
        )
        for path, content in (
            (f"{base}/SETUP/xugu.ini", xugu),
            (f"{base}/SETUP/cluster.ini", make_cluster_ini(cfg["my_nid"])),
            (f"{base}/BIN/startdb.sh", make_startdb()),
        ):
            with sftp.open(path, "w") as f:
                f.write(content)
            print("wrote", path)

        _, stdout, _ = client.exec_command(
            f"chmod +x {base}/BIN/startdb.sh {base}/BIN/{BINARY}"
        )
        stdout.channel.recv_exit_status()

    for nid in NODES:
        base = f"{CLUSTER_ROOT}/node{nid}/Server"
        _, stdout, _ = client.exec_command(
            f"echo NODE{nid}; "
            f"grep -E 'listen_port|data_buff_mem|system_sga_mem' {base}/SETUP/xugu.ini; "
            f"grep MY_NID {base}/SETUP/cluster.ini; "
            f"sed -n '1,6p' {base}/SETUP/cluster.ini; "
            f"cat {base}/BIN/startdb.sh"
        )
        print(stdout.read().decode("utf-8", errors="replace"))

    sftp.close()
    client.close()
    print("CONFIG DONE")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
