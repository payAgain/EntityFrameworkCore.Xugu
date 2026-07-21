#!/usr/bin/env python3
"""Check ports and start the 3-node dym-cluster on 192.168.2.239."""

from __future__ import annotations

import time

import paramiko

HOST = "192.168.2.239"
USER = "xugu"
PASSWORD = "rdb@2025"
CLUSTER_ROOT = "/nvme02/dym/dym-cluster"
BINARY = "xugu_linux_x86_64_20260521"


def run(client: paramiko.SSHClient, cmd: str, timeout: int = 120) -> tuple[int, str, str]:
    print(">>>", cmd[:300])
    _, stdout, stderr = client.exec_command(cmd, timeout=timeout)
    out = stdout.read().decode("utf-8", errors="replace")
    err = stderr.read().decode("utf-8", errors="replace")
    rc = stdout.channel.recv_exit_status()
    if out.strip():
        print(out)
    if err.strip():
        print("ERR:", err)
    print("rc=", rc)
    return rc, out, err


def main() -> int:
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(HOST, username=USER, password=PASSWORD, timeout=15)

    # Port check via remote python
    port_check = r"""
python3 -c "
import socket
ports_tcp=[5287,5288,5289]
ports_udp=[15287,15288,15289,15307,15308,15309]
for p in ports_tcp:
    s=socket.socket(); s.settimeout(1)
    try:
        r=s.connect_ex(('127.0.0.1',p))
        print('TCP',p, 'IN_USE' if r==0 else 'free')
    finally:
        s.close()
for p in ports_udp:
    s=socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.bind(('0.0.0.0',p)); print('UDP',p,'free')
    except OSError as e:
        print('UDP',p,'IN_USE',e)
    finally:
        s.close()
"
"""
    run(client, port_check)

    run(
        client,
        "for n in 1 2 3; do echo ==== node$n ====; cat "
        f"{CLUSTER_ROOT}/node$n/Server/SETUP/cluster.ini; done",
    )

    # Stop any previous instances of this binary under dym-cluster
    run(
        client,
        f"pkill -f '{CLUSTER_ROOT}/node.*/Server/BIN/{BINARY}' || true; sleep 1; "
        f"ps -ef | grep '{CLUSTER_ROOT}' | grep -v grep || echo no_old_procs",
    )

    start_cmd = f"""
set -e
for n in 1 2 3; do
  mkdir -p {CLUSTER_ROOT}/node$n/Server/XGLOG
  cd {CLUSTER_ROOT}/node$n/Server/BIN
  nohup ./startdb.sh > ../XGLOG/start.out 2>&1 &
  echo "launched node$n pid $!"
done
sleep 3
ps -ef | grep '{BINARY}' | grep '{CLUSTER_ROOT}' | grep -v grep || true
"""
    run(client, start_cmd, timeout=60)

    # Wait and poll listen / logs
    for i in range(24):
        time.sleep(5)
        _, out, _ = run(
            client,
            f"echo === poll {i} ===; "
            f"ps -ef | grep '{BINARY}' | grep '{CLUSTER_ROOT}' | grep -v grep | wc -l; "
            "ss -lnt | grep -E '5287|5288|5289' || true; "
            f"for n in 1 2 3; do echo --node$n--; "
            f"tail -n 8 {CLUSTER_ROOT}/node$n/Server/XGLOG/start.out 2>/dev/null || true; "
            f"tail -n 5 {CLUSTER_ROOT}/node$n/Server/XGLOG/EVENT.LOG 2>/dev/null || true; "
            f"ls {CLUSTER_ROOT}/node$n/Server/XGLOG/ 2>/dev/null || true; done",
            timeout=60,
        )
        if "5287" in out and "5288" in out and "5289" in out:
            print("All listen ports are up")
            break
    else:
        print("WARN: not all listen ports observed yet")

    client.close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
