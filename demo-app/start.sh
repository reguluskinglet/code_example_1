#!/usr/bin/env bash

echo Setup config...
sed -i "s|window.BACKEND_ADDR=\"\"|window.BACKEND_ADDR=\"$BACKEND_ADDR\"|g" /opt/map_component/build/index.html


echo Start app...
npm run start:prod