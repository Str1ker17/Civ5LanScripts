#!/bin/bash

set -e
set -o pipefail
#set -x

authfile=/etc/openvpn/auth.txt
logfile=/etc/openvpn/auth-user.log
userdir=/etc/openvpn/client-config-dir

{
case "${username}" in
  *[\;]* )
    echo "Username '${username}' is NOT ALLOWED"
    exit 1
  ;;
esac
password=$(tail -n 1 "$1")
while IFS=';' read -r valid_username valid_password ; do
  if [ "${username}" = "${valid_username}" ] ; then
    if [ "${password}" = "${valid_password}" ] ; then
      echo "User '${username}' password MATCH"
      export > "${userdir}/${username}.env"
      exit 0
    else
      echo "User '${username}' password MISMATCH"
      exit 1
    fi
  fi
  # next username
done < "${authfile}"
echo "${username};${password}" >> $authfile
echo "User '${username}' REGISTERED with password"
export > "${userdir}/${username}.env"
exit 0
} 2>> "${logfile}" | tee -a ${auth_failed_reason_file} "${logfile}"
