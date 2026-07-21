#!/usr/bin/env python3
"""Verify 3-node dym-cluster with SHOW CLUSTERS."""

from __future__ import annotations

import paramiko

HOST = "192.168.2.239"
USER = "xugu"
PASSWORD = "rdb@2025"
CLUSTER_ROOT = "/nvme02/dym/dym-cluster"


def run(client: paramiko.SSHClient, cmd: str, timeout: int = 60) -> str:
    print(">>>", cmd[:240])
    _, stdout, stderr = client.exec_command(cmd, timeout=timeout)
    out = stdout.read().decode("utf-8", errors="replace")
    err = stderr.read().decode("utf-8", errors="replace")
    rc = stdout.channel.recv_exit_status()
    if out.strip():
        print(out)
    if err.strip():
        print("ERR:", err)
    print("rc=", rc)
    return out


def main() -> int:
    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(HOST, username=USER, password=PASSWORD, timeout=15)

    run(client, "which xgconsole; ls -l /usr/sbin/xgconsole 2>/dev/null; type xgconsole 2>/dev/null")
    run(
        client,
        "ss -lnt | grep -E '5287|5288|5289'; "
        f"ps -ef | grep '{CLUSTER_ROOT}' | grep xugu_linux | grep -v grep",
    )

    # Try common console invocations on each listen port
    for port in (5287, 5288, 5289):
        print(f"\n===== VERIFY PORT {port} =====")
        # Prefer non-interactive if available
        sql = "SHOW CLUSTERS;"
        for cmd in (
            f"printf '%s\\n' \"{sql}\" | xgconsole nssl {HOST} {port} SYSTEM SYSDBA SYSDBA",
            f"printf '%s\\n' \"{sql}\" | /usr/sbin/xgconsole nssl {HOST} {port} SYSTEM SYSDBA SYSDBA",
            f"printf '%s\\n' \"{sql}\" | xgconsole2 -s nssl -h {HOST} -P {port} -d SYSTEM -u SYSDBA -p SYSDBA",
        ):
            out = run(client, cmd + " 2>&1 | head -n 80", timeout=45)
            if "NODE" in out.upper() or "cluster" in out.lower() or "192.168.2.239" in out:
                break

    # Also dump stdout.txt listening lines
    run(
        client,
        f"for n in 1 2 3; do echo ====node$n stdout====; "
        f"grep -E 'Listening|cluster|All service|error|Error|fail' "
        f"{CLUSTER_ROOT}/node$n/Server/BIN/stdout.txt 2>/dev/null | tail -n 30; done",
    )

    client.close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
