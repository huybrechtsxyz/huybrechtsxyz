echo y | ufw reset;
ufw default deny incoming;
ufw default allow outgoing;
ufw allow 22/tcp;
ufw allow 80/tcp;
ufw allow 443/tcp;
value=$( ufw status | grep -ic 'Status: active' );
if [ $value -eq 1 ]
then
  echo "Firewall is already active";
else
  echo "Enable firewall";
  echo "y" | ufw enable;
fi;