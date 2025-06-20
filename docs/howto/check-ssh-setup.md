# ✅ SSH Setup Verification Commands (Ubuntu Server)

This document contains commands to verify if SSH is properly installed, running, and accessible remotely. Also includes troubleshooting tips if you're using CI/CD pipelines with caching.

---

## 1. Check if OpenSSH is Installed

```bash
dpkg -l | grep openssh-server
If nothing is returned, install it with:

sudo apt update
sudo apt install openssh-server -y

2. Check SSH Service Status

sudo systemctl status ssh

You should see: Active: active (running)

To start and enable it:

sudo systemctl start ssh
sudo systemctl enable ssh

3. Check SSH Listening Port

sudo ss -tuln | grep :22

Expected output:

LISTEN 0 128 0.0.0.0:22 ...

If you only see 127.0.0.1:22, SSH is not listening publicly.
4. Verify SSH Listen Address in Config

grep ^ListenAddress /etc/ssh/sshd_config

If you see ListenAddress 127.0.0.1, change it to:

ListenAddress 0.0.0.0

Then restart the service:

sudo systemctl restart ssh

5. Test from Another Machine

From your local machine or CI runner:

ssh -o StrictHostKeyChecking=no root@<your-server-ip>

Or use scp:

scp -o StrictHostKeyChecking=no somefile root@<your-server-ip>:/root/

6. Firewall Checks (Optional)
UFW:

sudo ufw status
sudo ufw allow ssh

IPTables:

sudo iptables -L -n

7. Cloud Provider Firewall (e.g. Kamatera)

Ensure port 22 is open for inbound traffic in Kamatera's network/firewall settings.
8. 🧹 Clear CI/CD Cache (If Using Caching)

If you're using GitHub Actions, GitLab CI, or similar and SSH still fails unexpectedly:

    Change the cache key in your workflow config.

    Delete existing cache manually in the CI web UI.

    Or add a cleanup step:

rm -rf ~/.ssh/known_hosts ~/.ssh/id_rsa ~/.ssh/id_rsa.pub

Clearing the cache can resolve issues with stale SSH keys or host fingerprint mismatches.
✅ All set! If you see no errors and can connect, SSH is working properly.


Let me know if you want this exported to a file or formatted for a particular CI system.

