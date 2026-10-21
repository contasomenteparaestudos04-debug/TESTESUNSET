#!/bin/bash
cd /tmp/mock
fuser -k 5218/tcp 8099/tcp >/dev/null 2>&1; sleep 1
python3 mock_api.py > mock.log 2>&1 &
(cd /home/claude/work/sunset/sunset_app/build/web && exec python3 -m http.server 8099 > /tmp/mock/web.log 2>&1) &
sleep 2
timeout 170 python3 teste.py 2>&1 | tail -25
fuser -k 5218/tcp 8099/tcp >/dev/null 2>&1
