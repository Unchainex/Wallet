set -e

SERVICE="unchainexwallet.service"

# Restarting UnchainexWallet service....
sudo systemctl restart $SERVICE
echo "[OK] UnchainexWallet service was restarted"

# Checking deployment...
sleep 1
systemctl status $SERVICE --no-pager
UNCHAINEX_SERVICE_STATUS="$(systemctl is-active $SERVICE)"
if [ "${UNCHAINEX_SERVICE_STATUS}" = "active" ]; then
   echo "$SERVICE is running"
else
   echo "$SERVICE is NOT running"
fi
